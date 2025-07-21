using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using CSharpToJsonSchema;
using GenerativeAI.Microsoft;

using ChatOptions = Microsoft.Extensions.AI.ChatOptions;
using GenerativeAI.Microsoft.Extensions;
using GenerativeAI.Tests;
using GenerativeAI.Tools;
using Microsoft.Extensions.AI;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using AITool = Microsoft.Extensions.AI.AITool;

namespace GenerativeAI.IntegrationTests;



public class Microsoft_AIFunction_Tests:TestBase
{
    public Microsoft_AIFunction_Tests(ITestOutputHelper helper) : base(helper)
    {
        
    }
    [Fact]
    public async Task ShouldWorkWithTools()
    {
        Assert.SkipUnless(IsGeminiApiKeySet,GeminiTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey);
        var chatOptions = new ChatOptions();
        
        var message = new ChatMessage(ChatRole.User, "What is the weather in New York?");
        var response = await chatClient.GetResponseAsync(message,options:chatOptions);

        response.Text.Contains("New York", StringComparison.InvariantCultureIgnoreCase)
            .ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldWorkWithTools_with_Streaming()
    {
        Assert.SkipUnless(IsGeminiApiKeySet,GeminiTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey);
        var chatOptions = new ChatOptions();
        
        var message = new ChatMessage(ChatRole.User, "What is the weather in New York?");
        await foreach (var response in chatClient.GetStreamingResponseAsync(message, options: chatOptions))
        {
            Console.WriteLine(response.Text);
        }
        // var response = await chatClient.GetResponseAsync(message,options:chatOptions);
        //
        // response.Text.Contains("New York", StringComparison.InvariantCultureIgnoreCase)
        //     .ShouldBeTrue();
    }
    [Fact]
    public async Task ShouldWorkWithComplexClasses()
    {
        Assert.SkipUnless(IsGeminiApiKeySet,GeminiTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey, modelName:"models/gemini-2.0-flash");
        var chatOptions = new ChatOptions();
        
        chatOptions.Tools = new List<AITool>{AIFunctionFactory.Create(GetStudentRecordAsync)};
        var message = new ChatMessage(ChatRole.User, "How does student john doe in senior grade is doing this year, enrollment start 01-01-2024 to 01-01-2025?");
        var response = await chatClient.GetResponseAsync(message,options:chatOptions);

        response.Text.Contains("John", StringComparison.InvariantCultureIgnoreCase)
            .ShouldBeTrue();
        Console.WriteLine(response.Text);
    }
    
    [Fact]
    public async Task ShouldWorkWithComplexClasses_Streaming()
    {
        Assert.SkipUnless(IsGeminiApiKeySet,GeminiTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey, modelName: "models/gemini-2.0-flash")
        {
            AutoCallFunction = false
        };
        var chatOptions = new ChatOptions();
        
        chatOptions.Tools = new List<AITool>{AIFunctionFactory.Create(GetStudentRecordAsync)};
        var message = new ChatMessage(ChatRole.User, "How does student john doe in senior grade is doing this year, enrollment start 01-01-2024 to 01-01-2025?");
        //var response = await chatClient.GetResponseAsync(message,options:chatOptions);

        await foreach (var resp in chatClient.GetStreamingResponseAsync(message, options: chatOptions)
                           )
        {
            Console.WriteLine(resp.Text);
        }
        // response.Text.Contains("John", StringComparison.InvariantCultureIgnoreCase)
        //     .ShouldBeTrue();
        // Console.WriteLine(response.Text);
    }
    
    [Fact]
    public async Task ShouldWorkWith_BookStoreService()
    {
        Assert.SkipUnless(IsGeminiApiKeySet,GeminiTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey,GoogleAIModels.Gemini2Flash,false).AsBuilder().UseFunctionInvocation().Build();
        var chatOptions = new ChatOptions();
       
        // chatOptions.Tools = new List<AITool>{AIFunctionFactory.Create(GetBookPageContentAsync,new AIFunctionFactoryOptions()
        // {
        //     
        // })};
        var deleg = new Func<string, int, Task<string>>(GetBookPageContentAsync);
        
        chatOptions.Tools = new List<AITool>{AIFunctionFactory.Create(
            deleg.Method,
            this,
            "GetBookPageContentAsync",
            "Get book page content",
            null
        )};
        var message = new ChatMessage(ChatRole.User, "what is written on page 96 in the book 'damdamadum'");
        var response = await chatClient.GetResponseAsync(message,options:chatOptions);

        response.Text.ShouldContain("weather",Case.Insensitive);
    }
    
    [Fact]
    public async Task ShouldWorkWith_NoParameters_FunctionFactory()
    {
        Assert.SkipUnless(IsGeminiApiKeySet,GeminiTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey,GoogleAIModels.Gemini2Flash,false).AsBuilder().UseFunctionInvocation().Build();
        var chatOptions = new ChatOptions();
       
        // chatOptions.Tools = new List<AITool>{AIFunctionFactory.Create(GetBookPageContentAsync,new AIFunctionFactoryOptions()
        // {
        //     
        // })};
        var deleg2 = new Func<string>(GetCurrentDateTime);
        
        chatOptions.Tools = new List<AITool>{AIFunctionFactory.Create(
            deleg2.Method,
            this,
            "GetCurrentDateTime",
            "Returns the current date & time",
            null
        )};
        var message = new ChatMessage(ChatRole.User, "what is current date & time");
        var response = await chatClient.GetResponseAsync(message,options:chatOptions);

        response.Text.ShouldContain("date",Case.Insensitive);
    }
    
    [Fact]
    public async Task ShouldWorkWith_NoParameters_MeaiTools()
    {
        Assert.SkipUnless(IsGeminiApiKeySet,GeminiTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey,GoogleAIModels.Gemini2Flash,false).AsBuilder().UseFunctionInvocation().Build();
        var chatOptions = new ChatOptions();
       
        // chatOptions.Tools = new List<AITool>{AIFunctionFactory.Create(GetBookPageContentAsync,new AIFunctionFactoryOptions()
        // {
        //     
        // })};
        var deleg2 = new Func<string>(GetCurrentDateTime);

        chatOptions.Tools = (new Tools([GetCurrentDateTime])).AsMeaiTools();
        var message = new ChatMessage(ChatRole.User, "what is current date & time");
        var response = await chatClient.GetResponseAsync(message,options:chatOptions);

        response.Text.ShouldContain("date",Case.Insensitive);
    }
    
    [Fact]
    public async Task ShouldWorkWith_NoParameters_QuickTools()
    {
        Assert.SkipUnless(IsGeminiApiKeySet,GeminiTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey,GoogleAIModels.Gemini2Flash,false).AsBuilder().UseFunctionInvocation().Build();
        var chatOptions = new ChatOptions();
       
        // chatOptions.Tools = new List<AITool>{AIFunctionFactory.Create(GetBookPageContentAsync,new AIFunctionFactoryOptions()
        // {
        //     
        // })};
        var deleg2 = new Func<string>(GetCurrentDateTime);

        chatOptions.Tools = (new QuickTools([GetCurrentDateTime])).ToMeaiFunctions();
        var message = new ChatMessage(ChatRole.User, "what is current date & time");
        var response = await chatClient.GetResponseAsync(message,options:chatOptions);

        response.Text.ShouldContain("date",Case.Insensitive);
    }
    
     [Fact]
    public async Task ShouldWorkWith_NoParameters_FunctionFactory_SelfInvoking()
    {
        Assert.SkipUnless(IsGeminiApiKeySet,GeminiTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey,GoogleAIModels.Gemini2Flash,false).AsBuilder().UseFunctionInvocation().Build();
        var chatOptions = new ChatOptions();
       
        // chatOptions.Tools = new List<AITool>{AIFunctionFactory.Create(GetBookPageContentAsync,new AIFunctionFactoryOptions()
        // {
        //     
        // })};
        var deleg2 = new Func<string>(GetCurrentDateTime);
        
        chatOptions.Tools = new List<AITool>{AIFunctionFactory.Create(
            deleg2.Method,
            this,
            "GetCurrentDateTime",
            "Returns the current date & time",
            null
        )};
        var message = new ChatMessage(ChatRole.User, "what is current date & time");
        var response = await chatClient.GetResponseAsync(message,options:chatOptions);

        response.Text.ShouldContain("date",Case.Insensitive);
    }
    
    [Fact]
    public async Task ShouldWorkWith_NoParameters_MeaiTools_SelfInvoking()
    {
        Assert.SkipUnless(IsGeminiApiKeySet,GeminiTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey,GoogleAIModels.Gemini2Flash);
        var chatOptions = new ChatOptions();
       
        // chatOptions.Tools = new List<AITool>{AIFunctionFactory.Create(GetBookPageContentAsync,new AIFunctionFactoryOptions()
        // {
        //     
        // })};
        var deleg2 = new Func<string>(GetCurrentDateTime);

        chatOptions.Tools = (new Tools([GetCurrentDateTime])).AsMeaiTools();
        var message = new ChatMessage(ChatRole.User, "what is current date & time");
        var response = await chatClient.GetResponseAsync(message,options:chatOptions);

        response.Text.ShouldContain("date",Case.Insensitive);
    }
    
    [Fact]
    public async Task ShouldWorkWith_NoParameters_QuickTools_SelfInvoking()
    {
        Assert.SkipUnless(IsGeminiApiKeySet,GeminiTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey,GoogleAIModels.Gemini2Flash);
        var chatOptions = new ChatOptions();
       
        // chatOptions.Tools = new List<AITool>{AIFunctionFactory.Create(GetBookPageContentAsync,new AIFunctionFactoryOptions()
        // {
        //     
        // })};
        var deleg2 = new Func<string>(GetCurrentDateTime);

        chatOptions.Tools = (new QuickTools([GetCurrentDateTime])).ToMeaiFunctions();
        var message = new ChatMessage(ChatRole.User, "what is current date & time");
        var response = await chatClient.GetResponseAsync(message,options:chatOptions);

        response.Text.ShouldContain("date",Case.Insensitive);
    }
    
    [Fact]
    public async Task ShouldWorkWith_BookStoreService_with_Streaming()
    {
        Assert.SkipUnless(IsGeminiApiKeySet,GeminiTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey);
        var chatOptions = new ChatOptions();
       
        chatOptions.Tools = new List<AITool>{AIFunctionFactory.Create(GetBookPageContentAsync,new AIFunctionFactoryOptions()
        {
            
        })};
        var message = new ChatMessage(ChatRole.User, "what is written on page 96 in the book 'damdamadum'");
        await foreach (var resp in chatClient.GetStreamingResponseAsync(message,options:chatOptions))
        {
            Console.WriteLine(resp.Text);
        }
        // var response = await chatClient.GetResponseAsync(message,options:chatOptions);
        //
        // response.Text.ShouldContain("damdamadum",Case.Insensitive);
    }
    
    [System.ComponentModel.Description("Get book page content")]
    public static Task<string> GetBookPageContentAsync(string bookName, int bookPageNumber)
    {
        return Task.FromResult("this is a cool weather out there, and I am stuck at home.");
    }
    
    [System.ComponentModel.Description("Get the current weather in a given location")]
    public Weather GetCurrentWeather(string location, Unit unit = Unit.Celsius)
    {
        return new Weather
        {
            Location = location,
            Temperature = 30.0,
            Unit = unit,
            Description = "Sunny",
        };
    }
    [Description("Get the current date and time")]
    [FunctionTool(MeaiFunctionTool = true)]
    public static string GetCurrentDateTime()
    {
        return DateTime.Now.ToString("dddd dd MMMM yyyy HH:mm:ss");
    }
    
    [System.ComponentModel.Description("Get student record for the year")]

    public async Task<StudentRecord> GetStudentRecordAsync(QueryStudentRecordRequest query)
    {
        return new StudentRecord
        {
            StudentId = "12345",
            FullName = query.FullName,
            Level = StudentRecord.GradeLevel.Senior,
            EnrolledCourses = new List<string> { "Math 101", "Physics 202", "History 303" },
            Grades = new Dictionary<string, double>
            {
                { "Math 101", 3.5 },
                { "Physics 202", 3.8 },
                { "History 303", 3.9 }
            },
            EnrollmentDate = new DateTime(2020, 9, 1),
            IsActive = true
        };
    }
    
    
    public enum Unit
    {
        Celsius,
        Fahrenheit,
        Imperial
    }

    public class Weather
    {
        public string Location { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public Unit Unit { get; set; }
        public string Description { get; set; } = string.Empty;
    }
    
    public class StudentRecord
    {
        public enum GradeLevel
        {
            Freshman,
            Sophomore,
            Junior,
            Senior,
            Graduate
        }

        public string StudentId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public GradeLevel Level { get; set; } = GradeLevel.Freshman;
        public List<string> EnrolledCourses { get; set; } = new List<string>();
        public Dictionary<string, double> Grades { get; set; } = new Dictionary<string, double>();
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        public double CalculateGPA()
        {
            if (Grades.Count == 0) return 0.0;
            return Grades.Values.Average();
        }
    }

    [Description("Request class containing filters for querying student records.")]
    public class QueryStudentRecordRequest
    {
        [Description("The student's full name.")]
        public string FullName { get; set; } = string.Empty;
    
        [Description("Grade filters for querying specific grades, e.g., Freshman or Senior.")]
        public List<StudentRecord.GradeLevel> GradeFilters { get; set; } = new();
    
        [Description("The start date for the enrollment date range. ISO 8601 standard date")]
        public DateTime EnrollmentStartDate { get; set; }
        
        [Description("The end date for the enrollment date range. ISO 8601 standard date")]
        public DateTime EnrollmentEndDate { get; set; }
    
        [Description("The flag indicating whether to include only active students.")]
        public bool? IsActive { get; set; } = true;
    }
    
}