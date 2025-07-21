using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GenerativeAI.Types;
using GenerativeAI.Utility;

namespace GenerativeAI.Tools.Helpers;

/// <summary>
/// Helper class for creating function schemas and declarations.
/// </summary>
public static class FunctionSchemaHelper
{
    /// <summary>
    /// Creates a function declaration from a delegate.
    /// </summary>
    /// <param name="func">The delegate to create a declaration from.</param>
    /// <param name="name">Optional custom name for the function.</param>
    /// <param name="description">Optional custom description for the function.</param>
    /// <returns>A FunctionDeclaration representing the delegate.</returns>
    public static FunctionDeclaration CreateFunctionDecleration(Delegate func, string? name, string? description)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(func);
#else
        if (func == null) throw new ArgumentNullException(nameof(func));
#endif
        var parameters = func.Method.GetParameters();
        Schema parametersSchema = new Schema();
        var options = DefaultSerializerOptions.GenerateObjectJsonOptions;
        
        parametersSchema.Properties = new Dictionary<string, Schema>();
        parametersSchema.Required = new List<string>();
        parametersSchema.Type = "object";
        var paramCount = 0;
        foreach (var param in parameters)
        {
            var type = param.ParameterType;
            if(type.Name == "CancellationToken")
                continue;
            paramCount++;
            //var descriptionsDics = TypeDescriptionExtractor.GetDescriptionDic(type);
            var desc = TypeDescriptionExtractor.GetDescription(param);
            //descriptionsDics[param.Name.ToCamelCase()] = desc;

            var schema = GoogleSchemaHelper.ConvertToSchema(type, options);
            schema.Description = desc;
            var paramName = param.Name ?? "param" + paramCount;
            parametersSchema.Properties.Add(paramName.ToCamelCase(), schema);
            parametersSchema.Required.Add(paramName.ToCamelCase());
        }

        var functionDescription = TypeDescriptionExtractor.GetDescription(func.Method);

        FunctionDeclaration functionObject = new FunctionDeclaration();
        functionObject.Description = description ?? functionDescription;
        functionObject.Parameters = paramCount>0? parametersSchema:null;
        functionObject.Name = name ?? func.Method.Name;
       
        return functionObject;
    }
}