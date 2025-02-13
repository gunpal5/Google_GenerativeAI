using System.Text.Json.Nodes;
using CSharpToJsonSchema;
using GenerativeAI.Core;
using GenerativeAI.Types;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Tool = GenerativeAI.Types.Tool;

namespace GenerativeAI.Tools;

public class GenericFunctionTool:IFunctionTool
{
    public GenericFunctionTool(IEnumerable<CSharpToJsonSchema.Tool> tools, IReadOnlyDictionary<string, Func<string, CancellationToken, Task<string>>> calls)
    {
        Calls = calls;
        Tools = tools.ToList();
    }
    public IReadOnlyDictionary<string, Func<string, CancellationToken, Task<string>>> Calls { get; private set; }
    public IReadOnlyList<CSharpToJsonSchema.Tool> Tools { get; private set; }
    //public void Add()
    public Tool AsTool()
    {
        return new Tool()
        {
            FunctionDeclarations = Tools.Select(s => new FunctionDeclaration()
            {
                Description = s.Description,
                Name = s.Name,
                Parameters = ToSchema(s.Parameters)
            }).ToList(),
        };
    }

    Schema ToSchema(object parameters)
    {
        var param = JsonSerializer.Serialize(parameters);
        return JsonSerializer.Deserialize<Schema>(param);
    }

    public async Task<FunctionResponse?> CallAsync(FunctionCall functionCall, CancellationToken cancellationToken = default)
    {
        if (this.Calls.TryGetValue(functionCall.Name, out var call))
        {
            var str = JsonSerializer.Serialize(functionCall.Args);
            var response = await call(str, cancellationToken);

            var node = JsonNode.Parse(response);

            return new FunctionResponse() { Id = functionCall.Id, Name = functionCall.Name, Response = node };
        }
        return null;
    }

    public bool IsContainFunction(string name)
    {
        return Tools.Any(s => s.Name == name);
    }
}