using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using GenerativeAI.Core;
using GenerativeAI.Tools.Helpers;
using GenerativeAI.Types;

namespace GenerativeAI.Tools;

/// <summary>
/// Quick Function Tool, 
/// </summary>
/// <remarks>
/// This class usages reflection and is not compatible with AOT
/// </remarks>
public class QuickTool : GoogleFunctionTool
{
    /// <summary>
    /// Represents the declaration of a function used within the tool.
    /// </summary>
    /// <remarks>
    /// This property contains metadata about a function, such as its name,
    /// description, and parameter schema. It's primarily used for defining
    /// and managing functions available to tools in the framework.
    /// </remarks>
    public FunctionDeclaration FunctionDeclaration { get; private set; }
    private Delegate _func;

#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("QuickTool usages reflection to generate function schema and function invokation. Use GenericFunctionTool for NativeAOT and Trimming support.")]
#endif
    /// <summary>
    /// Represents a tool capable of leveraging a delegate function as its core functionality.
    /// </summary>
    /// <remarks>
    /// QuickTool is designed to work with a provided delegate, exposing its functionality as a tool.
    /// Note that this class utilizes runtime reflection and is not compatible with Ahead-Of-Time (AOT) compilation.
    /// </remarks>
    public QuickTool(Delegate func, string? name = null, string? description = null)
    {
        this._func = func;
        this.FunctionDeclaration = FunctionSchemaHelper.CreateFunctionDecleration(func, name, description);
    }

    /// <inheritdoc/>
    public override Tool AsTool()
    {
        return new Tool()
        {
            FunctionDeclarations = new List<FunctionDeclaration>() { FunctionDeclaration }
        };
    }

    /// <inheritdoc/>
    public override async Task<FunctionResponse?> CallAsync(FunctionCall functionCall,
        CancellationToken cancellationToken = default)
    {
        if (FunctionDeclaration.Name != functionCall.Name)
            throw new ArgumentException("Function name does not match");
        object?[]? param = MarshalParameters(functionCall.Args, cancellationToken);

        var result = await InvokeAsTaskAsync(_func, param);
        var responseNode = new JsonObject();
        responseNode["name"] = functionCall.Name;
        
        if (result != null)
        {
            var x = JsonSerializer.Serialize(result, DefaultSerializerOptions.GenerateObjectJsonOptions);
            var node = JsonNode.Parse(x);
            responseNode["content"] = node;
        }
        else
        {
            responseNode["content"] = string.Empty;
        }
        return new FunctionResponse()
        {
            Id = functionCall.Id,
            Name = functionCall.Name,
            Response = responseNode
        };;
    }

#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("The constructor will perform reflection on the delegate type provided")]
#endif
    private async Task<object?> InvokeAsTaskAsync(Delegate function, object?[]? parameters)
    {
        // Dynamically invoke
        var result = function.DynamicInvoke(parameters);

        // If it’s already a non-Task, just return it
        if (result is not Task)
            return result;

        // If the result is Task, figure out the T in Task<T>, if any
        var resultType = result.GetType();
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            // This is Task<T>. We can reflect on its "Result" property after awaiting
            var task = (Task)result;
            await task.ConfigureAwait(false);

            // Retrieve the .Result property via reflection
            var resultProperty = resultType.GetProperty("Result");
            return resultProperty?.GetValue(task);
        }
        else
        {
            // If it's just Task (no generic), await and return null
            var task = (Task)result;
            await task.ConfigureAwait(false);
            return null;
        }
    }


    /// <summary>
    /// Marshals the parameters provided in the <see cref="JsonNode"/> format into an object array suitable for method invocation.
    /// </summary>
    /// <param name="functionCallArgs">The JSON node containing the arguments for the function call.</param>
    /// <param name="cancellationToken">The cancellation token to handle any asynchronous operation cancellations.</param>
    /// <returns>An object array of marshaled parameters, or null if the <paramref name="functionCallArgs"/> is null.</returns>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("The constructor will perform reflection on the delegate type provided")]
#endif
    private object?[]? MarshalParameters(JsonNode? functionCallArgs, CancellationToken cancellationToken)
    {
        if (functionCallArgs == null)
            return null;

        List<object?> objects = new List<object?>();

        // Iterate over the parameters of the delegate function
        foreach (var param in _func.Method.GetParameters())
        {
            // If the parameter is a CancellationToken, directly add it
            if (param.ParameterType.Name == "CancellationToken")
            {
                objects.Add(cancellationToken);
                continue;
            }

            // Retrieve the parameter value from the JSON node using its name in camelCase
            var val = functionCallArgs[param.Name.ToCamelCase()];

            // If the value is not provided, add null
            if (val == null)
            {
                objects.Add(null);
            }
            else
            {
                // Deserialize the JSON value into the expected parameter type
                var obj = val.Deserialize(param.ParameterType, DefaultSerializerOptions.GenerateObjectJsonOptions);
                objects.Add(obj);
            }
        }

        return objects.ToArray();
    }

    /// <summary>
    /// Determines whether the specified function name matches the name of the function declared in the tool's FunctionDeclaration property.
    /// </summary>
    /// <param name="name">The name of the function to check for existence within the tool's declaration.</param>
    /// <returns>A boolean value indicating whether the specified function name is contained within the FunctionDeclaration.</returns>
    public override bool IsContainFunction(string name)
    {
        return FunctionDeclaration.Name == name;
    }
}