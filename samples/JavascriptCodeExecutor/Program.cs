// See https://aka.ms/new-console-template for more information

using CodeExecutor;
using GenerativeAI;
using GenerativeAI.Tools;


var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY", EnvironmentVariableTarget.User);
restart:
var model = new GenerativeModel(apiKey, GoogleAIModels.Gemini2Flash);

var javascripExecutor = new CodeExecutor.JavascriptCodeExecutor();
var javascriptTool = new GenericFunctionTool(javascripExecutor.AsTools(), javascripExecutor.AsCalls());

var chat = model.StartChat();
chat.AddFunctionTool(javascriptTool);
chat.SystemInstruction = "Your job is to create javascript code and pass it to ExecuteJavascriptCodeAsync for execution. \r\n- Always use ExecuteJavascriptCodeAsync function to execute the javascript code.\r\n- use console.log to print the results.";
Console.WriteLine(
    "Tell me what to do, and I'll whip up some JavaScript magic to get it done! For example, I can count from 1 to 100.");
Console.WriteLine("Examples:");
Console.WriteLine("Print fibonacci numbers up to 100");
Console.WriteLine("Print the first 100 prime numbers in reverse order");
Console.WriteLine("calculate the 4th power of 67th prime number");
Console.WriteLine("calculate the acceleration due to gravity on mars");
Console.WriteLine("** Note: If I start to hallucinate or get confused. Just type restart and press enter!");

while (true)
{
    Console.WriteLine();
    Console.WriteLine("Input: ");
    var input = Console.ReadLine();
    if(input == "restart")
        goto restart;
    var prompt =
        $"\r\nExecute the javascript code for this task:\r\n\r\n{input}\r\n\r\n";
    
    var result = await chat.GenerateContentAsync(prompt).ConfigureAwait(false);
    Console.WriteLine();
    Console.Write("Result from Gemini:\r\n");
    Console.WriteLine(result.Text());
}