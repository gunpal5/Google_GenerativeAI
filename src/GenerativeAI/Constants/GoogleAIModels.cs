namespace GenerativeAI;

/// <summary>
/// Provides constants for Gemini model variations and related information.
/// </summary>
/// <remarks>
/// This class defines constants based on the Gemini models documented at:
/// <seealso href="https://ai.google.dev/gemini-api/docs/models"/>.
/// Shutdown and deprecation dates come from
/// <seealso href="https://ai.google.dev/gemini-api/docs/deprecations"/>.
/// Constants for models that are shut down or deprecated are kept for compatibility and marked
/// <c>[Obsolete]</c> with the recommended replacement.
/// </remarks>
public static class GoogleAIModels
{
    /// <summary>
    /// Represents the default Gemini AI model identifier used by the Generative AI services.
    /// </summary>
    public const string DefaultGeminiModel = Gemini38Flash;

    #region Gemini 4

    /// <summary>
    /// Gemini 4 Hydrogen model name (GA, released November 12, 2026).
    /// </summary>
    public const string Gemini4Hydrogen = "models/gemini-4-hydrogen";

    /// <summary>
    /// Gemini 4 Carbon model name (GA, released November 10, 2026).
    /// </summary>
    public const string Gemini4Carbon = "models/gemini-4-carbon";

    /// <summary>
    /// Gemini 4 Argon model name (GA, released October 13, 2026).
    /// </summary>
    public const string Gemini4Argon = "models/gemini-4-argon";

    #endregion

    #region Gemini 3.x

    /// <summary>
    /// Gemini 3.8 Flash model name (stable).
    /// </summary>
    public const string Gemini38Flash = "models/gemini-3.8-flash";

    /// <summary>
    /// Gemini 3.6 Flash model name (stable).
    /// </summary>
    public const string Gemini36Flash = "models/gemini-3.6-flash";

    /// <summary>
    /// Gemini 3.5 Flash-Lite model name (stable).
    /// </summary>
    public const string Gemini35FlashLite = "models/gemini-3.5-flash-lite";

    /// <summary>
    /// Gemini 3.1 Flash-Lite model name (stable, scheduled for shutdown on May 7, 2027).
    /// </summary>
    public const string Gemini31FlashLite = "models/gemini-3.1-flash-lite";

    /// <summary>
    /// Gemini 3.1 Pro Preview model name.
    /// </summary>
    public const string Gemini31ProPreview = "models/gemini-3.1-pro-preview";

    /// <summary>
    /// Gemini 3.1 Pro Preview variant tuned for custom tools.
    /// </summary>
    public const string Gemini31ProPreviewCustomTools = "models/gemini-3.1-pro-preview-customtools";

    /// <summary>
    /// Gemini 3 Flash Preview model name.
    /// Supports advanced thinking features with thought signatures for function calling.
    /// Google recommends <see cref="Gemini36Flash"/> as its replacement.
    /// </summary>
    public const string Gemini3FlashPreview = "gemini-3-flash-preview";

    #endregion

    #region Live, speech and TTS

    /// <summary>
    /// Gemini 3.8 Live model name for the Live API (GA).
    /// </summary>
    public const string Gemini38Live = "models/gemini-3.8-live";

    /// <summary>
    /// Gemini 3.8 Live model with extended thinking for the Live API (GA).
    /// </summary>
    public const string Gemini38LiveExtendedThinking = "models/gemini-3.8-live-extended-thinking";

    /// <summary>
    /// Gemini 3.5 Live Translate Preview model name for real-time speech translation.
    /// </summary>
    public const string Gemini35LiveTranslatePreview = "models/gemini-3.5-live-translate-preview";

    /// <summary>
    /// Gemini 3.1 Flash Live Preview model name.
    /// </summary>
    public const string Gemini31FlashLivePreview = "models/gemini-3.1-flash-live-preview";

    /// <summary>
    /// Gemini 2.5 Flash Native Audio Preview (December 2025) model name. Scheduled for shutdown on March 31, 2027.
    /// </summary>
    public const string Gemini25FlashNativeAudioPreview122025 = "models/gemini-2.5-flash-native-audio-preview-12-2025";

    /// <summary>
    /// Gemini 3.5 Transcribe model name for speech-to-text.
    /// </summary>
    public const string Gemini35Transcribe = "models/gemini-3.5-transcribe";

    /// <summary>
    /// Gemini 3.5 Transcribe Live model name for streaming speech-to-text.
    /// </summary>
    public const string Gemini35TranscribeLive = "models/gemini-3.5-transcribe-live";

    /// <summary>
    /// Gemini 3.8 Flash text-to-speech model name (GA).
    /// </summary>
    public const string Gemini38FlashTts = "models/gemini-3.8-flash-tts";

    /// <summary>
    /// Gemini 3.8 Flash-Lite text-to-speech model name (GA).
    /// </summary>
    public const string Gemini38FlashLiteTts = "models/gemini-3.8-flash-lite-tts";

    /// <summary>
    /// Gemini 3.1 Flash TTS Preview model name. Scheduled for shutdown on December 18, 2026.
    /// </summary>
    public const string Gemini31FlashTtsPreview = "models/gemini-3.1-flash-tts-preview";

    #endregion

    #region Image generation

    /// <summary>
    /// Gemini Nano Banana 2.1 image generation and editing model name (GA).
    /// </summary>
    public const string GeminiNanoBanana21 = "models/gemini-nano-banana-2.1";

    /// <summary>
    /// Gemini 3.1 Flash Image model name (GA).
    /// </summary>
    public const string Gemini31FlashImage = "models/gemini-3.1-flash-image";

    /// <summary>
    /// Gemini 3.1 Flash-Lite Image model name (GA).
    /// </summary>
    public const string Gemini31FlashLiteImage = "models/gemini-3.1-flash-lite-image";

    /// <summary>
    /// Gemini 3 Pro Image model name (GA).
    /// </summary>
    public const string Gemini3ProImage = "models/gemini-3-pro-image";

    /// <summary>
    /// Gemini 2.5 Flash Image model name (stable). Scheduled for shutdown on March 15, 2027.
    /// </summary>
    public const string Gemini25FlashImage = "models/gemini-2.5-flash-image";

    #endregion

    #region Embeddings, robotics and specialized models

    /// <summary>
    /// Gemini Embedding 2 model name (stable).
    /// </summary>
    public const string GeminiEmbedding2 = "models/gemini-embedding-2";

    /// <summary>
    /// Gemini Embedding 001 model name (stable, scheduled for shutdown on May 14, 2028).
    /// </summary>
    public const string GeminiEmbedding = "models/gemini-embedding-001";

    /// <summary>
    /// Gemini Robotics-ER 2 Preview model name.
    /// </summary>
    public const string GeminiRoboticsEr2Preview = "models/gemini-robotics-er-2-preview";

    /// <summary>
    /// Gemini Robotics-ER 2 Streaming Preview model name.
    /// </summary>
    public const string GeminiRoboticsEr2StreamingPreview = "models/gemini-robotics-er-2-streaming-preview";

    /// <summary>
    /// Gemini Decision 1 model name, used by the Decisions API.
    /// </summary>
    public const string GeminiDecision1 = "models/gemini-decision-1";

    #endregion

    #region Gemini 2.5

    /// <summary>
    /// Gemini 2.5 Flash Lite (stable; not deprecated).
    /// </summary>
    public const string Gemini25FlashLite = "models/gemini-2.5-flash-lite";

    /// <summary>
    /// Gemini 2.5 Flash.
    /// </summary>
    [Obsolete("gemini-2.5-flash is deprecated. From December 1, 2026, requests are routed to gemini-3.5-flash-lite. Use Gemini38Flash or Gemini35FlashLite.", false)]
    public const string Gemini25Flash = "models/gemini-2.5-flash";

    /// <summary>
    /// Gemini 2.5 Pro.
    /// </summary>
    [Obsolete("gemini-2.5-pro is deprecated. From December 1, 2026, requests are routed to gemini-3.8-flash. Use Gemini38Flash or a Gemini 4 model.", false)]
    public const string Gemini25Pro = "models/gemini-2.5-pro";

    /// <summary>
    /// Gemini 2.5 Flash Preview TTS model name.
    /// </summary>
    [Obsolete("Scheduled for shutdown on December 18, 2026. Use Gemini38FlashTts.", false)]
    public const string Gemini25FlashPreviewTts = "gemini-2.5-flash-preview-tts";

    /// <summary>
    /// Gemini 2.5 Pro Preview TTS model name.
    /// </summary>
    [Obsolete("Scheduled for shutdown on December 18, 2026. Use Gemini38FlashTts.", false)]
    public const string Gemini25ProPreviewTts = "gemini-2.5-pro-preview-tts";

    #endregion

    #region Latest aliases

    /// <summary>
    /// Gemini Flash Latest - alias that Google re-points to the latest Gemini Flash release.
    /// </summary>
    public const string GeminiFlashLatest = "models/gemini-flash-latest";

    /// <summary>
    /// Gemini Flash-Lite Latest - alias that Google re-points to the latest Gemini Flash-Lite release.
    /// </summary>
    public const string GeminiFlashLiteLatest = "models/gemini-flash-lite-latest";

    /// <summary>
    /// Gemini Pro Latest - alias that Google re-points to the latest Gemini Pro release.
    /// </summary>
    public const string GeminiProLatest = "models/gemini-pro-latest";

    #endregion

    #region Gemma

    /// <summary>
    /// Gemma 4 26B (A4B mixture-of-experts) instruction-tuned model name.
    /// </summary>
    public const string Gemma4_26B_A4B = "models/gemma-4-26b-a4b-it";

    /// <summary>
    /// Gemma 4 31B instruction-tuned model name.
    /// </summary>
    public const string Gemma4_31B = "models/gemma-4-31b-it";

    /// <summary>
    /// Gemma 3.0 1B model name.
    /// </summary>
    public const string Gemma3_1B = "models/gemma-3-1b-it";

    /// <summary>
    /// Gemma 3.0 4B model name.
    /// </summary>
    public const string Gemma3_4B = "models/gemma-3-4b-it";

    /// <summary>
    /// Gemma 3.0 12B model name.
    /// </summary>
    public const string Gemma3_12B = "models/gemma-3-12b-it";

    /// <summary>
    /// Gemma 3.0 27B model name.
    /// </summary>
    public const string Gemmma3_27B = "gemma-3-27b-it";

    /// <summary>
    /// Gemma 3n E4B model name (efficient version).
    /// </summary>
    public const string Gemma3n_E4B = "models/gemma-3n-e4b-it";

    /// <summary>
    /// Gemma 3n E2B model name (efficient version).
    /// </summary>
    public const string Gemma3n_E2B = "models/gemma-3n-e2b-it";

    #endregion

    #region Shut down or no longer listed

    /// <summary>
    /// Gemini 3 Pro Preview model name.
    /// </summary>
    [Obsolete("Shut down on March 9, 2026. Use Gemini31ProPreview or a Gemini 4 model.", false)]
    public const string Gemini3ProPreview = "gemini-3-pro-preview";

    /// <summary>
    /// Gemini 3 Pro Image Preview model name.
    /// </summary>
    [Obsolete("Shut down on June 25, 2026. Use Gemini3ProImage.", false)]
    public const string Gemini3ProImagePreview = "gemini-3-pro-image-preview";

    /// <summary>
    /// Gemini 2.5 Flash-Lite Preview 06-17 model name.
    /// </summary>
    [Obsolete("No longer listed in the Gemini API model documentation. Use Gemini25FlashLite or Gemini35FlashLite.", false)]
    public const string Gemini25FlashLitePreview0617 = "models/gemini-2.5-flash-lite-preview-06-17";

    /// <summary>
    /// Gemini 2.0 Flash model name.
    /// </summary>
    [Obsolete("Shut down on June 1, 2026. Use Gemini38Flash.", false)]
    public const string Gemini2Flash = "models/gemini-2.0-flash";

    /// <summary>
    /// Gemini 2.0 Flash Lite model name.
    /// </summary>
    [Obsolete("Shut down on June 1, 2026. Use Gemini35FlashLite.", false)]
    public const string Gemini2FlashLite = "models/gemini-2.0-flash-lite";

    /// <summary>
    /// Gemini 2.5 Pro Exp 03-25 model name.
    /// </summary>
    [Obsolete("No longer listed in the Gemini API model documentation. Use a Gemini 4 model.", false)]
    public const string Gemini25ProExp0325 = "gemini-2.5-pro-exp-03-25";

    /// <summary>
    /// Gemini 2.0 Flash Exp 01-21 model name.
    /// </summary>
    [Obsolete("No longer listed in the Gemini API model documentation. Use Gemini38Flash.", false)]
    public const string Gemini2FlashThinkingExp0121 = "gemini-2.0-flash-thinking-exp-01-21";

    /// <summary>
    /// Gemini 2.5 Pro Preview model name from March 25th release.
    /// </summary>
    [Obsolete("Shut down on December 2, 2025. Use a Gemini 4 model.", false)]
    public const string Gemini25ProPreview0325 = "gemini-2.5-pro-preview-03-25";

    /// <summary>
    /// Gemini 2.5 Flash Preview 05-20 model name (despite the constant name, this is a Flash model).
    /// </summary>
    [Obsolete("Shut down on November 18, 2025. Use Gemini38Flash.", false)]
    public const string Gemini25ProPreview0520 = "gemini-2.5-flash-preview-05-20";

    /// <summary>
    /// Gemini 2.0 Flash Live model name version 001.
    /// </summary>
    [Obsolete("Shut down on December 9, 2025. Use Gemini38Live.", false)]
    public const string Gemini2FlashLive001 = "gemini-2.0-flash-live-001";

    /// <summary>
    /// Gemini 2.5 Flash Preview model name from April 17th release.
    /// </summary>
    [Obsolete("No longer listed in the Gemini API model documentation. Use Gemini38Flash.", false)]
    public const string Gemini25FlashPreview0417 = "gemini-2.5-flash-preview-04-17";

    /// <summary>
    /// Gemini 2.5 Flash Preview Native Audio Dialog model name.
    /// </summary>
    [Obsolete("No longer listed in the Gemini API model documentation. Use Gemini38Live.", false)]
    public const string Gemini25FlashPreviewNativeAudioDialog = "gemini-2.5-flash-preview-native-audio-dialog";

    /// <summary>
    /// Gemini 2.5 Flash Preview Native Audio Thinking Dialog model name.
    /// </summary>
    [Obsolete("No longer listed in the Gemini API model documentation. Use Gemini38LiveExtendedThinking.", false)]
    public const string Gemini25FlashPreviewNativeAudioThinkingDialog = "gemini-2.5-flash-exp-native-audio-thinking-dialog";

    /// <summary>
    /// Gemini Live 2.5 Flash Preview model name.
    /// </summary>
    [Obsolete("Shut down on December 9, 2025. Use Gemini38Live.", false)]
    public const string GeminiLive25FlashPreview = "gemini-live-2.5-flash-preview";

    /// <summary>
    /// Gemini 2.5 Flash Image Preview model name.
    /// </summary>
    [Obsolete("Shut down on January 15, 2026. Use GeminiNanoBanana21 or Gemini31FlashImage.", false)]
    public const string Gemini25FlashImagePreview = "gemini-2.5-flash-image-preview";

    /// <summary>
    /// Gemini 2.5 Pro Preview model name from May 6th release.
    /// </summary>
    [Obsolete("Shut down on December 2, 2025. Use a Gemini 4 model.", false)]
    public const string Gemini25ProPreview0506 = "gemini-2.5-pro-preview-05-06";

    /// <summary>
    /// Gemini 2.5 Pro Preview model name from June 5th release.
    /// </summary>
    [Obsolete("Shut down on December 2, 2025. Use a Gemini 4 model.", false)]
    public const string Gemini25ProPreview0605 = "models/gemini-2.5-pro-preview-06-05";

    /// <summary>
    /// Gemini 2.0 Flash Exp Image Generation model name.
    /// </summary>
    [Obsolete("No longer listed in the Gemini API model documentation. Use GeminiNanoBanana21.", false)]
    public const string Gemini2FlashExpImageGeneration = "gemini-2.0-flash-exp-image-generation";

    /// <summary>
    /// Gemini 2.0 Flash Preview Image Generation model name.
    /// </summary>
    [Obsolete("Shut down on November 14, 2025. Use GeminiNanoBanana21.", false)]
    public const string Gemini2FlashPreviewImageGeneration = "gemini-2.0-flash-preview-image-generation";

    /// <summary>
    /// Gemini 2.5 Flash Preview model name from September 2025 release.
    /// </summary>
    [Obsolete("Shut down on February 17, 2026. Use Gemini38Flash.", false)]
    public const string Gemini25FlashPreview092025 = "models/gemini-2.5-flash-preview-09-2025";

    /// <summary>
    /// Gemini 2.5 Flash-Lite Preview model name from September 2025 release.
    /// </summary>
    [Obsolete("Shut down on March 31, 2026. Use Gemini35FlashLite.", false)]
    public const string Gemini25FlashLitePreview092025 = "models/gemini-2.5-flash-lite-preview-09-2025";

    /// <summary>
    /// Gemini 2.5 Computer Use Preview model name from October 2025 release.
    /// </summary>
    [Obsolete("Shut down on July 28, 2026. Computer Use is now a GA tool available on current Gemini models.", false)]
    public const string Gemini25ComputerUsePreview102025 = "models/gemini-2.5-computer-use-preview-10-2025";

    /// <summary>
    /// Gemini 2.5 Flash Native Audio Latest model name.
    /// </summary>
    [Obsolete("No longer listed in the Gemini API model documentation. Use Gemini38Live.", false)]
    public const string Gemini25FlashNativeAudioLatest = "models/gemini-2.5-flash-native-audio-latest";

    /// <summary>
    /// Gemini 2.5 Flash Native Audio Preview model name from September 2025 release.
    /// </summary>
    [Obsolete("No longer listed in the Gemini API model documentation. Use Gemini38Live or Gemini25FlashNativeAudioPreview122025.", false)]
    public const string Gemini25FlashNativeAudioPreview092025 = "models/gemini-2.5-flash-native-audio-preview-09-2025";

    /// <summary>
    /// Gemini 2.5 Flash Live Preview model name.
    /// </summary>
    [Obsolete("No longer listed in the Gemini API model documentation. Use Gemini38Live.", false)]
    public const string Gemini25FlashLivePreview = "models/gemini-2.5-flash-live-preview";

    /// <summary>
    /// Gemini Robotics-ER 1.5 Preview model name.
    /// </summary>
    [Obsolete("Shut down on April 30, 2026. Use GeminiRoboticsEr2Preview.", false)]
    public const string GeminiRoboticsEr15Preview = "models/gemini-robotics-er-1.5-preview";

    /// <summary>
    /// Gemini 2.0 Flash model name.
    /// </summary>
    [Obsolete("Shut down on June 1, 2026. Use Gemini38Flash.", false)]
    public const string Gemini2FlashLatest = "gemini-2.0-flash";

    /// <summary>
    /// Gemini 2.0 Flash Exp Model Name
    /// </summary>
    [Obsolete("No longer listed in the Gemini API model documentation. Use Gemini38Flash.", false)]
    public const string Gemini2FlashExp = "gemini-2.0-flash-exp";

    /// <summary>
    /// Gemini 2.0 Flash-Lite Preview model name.
    /// </summary>
    [Obsolete("Shut down on December 9, 2025. Use Gemini35FlashLite.", false)]
    public const string Gemini2FlashLitePreview = "models/gemini-2.0-flash-lite-preview-02-05";

    /// <summary>
    /// Gemini 2.0 Flash Exp model name.
    /// </summary>
    [Obsolete("Gemini-embedding-exp is deprecated. Use GeminiEmbedding2 or GeminiEmbedding (gemini-embedding-001) instead.", false)]
    public const string GeminiEmbeddingExp = "gemini-embedding-exp";

    /// <summary>
    /// Text Embedding 005 model name.
    /// </summary>
    public const string TextEmbedding005 = "text-embedding-005";

    /// <summary>
    /// Text Embedding model 004 name.
    /// </summary>
    [Obsolete("text-embedding-004 was shut down on January 14, 2026. Use GeminiEmbedding2 or GeminiEmbedding.", false)]
    public const string TextEmbedding = "models/text-embedding-004";

    /// <summary>
    /// Embedding model name.
    /// </summary>
    [Obsolete("The Embedding model is older. Use GeminiEmbedding2 or GeminiEmbedding instead.", false)]
    public const string Embedding = "models/embedding-001";

    /// <summary>
    /// AQA model name.
    /// </summary>
    [Obsolete("No longer listed in the Gemini API model documentation.", false)]
    public const string Aqa = "models/aqa";

    #endregion

    #region Naming patterns

    /// <summary>
    /// Represents the latest version name pattern.
    /// </summary>
    public const string LatestVersionPattern = "<model>-<generation>-<variation>-latest";

    /// <summary>
    /// Represents the latest stable version name pattern.
    /// </summary>
    public const string LatestStableVersionPattern = "<model>-<generation>-<variation>";

    /// <summary>
    /// Represents the stable version name pattern.
    /// </summary>
    public const string StableVersionPattern = "<model>-<generation>-<variation>-<version>";

    /// <summary>
    /// Represents the experimental version name pattern.
    /// </summary>
    public const string ExperimentalVersionPattern = "<model>-<generation>-<variation>-<version>";

    #endregion

    /// <summary>
    /// Provides constants for experimental Gemini model variations.
    /// </summary>
    /// <remarks>
    /// These models are for experimentation and testing, and are not intended for production use.
    /// See <seealso href="https://ai.google.dev/gemini-api/docs/models/experimental-models"/> for details.
    /// </remarks>
    public static class Experimental
    {
        /// <summary>
        /// LearnLM 2.0 Flash Experimental model name.
        /// </summary>
        [Obsolete("No longer listed in the Gemini API model documentation.", false)]
        public const string LearnLM2FlashExp = "models/learnlm-2.0-flash-experimental";
    }

    /// <summary>
    /// Provides constants for Imagen model variations.
    /// </summary>
    /// <remarks>
    /// All Imagen models on the Gemini API are shut down. Use the Gemini image models
    /// (for example <see cref="GeminiNanoBanana21"/>) instead.
    /// </remarks>
    [Obsolete("Imagen models are shut down on the Gemini API. Use a Gemini image model, such as GoogleAIModels.GeminiNanoBanana21.", false)]
    public static class Imagen
    {
        /// <summary>
        /// Imagen 3 model names.
        /// </summary>
        public const string Imagen3Generate001 = "imagen-3.0-generate-001";
        /// <summary>
        /// Imagen 3 fast generation model for quick image generation.
        /// </summary>
        public const string Imagen3FastGenerate001 = "imagen-3.0-fast-generate-001";

        /// <summary>
        /// Imagen 3 generation model version 002.
        /// </summary>
        [Obsolete("Shut down on the Gemini API on November 10, 2025. Use GoogleAIModels.GeminiNanoBanana21.", false)]
        public const string Imagen3Generate002 = "models/imagen-3.0-generate-002";

        /// <summary>
        /// Imagen 4 generation model version 001.
        /// </summary>
        [Obsolete("Shut down on the Gemini API on August 17, 2026. Use GoogleAIModels.GeminiNanoBanana21.", false)]
        public const string Imagen4Generate001 = "models/imagen-4.0-generate-001";

        /// <summary>
        /// Imagen 4 Ultra generation model version 001.
        /// </summary>
        [Obsolete("Shut down on the Gemini API on August 17, 2026. Use GoogleAIModels.Gemini3ProImage.", false)]
        public const string Imagen4UltraGenerate001 = "models/imagen-4.0-ultra-generate-001";

        /// <summary>
        /// Imagen 4 Fast generation model version 001.
        /// </summary>
        [Obsolete("Shut down on the Gemini API on August 17, 2026. Use GoogleAIModels.Gemini31FlashLiteImage.", false)]
        public const string Imagen4FastGenerate001 = "models/imagen-4.0-fast-generate-001";

        /// <summary>
        /// Imagen 4 generation preview model from June 6th release.
        /// </summary>
        [Obsolete("Shut down on the Gemini API on February 17, 2026. Use GoogleAIModels.GeminiNanoBanana21.", false)]
        public const string Imagen4GeneratePreview0606 = "models/imagen-4.0-generate-preview-06-06";

        /// <summary>
        /// Imagen 4 Ultra generation preview model from June 6th release.
        /// </summary>
        [Obsolete("Shut down on the Gemini API on February 17, 2026. Use GoogleAIModels.Gemini3ProImage.", false)]
        public const string Imagen4UltraGeneratePreview0606 = "models/imagen-4.0-ultra-generate-preview-06-06";

        /// <summary>
        /// Imagen 2 model names.
        /// </summary>
        public const string ImageGeneration006 = "imagegeneration@006";
        /// <summary>
        /// Imagen 2 image generation model version 005.
        /// </summary>
        public const string ImageGeneration005 = "imagegeneration@005";

        /// <summary>
        /// Imagen model name.
        /// </summary>
        public const string ImageGeneration002 = "imagegeneration@002";

        /// <summary>
        /// Imagen 3 model name for editing and customization.
        /// </summary>
        public const string Imagen3Capability001 = "imagen-3.0-capability-001";
    }

    /// <summary>
    /// Provides constants for video generation model variations.
    /// </summary>
    /// <remarks>
    /// On the Gemini API, Gemini Omni replaces Veo for video generation.
    /// </remarks>
    public static class Video
    {
        /// <summary>
        /// Gemini Omni 1.2 Flash video generation model (GA).
        /// </summary>
        public const string GeminiOmni12Flash = "models/gemini-omni-1.2-flash";

        /// <summary>
        /// Gemini Omni 1.1 Flash video generation model. Requests are routed to Gemini Omni 1.2 Flash from December 3, 2026.
        /// </summary>
        public const string GeminiOmni11Flash = "models/gemini-omni-1.1-flash";

        /// <summary>
        /// Veo 2 video generation model.
        /// </summary>
        [Obsolete("Shut down on the Gemini API on June 30, 2026. Use GeminiOmni12Flash.", false)]
        public const string Veo2Generate001 = "models/veo-2.0-generate-001";

        /// <summary>
        /// Veo 3 video generation preview model.
        /// </summary>
        [Obsolete("Shut down on the Gemini API on November 12, 2025. Use GeminiOmni12Flash.", false)]
        public const string Veo3GeneratePreview = "models/veo-3.0-generate-preview";

        /// <summary>
        /// Veo 3 Fast video generation preview model.
        /// </summary>
        [Obsolete("Shut down on the Gemini API on November 12, 2025. Use GeminiOmni12Flash.", false)]
        public const string Veo3FastGeneratePreview = "models/veo-3.0-fast-generate-preview";

        /// <summary>
        /// Veo 3 video generation model version 001.
        /// </summary>
        [Obsolete("Shut down on the Gemini API on June 30, 2026. Use GeminiOmni12Flash.", false)]
        public const string Veo3Generate001 = "models/veo-3.0-generate-001";

        /// <summary>
        /// Veo 3 Fast video generation model version 001.
        /// </summary>
        [Obsolete("Shut down on the Gemini API on June 30, 2026. Use GeminiOmni12Flash.", false)]
        public const string Veo3FastGenerate001 = "models/veo-3.0-fast-generate-001";

        /// <summary>
        /// Veo 3.1 video generation preview model.
        /// </summary>
        [Obsolete("Shut down on the Gemini API on October 22, 2026. Use GeminiOmni12Flash.", false)]
        public const string Veo31GeneratePreview = "models/veo-3.1-generate-preview";

        /// <summary>
        /// Veo 3.1 Fast video generation preview model.
        /// </summary>
        [Obsolete("Shut down on the Gemini API on October 22, 2026. Use GeminiOmni12Flash.", false)]
        public const string Veo31FastGeneratePreview = "models/veo-3.1-fast-generate-preview";
    }

    /// <summary>
    /// Provides constants for Lyria music generation models.
    /// </summary>
    public static class Music
    {
        /// <summary>
        /// Lyria 3.5 music generation model (stable).
        /// </summary>
        public const string Lyria35 = "models/lyria-3.5";

        /// <summary>
        /// Lyria 3 Pro Preview music generation model.
        /// </summary>
        public const string Lyria3ProPreview = "models/lyria-3-pro-preview";

        /// <summary>
        /// Lyria 3 Clip Preview model for short music clips.
        /// </summary>
        public const string Lyria3ClipPreview = "models/lyria-3-clip-preview";

        /// <summary>
        /// Lyria RealTime experimental model for streaming music generation (Live Music API).
        /// </summary>
        public const string LyriaRealtimeExp = "models/lyria-realtime-exp";
    }

    /// <summary>
    /// Provides constants for Gemini agent models.
    /// </summary>
    public static class Agents
    {
        /// <summary>
        /// Deep Research agent preview (April 2026).
        /// </summary>
        public const string DeepResearchPreview042026 = "deep-research-preview-04-2026";

        /// <summary>
        /// Deep Research Max agent preview (April 2026).
        /// </summary>
        public const string DeepResearchMaxPreview042026 = "deep-research-max-preview-04-2026";

        /// <summary>
        /// Antigravity agent preview (September 2026).
        /// </summary>
        public const string AntigravityPreview092026 = "antigravity-preview-09-2026";
    }
}
