using GenerativeAI.Core;
using GenerativeAI.Types;

namespace GenerativeAI;

/// <summary>
/// Provides extension methods for enhancing the functionality of the <see cref="GenerateContentRequest"/> class.
/// </summary>
public static class GenerateContentRequestExtensions
{

    /// <summary>
    /// Adds a <see cref="Tool"/> to the <see cref="GenerateContentRequest"/> with an optional <see cref="ToolConfig"/>.
    /// </summary>
    /// <param name="request">The <see cref="GenerateContentRequest"/> instance to which the <see cref="Tool"/> will be added.</param>
    /// <param name="tool">The <see cref="Tool"/> to add.</param>
    /// <param name="config">Optional <see cref="ToolConfig"/> for configuring the <see cref="Tool"/>.</param>
    public static void AddTool(
        this GenerateContentRequest request,
        Tool tool, ToolConfig? config = null)
    {
        request.Tools ??= new List<Tool>();
        request.Tools.Add(tool);
        request.ToolConfig = config;
    }

    /// <summary>
    /// Configures the <see cref="GenerateContentRequest"/> to use JSON mode for responses of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type that defines the response schema for the JSON.</typeparam>
    /// <param name="request">The <see cref="GenerateContentRequest"/> on which JSON mode will be applied.</param>
    /// <remarks>Some of the complex data types are not supported such as Dictionary. So make sure to avoid these.</remarks>
    public static void UseJsonMode<T>(this GenerateContentRequest request) where T : class
    {
        if(request.GenerationConfig == null)
            request.GenerationConfig = new GenerationConfig();
        request.GenerationConfig.ResponseMimeType = "application/json";
        request.GenerationConfig.ResponseSchema = typeof(T);
    }
}