using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Core;

/// <summary>
/// Serializes a PascalCase enum member as a camelCase string (for example <c>Flex</c> as <c>"flex"</c>). Used by enums
/// whose wire values are lowercase or camelCase.
/// </summary>
/// <typeparam name="TEnum">The enum type.</typeparam>
public sealed class CamelCaseEnumConverter<TEnum> : JsonStringEnumConverter<TEnum> where TEnum : struct, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CamelCaseEnumConverter{TEnum}"/> class.
    /// </summary>
    public CamelCaseEnumConverter() : base(JsonNamingPolicy.CamelCase)
    {
    }
}

/// <summary>
/// Serializes a PascalCase enum member as a lowercase snake_case string (for example <c>VeryHigh</c> as
/// <c>"very_high"</c>). Used by enums whose wire values are lowercase snake_case.
/// </summary>
/// <typeparam name="TEnum">The enum type.</typeparam>
public sealed class SnakeCaseLowerEnumConverter<TEnum> : JsonStringEnumConverter<TEnum> where TEnum : struct, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SnakeCaseLowerEnumConverter{TEnum}"/> class.
    /// </summary>
    public SnakeCaseLowerEnumConverter() : base(JsonNamingPolicy.SnakeCaseLower)
    {
    }
}

/// <summary>
/// Serializes a PascalCase enum member as a lowercase kebab-case string (for example <c>VeryHigh</c> as
/// <c>"very-high"</c>). Used by enums whose wire values are lowercase kebab-case.
/// </summary>
/// <typeparam name="TEnum">The enum type.</typeparam>
public sealed class KebabCaseLowerEnumConverter<TEnum> : JsonStringEnumConverter<TEnum> where TEnum : struct, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KebabCaseLowerEnumConverter{TEnum}"/> class.
    /// </summary>
    public KebabCaseLowerEnumConverter() : base(JsonNamingPolicy.KebabCaseLower)
    {
    }
}
