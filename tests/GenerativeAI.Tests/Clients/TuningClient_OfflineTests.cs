using GenerativeAI.Clients;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients;

/// <summary>
/// Offline tests for <see cref="TuningClient"/>: request URL, method, query and body (checked against the
/// google-genai SDK converters) and response parsing.
/// </summary>
public class TuningClient_OfflineTests
{
    private const string VertexLocation = "https://us-central1-aiplatform.googleapis.com/v1beta1/projects/p/locations/us-central1";
    private const string GeminiBase = "https://generativelanguage.googleapis.com/v1beta";

    private static (TuningClient Client, RecordingHttpHandler Handler) Vertex()
    {
        var handler = new RecordingHttpHandler();
        var platform = new VertextPlatformAdapter("p", "us-central1", accessToken: "t", validateAccessToken: false);
        return (new TuningClient(platform, new HttpClient(handler)), handler);
    }

    private static (TuningClient Client, RecordingHttpHandler Handler) Gemini()
    {
        var handler = new RecordingHttpHandler();
        return (new TuningClient(new GoogleAIPlatformAdapter("key"), new HttpClient(handler)), handler);
    }

    private const string VertexJobJson = """
        {
          "name": "projects/p/locations/us-central1/tuningJobs/123",
          "state": "JOB_STATE_RUNNING",
          "createTime": "2026-01-02T03:04:05Z",
          "baseModel": "gemini-2.5-flash",
          "tunedModelDisplayName": "my model",
          "tuningJobState": "TUNING_JOB_STATE_TUNING",
          "tuningJobMetadata": { "completedEpochCount": "2" },
          "supervisedTuningSpec": {
            "trainingDatasetUri": "gs://b/train.jsonl",
            "hyperParameters": { "epochCount": "3", "adapterSize": "ADAPTER_SIZE_FOUR" }
          },
          "tunedModel": {
            "model": "projects/p/locations/us-central1/models/9@1",
            "endpoint": "projects/p/locations/us-central1/endpoints/8",
            "checkpoints": [ { "checkpointId": "1", "epoch": "1", "step": "10" } ]
          }
        }
        """;

    [Fact]
    public async Task Vertex_Tune_Supervised_PostsTuningJob()
    {
        var (client, handler) = Vertex();
        handler.Reply(VertexJobJson);

        var job = await client.TuneAsync("gemini-2.5-flash", new TuningDataset { GcsUri = "gs://b/train.jsonl" },
            new CreateTuningJobConfig
            {
                ValidationDataset = new TuningValidationDataset { GcsUri = "gs://b/val.jsonl" },
                TunedModelDisplayName = "my model",
                Description = "desc",
                EpochCount = 3,
                LearningRateMultiplier = 0.5,
                AdapterSize = AdapterSize.ADAPTER_SIZE_FOUR,
                ExportLastCheckpointOnly = true,
                TuningMode = TuningMode.TUNING_MODE_PEFT_ADAPTER,
                BatchSize = 8,
                LearningRate = 0.01,
                Labels = new Dictionary<string, string> { ["team"] = "ml" },
                OutputUri = "gs://b/out",
                EvaluationConfig = new EvaluationConfig
                {
                    Metrics = new List<Metric>
                    {
                        new() { Name = "bleu" },
                        new() { Name = "rouge_l_sum" },
                        new() { Name = "fluency", PromptTemplate = "Rate {response}", JudgeModelSystemInstruction = "Be strict", ReturnRawOutput = true }
                    },
                    OutputConfig = new OutputConfig { GcsDestination = new GcsDestination { OutputUriPrefix = "gs://b/eval" } }
                }
            }, TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe($"{VertexLocation}/tuningJobs");
        request.Headers["Authorization"].ShouldBe("Bearer t");
        var body = request.Body!;
        body["baseModel"]!.GetValue<string>().ShouldBe("gemini-2.5-flash");
        body["tunedModelDisplayName"]!.GetValue<string>().ShouldBe("my model");
        body["description"]!.GetValue<string>().ShouldBe("desc");
        body["labels"]!["team"]!.GetValue<string>().ShouldBe("ml");
        body["outputUri"]!.GetValue<string>().ShouldBe("gs://b/out");
        body["preferenceOptimizationSpec"].ShouldBeNull();
        var spec = body["supervisedTuningSpec"]!;
        spec["trainingDatasetUri"]!.GetValue<string>().ShouldBe("gs://b/train.jsonl");
        spec["validationDatasetUri"]!.GetValue<string>().ShouldBe("gs://b/val.jsonl");
        spec["exportLastCheckpointOnly"]!.GetValue<bool>().ShouldBeTrue();
        spec["tuningMode"]!.GetValue<string>().ShouldBe("TUNING_MODE_PEFT_ADAPTER");
        var hp = spec["hyperParameters"]!;
        hp["epochCount"]!.GetValue<int>().ShouldBe(3);
        hp["learningRateMultiplier"]!.GetValue<double>().ShouldBe(0.5);
        hp["adapterSize"]!.GetValue<string>().ShouldBe("ADAPTER_SIZE_FOUR");
        hp["batchSize"]!.GetValue<int>().ShouldBe(8);
        hp["learningRate"]!.GetValue<double>().ShouldBe(0.01);
        var metrics = spec["evaluationConfig"]!["metrics"]!.AsArray();
        metrics.Count.ShouldBe(3);
        metrics[0]!["bleuSpec"].ShouldNotBeNull();
        metrics[0]!["aggregationMetrics"]!.AsArray().Select(n => n!.GetValue<string>()).ShouldBe(new[] { "AVERAGE", "STANDARD_DEVIATION" });
        metrics[0]!["name"].ShouldBeNull();
        metrics[1]!["rougeSpec"]!["rougeType"]!.GetValue<string>().ShouldBe("rougelsum");
        var pointwise = metrics[2]!["pointwiseMetricSpec"]!;
        pointwise["metricPromptTemplate"]!.GetValue<string>().ShouldBe("Rate {response}");
        pointwise["systemInstruction"]!.GetValue<string>().ShouldBe("Be strict");
        pointwise["customOutputFormatConfig"]!["returnRawOutput"]!.GetValue<bool>().ShouldBeTrue();
        spec["evaluationConfig"]!["outputConfig"]!["gcsDestination"]!["outputUriPrefix"]!.GetValue<string>().ShouldBe("gs://b/eval");

        job.Name.ShouldBe("projects/p/locations/us-central1/tuningJobs/123");
        job.State.ShouldBe(JobState.JOB_STATE_RUNNING);
        job.TuningJobState.ShouldBe(TuningJobState.TUNING_JOB_STATE_TUNING);
        job.TuningJobMetadata!.CompletedEpochCount.ShouldBe(2);
        job.SupervisedTuningSpec!.HyperParameters!.EpochCount.ShouldBe(3);
        job.TunedModel!.Checkpoints![0].Step.ShouldBe(10);
        job.EvaluationConfig.ShouldNotBeNull();
        job.IsCompleted.ShouldBeFalse();
    }

    [Fact]
    public async Task Vertex_Tune_PreferenceTuning_UsesPreferenceOptimizationSpec()
    {
        var (client, handler) = Vertex();
        handler.Reply("""{"name": "projects/p/locations/us-central1/tuningJobs/1"}""");

        await client.TuneAsync("gemini-2.5-flash", new TuningDataset { VertexDatasetResource = "projects/p/locations/us-central1/datasets/7" },
            new CreateTuningJobConfig
            {
                Method = TuningMethod.PREFERENCE_TUNING,
                EpochCount = 2,
                Beta = 0.1,
                BatchSize = 16, // not mapped for preference tuning
                ExportLastCheckpointOnly = false
            }, TestContext.Current.CancellationToken);

        var body = handler.Requests.ShouldHaveSingleItem().Body!;
        body["supervisedTuningSpec"].ShouldBeNull();
        var spec = body["preferenceOptimizationSpec"]!;
        spec["trainingDatasetUri"]!.GetValue<string>().ShouldBe("projects/p/locations/us-central1/datasets/7");
        spec["exportLastCheckpointOnly"]!.GetValue<bool>().ShouldBeFalse();
        spec["hyperParameters"]!["epochCount"]!.GetValue<int>().ShouldBe(2);
        spec["hyperParameters"]!["beta"]!.GetValue<double>().ShouldBe(0.1);
        spec["hyperParameters"]!["batchSize"].ShouldBeNull();
    }

    [Fact]
    public async Task Vertex_Tune_Distillation_UsesDistillationSpec()
    {
        var (client, handler) = Vertex();
        handler.Reply("""{"name": "projects/p/locations/us-central1/tuningJobs/1"}""");

        await client.TuneAsync("gemma-3-4b-it", new TuningDataset { GcsUri = "gs://b/prompts.jsonl" },
            new CreateTuningJobConfig
            {
                Method = TuningMethod.DISTILLATION,
                ValidationDataset = new TuningValidationDataset { GcsUri = "gs://b/val.jsonl" },
                BaseTeacherModel = "gemini-2.5-pro",
                SftLossWeightMultiplier = 0.25,
                LearningRate = 0.001,
                TuningMode = TuningMode.TUNING_MODE_FULL,
                CustomBaseModel = "gs://b/weights"
            }, TestContext.Current.CancellationToken);

        var body = handler.Requests.ShouldHaveSingleItem().Body!;
        body["customBaseModel"]!.GetValue<string>().ShouldBe("gs://b/weights");
        var spec = body["distillationSpec"]!;
        spec["promptDatasetUri"]!.GetValue<string>().ShouldBe("gs://b/prompts.jsonl");
        spec["validationDatasetUri"]!.GetValue<string>().ShouldBe("gs://b/val.jsonl");
        spec["baseTeacherModel"]!.GetValue<string>().ShouldBe("gemini-2.5-pro");
        spec["tuningMode"]!.GetValue<string>().ShouldBe("TUNING_MODE_FULL");
        spec["hyperParameters"]!["sftLossWeightMultiplier"]!.GetValue<double>().ShouldBe(0.25);
        spec["hyperParameters"]!["learningRate"]!.GetValue<double>().ShouldBe(0.001);
    }

    [Fact]
    public async Task Vertex_Tune_Reinforcement_UsesReinforcementTuningSpec()
    {
        var (client, handler) = Vertex();
        handler.Reply("""
            {
              "name": "projects/p/locations/us-central1/tuningJobs/2",
              "reinforcementTuningSpec": {
                "singleRewardConfig": { "rewardName": "match", "stringMatchRewardScorer": { "correctAnswerReward": 1 } },
                "hyperParameters": { "samplesPerPrompt": 4, "thinkingLevel": "LOW", "stepCount": "100" }
              }
            }
            """);

        var job = await client.TuneAsync("gemini-2.5-flash", new TuningDataset { GcsUri = "gs://b/rl.jsonl" },
            new CreateTuningJobConfig
            {
                Method = TuningMethod.REINFORCEMENT_TUNING,
                EpochCount = 1,
                BatchSize = 32,
                SamplesPerPrompt = 4,
                EvaluateInterval = 10,
                CheckpointInterval = 20,
                MaxOutputTokens = 1024,
                ThinkingLevel = ReinforcementTuningThinkingLevel.LOW,
                ValidationDatasetUri = "gs://b/rl-val.jsonl",
                ExportLastCheckpointOnly = true, // not mapped for reinforcement tuning
                RewardConfig = new SingleReinforcementTuningRewardConfig
                {
                    RewardName = "match",
                    ParseResponseConfig = new ReinforcementTuningParseResponseConfig
                    {
                        ParseType = ResponseParseType.REGEX_EXTRACT,
                        RegexExtractExpression = "<a>(.*)</a>"
                    },
                    StringMatchRewardScorer = new ReinforcementTuningStringMatchRewardScorer
                    {
                        CorrectAnswerReward = 1,
                        WrongAnswerReward = 0,
                        StringMatchExpression = new ReinforcementTuningStringMatchRewardScorerStringMatchExpression
                        {
                            MatchOperation = MatchOperation.EXACT_MATCH,
                            Expression = "{{references.answer}}"
                        }
                    }
                }
            }, TestContext.Current.CancellationToken);

        var body = handler.Requests.ShouldHaveSingleItem().Body!;
        body["supervisedTuningSpec"].ShouldBeNull();
        var spec = body["reinforcementTuningSpec"]!;
        spec["trainingDatasetUri"]!.GetValue<string>().ShouldBe("gs://b/rl.jsonl");
        spec["validationDatasetUri"]!.GetValue<string>().ShouldBe("gs://b/rl-val.jsonl");
        spec["exportLastCheckpointOnly"].ShouldBeNull();
        var hp = spec["hyperParameters"]!;
        hp["epochCount"]!.GetValue<long>().ShouldBe(1);
        hp["batchSize"]!.GetValue<int>().ShouldBe(32);
        hp["samplesPerPrompt"]!.GetValue<int>().ShouldBe(4);
        hp["evaluateInterval"]!.GetValue<int>().ShouldBe(10);
        hp["checkpointInterval"]!.GetValue<int>().ShouldBe(20);
        hp["maxOutputTokens"]!.GetValue<int>().ShouldBe(1024);
        hp["thinkingLevel"]!.GetValue<string>().ShouldBe("LOW");
        var reward = spec["singleRewardConfig"]!;
        reward["rewardName"]!.GetValue<string>().ShouldBe("match");
        reward["parseResponseConfig"]!["parseType"]!.GetValue<string>().ShouldBe("REGEX_EXTRACT");
        reward["stringMatchRewardScorer"]!["stringMatchExpression"]!["matchOperation"]!.GetValue<string>().ShouldBe("EXACT_MATCH");

        job.ReinforcementTuningSpec!.SingleRewardConfig!.RewardName.ShouldBe("match");
        job.ReinforcementTuningSpec.HyperParameters!.ThinkingLevel.ShouldBe(ReinforcementTuningThinkingLevel.LOW);
        job.ReinforcementTuningSpec.HyperParameters.StepCount.ShouldBe(100);
    }

    [Fact]
    public async Task Vertex_Tune_FromTunedModel_SendsPreTunedModel()
    {
        var (client, handler) = Vertex();
        handler.Reply("""{"name": "projects/p/locations/us-central1/tuningJobs/3"}""");

        await client.TuneAsync("projects/p/locations/us-central1/models/9", new TuningDataset { GcsUri = "gs://b/t.jsonl" },
            new CreateTuningJobConfig { PreTunedModelCheckpointId = "4" }, TestContext.Current.CancellationToken);

        var body = handler.Requests.ShouldHaveSingleItem().Body!;
        body["baseModel"].ShouldBeNull();
        body["preTunedModel"]!["tunedModelName"]!.GetValue<string>().ShouldBe("projects/p/locations/us-central1/models/9");
        body["preTunedModel"]!["checkpointId"]!.GetValue<string>().ShouldBe("4");
    }

    [Fact]
    public async Task Vertex_Tune_RejectsInvalidInput()
    {
        var (client, handler) = Vertex();

        await Should.ThrowAsync<ArgumentException>(() => client.TuneAsync("gemini-2.5-flash",
            new TuningDataset { GcsUri = "gs://b/t.jsonl" },
            new CreateTuningJobConfig { EvaluationConfig = new EvaluationConfig { Metrics = new List<Metric> { new() { Name = "bleu" } } } },
            TestContext.Current.CancellationToken));
        await Should.ThrowAsync<NotSupportedException>(() => client.TuneAsync("gemini-2.5-flash",
            new TuningDataset { Examples = new List<TuningExample> { new() { TextInput = "a", Output = "b" } } },
            cancellationToken: TestContext.Current.CancellationToken));
        handler.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task Gemini_Tune_PostsTunedModelAndReturnsQueuedJob()
    {
        var (client, handler) = Gemini();
        handler.Reply("""
            {
              "name": "tunedModels/my-model-abc/operations/op1",
              "metadata": { "@type": "type.googleapis.com/google.ai.generativelanguage.v1beta.CreateTunedModelMetadata", "tunedModel": "tunedModels/my-model-abc", "totalSteps": 10 }
            }
            """);

        var job = await client.TuneAsync("models/gemini-1.5-flash-001-tuning", new TuningDataset
        {
            Examples = new List<TuningExample> { new() { TextInput = "1", Output = "2" } }
        }, new CreateTuningJobConfig
        {
            TunedModelDisplayName = "number generator",
            EpochCount = 5,
            BatchSize = 4,
            LearningRate = 0.001
        }, TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe($"{GeminiBase}/tunedModels");
        request.Headers["x-goog-api-key"].ShouldBe("key");
        var body = request.Body!;
        body["baseModel"]!.GetValue<string>().ShouldBe("models/gemini-1.5-flash-001-tuning");
        body["displayName"]!.GetValue<string>().ShouldBe("number generator");
        var task = body["tuningTask"]!;
        task["hyperparameters"]!["epochCount"]!.GetValue<int>().ShouldBe(5);
        task["hyperparameters"]!["batchSize"]!.GetValue<int>().ShouldBe(4);
        task["hyperparameters"]!["learningRate"]!.GetValue<double>().ShouldBe(0.001);
        var example = task["trainingData"]!["examples"]!["examples"]![0]!;
        example["textInput"]!.GetValue<string>().ShouldBe("1");
        example["output"]!.GetValue<string>().ShouldBe("2");

        job.Name.ShouldBe("tunedModels/my-model-abc");
        job.State.ShouldBe(JobState.JOB_STATE_QUEUED);
    }

    [Fact]
    public async Task Gemini_Tune_FallsBackToOperationNameAndRejectsVertexOnlyOptions()
    {
        var (client, handler) = Gemini();
        handler.Reply("""{"name": "tunedModels/xyz/operations/op1"}""");

        var job = await client.TuneAsync("models/gemini-1.5-flash-001-tuning",
            new TuningDataset { Examples = new List<TuningExample> { new() { TextInput = "a", Output = "b" } } },
            cancellationToken: TestContext.Current.CancellationToken);
        job.Name.ShouldBe("tunedModels/xyz");

        await Should.ThrowAsync<NotSupportedException>(() => client.TuneAsync("models/x",
            new TuningDataset { Examples = new List<TuningExample>() },
            new CreateTuningJobConfig { Description = "d" }, TestContext.Current.CancellationToken));
        await Should.ThrowAsync<NotSupportedException>(() => client.TuneAsync("models/x",
            new TuningDataset { GcsUri = "gs://b/t.jsonl" }, cancellationToken: TestContext.Current.CancellationToken));
        await Should.ThrowAsync<NotSupportedException>(() => client.ListAsync(cancellationToken: TestContext.Current.CancellationToken));
        await Should.ThrowAsync<NotSupportedException>(() => client.ValidateRewardAsync(new Content("r", Roles.Model),
            new ReinforcementTuningExample(), cancellationToken: TestContext.Current.CancellationToken));
        handler.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Vertex_Get_ResolvesNamesAndParsesJob()
    {
        var (client, handler) = Vertex();
        handler.Reply(VertexJobJson);

        await client.GetAsync("projects/p/locations/us-central1/tuningJobs/123", TestContext.Current.CancellationToken);
        await client.GetAsync("tuningJobs/123", TestContext.Current.CancellationToken);
        var job = await client.GetAsync("123", TestContext.Current.CancellationToken);

        handler.Requests.Count.ShouldBe(3);
        handler.Requests.ShouldAllBe(r => r.Method == HttpMethod.Get);
        handler.Requests.Select(r => r.Url.AbsoluteUri).Distinct().ShouldHaveSingleItem()
            .ShouldBe($"{VertexLocation}/tuningJobs/123");
        job.TunedModel!.Endpoint.ShouldBe("projects/p/locations/us-central1/endpoints/8");
        job.CreateTime!.ToDateTime().ShouldBe(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc));
    }

    [Fact]
    public async Task Gemini_Get_ConvertsTunedModelToTuningJob()
    {
        var (client, handler) = Gemini();
        handler.Reply("""
            {
              "name": "tunedModels/my-model-abc",
              "baseModel": "models/gemini-1.5-flash-001-tuning",
              "displayName": "number generator",
              "description": "d",
              "state": "ACTIVE",
              "createTime": "2026-01-01T00:00:00Z",
              "updateTime": "2026-01-01T02:00:00Z",
              "readerProjectNumbers": ["123"],
              "tuningTask": {
                "startTime": "2026-01-01T00:01:00Z",
                "completeTime": "2026-01-01T01:00:00Z",
                "snapshots": [ { "step": 1, "epoch": 1, "meanLoss": 0.5, "computeTime": "2026-01-01T00:02:00Z" } ],
                "hyperparameters": { "epochCount": 5, "batchSize": 4, "learningRate": 0.001 }
              }
            }
            """);

        var job = await client.GetAsync("my-model-abc", TestContext.Current.CancellationToken);

        handler.Requests.ShouldHaveSingleItem().Url.AbsoluteUri.ShouldBe($"{GeminiBase}/tunedModels/my-model-abc");
        job.Name.ShouldBe("tunedModels/my-model-abc");
        job.State.ShouldBe(JobState.JOB_STATE_SUCCEEDED);
        job.IsCompleted.ShouldBeTrue();
        job.BaseModel.ShouldBe("models/gemini-1.5-flash-001-tuning");
        job.Description.ShouldBe("d");
        job.StartTime!.ToDateTime().ShouldBe(new DateTime(2026, 1, 1, 0, 1, 0, DateTimeKind.Utc));
        job.EndTime!.ToDateTime().ShouldBe(new DateTime(2026, 1, 1, 1, 0, 0, DateTimeKind.Utc));
        job.TunedModel!.Model.ShouldBe("tunedModels/my-model-abc");
        job.TunedModel.Endpoint.ShouldBe("tunedModels/my-model-abc");
    }

    [Fact]
    public async Task Vertex_List_SendsPagingQueryAndListAllFollowsPages()
    {
        var (client, handler) = Vertex();
        handler.Reply("""{"tuningJobs": [{"name": "a"}, {"name": "b"}], "nextPageToken": "tok/2"}""")
            .Reply("""{"tuningJobs": [{"name": "c"}]}""");

        var names = new List<string?>();
        await foreach (var job in client.ListAllAsync(pageSize: 2, filter: "state=JOB_STATE_SUCCEEDED",
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            names.Add(job.Name);
        }

        names.ShouldBe(new[] { "a", "b", "c" });
        handler.Requests.Count.ShouldBe(2);
        handler.Requests[0].Url.AbsoluteUri.ShouldBe(
            $"{VertexLocation}/tuningJobs?pageSize=2&filter=state%3DJOB_STATE_SUCCEEDED");
        handler.Requests[1].Url.AbsoluteUri.ShouldBe(
            $"{VertexLocation}/tuningJobs?pageSize=2&pageToken=tok%2F2&filter=state%3DJOB_STATE_SUCCEEDED");

        var page = await client.ListAsync(cancellationToken: TestContext.Current.CancellationToken);
        handler.Requests[2].Url.AbsoluteUri.ShouldBe($"{VertexLocation}/tuningJobs");
        page.TuningJobs!.ShouldHaveSingleItem().Name.ShouldBe("c");
    }

    [Fact]
    public async Task Cancel_PostsToCancelEndpointOnBothPlatforms()
    {
        var (vertex, vertexHandler) = Vertex();
        await vertex.CancelAsync("projects/p/locations/us-central1/tuningJobs/123", TestContext.Current.CancellationToken);
        var vertexRequest = vertexHandler.Requests.ShouldHaveSingleItem();
        vertexRequest.Method.ShouldBe(HttpMethod.Post);
        vertexRequest.Url.AbsoluteUri.ShouldBe($"{VertexLocation}/tuningJobs/123:cancel");

        var (gemini, geminiHandler) = Gemini();
        await gemini.CancelAsync("tunedModels/abc", TestContext.Current.CancellationToken);
        var geminiRequest = geminiHandler.Requests.ShouldHaveSingleItem();
        geminiRequest.Method.ShouldBe(HttpMethod.Post);
        geminiRequest.Url.AbsoluteUri.ShouldBe($"{GeminiBase}/tunedModels/abc:cancel");
    }

    [Fact]
    public async Task Vertex_ValidateReward_PostsToValidateEndpoint()
    {
        var (client, handler) = Vertex();
        handler.Reply("""
            {
              "overallReward": 0.75,
              "rewardInfoDetails": {
                "match": { "reward": 1.0, "userRequestedAuxInfo": "ok" },
                "judge": { "reward": 0.5, "errorStatus": { "code": 3, "message": "bad" } }
              }
            }
            """);

        var response = await client.ValidateRewardAsync(
            new Content("The answer is <a>42</a>", Roles.Model),
            new ReinforcementTuningExample
            {
                Contents = new List<Content> { new("What is 6*7?", Roles.User) },
                References = new Dictionary<string, string> { ["answer"] = "42" },
                SystemInstruction = new Content("Answer in <a> tags", null)
            },
            compositeRewardConfig: new CompositeReinforcementTuningRewardConfig
            {
                WeightedRewardConfigs = new List<CompositeReinforcementTuningRewardConfigWeightedRewardConfig>
                {
                    new()
                    {
                        Weight = 0.5,
                        RewardConfig = new SingleReinforcementTuningRewardConfig
                        {
                            RewardName = "code",
                            CodeExecutionRewardScorer = new ReinforcementTuningCodeExecutionRewardScorer { PythonCodeSnippet = "def f(): pass" }
                        }
                    },
                    new()
                    {
                        Weight = 0.5,
                        RewardConfig = new SingleReinforcementTuningRewardConfig
                        {
                            RewardName = "judge",
                            AutoraterScorer = new ReinforcementTuningAutoraterScorer
                            {
                                AutoraterPrompt = "Score {{response}}",
                                ParsedResponseConversionScorer = new ReinforcementTuningAutoraterScorerParsedResponseConversionScorer()
                            }
                        }
                    }
                }
            },
            cancellationToken: TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe($"{VertexLocation}/tuningJobs:validateReinforcementTuningReward");
        var body = request.Body!;
        body["sampleResponse"]!["role"]!.GetValue<string>().ShouldBe("model");
        body["sampleResponse"]!["parts"]![0]!["text"]!.GetValue<string>().ShouldBe("The answer is <a>42</a>");
        body["example"]!["contents"]![0]!["parts"]![0]!["text"]!.GetValue<string>().ShouldBe("What is 6*7?");
        body["example"]!["references"]!["answer"]!.GetValue<string>().ShouldBe("42");
        body["example"]!["systemInstruction"].ShouldNotBeNull();
        body["singleRewardConfig"].ShouldBeNull();
        var weighted = body["compositeRewardConfig"]!["weightedRewardConfigs"]!.AsArray();
        weighted[0]!["rewardConfig"]!["codeExecutionRewardScorer"]!["pythonCodeSnippet"]!.GetValue<string>().ShouldBe("def f(): pass");
        weighted[1]!["rewardConfig"]!["autoraterScorer"]!["parsedResponseConversionScorer"].ShouldNotBeNull();

        response.OverallReward.ShouldBe(0.75);
        response.RewardInfoDetails!["match"].UserRequestedAuxInfo.ShouldBe("ok");
        response.RewardInfoDetails["judge"].ErrorStatus!.Message.ShouldBe("bad");

        await client.ValidateRewardAsync(new Content("x", Roles.Model), new ReinforcementTuningExample(),
            parent: "projects/other/locations/europe-west4", cancellationToken: TestContext.Current.CancellationToken);
        handler.Requests[1].Url.AbsoluteUri.ShouldBe(
            "https://us-central1-aiplatform.googleapis.com/v1beta1/projects/other/locations/europe-west4/tuningJobs:validateReinforcementTuningReward");
    }

    [Fact]
    public void SourceGeneratedContext_RoundTripsTuningTypesWithoutReflection()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new TuningJob
        {
            ReinforcementTuningSpec = new ReinforcementTuningSpec
            {
                HyperParameters = new ReinforcementTuningHyperParameters { ThinkingLevel = ReinforcementTuningThinkingLevel.HIGH }
            },
            SupervisedTuningSpec = new SupervisedTuningSpec
            {
                EvaluationConfig = new EvaluationConfig { Metrics = new List<Metric> { new() { BleuSpec = new() } } }
            }
        }, TuningSerializerContext.Default.TuningJob);
        json.ShouldContain("\"thinkingLevel\": \"HIGH\"");
        json.ShouldContain("\"bleuSpec\": {}");

        var job = System.Text.Json.JsonSerializer.Deserialize(VertexJobJson, TuningSerializerContext.Default.TuningJob)!;
        job.TuningJobMetadata!.CompletedEpochCount.ShouldBe(2);
        TuningSerializerContext.Default.ValidateRewardResponse.ShouldNotBeNull();
        TuningSerializerContext.Default.GoogleAITunedModel.ShouldNotBeNull();
    }

    [Fact]
    public async Task WaitForCompletion_PollsUntilTerminalState()
    {
        var (client, handler) = Vertex();
        handler.Reply("""{"name": "j", "state": "JOB_STATE_PENDING"}""")
            .Reply("""{"name": "j", "state": "JOB_STATE_RUNNING"}""")
            .Reply("""{"name": "j", "state": "JOB_STATE_SUCCEEDED", "tunedModel": {"model": "m"}}""");

        var job = await client.WaitForCompletionAsync("j", TimeSpan.FromMilliseconds(1),
            cancellationToken: TestContext.Current.CancellationToken);

        job.State.ShouldBe(JobState.JOB_STATE_SUCCEEDED);
        job.TunedModel!.Model.ShouldBe("m");
        handler.Requests.Count.ShouldBe(3);

        var (timingOut, timingOutHandler) = Vertex();
        timingOutHandler.Reply("""{"name": "j", "state": "JOB_STATE_RUNNING"}""");
        await Should.ThrowAsync<TimeoutException>(() => timingOut.WaitForCompletionAsync("j",
            TimeSpan.FromMilliseconds(5), TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken));
    }
}
