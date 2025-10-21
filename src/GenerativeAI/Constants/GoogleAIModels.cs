namespace GenerativeAI;

/// <summary>
/// Provides constants for Gemini model variations and related information.
/// </summary>
/// <remarks>
/// This class defines constants based on the Gemini models documented at:
/// <seealso href="https://ai.google.dev/gemini-api/docs/models/gemini"/>
/// </remarks>
public static class GoogleAIModels
{
    /// <summary>
    /// Represents the default Gemini AI model identifier used by the Generative AI services.
    /// </summary>
    public const string DefaultGeminiModel = Gemini25Flash;

    ///<summary>
    /// Gemini 2.5 Flash
    /// </summary>
    public const string Gemini25Flash = "models/gemini-2.5-flash";

    ///<summary>
    /// Gemini 2.5 Flash Lite
    /// </summary>
    public const string Gemini25FlashLite = "models/gemini-2.5-flash-lite";

    /// <summary>
    /// Gemini 2.5 Flash-Lite Preview 06-17 model name.
    /// </summary>
    public const string Gemini25FlashLitePreview0617 = "models/gemini-2.5-flash-lite-preview-06-17";

    /// <summary>
    /// Gemini 2.0 Flash model name.
    /// </summary>
    public const string Gemini2Flash = "models/gemini-2.0-flash";

    /// <summary>
    /// Gemini 2.0 Flash Lite model name.
    /// </summary>
    public const string Gemini2FlashLite = "models/gemini-2.0-flash-lite";
    
    /// <summary>
    /// Gemini 2.5 Pro
    /// </summary>
    public const string Gemini25Pro = "models/gemini-2.5-pro";

    /// <summary>
    /// Gemini 2.5 Pro Exp 03-25 model name.
    /// </summary>
    public const string Gemini25ProExp0325 = "gemini-2.5-pro-exp-03-25";

    /// <summary>
    /// Gemini 2.0 Flash Exp 01-21 model name.
    /// </summary>
    public const string Gemini2FlashThinkingExp0121 = "gemini-2.0-flash-thinking-exp-01-21";
    
    /// <summary>
    /// Gemini 2.5 Pro Preview model name from March 25th release.
    /// </summary>
    public const string Gemini25ProPreview0325 = "gemini-2.5-pro-preview-03-25";
    
    /// <summary>
    /// Gemini 2.5 Flash Preview 05-20 model name from March 25th release.
    /// </summary>
    public const string Gemini25ProPreview0520 = "gemini-2.5-flash-preview-05-20";
    
   
    
    /// <summary>
    /// Gemini 2.0 Flash Live model name version 001.
    /// </summary>
    public const string Gemini2FlashLive001 = "gemini-2.0-flash-live-001";
    
    /// <summary>
    /// Gemini 2.5 Flash Preview model name from April 17th release.
    /// </summary>
    public const string Gemini25FlashPreview0417 = "gemini-2.5-flash-preview-04-17";
    
    /// <summary>
    /// Gemini 2.5 Flash Preview Native Audio Dialog model name.
    /// </summary>
    public const string Gemini25FlashPreviewNativeAudioDialog = "gemini-2.5-flash-preview-native-audio-dialog";
    
    /// <summary>
    /// Gemini 2.5 Flash Preview Native Audio Thinking Dialog model name.
    /// </summary>
    public const string Gemini25FlashPreviewNativeAudioThinkingDialog = "gemini-2.5-flash-exp-native-audio-thinking-dialog";
    
    /// <summary>
    /// Gemini Live 2.5 Flash Preview model name.
    /// </summary>
    public const string GeminiLive25FlashPreview = "gemini-live-2.5-flash-preview";
    
    /// <summary>
    /// Gemini 2.5 Flash Image Preview model name.
    /// </summary>
    public const string Gemini25FlashImagePreview = "gemini-2.5-flash-image-preview";

    /// <summary>
    /// Gemini 2.5 Flash Image model name (stable).
    /// </summary>
    public const string Gemini25FlashImage = "models/gemini-2.5-flash-image";
    
    /// <summary>
    /// Gemini 2.5 Flash Preview TTS model name.
    /// </summary>
    public const string Gemini25FlashPreviewTts = "gemini-2.5-flash-preview-tts";
    
    /// <summary>
    /// Gemini 2.5 Pro Preview TTS model name.
    /// </summary>
    public const string Gemini25ProPreviewTts = "gemini-2.5-pro-preview-tts";
    
    /// <summary>
    /// Gemini 2.5 Pro Preview model name from May 6th release.
    /// </summary>
    public const string Gemini25ProPreview0506 = "gemini-2.5-pro-preview-05-06";

    /// <summary>
    /// Gemini 2.5 Pro Preview model name from June 5th release.
    /// </summary>
    public const string Gemini25ProPreview0605 = "models/gemini-2.5-pro-preview-06-05";
    
    /// <summary>
    /// Gemini 2.0 Flash Exp Image Generation model name.
    /// </summary>
    public const string Gemini2FlashExpImageGeneration = "gemini-2.0-flash-exp-image-generation";
    
    /// <summary>
    /// Gemini 2.0 Flash Preview Image Generation model name.
    /// </summary>
    public const string Gemini2FlashPreviewImageGeneration = "gemini-2.0-flash-preview-image-generation";

    /// <summary>
    /// Gemini 2.5 Flash Preview model name from September 2025 release.
    /// </summary>
    public const string Gemini25FlashPreview092025 = "models/gemini-2.5-flash-preview-09-2025";

    /// <summary>
    /// Gemini 2.5 Flash-Lite Preview model name from September 2025 release.
    /// </summary>
    public const string Gemini25FlashLitePreview092025 = "models/gemini-2.5-flash-lite-preview-09-2025";

    /// <summary>
    /// Gemini 2.5 Computer Use Preview model name from October 2025 release.
    /// </summary>
    public const string Gemini25ComputerUsePreview102025 = "models/gemini-2.5-computer-use-preview-10-2025";

    /// <summary>
    /// Gemini 2.5 Flash Native Audio Latest model name.
    /// </summary>
    public const string Gemini25FlashNativeAudioLatest = "models/gemini-2.5-flash-native-audio-latest";

    /// <summary>
    /// Gemini 2.5 Flash Native Audio Preview model name from September 2025 release.
    /// </summary>
    public const string Gemini25FlashNativeAudioPreview092025 = "models/gemini-2.5-flash-native-audio-preview-09-2025";

    /// <summary>
    /// Gemini 2.5 Flash Live Preview model name.
    /// </summary>
    public const string Gemini25FlashLivePreview = "models/gemini-2.5-flash-live-preview";

    /// <summary>
    /// Gemini Robotics-ER 1.5 Preview model name.
    /// </summary>
    public const string GeminiRoboticsEr15Preview = "models/gemini-robotics-er-1.5-preview";
    
    /// <summary>
    /// The current Gemini Embedding model name.
    /// </summary>
    public const string GeminiEmbedding = "models/gemini-embedding-001";

    /// <summary>
    /// Gemini 2.0 Flash Exp model name.
    /// </summary>
    [Obsolete("Gemini-embedding-exp is deprecated. Use GeminiEmbedding (gemini-embedding-001) instead.", false)]
    public const string GeminiEmbeddingExp = "gemini-embedding-exp";
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

    /// <summary>
    /// Gemini 2.0 Flash model name.
    /// </summary>
    public const string Gemini2FlashLatest = "gemini-2.0-flash";


    /// <summary>
    /// Gemini 2.0 Flash Exp Model Name
    /// </summary>
    public const string Gemini2FlashExp = "gemini-2.0-flash-exp";
    /// <summary>
    /// Gemini 2.0 Flash-Lite Preview model name.
    /// </summary>
    public const string Gemini2FlashLitePreview = "models/gemini-2.0-flash-lite-preview-02-05";
    /// <summary>
    /// Text Embedding 005 model name.
    /// </summary>
    public const string TextEmbedding005 = "text-embedding-005";
    /// <summary>
    /// Gemini Flash Latest - Latest stable release of Gemini Flash (currently points to Gemini 2.5 Flash).
    /// </summary>
    public const string GeminiFlashLatest = "models/gemini-flash-latest";

    /// <summary>
    /// Gemini Flash-Lite Latest - Latest stable release of Gemini Flash-Lite (currently points to Gemini 2.5 Flash-Lite).
    /// </summary>
    public const string GeminiFlashLiteLatest = "models/gemini-flash-lite-latest";

    /// <summary>
    /// Gemini Pro Latest - Latest stable release of Gemini Pro (currently points to Gemini 2.5 Pro).
    /// </summary>
    public const string GeminiProLatest = "models/gemini-pro-latest";

    /// <summary>
    /// Text Embedding model 004 name.
    /// </summary>
    public const string TextEmbedding = "models/text-embedding-004";
    
   

    /// <summary>
    /// Embedding model name.
    /// </summary>
    [Obsolete("The Embedding model is older. Use TextEmbedding instead.", false)]
    public const string Embedding = "models/embedding-001";

    /// <summary>
    /// AQA model name.
    /// </summary>
    public const string Aqa = "models/aqa";

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
        public const string LearnLM2FlashExp = "models/learnlm-2.0-flash-experimental";
    }
    
    /// <summary>
    /// Provides constants for Imagen model variations.
    /// </summary>
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
        public const string Imagen3Generate002 = "models/imagen-3.0-generate-002";

        /// <summary>
        /// Imagen 4 generation model version 001.
        /// </summary>
        public const string Imagen4Generate001 = "models/imagen-4.0-generate-001";

        /// <summary>
        /// Imagen 4 Ultra generation model version 001.
        /// </summary>
        public const string Imagen4UltraGenerate001 = "models/imagen-4.0-ultra-generate-001";

        /// <summary>
        /// Imagen 4 Fast generation model version 001.
        /// </summary>
        public const string Imagen4FastGenerate001 = "models/imagen-4.0-fast-generate-001";

        /// <summary>
        /// Imagen 4 generation preview model from June 6th release.
        /// </summary>
        public const string Imagen4GeneratePreview0606 = "models/imagen-4.0-generate-preview-06-06";

        /// <summary>
        /// Imagen 4 Ultra generation preview model from June 6th release.
        /// </summary>
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
    /// Provides constants for Veo video generation model variations.
    /// </summary>
    public static class Video
    {
        /// <summary>
        /// Veo 2 video generation model.
        /// </summary>
        public const string Veo2Generate001 = "models/veo-2.0-generate-001";

        /// <summary>
        /// Veo 3 video generation preview model.
        /// </summary>
        public const string Veo3GeneratePreview = "models/veo-3.0-generate-preview";

        /// <summary>
        /// Veo 3 Fast video generation preview model.
        /// </summary>
        public const string Veo3FastGeneratePreview = "models/veo-3.0-fast-generate-preview";

        /// <summary>
        /// Veo 3 video generation model version 001.
        /// </summary>
        public const string Veo3Generate001 = "models/veo-3.0-generate-001";

        /// <summary>
        /// Veo 3 Fast video generation model version 001.
        /// </summary>
        public const string Veo3FastGenerate001 = "models/veo-3.0-fast-generate-001";

        /// <summary>
        /// Veo 3.1 video generation preview model.
        /// </summary>
        public const string Veo31GeneratePreview = "models/veo-3.1-generate-preview";

        /// <summary>
        /// Veo 3.1 Fast video generation preview model.
        /// </summary>
        public const string Veo31FastGeneratePreview = "models/veo-3.1-fast-generate-preview";
    }
}