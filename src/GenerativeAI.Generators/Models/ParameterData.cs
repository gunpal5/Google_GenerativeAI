#nullable enable
using System.Collections.Generic;

namespace GenerativeAI.Generators.Models;

public readonly record struct ParameterData(
    string Name,
    string Description,
    string Type,
    string SchemaType,
    string? Format,
    IReadOnlyCollection<string> EnumValues,
    IReadOnlyCollection<ParameterData> Properties,
    IReadOnlyCollection<ParameterData> ArrayItem,
    bool IsRequired,
    bool IsNullable,
    string DefaultValue);