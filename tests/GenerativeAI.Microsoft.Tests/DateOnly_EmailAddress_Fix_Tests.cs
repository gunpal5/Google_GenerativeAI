using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using GenerativeAI.Microsoft;
using GenerativeAI.Microsoft.Extensions;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;
using GenerativeAI.Tests;

namespace GenerativeAI.IntegrationTests;

public class DateOnly_EmailAddress_Fix_Tests : TestBase
{
    public DateOnly_EmailAddress_Fix_Tests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public async Task ShouldWorkWith_DateOnly_Properties()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey, modelName: "models/gemini-2.0-flash");
        var chatOptions = new ChatOptions();

        chatOptions.Tools = new List<AITool> { AIFunctionFactory.Create(GetUserDetailsAsync) };
        var message = new ChatMessage(ChatRole.User, "Get user details for John Doe with birthdate 1990-01-15");
        
        // This should not throw an exception about unsupported format
        var response = await chatClient.GetResponseAsync(message, options: chatOptions, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.Text.ShouldContain("John", Case.Insensitive);
    }

    [Fact]
    public async Task ShouldWorkWith_EmailAddress_DataAnnotation()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey, modelName: "models/gemini-2.0-flash");
        var chatOptions = new ChatOptions();

        chatOptions.Tools = new List<AITool> { AIFunctionFactory.Create(GetContactInfoAsync) };
        var message = new ChatMessage(ChatRole.User, "Get contact info for user@example.com");
        
        // This should not throw an exception about unsupported format
        var response = await chatClient.GetResponseAsync(message, options: chatOptions, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.Text.ShouldContain("contact", Case.Insensitive);
    }

    [Description("Get user details including birthdate")]
    public Task<UserDetails> GetUserDetailsAsync(string name, DateOnly birthDate)
    {
        return Task.FromResult(new UserDetails
        {
            Name = name,
            BirthDate = birthDate,
            RegistrationDate = DateOnly.FromDateTime(DateTime.Now),
            LastLoginTime = TimeOnly.FromDateTime(DateTime.Now)
        });
    }

    [Description("Get contact information")]
    public Task<ContactInfo> GetContactInfoAsync(ContactRequest request)
    {
        return Task.FromResult(new ContactInfo
        {
            Email = request.Email,
            Phone = request.Phone,
            IsVerified = true
        });
    }

    public class UserDetails
    {
        public string Name { get; set; } = string.Empty;
        public DateOnly BirthDate { get; set; }
        public DateOnly RegistrationDate { get; set; }
        public TimeOnly LastLoginTime { get; set; }
    }

    public class ContactRequest
    {
        [EmailAddress]
        [Description("User's email address")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Description("User's phone number")]
        public string Phone { get; set; } = string.Empty;
    }

    public class ContactInfo
    {
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
    }
}