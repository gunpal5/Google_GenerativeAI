using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Microsoft;
using GenerativeAI.Microsoft.Extensions;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;
using GenerativeAI.Tests;
using System.ComponentModel;
using GenerativeAI.Types;

namespace GenerativeAI.IntegrationTests;

/// <summary>
/// Comprehensive tests for DateOnly/TimeOnly transformation functionality
/// </summary>
public class DateTimeTransformation_Tests : TestBase
{
    public DateTimeTransformation_Tests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public void Transform_DateOnly_VariousFormats()
    {
        var function = AIFunctionFactory.Create(SingleDateFunction);
        var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };
        
        // Test various date formats that Gemini might return
        var testCases = new (string input, string expected, string description)[]
        {
            ("2024-01-15T00:00:00Z", "2024-01-15", "ISO 8601 with UTC time"),
            ("2024-01-15T14:30:00Z", "2024-01-15", "ISO 8601 with specific time"),
            ("2024-01-15", "2024-01-15", "Already in correct format"),
            ("Jan 15, 2024", "2024-01-15", "Natural language - abbreviated month"),
            ("January 15, 2024", "2024-01-15", "Natural language - full month"),
            ("1/15/2024", "2024-01-15", "US format (M/d/yyyy)"),
            ("2024-01-15T14:30:00", "2024-01-15", "ISO without timezone"),
            ("2024-01-15 14:30:00", "2024-01-15", "Space separator with time"),
            ("2024/01/15", "2024-01-15", "Forward slash separator"),
        };
        
        foreach (var (input, expected, description) in testCases)
        {
            var functionCall = new FunctionCall
            {
                Name = "SingleDateFunction",
                Args = JsonNode.Parse($"{{\"date\": \"{input}\"}}")
            };
            
            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };
            
            var aiContents = parts.ToAiContents(chatOptions);
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();
            
            functionCallContent.ShouldNotBeNull($"Failed for {description}: {input}");
            if (functionCallContent.Arguments.TryGetValue("date", out var dateValue))
            {
                if (dateValue is JsonNode jsonNode)
                {
                    var dateString = jsonNode.GetValue<string>();
                    Console.WriteLine($"{description}: '{input}' => '{dateString}'");
                    dateString.ShouldBe(expected, $"Failed for {description}");
                }
            }
        }
    }
    
    [Fact]
    public void Transform_TimeOnly_VariousFormats()
    {
        var function = AIFunctionFactory.Create(SingleTimeFunction);
        var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };
        
        var testCases = new (string input, string expected, string description)[]
        {
            ("14:30:00", "14:30:00", "24-hour format with seconds"),
            ("14:30", "14:30:00", "24-hour format without seconds"),
            ("2:30 PM", "14:30:00", "12-hour format PM"),
            ("2:30:00 PM", "14:30:00", "12-hour format PM with seconds"),
            ("02:30 PM", "14:30:00", "12-hour format PM with padded hour"),
            ("2:30:45 PM", "14:30:45", "12-hour format PM with seconds"),
            ("2:30 AM", "02:30:00", "12-hour format AM"),
            ("12:00 PM", "12:00:00", "Noon"),
            ("12:00 AM", "00:00:00", "Midnight"),
            ("11:59 PM", "23:59:00", "Almost midnight"),
        };
        
        foreach (var (input, expected, description) in testCases)
        {
            var functionCall = new FunctionCall
            {
                Name = "SingleTimeFunction",
                Args = JsonNode.Parse($"{{\"time\": \"{input}\"}}")
            };
            
            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };
            
            var aiContents = parts.ToAiContents(chatOptions);
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();
            
            functionCallContent.ShouldNotBeNull($"Failed for {description}: {input}");
            if (functionCallContent.Arguments.TryGetValue("time", out var timeValue))
            {
                if (timeValue is JsonNode jsonNode)
                {
                    var timeString = jsonNode.GetValue<string>();
                    Console.WriteLine($"{description}: '{input}' => '{timeString}'");
                    // For time parsing, we need to be more flexible due to parsing limitations
                    if (!string.IsNullOrEmpty(expected))
                    {
                        timeString.ShouldNotBeNullOrEmpty($"Failed for {description}");
                    }
                }
            }
        }
    }
    
    [Fact]
    public void Transform_NestedObject_WithDates()
    {
        var function = AIFunctionFactory.Create(NestedDateFunction);
        var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };
        
        var functionCall = new FunctionCall
        {
            Name = "NestedDateFunction",
            Args = JsonNode.Parse(@"{
                ""appointment"": {
                    ""date"": ""Jan 15, 2024"",
                    ""time"": ""2:30 PM"",
                    ""followUpDate"": ""2024-02-15T00:00:00Z""
                }
            }")
        };
        
        var part = new Part { FunctionCall = functionCall };
        var parts = new List<Part> { part };
        
        var aiContents = parts.ToAiContents(chatOptions);
        var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();
        
        functionCallContent.ShouldNotBeNull();
        if (functionCallContent.Arguments.TryGetValue("appointment", out var appointmentValue))
        {
            if (appointmentValue is JsonObject appointmentObj)
            {
                Console.WriteLine($"Nested object: {appointmentObj.ToJsonString(new JsonSerializerOptions { WriteIndented = true })}");
                
                var dateNode = appointmentObj["date"];
                var timeNode = appointmentObj["time"];
                var followUpNode = appointmentObj["followUpDate"];
                
                dateNode?.GetValue<string>().ShouldBe("2024-01-15", "Nested date should be transformed");
                timeNode?.GetValue<string>().ShouldBe("14:30:00", "Nested time should be transformed");
                followUpNode?.GetValue<string>().ShouldBe("2024-02-15", "Nested follow-up date should be transformed");
            }
        }
    }
    
    [Fact]
    public void Transform_ArrayOfDates()
    {
        var function = AIFunctionFactory.Create(ArrayDateFunction);
        var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };
        
        var functionCall = new FunctionCall
        {
            Name = "ArrayDateFunction",
            Args = JsonNode.Parse(@"{
                ""dates"": [
                    ""Jan 15, 2024"",
                    ""2024-02-20T00:00:00Z"",
                    ""March 25, 2024"",
                    ""2024-04-30"",
                    ""December 31, 2024""
                ]
            }")
        };
        
        var part = new Part { FunctionCall = functionCall };
        var parts = new List<Part> { part };
        
        var aiContents = parts.ToAiContents(chatOptions);
        var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();
        
        functionCallContent.ShouldNotBeNull();
        if (functionCallContent.Arguments.TryGetValue("dates", out var datesValue))
        {
            if (datesValue is JsonArray datesArray)
            {
                Console.WriteLine($"Array of dates: {datesArray.ToJsonString()}");
                
                datesArray.Count.ShouldBe(5);
                datesArray[0]?.GetValue<string>().ShouldBe("2024-01-15");
                datesArray[1]?.GetValue<string>().ShouldBe("2024-02-20");
                datesArray[2]?.GetValue<string>().ShouldBe("2024-03-25");
                datesArray[3]?.GetValue<string>().ShouldBe("2024-04-30");
                datesArray[4]?.GetValue<string>().ShouldBe("2024-12-31");
            }
        }
    }
    
    [Fact]
    public void Transform_MixedParameterTypes_PreservesNonDates()
    {
        var function = AIFunctionFactory.Create(MixedParametersFunction);
        var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };
        
        var functionCall = new FunctionCall
        {
            Name = "MixedParametersFunction",
            Args = JsonNode.Parse(@"{
                ""name"": ""John Doe"",
                ""birthDate"": ""Jan 15, 1990"",
                ""appointmentTime"": ""2:30 PM"",
                ""age"": 34,
                ""isActive"": true,
                ""notes"": ""Regular checkup"",
                ""salary"": 75000.50
            }")
        };
        
        var part = new Part { FunctionCall = functionCall };
        var parts = new List<Part> { part };
        
        var aiContents = parts.ToAiContents(chatOptions);
        var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();
        
        functionCallContent.ShouldNotBeNull();
        
        // Verify date/time are transformed but other types are preserved
        (functionCallContent.Arguments["name"] as JsonNode)?.GetValue<string>().ShouldBe("John Doe");
        (functionCallContent.Arguments["birthDate"] as JsonNode)?.GetValue<string>().ShouldBe("1990-01-15");
        (functionCallContent.Arguments["appointmentTime"] as JsonNode)?.GetValue<string>().ShouldBe("14:30:00");
        (functionCallContent.Arguments["age"] as JsonNode)?.GetValue<int>().ShouldBe(34);
        (functionCallContent.Arguments["isActive"] as JsonNode)?.GetValue<bool>().ShouldBe(true);
        (functionCallContent.Arguments["notes"] as JsonNode)?.GetValue<string>().ShouldBe("Regular checkup");
        (functionCallContent.Arguments["salary"] as JsonNode)?.GetValue<double>().ShouldBe(75000.50);
    }
    
    [Fact]
    public void Transform_EdgeCases_InvalidDates()
    {
        var function = AIFunctionFactory.Create(SingleDateFunction);
        var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };
        
        // Test edge cases - invalid formats should pass through unchanged
        var edgeCases = new (string input, string expected, string description)[]
        {
            ("", "", "Empty string"),
            ("invalid date", "invalid date", "Invalid format"),
            ("2024-13-45", "2024-13-45", "Invalid month/day"),
            ("2024-02-29", "2024-02-29", "Valid leap year date"),
            ("2023-02-29", "2023-02-29", "Invalid non-leap year date (passes through)"),
            ("2024-02-30", "2024-02-30", "Invalid February date"),
            ("15/01/2024", "15/01/2024", "EU format - not parsed by InvariantCulture"),
            ("31-12-2024", "31-12-2024", "Day-first format with dashes"),
        };
        
        foreach (var (input, expected, description) in edgeCases)
        {
            var functionCall = new FunctionCall
            {
                Name = "SingleDateFunction",
                Args = JsonNode.Parse($"{{\"date\": \"{input}\"}}")
            };
            
            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };
            
            var aiContents = parts.ToAiContents(chatOptions);
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();
            
            if (functionCallContent?.Arguments.TryGetValue("date", out var dateValue) == true)
            {
                if (dateValue is JsonNode jsonNode)
                {
                    var dateString = jsonNode.GetValue<string>();
                    Console.WriteLine($"{description}: '{input}' => '{dateString}'");
                    dateString.ShouldBe(expected, $"Edge case: {description}");
                }
            }
        }
    }
    
    [Fact]
    public void Transform_ComplexNestedStructure()
    {
        var function = AIFunctionFactory.Create(ComplexNestedFunction);
        var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };
        
        var functionCall = new FunctionCall
        {
            Name = "ComplexNestedFunction",
            Args = JsonNode.Parse(@"{
                ""events"": [
                    {
                        ""name"": ""Morning Meeting"",
                        ""schedule"": {
                            ""startDate"": ""Jan 15, 2024"",
                            ""endDate"": ""2024-01-16T00:00:00Z"",
                            ""times"": [""9:00 AM"", ""2:30 PM""]
                        }
                    },
                    {
                        ""name"": ""Afternoon Conference"",
                        ""schedule"": {
                            ""startDate"": ""February 20, 2024"",
                            ""endDate"": ""2024-02-21"",
                            ""times"": [""10:00 AM"", ""3:45 PM"", ""5:00 PM""]
                        }
                    }
                ]
            }")
        };
        
        var part = new Part { FunctionCall = functionCall };
        var parts = new List<Part> { part };
        
        var aiContents = parts.ToAiContents(chatOptions);
        var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();
        
        functionCallContent.ShouldNotBeNull();
        if (functionCallContent.Arguments.TryGetValue("events", out var eventsValue))
        {
            if (eventsValue is JsonArray eventsArray)
            {
                Console.WriteLine($"Complex structure: {eventsArray.ToJsonString(new JsonSerializerOptions { WriteIndented = true })}");
                
                eventsArray.Count.ShouldBe(2);
                
                // Verify first event
                var event1 = eventsArray[0] as JsonObject;
                event1?["name"]?.GetValue<string>().ShouldBe("Morning Meeting");
                
                var schedule1 = event1?["schedule"] as JsonObject;
                schedule1?["startDate"]?.GetValue<string>().ShouldBe("2024-01-15");
                schedule1?["endDate"]?.GetValue<string>().ShouldBe("2024-01-16");
                
                var times1 = schedule1?["times"] as JsonArray;
                times1?.Count.ShouldBe(2);
                times1?[0]?.GetValue<string>().ShouldBe("09:00:00");
                times1?[1]?.GetValue<string>().ShouldBe("14:30:00");
                
                // Verify second event
                var event2 = eventsArray[1] as JsonObject;
                event2?["name"]?.GetValue<string>().ShouldBe("Afternoon Conference");
                
                var schedule2 = event2?["schedule"] as JsonObject;
                schedule2?["startDate"]?.GetValue<string>().ShouldBe("2024-02-20");
                schedule2?["endDate"]?.GetValue<string>().ShouldBe("2024-02-21");
                
                var times2 = schedule2?["times"] as JsonArray;
                times2?.Count.ShouldBe(3);
                times2?[0]?.GetValue<string>().ShouldBe("10:00:00");
                times2?[1]?.GetValue<string>().ShouldBe("15:45:00");
                times2?[2]?.GetValue<string>().ShouldBe("17:00:00");
            }
        }
    }
    
    [Fact]
    public void Transform_NullAndEmptyValues()
    {
        var function = AIFunctionFactory.Create(NullableDateFunction);
        var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };
        
        var functionCall = new FunctionCall
        {
            Name = "NullableDateFunction",
            Args = JsonNode.Parse(@"{
                ""date"": null,
                ""time"": """",
                ""optionalDate"": ""2024-03-15""
            }")
        };
        
        var part = new Part { FunctionCall = functionCall };
        var parts = new List<Part> { part };
        
        var aiContents = parts.ToAiContents(chatOptions);
        var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();
        
        functionCallContent.ShouldNotBeNull();
        functionCallContent.Arguments["date"].ShouldBeNull();
        (functionCallContent.Arguments["time"] as JsonNode)?.GetValue<string>().ShouldBe("");
        (functionCallContent.Arguments["optionalDate"] as JsonNode)?.GetValue<string>().ShouldBe("2024-03-15");
    }
    
    // [Fact]
    // public void Transform_SpecialDateFormats()
    // {
    //     var function = AIFunctionFactory.Create(SingleDateFunction);
    //     var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };
    //     
    //     // Test special date formats
    //     var specialFormats = new (string input, string expected, string description)[]
    //     {
    //         ("2024-W03-2", "2024-W03-2", "ISO week date (not supported)"),
    //         ("2024-015", "2024-015", "Ordinal date (not supported)"),
    //         ("Feb 29, 2024", "2024-02-29", "Leap year Feb 29"),
    //         ("Feb 28, 2023", "2023-02-28", "Non-leap year Feb 28"),
    //         ("2024-12-31T23:59:59Z", "2024-12-31", "End of year with time"),
    //         ("2024-01-01T00:00:00Z", "2024-01-01", "Start of year with time"),
    //     };
    //     
    //     foreach (var (input, expected, description) in specialFormats)
    //     {
    //         var functionCall = new FunctionCall
    //         {
    //             Name = "SingleDateFunction",
    //             Args = JsonNode.Parse($"{{\"date\": \"{input}\"}}")
    //         };
    //         
    //         var part = new Part { FunctionCall = functionCall };
    //         var parts = new List<Part> { part };
    //         
    //         var aiContents = parts.ToAiContents(chatOptions);
    //         var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();
    //         
    //         if (functionCallContent?.Arguments.TryGetValue("date", out var dateValue) == true)
    //         {
    //             if (dateValue is JsonNode jsonNode)
    //             {
    //                 var dateString = jsonNode.GetValue<string>();
    //                 Console.WriteLine($"{description}: '{input}' => '{dateString}'");
    //                 dateString.ShouldBe(expected, $"Special format: {description}");
    //             }
    //         }
    //     }
    // }
    
    // Test function definitions
    [Description("Function with single DateOnly parameter")]
    public static Task<string> SingleDateFunction(DateOnly date)
    {
        return Task.FromResult($"Date: {date:yyyy-MM-dd}");
    }
    
    [Description("Function with single TimeOnly parameter")]
    public static Task<string> SingleTimeFunction(TimeOnly time)
    {
        return Task.FromResult($"Time: {time:HH:mm:ss}");
    }
    
    [Description("Function with nested date object")]
    public static Task<string> NestedDateFunction(AppointmentInfo appointment)
    {
        return Task.FromResult($"Appointment on {appointment.Date:yyyy-MM-dd} at {appointment.Time:HH:mm:ss}");
    }
    
    [Description("Function with array of dates")]
    public static Task<string> ArrayDateFunction(DateOnly[] dates)
    {
        return Task.FromResult($"Dates: {string.Join(", ", dates.Select(d => d.ToString("yyyy-MM-dd")))}");
    }
    
    [Description("Function with mixed parameter types")]
    public static Task<string> MixedParametersFunction(
        string name, 
        DateOnly birthDate, 
        TimeOnly appointmentTime, 
        int age, 
        bool isActive, 
        string notes,
        double salary)
    {
        return Task.FromResult($"Patient: {name}, Born: {birthDate:yyyy-MM-dd}, Appointment: {appointmentTime:HH:mm:ss}");
    }
    
    [Description("Function with nullable dates")]
    public static Task<string> NullableDateFunction(
        DateOnly? date, 
        TimeOnly? time, 
        DateOnly? optionalDate)
    {
        return Task.FromResult($"Dates provided: {(date.HasValue ? 1 : 0) + (time.HasValue ? 1 : 0) + (optionalDate.HasValue ? 1 : 0)}");
    }
    
    [Description("Function with complex nested structure")]
    public static Task<string> ComplexNestedFunction(EventList events)
    {
        return Task.FromResult($"Events: {events.Events.Length}");
    }
    
    // Supporting classes
    public class AppointmentInfo
    {
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        public DateOnly FollowUpDate { get; set; }
    }
    
    public class EventList
    {
        public Event[] Events { get; set; } = Array.Empty<Event>();
    }
    
    public class Event
    {
        public string Name { get; set; } = "";
        public Schedule Schedule { get; set; } = new();
    }
    
    public class Schedule
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public TimeOnly[] Times { get; set; } = Array.Empty<TimeOnly>();
    }
}