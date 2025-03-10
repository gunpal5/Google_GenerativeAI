using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using GenerativeAI.Types;

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
            var descriptionsDics = GetDescriptionDic(type);
            var desc = GetDescription(param);
            descriptionsDics[param.Name.ToCamelCase()] = desc;

            var schema = GoogleSchemaHelper.ConvertToSchema(type, options, descriptionsDics);
            schema.Description = desc;
            parametersSchema.Properties.Add(param.Name.ToCamelCase(), schema);
            parametersSchema.Required.Add(param.Name.ToCamelCase());
        }

        var functionDescription = GetDescription(func.Method);

        FunctionDeclaration functionObject = new FunctionDeclaration();
        functionObject.Description = description ?? functionDescription;
        functionObject.Parameters = parametersSchema;
        functionObject.Name = name ?? func.Method.Name;
       
        return functionObject;
    }

    public static string GetDescription(ParameterInfo paramInfo)
    {
        var attribute = paramInfo.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
        return attribute?.Description ?? string.Empty;
    }

    private static Dictionary<string, string> GetDescriptionDic(Type type, Dictionary<string, string>? descriptions = null)
    {
        descriptions = descriptions ?? new Dictionary<string, string>();
        descriptions[type.Name.ToCamelCase()] = GetDescription(type);
        foreach (var member in type.GetMembers())
        {
            var description = GetDescription(member);
            if (!string.IsNullOrEmpty(description))
            {
                descriptions[member.Name.ToCamelCase()] = description;
            }

            if (member.MemberType == MemberTypes.TypeInfo || member.MemberType == MemberTypes.NestedType)
            {
                var nestedType = member as Type;
                if (nestedType != null)
                {
                    GetDescriptionDic(nestedType, descriptions);
                }
            }
        }

        return descriptions;
    }

    private static string GetDescription(MemberInfo member)
    {
        var attribute = member.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
        return attribute?.Description ?? string.Empty;
    }
}