namespace GenerativeAI.Types;

/// <summary>
/// Model identifiers listed by the Interactions API <c>Model</c> enum (python-genai). The API also accepts other model IDs.
/// </summary>
public static class InteractionModels
{
    /// <summary>Gemma 4 26B A4B IT (<c>gemma-4-26b-a4b-it</c>).</summary>
    public const string Gemma426bA4bIt = "gemma-4-26b-a4b-it";

    /// <summary>Gemma 4 31B IT (<c>gemma-4-31b-it</c>).</summary>
    public const string Gemma431bIt = "gemma-4-31b-it";

    /// <summary>Latest release of Gemini Flash (<c>gemini-flash-latest</c>).</summary>
    public const string GeminiFlashLatest = "gemini-flash-latest";

    /// <summary>Latest release of Gemini Flash-Lite (<c>gemini-flash-lite-latest</c>).</summary>
    public const string GeminiFlashLiteLatest = "gemini-flash-lite-latest";

    /// <summary>Latest release of Gemini Pro (<c>gemini-pro-latest</c>).</summary>
    public const string GeminiProLatest = "gemini-pro-latest";

    /// <summary>Our first hybrid reasoning model which supports a 1M token context window and has thinking budgets. (<c>gemini-2.5-flash</c>).</summary>
    public const string Gemini25Flash = "gemini-2.5-flash";

    /// <summary>Our state-of-the-art multipurpose model, which excels at coding and complex reasoning tasks. (<c>gemini-2.5-pro</c>).</summary>
    public const string Gemini25Pro = "gemini-2.5-pro";

    /// <summary>Our smallest and most cost effective model, built for at scale usage. (<c>gemini-2.5-flash-lite</c>).</summary>
    public const string Gemini25FlashLite = "gemini-2.5-flash-lite";

    /// <summary>Gemini 3.5 Flash-Lite - Our fastest, most cost-effective 3.5 model for high-throughput execution. (<c>gemini-3.5-flash-lite</c>).</summary>
    public const string Gemini35FlashLite = "gemini-3.5-flash-lite";

    /// <summary>Our native image generation model, optimized for speed, flexibility, and contextual understanding. Text input and output is priced the same as 2.5 Flash. (<c>gemini-2.5-flash-image</c>).</summary>
    public const string Gemini25FlashImage = "gemini-2.5-flash-image";

    /// <summary>Our most intelligent model built for speed, combining frontier intelligence with superior search and grounding. (<c>gemini-3-flash-preview</c>).</summary>
    public const string Gemini3FlashPreview = "gemini-3-flash-preview";

    /// <summary>Our latest SOTA reasoning model with unprecedented depth and nuance, and powerful multimodal understanding and coding capabilities. (<c>gemini-3.1-pro-preview</c>).</summary>
    public const string Gemini31ProPreview = "gemini-3.1-pro-preview";

    /// <summary>Gemini 3.1 Pro Preview optimized for custom tool usage (<c>gemini-3.1-pro-preview-customtools</c>).</summary>
    public const string Gemini31ProPreviewCustomtools = "gemini-3.1-pro-preview-customtools";

    /// <summary>Our most cost-efficient model, optimized for high-volume agentic tasks, translation, and simple data processing. (<c>gemini-3.1-flash-lite</c>).</summary>
    public const string Gemini31FlashLite = "gemini-3.1-flash-lite";

    /// <summary>Gemini 3.1 Flash Lite Image. (<c>gemini-3.1-flash-lite-image</c>).</summary>
    public const string Gemini31FlashLiteImage = "gemini-3.1-flash-lite-image";

    /// <summary>Gemini 3 Pro Image (<c>gemini-3-pro-image</c>).</summary>
    public const string Gemini3ProImage = "gemini-3-pro-image";

    /// <summary>Gemini 3 Pro Image Preview (<c>nano-banana-pro-preview</c>).</summary>
    public const string NanoBananaProPreview = "nano-banana-pro-preview";

    /// <summary>Gemini 3.1 Flash Image. (<c>gemini-3.1-flash-image</c>).</summary>
    public const string Gemini31FlashImage = "gemini-3.1-flash-image";

    /// <summary>Gemini Nano Banana 2.1. (<c>gemini-nano-banana-2.1</c>).</summary>
    public const string GeminiNanoBanana21 = "gemini-nano-banana-2.1";

    /// <summary>Gemini 3.1 Flash TTS: Powerful, low-latency speech generation. Enjoy natural outputs, steerable prompts, and new expressive audio tags for precise narration control. (<c>gemini-3.1-flash-tts-preview</c>).</summary>
    public const string Gemini31FlashTtsPreview = "gemini-3.1-flash-tts-preview";

    /// <summary>Gemini 3.5 Flash - Our earlier Flash model, built for speed and foundational performance across routine, high-throughput workloads. (<c>gemini-3.5-flash</c>).</summary>
    public const string Gemini35Flash = "gemini-3.5-flash";

    /// <summary>Gemini 3.6 Flash - Our previous generation Flash model, balancing speed and multimodal capabilities across general agentic and everyday tasks. (<c>gemini-3.6-flash</c>).</summary>
    public const string Gemini36Flash = "gemini-3.6-flash";

    /// <summary>Gemini 3.7 Flash - Our high-speed, efficient Flash model built for everyday coding, agentic tool use, and reliable multi-step execution. (<c>gemini-3.7-flash</c>).</summary>
    public const string Gemini37Flash = "gemini-3.7-flash";

    /// <summary>Gemini 3.8 Flash - Our most intelligent Flash model, engineered for long-horizon software engineering, autonomous agents, and complex enterprise workflows. (<c>gemini-3.8-flash</c>).</summary>
    public const string Gemini38Flash = "gemini-3.8-flash";

    /// <summary>Gemini 4 Argon - Our new frontier model, built to solve the hardest problems with depth and precision across real-world software engineering and knowledge work. (<c>gemini-4-argon</c>).</summary>
    public const string Gemini4Argon = "gemini-4-argon";

    /// <summary>Gemini 4 Carbon - Our most capable reasoning model, built for sustained agentic coding, multimodal knowledge work, scientific discovery, and cybersecurity defense. (<c>gemini-4-carbon</c>).</summary>
    public const string Gemini4Carbon = "gemini-4-carbon";

    /// <summary>Gemini 4 Hydrogen - A model with frontier-level intelligence, built for fast, dependable execution across coding, agentic workflows, and multimodal knowledge work. (<c>gemini-4-hydrogen</c>).</summary>
    public const string Gemini4Hydrogen = "gemini-4-hydrogen";

    /// <summary>Gemini 4 Neon - Frontier-level intelligence with fast end-to-end task completion across complex software engineering, analytical, and science workflows. (<c>gemini-4-neon</c>).</summary>
    public const string Gemini4Neon = "gemini-4-neon";

    /// <summary>Gemini 3.8 Flash TTS - Flagship TTS model for Voice Design and dual-speaker screenplay control. Prompt custom vocal personas, direct line-by-line delivery, and add vocal bursts. (<c>gemini-3.8-flash-tts</c>).</summary>
    public const string Gemini38FlashTts = "gemini-3.8-flash-tts";

    /// <summary>Gemini 3.8 Flash Lite TTS - High-speed and cost-efficient, ideal for rapid dubbing, media localization, and high-throughput voice agents. Direct replacement for gemini-3.1-flash-tts-preview. (<c>gemini-3.8-flash-lite-tts</c>).</summary>
    public const string Gemini38FlashLiteTts = "gemini-3.8-flash-lite-tts";

    /// <summary>Gemini 3.5 Transcribe - Low-latency speech-to-text model with utterance-based language detection, speaker diarization, word-level timestamps, and custom vocabulary biasing. (<c>gemini-3.5-transcribe</c>).</summary>
    public const string Gemini35Transcribe = "gemini-3.5-transcribe";

    /// <summary>Our low-latency, music generation model optimized for high-fidelity audio clips and precise rhythmic control. (<c>lyria-3-clip-preview</c>).</summary>
    public const string Lyria3ClipPreview = "lyria-3-clip-preview";

    /// <summary>Our advanced, full-song generative model with deep compositional understanding, optimized for precise structural control and complex transitions across diverse musical styles. (<c>lyria-3-pro-preview</c>).</summary>
    public const string Lyria3ProPreview = "lyria-3-pro-preview";

    /// <summary>Gemini Robotics-ER 1.6 Preview (<c>gemini-robotics-er-1.6-preview</c>).</summary>
    public const string GeminiRoboticsEr16Preview = "gemini-robotics-er-1.6-preview";

    /// <summary>Gemini Robotics Embodied Reasoning 2 Preview (<c>gemini-robotics-er-2-preview</c>).</summary>
    public const string GeminiRoboticsEr2Preview = "gemini-robotics-er-2-preview";

    /// <summary>Our flagship music generation model, optimized for full-length songs with complex structural coherence. (<c>lyria-3.5</c>).</summary>
    public const string Lyria35 = "lyria-3.5";

    /// <summary>Gemini Omni 1.2 Flash - Our latest multimodal model for fast, conversational video generation, editing, and cinematic control. (<c>gemini-omni-1.2-flash</c>).</summary>
    public const string GeminiOmni12Flash = "gemini-omni-1.2-flash";

    /// <summary>Our high-performance multimodal model designed for fast, conversational video generation, editing, and cinematic control. (<c>gemini-omni-1.1-flash</c>).</summary>
    public const string GeminiOmni11Flash = "gemini-omni-1.1-flash";

    /// <summary>Preview release of our multimodal model for conversational video generation, editing, and cinematic control. (<c>gemini-omni-flash-preview</c>).</summary>
    public const string GeminiOmniFlashPreview = "gemini-omni-flash-preview";

}

/// <summary>
/// Agent identifiers listed by the Interactions API <c>AgentOption</c> enum (python-genai). The API also accepts other agent IDs.
/// </summary>
public static class InteractionAgents
{
    /// <summary>Gemini Deep Research Agent (<c>deep-research-pro-preview-12-2025</c>).</summary>
    public const string DeepResearchProPreview122025 = "deep-research-pro-preview-12-2025";

    /// <summary>Gemini Deep Research Agent (<c>deep-research-preview-04-2026</c>).</summary>
    public const string DeepResearchPreview042026 = "deep-research-preview-04-2026";

    /// <summary>Gemini Deep Research Max Agent (<c>deep-research-max-preview-04-2026</c>).</summary>
    public const string DeepResearchMaxPreview042026 = "deep-research-max-preview-04-2026";

    /// <summary>Use the Antigravity managed agent to perform multi-step tasks that require reasoning, file operations, and tool use. (<c>antigravity-preview-05-2026</c>).</summary>
    public const string AntigravityPreview052026 = "antigravity-preview-05-2026";

    /// <summary>Use the Antigravity managed agent to perform multi-step tasks that require reasoning, file operations, and tool use. (<c>antigravity-preview-09-2026</c>).</summary>
    public const string AntigravityPreview092026 = "antigravity-preview-09-2026";

}
