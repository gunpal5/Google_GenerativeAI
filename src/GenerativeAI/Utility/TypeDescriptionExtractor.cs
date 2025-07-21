using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace GenerativeAI.Utility;

/// <summary>
/// Utility class for extracting description information from types and their members using reflection.
/// </summary>
public static class TypeDescriptionExtractor
{
    // public static string GetDescription(ParameterInfo paramInfo)
    // {
    //     var attribute = paramInfo.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
    //     return attribute?.Description ?? string.Empty;
    // }
    /// <summary>
    /// Extracts the description from a DescriptionAttribute on the provided attribute provider.
    /// </summary>
    /// <param name="attributeProvider">The attribute provider (e.g., PropertyInfo, ParameterInfo) to extract description from.</param>
    /// <returns>The description text, or empty string if no description is found.</returns>
    public static string GetDescription(ICustomAttributeProvider attributeProvider)
    {
        // Look up any description attributes.
        DescriptionAttribute? descriptionAttr = attributeProvider?
            .GetCustomAttributes(inherit: true)
            .Select(attr => attr as DescriptionAttribute)
            .FirstOrDefault(attr => attr is not null);
        
        return descriptionAttr?.Description ?? string.Empty;
    }
    /// <summary>
    /// Extracts descriptions from a type and its members, returning them as a dictionary.
    /// </summary>
    /// <param name="type">The type to extract descriptions from.</param>
    /// <param name="descriptions">Optional existing dictionary to add descriptions to.</param>
    /// <returns>A dictionary containing member names (in camelCase) mapped to their descriptions.</returns>
    public static Dictionary<string, string> GetDescriptionDic(Type type, Dictionary<string, string>? descriptions = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(type);
#else
        if (type == null) throw new ArgumentNullException(nameof(type));
#endif
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