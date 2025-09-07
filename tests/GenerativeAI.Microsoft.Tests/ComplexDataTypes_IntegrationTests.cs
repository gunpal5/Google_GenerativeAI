using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using GenerativeAI.Clients;
using GenerativeAI.Microsoft;
using GenerativeAI.Microsoft.Extensions;
using GenerativeAI.Types;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;

namespace GenerativeAI.Microsoft.Tests
{
    /// <summary>
    /// Integration tests for complex data types with real Gemini API calls
    /// </summary>
    public class ComplexDataTypes_IntegrationTests
    {
        private readonly ITestOutputHelper _output;
        private readonly string? _apiKey;
        private readonly bool _canRunIntegrationTests;

        public ComplexDataTypes_IntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            _apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
            _canRunIntegrationTests = !string.IsNullOrEmpty(_apiKey);
            
            if (!_canRunIntegrationTests)
            {
                _output.WriteLine("GOOGLE_API_KEY_TEST not set. Integration tests will be skipped.");
            }
        }

        [Fact]
        public async Task ComplexBookingSystem_WithNestedDatesAndTimes()
        {
            Assert.SkipWhen(!_canRunIntegrationTests, "GOOGLE_API_KEY_TEST not set");

            var chatClient = new GenerativeAIChatClient(_apiKey, GoogleAIModels.Gemini25Flash);

            var function = AIFunctionFactory.Create(ProcessBookingRequest);
            var chatOptions = new ChatOptions 
            { 
                Tools = new List<AITool> { function },
                Temperature = 0.1f
            };

            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, 
                    "Book a hotel room for John Smith from January 15, 2024 to January 20, 2024. " +
                    "He needs a deluxe room with breakfast included. Check-in at 3:00 PM, check-out at 11:00 AM. " +
                    "Add a conference room booking for January 16, 2024 from 9:00 AM to 5:00 PM for 20 people.")
            };

            var response = await chatClient.GetResponseAsync(messages, chatOptions, TestContext.Current.CancellationToken);
            
            response.ShouldNotBeNull();
            response.Text.ShouldNotBeNullOrWhiteSpace();
            
            _output.WriteLine($"Response text: {response.Text}");
            
            // For integration test, we verify that the API responds appropriately
            // The actual date transformation testing is done in unit tests
            response.Text.ToLower().ShouldContain("booking");
        }

        [Fact]
        public async Task HealthcareAppointmentSystem_WithComplexScheduling()
        {
            Assert.SkipWhen(!_canRunIntegrationTests, "GOOGLE_API_KEY_TEST not set");

            var chatClient = new GenerativeAIChatClient(_apiKey, GoogleAIModels.Gemini15Flash);

            var function = AIFunctionFactory.Create(ScheduleHealthcareAppointments);
            var chatOptions = new ChatOptions 
            { 
                Tools = new List<AITool> { function },
                Temperature = 0.1f
            };

            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, 
                    "Schedule the following appointments for patient Mary Johnson (ID: P12345):\n" +
                    "1. General checkup with Dr. Smith on Feb 5, 2024 at 10:30 AM\n" +
                    "2. Blood test on Feb 6, 2024 at 8:00 AM (fasting required)\n" +
                    "3. Follow-up consultation on Feb 12, 2024 at 2:15 PM\n" +
                    "Also add reminders: Take medication daily at 9:00 AM and 9:00 PM")
            };

            var response = await chatClient.GetResponseAsync(messages, chatOptions, TestContext.Current.CancellationToken);
            
_output.WriteLine($"Healthcare response: {response.Text}");
            
            // For integration test, verify that the API responds with healthcare-related content
            response.Text.ToLower().ShouldContain("appointment");
        }

        [Fact]
        public async Task ProjectManagement_WithMilestonesAndDeadlines()
        {
            Assert.SkipWhen(!_canRunIntegrationTests, "GOOGLE_API_KEY_TEST not set");

            var chatClient = new GenerativeAIChatClient(_apiKey, GoogleAIModels.Gemini15Flash);

            var function = AIFunctionFactory.Create(CreateProjectPlan);
            var chatOptions = new ChatOptions 
            { 
                Tools = new List<AITool> { function },
                Temperature = 0.1f
            };

            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, 
                    "Create a project plan for building a mobile app:\n" +
                    "Project: E-commerce Mobile App\n" +
                    "Start: March 1, 2024\n" +
                    "Milestones:\n" +
                    "- Design phase: March 1-15, 2024\n" +
                    "- Development sprint 1: March 16-31, 2024\n" +
                    "- Development sprint 2: April 1-15, 2024\n" +
                    "- Testing: April 16-25, 2024\n" +
                    "- Launch: April 30, 2024\n" +
                    "Daily standup at 9:30 AM, Sprint reviews every Friday at 3:00 PM")
            };

            var response = await chatClient.GetResponseAsync(messages, chatOptions, TestContext.Current.CancellationToken);
            
_output.WriteLine($"Project response: {response.Text}");
            
            // For integration test, verify that the API responds with project-related content
            response.Text.ToLower().ShouldContain("project");
        }

        [Fact]
        public async Task EventManagement_WithComplexSchedules()
        {
            Assert.SkipWhen(!_canRunIntegrationTests, "GOOGLE_API_KEY_TEST not set");

            var chatClient = new GenerativeAIChatClient(_apiKey, GoogleAIModels.Gemini25Flash);

            var function = AIFunctionFactory.Create(PlanConferenceEvent);
            var chatOptions = new ChatOptions 
            { 
                Tools = new List<AITool> { function },
                Temperature = 0.1f
            };

            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, 
                    "Plan a tech conference:\n" +
                    "Name: AI Summit 2024\n" +
                    "Date: May 15-17, 2024\n" +
                    "Venue: Convention Center, San Francisco\n" +
                    "Schedule:\n" +
                    "Day 1 (May 15): Registration 8:00 AM, Keynote 9:00 AM, Sessions 10:30 AM-5:00 PM\n" +
                    "Day 2 (May 16): Sessions 9:00 AM-5:00 PM, Networking 6:00 PM\n" +
                    "Day 3 (May 17): Workshops 9:00 AM-12:00 PM, Closing 2:00 PM\n" +
                    "Expected attendees: 500")
            };

            var response = await chatClient.GetResponseAsync(messages, chatOptions, TestContext.Current.CancellationToken);
            
_output.WriteLine($"Conference response: {response.Text}");
            
            // For integration test, verify that the API responds with conference-related content
            response.Text.ToLower().ShouldContain("planned");
        }

        // Function definitions for the tests

        [Description("Process a complex booking request with hotel and conference rooms")]
        public static Task<string> ProcessBookingRequest(BookingRequest booking)
        {
            return Task.FromResult($"Booking processed for {booking.HotelBooking?.GuestName}");
        }

        [Description("Schedule healthcare appointments with reminders")]
        public static Task<string> ScheduleHealthcareAppointments(HealthcareSchedule schedule)
        {
            return Task.FromResult($"Scheduled {schedule.Appointments.Length} appointments");
        }

        [Description("Create a project plan with milestones and meetings")]
        public static Task<string> CreateProjectPlan(ProjectPlan project)
        {
            return Task.FromResult($"Project {project.Name} created");
        }

        [Description("Plan a conference event with detailed schedule")]
        public static Task<string> PlanConferenceEvent(ConferenceEvent conference)
        {
            return Task.FromResult($"Conference {conference.Name} planned");
        }

        // Complex data type definitions

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
            
            [Description("Check-in date in YYYY-MM-DD format")]
            public DateOnly CheckInDate { get; set; }
            
            [Description("Check-out date in YYYY-MM-DD format")]
            public DateOnly CheckOutDate { get; set; }
            
            [Description("Check-in time in HH:mm:ss format")]
            public TimeOnly CheckInTime { get; set; }
            
            [Description("Check-out time in HH:mm:ss format")]
            public TimeOnly CheckOutTime { get; set; }
            
            public string RoomType { get; set; } = "";
            public string[] Amenities { get; set; } = Array.Empty<string>();
        }

        public class ConferenceRoomBooking
        {
            [Description("Booking date in YYYY-MM-DD format")]
            public DateOnly Date { get; set; }
            
            [Description("Start time in HH:mm:ss format")]
            public TimeOnly StartTime { get; set; }
            
            [Description("End time in HH:mm:ss format")]
            public TimeOnly EndTime { get; set; }
            
            public int Capacity { get; set; }
            public string[] Equipment { get; set; } = Array.Empty<string>();
        }

        public class SpecialRequests
        {
            public string[] DietaryRestrictions { get; set; } = Array.Empty<string>();
            public string[] Accessibility { get; set; } = Array.Empty<string>();
        }

        public class HealthcareSchedule
        {
            public string PatientId { get; set; } = "";
            public string PatientName { get; set; } = "";
            public Appointment[] Appointments { get; set; } = Array.Empty<Appointment>();
            public MedicationReminder[] MedicationReminders { get; set; } = Array.Empty<MedicationReminder>();
        }

        public class Appointment
        {
            [Description("Appointment date in YYYY-MM-DD format")]
            public DateOnly Date { get; set; }
            
            [Description("Appointment time in HH:mm:ss format")]
            public TimeOnly Time { get; set; }
            
            public string Type { get; set; } = "";
            public string Provider { get; set; } = "";
            public string[] PreparationInstructions { get; set; } = Array.Empty<string>();
        }

        public class MedicationReminder
        {
            public string MedicationName { get; set; } = "";
            
            [Description("Medication time in HH:mm:ss format")]
            public TimeOnly Time { get; set; }
            
            public string Frequency { get; set; } = "";
        }

        public class ProjectPlan
        {
            public string Name { get; set; } = "";
            
            [Description("Project start date in YYYY-MM-DD format")]
            public DateOnly StartDate { get; set; }
            
            [Description("Project launch date in YYYY-MM-DD format")]
            public DateOnly LaunchDate { get; set; }
            
            public Milestone[] Milestones { get; set; } = Array.Empty<Milestone>();
            public RecurringMeeting[] RecurringMeetings { get; set; } = Array.Empty<RecurringMeeting>();
        }

        public class Milestone
        {
            public string Name { get; set; } = "";
            
            [Description("Milestone start date in YYYY-MM-DD format")]
            public DateOnly StartDate { get; set; }
            
            [Description("Milestone end date in YYYY-MM-DD format")]
            public DateOnly EndDate { get; set; }
            
            public string[] Deliverables { get; set; } = Array.Empty<string>();
        }

        public class RecurringMeeting
        {
            public string Name { get; set; } = "";
            
            [Description("Meeting time in HH:mm:ss format")]
            public TimeOnly Time { get; set; }
            
            public string Frequency { get; set; } = "";
            public string[] DaysOfWeek { get; set; } = Array.Empty<string>();
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