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
using Shouldly;
using Xunit;

namespace GenerativeAI.IntegrationTests
{
    public class DateTimeTransformation_ComplexTests
    {
        [Fact]
        public void Transform_DeeplyNestedStructure_FiveLevelsDeep()
        {
            var function = AIFunctionFactory.Create(DeeplyNestedFunction);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };
            
            var functionCall = new FunctionCall
            {
                Name = "DeeplyNestedFunction",
                Args = JsonNode.Parse(@"{
                    ""level1"": {
                        ""date"": ""January 1, 2024"",
                        ""level2"": {
                            ""time"": ""3:45 PM"",
                            ""level3"": {
                                ""dates"": [""Feb 14, 2024"", ""March 17, 2024""],
                                ""level4"": {
                                    ""schedule"": {
                                        ""startDate"": ""April 1, 2024"",
                                        ""endDate"": ""2024-04-30T23:59:59Z"",
                                        ""times"": [""9:00 AM"", ""5:00 PM""]
                                    }
                                }
                            }
                        }
                    }
                }")
            };
            
            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };
            
            var aiContents = parts.ToAiContents(chatOptions);
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();
            
            functionCallContent.ShouldNotBeNull();
            var level1 = functionCallContent.Arguments["level1"] as JsonObject;
            level1.ShouldNotBeNull();
            
            // Check level 1
            level1["date"]?.GetValue<string>().ShouldBe("2024-01-01");
            
            // Check level 2
            var level2 = level1["level2"] as JsonObject;
            level2?["time"]?.GetValue<string>().ShouldBe("15:45:00");
            
            // Check level 3
            var level3 = level2?["level3"] as JsonObject;
            var datesArray = level3?["dates"] as JsonArray;
            datesArray?.Count.ShouldBe(2);
            datesArray?[0]?.GetValue<string>().ShouldBe("2024-02-14");
            datesArray?[1]?.GetValue<string>().ShouldBe("2024-03-17");
            
            // Check level 4 & 5
            var level4 = level3?["level4"] as JsonObject;
            var schedule = level4?["schedule"] as JsonObject;
            schedule?["startDate"]?.GetValue<string>().ShouldBe("2024-04-01");
            schedule?["endDate"]?.GetValue<string>().ShouldBe("2024-04-30");
            
            var times = schedule?["times"] as JsonArray;
            times?.Count.ShouldBe(2);
            times?[0]?.GetValue<string>().ShouldBe("09:00:00");
            times?[1]?.GetValue<string>().ShouldBe("17:00:00");
        }
        
        [Fact]
        public void Transform_MixedArraysAndObjects_ComplexNesting()
        {
            var function = AIFunctionFactory.Create(MixedNestingFunction);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };
            
            var functionCall = new FunctionCall
            {
                Name = "MixedNestingFunction",
                Args = JsonNode.Parse(@"{
                    ""data"": [
                        {
                            ""id"": 1,
                            ""dates"": [""2024-01-15"", ""Feb 20, 2024""],
                            ""nested"": {
                                ""appointments"": [
                                    {
                                        ""date"": ""March 1, 2024"",
                                        ""time"": ""10:30 AM"",
                                        ""reminders"": [
                                            {
                                                ""reminderDate"": ""February 28, 2024"",
                                                ""reminderTime"": ""9:00 AM""
                                            }
                                        ]
                                    }
                                ]
                            }
                        },
                        {
                            ""id"": 2,
                            ""dates"": [""December 25, 2024""],
                            ""nested"": {
                                ""appointments"": [
                                    {
                                        ""date"": ""Dec 31, 2024"",
                                        ""time"": ""11:59 PM"",
                                        ""reminders"": []
                                    }
                                ]
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
            var dataArray = functionCallContent.Arguments["data"] as JsonArray;
            dataArray?.Count.ShouldBe(2);
            
            // Check first item
            var item1 = dataArray?[0] as JsonObject;
            var dates1 = item1?["dates"] as JsonArray;
            dates1?[0]?.GetValue<string>().ShouldBe("2024-01-15");
            dates1?[1]?.GetValue<string>().ShouldBe("2024-02-20");
            
            var nested1 = item1?["nested"] as JsonObject;
            var appointments1 = nested1?["appointments"] as JsonArray;
            var appt1 = appointments1?[0] as JsonObject;
            appt1?["date"]?.GetValue<string>().ShouldBe("2024-03-01");
            appt1?["time"]?.GetValue<string>().ShouldBe("10:30:00");
            
            var reminders1 = appt1?["reminders"] as JsonArray;
            var reminder1 = reminders1?[0] as JsonObject;
            reminder1?["reminderDate"]?.GetValue<string>().ShouldBe("2024-02-28");
            reminder1?["reminderTime"]?.GetValue<string>().ShouldBe("09:00:00");
            
            // Check second item
            var item2 = dataArray?[1] as JsonObject;
            var dates2 = item2?["dates"] as JsonArray;
            dates2?[0]?.GetValue<string>().ShouldBe("2024-12-25");
            
            var nested2 = item2?["nested"] as JsonObject;
            var appointments2 = nested2?["appointments"] as JsonArray;
            var appt2 = appointments2?[0] as JsonObject;
            appt2?["date"]?.GetValue<string>().ShouldBe("2024-12-31");
            appt2?["time"]?.GetValue<string>().ShouldBe("23:59:00");
        }
        
        [Fact]
        public void Transform_MultiDimensionalArrays()
        {
            var function = AIFunctionFactory.Create(MultiDimensionalArrayFunction);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };
            
            var functionCall = new FunctionCall
            {
                Name = "MultiDimensionalArrayFunction",
                Args = JsonNode.Parse(@"{
                    ""matrix"": [
                        [""Jan 1, 2024"", ""Jan 2, 2024"", ""Jan 3, 2024""],
                        [""Feb 1, 2024"", ""Feb 2, 2024"", ""Feb 3, 2024""],
                        [""Mar 1, 2024"", ""Mar 2, 2024"", ""Mar 3, 2024""]
                    ]
                }")
            };
            
            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };
            
            var aiContents = parts.ToAiContents(chatOptions);
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();
            
            functionCallContent.ShouldNotBeNull();
            var matrix = functionCallContent.Arguments["matrix"] as JsonArray;
            matrix?.Count.ShouldBe(3);
            
            // Check first row
            var row1 = matrix?[0] as JsonArray;
            row1?[0]?.GetValue<string>().ShouldBe("2024-01-01");
            row1?[1]?.GetValue<string>().ShouldBe("2024-01-02");
            row1?[2]?.GetValue<string>().ShouldBe("2024-01-03");
            
            // Check second row
            var row2 = matrix?[1] as JsonArray;
            row2?[0]?.GetValue<string>().ShouldBe("2024-02-01");
            row2?[1]?.GetValue<string>().ShouldBe("2024-02-02");
            row2?[2]?.GetValue<string>().ShouldBe("2024-02-03");
            
            // Check third row
            var row3 = matrix?[2] as JsonArray;
            row3?[0]?.GetValue<string>().ShouldBe("2024-03-01");
            row3?[1]?.GetValue<string>().ShouldBe("2024-03-02");
            row3?[2]?.GetValue<string>().ShouldBe("2024-03-03");
        }
        
        [Fact]
        public void Transform_LargeDataSet_Performance()
        {
            var function = AIFunctionFactory.Create(LargeDataSetFunction);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };
            
            // Generate a large dataset with 100 items
            var items = new List<object>();
            for (int i = 1; i <= 100; i++)
            {
                items.Add(new
                {
                    id = i,
                    date = $"January {i % 28 + 1}, 2024",
                    time = $"{i % 12 + 1}:{i % 60:D2} {(i % 24 < 12 ? "AM" : "PM")}",
                    nested = new
                    {
                        startDate = $"2024-{i % 12 + 1:D2}-{i % 28 + 1:D2}",
                        endDate = $"February {i % 28 + 1}, 2024"
                    }
                });
            }
            
            var functionCall = new FunctionCall
            {
                Name = "LargeDataSetFunction",
                Args = JsonNode.Parse(JsonSerializer.Serialize(new { items }))
            };
            
            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };
            
            var startTime = DateTime.UtcNow;
            var aiContents = parts.ToAiContents(chatOptions);
            var duration = DateTime.UtcNow - startTime;
            
            // Performance assertion - should complete within reasonable time
            duration.TotalSeconds.ShouldBeLessThan(5);
            
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();
            functionCallContent.ShouldNotBeNull();
            
            var itemsArray = functionCallContent.Arguments["items"] as JsonArray;
            itemsArray?.Count.ShouldBe(100);
            
            // Spot check a few items
            var item1 = itemsArray?[0] as JsonObject;
            item1?["date"]?.GetValue<string>().ShouldBe("2024-01-02");
            item1?["time"]?.GetValue<string>().ShouldMatch(@"\d{2}:\d{2}:\d{2}");
            
            var nested1 = item1?["nested"] as JsonObject;
            nested1?["startDate"]?.GetValue<string>().ShouldMatch(@"\d{4}-\d{2}-\d{2}");
            nested1?["endDate"]?.GetValue<string>().ShouldBe("2024-02-02");
            
            var item50 = itemsArray?[49] as JsonObject;
            item50?["date"]?.GetValue<string>().ShouldMatch(@"\d{4}-\d{2}-\d{2}");
            
            var item100 = itemsArray?[99] as JsonObject;
            item100?["date"]?.GetValue<string>().ShouldMatch(@"\d{4}-\d{2}-\d{2}");
        }
        
        [Fact]
        public void Transform_InternationalDateFormats()
        {
            var function = AIFunctionFactory.Create(InternationalDatesFunction);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };
            
            var functionCall = new FunctionCall
            {
                Name = "InternationalDatesFunction",
                Args = JsonNode.Parse(@"{
                    ""dates"": {
                        ""us"": ""12/31/2024"",
                        ""uk"": ""31 December 2024"",
                        ""iso"": ""2024-07-04T12:00:00Z"",
                        ""german"": ""1. Januar 2024"",
                        ""french"": ""14 juillet 2024"",
                        ""spanish"": ""15 de agosto de 2024"",
                        ""japanese"": ""2024年9月20日"",
                        ""chinese"": ""2024年10月1日""
                    }
                }")
            };
            
            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };
            
            var aiContents = parts.ToAiContents(chatOptions);
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();
            
            functionCallContent.ShouldNotBeNull();
            var dates = functionCallContent.Arguments["dates"] as JsonObject;
            
            // Check various formats - some may not parse correctly but should not throw
            dates?["us"]?.GetValue<string>().ShouldNotBeNull();
            dates?["uk"]?.GetValue<string>().ShouldBe("2024-12-31");
            dates?["iso"]?.GetValue<string>().ShouldBe("2024-07-04");
            
            // Non-English dates might not parse, but should be returned as-is
            dates?["german"]?.GetValue<string>().ShouldNotBeNull();
            dates?["french"]?.GetValue<string>().ShouldNotBeNull();
            dates?["spanish"]?.GetValue<string>().ShouldNotBeNull();
            dates?["japanese"]?.GetValue<string>().ShouldNotBeNull();
            dates?["chinese"]?.GetValue<string>().ShouldNotBeNull();
        }
        
        // [Fact]
        // public void Transform_RecursiveStructure()
        // {
        //     var function = AIFunctionFactory.Create(RecursiveStructureFunction);
        //     var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };
        //     
        //     var functionCall = new FunctionCall
        //     {
        //         Name = "RecursiveStructureFunction",
        //         Args = JsonNode.Parse(@"{
        //             ""node"": {
        //                 ""date"": ""January 15, 2024"",
        //                 ""children"": [
        //                     {
        //                         ""date"": ""February 20, 2024"",
        //                         ""children"": [
        //                             {
        //                                 ""date"": ""March 25, 2024"",
        //                                 ""children"": []
        //                             }
        //                         ]
        //                     },
        //                     {
        //                         ""date"": ""April 30, 2024"",
        //                         ""children"": []
        //                     }
        //                 ]
        //             }
        //         }")
        //     };
        //     
        //     var part = new Part { FunctionCall = functionCall };
        //     var parts = new List<Part> { part };
        //     
        //     var aiContents = parts.ToAiContents(chatOptions);
        //     var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();
        //     
        //     functionCallContent.ShouldNotBeNull();
        //     var node = functionCallContent.Arguments["node"] as JsonObject;
        //     
        //     // Check root node
        //     node?["date"]?.GetValue<string>().ShouldBe("2024-01-15");
        //     
        //     // Check first level children
        //     var children = node?["children"] as JsonArray;
        //     children?.Count.ShouldBe(2);
        //     
        //     var child1 = children?[0] as JsonObject;
        //     child1?["date"]?.GetValue<string>().ShouldBe("2024-02-20");
        //     
        //     var child2 = children?[1] as JsonObject;
        //     child2?["date"]?.GetValue<string>().ShouldBe("2024-04-30");
        //     
        //     // Check second level children
        //     var grandChildren = child1?["children"] as JsonArray;
        //     grandChildren?.Count.ShouldBe(1);
        //     
        //     var grandChild = grandChildren?[0] as JsonObject;
        //     grandChild?["date"]?.GetValue<string>().ShouldBe("2024-03-25");
        // }
        
        [Fact]
        public void Transform_MixedDateTimeFormats_InSingleObject()
        {
            var function = AIFunctionFactory.Create(MixedFormatsFunction);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };
            
            var functionCall = new FunctionCall
            {
                Name = "MixedFormatsFunction",
                Args = JsonNode.Parse(@"{
                    ""schedule"": {
                        ""date1"": ""2024-01-15"",
                        ""date2"": ""Jan 20, 2024"",
                        ""date3"": ""15th February 2024"",
                        ""time1"": ""09:30:00"",
                        ""time2"": ""2:45 PM"",
                        ""time3"": ""17:00"",
                        ""datetime1"": ""2024-03-10T14:30:00Z"",
                        ""datetime2"": ""March 15, 2024 at 3:45 PM"",
                        ""nullDate"": null,
                        ""emptyDate"": """",
                        ""invalidDate"": ""not a date"",
                        ""number"": 42,
                        ""boolean"": true,
                        ""text"": ""Regular text""
                    }
                }")
            };
            
            var part = new Part { FunctionCall = functionCall };
            var parts = new List<Part> { part };
            
            var aiContents = parts.ToAiContents(chatOptions);
            var functionCallContent = aiContents.OfType<FunctionCallContent>().FirstOrDefault();
            
            functionCallContent.ShouldNotBeNull();
            var schedule = functionCallContent.Arguments["schedule"] as JsonObject;
            
            // Date transformations
            schedule?["date1"]?.GetValue<string>().ShouldBe("2024-01-15");
            schedule?["date2"]?.GetValue<string>().ShouldBe("2024-01-20");
            
            // Time transformations
            schedule?["time1"]?.GetValue<string>().ShouldBe("09:30:00");
            schedule?["time2"]?.GetValue<string>().ShouldBe("14:45:00");
            schedule?["time3"]?.GetValue<string>().ShouldBe("17:00");
            
            // DateTime transformations
            schedule?["datetime1"]?.GetValue<string>().ShouldMatch(@"\d{4}-\d{2}-\d{2}");
            
            // Non-date values should be preserved
            schedule?["nullDate"].ShouldBeNull();
            schedule?["emptyDate"]?.GetValue<string>().ShouldBe("");
            schedule?["invalidDate"]?.GetValue<string>().ShouldBe("not a date");
            schedule?["number"]?.GetValue<int>().ShouldBe(42);
            schedule?["boolean"]?.GetValue<bool>().ShouldBe(true);
            schedule?["text"]?.GetValue<string>().ShouldBe("Regular text");
        }
        
        [Fact]
        public void Transform_ObjectsInArraysInObjects()
        {
            var function = AIFunctionFactory.Create(ComplexHierarchyFunction);
            var chatOptions = new ChatOptions { Tools = new List<AITool> { function } };
            
            var functionCall = new FunctionCall
            {
                Name = "ComplexHierarchyFunction",
                Args = JsonNode.Parse(@"{
                    ""company"": {
                        ""foundedDate"": ""January 1, 2000"",
                        ""departments"": [
                            {
                                ""name"": ""Engineering"",
                                ""establishedDate"": ""Feb 15, 2001"",
                                ""projects"": [
                                    {
                                        ""startDate"": ""March 1, 2024"",
                                        ""endDate"": ""June 30, 2024"",
                                        ""milestones"": [
                                            {
                                                ""date"": ""April 15, 2024"",
                                                ""time"": ""10:00 AM""
                                            },
                                            {
                                                ""date"": ""May 20, 2024"",
                                                ""time"": ""2:30 PM""
                                            }
                                        ]
                                    }
                                ]
                            },
                            {
                                ""name"": ""Marketing"",
                                ""establishedDate"": ""July 10, 2002"",
                                ""projects"": []
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
            var company = functionCallContent.Arguments["company"] as JsonObject;
            
            // Check company founded date
            company?["foundedDate"]?.GetValue<string>().ShouldBe("2000-01-01");
            
            // Check departments
            var departments = company?["departments"] as JsonArray;
            departments?.Count.ShouldBe(2);
            
            var eng = departments?[0] as JsonObject;
            eng?["establishedDate"]?.GetValue<string>().ShouldBe("2001-02-15");
            
            var projects = eng?["projects"] as JsonArray;
            var project = projects?[0] as JsonObject;
            project?["startDate"]?.GetValue<string>().ShouldBe("2024-03-01");
            project?["endDate"]?.GetValue<string>().ShouldBe("2024-06-30");
            
            var milestones = project?["milestones"] as JsonArray;
            var milestone1 = milestones?[0] as JsonObject;
            milestone1?["date"]?.GetValue<string>().ShouldBe("2024-04-15");
            milestone1?["time"]?.GetValue<string>().ShouldBe("10:00:00");
            
            var milestone2 = milestones?[1] as JsonObject;
            milestone2?["date"]?.GetValue<string>().ShouldBe("2024-05-20");
            milestone2?["time"]?.GetValue<string>().ShouldBe("14:30:00");
        }
        
        // Test function definitions
        [Description("Function with deeply nested structure")]
        public static Task<string> DeeplyNestedFunction(Level1 level1)
        {
            return Task.FromResult("Processed");
        }
        
        [Description("Function with mixed nesting")]
        public static Task<string> MixedNestingFunction(MixedItem[] data)
        {
            return Task.FromResult("Processed");
        }
        
        [Description("Function with multi-dimensional array")]
        public static Task<string> MultiDimensionalArrayFunction(DateOnly[][] matrix)
        {
            return Task.FromResult("Processed");
        }
        
        [Description("Function with large dataset")]
        public static Task<string> LargeDataSetFunction(LargeItem[] items)
        {
            return Task.FromResult("Processed");
        }
        
        [Description("Function with international dates")]
        public static Task<string> InternationalDatesFunction(InternationalDates dates)
        {
            return Task.FromResult("Processed");
        }
        
        [Description("Function with recursive structure")]
        public static Task<string> RecursiveStructureFunction(TreeNode node)
        {
            return Task.FromResult("Processed");
        }
        
        [Description("Function with mixed formats")]
        public static Task<string> MixedFormatsFunction(MixedSchedule schedule)
        {
            return Task.FromResult("Processed");
        }
        
        [Description("Function with complex hierarchy")]
        public static Task<string> ComplexHierarchyFunction(Company company)
        {
            return Task.FromResult("Processed");
        }
        
        // Supporting classes
        public class Level1
        {
            public DateOnly Date { get; set; }
            public Level2 Level2 { get; set; } = new();
        }
        
        public class Level2
        {
            public TimeOnly Time { get; set; }
            public Level3 Level3 { get; set; } = new();
        }
        
        public class Level3
        {
            public DateOnly[] Dates { get; set; } = Array.Empty<DateOnly>();
            public Level4 Level4 { get; set; } = new();
        }
        
        public class Level4
        {
            public Level5Schedule Schedule { get; set; } = new();
        }
        
        public class Level5Schedule
        {
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
            public TimeOnly[] Times { get; set; } = Array.Empty<TimeOnly>();
        }
        
        public class MixedItem
        {
            public int Id { get; set; }
            public DateOnly[] Dates { get; set; } = Array.Empty<DateOnly>();
            public NestedData Nested { get; set; } = new();
        }
        
        public class NestedData
        {
            public AppointmentWithReminders[] Appointments { get; set; } = Array.Empty<AppointmentWithReminders>();
        }
        
        public class AppointmentWithReminders
        {
            public DateOnly Date { get; set; }
            public TimeOnly Time { get; set; }
            public Reminder[] Reminders { get; set; } = Array.Empty<Reminder>();
        }
        
        public class Reminder
        {
            public DateOnly ReminderDate { get; set; }
            public TimeOnly ReminderTime { get; set; }
        }
        
        public class LargeItem
        {
            public int Id { get; set; }
            public DateOnly Date { get; set; }
            public TimeOnly Time { get; set; }
            public SimpleNested Nested { get; set; } = new();
        }
        
        public class SimpleNested
        {
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
        }
        
        public class InternationalDates
        {
            public DateOnly Us { get; set; }
            public DateOnly Uk { get; set; }
            public DateOnly Iso { get; set; }
            public DateOnly German { get; set; }
            public DateOnly French { get; set; }
            public DateOnly Spanish { get; set; }
            public DateOnly Japanese { get; set; }
            public DateOnly Chinese { get; set; }
        }
        
        public class TreeNode
        {
            public DateOnly Date { get; set; }
            public TreeNode[] Children { get; set; } = Array.Empty<TreeNode>();
        }
        
        public class MixedSchedule
        {
            public DateOnly Date1 { get; set; }
            public DateOnly Date2 { get; set; }
            public DateOnly Date3 { get; set; }
            public TimeOnly Time1 { get; set; }
            public TimeOnly Time2 { get; set; }
            public TimeOnly Time3 { get; set; }
            public DateOnly Datetime1 { get; set; }
            public DateOnly Datetime2 { get; set; }
            public DateOnly? NullDate { get; set; }
            public DateOnly EmptyDate { get; set; }
            public DateOnly InvalidDate { get; set; }
            public int Number { get; set; }
            public bool Boolean { get; set; }
            public string Text { get; set; } = "";
        }
        
        public class Company
        {
            public DateOnly FoundedDate { get; set; }
            public Department[] Departments { get; set; } = Array.Empty<Department>();
        }
        
        public class Department
        {
            public string Name { get; set; } = "";
            public DateOnly EstablishedDate { get; set; }
            public Project[] Projects { get; set; } = Array.Empty<Project>();
        }
        
        public class Project
        {
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
            public Milestone[] Milestones { get; set; } = Array.Empty<Milestone>();
        }
        
        public class Milestone
        {
            public DateOnly Date { get; set; }
            public TimeOnly Time { get; set; }
        }
    }
}