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
        request.UseJsonMode<T>();
    
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
        return response.ToObject<T>();
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
}