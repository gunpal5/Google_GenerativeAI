using System.Net;
using System.Text;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients;

/// <summary>
/// Error responses from Google must surface as <see cref="ApiException"/> with the API's details,
/// whether the body is an object or (as some endpoints return) an array.
/// </summary>
public class ApiErrorHandling_Tests
{
    private sealed class ErrorHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _body;

        public ErrorHandler(HttpStatusCode status, string body)
        {
            _status = status;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            });
    }

    private static Task<EmbedContentResponse> EmbedWith(HttpStatusCode status, string body)
    {
        var model = new EmbeddingModel(new GoogleAIPlatformAdapter("test-key"), GoogleAIModels.GeminiEmbedding,
            new HttpClient(new ErrorHandler(status, body)));
        return model.EmbedContentAsync(new EmbedContentRequest { Content = new Content("hi", Roles.User) },
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ObjectErrorBody_ThrowsApiException()
    {
        var ex = await Should.ThrowAsync<ApiException>(() => EmbedWith(HttpStatusCode.BadRequest,
            """{"error":{"code":400,"message":"Invalid argument","status":"INVALID_ARGUMENT"}}"""));

        ex.ErrorCode.ShouldBe(400);
        ex.ErrorMessage.ShouldBe("Invalid argument");
        ex.ErrorStatus.ShouldBe("INVALID_ARGUMENT");
    }

    [Fact]
    public async Task ArrayErrorBody_ThrowsApiException()
    {
        var ex = await Should.ThrowAsync<ApiException>(() => EmbedWith(HttpStatusCode.PaymentRequired,
            """[{"error":{"code":402,"message":"Your prepayment credits are depleted.","status":"RESOURCE_EXHAUSTED"}}]"""));

        ex.ErrorCode.ShouldBe(402);
        ex.ErrorMessage.ShouldContain("credits are depleted");
        ex.ErrorStatus.ShouldBe("RESOURCE_EXHAUSTED");
    }

    [Fact]
    public async Task ErrorWithoutNumericCode_UsesHttpStatusCode()
    {
        var ex = await Should.ThrowAsync<ApiException>(() => EmbedWith(HttpStatusCode.TooManyRequests,
            """{"error":{"message":"Slow down","status":"RESOURCE_EXHAUSTED"}}"""));

        ex.ErrorCode.ShouldBe(429);
    }

    [Fact]
    public async Task NonJsonBody_ThrowsHttpRequestExceptionWithStatusAndBody()
    {
        var ex = await Should.ThrowAsync<HttpRequestException>(() => EmbedWith(HttpStatusCode.BadGateway,
            "<html>upstream error</html>"));

        ex.Message.ShouldContain("502");
        ex.Message.ShouldContain("upstream error");
        ex.Message.ShouldNotContain("test-key");
    }
}
