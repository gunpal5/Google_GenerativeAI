using System.ComponentModel;
using System.Text;
using System.Text.Json;
using CSharpToJsonSchema;
using Jint;
using Jint.Native;
using Jint.Runtime;

namespace CodeExecutor;

/// <summary>
/// Provides functionality to execute JavaScript code using a JavaScript engine and return the result.
/// </summary>
public class 
    
    JavascriptCodeExecutor : IJavascriptCodeExecutor
{
    public async Task<object?> ExecuteJavascriptCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        using var engine = new Engine();
        var sb = new StringBuilder();

        Console.WriteLine("Result from Javascript Engine:");
        engine.SetValue("console", new
        {
            log = new Action<object>(o =>
            {
                var output = o switch
                {
                    JsValue jsValue when jsValue.IsString() => jsValue.AsString(),
                    JsValue jsValue => jsValue.ToObject()?.ToString() ?? string.Empty,
                    string str => str,
                    _ => JsonSerializer.Serialize(o)
                };
                Console.WriteLine(output);
                sb.AppendLine(output);
            })
        });

        var evaluationResult = engine.Evaluate(code);

        return evaluationResult.Type switch
        {
            Types.Null => sb.ToString(),
            _ when evaluationResult.IsArray() => evaluationResult.AsArray()
                .Select(j => j.ToObject())
                .Where(obj => obj != null)
                .ToList(),
            _ => evaluationResult.ToObject()
        };
    }
}

[GenerateJsonSchema]
public interface IJavascriptCodeExecutor
{
    [Description("this function executes javascript code. pass any javascript code to execute.")]
    public Task<object?> ExecuteJavascriptCodeAsync([Description("Javascript code to execute")] string code,
        CancellationToken cancellationToken = default);
}