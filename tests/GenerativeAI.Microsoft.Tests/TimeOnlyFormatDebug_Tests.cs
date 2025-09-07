using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using GenerativeAI.Microsoft.Extensions;
using GenerativeAI.Types;
using Microsoft.Extensions.AI;
using Xunit;

namespace GenerativeAI.Microsoft.Tests
{
    /// <summary>
    /// Debug tests to understand TimeOnly format issues from real API calls
    /// </summary>
    public class TimeOnlyFormatDebug_Tests
    {
        [Fact]
        public void Debug_TimeOnlyFormat_VariousInputs()
        {
            // Test various TimeOnly formats that Gemini might return
            var testFormats = new[]
            {
                "3:00 PM",
                "15:00",
                "15:00:00",
                "3:00:00 PM",
                "03:00 PM",
                "03:00:00 PM",
                "15:00:00.000",
                "T15:00:00",
                "11:00 AM",
                "11:00:00 AM",
                "23:00:00"
            };

            Console.WriteLine("=== TimeOnly Format Testing ===");
            
            foreach (var format in testFormats)
            {
                try
                {
                    // Test direct TimeOnly parsing
                    var timeOnly = TimeOnly.Parse(format);
                    Console.WriteLine($"✅ Direct Parse: '{format}' -> {timeOnly}");
                    
                    // Test JSON deserialization
                    var jsonString = $"{{\"time\": \"{format}\"}}";
                    var obj = JsonSerializer.Deserialize<TimeContainer>(jsonString);
                    Console.WriteLine($"✅ JSON Parse: '{format}' -> {obj?.Time}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed: '{format}' -> {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        [Fact]
        public void Debug_TransformTimeOnlyValues()
        {
            // Test our transformer with various TimeOnly formats
            var function = AIFunctionFactory.Create(TestTimeMethod);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };

            var testCases = new[]
            {
                "15:00",
                "15:00:00",
                "3:00 PM",
                "03:00:00 PM"
            };

            foreach (var timeFormat in testCases)
            {
                Console.WriteLine($"\n=== Testing transformation for: '{timeFormat}' ===");
                
                var functionCall = new FunctionCall
                {
                    Name = "TestTimeMethod",
                    Args = JsonNode.Parse($@"{{
                        ""time"": ""{timeFormat}""
                    }}")
                };

                var part = new Part { FunctionCall = functionCall };
                var parts = new List<Part> { part };

                try
                {
                    var aiContents = parts.ToAiContents(chatOptions);
                    var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();

                    if (functionCallContent != null)
                    {
                        var timeArg = functionCallContent.Arguments["time"];
                        var timeJson = JsonSerializer.Serialize(timeArg);
                        
                        Console.WriteLine($"Original: '{timeFormat}'");
                        Console.WriteLine($"Transformed: {timeJson}");
                        
                        // Try deserializing the transformed value
                        var container = JsonSerializer.Deserialize<TimeContainer>($"{{\"time\": {timeJson}}}");
                        Console.WriteLine($"Final TimeOnly: {container?.Time}");
                        Console.WriteLine("✅ Success");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        [Fact]
        public void Debug_ComplexBookingTimeFormat()
        {
            // Test the exact scenario from the failing booking test
            var function = AIFunctionFactory.Create(TestBookingMethod);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };

            // Simulate what Gemini might return for booking times
            var functionCall = new FunctionCall
            {
                Name = "TestBookingMethod",
                Args = JsonNode.Parse(@"{
                    ""booking"": {
                        ""checkInTime"": ""3:00 PM"",
                        ""checkOutTime"": ""11:00 AM""
                    }
                }")
            };

            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };

            try
            {
                var aiContents = parts.ToAiContents(chatOptions);
                var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();

                if (functionCallContent != null)
                {
                    var bookingArg = functionCallContent.Arguments["booking"];
                    var bookingJson = JsonSerializer.Serialize(bookingArg, new JsonSerializerOptions { WriteIndented = true });
                    
                    Console.WriteLine("=== Transformed Booking JSON ===");
                    Console.WriteLine(bookingJson);
                    
                    // Try deserializing
                    var booking = JsonSerializer.Deserialize<TestBooking>(bookingJson);
                    Console.WriteLine($"✅ CheckInTime: {booking?.CheckInTime}");
                    Console.WriteLine($"✅ CheckOutTime: {booking?.CheckOutTime}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Booking transformation failed: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                }
            }
        }

        // Test methods
        [Description("Test time method")]
        public static Task<string> TestTimeMethod(TimeOnly time)
        {
            return Task.FromResult($"Time received: {time}");
        }

        [Description("Test booking method")]
        public static Task<string> TestBookingMethod(TestBooking booking)
        {
            return Task.FromResult($"Booking received");
        }

        // Test classes
        public class TimeContainer
        {
            public TimeOnly Time { get; set; }
        }

        public class TestBooking
        {
            public TimeOnly CheckInTime { get; set; }
            public TimeOnly CheckOutTime { get; set; }
        }
    }
}