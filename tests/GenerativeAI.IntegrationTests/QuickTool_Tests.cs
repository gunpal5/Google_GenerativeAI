using System.ComponentModel;
using System.Text;
using System.Text.Json.Nodes;
using GenerativeAI.Tests;
using GenerativeAI.Tools;
using GenerativeAI.Types;
using Shouldly;
using Xunit;

namespace GenerativeAI.IntegrationTests;

public class QuickTool_Tests : TestBase
{
    public QuickTool_Tests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public async Task ShouldCreateQuickTool_Async()
    {
        var func =
            ( ([Description("Student Name")] string studentName,
                [Description("Student Grade")] GradeLevel grade) =>
            {
                return
                    $"{studentName} in {grade} grade is achieving remarkable scores in math and physics, showcasing outstanding progress.";
            });

        var quickFt = new QuickTool(func, "GetStudentRecordAsync", "Return student record for the year");

        var args = new JsonObject();
        args.Add("studentName", "John");
        args.Add("grade", "Freshman");
        var res = await quickFt.CallAsync(new FunctionCall()
        {
            Name = "GetStudentRecordAsync",
            Args = args
        }, cancellationToken: TestContext.Current.CancellationToken);

        (res.Response as JsonNode)["content"].GetValue<string>().ShouldContain("John");
    }

    [Fact]
    public async Task ShouldCreateQuickTool()
    {
        var func =
            (([Description("Student Name")] string studentName, [Description("Student Grade")] GradeLevel grade) =>
            {
                return
                    $"{studentName} in {grade} grade is achieving remarkable scores in math and physics, showcasing outstanding progress.";
            });

        var quickFt = new QuickTool(func, "GetStudentRecordAsync", "Return student record for the year");

        var args = new JsonObject();
        args.Add("studentName", "John");
        args.Add("grade", "Freshman");
        var res = await quickFt.CallAsync(new FunctionCall()
        {
            Name = "GetStudentRecordAsync",
            Args = args
        }, cancellationToken: TestContext.Current.CancellationToken);
        (res.Response as JsonNode)["content"].GetValue<string>().ShouldContain("John");
    }

    [Fact]
    public async Task ShouldCreateQuickTool_void()
    {
        bool invoked = false;
        var func =
            (([Description("Student Name")] string studentName, [Description("Student Grade")] GradeLevel grade) =>
            {
                var str =
                    $"{studentName} in {grade} grade is achieving remarkable scores in math and physics, showcasing outstanding progress.";
                Console.WriteLine(str);
                invoked = true;
            });

        var quickFt = new QuickTool(func, "GetStudentRecordAsync", "Return student record for the year");

        var args = new JsonObject();
        args.Add("studentName", "John");
        args.Add("grade", "Freshman");
        var res = await quickFt.CallAsync(new FunctionCall()
        {
            Name = "GetStudentRecordAsync",
            Args = args
        }, cancellationToken: TestContext.Current.CancellationToken);
        invoked.ShouldBeTrue();
        (res.Response as JsonNode)["content"].GetValue<string>().ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldCreateQuickTool_Task()
    {
        bool invoked = false;
        var func = (async ([Description("Student Name")] string studentName,
            [Description("Student Grade")] GradeLevel grade) =>
        {
            var str =
                $"{studentName} in {grade} grade is achieving remarkable scores in math and physics, showcasing outstanding progress.";
            await Task.Delay(100, cancellationToken: TestContext.Current.CancellationToken);
            invoked = true;
        });

        var quickFt = new QuickTool(func, "GetStudentRecordAsync", "Return student record for the year");

        var args = new JsonObject();
        args.Add("studentName", "John");
        args.Add("grade", "Freshman");
        var res = await quickFt.CallAsync(new FunctionCall()
        {
            Name = "GetStudentRecordAsync",
            Args = args
        }, cancellationToken: TestContext.Current.CancellationToken);
        invoked.ShouldBeTrue();
        (res.Response as JsonNode)["content"].GetValue<string>().ShouldBeEmpty();

        quickFt.FunctionDeclaration.Parameters.ShouldSatisfyAllConditions(
            parameters =>
            {
                parameters.ShouldNotBeNull();
                parameters.Properties.Keys.ShouldContain("studentName");
                parameters.Properties.Keys.ShouldContain("grade");
                parameters.Properties["studentName"].Type.ShouldBe("string");
                parameters.Properties["studentName"].Description.ShouldBe("Student Name");
                parameters.Properties["grade"].Type.ShouldBe("string");
                parameters.Properties["grade"].Description.ShouldBe("Student Grade");
              
            });
    }

    [Fact]
    public async Task ShouldCreateQuickTool_ComplexDataTypes()
    {
        var func = (([Description("Request to query student record")] QueryStudentRecordRequest query) =>
        {
            return Task.FromResult(new StudentRecord
            {
                StudentId = "12345",
                FullName = "John Doe",
                Level = GradeLevel.Freshman,
                EnrolledCourses = new List<string> { "Math", "Physics", "Chemistry" },
                Grades = new Dictionary<string, double>
                {
                    { "Math", 95.0 },
                    { "Physics", 89.0 },
                    { "Chemistry", 88.0 }
                },
                EnrollmentDate = new DateTime(2023, 1, 10),
                IsActive = true
            });
        });
        
        

        var quickFt = new QuickTool(func, "GetStudentRecordAsync", "Return student record for the year");

        var args = new JsonObject();
        args.Add("studentName", "John");
        args.Add("grade", "Freshman");
        var res = await quickFt.CallAsync(new FunctionCall()
        {
            Name = "GetStudentRecordAsync",
            Args = args
        }, cancellationToken: TestContext.Current.CancellationToken);

        var content = res.Response as JsonNode;
        content = content["content"] as JsonObject;
        content["studentId"].GetValue<string>().ShouldBe("12345");
        content["fullName"].GetValue<string>().ShouldBe("John Doe");
        content["level"].GetValue<string>().ShouldBe("Freshman");
        content["enrolledCourses"].AsArray().Select(x => x.GetValue<string>())
            .ShouldBe(new List<string> { "Math", "Physics", "Chemistry" });
        content["grades"]["Math"].GetValue<double>().ShouldBe(95.0);
        content["grades"]["Physics"].GetValue<double>().ShouldBe(89.0);
        content["grades"]["Chemistry"].GetValue<double>().ShouldBe(88.0);
        content["enrollmentDate"].GetValue<DateTime>().ShouldBe(new DateTime(2023, 1, 10));
        content["isActive"].GetValue<bool>().ShouldBe(true);
        
        quickFt.FunctionDeclaration.Parameters.ShouldSatisfyAllConditions(
            schema =>
            {
                schema.ShouldNotBeNull();

                var querySchema = schema.Properties["query"];
                querySchema.Properties.Keys.ShouldBe(new[]
                {
                    "fullName", "gradeFilters", "enrollmentStartDate", "enrollmentEndDate", "isActive"
                });
                querySchema.Properties["fullName"].Type.ShouldBe("string");
                querySchema.Properties["fullName"].Description.ShouldBe("The student's full name.");
                querySchema.Properties["gradeFilters"].Type.ShouldBe("array");
                querySchema.Properties["gradeFilters"].Description.ShouldBe("Grade filters for querying specific grades, e.g., Freshman or Senior.");
                querySchema.Properties["enrollmentStartDate"].Type.ShouldBe("string");
                querySchema.Properties["enrollmentStartDate"].Format.ShouldBe("date-time");
                querySchema.Properties["enrollmentStartDate"].Description.ShouldBe("The start date for the enrollment date range. ISO 8601 standard date");
                querySchema.Properties["enrollmentEndDate"].Type.ShouldBe("string");
                querySchema.Properties["enrollmentEndDate"].Format.ShouldBe("date-time");
                querySchema.Properties["enrollmentEndDate"].Description.ShouldBe("The end date for the enrollment date range. ISO 8601 standard date");
                querySchema.Properties["isActive"].Type.ShouldBe("boolean");
                querySchema.Properties["isActive"].Description.ShouldBe("The flag indicating whether to include only active students.");
            });
    }
    
    [Fact]
    public async Task ShouldInvokeWetherService()
    {
        Assert.SkipUnless(IsGoogleApiKeySet,GoogleTestSkipMessage);
        
        var func = (([Description("Request to query student record")] QueryStudentRecordRequest query) =>
        {
            return Task.FromResult(new StudentRecord
            {
                StudentId = "12345",
                FullName = query.FullName,
                Level = GradeLevel.Freshman,
                EnrolledCourses = new List<string> { "Math", "Physics", "Chemistry" },
                Grades = new Dictionary<string, double>
                {
                    { "Math", 95.0 },
                    { "Physics", 89.0 },
                    { "Chemistry", 88.0 }
                },
                EnrollmentDate = new DateTime(2023, 1, 10),
                IsActive = true
            });
        });
        
        var quickFt = new QuickTool(func, "GetStudentRecordAsync", "Return student record for the year");

        var tool = quickFt;
            
        var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.Gemini2Flash);
            
        model.AddFunctionTool(tool);

        var result = await model.GenerateContentAsync("How's Amit Rana is doing in Senior Grade? in enrollment year 01-01-2024 to 01-01-2025", cancellationToken: TestContext.Current.CancellationToken);
        
        result.Text().ShouldContain("Amit Rana",Case.Insensitive);
        Console.WriteLine(result.Text());
    }
    
    // [Fact]
    // public async Task ShouldUseMultipleQuickTools()
    // {
    //     Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);
    //
    //     var getStudentFunc = (([Description("Student Name")] string name) =>
    //     {
    //         return $"Student {name} is currently enrolled.";
    //     });
    //
    //     var getGradesFunc = (([Description("Student Name")] string name) =>
    //     {
    //         return new Dictionary<string, double>
    //         {
    //             { "Math", 95.0 },
    //             { "Physics", 88.0 }
    //         };
    //     });
    //
    //     var getAttendanceFunc = (([Description("Student Name")] string name) =>
    //     {
    //         return new { Present = 90, Total = 100 };
    //     });
    //
    //     var studentTool = new QuickTool(getStudentFunc, "GetStudentStatus", "Get student enrollment status");
    //     var gradesTool = new QuickTool(getGradesFunc, "GetGrades", "Get student grades");
    //     var attendanceTool = new QuickTool(getAttendanceFunc, "GetAttendance", "Get student attendance");
    //
    //     var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.Gemini2Flash);
    //
    //     model.AddFunctionTool(studentTool);
    //     model.AddFunctionTool(gradesTool);
    //     model.AddFunctionTool(attendanceTool);
    //
    //     var result = await model.GenerateContentAsync("What is John's enrollment status, grades and attendance?", 
    //         cancellationToken: TestContext.Current.CancellationToken);
    //
    //     result.Text().ShouldContain("John", Case.Insensitive);
    //     result.Text().ShouldContain("95.0", Case.Insensitive);
    //     result.Text().ShouldContain("90", Case.Insensitive);
    //     Console.WriteLine(result.Text());
    // }
    //
    //
    
    // [Fact]
    // public async Task ShouldUseMultipleQuickToolsStreaming()
    // {
    //     Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);
    //
    //     var getStudentFunc = (([Description("Student Name")] string name) =>
    //     {
    //         return $"Student {name} is currently enrolled.";
    //     });
    //
    //     var getGradesFunc = (([Description("Student Name")] string name) =>
    //     {
    //         return new Dictionary<string, double>
    //         {
    //             { "Math", 95.0 },
    //             { "Physics", 88.0 }
    //         };
    //     });
    //
    //     var getAttendanceFunc = (([Description("Student Name")] string name) =>
    //     {
    //         return new { Present = 90, Total = 100 };
    //     });
    //
    //     var studentTool = new QuickTool(getStudentFunc, "GetStudentStatus", "Get student enrollment status");
    //     var gradesTool = new QuickTool(getGradesFunc, "GetGrades", "Get student grades");
    //     var attendanceTool = new QuickTool(getAttendanceFunc, "GetAttendance", "Get student attendance");
    //
    //     var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.Gemini2Flash);
    //
    //     model.AddFunctionTool(studentTool);
    //     model.AddFunctionTool(gradesTool);
    //     model.AddFunctionTool(attendanceTool);
    //
    //     var responseText = new StringBuilder();
    //     await foreach (var response in model.StreamContentAsync(
    //         "What is John's enrollment status, grades and attendance?",
    //         cancellationToken: TestContext.Current.CancellationToken))
    //     {
    //         responseText.Append(response.Text());
    //         Console.WriteLine(response.Text());
    //     }
    //
    //     var finalResponse = responseText.ToString();
    //     finalResponse.ShouldContain("John", Case.Insensitive);
    //     finalResponse.ShouldContain("95", Case.Insensitive);
    //     finalResponse.ShouldContain("90", Case.Insensitive);
    // }
    //
    // [Fact]
    // public async Task ShouldReproduceIssue55_MultipleQuickToolsNotWorking()
    // {
    //     Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);
    //
    //     // Recreate the exact scenario from issue #55
    //     var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.Gemini2Flash);
    //
    //     model.AddFunctionTool(new QuickTool(
    //         () =>
    //         {
    //             // Simulate getting processes (safe for testing)
    //             var processes = new[] { "process1 PID:1234", "process2 PID:5678" };
    //             return string.Join("\n", processes);
    //         },
    //         name: "get_processes",
    //         description: "gets a list of opened processes"
    //     ));
    //
    //     model.AddFunctionTool(new QuickTool(
    //         ([Description("The process name to close")] string procName) =>
    //         {
    //             Console.WriteLine("Would close process: " + procName);
    //             return $"Would close process: {procName}";
    //         },
    //         name: "close_process", 
    //         description: "close a process by its process name"
    //     ));
    //
    //     // Test if both tools are accessible - prompt that encourages parallel function calling
    //     var result = await model.GenerateContentAsync(
    //         "I need you to call two functions: 1) get_processes to see what's running, and 2) close_process to close 'process1'. Please call both functions to complete this task.", 
    //         cancellationToken: TestContext.Current.CancellationToken);
    //
    //     var response = result.Text();
    //     Console.WriteLine(response);
    //     
    //     // Should contain evidence that both functions were called
    //     response.ShouldContain("process1", Case.Insensitive);
    //     response.ShouldContain("process2", Case.Insensitive);
    //     response.ShouldContain("close", Case.Insensitive);
    // }
    //
    // [Fact]
    // public async Task ShouldTestQuickToolsCollectionWorkaround()
    // {
    //     Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);
    //
    //     // Test the workaround using QuickTools collection
    //     var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.Gemini2Flash);
    //
    //     var getProcessesTool = new QuickTool(
    //         () =>
    //         {
    //             var processes = new[] { "process1 PID:1234", "process2 PID:5678" };
    //             return string.Join("\n", processes);
    //         },
    //         name: "get_processes",
    //         description: "gets a list of opened processes"
    //     );
    //
    //     var closeProcessTool = new QuickTool(
    //         ([Description("The process name to close")] string procName) =>
    //         {
    //             Console.WriteLine("Would close process: " + procName);
    //             return $"Would close process: {procName}";
    //         },
    //         name: "close_process",
    //         description: "close a process by its process name"
    //     );
    //
    //     // Use QuickTools collection instead of individual AddFunctionTool calls
    //     var quickTools = new QuickTools(new[] { getProcessesTool, closeProcessTool });
    //     model.AddFunctionTool(quickTools);
    //
    //     var result = await model.GenerateContentAsync(
    //         "Please get the list of currently running processes first, then close the process named 'process1'",
    //         cancellationToken: TestContext.Current.CancellationToken);
    //
    //     var response = result.Text();
    //     Console.WriteLine(response);
    //     
    //     // Should contain evidence that both functions were called
    //     response.ShouldContain("process1", Case.Insensitive);
    //     response.ShouldContain("process2", Case.Insensitive);
    //     response.ShouldContain("close", Case.Insensitive);
    // }
    
    [Fact]
    public void ShouldVerifyMultipleFunctionToolsAreAdded()
    {
        // Unit test to verify that multiple AddFunctionTool calls actually add tools to the list
        var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.Gemini2Flash);

        var tool1 = new QuickTool(
            () => "result1",
            name: "function1",
            description: "first function"
        );

        var tool2 = new QuickTool(
            () => "result2", 
            name: "function2",
            description: "second function"
        );

        model.AddFunctionTool(tool1);
        model.AddFunctionTool(tool2);

        // Verify both tools are in the collection
        model.FunctionTools.Count.ShouldBe(2);
        
        // Verify each tool contains the expected function
        model.FunctionTools[0].IsContainFunction("function1").ShouldBeTrue();
        model.FunctionTools[1].IsContainFunction("function2").ShouldBeTrue();
        
        // Verify tools don't contain each other's functions
        model.FunctionTools[0].IsContainFunction("function2").ShouldBeFalse();
        model.FunctionTools[1].IsContainFunction("function1").ShouldBeFalse();
    }
    
    [Fact]
    public async Task ShouldDiagnoseAvailableFunctions()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.Gemini2Flash);

        model.AddFunctionTool(new QuickTool(
            () => "Process list result",
            name: "get_processes",
            description: "gets a list of opened processes"
        ));

        model.AddFunctionTool(new QuickTool(
            ([Description("The process name to close")] string procName) => $"Closed {procName}",
            name: "close_process", 
            description: "close a process by its process name"
        ));

        // Ask the model to list all available functions
        var result = await model.GenerateContentAsync(
            "What functions can you call? Please list all available functions and their descriptions.", 
            cancellationToken: TestContext.Current.CancellationToken);

        var response = result.Text() ?? "NULL RESPONSE";
        Console.WriteLine("=== Available Functions Response ===");
        Console.WriteLine(response);
        Console.WriteLine("=== Function Tools Count ===");
        Console.WriteLine($"Model has {model.FunctionTools.Count} function tools");
        
        for (int i = 0; i < model.FunctionTools.Count; i++)
        {
            var tool = model.FunctionTools[i];
            Console.WriteLine($"Tool {i}: contains get_processes={tool.IsContainFunction("get_processes")}, contains close_process={tool.IsContainFunction("close_process")}");
        }
        
        // Check the actual tools being sent
        Console.WriteLine("=== Diagnostics Complete ===");
        
        // If response is not null, check for functions
        if (response != "NULL RESPONSE")
        {
            response.ShouldContain("get_processes", Case.Insensitive);
            response.ShouldContain("close_process", Case.Insensitive);
        }
    }
}