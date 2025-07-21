using System.ComponentModel;
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
        Assert.SkipUnless(IsGeminiApiKeySet,GeminiTestSkipMessage);
        
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
}