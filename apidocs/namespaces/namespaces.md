---
uid: GenerativeAI
summary: *content
---
Entry points of the core SDK (`Google_GenerativeAI` package): the `GoogleAi` (Gemini API) and `VertexAI` platforms
and their adapters, `GenerativeModel`, `ChatSession`, `EmbeddingModel`, `VideoGenerationModel`, the model constants
(`GoogleAIModels`, `VertexAIModels`) and the extension methods for building requests and reading responses.

---
uid: GenerativeAI.Types
summary: *content
---
Request and response types (DTOs) for the Gemini API and Vertex AI: content, parts, generation and tool configuration,
the Live API messages (`LiveClientMessage`, `LiveServerMessage`), batch, caching, files, File Search, Interactions,
tuning, webhooks and agent API types. Names and JSON property names follow the official REST API and python-genai.

---
uid: GenerativeAI.Types.RagEngine
summary: *content
---
Vertex AI RAG Engine: `VertexRagManager`, RAG corpora and files, import and upload configuration, retrieval settings
and the RAG Engine configuration (Serverless or Spanner mode).

---
uid: GenerativeAI.Types.Converters
summary: *content
---
JSON converters used by the SDK's types, such as tolerant enum converters and `DateOnly` / `TimeOnly` converters.

---
uid: GenerativeAI.Clients
summary: *content
---
Low-level REST clients for individual API resources: files, models, caching, batch, File Search stores,
Interactions, tuning, webhooks, ephemeral auth tokens, agent APIs and semantic retrieval.

---
uid: GenerativeAI.Core
summary: *content
---
Core abstractions shared by the SDK: the `ApiBase` HTTP layer, `IPlatformAdapter` and `IGoogleAuthenticator`,
function tool interfaces (`IFunctionTool`, `GoogleFunctionTool`), `FunctionCallingBehaviour` and credential types.

---
uid: GenerativeAI.Exceptions
summary: *content
---
Exceptions thrown by the SDK. `ApiException` carries the parsed API error (`ErrorCode`, `ErrorStatus`,
`ErrorMessage`); `VertexAIException` reports failed Vertex AI long-running operations.

---
uid: GenerativeAI.Helpers
summary: *content
---
Helpers for building JSON Lines input files for batch jobs.

---
uid: GenerativeAI.Logging
summary: *content
---
Source-generated logging messages used by the core SDK.

---
uid: GenerativeAI.Utility
summary: *content
---
Utilities for extracting code and JSON blocks from Markdown and describing .NET types for prompts.

---
uid: GenerativeAI.Authenticators
summary: *content
---
Authenticators for Google Cloud credentials: Application Default Credentials, OAuth and service accounts. The
advanced authenticators are in the `Google_GenerativeAI.Auth` package.

---
uid: GenerativeAI.Tools
summary: *content
---
Function calling tools (`Google_GenerativeAI.Tools` package): `QuickTool` for wrapping delegates, generic tool
collections and support for source-generated tools.

---
uid: GenerativeAI.Tools.Mcp
summary: *content
---
Model Context Protocol (MCP) integration: `McpTool` exposes the tools of any MCP server to Gemini, and
`McpTransportFactory` creates stdio and HTTP transports.

---
uid: GenerativeAI.Tools.Extensions
summary: *content
---
Extension methods for converting OpenAPI schemas into Gemini function declarations.

---
uid: GenerativeAI.Tools.Helpers
summary: *content
---
Helpers for generating function schemas from .NET methods.

---
uid: GenerativeAI.Microsoft
summary: *content
---
`Microsoft.Extensions.AI` implementations (`Google_GenerativeAI.Microsoft` package): `GenerativeAIChatClient`
(`IChatClient`), `GenerativeAIEmbeddingGenerator` and the image generators.

---
uid: GenerativeAI.Microsoft.Extensions
summary: *content
---
Conversions between `Microsoft.Extensions.AI` types (messages, contents, options, tools) and Gemini request and
response types.

---
uid: GenerativeAI.Web
summary: *content
---
ASP.NET Core integration (`Google_GenerativeAI.Web` package): `AddGenerativeAI` dependency injection,
`GenerativeAIOptions` and `IGenerativeAiService`.

---
uid: GenerativeAI.Live
summary: *content
---
Real-time APIs over WebSockets (`Google_GenerativeAI.Live` package): `MultiModalLiveClient` for the Multimodal Live
API and `LiveMusicClient` for Lyria RealTime music generation, with their events.

---
uid: GenerativeAI.Live.Events
summary: *content
---
Event argument types raised by the Live API clients.

---
uid: GenerativeAI.Live.Extensions
summary: *content
---
Extension methods for creating Live API clients from generative models.

---
uid: GenerativeAI.Live.Helper
summary: *content
---
Audio helpers for the Live API, such as adding WAV headers to PCM audio.

---
uid: GenerativeAI.Live.Logging
summary: *content
---
Source-generated logging messages used by the Live API clients.
