using System.Text.Json;
using GenerativeAI.Clients;
using GenerativeAI.Helpers;
using GenerativeAI.Tests.Helpers;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Batch;

/// <summary>
/// Tests for Issue #101: Batch Inference features including:
/// - BatchJsonLGenerator with schema support from C# types
/// - BatchOutputInfo for getting output location from completed jobs
/// - InlinedRequest with ResponseSchema for structured output
/// </summary>
public class Issue101_BatchInference_Tests : TestBase
{
    public Issue101_BatchInference_Tests(ITestOutputHelper helper) : base(helper)
    {
    }

    #region Test Schema Classes

    /// <summary>
    /// Sample schema class for testing batch inference with structured output.
    /// </summary>
    public class TestStructure
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Another sample schema for testing.
    /// </summary>
    public class ProductInfo
    {
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public string? Category { get; set; }
        public bool InStock { get; set; }
    }

    #endregion

    #region BatchJsonLGenerator Schema Tests

    [Fact]
    public void BatchJsonLGenerator_ShouldGenerateJsonL_WithSchemaFromType()
    {
        // Arrange
        var prompts = new[] { "Describe a person named John who is 30 years old." };

        // Act
        var jsonl = BatchJsonLGenerator.GenerateContentJsonL<TestStructure>(prompts);

        // Assert
        jsonl.ShouldNotBeNullOrEmpty();
        Console?.WriteLine("Generated JSONL:");
        Console?.WriteLine(jsonl);

        // Parse and verify structure
        var lines = jsonl.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.ShouldBe(1);

        var parsed = JsonSerializer.Deserialize<JsonElement>(lines[0]);

        // Should have request wrapper
        parsed.TryGetProperty("request", out var request).ShouldBeTrue();

        // Should have contents
        request.TryGetProperty("contents", out var contents).ShouldBeTrue();
        contents.GetArrayLength().ShouldBeGreaterThan(0);

        // Should have generationConfig with responseSchema
        request.TryGetProperty("generationConfig", out var genConfig).ShouldBeTrue();
        genConfig.TryGetProperty("responseMimeType", out var mimeType).ShouldBeTrue();
        mimeType.GetString().ShouldBe("application/json");
        genConfig.TryGetProperty("responseSchema", out var schema).ShouldBeTrue();

        // Schema should have type and properties
        schema.TryGetProperty("type", out _).ShouldBeTrue();
    }

    [Fact]
    public void BatchJsonLGenerator_ShouldGenerateJsonL_WithMultiplePrompts()
    {
        // Arrange
        var prompts = new[]
        {
            "Extract product info: iPhone 15 Pro costs $999 in Electronics",
            "Extract product info: Nike Air Max costs $150 in Footwear",
            "Extract product info: Sony Headphones costs $299 in Electronics"
        };

        // Act
        var jsonl = BatchJsonLGenerator.GenerateContentJsonL<ProductInfo>(prompts);

        // Assert
        var lines = jsonl.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.ShouldBe(3);

        Console?.WriteLine($"Generated {lines.Length} JSONL lines for batch processing");
        foreach (var line in lines)
        {
            var parsed = JsonSerializer.Deserialize<JsonElement>(line);
            parsed.TryGetProperty("request", out var request).ShouldBeTrue();
            request.TryGetProperty("generationConfig", out var genConfig).ShouldBeTrue();
            genConfig.TryGetProperty("responseSchema", out _).ShouldBeTrue();
        }
    }

    [Fact]
    public void BatchJsonLGenerator_ShouldGenerateJsonL_WithSystemInstruction()
    {
        // Arrange
        var prompts = new[] { "Analyze this person's profile" };
        var systemInstruction = new Content
        {
            Parts = new List<Part>
            {
                new Part { Text = "You are a data extraction assistant. Always return structured JSON." }
            }
        };

        // Act
        var jsonl = BatchJsonLGenerator.GenerateContentJsonL<TestStructure>(
            prompts,
            systemInstruction: systemInstruction);

        // Assert
        jsonl.ShouldNotBeNullOrEmpty();
        var parsed = JsonSerializer.Deserialize<JsonElement>(jsonl.Trim());
        parsed.TryGetProperty("request", out var request).ShouldBeTrue();
        request.TryGetProperty("systemInstruction", out var sysInst).ShouldBeTrue();

        Console?.WriteLine("Generated JSONL with system instruction:");
        Console?.WriteLine(jsonl);
    }

    [Fact]
    public void BatchJsonLGenerator_ShouldGenerateJsonL_WithFilesAndSchema()
    {
        // Arrange
        var requests = new[]
        {
            new BatchRequestItem("Extract product info from this image", "gs://my-bucket/product1.jpg", "image/jpeg"),
            new BatchRequestItem("Extract product info from this document", "gs://my-bucket/product2.pdf", "application/pdf")
        };

        // Act
        var jsonl = BatchJsonLGenerator.GenerateContentWithFilesJsonL<ProductInfo>(requests);

        // Assert
        var lines = jsonl.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.ShouldBe(2);

        Console?.WriteLine("Generated JSONL with files:");
        foreach (var line in lines)
        {
            Console?.WriteLine(line);
            var parsed = JsonSerializer.Deserialize<JsonElement>(line);

            // Should have request wrapper
            parsed.TryGetProperty("request", out var request).ShouldBeTrue();
            request.TryGetProperty("contents", out var contents).ShouldBeTrue();
            request.TryGetProperty("generationConfig", out var genConfig).ShouldBeTrue();
            genConfig.TryGetProperty("responseSchema", out _).ShouldBeTrue();
        }
    }

    [Fact]
    public void BatchJsonLGenerator_ShouldGenerateJsonL_WithoutSchema()
    {
        // Arrange
        var prompts = new[] { "What is the capital of France?" };

        // Act - using BatchOptions without schema
        var jsonl = BatchJsonLGenerator.GenerateContentJsonL(prompts);

        // Assert
        jsonl.ShouldNotBeNullOrEmpty();
        var parsed = JsonSerializer.Deserialize<JsonElement>(jsonl.Trim());

        // Should have request wrapper but NOT have generationConfig when no schema
        parsed.TryGetProperty("request", out var request).ShouldBeTrue();
        request.TryGetProperty("generationConfig", out _).ShouldBeFalse();

        Console?.WriteLine("Generated JSONL without schema:");
        Console?.WriteLine(jsonl);
    }

    #endregion

    #region BatchOutputInfo Tests

    [Fact]
    public void BatchOutputInfo_ShouldDeserialize_GcsOutputDirectory()
    {
        // Arrange - simulating Vertex AI batch job response
        var json = @"{
            ""name"": ""projects/123/locations/us-central1/batchPredictionJobs/456"",
            ""displayName"": ""Test Batch Job"",
            ""state"": ""JOB_STATE_SUCCEEDED"",
            ""outputInfo"": {
                ""gcsOutputDirectory"": ""gs://my-bucket/batch-output/prediction-model-2026-01-07T07:22:13.735709Z""
            }
        }";

        // Act
        var batchJob = JsonSerializer.Deserialize<BatchJob>(json);

        // Assert
        batchJob.ShouldNotBeNull();
        batchJob.OutputInfo.ShouldNotBeNull();
        batchJob.OutputInfo!.GcsOutputDirectory.ShouldNotBeNullOrEmpty();
        batchJob.OutputInfo.GcsOutputDirectory.ShouldContain("gs://my-bucket/batch-output/");

        Console?.WriteLine($"Output Directory: {batchJob.OutputInfo.GcsOutputDirectory}");
    }

    [Fact]
    public void BatchOutputInfo_ShouldDeserialize_BigQueryOutput()
    {
        // Arrange - simulating Vertex AI batch job response with BigQuery output
        var json = @"{
            ""name"": ""projects/123/locations/us-central1/batchPredictionJobs/789"",
            ""displayName"": ""Test Batch Job BQ"",
            ""state"": ""JOB_STATE_SUCCEEDED"",
            ""outputInfo"": {
                ""bigqueryOutputDataset"": ""bq://my-project.my_dataset"",
                ""bigqueryOutputTable"": ""predictions_20260107_072213""
            }
        }";

        // Act
        var batchJob = JsonSerializer.Deserialize<BatchJob>(json);

        // Assert
        batchJob.ShouldNotBeNull();
        batchJob.OutputInfo.ShouldNotBeNull();
        batchJob.OutputInfo!.BigqueryOutputDataset.ShouldBe("bq://my-project.my_dataset");
        batchJob.OutputInfo.BigqueryOutputTable.ShouldBe("predictions_20260107_072213");

        Console?.WriteLine($"BigQuery Dataset: {batchJob.OutputInfo.BigqueryOutputDataset}");
        Console?.WriteLine($"BigQuery Table: {batchJob.OutputInfo.BigqueryOutputTable}");
    }

    [Fact]
    public void BatchJob_ShouldDeserialize_InputAndOutputConfig()
    {
        // Arrange - full Vertex AI batch job response
        var json = @"{
            ""name"": ""projects/123/locations/us-central1/batchPredictionJobs/456"",
            ""displayName"": ""Full Batch Job"",
            ""state"": ""JOB_STATE_SUCCEEDED"",
            ""model"": ""publishers/google/models/gemini-2.0-flash"",
            ""inputConfig"": {
                ""instancesFormat"": ""jsonl"",
                ""gcsSource"": {
                    ""uris"": [""gs://my-bucket/input/requests.jsonl""]
                }
            },
            ""outputConfig"": {
                ""predictionsFormat"": ""jsonl"",
                ""gcsDestination"": {
                    ""outputUriPrefix"": ""gs://my-bucket/output/""
                }
            },
            ""outputInfo"": {
                ""gcsOutputDirectory"": ""gs://my-bucket/output/prediction-model-2026-01-07/""
            }
        }";

        // Act
        var batchJob = JsonSerializer.Deserialize<BatchJob>(json);

        // Assert
        batchJob.ShouldNotBeNull();
        batchJob.Model.ShouldBe("publishers/google/models/gemini-2.0-flash");

        // Input config
        batchJob.InputConfig.ShouldNotBeNull();
        batchJob.InputConfig!.InstancesFormat.ShouldBe("jsonl");
        batchJob.InputConfig.GcsSource.ShouldNotBeNull();

        // Output config
        batchJob.OutputConfig.ShouldNotBeNull();
        batchJob.OutputConfig!.PredictionsFormat.ShouldBe("jsonl");
        batchJob.OutputConfig.GcsDestination.ShouldNotBeNull();

        // Output info (the actual output location)
        batchJob.OutputInfo.ShouldNotBeNull();
        batchJob.OutputInfo!.GcsOutputDirectory.ShouldContain("prediction-model");

        Console?.WriteLine($"Input: {batchJob.InputConfig.GcsSource?.Uris?.FirstOrDefault()}");
        Console?.WriteLine($"Output Config: {batchJob.OutputConfig.GcsDestination?.OutputUriPrefix}");
        Console?.WriteLine($"Actual Output: {batchJob.OutputInfo.GcsOutputDirectory}");
    }

    #endregion

    #region InlinedRequest with Schema Tests

    [Fact]
    public void InlinedRequest_ShouldSupportResponseSchema()
    {
        // Arrange - Create InlinedRequest with schema like GenerateObjectAsync
        var schema = GoogleSchemaHelper.ConvertToSchema(typeof(TestStructure));

        var inlinedRequest = new InlinedRequest
        {
            Contents = new List<Content>
            {
                new Content
                {
                    Role = "user",
                    Parts = new List<Part>
                    {
                        new Part { Text = "Describe a person named Alice who is 25 years old." }
                    }
                }
            },
            GenerationConfig = new GenerationConfig
            {
                ResponseMimeType = "application/json",
                ResponseSchema = schema
            }
        };

        // Assert
        inlinedRequest.GenerationConfig.ShouldNotBeNull();
        inlinedRequest.GenerationConfig!.ResponseMimeType.ShouldBe("application/json");
        inlinedRequest.GenerationConfig.ResponseSchema.ShouldNotBeNull();

        // Serialize to verify structure
        var json = JsonSerializer.Serialize(inlinedRequest, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        Console?.WriteLine("InlinedRequest with schema:");
        Console?.WriteLine(json);

        json.ShouldContain("responseSchema");
        json.ShouldContain("application/json");
    }

    [Fact]
    public void BatchJobSource_ShouldAcceptInlinedRequestsWithSchema()
    {
        // Arrange
        var schema = GoogleSchemaHelper.ConvertToSchema(typeof(ProductInfo));

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
                            Role = "user",
                            Parts = new List<Part>
                            {
                                new Part { Text = "Extract: iPhone 15 Pro, $999, Electronics, in stock" }
                            }
                        }
                    },
                    GenerationConfig = new GenerationConfig
                    {
                        ResponseMimeType = "application/json",
                        ResponseSchema = schema
                    }
                },
                new InlinedRequest
                {
                    Contents = new List<Content>
                    {
                        new Content
                        {
                            Role = "user",
                            Parts = new List<Part>
                            {
                                new Part { Text = "Extract: MacBook Air, $1299, Computers, out of stock" }
                            }
                        }
                    },
                    GenerationConfig = new GenerationConfig
                    {
                        ResponseMimeType = "application/json",
                        ResponseSchema = schema
                    }
                }
            }
        };

        // Assert
        source.InlinedRequests.ShouldNotBeNull();
        source.InlinedRequests!.Count.ShouldBe(2);

        foreach (var request in source.InlinedRequests)
        {
            request.GenerationConfig.ShouldNotBeNull();
            request.GenerationConfig!.ResponseSchema.ShouldNotBeNull();
        }

        Console?.WriteLine($"Created BatchJobSource with {source.InlinedRequests.Count} schema-enabled requests");
    }

    [Fact]
    public void GenerateFromInlinedRequests_ShouldIncludeSchema()
    {
        // Arrange
        var schema = GoogleSchemaHelper.ConvertToSchema(typeof(TestStructure));

        var requests = new List<InlinedRequest>
        {
            new InlinedRequest
            {
                Contents = new List<Content>
                {
                    new Content
                    {
                        Role = "user",
                        Parts = new List<Part>
                        {
                            new Part { Text = "Describe person: Bob, 40" }
                        }
                    }
                },
                GenerationConfig = new GenerationConfig
                {
                    ResponseMimeType = "application/json",
                    ResponseSchema = schema
                }
            }
        };

        // Act
        var jsonl = BatchJsonLGenerator.GenerateFromInlinedRequests(requests);

        // Assert
        jsonl.ShouldNotBeNullOrEmpty();

        Console?.WriteLine("Generated InlinedRequest JSONL:");
        Console?.WriteLine(jsonl);

        var parsed = JsonSerializer.Deserialize<JsonElement>(jsonl.Trim());
        parsed.TryGetProperty("request", out var request).ShouldBeTrue();
        request.TryGetProperty("generationConfig", out var genConfig).ShouldBeTrue();
        genConfig.TryGetProperty("responseSchema", out _).ShouldBeTrue();
        genConfig.TryGetProperty("responseMimeType", out var mimeType).ShouldBeTrue();
        mimeType.GetString().ShouldBe("application/json");
    }

    #endregion

    #region Integration Tests (Require API Access)

    private bool IsGcsConfigured =>
        !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("GCS_TEST_BUCKET")) &&
        !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID"));

    [Fact]
    public async Task ShouldCreateBatchJob_WithSchemaInlinedRequests_GoogleAI()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        var client = new BatchClient(GetTestGooglePlatform());
        var schema = GoogleSchemaHelper.ConvertToSchema(typeof(TestStructure));

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
                            Role = "user",
                            Parts = new List<Part>
                            {
                                new Part { Text = "Create a profile for: John Smith, age 35, software engineer" }
                            }
                        }
                    },
                    GenerationConfig = new GenerationConfig
                    {
                        ResponseMimeType = "application/json",
                        ResponseSchema = schema
                    }
                }
            }
        };

        // Act
        var batchJob = await client.CreateAsync(
            GoogleAIModels.DefaultGeminiModel,
            source,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        batchJob.ShouldNotBeNull();
        batchJob.Name.ShouldNotBeNullOrEmpty();

        Console?.WriteLine($"Created batch job with schema: {batchJob.Name}");
        Console?.WriteLine($"State: {batchJob.State}");

        // Cleanup - cancel the job
        try
        {
            await client.CancelAsync(batchJob.Name!, cancellationToken: TestContext.Current.CancellationToken);
            Console?.WriteLine("Cancelled batch job");
        }
        catch
        {
            // Job may have already completed
        }
    }

    [Fact]
    public async Task ShouldExecuteBatchJob_WithSchema_GoogleAI()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        var client = new BatchClient(GetTestGooglePlatform());
        var schema = GoogleSchemaHelper.ConvertToSchema(typeof(TestStructure));

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
                            Role = "user",
                            Parts = new List<Part>
                            {
                                new Part { Text = "Create a profile for: John Smith, age 35, software engineer" }
                            }
                        }
                    },
                    GenerationConfig = new GenerationConfig
                    {
                        ResponseMimeType = "application/json",
                        ResponseSchema = schema
                    }
                }
            }
        };

        // Create the batch job
        var batchJob = await client.CreateAsync(
            GoogleAIModels.Gemini3FlashPreview,
            source,
            cancellationToken: TestContext.Current.CancellationToken);

        batchJob.ShouldNotBeNull();
        var jobName = batchJob.Name!;

        Console?.WriteLine($"Created batch job: {jobName}");
        Console?.WriteLine($"Initial state: {batchJob.State}");

        // Poll for completion
        var maxWaitTime = TimeSpan.FromMinutes(5);
        var pollInterval = TimeSpan.FromSeconds(10);
        var startTime = DateTime.UtcNow;
        BatchJob? completedJob = null;

        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            var response = await client.GetAsync(jobName, cancellationToken: TestContext.Current.CancellationToken);
            completedJob = response.Metadata;

            Console?.WriteLine($"State: {completedJob?.State} at {DateTime.UtcNow:HH:mm:ss}");

            if (completedJob?.State == JobState.JOB_STATE_SUCCEEDED ||
                completedJob?.State == JobState.JOB_STATE_FAILED ||
                completedJob?.State == JobState.JOB_STATE_CANCELLED)
            {
                break;
            }

            await Task.Delay(pollInterval, TestContext.Current.CancellationToken);
        }

        // Assert
        completedJob.ShouldNotBeNull();
        Console?.WriteLine($"Final state: {completedJob!.State}");

        if (completedJob.State == JobState.JOB_STATE_SUCCEEDED)
        {
            Console?.WriteLine("SUCCESS - Batch job completed!");

            // Check batch stats
            if (completedJob.BatchStats != null)
            {
                Console?.WriteLine($"Total requests: {completedJob.BatchStats.RequestCount}");
                Console?.WriteLine($"Succeeded: {completedJob.BatchStats.SucceededRequestCount}");
                Console?.WriteLine($"Failed: {completedJob.BatchStats.FailedRequestCount}");
                Console?.WriteLine($"Pending: {completedJob.BatchStats.PendingRequestCount}");
            }

            // Get the full response to check content output
            var finalResponse = await client.GetAsync(jobName, cancellationToken: TestContext.Current.CancellationToken);

            // Check if we have content output with the structured response
            if (finalResponse.ContentOutput?.InlinedResponses?.InlinedResponses != null)
            {
                Console?.WriteLine($"\n=== Content Output ===");
                foreach (var inlinedResponse in finalResponse.ContentOutput.InlinedResponses.InlinedResponses)
                {
                    if (inlinedResponse.Response != null)
                    {
                        var text = inlinedResponse.Response.Text();
                        Console?.WriteLine($"Response text: {text}");

                        // Verify it's valid JSON matching our schema
                        if (!string.IsNullOrEmpty(text))
                        {
                            var parsed = JsonSerializer.Deserialize<TestStructure>(text);
                            parsed.ShouldNotBeNull();
                            Console?.WriteLine($"Parsed Name: {parsed!.Name}");
                            Console?.WriteLine($"Parsed Age: {parsed.Age}");
                            Console?.WriteLine($"Parsed Description: {parsed.Description}");
                        }
                    }
                    else if (inlinedResponse.Error != null)
                    {
                        Console?.WriteLine($"Response error: {inlinedResponse.Error.Message}");
                    }
                }
            }
        }
        else if (completedJob.State == JobState.JOB_STATE_FAILED)
        {
            Console?.WriteLine($"Job failed: {completedJob.Error?.Message}");
        }

        completedJob.State.ShouldBe(JobState.JOB_STATE_SUCCEEDED);
    }

    [Fact]
    public async Task ShouldCreateBatchJob_WithSchemaInGcs_VertexAI()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);
        Assert.SkipUnless(IsGcsConfigured, "Skip - GCS not configured. Set GCS_TEST_BUCKET and GOOGLE_PROJECT_ID environment variables");

        var client = CreateVertexBatchClient();
        using var gcsHelper = new GcsTestHelper();

        // Upload schema-based batch requests to GCS
        var inputUri = await gcsHelper.UploadSampleContentRequestsWithSchemaAsync(typeof(TestStructure));
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

        // Act
        var batchJob = await client.CreateAsync(
            VertexAIModels.Gemini.Gemini25Flash,
            source,
            destination,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        batchJob.ShouldNotBeNull();
        batchJob.Name.ShouldNotBeNullOrEmpty();
        batchJob.State.ShouldNotBeNull();

        Console?.WriteLine($"Created batch job: {batchJob.Name}");
        Console?.WriteLine($"State: {batchJob.State}");
        Console?.WriteLine($"Model: {batchJob.Model}");

        // InputConfig and OutputConfig should be present in the response
        if (batchJob.InputConfig != null)
        {
            Console?.WriteLine($"Input Format: {batchJob.InputConfig.InstancesFormat}");
        }

        if (batchJob.OutputConfig != null)
        {
            Console?.WriteLine($"Output Format: {batchJob.OutputConfig.PredictionsFormat}");
        }
    }

    [Fact]
    public async Task ShouldGetBatchJob_OutputInfo_AfterCompletion_VertexAI()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);
        Assert.SkipUnless(IsGcsConfigured, "Skip - GCS not configured. Set GCS_TEST_BUCKET and GOOGLE_PROJECT_ID environment variables");

        var client = CreateVertexBatchClient();
        using var gcsHelper = new GcsTestHelper();

        // Upload schema-based batch requests to GCS
        var inputUri = await gcsHelper.UploadSampleContentRequestsWithSchemaAsync(typeof(TestStructure));
        var outputUriPrefix = gcsHelper.GetOutputUriPrefix();

        Console?.WriteLine($"Creating batch job...");
        Console?.WriteLine($"Input URI: {inputUri}");
        Console?.WriteLine($"Output URI: {outputUriPrefix}");

        var source = new BatchJobSource
        {
            GcsUri = new List<string> { inputUri }
        };

        var destination = new BatchJobDestination
        {
            GcsUri = outputUriPrefix
        };

        // Create the batch job
        var batchJob = await client.CreateAsync(
            VertexAIModels.Gemini.Gemini25Flash,
            source,
            destination,
            cancellationToken: TestContext.Current.CancellationToken);

        batchJob.ShouldNotBeNull();
        var jobName = batchJob.Name!;

        Console?.WriteLine($"Created batch job: {jobName}");
        Console?.WriteLine($"Initial state: {batchJob.State}");

        // Poll for completion (with timeout)
        var maxWaitTime = TimeSpan.FromMinutes(10);
        var pollInterval = TimeSpan.FromSeconds(30);
        var startTime = DateTime.UtcNow;
        BatchJob? completedJob = null;

        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            var response = await client.GetAsync(jobName, cancellationToken: TestContext.Current.CancellationToken);
            completedJob = response.Metadata;

            if (completedJob == null)
            {
                Console?.WriteLine("Failed to get job metadata");
                break;
            }

            Console?.WriteLine($"Current state: {completedJob.State} at {DateTime.UtcNow:HH:mm:ss}");

            if (completedJob.State == JobState.JOB_STATE_SUCCEEDED ||
                completedJob.State == JobState.JOB_STATE_FAILED ||
                completedJob.State == JobState.JOB_STATE_CANCELLED)
            {
                break;
            }

            await Task.Delay(pollInterval, TestContext.Current.CancellationToken);
        }

        // Assert
        completedJob.ShouldNotBeNull();

        if (completedJob!.State == JobState.JOB_STATE_SUCCEEDED)
        {
            // Issue #101: OutputInfo should be populated for completed jobs
            completedJob.OutputInfo.ShouldNotBeNull("OutputInfo should be populated when job succeeds");
            completedJob.OutputInfo!.GcsOutputDirectory.ShouldNotBeNullOrEmpty(
                "GcsOutputDirectory should be populated for GCS-based batch jobs");

            Console?.WriteLine($"SUCCESS - Job completed!");
            Console?.WriteLine($"GCS Output Directory: {completedJob.OutputInfo.GcsOutputDirectory}");

            // The output directory should match the prefix we provided
            completedJob.OutputInfo.GcsOutputDirectory.ShouldContain("test-output-");
        }
        else if (completedJob.State == JobState.JOB_STATE_FAILED)
        {
            Console?.WriteLine($"Job failed: {completedJob.Error?.Message}");
            // Even failed jobs should have error info
            completedJob.Error.ShouldNotBeNull();
        }
        else
        {
            Console?.WriteLine($"Job did not complete within timeout. Final state: {completedJob.State}");
            // Don't fail the test - batch jobs can take time
            Assert.Skip($"Batch job did not complete within {maxWaitTime.TotalMinutes} minutes");
        }
    }

    [Fact]
    public async Task ShouldListBatchJobs_WithOutputInfo_VertexAI()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);

        var client = CreateVertexBatchClient();

        // Act
        var response = await client.ListAsync(
            pageSize: 10,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        Console?.WriteLine($"Retrieved {response.BatchJobs?.Count ?? 0} batch jobs");

        if (response.BatchJobs != null && response.BatchJobs.Count > 0)
        {
            foreach (var job in response.BatchJobs)
            {
                Console?.WriteLine($"---");
                Console?.WriteLine($"Job: {job.Name}");
                Console?.WriteLine($"State: {job.State}");
                Console?.WriteLine($"Model: {job.Model}");

                if (job.State == JobState.JOB_STATE_SUCCEEDED && job.OutputInfo != null)
                {
                    // Issue #101: Verify OutputInfo is properly deserialized
                    Console?.WriteLine($"OutputInfo.GcsOutputDirectory: {job.OutputInfo.GcsOutputDirectory}");

                    if (job.OutputInfo.BigqueryOutputDataset != null)
                    {
                        Console?.WriteLine($"OutputInfo.BigqueryOutputDataset: {job.OutputInfo.BigqueryOutputDataset}");
                        Console?.WriteLine($"OutputInfo.BigqueryOutputTable: {job.OutputInfo.BigqueryOutputTable}");
                    }
                }

                if (job.InputConfig != null)
                {
                    Console?.WriteLine($"InputConfig.Format: {job.InputConfig.InstancesFormat}");
                }

                if (job.OutputConfig != null)
                {
                    Console?.WriteLine($"OutputConfig.Format: {job.OutputConfig.PredictionsFormat}");
                }
            }
        }
    }

    [Fact]
    public async Task ShouldVerifyExistingCompletedJob_HasOutputInfo_VertexAI()
    {
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);

        var client = CreateVertexBatchClient();

        // First, find a completed job from the list
        var listResponse = await client.ListAsync(
            pageSize: 20,
            cancellationToken: TestContext.Current.CancellationToken);

        var completedJob = listResponse.BatchJobs?.FirstOrDefault(j => j.State == JobState.JOB_STATE_SUCCEEDED);

        if (completedJob == null)
        {
            Console?.WriteLine("No completed batch jobs found in the project");
            Assert.Skip("No completed batch jobs available for testing");
            return;
        }

        Console?.WriteLine($"Found completed job: {completedJob.Name}");

        // Get full details of the completed job
        var response = await client.GetAsync(completedJob.Name!, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.Metadata.ShouldNotBeNull();

        var job = response.Metadata!;
        job.State.ShouldBe(JobState.JOB_STATE_SUCCEEDED);

        // Issue #101: OutputInfo should be populated for completed jobs
        job.OutputInfo.ShouldNotBeNull("OutputInfo should be populated for completed batch jobs");

        Console?.WriteLine($"Job: {job.Name}");
        Console?.WriteLine($"State: {job.State}");

        if (job.OutputInfo!.GcsOutputDirectory != null)
        {
            Console?.WriteLine($"GCS Output Directory: {job.OutputInfo.GcsOutputDirectory}");
            job.OutputInfo.GcsOutputDirectory.ShouldStartWith("gs://");
        }

        if (job.OutputInfo.BigqueryOutputDataset != null)
        {
            Console?.WriteLine($"BigQuery Dataset: {job.OutputInfo.BigqueryOutputDataset}");
            Console?.WriteLine($"BigQuery Table: {job.OutputInfo.BigqueryOutputTable}");
            job.OutputInfo.BigqueryOutputDataset.ShouldStartWith("bq://");
        }
    }

    private BatchClient CreateVertexBatchClient()
    {
        var adapter = GetTestVertexAIPlatform();
        return new BatchClient(adapter);
    }

    #endregion
}
