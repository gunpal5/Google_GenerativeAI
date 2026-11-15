using GenerativeAI.Clients;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients
{
    public class BatchClient_Tests : TestBase
    {
        public BatchClient_Tests(ITestOutputHelper helper) : base(helper)
        {
        }

        [Fact]
        public async Task ShouldCreateBatchJob_WithGcsSource()
        {
            // GCS sources are only supported on Vertex AI, not Google AI
            Assert.Skip("Skip batch tests - GCS sources require Vertex AI platform and GCS bucket setup with real data files");

            var client = CreateClient();

            var source = new BatchJobSource
            {
                GcsUri = new List<string> { "gs://test-bucket/input.jsonl" }
            };

            var destination = new BatchJobDestination
            {
                GcsUri = "gs://test-bucket/output/"
            };

            var batchJob = await client.CreateAsync(
                GoogleAIModels.DefaultGeminiModel,
                source,
                destination,
                cancellationToken: TestContext.Current.CancellationToken);

            batchJob.ShouldNotBeNull();
            batchJob.Name.ShouldNotBeNullOrEmpty();
            batchJob.State.ShouldNotBeNull();
            Console?.WriteLine($"Created batch job: {batchJob.Name}");
            Console?.WriteLine($"State: {batchJob.State}");
        }

        [Fact]
        public async Task ShouldCreateBatchJob_WithInlinedRequests()
        {
            var client = CreateClient();
            //Assert.Skip("Skip batch tests - requires actual model access and can incur costs");

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
                    },
                    new InlinedRequest
                    {
                        Contents = new List<Content>
                        {
                            new Content
                            {
                                Parts = new List<Part>
                                {
                                    new Part { Text = "What is 2 + 2?" }
                                }
                            }
                        }
                    }
                }
            };

            var batchJob = await client.CreateAsync(
                GoogleAIModels.DefaultGeminiModel,
                source,
                cancellationToken: TestContext.Current.CancellationToken);

            batchJob.ShouldNotBeNull();
            batchJob.Name.ShouldNotBeNullOrEmpty();
            batchJob.State.ShouldNotBeNull();
            Console?.WriteLine($"Created batch job: {batchJob.Name}");
            Console?.WriteLine($"State: {batchJob.State}");
        }

        [Fact]
        public async Task ShouldCreateEmbeddingsBatchJob()
        {
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
                                    new Part { Text = "Hello world" }
                                }
                            }
                        }
                    }
                }
            };
            

            var batchJob = await client.CreateEmbeddingsAsync(
                GoogleAIModels.GeminiEmbedding,
                source,
                cancellationToken: TestContext.Current.CancellationToken);

            batchJob.ShouldNotBeNull();
            batchJob.Name.ShouldNotBeNullOrEmpty();
            batchJob.State.ShouldNotBeNull();
            Console?.WriteLine($"Created embeddings batch job: {batchJob.Name}");
            Console?.WriteLine($"State: {batchJob.State}");
        }

        [Fact]
        public async Task ShouldGetBatchJob()
        {
            var client = CreateClient();
            var ct = TestContext.Current.CancellationToken;
            var created = await CreateSmallInlineBatchJobAsync(client, ct);

            try
            {
                var batchJobResponse = await client.GetAsync(created.Name!, cancellationToken: ct);

                var batchJob = batchJobResponse.Metadata;
                batchJob.ShouldNotBeNull();
                batchJob.Name.ShouldBe(created.Name);
                batchJob.State.ShouldNotBeNull();
                Console?.WriteLine($"Job Name: {batchJob.Name}");
                Console?.WriteLine($"State: {batchJob.State}");
                Console?.WriteLine($"Create Time: {batchJob.CreateTime}");
            }
            finally
            {
                await TryCancelAsync(client, created.Name!);
            }
        }

        /// <summary>
        /// Creates a tiny inline batch job so tests don't depend on jobs that existed in someone's account.
        /// </summary>
        private static async Task<BatchJob> CreateSmallInlineBatchJobAsync(BatchClient client, CancellationToken ct)
        {
            var source = new BatchJobSource
            {
                InlinedRequests = new List<InlinedRequest>
                {
                    new InlinedRequest
                    {
                        Contents = new List<Content>
                        {
                            new Content { Parts = new List<Part> { new Part { Text = "What is 1 + 1?" } } }
                        }
                    }
                }
            };

            var job = await client.CreateAsync(GoogleAIModels.DefaultGeminiModel, source, cancellationToken: ct);
            job.ShouldNotBeNull();
            job.Name.ShouldNotBeNullOrEmpty();
            return job;
        }

        private static async Task TryCancelAsync(BatchClient client, string name)
        {
            try
            {
                await client.CancelAsync(name, cancellationToken: CancellationToken.None);
            }
            catch (ApiException)
            {
                // Already finished or cancelled.
            }
        }

        private static bool IsTerminal(JobState? state) =>
            state is JobState.JOB_STATE_SUCCEEDED or JobState.JOB_STATE_FAILED or JobState.JOB_STATE_CANCELLED
                or JobState.JOB_STATE_EXPIRED;

        [Fact]
        public async Task ShouldListBatchJobs()
        {
            var client = CreateClient();
            //Assert.Skip("Skip batch tests - list operation may return empty or require existing jobs");

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
            var client = CreateClient();
            //Assert.Skip("Skip batch tests - pagination requires multiple existing jobs");

            // Get first page
            var firstPage = await client.ListAsync(
                pageSize: 2,
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
            var client = CreateClient();

            // Create a batch job with two fast requests for testing cancellation
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
                                    new Part { Text = "What is 1 + 1?" }
                                }
                            }
                        }
                    },
                    new InlinedRequest
                    {
                        Contents = new List<Content>
                        {
                            new Content
                            {
                                Parts = new List<Part>
                                {
                                    new Part { Text = "What is 2 + 2?" }
                                }
                            }
                        }
                    }
                }
            };

            // Create the batch job
            var batchJob = await client.CreateAsync(
                GoogleAIModels.DefaultGeminiModel,
                source,
                cancellationToken: TestContext.Current.CancellationToken);

            batchJob.ShouldNotBeNull();
            batchJob.Name.ShouldNotBeNullOrEmpty();
            Console?.WriteLine($"Created batch job: {batchJob.Name}");

            // Immediately cancel it
            await client.CancelAsync(batchJob.Name!, cancellationToken: TestContext.Current.CancellationToken);
            Console?.WriteLine($"Cancelled batch job: {batchJob.Name}");

            // Verify cancellation
            var job = await client.GetAsync(batchJob.Name!, cancellationToken: TestContext.Current.CancellationToken);
            Console?.WriteLine($"Job state after cancel: {job.Metadata?.State}");

            // Job should be cancelled or cancelling
            (job.Metadata?.State == JobState.JOB_STATE_CANCELLED ||
             job.Metadata?.State == JobState.JOB_STATE_CANCELLING).ShouldBeTrue();
        }

        [Fact]
        public async Task ShouldDeleteBatchJob()
        {
            var client = CreateClient();
            var ct = TestContext.Current.CancellationToken;
            var created = await CreateSmallInlineBatchJobAsync(client, ct);
            var jobName = created.Name!;

            // Only finished jobs can be deleted: cancel, then wait for a terminal state.
            await TryCancelAsync(client, jobName);
            var deadline = DateTime.UtcNow.AddMinutes(3);
            while (true)
            {
                var state = (await client.GetAsync(jobName, cancellationToken: ct)).Metadata?.State;
                if (IsTerminal(state))
                    break;
                if (DateTime.UtcNow > deadline)
                    throw new TimeoutException($"Batch job {jobName} did not finish cancelling (state: {state}).");
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }

            await client.DeleteBatchJobAsync(jobName, cancellationToken: ct);
            Console?.WriteLine($"Deleted batch job: {jobName}");

            var ex = await Should.ThrowAsync<ApiException>(() => client.GetAsync(jobName, cancellationToken: ct));
            ex.ErrorCode.ShouldBe(404);
        }

        [Fact]
        public void ShouldThrowException_WhenModelIsNull()
        {
            var client = CreateClient();
            // Use FileName for Google AI compatibility
            var source = new BatchJobSource { FileName = "files/test-input" };

            Should.Throw<ArgumentException>(async () =>
                await client.CreateAsync(null!, source, cancellationToken: TestContext.Current.CancellationToken));
        }

        [Fact]
        public void ShouldThrowException_WhenModelIsEmpty()
        {
            var client = CreateClient();
            // Use FileName for Google AI compatibility
            var source = new BatchJobSource { FileName = "files/test-input" };

            Should.Throw<ArgumentException>(async () =>
                await client.CreateAsync("", source, cancellationToken: TestContext.Current.CancellationToken));
        }

        [Fact]
        public void ShouldThrowException_WhenSourceIsNull()
        {
            var client = CreateClient();

            Should.Throw<ArgumentNullException>(async () =>
                await client.CreateAsync(GoogleAIModels.DefaultGeminiModel, null!, cancellationToken: TestContext.Current.CancellationToken));
        }

        [Fact]
        public void ShouldThrowException_WhenJobNameIsNull_OnGet()
        {
            var client = CreateClient();

            Should.Throw<ArgumentException>(async () =>
                await client.GetAsync(null!, cancellationToken: TestContext.Current.CancellationToken));
        }

        [Fact]
        public void ShouldThrowException_WhenJobNameIsEmpty_OnGet()
        {
            var client = CreateClient();

            Should.Throw<ArgumentException>(async () =>
                await client.GetAsync("", cancellationToken: TestContext.Current.CancellationToken));
        }

        [Fact]
        public void ShouldThrowException_WhenJobNameIsNull_OnCancel()
        {
            var client = CreateClient();

            Should.Throw<ArgumentException>(async () =>
                await client.CancelAsync(null!, cancellationToken: TestContext.Current.CancellationToken));
        }

        [Fact]
        public void ShouldThrowException_WhenJobNameIsNull_OnDelete()
        {
            var client = CreateClient();

            Should.Throw<ArgumentException>(async () =>
                await client.DeleteBatchJobAsync(null!, cancellationToken: TestContext.Current.CancellationToken));
        }

        public BatchClient CreateClient()
        {
            Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);
            return new BatchClient(GetTestGooglePlatform());
        }
    }
}
