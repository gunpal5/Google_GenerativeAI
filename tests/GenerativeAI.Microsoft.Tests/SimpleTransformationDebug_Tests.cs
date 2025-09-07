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
    /// Simple debug test to understand what's happening with string property transformation
    /// </summary>
    public class SimpleTransformationDebug_Tests
    {
        [Fact]
        public void Debug_SimpleStringProperty_Transformation()
        {
            // Arrange - Create a simple case with just name and venue (no dates)
            var function = AIFunctionFactory.Create(SimpleMethod);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };

            var functionCall = new FunctionCall
            {
                Name = "SimpleMethod",
                Args = JsonNode.Parse(@"{
                    ""event"": {
                        ""name"": ""Test Event"",
                        ""venue"": ""Test Venue"",
                        ""capacity"": 100
                    }
                }")
            };

            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };

            // Act
            var aiContents = parts.ToAiContents(chatOptions);
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();

            // Output for debugging
            if (functionCallContent != null)
            {
                var eventArg = functionCallContent.Arguments["event"];
                var eventJson = JsonSerializer.Serialize(eventArg, new JsonSerializerOptions { WriteIndented = true });
                
                Console.WriteLine("=== Simple Event Original ===");
                Console.WriteLine(@"{""event"": {""name"": ""Test Event"", ""venue"": ""Test Venue"", ""capacity"": 100}}");
                
                Console.WriteLine("\n=== Simple Event Transformed ===");
                Console.WriteLine(eventJson);

                // Try deserialization
                var deserialized = JsonSerializer.Deserialize<SimpleEvent>(eventJson);
                Console.WriteLine($"\n=== Simple Event Deserialized ===");
                Console.WriteLine($"Name: '{deserialized?.Name ?? "NULL"}'");
                Console.WriteLine($"Venue: '{deserialized?.Venue ?? "NULL"}'");
                Console.WriteLine($"Capacity: {deserialized?.Capacity}");
            }
        }

        [Fact]
        public void Debug_WithOneDateProperty_Transformation()
        {
            // Arrange - Add one date to see if it affects other properties
            var function = AIFunctionFactory.Create(EventWithDate);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };

            var functionCall = new FunctionCall
            {
                Name = "EventWithDate",
                Args = JsonNode.Parse(@"{
                    ""event"": {
                        ""name"": ""Test Event"",
                        ""venue"": ""Test Venue"",
                        ""eventDate"": ""2024-05-15"",
                        ""capacity"": 100
                    }
                }")
            };

            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };

            // Act
            var aiContents = parts.ToAiContents(chatOptions);
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();

            // Output for debugging
            if (functionCallContent != null)
            {
                var eventArg = functionCallContent.Arguments["event"];
                var eventJson = JsonSerializer.Serialize(eventArg, new JsonSerializerOptions { WriteIndented = true });
                
                Console.WriteLine("=== Event With Date Original ===");
                Console.WriteLine(@"{""event"": {""name"": ""Test Event"", ""venue"": ""Test Venue"", ""eventDate"": ""2024-05-15"", ""capacity"": 100}}");
                
                Console.WriteLine("\n=== Event With Date Transformed ===");
                Console.WriteLine(eventJson);

                // Try deserialization
                var deserialized = JsonSerializer.Deserialize<EventWithDateClass>(eventJson);
                Console.WriteLine($"\n=== Event With Date Deserialized ===");
                Console.WriteLine($"Name: '{deserialized?.Name ?? "NULL"}'");
                Console.WriteLine($"Venue: '{deserialized?.Venue ?? "NULL"}'");
                Console.WriteLine($"EventDate: {deserialized?.EventDate}");
                Console.WriteLine($"Capacity: {deserialized?.Capacity}");
            }
        }

        // Simple test methods
        [Description("Test simple event")]
        public static Task<string> SimpleMethod(SimpleEvent @event)
        {
            return Task.FromResult("OK");
        }

        [Description("Test event with date")]
        public static Task<string> EventWithDate(EventWithDateClass @event)
        {
            return Task.FromResult("OK");
        }

        // Simple data classes
        public class SimpleEvent
        {
            public string Name { get; set; } = "";
            public string Venue { get; set; } = "";
            public int Capacity { get; set; }
        }

        public class EventWithDateClass
        {
            public string Name { get; set; } = "";
            public string Venue { get; set; } = "";
            public DateOnly EventDate { get; set; }
            public int Capacity { get; set; }
        }
    }
}