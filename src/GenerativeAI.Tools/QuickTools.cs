using System.Diagnostics.CodeAnalysis;
using GenerativeAI.Core;
using GenerativeAI.Types;

namespace GenerativeAI.Tools;

/// <summary>
/// Represents a collection of quick tools that can be used as Google function tools.
/// </summary>
public class QuickTools : GoogleFunctionTool
{
    private readonly List<QuickTool> _tools;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuickTools"/> class with an array of <see cref="QuickTool"/> objects.
    /// </summary>
    /// <param name="tools">An array of <see cref="QuickTool"/> objects to initialize the tool collection.</param>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("QuickTool usages reflection to generate function schema and function invokation. Use GenericFunctionTool for NativeAOT and Trimming support.")]
#endif
    public QuickTools(QuickTool[] tools)
    {
        _tools = tools.ToList();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuickTools"/> class with an array of delegates.
    /// </summary>
    /// <param name="delegates">An array of delegates to be converted into <see cref="QuickTool"/> objects.</param>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("QuickTool usages reflection to generate function schema and function invokation. Use GenericFunctionTool for NativeAOT and Trimming support.")]
#endif
    public QuickTools(Delegate[] delegates)
    {
        _tools = delegates.Select(s => new QuickTool(s)).ToList();
    }

    /// <inheritdoc />
    public override Tool AsTool()
    {
        return new Tool()
        {
            FunctionDeclarations = this._tools.Select(s => s.FunctionDeclaration).ToList(),
        };
    }

    /// <inheritdoc />
    public override async Task<FunctionResponse?> CallAsync(FunctionCall functionCall,
        CancellationToken cancellationToken = default)
    {
        var ft = _tools.FirstOrDefault(s => s.FunctionDeclaration.Name == functionCall.Name);
        if (ft == null)
            throw new ArgumentException("Function name does not match");
        return await ft.CallAsync(functionCall, cancellationToken);
    }

    /// <inheritdoc />
    public override bool IsContainFunction(string name)
    {
        return _tools.Any(s => s.FunctionDeclaration.Name == name);
    }
}