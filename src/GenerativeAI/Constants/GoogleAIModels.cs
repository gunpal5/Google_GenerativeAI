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
    public const string DefaultGeminiModel = Gemini15Flash;
    
    /// <summary>
    /// Gemini 2.0 Flash model name.
    /// </summary>
    public const string Gemini2Flash = "models/gemini-2.0-flash";

    /// <summary>
    /// Gemini 2.0 Flash model name.
    /// </summary>
    public const string Gemini2FlashLatest = "gemini-2.0-flash";


    /// <summary>
    /// Gemini 2.0 Flash-Lite Preview model name.
    /// </summary>
    public const string Gemini2FlashLitePreview = "models/gemini-2.0-flash-lite-preview-02-05";

    /// <summary>
    /// Gemini 1.5 Flash model name.
    /// </summary>
    public const string Gemini15Flash = "models/gemini-1.5-flash";

    /// <summary>
    /// Gemini 1.5 Flash model name - latest stable version.
    /// </summary>
    public const string Gemini15FlashLatest = "gemini-1.5-flash-latest";

    /// <summary>
    /// Gemini 1.5 Flash-8B model name.
    /// </summary>
    public const string Gemini15Flash8B = "models/gemini-1.5-flash-8b";

    /// <summary>
    /// Gemini 1.5 Flash-8B model name - latest stable version.
    /// </summary>
    public const string Gemini15Flash8BLatest = "gemini-1.5-flash-8b-latest";

    /// <summary>
    /// Gemini 1.5 Pro model name.
    /// </summary>
    public const string Gemini15Pro = "models/gemini-1.5-pro";

    /// <summary>
    /// Gemini 1.5 Pro model name - latest stable version.
    /// </summary>
    public const string Gemini15ProLatest = "gemini-1.5-pro-latest";


    /// <summary>
    /// Gemini 1.0 Pro model name (Deprecated).
    /// </summary>
    [Obsolete("Gemini 1.0 Pro is deprecated. Use Gemini 1.5 Pro or Gemini 1.5 Flash instead.", false)]
    public const string Gemini10Pro = "models/gemini-1.0-pro";

    /// <summary>
    /// Gemini 1.0 Pro model name (Deprecated) - latest stable version.
    /// </summary>
    [Obsolete("Gemini 1.0 Pro is deprecated. Use Gemini 1.5 Pro or Gemini 1.5 Flash instead.", false)]
    public const string Gemini10ProLatest = "gemini-1.0-pro-latest";

    /// <summary>
    /// Text Embedding model name.
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
        /// Gemini 2.0 Pro Experimental model name.
        /// </summary>
        public const string Gemini2ProExp = "gemini-2.0-pro-exp-02-05";

        /// <summary>
        /// Gemini 2.0 Flash Thinking Experimental model name.
        /// </summary>
        public const string Gemini2FlashThinkingExp = "gemini-2.0-flash-thinking-exp-01-21";

        /// <summary>
        /// Gemini 2.0 Flash Experimental model name.
        /// </summary>
        public const string Gemini2FlashExp = "gemini-2.0-flash-exp";

        /// <summary>
        /// Gemini Experimental model name.
        /// </summary>
        public const string GeminiExp = "gemini-exp-1206";

        /// <summary>
        /// LearnLM 1.5 Pro Experimental model name.
        /// </summary>
        public const string LearnLM15ProExp = "learnlm-1.5-pro-experimental";
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
        public const string Imagen3FastGenerate001 = "imagen-3.0-fast-generate-001";

        public const string Imagen3Generate002 = "imagen-3.0-generate-002";
        /// <summary>
        /// Imagen 2 model names.
        /// </summary>
        public const string ImageGeneration006 = "imagegeneration@006";
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
}