using System.Text.Json.Serialization;

namespace GenerativeAI.Types;


/// <summary>
/// Defines the execution behavior for function calling by defining the execution mode.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#Mode_1">See Official API Documentation</seealso>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FunctionCallingMode // Renamed to FunctionCallingMode
{
    /// <summary>
    /// Unspecified function calling mode. This value should not be used.
    /// </summary>
    FUNCTION_CALLING_MODE_UNSPECIFIED = 0, // Renamed to match the class name

    /// <summary>
    /// Default model behavior, model decides to predict either a function call or a natural language response.
    /// </summary>
    AUTO = 1,

    /// <summary>
    /// Model is constrained to always predicting a function call only. If "allowedFunctionNames" are set,
    /// the predicted function call will be limited to any one of "allowedFunctionNames", else the predicted
    /// function call will be any one of the provided "functionDeclarations".
    /// </summary>
    ANY = 2,

    /// <summary>
    /// Model will not predict any function call. Model behavior is same as when not passing any function declarations.
    /// </summary>
    NONE = 3,
}