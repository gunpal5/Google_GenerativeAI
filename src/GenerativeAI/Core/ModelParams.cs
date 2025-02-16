using GenerativeAI.Types;

namespace GenerativeAI.Core;

/// <summary>
/// Base parameters for a number of methods.
/// </summary>
public class BaseParams
{
    public SafetySetting[]? SafetySettings { get; set; }
    public GenerationConfig? GenerationConfig { get; set; }
    public string? SystemInstruction { get; set; }
}

/// <summary>
/// Params passed to {@link GoogleGenerativeAI.getGenerativeModel}.
/// </summary>
public class ModelParams : BaseParams
{
    public string? Model { get; set; }
}