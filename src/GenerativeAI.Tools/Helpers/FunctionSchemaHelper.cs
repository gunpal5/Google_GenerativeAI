using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GenerativeAI.Types;
using GenerativeAI.Utility;

namespace GenerativeAI.Tools.Helpers;

public static class FunctionSchemaHelper
{
    #if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Create Schema will perform reflection on the delegate type provided to generate Schema")]
    #endif
    public static FunctionDeclaration CreateFunctionDecleration(Delegate func, string? name, string? description)
    {
        var parameters = func.Method.GetParameters();
        Schema parametersSchema = new Schema();
        var options = DefaultSerializerOptions.GenerateObjectJsonOptions;
        
        parametersSchema.Properties = new Dictionary<string, Schema>();
        parametersSchema.Required = new List<string>();
        parametersSchema.Type = "object";
        foreach (var param in parameters)
        {
            
            var type = param.ParameterType;
            if(type.Name == "CancellationToken")
                continue;
            var descriptionsDics = TypeDescriptionExtractor.GetDescriptionDic(type);
            var desc = TypeDescriptionExtractor.GetDescription(param);
            descriptionsDics[param.Name.ToCamelCase()] = desc;

            var schema = GoogleSchemaHelper.ConvertToSchema(type, options, descriptionsDics);
            schema.Description = desc;
            parametersSchema.Properties.Add(param.Name.ToCamelCase(), schema);
            parametersSchema.Required.Add(param.Name.ToCamelCase());
        }

        var functionDescription = TypeDescriptionExtractor.GetDescription(func.Method);

        FunctionDeclaration functionObject = new FunctionDeclaration();
        functionObject.Description = description ?? functionDescription;
        functionObject.Parameters = parametersSchema;
        functionObject.Name = name ?? func.Method.Name;
       
        return functionObject;
    }
}