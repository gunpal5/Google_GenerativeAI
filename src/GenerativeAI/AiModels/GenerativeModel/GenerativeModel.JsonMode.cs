using System.Text.Json;
using GenerativeAI.Types;

namespace GenerativeAI;

public partial class GenerativeModel
{
    /// <summary>
    /// Determines whether JSON mode is enabled. JSON mode adjusts the content generation response
    /// to specifically produce outputs in JSON format as defined in generation configurations.
    /// </summary>
    /// <remarks>
    /// JSON mode is incompatible with grounding, Google Search, and code execution tools.
    /// Enabling this mode will override other response formats with "application/json".
    /// </remarks>
    public bool UseJsonMode { get; set; } = false;

    private JsonSerializerOptions _jsonSerializerOptions = DefaultSerializerOptions.GenerateObjectJsonOptions;

    /// <summary>
    /// Specifies the JSON serializer options to be used when generating objects as JSON outputs.
    /// These options configure the behavior of JSON serialization and deserialization
    /// for object generation in the context of JSON mode.
    /// </summary>
    /// <remarks>
    /// For NativeAOT/Trimming GenerateObjectJsonSerializerOptions are required, you can ignore it if you have already specified a resolver in <c ref="DefaultSerializerOptions.CustomJsonTypeResolvers"/>
    /// </remarks>
    public JsonSerializerOptions GenerateObjectJsonSerializerOptions
    {
        get => new JsonSerializerOptions(this._jsonSerializerOptions);
        set
        {
            this._jsonSerializerOptions = value;
        }
    }

    #region Generate Object
    
    /// <summary>
    /// Generates content asynchronously using JSON mode based on the given input parameters and converts the resulting JSON into a C# object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of C# object to send as JSON schema input in request.</typeparam>
    /// <param name="request">An instance of <see cref="GenerateContentRequest"/> containing the input configuration and settings for content generation.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the generated content response of type <typeparamref name="T"/>.</returns>
    /// <remarks>Some of the complex data types are not supported such as Dictionary. So make sure to avoid these.</remarks>
    public virtual async Task<GenerateContentResponse> GenerateContentAsync<T>(
        GenerateContentRequest request,
        CancellationToken cancellationToken = default) where T : class
    {
        request.GenerationConfig ??= this.Config;
        request.UseJsonMode<T>(GenerateObjectJsonSerializerOptions);
    
        return await GenerateContentAsync(request, cancellationToken).ConfigureAwait(false);
    }
    
   
    
    /// <summary>
    /// Generates content asynchronously using JSON mode based on the given input parameters and converts the resulting JSON into a C# object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of C# object to convert the resulting JSON into.</typeparam>
    /// <param name="request">An instance of <see cref="GenerateContentRequest"/> containing the input configuration and settings for content generation.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the generated object of type <typeparamref name="T"/>.</returns>
    /// <remarks>Some of the complex data types are not supported such as Dictionary. So make sure to avoid these.</remarks>
    public virtual async Task<T?> GenerateObjectAsync<T>(
        GenerateContentRequest request,
        CancellationToken cancellationToken = default) where T : class
    {
        var response = await GenerateContentAsync<T>(request, cancellationToken).ConfigureAwait(false);
        return response.ToObject<T>(GenerateObjectJsonSerializerOptions);
    }

    /// <summary>
    /// Generates content asynchronously using JSON mode based on the specified text input prompt and converts the resulting JSON into a C# object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of C# object to convert the resulting JSON into.</typeparam>
    /// <param name="prompt">The text input used to generate the content.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the generated object of type <typeparamref name="T"/>.</returns>
    /// <remarks>Some of the complex data types are not supported such as Dictionary. So make sure to avoid these.</remarks>
    public async Task<T?> GenerateObjectAsync<T>(
        string prompt,
        CancellationToken cancellationToken = default) where T : class
    {
        var request = new GenerateContentRequest();
        request.AddText(prompt, false);
    
        return await GenerateObjectAsync<T>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Generates content asynchronously using JSON mode based on the specified content generation request parts and converts the resulting JSON into a C# object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of C# object to convert the resulting JSON into.</typeparam>
    /// <param name="parts">An enumerable containing the input parts used for generating the content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the generated object of type <typeparamref name="T"/>.</returns>
    /// <remarks>Some of the complex data types are not supported such as Dictionary. So make sure to avoid these.</remarks>
    public async Task<T?> GenerateObjectAsync<T>(
        IEnumerable<Part> parts,
        CancellationToken cancellationToken = default) where T : class
    {
        var request = new GenerateContentRequest
        {
            Contents = new[] { RequestExtensions.FormatGenerateContentInput(parts) }.ToList()
        };

        return await GenerateObjectAsync<T>(request, cancellationToken).ConfigureAwait(false);
    }
    
  
    #endregion
    
    #region Generate Enum

    /// <summary>
    /// Generates an enumerated value asynchronously using Enum mode based on the given input parameters.
    /// </summary>
    /// <typeparam name="T">The enumeration type to convert the resulting JSON into.</typeparam>
    /// <param name="request">An instance of <see cref="GenerateContentRequest"/> containing the input configuration and settings for enum generation.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the generated enumeration value of type <typeparamref name="T"/>.</returns>
    public virtual async Task<T?> GenerateEnumAsync<T>(
        GenerateContentRequest request,
        CancellationToken cancellationToken = default) where T : Enum
    {
        request.UseEnumMode<T>();

        var response = await GenerateContentAsync(request, cancellationToken).ConfigureAwait(false);
        return response.ToEnum<T>(GenerateObjectJsonSerializerOptions);
    }

    /// <summary>
    /// Generates an enumeration asynchronously using Enum mode based on the provided prompt and converts the resulting Enum data into a C# enumeration of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of C# enumeration to convert the resulting JSON into. Must be an <see cref="Enum"/>.</typeparam>
    /// <param name="prompt">The textual input used to generate an enumeration of type <typeparamref name="T"/>.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests during the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the generated enumeration of type <typeparamref name="T"/>.</returns>
    /// <remarks>Some of the complex data types are not supported, such as Dictionary. Ensure that all input and output types conform to the supported JSON conversion structure.</remarks>
    public async Task<T?> GenerateEnumAsync<T>(
        string prompt,
        CancellationToken cancellationToken = default) where T : Enum
    {
        var request = new GenerateContentRequest();
        request.AddText(prompt, false);

        return await GenerateEnumAsync<T>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously generates an enumeration of the specified type <typeparamref name="T"/> based on the provided parts input.
    /// </summary>
    /// <typeparam name="T">The enumeration type to convert the resulting data into. Must be an enumeration.</typeparam>
    /// <param name="parts">A collection of <see cref="Part"/> objects representing the input components for generating the enumeration.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the generated enumeration of type <typeparamref name="T"/>.</returns>
    /// <remarks>Ensure the type <typeparamref name="T"/> is an enumeration; non-enumeration types are not supported.</remarks>
    public async Task<T?> GenerateEnumAsync<T>(
        IEnumerable<Part> parts,
        CancellationToken cancellationToken = default) where T : Enum
    {
        var request = new GenerateContentRequest
        {
            Contents = new[] { RequestExtensions.FormatGenerateContentInput(parts) }.ToList()
        };

        return await GenerateEnumAsync<T>(request, cancellationToken).ConfigureAwait(false);
    }

    #endregion
}