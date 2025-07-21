using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using CSharpToJsonSchema;
using GenerativeAI.Core;
using GenerativeAI.Tools.Extensions;
using GenerativeAI.Tools.Helpers;
using GenerativeAI.Types;
using Microsoft.Extensions.AI;
using Tool = GenerativeAI.Types.Tool;

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

    private JsonSerializerOptions _options;

    private Type _returnType;
    /// <summary>
    /// Represents a tool capable of wrapping a delegate function and exposing it as an interactive tool.
    /// </summary>
    /// <param name="func">The delegate function to be wrapped by the tool.</param>
    /// <param name="name">The name of the function for identification. Optional.</param>
    /// <param name="description">A description of the function for metadata purposes. Optional.</param>
    /// <param name="options">The JSON serializer options for serialization and deserialization. If not provided, 
    /// <see cref="DefaultSerializerOptions.GenerateObjectJsonOptions"/> will be used, including any custom type resolvers added to it. This can also be ignored if not using NativeAOT or Trimming</param>
    /// <remarks>
    /// QuickTool enables the use of delegate-based functions as tools by utilizing runtime reflection for schema generation and invocation.
    /// It is incompatible with Ahead-Of-Time (AOT) compilation scenarios and environments that require trimming support.
    /// </remarks>
    public QuickTool(
        Delegate func,
        string? name = null, string? description = null,
        JsonSerializerOptions? options = null)
    {
        _options = options ?? DefaultSerializerOptions.GenerateObjectJsonOptions;
        this._func = func;
        this.FunctionDeclaration = FunctionSchemaHelper.CreateFunctionDecleration(func, name, description);
        _returnType = func.Method.ReturnType;
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

        var result = await InvokeAsTaskAsync(_func, param).ConfigureAwait(false);
        var responseNode = new JsonObject();
        responseNode["name"] = functionCall.Name;

        if (result != null)
        {
            var info = _options.GetTypeInfo(result.GetType());
            var x = JsonSerializer.Serialize(result, info);
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
        };
    }

    private async Task<object?> InvokeAsTaskAsync(Delegate function, object?[]? parameters)
    {
        // Dynamically invoke
        var result = function.DynamicInvoke(parameters);

        // If it’s already a non-Task, just return it
        if (result is not (Task or ValueTask))
            return result;

        // If the result is Task, figure out the T in Task<T>, if any
        var resultType = _returnType;
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            // This is Task<T>. We can reflect on its "Result" property after awaiting
            var task = (Task)result;
            await task.ConfigureAwait(false);

            var val = GetMethodFromGenericMethodDefinition(resultType, _taskGetResult);
            var value = val.Invoke(task, null);
            return value;
        }
        else if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            // This is Task<T>. We can reflect on its "Result" property after awaiting
            var task = (ValueTask)result;
            await task.ConfigureAwait(false);

            var val = GetMethodFromGenericMethodDefinition(resultType, _valueTaskAsTask);
            var value = val.Invoke(task, null);
            return value;
        }
        else if (resultType == typeof(ValueTask))
        {
            var valueTask = (ValueTask)result;
            await valueTask.ConfigureAwait(false);
            return null;
        }
        else if (resultType == typeof(Task))
        {
            // If it's just Task (no generic), await and return null
            var task = (Task)result;
            await task.ConfigureAwait(false);
            return null;
        }
        else throw new NotImplementedException();
    }

    /// <summary>
    /// Marshals the parameters provided in the <see cref="JsonNode"/> format into an object array suitable for method invocation.
    /// </summary>
    /// <param name="functionCallArgs">The JSON node containing the arguments for the function call.</param>
    /// <param name="cancellationToken">The cancellation token to handle any asynchronous operation cancellations.</param>
    /// <returns>An object array of marshaled parameters, or null if the <paramref name="functionCallArgs"/> is null.</returns>
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
            var paramName = param.Name ?? $"param{objects.Count}";
            var val = functionCallArgs[paramName.ToCamelCase()];

            // If the value is not provided, add null
            if (val == null)
            {
                objects.Add(null);
            }
            else
            {
                // Deserialize the JSON value into the expected parameter type
                var typeInfo = _options.GetTypeInfo(param.ParameterType);
                var obj = val.Deserialize(typeInfo);
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

    /// <summary>
    /// Converts the current QuickTool instance into a MeaiFunction representation.
    /// </summary>
    /// <returns>
    /// A MeaiFunction instance populated with properties from the current QuickTool,
    /// including the defined function name, description, parameters, and invocation logic.
    /// </returns>
    public AITool AsMeaiTool()
    {
        var tool = new CSharpToJsonSchema.Tool()
        {
            Name = FunctionDeclaration.Name,
            Description = FunctionDeclaration.Description,
            Parameters = FunctionDeclaration.Parameters?.ToOpenApiSchema()
        };
        
        return new MeaiFunction(tool, MeaiFunctionInvoker, this._options);
    }

    private async Task<string> MeaiFunctionInvoker(string param, CancellationToken cancellationToken)
    {
        var node = JsonNode.Parse(param);
        var paramerters = MarshalParameters(node, cancellationToken);
        var result = await InvokeAsTaskAsync(_func, paramerters).ConfigureAwait(false);
        if (result != null)
        {
            var typeInfo = _options.GetTypeInfo(result.GetType());
            var x = JsonSerializer.Serialize(result, typeInfo);
            return x;
        }
        else
        {
            return string.Empty;
        }
    }

    #region MEAI Reference

    //This code is copied from https://github.com/dotnet/extensions/blob/main/src/Libraries/Microsoft.Extensions.AI/Functions/AIFunctionFactory.cs
    private static MethodInfo GetMethodFromGenericMethodDefinition(Type specializedType,
        MethodInfo genericMethodDefinition)
    {
#if NET
        return (MethodInfo)specializedType.GetMemberWithSameMetadataDefinitionAs(genericMethodDefinition);
#else
        #pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
        const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        #pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
        return specializedType.GetMethods(All).First(m => m.MetadataToken == genericMethodDefinition.MetadataToken);
#endif
    }

    private static readonly MethodInfo _taskGetResult =
        typeof(Task<>).GetProperty(nameof(Task<int>.Result), BindingFlags.Instance | BindingFlags.Public)!.GetMethod!;

    private static readonly MethodInfo _valueTaskAsTask =
        typeof(ValueTask<>).GetMethod(nameof(ValueTask<int>.AsTask), BindingFlags.Instance | BindingFlags.Public)!;

    #endregion
}