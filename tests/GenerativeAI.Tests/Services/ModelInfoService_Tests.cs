using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerativeAI.Services;
using Shouldly;
using Xunit.Abstractions;

namespace GenerativeAI.Tests.Services
{
    public class ModelInfoService_Tests
    {
        private ITestOutputHelper Console;
        public ModelInfoService_Tests(ITestOutputHelper helper)
        {
            Console = helper;
        }
        [Fact]
        public async Task ShouldGetModels()
        {
            var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

            var service = new ModelInfoService(apiKey);

            var models = await service.GetModelsAsync();

            models.ShouldNotBeNull();
            models.Count.ShouldBeGreaterThan(0);
            foreach (var modelInfo in models)
            {
                modelInfo.Name.ShouldNotBeNullOrEmpty();
                modelInfo.Description.ShouldNotBeNullOrEmpty();
                modelInfo.DisplayName.ShouldNotBeNullOrEmpty();
                modelInfo.InputTokenLimit.ShouldBeGreaterThan(0);
                modelInfo.OutputTokenLimit.ShouldBeGreaterThan(0);
                //modelInfo.Temperature.ShouldBeGreaterThan(0);
                //modelInfo.TopK.ShouldBeGreaterThan(0);
                //modelInfo.TopP.ShouldBeGreaterThan(0);
                modelInfo.Version.ShouldNotBeNullOrEmpty();
                modelInfo.SupportedGenerationMethods.ShouldNotBeNull();
                modelInfo.ModelId.ShouldNotBeNullOrEmpty();
                modelInfo.SupportedGenerationMethods.Count.ShouldBeGreaterThan(0);
                Console.WriteLine(modelInfo.Name);
                Console.WriteLine(modelInfo.ModelId);
                Console.WriteLine(modelInfo.DisplayName);
                Console.WriteLine(modelInfo.Description);
                Console.WriteLine("");

            }
        }

        [Fact]
        public async Task GetModelInfo()
        {
            var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

            var service = new ModelInfoService(apiKey);

            var modelInfo = await service.GetModelInfoAsync("gemini-pro");

            modelInfo.Name.ShouldNotBeNullOrEmpty();
            modelInfo.Description.ShouldNotBeNullOrEmpty();
            modelInfo.DisplayName.ShouldNotBeNullOrEmpty();
            modelInfo.InputTokenLimit.ShouldBeGreaterThan(0);
            modelInfo.OutputTokenLimit.ShouldBeGreaterThan(0);
            modelInfo.ModelId.ShouldNotBeNullOrEmpty();
            //modelInfo.Temperature.ShouldBeGreaterThan(0);
            //modelInfo.TopK.ShouldBeGreaterThan(0);
            //modelInfo.TopP.ShouldBeGreaterThan(0);
            modelInfo.Version.ShouldNotBeNullOrEmpty();
            modelInfo.SupportedGenerationMethods.ShouldNotBeNull();
            modelInfo.SupportedGenerationMethods.Count.ShouldBeGreaterThan(0);
            Console.WriteLine(modelInfo.Name);
            Console.WriteLine(modelInfo.ModelId);
            Console.WriteLine(modelInfo.DisplayName);
            Console.WriteLine(modelInfo.Description);
            Console.WriteLine("");
        }
    }
}
