# Contributing to Google GenerativeAI .NET SDK

Thanks for helping improve the SDK! Bug reports, documentation fixes, new API coverage and samples are all welcome.

## Ways to contribute

- **Report a bug:** open an [issue](https://github.com/gunpal5/Google_GenerativeAI/issues) with the package version,
  target framework, platform (Gemini API or Vertex AI), the model you used, a minimal code sample, and the full
  exception (`ApiException` includes `ErrorCode`, `ErrorStatus` and `ErrorMessage`). Remove API keys and project ids.
- **Request a feature:** open an issue that links the Google API documentation (or the python-genai equivalent) for
  the feature.
- **Improve documentation:** the user docs live in the [wiki](https://github.com/gunpal5/Google_GenerativeAI/wiki).
  Suggest changes in an issue or pull request.
- **Send a pull request:** for anything larger than a small fix, open an issue first so we can agree on the approach.

## Development setup

**Requirements:**

- The .NET 10 SDK (install the .NET 11 SDK too if you want to build the `net11.0` target).
- An IDE such as Rider, Visual Studio or VS Code with C# Dev Kit.

```bash
git clone https://github.com/gunpal5/Google_GenerativeAI.git
cd Google_GenerativeAI
dotnet build Google_GenerativeAI.sln
```

The build must finish with **0 warnings**. See [`AGENTS.md`](AGENTS.md) for the repository layout and the
conventions below in more detail. AI coding agents should read that file.

## Coding guidelines

- **Follow the official APIs:** model new types on the Google REST/WebSocket APIs and the
  [python-genai](https://github.com/googleapis/python-genai) SDK. Use the same type names, and set
  `[JsonPropertyName]` to the camelCase wire name.
- **Support every target framework:** the libraries build for .NET 6 to .NET 11, .NET Standard 2.0 and .NET Framework 4.6.2.
  Guard newer APIs with `#if NET6_0_OR_GREATER` and keep the per-framework package groups in the `.csproj` files.
- **Stay NativeAOT-safe:** register every new serializable type in the matching `JsonSerializerContext`
  (`TypesSerializerContext` or the feature-specific contexts), and don't use reflection-based serialization.
- **Write public API docs:** every public member needs XML documentation.
- **Cover both platforms:** when a feature exists on both the Gemini API and Vertex AI, handle both, or fail clearly
  (for example with `NotSupportedException`) on the one that lacks it.
- **Keep changes focused:** keep unrelated formatting and refactoring out of a pull request.

## Tests

The tests use xUnit v3 and Shouldly.

```bash
dotnet test Google_GenerativeAI.sln
```

- **Add offline unit tests for every change.** Capture HTTP calls with a fake `HttpMessageHandler` and assert on the
  request JSON, so the tests need no credentials. `tests/GenerativeAI.Tests/Clients/WebhooksClient_Tests.cs` is a
  good example.
- **Live tests** call real Google APIs and skip automatically without credentials:
  - Gemini API: set `GOOGLE_API_KEY`.
  - Vertex AI: set `VERTEX_AI_TESTS_ENABLED=true`, `GOOGLE_PROJECT_ID` and `GOOGLE_REGION`, and sign in with
    `gcloud auth application-default login`. Regional services such as the RAG Engine use `VERTEX_REGIONAL_LOCATION`
    (default `us-central1`).
- **Costly tests are opt-in:** video generation runs only with `RUN_VIDEO_GENERATION_TESTS=true`, and voice design
  only with `LIVE_TESTS_DESIGN_VOICE=true`.
- **Write new live tests the same way:** skip when their credentials are missing, and delete anything they create.

## Pull requests

1. Fork the repository and create a branch: `feature/...`, `fix/...`, `refactor/...` or `docs/...`.
2. Write [conventional commit](https://www.conventionalcommits.org/) messages (`feat:`, `fix:`, `docs:`, `refactor:`,
   `test:`, `chore:`). Add `!` for breaking changes, for example `refactor!:`.
3. Check that `dotnet build` shows 0 warnings and that the tests pass.
4. Update the wiki page and README if user-facing behaviour changes. Describe breaking changes in the pull request so
   they can go into the migration guide.
5. Open the pull request against `main` with a clear description and a link to the related issue. CI builds and tests
   every pull request.

## Releases

Maintainers publish releases. All six packages share one version and follow [semantic versioning](https://semver.org/).

## License

By contributing, you agree that your contributions are licensed under the [MIT License](LICENSE).
