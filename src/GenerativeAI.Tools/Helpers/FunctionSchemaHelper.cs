using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GenerativeAI.Types;
using GenerativeAI.Utility;

namespace GenerativeAI.Tools.Helpers;

public static class FunctionSchemaHelper
{
    public static FunctionDeclaration CreateFunctionDecleration(Delegate func, string? name, string? description)
    {
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