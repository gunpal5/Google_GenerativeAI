﻿namespace GenerativeAI;


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
        /// Gemini 2.0 Flash model name.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public const string Gemini2Flash = "gemini-2.0-flash-001";

        /// <summary>
        /// Gemini 2.5 Flash model name.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public const string Gemini25Flash = "gemini-2.5-flash";

        /// <summary>
        /// Gemini 2.0 Pro Experimental model name.
        /// </summary>
        public const string Gemini2ProExp = "gemini-2.0-pro-exp-02-05";

        /// <summary>
        /// Gemini 2.5 Pro Exp 03-25 model name.
        /// </summary>
        public const string Gemini25ProExp0325 = "gemini-2.5-pro-exp-03-25";
        
        /// <summary>
        /// Gemini 2.5 Pro Preview model name from March 25th release.
        /// </summary>
        public const string Gemini25ProPreview0325 = "gemini-2.5-pro-preview-03-25";

        /// <summary>
        /// Gemini 2.0 Flash Live model name version 001.
        /// </summary>
        public const string Gemini2FlashLive001 = "gemini-2.0-flash-live-001";
    
        /// <summary>
        /// Gemini 2.5 Flash Preview model name from April 17th release.
        /// </summary>
        
        public const string Gemini25FlashPreview0417 = "gemini-2.5-flash-preview-04-17";
        
        /// <summary>
        /// Gemini 2.5 Flash Preview model name from May 20th release.
        /// </summary>
        public const string Gemini25FlashPreview0520 = "gemini-2.5-flash-preview-05-20";
        
        /// <summary>
        /// Gemini 2.5 Flash Native Audio model name.
        /// </summary>
        public const string Gemini25FlashNativeAudioDialog = "gemini-2.5-flash-preview-native-audio-dialog";
        
        /// <summary>
        /// Gemini 2.5 Flash Preview TTS model name.
        /// </summary>
        public const string Gemini25FlashPreviewTTS = "gemini-2.5-flash-preview-tts";
        
        /// <summary>
        /// Gemini 2.5 Pro Preview model name from May 6th release.
        /// </summary>
        public const string Gemini25ProPreview0506 = "gemini-2.5-pro-preview-05-06";
        
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
        /// Gemini 2.0 Flash Exp 01-21 model name.
        /// </summary>
        public const string Gemini2FlashThinkingExp0121 = "gemini-2.0-flash-thinking-exp-01-21";

        /// <summary>
        /// Gemini 2.0 Flash Exp Image Generation model name.
        /// </summary>
        public const string Gemini2FlashExpImageGeneration = "gemini-2.0-flash-exp-image-generation";

        /// <summary>
        /// Gemini 2.0 Flash Exp model name.
        /// </summary>
        public const string GeminiEmbeddingExp = "gemini-embedding-exp";
        /// <summary>
        /// Gemma 3.0 27B model name.
        /// </summary>
        public const string Gemmma3_27B = "gemma-3-27b-it";

        /// <summary>
        /// Gemini 2.0 Flash model name.
        /// </summary>
        public const string Gemini2FlashLatest = "gemini-2.0-flash";
        
        /// <summary>
        /// Gemini 2.0 Flash Exp model name.
        /// </summary>
        public const string Gemini2FlashExp = "gemini-2.0-flash-exp";

        /// <summary>
        /// Text Embedding 005 model name.
        /// </summary>
        public const string TextEmbedding005 = "text-embedding-005";
        
        /// <summary>
        /// Gemini 2.0 Flash-Lite Preview model name.
        /// </summary>
        public const string Gemini2FlashLitePreview = "gemini-2.0-flash-lite-preview-02-05";

        /// <summary>
        /// Gemini 2.0 Flash Thinking Experimental model name.
        /// </summary>
        public const string Gemini2FlashThinkingExp = "gemini-2.0-flash-thinking-exp-01-21";

        /// <summary>
        /// Gemini 1.5 Flash model name.
        /// </summary>
        public const string Gemini15Flash = "gemini-1.5-flash";

        /// <summary>
        /// Gemini 1.5 Pro model name.
        /// </summary>
        public const string Gemini15Pro = "gemini-1.5-pro";

        /// <summary>
        /// Gemini 1.0 Pro model name.
        /// </summary>
        public const string Gemini10Pro = "gemini-1.0-pro";

        /// <summary>
        /// Gemini 1.0 Pro Vision model name.
        /// </summary>
        public const string Gemini10ProVision = "gemini-1.0-pro-vision";
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
        /// Embeddings for text model names.
        /// </summary>
        public const string TextEmbeddingGecko001 = "textembedding-gecko@001";

        /// <summary>
        /// Text Embedding Gecko 2.0 model name.
        /// </summary>
        public const string TextEmbeddingGecko002 = "textembedding-gecko@002";

        /// <summary>
        /// Text Embedding Gecko model version 003.
        /// </summary>
        public const string TextEmbeddingGecko003 = "textembedding-gecko@003";

        /// <summary>
        /// Text Embedding model version 004.
        /// </summary>
        public const string TextEmbedding004 = "text-embedding-004";

        /// <summary>
        /// Embeddings for text multilingual model names.
        /// </summary>
        public const string TextEmbeddingGeckoMultilingual001 = "textembedding-gecko-multilingual@001";

        /// <summary>
        /// Text Multilingual Embedding model version 002.
        /// </summary>
        public const string TextMultilingualEmbedding002 = "text-multilingual-embedding-002";

        /// <summary>
        /// Embeddings for multimodal model name.
        /// </summary>
        public const string MultimodalEmbedding = "multimodalembedding";
    }

    /// <summary>
    /// Provides constants for video generation model names.
    /// </summary>
    public static class Video
    {
        /// <summary>
        /// Veo 2 video generation model.
        /// </summary>
        public const string Veo2Generate001 = "veo-2.0-generate-001";
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
    /// Provides constants for Codey model variations.
    /// </summary>
    public static class Codey
    {
        /// <summary>
        /// Codey for Code Completion model name.
        /// </summary>
        public const string CodeGecko = "code-gecko";
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