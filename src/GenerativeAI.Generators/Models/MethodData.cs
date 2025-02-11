using System.Collections.Generic;

namespace GenerativeAI.Generators.Models;

public readonly record struct MethodData(
    string Name,
    string Description,
    bool IsAsync,
    bool IsVoid,
    IReadOnlyCollection<ParameterData> Parameters);