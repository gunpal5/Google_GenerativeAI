namespace GenerativeAI;


/// <summary>
/// Provides constants for Vertex AI model variations and related information.
/// </summary>
/// <remarks>
/// This class defines constants based on the Vertex AI models documented at:
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/learn/models"/>
/// </remarks>
// ReSharper disable InconsistentNaming
public static class VertexAIModels
{
    /// <summary>
    /// Provides constants for Gemini model variations.
    /// </summary>
    public static class Gemini
    {
        
        /// <summary>
        /// Gemini 2.5 Flash model name.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        [Obsolete("Google lists gemini-2.5-flash as retired on Vertex AI since October 20, 2026. Use Gemini38Flash or Gemini35FlashLite.", false)]
        public const string Gemini25Flash = "gemini-2.5-flash";

        /// <summary>
        /// Gemini 2.5 Pro model name (stable).
        /// </summary>
        [Obsolete("Google lists gemini-2.5-pro as retired on Vertex AI since October 20, 2026. Use Gemini38Flash or Gemini35Flash.", false)]
        public const string Gemini25Pro = "gemini-2.5-pro";

        /// <summary>
        /// Gemini 2.5 Flash-Lite model name (stable).
        /// </summary>
        [Obsolete("Google lists gemini-2.5-flash-lite as retired on Vertex AI since October 20, 2026. Use Gemini38Flash or Gemini35FlashLite.", false)]
        public const string Gemini25FlashLite = "gemini-2.5-flash-lite";

        /// <summary>
        /// Gemini 2.5 Flash Preview model name from April 17th release.
        /// </summary>
        
        /// <summary>
        /// Gemini 2.5 Flash Native Audio model name.
        /// </summary>
        public const string Gemini25FlashNativeAudioDialog = "gemini-2.5-flash-preview-native-audio-dialog";
        
        /// <summary>
        /// Gemini 2.5 Flash Preview TTS model name.
        /// </summary>
        public const string Gemini25FlashPreviewTTS = "gemini-2.5-flash-preview-tts";
        
        /// <summary>
        /// Gemini 2.5 Pro Preview TTS model name.
        /// </summary>
        public const string Gemini25ProPreviewTTS = "gemini-2.5-pro-preview-tts";

        /// <summary>
        /// Gemini 2.5 Flash Image Preview model name.
        /// Supports text and image inputs with text and image outputs.
        /// Currently in public preview with limited capabilities.
        /// </summary>
        public const string Gemini25FlashImagePreview = "gemini-2.5-flash-image-preview";

        /// <summary>
        /// Gemini 4 Hydrogen model name.
        /// </summary>
        public const string Gemini4Hydrogen = "gemini-4-hydrogen";

        /// <summary>
        /// Gemini 4 Carbon model name.
        /// </summary>
        public const string Gemini4Carbon = "gemini-4-carbon";

        /// <summary>
        /// Gemini 4 Argon model name.
        /// </summary>
        public const string Gemini4Argon = "gemini-4-argon";

        /// <summary>
        /// Gemini 4 Carbon for defensive cybersecurity use cases. Available behind an allowlist.
        /// </summary>
        public const string Gemini4CarbonCyberPermissive = "gemini-4-carbon-cyber-permissive";

        /// <summary>
        /// Gemini 4 Argon for defensive cybersecurity use cases. Available behind an allowlist.
        /// </summary>
        public const string Gemini4ArgonCyberPermissive = "gemini-4-argon-cyber-permissive";

        /// <summary>
        /// Gemini 3.8 Flash model name.
        /// </summary>
        public const string Gemini38Flash = "gemini-3.8-flash";

        /// <summary>
        /// Gemini 3.8 Flash Cyber model name, optimized for cybersecurity.
        /// </summary>
        public const string Gemini38FlashCyber = "gemini-3.8-flash-cyber";

        /// <summary>
        /// Gemini 3.7 Flash model name. Scheduled for retirement on January 28, 2027; the replacement is
        /// <see cref="Gemini38Flash"/>.
        /// </summary>
        public const string Gemini37Flash = "gemini-3.7-flash";

        /// <summary>
        /// Gemini 3.6 Flash model name.
        /// </summary>
        [Obsolete("gemini-3.6-flash retires on Vertex AI on November 19, 2026. Use Gemini38Flash.", false)]
        public const string Gemini36Flash = "gemini-3.6-flash";

        /// <summary>
        /// Gemini 3.5 Flash model name.
        /// </summary>
        public const string Gemini35Flash = "gemini-3.5-flash";

        /// <summary>
        /// Gemini 3.5 Flash-Lite model name.
        /// </summary>
        public const string Gemini35FlashLite = "gemini-3.5-flash-lite";

        /// <summary>
        /// Gemini 3.1 Flash-Lite model name.
        /// </summary>
        public const string Gemini31FlashLite = "gemini-3.1-flash-lite";

        /// <summary>
        /// Gemini 3.1 Pro Preview model name.
        /// </summary>
        public const string Gemini31ProPreview = "gemini-3.1-pro-preview";

        /// <summary>
        /// Gemini 3.8 Live model name.
        /// </summary>
        public const string Gemini38Live = "gemini-3.8-live";

        /// <summary>
        /// Gemini 3.8 Flash text-to-speech model name.
        /// </summary>
        public const string Gemini38FlashTts = "gemini-3.8-flash-tts";

        /// <summary>
        /// Gemini 3.8 Flash-Lite text-to-speech model name.
        /// </summary>
        public const string Gemini38FlashLiteTts = "gemini-3.8-flash-lite-tts";

        /// <summary>
        /// Gemini Nano Banana 2.1 image generation model name.
        /// </summary>
        public const string GeminiNanoBanana21 = "gemini-nano-banana-2.1";

        /// <summary>
        /// Gemini 3 Pro Image model name.
        /// </summary>
        public const string Gemini3ProImage = "gemini-3-pro-image";

        /// <summary>
        /// Gemini 3.1 Flash Image (Nano Banana 2) model name.
        /// </summary>
        public const string Gemini31FlashImage = "gemini-3.1-flash-image";

        /// <summary>
        /// Gemini 3.1 Flash-Lite Image (Nano Banana 2 Lite) model name, for high-volume, latency-sensitive workloads.
        /// </summary>
        public const string Gemini31FlashLiteImage = "gemini-3.1-flash-lite-image";

        /// <summary>
        /// Gemini 2.5 Flash Image model name. Scheduled for retirement on March 15, 2027; the replacement is
        /// <see cref="Gemini31FlashLiteImage"/>.
        /// </summary>
        public const string Gemini25FlashImage = "gemini-2.5-flash-image";

        /// <summary>
        /// Gemini 3 Flash Preview model name.
        /// Supports advanced thinking features with thought signatures for function calling.
        /// </summary>
        public const string Gemini3FlashPreview = "gemini-3-flash-preview";

        /// <summary>
        /// Gemini 3 Pro Preview model name.
        /// Supports advanced thinking features with thought signatures for function calling.
        /// </summary>
        public const string Gemini3ProPreview = "gemini-3-pro-preview";

        /// <summary>
        /// Gemini 3 Pro Image Preview model name.
        /// Supports image generation capabilities with advanced reasoning.
        /// </summary>
        public const string Gemini3ProImagePreview = "gemini-3-pro-image-preview";

        /// <summary>
        /// Gemma 3.0 1B model name.
        /// </summary>
        public const string Gemma3_1B = "gemma-3-1b-it";

        /// <summary>
        /// Gemma 3.0 4B model name.
        /// </summary>
        public const string Gemma3_4B = "gemma-3-4b-it";

        /// <summary>
        /// Gemma 3.0 12B model name.
        /// </summary>
        public const string Gemma3_12B = "gemma-3-12b-it";

        /// <summary>
        /// Gemma 3.0 27B model name.
        /// </summary>
        public const string Gemmma3_27B = "gemma-3-27b-it";

        /// <summary>
        /// Gemma 3n E4B model name (efficient version).
        /// </summary>
        public const string Gemma3n_E4B = "gemma-3n-e4b-it";

        /// <summary>
        /// Gemma 3n E2B model name (efficient version).
        /// </summary>
        public const string Gemma3n_E2B = "gemma-3n-e2b-it";

        /// <summary>
        /// Text Embedding 005 model name.
        /// </summary>
        public const string TextEmbedding005 = "text-embedding-005";
    }

    /// <summary>
    /// Provides constants for Gemma model variations.
    /// </summary>
    public static class Gemma
    {
        /// <summary>
        /// Gemma model name.
        /// </summary>
        public const string GemmaModel = "gemma";

        /// <summary>
        /// CodeGemma model name.
        /// </summary>
        public const string CodeGemma = "codegemma";

        /// <summary>
        /// PaliGemma model name.
        /// </summary>
        public const string PaliGemma = "paligemma";
    }

    /// <summary>
    /// Provides constants for Embeddings model variations.
    /// </summary>
    public static class Embeddings
    {
        /// <summary>
        /// Text Embedding model version 004.
        /// </summary>
        public const string TextEmbedding004 = "text-embedding-004";

        /// <summary>
        /// Text Multilingual Embedding model version 002.
        /// </summary>
        public const string TextMultilingualEmbedding002 = "text-multilingual-embedding-002";

        /// <summary>
        /// Embeddings for multimodal model name.
        /// </summary>
        public const string MultimodalEmbedding = "multimodalembedding";

        /// <summary>
        /// Gemini Embedding 001 model name.
        /// </summary>
        public const string GeminiEmbedding001 = "gemini-embedding-001";
    }

    /// <summary>
    /// Provides constants for video generation model names.
    /// </summary>
    public static class Video
    {
        /// <summary>
        /// Gemini Omni 1.2 Flash: video generation and conversational video editing (up to 15 seconds), video
        /// extension and upscaling up to 4K, alongside text responses.
        /// </summary>
        public const string GeminiOmni12Flash = "gemini-omni-1.2-flash";

        /// <summary>
        /// Veo 3.1 video generation model.
        /// </summary>
        public const string Veo31Generate001 = "veo-3.1-generate-001";

        /// <summary>
        /// Veo 3.1 Fast video generation model, optimized for lower latency.
        /// </summary>
        public const string Veo31FastGenerate001 = "veo-3.1-fast-generate-001";
    }

    /// <summary>
    /// Provides constants for MedLM model variations.
    /// </summary>
    public static class MedLM
    {
        /// <summary>
        /// MedLM-medium model name.
        /// </summary>
        public const string MedLMMedium = "medlm-medium";

        /// <summary>
        /// MedLM-large model name.
        /// </summary>
        public const string MedLMLarge = "medlm-large";
    }
}