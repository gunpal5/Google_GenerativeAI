# Claude Code Project Memory

Read **[AGENTS.md](AGENTS.md)** first. It's the shared guide for AI agents and covers the repository layout, building,
per-framework package versions, JSON source-generation contexts, test gating and documentation. This file adds
Claude-specific conventions and the maintainer's preferences.

## Project

Google_GenerativeAI is the unofficial C# .NET SDK for Google Gemini (Gemini API) and Vertex AI. The current major version is
**4.x**. All six packages share one version (`<Version>`, `<AssemblyVersion>` and `<FileVersion>` in each
`src/*/*.csproj`).

- Owner: Gunpal Jain (GitHub: gunpal5)
- Docs: the [GitHub wiki](https://github.com/gunpal5/Google_GenerativeAI/wiki); the wiki repo is
  `https://github.com/gunpal5/Google_GenerativeAI.wiki.git`.
- API reference: https://gunpal5.github.io/generative-ai/, built with DocFX from `apidocs/` and published by the
  manual `Publish API reference` workflow to the `generative-ai` folder of `gunpal5/gunpal5.github.com`.
- Agent docs: `llms.txt`, `llms-full.txt` (regenerate with `scripts/generate-llms-full.py` after wiki changes) and
  `skills/google-generativeai-dotnet/SKILL.md`.

## Working preferences

- **Never run video generation tests or anything that generates video.** They cost credits and stay off unless the
  maintainer asks (`RUN_VIDEO_GENERATION_TESTS=true`).
- Keep the build at **0 warnings**. Verify code examples you add to the README, wiki or skill by compiling them.
- Work on a branch and merge to `main` through a pull request after CI passes. Do **not** publish NuGet packages,
  create releases or bump the version unless explicitly asked.
- Mirror python-genai and the official REST API for names and wire formats.

## Git conventions

- Branch names: `feature/description`, `fix/description`, `refactor/description` or `docs/description`.
- Conventional commits: `type: description` with `feat`, `fix`, `docs`, `refactor`, `test` or `chore`; add `!` for
  breaking changes (for example `refactor!:`).
- Machine-specific settings (commit authorship, local tooling) live in `CLAUDE.local.md`, which is not committed.

## Code style

- C# with the latest language features, nullable reference types enabled, XML documentation on all public members.
- Follow existing patterns in the codebase; keep pull requests focused.
