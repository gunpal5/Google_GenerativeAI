using System.Text.Json;
using System.Text.Json.Nodes;
using CSharpToJsonSchema;
using GenerativeAI.Core;
using GenerativeAI.Types;

using JsonSerializer = System.Text.Json.JsonSerializer;
using Tool = GenerativeAI.Types.Tool;

namespace GenerativeAI.Tools;

/// <summary>
/// GenericFunctionTool provides a generic implementation of the <see cref="IFunctionTool"/> interface,
/// allowing seamless interaction with a collection of tools and their corresponding functions.
/// It utilizes the code generation capabilities available in <see href="https://www.nuget.org/packages/CSharpToJsonSchema">CSharpToJsonSchema</see> for transforming
/// tool definitions into executable formats and managing function invocations.
/// </summary>
public class GenericFunctionTool:GoogleFunctionTool
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
    public override Tool AsTool()
    {
        return new Tool()
        {
            FunctionDeclarations = Tools.Select(s => new FunctionDeclaration()
            {
                Description = s.Description,
                Name = s.Name ?? throw new InvalidOperationException("Tool name cannot be null"),
                Parameters = s.Parameters != null ? ToSchema(s.Parameters) : null
            }).ToList(),
        };
    }

    private Schema? ToSchema(object parameters)
    {
        var param = JsonSerializer.Serialize(parameters, OpenApiSchemaSourceGenerationContext.Default.OpenApiSchema);
        return JsonSerializer.Deserialize(param,SchemaSourceGenerationContext.Default.Schema);
    }

    /// <inheritdoc/>
    public override async Task<FunctionResponse?> CallAsync(FunctionCall functionCall, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(functionCall);
#else
        if (functionCall == null) throw new ArgumentNullException(nameof(functionCall));
#endif
        #pragma disable warning IL2026, IL3050
        if (this.Calls.TryGetValue(functionCall.Name, out var call))
        {
            string? args = null;
            if (functionCall.Args !=null)
            {
                args = functionCall.Args.ToJsonString();
            }
            // else if (functionCall.Args is JsonNode jsonNode)
            // {
            //     args = jsonNode.ToJsonString();
            // }
            // else if (functionCall.Args is JsonObject jsonObject)
            // {
            //     args = jsonObject.ToJsonString();
            // }
            else
            {
                throw new NotImplementedException();
                //args = JsonSerializer.Serialize(functionCall.Args, DefaultSerializerOptions.Options.GetTypeInfo());
            }
            var response = await call(args, cancellationToken).ConfigureAwait(false);

            var node = JsonNode.Parse(response);
            var responseNode = new JsonObject();

            responseNode["name"] = functionCall.Name;
            responseNode["content"] = node;
            return new FunctionResponse() { Id = functionCall.Id, Name = functionCall.Name, 
                
                Response = responseNode,
            };
#pragma restore warning IL2026, IL3050
        }
        return null;
    }

    /// <inheritdoc/>
    public override bool IsContainFunction(string name)
    {
        return Tools.Any(s => s.Name == name);
    }
}