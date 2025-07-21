using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using CSharpToJsonSchema;
using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.AI;
using Tool = GenerativeAI.Types.Tool;

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
    public QuickTools(QuickTool[] tools)
    {
        _tools = tools.ToList();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuickTools"/> class with an array of delegates.
    /// </summary>
    /// <param name="delegates">An array of delegates to be converted into <see cref="QuickTool"/> objects.</param>
    /// <param name="options">JSON Serializer context to appropriately parse the arguments, this can be ignored if JsonTypeResolver is set in global settings or not using NativeAOT</param>
    public QuickTools(Delegate[] delegates, JsonSerializerOptions? options = null)
    {
        _tools = delegates.Select(s => new QuickTool(s, options: options ?? DefaultSerializerOptions.GenerateObjectJsonOptions)).ToList();
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
        return await ft.CallAsync(functionCall, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override bool IsContainFunction(string name)
    {
        return _tools.Any(s => s.FunctionDeclaration.Name == name);
    }

    /// <summary>
    /// Converts the tools to Microsoft Extensions AI tool format.
    /// </summary>
    /// <returns>A read-only collection of AITool objects for Microsoft Extensions AI integration.</returns>
    #pragma warning disable CA1002
    public List<AITool> ToMeaiFunctions()
    {
        return this._tools.Select(s => s.AsMeaiTool()).ToList();
    }
    #pragma warning restore CA1002
}