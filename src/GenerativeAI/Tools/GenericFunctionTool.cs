using System.Text.Json.Nodes;
using CSharpToJsonSchema;
using GenerativeAI.Core;
using GenerativeAI.Types;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Tool = GenerativeAI.Types.Tool;

namespace GenerativeAI.Tools;


/// <inheritdoc/>
public class GenericFunctionTool:IFunctionTool
{
    /// <summary>
    /// Represents a generic functional tool that enables interaction with a set of tools and their associated functions,
    /// facilitating the conversion of provided tools into a compatible format for execution and integration.
    /// </summary>
    public GenericFunctionTool(IEnumerable<CSharpToJsonSchema.Tool> tools, IReadOnlyDictionary<string, Func<string, CancellationToken, Task<string>>> calls)
    {
        Calls = calls;
        Tools = tools.ToList();
    }
    public IReadOnlyDictionary<string, Func<string, CancellationToken, Task<string>>> Calls { get; private set; }
    public IReadOnlyList<CSharpToJsonSchema.Tool> Tools { get; private set; }
    
   
    /// <inheritdoc/>
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

    private Schema ToSchema(object parameters)
    {
        var param = JsonSerializer.Serialize(parameters);
        return JsonSerializer.Deserialize<Schema>(param);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public bool IsContainFunction(string name)
    {
        return Tools.Any(s => s.Name == name);
    }
}