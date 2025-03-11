using System.Collections.Generic;
using System.Reflection;

namespace GenerativeAI.Utility;

public static class TypeDescriptionExtractor
{
    public static string GetDescription(ParameterInfo paramInfo)
    {
        var attribute = paramInfo.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
        return attribute?.Description ?? string.Empty;
    }
    public static string GetDescription(MemberInfo member)
    {
        var attribute = member.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
        return attribute?.Description ?? string.Empty;
    }
    public static Dictionary<string, string> GetDescriptionDic(Type type, Dictionary<string, string>? descriptions = null)
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
}