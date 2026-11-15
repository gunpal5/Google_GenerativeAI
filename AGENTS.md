# AGENTS.md

Guidance for AI coding agents (and humans) contributing to this repository. For *using* the SDK in another project,
see [`skills/google-generativeai-dotnet`](skills/google-generativeai-dotnet/SKILL.md) and [`llms.txt`](llms.txt).

## Repository layout

| Path | Contents |
|---|---|
| `src/GenerativeAI` | Core SDK: platforms (`GoogleAi`, `VertexAI`, adapters), models, clients, types, constants |
| `src/GenerativeAI.Tools` | Function tools (`QuickTool`, source-generated tools) and MCP integration |
| `src/GenerativeAI.Auth` | OAuth, service account and ADC authenticators |
| `src/GenerativeAI.Microsoft` | `Microsoft.Extensions.AI` implementations |
| `src/GenerativeAI.Web` | ASP.NET Core dependency injection |
| `src/GenerativeAI.Live` | Multimodal Live API and Live Music WebSocket clients |
| `tests/*` | xUnit v3 test projects; `tests/AotTest` checks NativeAOT compatibility |
| `samples/*` | Runnable sample apps |
| `scripts/generate-llms-full.py` | Regenerates `llms-full.txt` from the GitHub wiki |

## Build

```bash
dotnet build Google_GenerativeAI.sln
```

- Libraries target `net6.0`-`net10.0`, `netstandard2.0` and `net462`; `net11.0` is added only when the .NET 11 SDK
  is installed (`NETCoreSdkVersion >= 11`).
- The build must stay at **0 warnings**. Analyzer warnings (CA*) count; fix them rather than suppressing them.
- **Package versions differ per target framework.** `Microsoft.Extensions.*` and `Microsoft.Extensions.AI.Abstractions`
  11.x only support net10+, netstandard2.0 and net462, so the `.csproj` files use conditional `ItemGroup`s: 11.x for
  net10+/netstandard2.0/net462, 10.x for net8/net9, older versions for net6/net7. Keep this pattern when updating
  packages.
- Keep code compatible with every target: guard newer APIs with `#if NET6_0_OR_GREATER` (see existing
  `ArgumentNullException.ThrowIfNull` patterns).

## Adding API types (DTOs)

- Mirror the official Google APIs and the python-genai SDK: same type names (for example `LiveClientMessage`,
  `LiveServerContent`) and `[JsonPropertyName]` values that match the REST/WebSocket JSON (camelCase).
- **Register every new type with System.Text.Json source generation**, or NativeAOT and trimmed apps fail at runtime:
  - Most types: `src/GenerativeAI/Types/TypesSerializerContext.cs`.
  - Feature areas with their own contexts: `FileSearchSerializerContext`, `InteractionsSerializerContext`,
    `WebhooksSerializerContext`, `TuningSerializerContext`, `AgentApisSerializerContext`.
- Enums serialized as strings use `[JsonConverter(typeof(JsonStringEnumConverter<TEnum>))]`.
- Add XML documentation to every public member (the build treats missing docs as warnings).
- Model ids go in `src/GenerativeAI/Constants/GoogleAIModels.cs` or `VertexAIModels.cs`. Retired models stay as
  constants marked `[Obsolete("Shut down on <date>. Use <replacement>.", false)]`.

## Tests

- xUnit v3 with Shouldly. `tests/Directory.Build.props` sets `IsTestingPlatformApplication=false` so the VSTest runner
  keeps working with xUnit v3 4.x on the .NET 10+ SDK; don't remove it.
- Prefer **offline unit tests**: capture HTTP requests with a fake `HttpMessageHandler` and assert on the JSON
  (see `tests/GenerativeAI.Tests/Clients/WebhooksClient_Tests.cs`).
- Live tests call real Google APIs and **skip** unless credentials are configured:
  - Gemini API: `GOOGLE_API_KEY` (some tests use `GOOGLE_API_KEY_TEST`).
  - Vertex AI: `VERTEX_AI_TESTS_ENABLED=true`, Application Default Credentials, `GOOGLE_PROJECT_ID`, `GOOGLE_REGION`;
    regional services use `VERTEX_REGIONAL_LOCATION` (default `us-central1`) because the RAG Engine is not
    available on `global`.
  - Optional resources: `GCS_TEST_BUCKET`, `GCS_TEST_FILE_URI`, `PINECONE_*`, `RAG_IMPORT_DRIVE_FILE_ID`,
    `SEMANTIC_TESTS_ENABLED`.
- **Never enable video generation tests casually.** They cost money and only run with `RUN_VIDEO_GENERATION_TESTS=true`.
  Voice design runs only with `LIVE_TESTS_DESIGN_VOICE=true`.
- New live tests must skip cleanly when their credentials or resources are missing (`Assert.SkipWhen` /
  `Assert.SkipUnless`) and clean up what they create.

## Behaviour to preserve

- Function results are sent with the `user` role (Gemini 3.x rejects `function`); `GenerativeModel.PrepareRequest`
  converts legacy `function`-role content.
- Gemini API and Vertex AI share one API surface; check both paths (`Platform is VertextPlatformAdapter`) when you
  change URLs or payloads. Vertex AI often needs a different request shape (embeddings, video, batch).

## Documentation

- User documentation lives in the [GitHub wiki](https://github.com/gunpal5/Google_GenerativeAI/wiki). Update the
  relevant page when you change public behaviour, then regenerate `llms-full.txt`:
  `python scripts/generate-llms-full.py <wiki-clone> llms-full.txt`.
- Keep `README.md` short; put details on wiki pages and link to them.
- The API reference (https://gunpal5.github.io/generative-ai/) is generated with DocFX from `apidocs/`
  (`dotnet tool restore`, then `dotnet docfx apidocs/docfx.json`) and published by the manual
  `Publish API reference` workflow.

## Keeping up with Google's APIs

The weekly **API drift check** workflow (`.github/workflows/api-drift.yml`) runs `scripts/drift/check_drift.py` and
keeps one open issue (label `api-drift`) per kind of drift:

- **models**: new Gemini API models without a constant in `GoogleAIModels`, or constants whose model has disappeared
  (needs the `GEMINI_API_KEY` repository secret).
- **api**: new or removed request/response types and fields in the Gemini API discovery document.
- **python-genai**: new releases of python-genai, the reference SDK.
- **dto**: types, fields and enum values in python-genai's `types.py` that the SDK's DTOs don't have yet. The check
  parses `types.py` (without running it) and the C# sources (`[JsonPropertyName]` and property names), matches classes by
  name, and reports only gaps that are new since `.github/drift/dto-baseline.json`. Renamed types and Python-only
  fields go in `.github/drift/dto-map.json`.

After handling a drift, refresh its baseline and commit `.github/drift/`:
`python scripts/drift/check_drift.py <models|api|python-genai|dto> --update`. Model IDs that intentionally have no
constant go in `.github/drift/models-ignore.txt`. To see every known DTO gap (not just new ones), read
`.github/drift/dto-baseline.json`.

The weekly **DTO update** workflow (`.github/workflows/dto-update.yml`) closes DTO gaps automatically. It runs
`scripts/drift/update_dtos.py --apply`, which adds missing fields and enum values to existing types, marks fields that
python-genai removed as `[Obsolete]`, and generates a new type (in `src/GenerativeAI/Types/Generated/`, registered in
`TypesSerializerContext`) only when an added field needs it. It never adds client methods. When the `ANTHROPIC_API_KEY`
secret is set, Claude Haiku (`scripts/drift/dto_review.py`) reviews the proposals first: it can leave out Python-only
ones and writes the XML doc summaries and the summary, but never writes code. The workflow first opens or updates
the issue "[Drift] DTO update from python-genai" listing the changes. If the build has 0 warnings and the tests pass,
it then opens or updates a pull request from `drift/dto-update` that closes the issue; otherwise it comments on the
issue with the build or test output. Run `python scripts/drift/update_dtos.py` (dry run) to
preview; add `--no-ai` to skip the review. Python-only types and fields go in `.github/drift/dto-map.json`
(`ignoreTypes`, `ignoreFields`, `ignoreClassFields`).

## Git conventions

- Conventional commits (`feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `chore:`); use `!` for breaking changes.
- Branch names: `feature/...`, `fix/...`, `refactor/...`, `docs/...`.
