using GenerativeAI.Types;

namespace GenerativeAI.Core;


/// <inheritdoc cref="IFunctionTool"/>
public abstract class GoogleFunctionTool : IFunctionTool
{
    /// <inheritdoc/>
    public abstract Tool AsTool();
    
    /// <inheritdoc/>
    public abstract Task<FunctionResponse?> CallAsync(FunctionCall functionCall,
        CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract bool IsContainFunction(string name);
}