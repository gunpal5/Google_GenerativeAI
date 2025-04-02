using System.Text.Json;
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
    /// <typeparam name="T">The type that defines the response schema for the JSON. Must be a class.</typeparam>
    /// <param name="request">The <see cref="GenerateContentRequest"/> on which JSON mode will be applied.</param>
    /// <param name="options">Optional <see cref="JsonSerializerOptions"/> for customizing the serialization process.</param>
    /// <remarks>
    /// For NativeAOT/Trimming JsonSerializerOptions are required, you can ignore it if you have already specified a resolver in <c ref="DefaultSerializerOptions.CustomJsonTypeResolvers"/>   
    /// </remarks>
    public static void UseJsonMode<T>(this GenerateContentRequest request, JsonSerializerOptions? options = null)
        where T : class
    {
        if (request.GenerationConfig == null)
            request.GenerationConfig = new GenerationConfig();
        request.GenerationConfig.ResponseMimeType = "application/json";
        request.GenerationConfig.ResponseSchema =
            GoogleSchemaHelper.ConvertToSchema<T>(options); 
    }
    
    /// <summary>
    /// Configures the <see cref="GenerateContentRequest"/> to use JSON mode for responses of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type that defines the response schema for the JSON. Must be a class.</typeparam>
    /// <param name="request">The <see cref="GenerateContentRequest"/> on which JSON mode will be applied.</param>
    /// <param name="options">Optional <see cref="JsonSerializerOptions"/> for customizing the serialization process.</param>
    /// <remarks>
    /// For NativeAOT/Trimming JsonSerializerOptions are required, you can ignore it if you have already specified a resolver in <c ref="DefaultSerializerOptions.CustomJsonTypeResolvers"/>   
    /// </remarks>
    public static void UseEnumMode<T>(this GenerateContentRequest request, JsonSerializerOptions? options = null)
        where T : Enum
    {
        if (request.GenerationConfig == null)
            request.GenerationConfig = new GenerationConfig();
        request.GenerationConfig.ResponseMimeType = "text/x.enum";
        request.GenerationConfig.ResponseSchema = Schema.FromEnum<T>(options);
    }
}