using CodeExecutor;
using GenerativeAI;

//Get API Key
var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY", EnvironmentVariableTarget.User);
if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("Please set the GOOGLE_API_KEY environment variable.");
    return;
}
restart:

//Initialize Model
var model = new GenerativeModel(apiKey, GoogleAIModels.Gemini2Flash);

//Initialize Javascript Code Executor
var javascripExecutor = new JavascriptCodeExecutor();
var javascriptTool = javascripExecutor.AsGoogleFunctionTool();

//Start Chat
var chat = model.StartChat();

//Add Javascript Code Executor
chat.AddFunctionTool(javascriptTool);


//System Instruction
chat.SystemInstruction = "Your job is to create javascript code and pass it to ExecuteJavascriptCodeAsync for execution. \r\n- Always use ExecuteJavascriptCodeAsync function to execute the javascript code.\r\n- use console.log to print the results.";

//write some information to make some sense
Console.Clear();
Console.WriteLine(
    "Tell me what to do, and I'll whip up some JavaScript magic to get it done! For example, I can count from 1 to 100.");
Console.WriteLine("Examples:");
Console.WriteLine("Print fibonacci numbers up to 100");
Console.WriteLine("Print the first 100 prime numbers in reverse order");
Console.WriteLine("calculate the 4th power of 67th prime number");
Console.WriteLine("calculate the acceleration due to gravity on mars");
Console.WriteLine("** Note: If I start to hallucinate or get confused. Just type restart and press enter!");

//Main Loop
while (true)
{
    //Get Input Prompt
    Console.WriteLine();
    Console.WriteLine("Input: ");
    var input = Console.ReadLine();
    if(input == "restart")
        goto restart;
    var prompt =
        $"\r\nExecute the javascript code for this task:\r\n\r\n{input}\r\n\r\n";
    
    //Generate Response
    var result = await chat.GenerateContentAsync(prompt).ConfigureAwait(false);
    
    //Print Result
    Console.WriteLine();
    Console.Write("Result from Gemini:\r\n");
    Console.WriteLine(result.Text());
}