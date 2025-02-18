using GenerativeAI.Types;

namespace GenerativeAI.Core;

/// <summary>
/// Base parameters for a number of methods.
/// </summary>
public class BaseParams
{
    /// <summary>
    /// Specifies safety settings to control safety-blocking behavior for content generation.
    /// </summary>
    public SafetySetting[]? SafetySettings { get; set; }

    /// <summary>
    /// Configuration for content generation behavior and parameters.
    /// </summary>
    public GenerationConfig? GenerationConfig { get; set; }

    /// <summary>
    /// Instruction provided to the system for guiding the content generation process.
    /// </summary>
    public string? SystemInstruction { get; set; }
}

/// <summary>
/// Params passed to {@link GoogleGenerativeAI.getGenerativeModel}.
/// </summary>
public class ModelParams : BaseParams
{
    /// <summary>
    /// Specifies the name or identifier of the generative model to be used for content generation.
    /// </summary>
    public string? Model { get; set; }
}