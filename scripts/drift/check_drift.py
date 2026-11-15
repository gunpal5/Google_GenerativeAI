"""Detects drift between this SDK and Google's APIs.

Checks:
  models       Gemini API model list (/v1beta/models) vs. the constants in GoogleAIModels.cs.
               Needs GEMINI_API_KEY (or GOOGLE_API_KEY).
  api          Gemini API discovery document (request/response schemas) vs. the snapshot in
               .github/drift/gemini-api-v1beta.json.
  python-genai Latest google-genai release on PyPI vs. the version recorded in
               .github/drift/python-genai-version.txt (the reference SDK we mirror).

Usage:
  python scripts/drift/check_drift.py <check> [--report report.md] [--update]

Exit code 0 always (unless the check itself fails); drift is reported through the report file and by printing
"DRIFT=true" or "DRIFT=false". With --update, the baseline for that check is refreshed after you have handled
the drift (for "models" this appends the current unmatched model IDs to the ignore list).
Uses only the Python standard library.
"""
from __future__ import annotations

import argparse
import fnmatch
import json
import os
import re
import sys
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
DRIFT_DIR = ROOT / ".github" / "drift"
GOOGLE_MODELS_CS = ROOT / "src" / "GenerativeAI" / "Constants" / "GoogleAIModels.cs"
MODELS_IGNORE = DRIFT_DIR / "models-ignore.txt"
API_SNAPSHOT = DRIFT_DIR / "gemini-api-v1beta.json"
PYGENAI_VERSION = DRIFT_DIR / "python-genai-version.txt"

DISCOVERY_URL = "https://generativelanguage.googleapis.com/$discovery/rest?version=v1beta"
MODELS_URL = "https://generativelanguage.googleapis.com/v1beta/models?pageSize=1000"
PYPI_URL = "https://pypi.org/pypi/google-genai/json"


def http_json(url: str, headers: dict[str, str] | None = None) -> dict:
    request = urllib.request.Request(url, headers={"User-Agent": "sdk-drift-check", **(headers or {})})
    with urllib.request.urlopen(request, timeout=60) as response:
        return json.load(response)


# --------------------------------------------------------------------------------------------- models

def parse_constants(path: Path) -> list[tuple[str, str, bool]]:
    """Returns (name, bare model id, obsolete) for every `public const string` in the file."""
    result, obsolete = [], False
    for line in path.read_text(encoding="utf-8-sig").splitlines():
        stripped = line.strip()
        if stripped.startswith("[Obsolete"):
            obsolete = True
            continue
        match = re.match(r'public const string (\w+)\s*=\s*"([^"]*)"', stripped)
        if match:
            result.append((match.group(1), match.group(2).removeprefix("models/"), obsolete))
            obsolete = False
        elif stripped and not stripped.startswith(("///", "//", "[")):
            obsolete = False
    return result


def read_ignore() -> list[str]:
    if not MODELS_IGNORE.exists():
        return []
    return [l.split("#", 1)[0].strip() for l in MODELS_IGNORE.read_text(encoding="utf-8").splitlines()
            if l.split("#", 1)[0].strip()]


def check_models(update: bool) -> tuple[bool, str]:
    key = os.environ.get("GEMINI_API_KEY") or os.environ.get("GOOGLE_API_KEY")
    if not key:
        return False, ("Skipped: set the `GEMINI_API_KEY` repository secret to compare the live Gemini API model "
                       "list with `GoogleAIModels`.\n")

    live: list[dict] = []
    url = MODELS_URL
    while True:
        page = http_json(url, {"x-goog-api-key": key})
        live.extend(page.get("models", []))
        token = page.get("nextPageToken")
        if not token:
            break
        url = f"{MODELS_URL}&pageToken={token}"
    live_ids = {m["name"].removeprefix("models/"): m.get("displayName", "") for m in live}

    constants = parse_constants(GOOGLE_MODELS_CS)
    constant_ids = {mid for _, mid, _ in constants}
    ignore = read_ignore()

    new_models = sorted(mid for mid in live_ids
                        if mid not in constant_ids and not any(fnmatch.fnmatch(mid, p) for p in ignore))
    gone_active = sorted((name, mid) for name, mid, obs in constants
                         if not obs and mid not in live_ids and re.match(r"^(gemini|gemma|imagen|veo|lyria)", mid))
    gone_obsolete = sorted((name, mid) for name, mid, obs in constants if obs and mid not in live_ids)

    if update and new_models:
        with MODELS_IGNORE.open("a", encoding="utf-8") as f:
            f.write("".join(f"{mid}\n" for mid in new_models))

    lines = [f"Compared {len(live_ids)} models from the Gemini API with {len(constants)} constants in "
             f"`GoogleAIModels.cs`.\n"]
    if new_models:
        lines.append("\n### New models without a constant\n\nAdd a constant, or add the ID to "
                     "`.github/drift/models-ignore.txt` if it shouldn't have one.\n")
        lines += [f"- `{mid}`: {live_ids[mid]}" for mid in new_models]
    if gone_active:
        lines.append("\n### Constants whose model is no longer served\n\nMark these `[Obsolete]` with the "
                     "replacement, or remove them if Google has shut the model down.\n")
        lines += [f"- `GoogleAIModels.{name}` (`{mid}`)" for name, mid in gone_active]
    if gone_obsolete:
        lines.append("\n### Obsolete constants that can now be removed\n\nThe model is no longer served.\n")
        lines += [f"- `GoogleAIModels.{name}` (`{mid}`)" for name, mid in gone_obsolete]
    drift = bool(new_models or gone_active or gone_obsolete)
    if not drift:
        lines.append("\nNo drift found.")
    return drift, "\n".join(lines) + "\n"


# ------------------------------------------------------------------------------------------------ api

def discovery_snapshot() -> dict:
    doc = http_json(DISCOVERY_URL)
    schemas = {name: sorted((schema.get("properties") or {}).keys())
               for name, schema in sorted(doc.get("schemas", {}).items())}
    return {"revision": doc.get("revision"), "schemas": schemas}


def check_api(update: bool) -> tuple[bool, str]:
    current = discovery_snapshot()
    if update or not API_SNAPSHOT.exists():
        API_SNAPSHOT.parent.mkdir(parents=True, exist_ok=True)
        API_SNAPSHOT.write_text(json.dumps(current, indent=1, sort_keys=True) + "\n", encoding="utf-8")
        if not update:
            return False, f"Created the baseline snapshot (revision {current['revision']}).\n"

    baseline = json.loads(API_SNAPSHOT.read_text(encoding="utf-8"))
    old, new = baseline["schemas"], current["schemas"]
    added = sorted(set(new) - set(old))
    removed = sorted(set(old) - set(new))
    changed = []
    for name in sorted(set(old) & set(new)):
        plus = sorted(set(new[name]) - set(old[name]))
        minus = sorted(set(old[name]) - set(new[name]))
        if plus or minus:
            changed.append((name, plus, minus))

    lines = [f"Gemini API (v1beta) discovery revision: baseline `{baseline.get('revision')}`, "
             f"current `{current['revision']}`.\n"]
    if added:
        lines.append("\n### New types\n")
        lines += [f"- `{n}`: {', '.join(f'`{p}`' for p in new[n]) or '(no fields)'}" for n in added]
    if removed:
        lines.append("\n### Removed types\n")
        lines += [f"- `{n}`" for n in removed]
    if changed:
        lines.append("\n### Changed types\n")
        for name, plus, minus in changed:
            parts = []
            if plus:
                parts.append("added " + ", ".join(f"`{p}`" for p in plus))
            if minus:
                parts.append("removed " + ", ".join(f"`{p}`" for p in minus))
            lines.append(f"- `{name}`: {'; '.join(parts)}")
    drift = bool(added or removed or changed) and not update
    if drift:
        lines.append("\nAfter updating the SDK types, refresh the baseline with "
                     "`python scripts/drift/check_drift.py api --update` and commit `.github/drift/`.")
    else:
        lines.append("\nNo drift found." if not update else "\nBaseline updated.")
    return drift, "\n".join(lines) + "\n"


# --------------------------------------------------------------------------------------- python-genai

def version_key(v: str) -> tuple:
    return tuple(int(p) if p.isdigit() else -1 for p in re.split(r"[.\-]", v))


def check_python_genai(update: bool) -> tuple[bool, str]:
    info = http_json(PYPI_URL)
    latest = info["info"]["version"]
    if update or not PYGENAI_VERSION.exists():
        PYGENAI_VERSION.parent.mkdir(parents=True, exist_ok=True)
        PYGENAI_VERSION.write_text(latest + "\n", encoding="utf-8")
        return False, f"Recorded python-genai {latest} as the baseline.\n"

    baseline = PYGENAI_VERSION.read_text(encoding="utf-8").strip()
    if version_key(latest) <= version_key(baseline):
        return False, f"python-genai is at {latest}; the SDK was last synced with {baseline}. No drift found.\n"

    newer = sorted((v for v in info.get("releases", {})
                    if re.fullmatch(r"\d+(\.\d+)*", v) and version_key(baseline) < version_key(v) <= version_key(latest)),
                   key=version_key)
    lines = [f"python-genai (the reference SDK) has new releases since {baseline}, the version this SDK was last "
             f"synced with.\n",
             "\n### New releases\n"]
    lines += [f"- [{v}](https://github.com/googleapis/python-genai/releases/tag/v{v})" for v in newer]
    lines.append("\nReview the [changelog](https://github.com/googleapis/python-genai/blob/main/CHANGELOG.md) for "
                 "new fields, features and models. When the SDK is up to date, run "
                 "`python scripts/drift/check_drift.py python-genai --update` and commit `.github/drift/`.")
    return True, "\n".join(lines) + "\n"


# ------------------------------------------------------------------------------------------------ dto

DTO_BASELINE = DRIFT_DIR / "dto-baseline.json"
DTO_MAP = DRIFT_DIR / "dto-map.json"
PYGENAI_TYPES_URL = "https://raw.githubusercontent.com/googleapis/python-genai/v{version}/google/genai/types.py"
SRC_DIR = ROOT / "src"


def to_camel(snake: str) -> str:
    """python-genai's pydantic alias generator: response_mime_type -> responseMimeType."""
    head, *rest = snake.split("_")
    return head + "".join(part[:1].upper() + part[1:] for part in rest)


def python_types(version: str) -> tuple[dict[str, set[str]], dict[str, set[str]]]:
    """Parses python-genai's types.py (without executing it) into {class: camelCase fields} and {enum: values}."""
    import ast

    request = urllib.request.Request(PYGENAI_TYPES_URL.format(version=version),
                                     headers={"User-Agent": "sdk-drift-check"})
    import warnings

    with urllib.request.urlopen(request, timeout=60) as response:
        source = response.read().decode("utf-8")
    with warnings.catch_warnings():
        warnings.simplefilter("ignore", SyntaxWarning)  # docstrings in types.py contain "\/"
        tree = ast.parse(source)

    def base_names(node: "ast.ClassDef") -> set[str]:
        return {b.attr if isinstance(b, ast.Attribute) else getattr(b, "id", "") for b in node.bases}

    classes, enums = {}, {}
    for node in tree.body:
        if not isinstance(node, ast.ClassDef):
            continue
        bases = base_names(node)
        if "BaseModel" in bases:
            classes[node.name] = {to_camel(stmt.target.id) for stmt in node.body
                                  if isinstance(stmt, ast.AnnAssign) and isinstance(stmt.target, ast.Name)
                                  and not stmt.target.id.startswith("_") and stmt.target.id != "model_config"}
        elif "CaseInSensitiveEnum" in bases or "Enum" in bases:
            enums[node.name] = {stmt.value.value for stmt in node.body
                                if isinstance(stmt, ast.Assign) and isinstance(stmt.value, ast.Constant)
                                and isinstance(stmt.value.value, str)}
    return classes, enums


TYPE_DECL = re.compile(r"\b(class|record|struct|enum|interface)\s+(\w+)")
JSON_NAME = re.compile(r'JsonPropertyName\(\s*"([^"]+)"\s*\)')
ENUM_NAME = re.compile(r'(?:JsonStringEnumMemberName|EnumMember\s*\(\s*Value\s*=)\s*\(?\s*"([^"]+)"')
PROPERTY = re.compile(r"^\s*public\s+(?:required\s+|virtual\s+|override\s+|new\s+)*[\w<>\[\]?,. ]+\s+(\w+)\s*\{\s*get")
ENUM_MEMBER = re.compile(r"^\s*([A-Za-z_]\w*)\s*(?:=\s*[^,]+)?,?\s*$")


def csharp_types() -> tuple[dict[str, set[str]], dict[str, set[str]]]:
    """Collects {class: JSON names} and {enum: JSON values} from the SDK sources (a lightweight parser)."""
    classes: dict[str, set[str]] = {}
    enums: dict[str, set[str]] = {}
    for path in SRC_DIR.rglob("*.cs"):
        if any(part in ("bin", "obj") for part in path.parts):
            continue
        stack: list[tuple[str, str, int]] = []
        pending: tuple[str, str] | None = None
        pending_json: str | None = None
        depth = 0
        for raw in path.read_text(encoding="utf-8-sig", errors="replace").splitlines():
            line = raw.strip()
            if line.startswith("//"):
                continue
            decl = TYPE_DECL.search(line)
            if decl and not line.startswith("["):
                pending = (decl.group(1), decl.group(2))
            current = stack[-1] if stack else None
            if current:
                kind, name, _ = current
                if kind == "enum":
                    attr = ENUM_NAME.search(line)
                    if attr:
                        pending_json = attr.group(1)
                    else:
                        member = ENUM_MEMBER.match(line)
                        if member and line not in ("{", "}"):
                            enums.setdefault(name, set()).add(pending_json or member.group(1))
                            pending_json = None
                else:
                    attr = JSON_NAME.search(line)
                    if attr:
                        classes.setdefault(name, set()).add(attr.group(1))
                    prop = PROPERTY.match(raw)
                    if prop:
                        classes.setdefault(name, set()).add(prop.group(1)[:1].lower() + prop.group(1)[1:])
            for char in line:
                if char == "{":
                    depth += 1
                    if pending:
                        stack.append((pending[0], pending[1], depth))
                        if pending[0] == "enum":
                            enums.setdefault(pending[1], set())
                        else:
                            classes.setdefault(pending[1], set())
                        pending = None
                elif char == "}":
                    while stack and stack[-1][2] >= depth:
                        stack.pop()
                    depth -= 1
            if pending and line.endswith(";"):
                pending = None  # e.g. "record Foo(...);" without a body
    return classes, enums


def dto_gaps(version: str) -> dict:
    py_classes, py_enums = python_types(version)
    python_types_cache.update(py_classes)
    cs_classes, cs_enums = csharp_types()
    mapping = json.loads(DTO_MAP.read_text(encoding="utf-8")) if DTO_MAP.exists() else {}
    renames = mapping.get("classes", {})
    ignore_fields = set(mapping.get("ignoreFields", []))

    missing_classes, missing_fields, missing_enums, missing_values = [], {}, [], {}
    for name, fields in sorted(py_classes.items()):
        cs_name = renames.get(name, name)
        if cs_name not in cs_classes:
            missing_classes.append(name)
            continue
        gap = sorted(fields - cs_classes[cs_name] - ignore_fields)
        if gap:
            missing_fields[name] = gap
    for name, values in sorted(py_enums.items()):
        cs_name = renames.get(name, name)
        if cs_name not in cs_enums:
            missing_enums.append(name)
            continue
        cs_values = {v.upper() for v in cs_enums[cs_name]}
        gap = sorted(v for v in values if v.upper() not in cs_values)
        if gap:
            missing_values[name] = gap
    return {"python_genai": version, "missing_classes": missing_classes, "missing_fields": missing_fields,
            "missing_enums": missing_enums, "missing_enum_values": missing_values,
            "counts": {"python_classes": len(py_classes), "python_enums": len(py_enums)}}


def check_dto(update: bool) -> tuple[bool, str]:
    version = http_json(PYPI_URL)["info"]["version"]
    current = dto_gaps(version)
    if update or not DTO_BASELINE.exists():
        DTO_BASELINE.parent.mkdir(parents=True, exist_ok=True)
        DTO_BASELINE.write_text(json.dumps(current, indent=1, sort_keys=True) + "\n", encoding="utf-8")
        return False, (f"Recorded the DTO baseline against python-genai {version}: "
                       f"{len(current['missing_classes'])} classes and {len(current['missing_fields'])} classes with "
                       f"field gaps are known.\n")

    base = json.loads(DTO_BASELINE.read_text(encoding="utf-8"))
    new_classes = [c for c in current["missing_classes"] if c not in base["missing_classes"]]
    new_enums = [e for e in current["missing_enums"] if e not in base["missing_enums"]]
    new_fields = {c: [f for f in fs if f not in base["missing_fields"].get(c, [])]
                  for c, fs in current["missing_fields"].items()}
    new_fields = {c: fs for c, fs in new_fields.items() if fs and c not in new_classes}
    new_values = {e: [v for v in vs if v not in base["missing_enum_values"].get(e, [])]
                  for e, vs in current["missing_enum_values"].items()}
    new_values = {e: vs for e, vs in new_values.items() if vs and e not in new_enums}
    fixed = [c for c in base["missing_classes"] if c not in current["missing_classes"]]

    lines = [f"Compared python-genai {version} ({current['counts']['python_classes']} classes, "
             f"{current['counts']['python_enums']} enums) with the SDK's DTOs. Baseline: python-genai "
             f"{base.get('python_genai')}. Only gaps that are **new since the baseline** are listed.\n"]
    if new_fields:
        lines.append("\n### Fields missing from existing SDK types\n")
        lines += [f"- `{c}`: {', '.join(f'`{f}`' for f in fs)}" for c, fs in sorted(new_fields.items())]
    if new_classes:
        lines.append("\n### python-genai types with no SDK equivalent\n\nAdd them, or map renamed types in "
                     "`.github/drift/dto-map.json`.\n")
        lines += [f"- `{c}`: {', '.join(f'`{f}`' for f in sorted(dto_py_fields)) or '(no fields)'}"
                  for c, dto_py_fields in ((c, python_types_cache.get(c, set())) for c in new_classes)]
    if new_values:
        lines.append("\n### Enum values missing from existing SDK enums\n")
        lines += [f"- `{e}`: {', '.join(f'`{v}`' for v in vs)}" for e, vs in sorted(new_values.items())]
    if new_enums:
        lines.append("\n### python-genai enums with no SDK equivalent\n")
        lines += [f"- `{e}`" for e in new_enums]
    if fixed:
        lines.append(f"\n{len(fixed)} type(s) from the baseline now exist in the SDK; run `--update` to record that.")
    drift = bool(new_fields or new_classes or new_values or new_enums)
    if drift:
        lines.append("\nAfter adding the missing types and fields, run "
                     "`python scripts/drift/check_drift.py dto --update` and commit `.github/drift/`.")
    else:
        lines.append("\nNo new DTO gaps.")
    return drift, "\n".join(lines) + "\n"


python_types_cache: dict[str, set[str]] = {}

CHECKS = {"models": check_models, "api": check_api, "python-genai": check_python_genai, "dto": check_dto}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("check", choices=sorted(CHECKS))
    parser.add_argument("--report", help="Write the Markdown report to this file.")
    parser.add_argument("--update", action="store_true", help="Refresh the baseline for this check.")
    args = parser.parse_args()

    drift, report = CHECKS[args.check](args.update)
    if args.report:
        Path(args.report).write_text(report, encoding="utf-8")
    print(report)
    print(f"DRIFT={'true' if drift else 'false'}")
    if os.environ.get("GITHUB_OUTPUT"):
        with open(os.environ["GITHUB_OUTPUT"], "a", encoding="utf-8") as out:
            out.write(f"drift={'true' if drift else 'false'}\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
