using GenerativeAI.Authenticators;
using GenerativeAI.Clients;
using GenerativeAI.Tests.Helpers;
using GenerativeAI.Types;
using Shouldly;
using Environment = System.Environment;

namespace GenerativeAI.Tests.Platforms;

public class VertexBatchClient_Tests : TestBase
{
    public VertexBatchClient_Tests(ITestOutputHelper helper) : base(helper)
    {
    }

    private bool IsGcsConfigured =>
        !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("GCS_TEST_BUCKET")) &&
        !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID"));

    [Fact]
    public void ShouldThrowException_WhenUsingInlinedRequests()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);

        var client = CreateClient();

        var source = new BatchJobSource
        {
            InlinedRequests = new List<InlinedRequest>
            {
                new InlinedRequest
                {
                    Contents = new List<Content>
                    {
                        new Content
                        {
                            Parts = new List<Part>
                            {
                                new Part { Text = "What is the capital of France?" }
                            }
                        }
                    }
                }
            }
        };

        var destination = new BatchJobDestination
        {
            GcsUri = "gs://your-bucket/output/"
        };

        Should.Throw<NotSupportedException>(async () =>
            await client.CreateAsync(
                VertexAIModels.Gemini.Gemini38Flash,
                source,
                destination,
                cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ShouldCreateBatchJob_WithGcsSource()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);
        Assert.SkipUnless(IsGcsConfigured, "Skip - GCS not configured. Set GCS_TEST_BUCKET and GOOGLE_PROJECT_ID environment variables");

        var client = CreateClient();

        // Use GcsTestHelper to upload test data
        using var gcsHelper = new GcsTestHelper();

        // Upload sample requests to GCS
        var inputUri = await gcsHelper.UploadSampleContentRequestsAsync();
        var outputUriPrefix = gcsHelper.GetOutputUriPrefix();

        Console?.WriteLine($"Input URI: {inputUri}");
        Console?.WriteLine($"Output URI: {outputUriPrefix}");

        // Verify the uploaded file
        var verified = await gcsHelper.VerifyUploadedFileAsync(inputUri);
        verified.ShouldBeTrue("Failed to verify uploaded file");

        var source = new BatchJobSource
        {
            GcsUri = new List<string> { inputUri }
        };

        var destination = new BatchJobDestination
        {
            GcsUri = outputUriPrefix
        };

        var batchJob = await client.CreateAsync(
            VertexAIModels.Gemini.Gemini38Flash,
            source,
            destination,
            cancellationToken: TestContext.Current.CancellationToken);

        batchJob.ShouldNotBeNull();
        batchJob.Name.ShouldNotBeNullOrEmpty();
        batchJob.State.ShouldNotBeNull();
        Console?.WriteLine($"Created batch job: {batchJob.Name}");
        Console?.WriteLine($"State: {batchJob.State}");

        // Cleanup happens automatically via Dispose
    }

    [Fact]
    public async Task ShouldGetBatchJob()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);
       // Assert.Skip("Skip - GetAsync for Vertex AI not yet implemented");

        var client = CreateClient();
        var jobName = "projects/880800811905/locations/us-central1/batchPredictionJobs/429950226606850048"; // Replace with actual job name

        var response = await client.GetAsync(jobName, cancellationToken: TestContext.Current.CancellationToken);

        response.ShouldNotBeNull();
        response.Metadata.ShouldNotBeNull();
        response.Metadata!.Name.ShouldBe(jobName);
        response.Metadata.State.ShouldNotBeNull();
        Console?.WriteLine($"Job Name: {response.Metadata.Name}");
        Console?.WriteLine($"State: {response.Metadata.State}");
        Console?.WriteLine($"Create Time: {response.Metadata.CreateTime}");
        Console?.WriteLine($"Update Time: {response.Metadata.UpdateTime}");

        if (response.Metadata.StartTime != null)
            Console?.WriteLine($"Start Time: {response.Metadata.StartTime}");

        if (response.Metadata.EndTime != null)
            Console?.WriteLine($"End Time: {response.Metadata.EndTime}");
    }

    [Fact]
    public async Task ShouldListBatchJobs()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);

        var client = CreateClient();

        var response = await client.ListAsync(
            pageSize: 10,
            cancellationToken: TestContext.Current.CancellationToken);

        response.ShouldNotBeNull();
        Console?.WriteLine($"Retrieved {response.BatchJobs?.Count ?? 0} batch jobs");

        if (response.BatchJobs != null && response.BatchJobs.Count > 0)
        {
            foreach (var job in response.BatchJobs)
            {
                Console?.WriteLine($"Job: {job.Name}, State: {job.State}");
            }

            if (!string.IsNullOrEmpty(response.NextPageToken))
            {
                Console?.WriteLine($"Next page token: {response.NextPageToken}");
            }
        }
    }

    [Fact]
    public async Task ShouldListBatchJobs_WithPagination()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);
        //Assert.Skip("Skip batch tests - pagination requires multiple existing jobs");

        var client = CreateClient();

        // Get first page
        var firstPage = await client.ListAsync(
            pageSize: 1,
            cancellationToken: TestContext.Current.CancellationToken);

        firstPage.ShouldNotBeNull();
        Console?.WriteLine($"First page: {firstPage.BatchJobs?.Count ?? 0} jobs");

        if (!string.IsNullOrEmpty(firstPage.NextPageToken))
        {
            // Get second page
            var secondPage = await client.ListAsync(
                pageSize: 2,
                pageToken: firstPage.NextPageToken,
                cancellationToken: TestContext.Current.CancellationToken);

            secondPage.ShouldNotBeNull();
            Console?.WriteLine($"Second page: {secondPage.BatchJobs?.Count ?? 0} jobs");
        }
    }

    [Fact]
    public async Task ShouldCancelBatchJob()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);
        Assert.SkipUnless(IsGcsConfigured, "Skip - GCS not configured. Set GCS_TEST_BUCKET and GOOGLE_PROJECT_ID environment variables");
        var ct = TestContext.Current.CancellationToken;

        var client = CreateClient();
        using var gcsHelper = new GcsTestHelper();
        var jobName = (await CreateSmallGcsBatchJobAsync(client, gcsHelper, ct)).Name!;

        await client.CancelAsync(jobName, cancellationToken: ct);
        Console?.WriteLine($"Cancelled batch job: {jobName}");

        // Vertex applies the cancellation asynchronously; poll briefly for the state change.
        JobState? state = null;
        for (var attempt = 0; attempt < 12; attempt++)
        {
            state = (await client.GetAsync(jobName, cancellationToken: ct)).Metadata?.State;
            if (state is JobState.JOB_STATE_CANCELLING or JobState.JOB_STATE_CANCELLED)
                break;
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }

        (state is JobState.JOB_STATE_CANCELLING or JobState.JOB_STATE_CANCELLED).ShouldBeTrue($"Unexpected state {state}");
    }

    [Fact]
    public async Task ShouldDeleteBatchJob()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);
        Assert.SkipUnless(IsGcsConfigured, "Skip - GCS not configured. Set GCS_TEST_BUCKET and GOOGLE_PROJECT_ID environment variables");
        var ct = TestContext.Current.CancellationToken;

        var client = CreateClient();
        using var gcsHelper = new GcsTestHelper();
        var jobName = (await CreateSmallGcsBatchJobAsync(client, gcsHelper, ct)).Name!;

        // Only finished jobs can be deleted: cancel, then wait for a terminal state.
        await client.CancelAsync(jobName, cancellationToken: ct);
        var deadline = DateTime.UtcNow.AddMinutes(5);
        while (true)
        {
            var state = (await client.GetAsync(jobName, cancellationToken: ct)).Metadata?.State;
            if (state is JobState.JOB_STATE_CANCELLED or JobState.JOB_STATE_FAILED or JobState.JOB_STATE_SUCCEEDED
                or JobState.JOB_STATE_EXPIRED)
                break;
            if (DateTime.UtcNow > deadline)
                throw new TimeoutException($"Batch job {jobName} did not finish cancelling (state: {state}).");
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
        }

        await client.DeleteBatchJobAsync(jobName, cancellationToken: ct);
        Console?.WriteLine($"Deleted batch job: {jobName}");
    }

    /// <summary>
    /// Creates a small batch job from sample requests uploaded to the test bucket, so tests don't depend on
    /// job IDs that existed in someone's project.
    /// </summary>
    private static async Task<BatchJob> CreateSmallGcsBatchJobAsync(BatchClient client, GcsTestHelper gcsHelper,
        CancellationToken ct)
    {
        var inputUri = await gcsHelper.UploadSampleContentRequestsAsync();
        var job = await client.CreateAsync(
            VertexAIModels.Gemini.Gemini38Flash,
            new BatchJobSource { GcsUri = new List<string> { inputUri } },
            new BatchJobDestination { GcsUri = gcsHelper.GetOutputUriPrefix() },
            cancellationToken: ct);
        job.Name.ShouldNotBeNullOrEmpty();
        return job;
    }

    [Fact]
    public void ShouldThrowException_WhenModelIsNull()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);

        var client = CreateClient();
        var source = new BatchJobSource { GcsUri = new List<string> { "gs://test-bucket/input.jsonl" } };
        var destination = new BatchJobDestination { GcsUri = "gs://test-bucket/output/" };

        Should.Throw<ArgumentException>(async () =>
            await client.CreateAsync(null!, source, destination, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldThrowException_WhenModelIsEmpty()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);

        var client = CreateClient();
        var source = new BatchJobSource { GcsUri = new List<string> { "gs://test-bucket/input.jsonl" } };
        var destination = new BatchJobDestination { GcsUri = "gs://test-bucket/output/" };

        Should.Throw<ArgumentException>(async () =>
            await client.CreateAsync("", source, destination, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldThrowException_WhenSourceIsNull()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);

        var client = CreateClient();
        var destination = new BatchJobDestination { GcsUri = "gs://test-bucket/output/" };

        Should.Throw<ArgumentNullException>(async () =>
            await client.CreateAsync(VertexAIModels.Gemini.Gemini38Flash, null!, destination, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldThrowException_WhenJobNameIsNull_OnGet()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);

        var client = CreateClient();

        Should.Throw<ArgumentException>(async () =>
            await client.GetAsync(null!, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldThrowException_WhenJobNameIsEmpty_OnGet()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);

        var client = CreateClient();

        Should.Throw<ArgumentException>(async () =>
            await client.GetAsync("", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldThrowException_WhenJobNameIsNull_OnCancel()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);

        var client = CreateClient();

        Should.Throw<ArgumentException>(async () =>
            await client.CancelAsync(null!, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldThrowException_WhenJobNameIsNull_OnDelete()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);

        var client = CreateClient();

        Should.Throw<ArgumentException>(async () =>
            await client.DeleteBatchJobAsync(null!, cancellationToken: TestContext.Current.CancellationToken));
    }

    public BatchClient CreateClient()
    {
        var adapter = GetTestVertexAIPlatform();
      //  adapter.SetAuthenticator(new GoogleServiceAccountAuthenticator(Environment.GetEnvironmentVariable("GOOGLE_SERVICE_ACCOUNT_JSON")));
        return new BatchClient(adapter);
    }
}
