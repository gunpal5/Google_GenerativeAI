using GenerativeAI.Types;

namespace GenerativeAI.Core;

/// <summary>
/// Interface for defining <see cref="Tool">tools</see> that support function calls within the model's execution context.
/// A tool is a component that can perform external actions or provide additional functionality
/// beyond the intrinsic scope of the model.
/// </summary>
/// <remarks>
/// Use an implementation from <see href="https://www.nuget.org/packages/Google_GenerativeAI.Tools">Google_GenerativeAI.Tools</see>
/// </remarks>
public interface IFunctionTool
{
    /// <summary>
    /// Converts the current instance to a <see cref="Tool"/> object.
    /// </summary>
    /// <returns>A <see cref="Tool"/> representation of the current instance.</returns>
    Tool AsTool();

    /// <summary>
    /// Executes the specified <see cref="FunctionCall"/> asynchronously and retrieves the resulting
    /// <see cref="FunctionResponse"/> containing any output from the function execution.
    /// </summary>
    /// <param name="functionCall">The <see cref="FunctionCall"/> instance containing the name and arguments required for the function execution.</param>
    /// <returns>A task representing the asynchronous operation, which, upon completion, provides a <see cref="FunctionResponse"/> with the execution results.</returns>
    Task<FunctionResponse?> CallAsync(FunctionCall functionCall, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the specified function exists within the current tool's context.
    /// </summary>
    /// <param name="name">The name of the function to check for existence.</param>
    /// <returns>A boolean value indicating whether the function is present.</returns>
    bool IsContainFunction(string name);
}