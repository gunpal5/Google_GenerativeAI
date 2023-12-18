using System.Collections.Generic;

namespace GenerativeAI.Generators.Models;

public readonly record struct InterfaceData(
    string Namespace,
    string Name,
    IReadOnlyCollection<MethodData> Methods);