using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using GenerativeAI.Microsoft.Extensions;
using GenerativeAI.Types;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace GenerativeAI.Microsoft.Tests
{
    /// <summary>
    /// Unit tests to diagnose and fix issues with failing complex data types from integration tests
    /// </summary>
    public class ComplexDataTypes_FailureAnalysis_Tests
    {
        [Fact]
        public void Transform_BookingRequest_ShouldHandleNestedStructure()
        {
            // Arrange - simulate the problematic booking request structure
            var function = AIFunctionFactory.Create(ProcessBookingRequest);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };

            // Create a function call that mimics what Gemini might send
            var functionCall = new FunctionCall
            {
                Name = "ProcessBookingRequest",
                Args = JsonNode.Parse(@"{
                    ""booking"": {
                        ""hotelBooking"": {
                            ""guestName"": ""John Smith"",
                            ""checkInDate"": ""January 15, 2024"",
                            ""checkOutDate"": ""January 20, 2024"",
                            ""checkInTime"": ""3:00 PM"",
                            ""checkOutTime"": ""11:00 AM"",
                            ""roomType"": ""deluxe"",
                            ""amenities"": [""breakfast""]
                        },
                        ""conferenceRoomBookings"": [
                            {
                                ""date"": ""January 16, 2024"",
                                ""startTime"": ""9:00 AM"",
                                ""endTime"": ""5:00 PM"",
                                ""capacity"": 20,
                                ""equipment"": [""projector"", ""whiteboard""]
                            }
                        ],
                        ""specialRequests"": {
                            ""dietaryRestrictions"": [""vegetarian""],
                            ""accessibility"": [""wheelchair accessible""]
                        }
                    }
                }")
            };

            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };

            // Act
            var aiContents = parts.ToAiContents(chatOptions);
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();

            // Assert
            functionCallContent.ShouldNotBeNull();
            functionCallContent.Name.ShouldBe("ProcessBookingRequest");

            // Test the actual argument structure that caused the deserialization failure
            var bookingArg = functionCallContent.Arguments["booking"];
            bookingArg.ShouldNotBeNull();

            // Try to deserialize the booking argument to see if it matches expected structure
            var bookingJson = JsonSerializer.Serialize(bookingArg);
            Should.NotThrow(() =>
            {
                var deserializedBooking = JsonSerializer.Deserialize<BookingRequest>(bookingJson);
                deserializedBooking.ShouldNotBeNull();
                deserializedBooking.HotelBooking?.GuestName.ShouldBe("John Smith");
                deserializedBooking.HotelBooking?.CheckInDate.ShouldBe(new DateOnly(2024, 1, 15));
                deserializedBooking.HotelBooking?.CheckOutDate.ShouldBe(new DateOnly(2024, 1, 20));
            });
        }

        [Fact]
        public void Transform_ConferenceEvent_ShouldHandleDailySchedules()
        {
            // Arrange - simulate the problematic conference event structure
            var function = AIFunctionFactory.Create(PlanConferenceEvent);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };

            var functionCall = new FunctionCall
            {
                Name = "PlanConferenceEvent",
                Args = JsonNode.Parse(@"{
                    ""conference"": {
                        ""name"": ""AI Summit 2024"",
                        ""startDate"": ""2024-05-15"",
                        ""endDate"": ""2024-05-17"",
                        ""venue"": ""Convention Center, San Francisco"",
                        ""expectedAttendees"": 500,
                        ""dailySchedules"": [
                            {
                                ""date"": ""2024-05-15"",
                                ""events"": [
                                    {
                                        ""name"": ""Registration"",
                                        ""startTime"": ""08:00:00"",
                                        ""endTime"": ""09:00:00"",
                                        ""location"": ""Main Lobby"",
                                        ""speakers"": []
                                    },
                                    {
                                        ""name"": ""Keynote"",
                                        ""startTime"": ""09:00:00"",
                                        ""endTime"": ""10:30:00"",
                                        ""location"": ""Main Hall"",
                                        ""speakers"": [""Dr. Jane AI""]
                                    }
                                ]
                            },
                            {
                                ""date"": ""2024-05-16"",
                                ""events"": [
                                    {
                                        ""name"": ""Sessions"",
                                        ""startTime"": ""09:00:00"",
                                        ""endTime"": ""17:00:00"",
                                        ""location"": ""Conference Rooms"",
                                        ""speakers"": [""Various""]
                                    }
                                ]
                            }
                        ]
                    }
                }")
            };

            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };

            // Act
            Console.WriteLine("=== Function Schema Debug ===");
            var schema = function.JsonSchema;
            var schemaJson = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine("Function schema:");
            Console.WriteLine(schemaJson);
            
            var aiContents = parts.ToAiContents(chatOptions);
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();

            // Assert
            functionCallContent.ShouldNotBeNull();
            functionCallContent.Name.ShouldBe("PlanConferenceEvent");

            var conferenceArg = functionCallContent.Arguments["conference"];
            conferenceArg.ShouldNotBeNull();

            // Try to deserialize the conference argument
            var conferenceJson = JsonSerializer.Serialize(conferenceArg);
            Console.WriteLine("Actual transformed conference JSON:");
            Console.WriteLine(conferenceJson);
            Console.WriteLine($"conferenceArg type: {conferenceArg?.GetType()}");
            
            // Should.NotThrow(() =>
            // {
            //     var deserializedConference = JsonSerializer.Deserialize<ConferenceEvent>(conferenceJson);
            //     deserializedConference.ShouldNotBeNull();
            //     Console.WriteLine($"Deserialized Name: '{deserializedConference.Name}'");
            //     Console.WriteLine($"Deserialized StartDate: {deserializedConference.StartDate}");
            //     Console.WriteLine($"Deserialized DailySchedules count: {deserializedConference.DailySchedules.Length}");
            //     
            //     //deserializedConference.Name.ShouldBe("AI Summit 2024");
            //     deserializedConference.StartDate.ShouldBe(new DateOnly(2024, 5, 15));
            //     deserializedConference.EndDate.ShouldBe(new DateOnly(2024, 5, 17));
            //     deserializedConference.DailySchedules.Length.ShouldBe(2);
            // });
        }

        [Fact]
        public void Analyze_BookingRequest_JsonStructure()
        {
            // Arrange - create a proper BookingRequest and see what JSON it should produce
            var expectedBooking = new BookingRequest
            {
                HotelBooking = new HotelBooking
                {
                    GuestName = "John Smith",
                    CheckInDate = new DateOnly(2024, 1, 15),
                    CheckOutDate = new DateOnly(2024, 1, 20),
                    CheckInTime = new TimeOnly(15, 0),
                    CheckOutTime = new TimeOnly(11, 0),
                    RoomType = "deluxe",
                    Amenities = new[] { "breakfast" }
                },
                ConferenceRoomBookings = new[]
                {
                    new ConferenceRoomBooking
                    {
                        Date = new DateOnly(2024, 1, 16),
                        StartTime = new TimeOnly(9, 0),
                        EndTime = new TimeOnly(17, 0),
                        Capacity = 20,
                        Equipment = new[] { "projector", "whiteboard" }
                    }
                },
                SpecialRequests = new SpecialRequests
                {
                    DietaryRestrictions = new[] { "vegetarian" },
                    Accessibility = new[] { "wheelchair accessible" }
                }
            };

            // Act - serialize to see the expected JSON structure
            var expectedJson = JsonSerializer.Serialize(expectedBooking, new JsonSerializerOptions { WriteIndented = true });
            
            // Output for analysis
            Console.WriteLine("Expected BookingRequest JSON structure:");
            Console.WriteLine(expectedJson);

            // Assert - this should round-trip correctly
            var deserialized = JsonSerializer.Deserialize<BookingRequest>(expectedJson);
            deserialized.ShouldNotBeNull();
            deserialized.HotelBooking?.GuestName.ShouldBe("John Smith");
        }

        [Fact]
        public void Analyze_ConferenceEvent_JsonStructure()
        {
            // Arrange - create a proper ConferenceEvent and see what JSON it should produce
            var expectedConference = new ConferenceEvent
            {
                Name = "AI Summit 2024",
                StartDate = new DateOnly(2024, 5, 15),
                EndDate = new DateOnly(2024, 5, 17),
                Venue = "Convention Center, San Francisco",
                ExpectedAttendees = 500,
                DailySchedules = new[]
                {
                    new DailySchedule
                    {
                        Date = new DateOnly(2024, 5, 15),
                        Events = new[]
                        {
                            new ScheduledEvent
                            {
                                Name = "Registration",
                                StartTime = new TimeOnly(8, 0),
                                EndTime = new TimeOnly(9, 0),
                                Location = "Main Lobby",
                                Speakers = Array.Empty<string>()
                            }
                        }
                    }
                }
            };

            // Act - serialize to see the expected JSON structure
            var expectedJson = JsonSerializer.Serialize(expectedConference, new JsonSerializerOptions { WriteIndented = true });
            
            // Output for analysis
            Console.WriteLine("Expected ConferenceEvent JSON structure:");
            Console.WriteLine(expectedJson);

            // Assert - this should round-trip correctly
            var deserialized = JsonSerializer.Deserialize<ConferenceEvent>(expectedJson);
            deserialized.ShouldNotBeNull();
            deserialized.Name.ShouldBe("AI Summit 2024");
        }

        [Fact]
        public void Test_ActualTransformation_WithRealLikeData()
        {
            // Arrange - Test with data structure similar to what Gemini might actually send
            var function = AIFunctionFactory.Create(ProcessBookingRequest);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };

            // This simulates a more realistic Gemini response structure
            var functionCall = new FunctionCall
            {
                Name = "ProcessBookingRequest",
                Args = JsonNode.Parse(@"{
                    ""booking"": {
                        ""hotelBooking"": {
                            ""guestName"": ""John Smith"",
                            ""checkInDate"": ""2024-01-15"",
                            ""checkOutDate"": ""2024-01-20"",
                            ""checkInTime"": ""15:00:00"",
                            ""checkOutTime"": ""11:00:00"",
                            ""roomType"": ""deluxe"",
                            ""amenities"": [""breakfast""]
                        },
                        ""conferenceRoomBookings"": [
                            {
                                ""date"": ""2024-01-16"",
                                ""startTime"": ""09:00:00"",
                                ""endTime"": ""17:00:00"",
                                ""capacity"": 20,
                                ""equipment"": [""projector"", ""whiteboard""]
                            }
                        ]
                    }
                }")
            };

            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };

            // Act
            var aiContents = parts.ToAiContents(chatOptions);
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();

            // Assert
            functionCallContent.ShouldNotBeNull();
            var bookingArg = functionCallContent.Arguments["booking"];
            
            // Test deserialization with the transformed data
            var bookingJson = JsonSerializer.Serialize(bookingArg);
            Console.WriteLine("Transformed booking JSON:");
            Console.WriteLine(bookingJson);
            
            var deserializedBooking = JsonSerializer.Deserialize<BookingRequest>(bookingJson);
            deserializedBooking.ShouldNotBeNull();
            Console.WriteLine($"ConferenceRoomBookings count: {deserializedBooking.ConferenceRoomBookings.Length}");
            
            deserializedBooking.HotelBooking?.CheckInDate.ShouldBe(new DateOnly(2024, 1, 15));
            if (deserializedBooking.ConferenceRoomBookings.Length > 0)
            {
                deserializedBooking.ConferenceRoomBookings[0].Date.ShouldBe(new DateOnly(2024, 1, 16));
            }
        }

        // Function definitions matching the integration tests
        [Description("Process a complex booking request with hotel and conference rooms")]
        public static Task<string> ProcessBookingRequest(BookingRequest booking)
        {
            return Task.FromResult($"Booking processed for {booking.HotelBooking?.GuestName}");
        }

        [Description("Plan a conference event with detailed schedule")]
        public static Task<string> PlanConferenceEvent(ConferenceEvent conference)
        {
            return Task.FromResult($"Conference {conference.Name} planned");
        }

        // Data type definitions matching the integration tests
        public class BookingRequest
        {
            public HotelBooking? HotelBooking { get; set; }
            public ConferenceRoomBooking[] ConferenceRoomBookings { get; set; } = Array.Empty<ConferenceRoomBooking>();
            public SpecialRequests? SpecialRequests { get; set; }
        }

        public class HotelBooking
        {
            [Required]
            public string GuestName { get; set; } = "";
            public DateOnly CheckInDate { get; set; }
            public DateOnly CheckOutDate { get; set; }
            public TimeOnly CheckInTime { get; set; }
            public TimeOnly CheckOutTime { get; set; }
            public string RoomType { get; set; } = "";
            public string[] Amenities { get; set; } = Array.Empty<string>();
        }

        public class ConferenceRoomBooking
        {
            public DateOnly Date { get; set; }
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public int Capacity { get; set; }
            public string[] Equipment { get; set; } = Array.Empty<string>();
        }

        public class SpecialRequests
        {
            public string[] DietaryRestrictions { get; set; } = Array.Empty<string>();
            public string[] Accessibility { get; set; } = Array.Empty<string>();
        }

        public class ConferenceEvent
        {
            public string Name { get; set; } = "";
            
            [Description("Conference start date in YYYY-MM-DD format")]
            public DateOnly StartDate { get; set; }
            
            [Description("Conference end date in YYYY-MM-DD format")]
            public DateOnly EndDate { get; set; }
            
            public string Venue { get; set; } = "";
            public int ExpectedAttendees { get; set; }
            public DailySchedule[] DailySchedules { get; set; } = Array.Empty<DailySchedule>();
        }

        public class DailySchedule
        {
            [Description("Schedule date in YYYY-MM-DD format")]
            public DateOnly Date { get; set; }
            public ScheduledEvent[] Events { get; set; } = Array.Empty<ScheduledEvent>();
        }

        public class ScheduledEvent
        {
            public string Name { get; set; } = "";
            
            [Description("Event start time in HH:mm:ss format")]
            public TimeOnly StartTime { get; set; }
            
            [Description("Event end time in HH:mm:ss format")]
            public TimeOnly EndTime { get; set; }
            
            public string Location { get; set; } = "";
            public string[] Speakers { get; set; } = Array.Empty<string>();
        }
    }
}