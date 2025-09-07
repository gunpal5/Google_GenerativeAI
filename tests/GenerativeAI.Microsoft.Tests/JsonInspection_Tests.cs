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
    /// Tests to inspect JSON structures and understand transformation issues
    /// </summary>
    public class JsonInspection_Tests
    {
        [Fact]
        public void Inspect_ConferenceEvent_Transformation()
        {
            // Arrange
            var function = AIFunctionFactory.Create(PlanConferenceEvent);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };

            var functionCall = new FunctionCall
            {
                Name = "PlanConferenceEvent",
                Args = JsonNode.Parse(@"{
                    ""conference"": {
                        ""name"": ""AI Summit 2024"",
                        ""startDate"": ""May 15, 2024"",
                        ""endDate"": ""May 17, 2024"",
                        ""venue"": ""Convention Center"",
                        ""expectedAttendees"": 500,
                        ""dailySchedules"": [
                            {
                                ""date"": ""May 15, 2024"",
                                ""events"": [
                                    {
                                        ""name"": ""Registration"",
                                        ""startTime"": ""8:00 AM"",
                                        ""endTime"": ""9:00 AM"",
                                        ""location"": ""Lobby"",
                                        ""speakers"": []
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
            var aiContents = parts.ToAiContents(chatOptions);
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();

            // Output for inspection
            if (functionCallContent != null)
            {
                var conferenceArg = functionCallContent.Arguments["conference"];
                var conferenceJson = JsonSerializer.Serialize(conferenceArg, new JsonSerializerOptions { WriteIndented = true });
                
                Console.WriteLine("=== Original JSON ===");
                Console.WriteLine(@"{
    ""conference"": {
        ""name"": ""AI Summit 2024"",
        ""startDate"": ""May 15, 2024"",
        ""endDate"": ""May 17, 2024"",
        ""venue"": ""Convention Center"",
        ""expectedAttendees"": 500,
        ""dailySchedules"": [...]
    }
}");
                
                Console.WriteLine("\n=== Transformed JSON ===");
                Console.WriteLine(conferenceJson);

                // Try basic deserialization
                try
                {
                    var deserialized = JsonSerializer.Deserialize<ConferenceEvent>(conferenceJson);
                    Console.WriteLine($"\n=== Deserialization Results ===");
                    Console.WriteLine($"Name: '{deserialized?.Name ?? "NULL"}'");
                    Console.WriteLine($"Venue: '{deserialized?.Venue ?? "NULL"}'");
                    Console.WriteLine($"StartDate: {deserialized?.StartDate}");
                    Console.WriteLine($"DailySchedules Count: {deserialized?.DailySchedules?.Length ?? -1}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n=== Deserialization Error ===");
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        [Fact]
        public void Inspect_BookingRequest_Transformation()
        {
            // Arrange
            var function = AIFunctionFactory.Create(ProcessBookingRequest);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };

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
                                ""equipment"": [""projector""]
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

            // Output for inspection
            if (functionCallContent != null)
            {
                var bookingArg = functionCallContent.Arguments["booking"];
                var bookingJson = JsonSerializer.Serialize(bookingArg, new JsonSerializerOptions { WriteIndented = true });
                
                Console.WriteLine("=== Original Booking JSON ===");
                Console.WriteLine(@"{
    ""booking"": {
        ""hotelBooking"": {
            ""guestName"": ""John Smith"",
            ""checkInDate"": ""2024-01-15"",
            ...
        },
        ""conferenceRoomBookings"": [
            {
                ""date"": ""2024-01-16"",
                ...
            }
        ]
    }
}");
                
                Console.WriteLine("\n=== Transformed Booking JSON ===");
                Console.WriteLine(bookingJson);

                // Try basic deserialization
                try
                {
                    var deserialized = JsonSerializer.Deserialize<BookingRequest>(bookingJson);
                    Console.WriteLine($"\n=== Booking Deserialization Results ===");
                    Console.WriteLine($"HotelBooking.GuestName: '{deserialized?.HotelBooking?.GuestName ?? "NULL"}'");
                    Console.WriteLine($"HotelBooking.CheckInDate: {deserialized?.HotelBooking?.CheckInDate}");
                    Console.WriteLine($"ConferenceRoomBookings Count: {deserialized?.ConferenceRoomBookings?.Length ?? -1}");
                    if (deserialized?.ConferenceRoomBookings?.Length > 0)
                    {
                        Console.WriteLine($"First ConferenceRoom Date: {deserialized.ConferenceRoomBookings[0].Date}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n=== Booking Deserialization Error ===");
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        // Data type definitions
        [Description("Process a complex booking request with hotel and conference rooms")]
        public static Task<string> ProcessBookingRequest(BookingRequest booking)
        {
            return Task.FromResult($"Booking processed");
        }

        [Description("Plan a conference event with detailed schedule")]
        public static Task<string> PlanConferenceEvent(ConferenceEvent conference)
        {
            return Task.FromResult($"Conference planned");
        }

        public class BookingRequest
        {
            public HotelBooking? HotelBooking { get; set; }
            public ConferenceRoomBooking[] ConferenceRoomBookings { get; set; } = Array.Empty<ConferenceRoomBooking>();
        }

        public class HotelBooking
        {
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

        public class ConferenceEvent
        {
            public string Name { get; set; } = "";
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
            public string Venue { get; set; } = "";
            public int ExpectedAttendees { get; set; }
            public DailySchedule[] DailySchedules { get; set; } = Array.Empty<DailySchedule>();
        }

        public class DailySchedule
        {
            public DateOnly Date { get; set; }
            public ScheduledEvent[] Events { get; set; } = Array.Empty<ScheduledEvent>();
        }

        public class ScheduledEvent
        {
            public string Name { get; set; } = "";
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public string Location { get; set; } = "";
            public string[] Speakers { get; set; } = Array.Empty<string>();
        }
    }
}