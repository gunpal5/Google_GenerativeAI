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


CHECKS = {"models": check_models, "api": check_api, "python-genai": check_python_genai}


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
