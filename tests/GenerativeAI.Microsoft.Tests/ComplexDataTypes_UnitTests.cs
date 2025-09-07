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
    /// Unit tests for complex data type transformations without API calls
    /// </summary>
    public class ComplexDataTypes_UnitTests
    {
        [Fact]
        public void Transform_FinancialTransaction_WithComplexAmounts()
        {
            var function = AIFunctionFactory.Create(ProcessFinancialTransaction);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };

            var functionCall = new FunctionCall
            {
                Name = "ProcessFinancialTransaction",
                Args = JsonNode.Parse(@"{
                    ""transaction"": {
                        ""transactionDate"": ""March 15, 2024"",
                        ""settlementDate"": ""March 18, 2024"",
                        ""amount"": 10000.50,
                        ""currency"": ""USD"",
                        ""parties"": {
                            ""sender"": {
                                ""name"": ""John Doe"",
                                ""accountNumber"": ""ACC123456"",
                                ""bankCode"": ""BANK001""
                            },
                            ""receiver"": {
                                ""name"": ""Jane Smith"",
                                ""accountNumber"": ""ACC789012"",
                                ""bankCode"": ""BANK002""
                            }
                        },
                        ""schedule"": {
                            ""recurringPayments"": [
                                {
                                    ""date"": ""April 15, 2024"",
                                    ""amount"": 1000.00
                                },
                                {
                                    ""date"": ""May 15, 2024"",
                                    ""amount"": 1000.00
                                }
                            ],
                            ""reminderTime"": ""09:00 AM""
                        }
                    }
                }")
            };

            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };

            var aiContents = parts.ToAiContents(chatOptions);
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();

            functionCallContent.ShouldNotBeNull();
            var transaction = functionCallContent.Arguments["transaction"] as JsonObject;
            
            // Verify date transformations
            transaction?["transactionDate"]?.GetValue<string>().ShouldBe("2024-03-15");
            transaction?["settlementDate"]?.GetValue<string>().ShouldBe("2024-03-18");

            // Verify nested schedule transformations
            var schedule = transaction?["schedule"] as JsonObject;
            var recurringPayments = schedule?["recurringPayments"] as JsonArray;
            recurringPayments?.Count.ShouldBe(2);
            
            var payment1 = recurringPayments?[0] as JsonObject;
            payment1?["date"]?.GetValue<string>().ShouldBe("2024-04-15");
            
            var payment2 = recurringPayments?[1] as JsonObject;
            payment2?["date"]?.GetValue<string>().ShouldBe("2024-05-15");
            
            schedule?["reminderTime"]?.GetValue<string>().ShouldBe("09:00:00");
        }

        [Fact]
        public void Transform_EducationSchedule_WithComplexTimetable()
        {
            var function = AIFunctionFactory.Create(CreateEducationSchedule);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };

            var functionCall = new FunctionCall
            {
                Name = "CreateEducationSchedule",
                Args = JsonNode.Parse(@"{
                    ""schedule"": {
                        ""semester"": ""Spring 2024"",
                        ""startDate"": ""January 8, 2024"",
                        ""endDate"": ""May 17, 2024"",
                        ""courses"": [
                            {
                                ""name"": ""Advanced Mathematics"",
                                ""schedule"": {
                                    ""days"": [""Monday"", ""Wednesday"", ""Friday""],
                                    ""startTime"": ""9:00 AM"",
                                    ""endTime"": ""10:30 AM"",
                                    ""room"": ""Room 101""
                                },
                                ""exams"": [
                                    {
                                        ""type"": ""Midterm"",
                                        ""date"": ""March 8, 2024"",
                                        ""startTime"": ""2:00 PM"",
                                        ""duration"": 120
                                    },
                                    {
                                        ""type"": ""Final"",
                                        ""date"": ""May 10, 2024"",
                                        ""startTime"": ""9:00 AM"",
                                        ""duration"": 180
                                    }
                                ]
                            }
                        ],
                        ""holidays"": [
                            {
                                ""name"": ""Spring Break"",
                                ""startDate"": ""March 11, 2024"",
                                ""endDate"": ""March 15, 2024""
                            }
                        ]
                    }
                }")
            };

            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };

            var aiContents = parts.ToAiContents(chatOptions);
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();

            functionCallContent.ShouldNotBeNull();
            var schedule = functionCallContent.Arguments["schedule"] as JsonObject;
            
            // Verify main dates
            schedule?["startDate"]?.GetValue<string>().ShouldBe("2024-01-08");
            schedule?["endDate"]?.GetValue<string>().ShouldBe("2024-05-17");

            // Verify course schedule times
            var courses = schedule?["courses"] as JsonArray;
            var course = courses?[0] as JsonObject;
            var courseSchedule = course?["schedule"] as JsonObject;
            courseSchedule?["startTime"]?.GetValue<string>().ShouldBe("09:00:00");
            courseSchedule?["endTime"]?.GetValue<string>().ShouldBe("10:30:00");

            // Verify exam dates and times
            var exams = course?["exams"] as JsonArray;
            var midterm = exams?[0] as JsonObject;
            midterm?["date"]?.GetValue<string>().ShouldBe("2024-03-08");
            midterm?["startTime"]?.GetValue<string>().ShouldBe("14:00:00");

            var final = exams?[1] as JsonObject;
            final?["date"]?.GetValue<string>().ShouldBe("2024-05-10");
            final?["startTime"]?.GetValue<string>().ShouldBe("09:00:00");

            // Verify holidays
            var holidays = schedule?["holidays"] as JsonArray;
            var springBreak = holidays?[0] as JsonObject;
            springBreak?["startDate"]?.GetValue<string>().ShouldBe("2024-03-11");
            springBreak?["endDate"]?.GetValue<string>().ShouldBe("2024-03-15");
        }

        [Fact]
        public void Transform_ManufacturingSchedule_WithShifts()
        {
            var function = AIFunctionFactory.Create(CreateManufacturingSchedule);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };

            var functionCall = new FunctionCall
            {
                Name = "CreateManufacturingSchedule",
                Args = JsonNode.Parse(@"{
                    ""production"": {
                        ""orderId"": ""ORD-2024-001"",
                        ""startDate"": ""February 1, 2024"",
                        ""dueDate"": ""February 28, 2024"",
                        ""shifts"": [
                            {
                                ""name"": ""Morning Shift"",
                                ""startTime"": ""6:00 AM"",
                                ""endTime"": ""2:00 PM"",
                                ""breakTimes"": [""10:00 AM"", ""12:00 PM""]
                            },
                            {
                                ""name"": ""Evening Shift"",
                                ""startTime"": ""2:00 PM"",
                                ""endTime"": ""10:00 PM"",
                                ""breakTimes"": [""6:00 PM"", ""8:00 PM""]
                            }
                        ],
                        ""maintenanceWindows"": [
                            {
                                ""date"": ""February 10, 2024"",
                                ""startTime"": ""11:00 PM"",
                                ""endTime"": ""3:00 AM"",
                                ""equipment"": [""Line A"", ""Line B""]
                            }
                        ],
                        ""qualityChecks"": [
                            {
                                ""date"": ""February 5, 2024"",
                                ""time"": ""9:30 AM"",
                                ""inspector"": ""John Doe""
                            },
                            {
                                ""date"": ""February 15, 2024"",
                                ""time"": ""2:30 PM"",
                                ""inspector"": ""Jane Smith""
                            }
                        ]
                    }
                }")
            };

            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };

            var aiContents = parts.ToAiContents(chatOptions);
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();

            functionCallContent.ShouldNotBeNull();
            var production = functionCallContent.Arguments["production"] as JsonObject;
            
            // Verify production dates
            production?["startDate"]?.GetValue<string>().ShouldBe("2024-02-01");
            production?["dueDate"]?.GetValue<string>().ShouldBe("2024-02-28");

            // Verify shift times
            var shifts = production?["shifts"] as JsonArray;
            var morningShift = shifts?[0] as JsonObject;
            morningShift?["startTime"]?.GetValue<string>().ShouldBe("06:00:00");
            morningShift?["endTime"]?.GetValue<string>().ShouldBe("14:00:00");
            
            var breakTimes = morningShift?["breakTimes"] as JsonArray;
            breakTimes?[0]?.GetValue<string>().ShouldBe("10:00:00");
            breakTimes?[1]?.GetValue<string>().ShouldBe("12:00:00");

            // Verify maintenance windows
            var maintenance = (production?["maintenanceWindows"] as JsonArray)?[0] as JsonObject;
            maintenance?["date"]?.GetValue<string>().ShouldBe("2024-02-10");
            maintenance?["startTime"]?.GetValue<string>().ShouldBe("23:00:00");
            maintenance?["endTime"]?.GetValue<string>().ShouldBe("03:00:00");

            // Verify quality checks
            var qualityChecks = production?["qualityChecks"] as JsonArray;
            var check1 = qualityChecks?[0] as JsonObject;
            check1?["date"]?.GetValue<string>().ShouldBe("2024-02-05");
            check1?["time"]?.GetValue<string>().ShouldBe("09:30:00");
        }

        [Fact]
        public void Transform_TravelItinerary_WithMultipleSegments()
        {
            var function = AIFunctionFactory.Create(CreateTravelItinerary);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };

            var functionCall = new FunctionCall
            {
                Name = "CreateTravelItinerary",
                Args = JsonNode.Parse(@"{
                    ""itinerary"": {
                        ""travelerName"": ""Alice Johnson"",
                        ""tripStartDate"": ""June 1, 2024"",
                        ""tripEndDate"": ""June 10, 2024"",
                        ""segments"": [
                            {
                                ""type"": ""Flight"",
                                ""departureDate"": ""June 1, 2024"",
                                ""departureTime"": ""8:30 AM"",
                                ""arrivalDate"": ""June 1, 2024"",
                                ""arrivalTime"": ""11:45 AM"",
                                ""from"": ""New York"",
                                ""to"": ""Los Angeles""
                            },
                            {
                                ""type"": ""Hotel"",
                                ""checkInDate"": ""June 1, 2024"",
                                ""checkInTime"": ""3:00 PM"",
                                ""checkOutDate"": ""June 5, 2024"",
                                ""checkOutTime"": ""11:00 AM"",
                                ""hotelName"": ""Grand Hotel LA""
                            }
                        ],
                        ""activities"": [
                            {
                                ""name"": ""City Tour"",
                                ""date"": ""June 2, 2024"",
                                ""startTime"": ""9:00 AM"",
                                ""endTime"": ""5:00 PM"",
                                ""meetingPoint"": ""Hotel Lobby""
                            },
                            {
                                ""name"": ""Beach Day"",
                                ""date"": ""June 3, 2024"",
                                ""startTime"": ""10:00 AM"",
                                ""endTime"": ""4:00 PM"",
                                ""location"": ""Santa Monica Beach""
                            }
                        ]
                    }
                }")
            };

            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };

            var aiContents = parts.ToAiContents(chatOptions);
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();

            functionCallContent.ShouldNotBeNull();
            var itinerary = functionCallContent.Arguments["itinerary"] as JsonObject;
            
            // Verify trip dates
            itinerary?["tripStartDate"]?.GetValue<string>().ShouldBe("2024-06-01");
            itinerary?["tripEndDate"]?.GetValue<string>().ShouldBe("2024-06-10");

            // Verify flight segment
            var segments = itinerary?["segments"] as JsonArray;
            var flight = segments?[0] as JsonObject;
            flight?["departureDate"]?.GetValue<string>().ShouldBe("2024-06-01");
            flight?["departureTime"]?.GetValue<string>().ShouldBe("08:30:00");
            flight?["arrivalDate"]?.GetValue<string>().ShouldBe("2024-06-01");
            flight?["arrivalTime"]?.GetValue<string>().ShouldBe("11:45:00");

            // Verify hotel segment
            var hotel = segments?[1] as JsonObject;
            hotel?["checkInDate"]?.GetValue<string>().ShouldBe("2024-06-01");
            hotel?["checkInTime"]?.GetValue<string>().ShouldBe("15:00:00");
            hotel?["checkOutDate"]?.GetValue<string>().ShouldBe("2024-06-05");
            hotel?["checkOutTime"]?.GetValue<string>().ShouldBe("11:00:00");

            // Verify activities
            var activities = itinerary?["activities"] as JsonArray;
            var cityTour = activities?[0] as JsonObject;
            cityTour?["date"]?.GetValue<string>().ShouldBe("2024-06-02");
            cityTour?["startTime"]?.GetValue<string>().ShouldBe("09:00:00");
            cityTour?["endTime"]?.GetValue<string>().ShouldBe("17:00:00");
        }

        // Function definitions
        [Description("Process a complex financial transaction")]
        public static Task<string> ProcessFinancialTransaction(FinancialTransaction transaction)
        {
            return Task.FromResult($"Transaction processed: {transaction.Amount} {transaction.Currency}");
        }

        [Description("Create an education schedule")]
        public static Task<string> CreateEducationSchedule(EducationSchedule schedule)
        {
            return Task.FromResult($"Schedule created for {schedule.Semester}");
        }

        [Description("Create a manufacturing schedule")]
        public static Task<string> CreateManufacturingSchedule(ManufacturingProduction production)
        {
            return Task.FromResult($"Production schedule created for {production.OrderId}");
        }

        [Description("Create a travel itinerary")]
        public static Task<string> CreateTravelItinerary(TravelItinerary itinerary)
        {
            return Task.FromResult($"Itinerary created for {itinerary.TravelerName}");
        }

        // Complex data type definitions
        public class FinancialTransaction
        {
            public DateOnly TransactionDate { get; set; }
            public DateOnly SettlementDate { get; set; }
            public decimal Amount { get; set; }
            public string Currency { get; set; } = "";
            public TransactionParties Parties { get; set; } = new();
            public PaymentSchedule Schedule { get; set; } = new();
        }

        public class TransactionParties
        {
            public Party Sender { get; set; } = new();
            public Party Receiver { get; set; } = new();
        }

        public class Party
        {
            public string Name { get; set; } = "";
            public string AccountNumber { get; set; } = "";
            public string BankCode { get; set; } = "";
        }

        public class PaymentSchedule
        {
            public RecurringPayment[] RecurringPayments { get; set; } = Array.Empty<RecurringPayment>();
            public TimeOnly ReminderTime { get; set; }
        }

        public class RecurringPayment
        {
            public DateOnly Date { get; set; }
            public decimal Amount { get; set; }
        }

        public class EducationSchedule
        {
            public string Semester { get; set; } = "";
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
            public Course[] Courses { get; set; } = Array.Empty<Course>();
            public Holiday[] Holidays { get; set; } = Array.Empty<Holiday>();
        }

        public class Course
        {
            public string Name { get; set; } = "";
            public CourseSchedule Schedule { get; set; } = new();
            public Exam[] Exams { get; set; } = Array.Empty<Exam>();
        }

        public class CourseSchedule
        {
            public string[] Days { get; set; } = Array.Empty<string>();
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public string Room { get; set; } = "";
        }

        public class Exam
        {
            public string Type { get; set; } = "";
            public DateOnly Date { get; set; }
            public TimeOnly StartTime { get; set; }
            public int Duration { get; set; } // in minutes
        }

        public class Holiday
        {
            public string Name { get; set; } = "";
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
        }

        public class ManufacturingProduction
        {
            public string OrderId { get; set; } = "";
            public DateOnly StartDate { get; set; }
            public DateOnly DueDate { get; set; }
            public Shift[] Shifts { get; set; } = Array.Empty<Shift>();
            public MaintenanceWindow[] MaintenanceWindows { get; set; } = Array.Empty<MaintenanceWindow>();
            public QualityCheck[] QualityChecks { get; set; } = Array.Empty<QualityCheck>();
        }

        public class Shift
        {
            public string Name { get; set; } = "";
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public TimeOnly[] BreakTimes { get; set; } = Array.Empty<TimeOnly>();
        }

        public class MaintenanceWindow
        {
            public DateOnly Date { get; set; }
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public string[] Equipment { get; set; } = Array.Empty<string>();
        }

        public class QualityCheck
        {
            public DateOnly Date { get; set; }
            public TimeOnly Time { get; set; }
            public string Inspector { get; set; } = "";
        }

        public class TravelItinerary
        {
            public string TravelerName { get; set; } = "";
            public DateOnly TripStartDate { get; set; }
            public DateOnly TripEndDate { get; set; }
            public TravelSegment[] Segments { get; set; } = Array.Empty<TravelSegment>();
            public Activity[] Activities { get; set; } = Array.Empty<Activity>();
        }

        public class TravelSegment
        {
            public string Type { get; set; } = "";
            public DateOnly DepartureDate { get; set; }
            public TimeOnly DepartureTime { get; set; }
            public DateOnly ArrivalDate { get; set; }
            public TimeOnly ArrivalTime { get; set; }
            public string From { get; set; } = "";
            public string To { get; set; } = "";
            public DateOnly CheckInDate { get; set; }
            public TimeOnly CheckInTime { get; set; }
            public DateOnly CheckOutDate { get; set; }
            public TimeOnly CheckOutTime { get; set; }
            public string HotelName { get; set; } = "";
        }

        public class Activity
        {
            public string Name { get; set; } = "";
            public DateOnly Date { get; set; }
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public string Location { get; set; } = "";
            public string MeetingPoint { get; set; } = "";
        }
    }
}