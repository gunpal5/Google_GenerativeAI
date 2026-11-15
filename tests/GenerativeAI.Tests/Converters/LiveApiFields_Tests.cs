using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Converters;

/// <summary>
/// Verifies the wire format of Live API setup, realtime input, context update and server message fields.
/// </summary>
public class LiveApiFields_Tests
{
    private static JsonNode SerializeToNode(LiveClientMessage payload)
    {
        var json = JsonSerializer.Serialize(payload, DefaultSerializerOptions.Options);
        return JsonNode.Parse(json)!;
    }

    private static LiveServerMessage Deserialize(string json)
    {
        var payload = JsonSerializer.Deserialize<LiveServerMessage>(json, DefaultSerializerOptions.Options);
        payload.ShouldNotBeNull();
        return payload;
    }

    [Fact]
    public void Setup_SerializesRealtimeInputConfig()
    {
        var payload = new LiveClientMessage
        {
            Setup = new LiveClientSetup
            {
                Model = GoogleAIModels.Gemini38Live,
                RealtimeInputConfig = new RealtimeInputConfig
                {
                    AutomaticActivityDetection = new AutomaticActivityDetection
                    {
                        Disabled = false,
                        StartOfSpeechSensitivity = StartSensitivity.START_SENSITIVITY_LOW,
                        EndOfSpeechSensitivity = EndSensitivity.END_SENSITIVITY_HIGH,
                        PrefixPaddingMs = 20,
                        SilenceDurationMs = 100
                    },
                    ActivityHandling = ActivityHandling.NO_INTERRUPTION,
                    TurnCoverage = TurnCoverage.TURN_INCLUDES_AUDIO_ACTIVITY_AND_ALL_VIDEO,
                    InterimTranscriptTimestampEnabled = true
                }
            }
        };

        var config = SerializeToNode(payload)["setup"]!["realtimeInputConfig"]!;
        var aad = config["automaticActivityDetection"]!;
        aad["disabled"]!.GetValue<bool>().ShouldBeFalse();
        aad["startOfSpeechSensitivity"]!.GetValue<string>().ShouldBe("START_SENSITIVITY_LOW");
        aad["endOfSpeechSensitivity"]!.GetValue<string>().ShouldBe("END_SENSITIVITY_HIGH");
        aad["prefixPaddingMs"]!.GetValue<int>().ShouldBe(20);
        aad["silenceDurationMs"]!.GetValue<int>().ShouldBe(100);
        config["activityHandling"]!.GetValue<string>().ShouldBe("NO_INTERRUPTION");
        config["turnCoverage"]!.GetValue<string>().ShouldBe("TURN_INCLUDES_AUDIO_ACTIVITY_AND_ALL_VIDEO");
        config["interimTranscriptTimestampEnabled"]!.GetValue<bool>().ShouldBeTrue();
    }

    [Fact]
    public void Setup_SerializesHistoryProactivityTranscriptionAndOtherFields()
    {
        var payload = new LiveClientMessage
        {
            Setup = new LiveClientSetup
            {
                HistoryConfig = new HistoryConfig { InitialHistoryInClientContent = true },
                Proactivity = new ProactivityConfig { ProactiveAudio = true, ProactiveVideo = true },
                ExplicitVadSignal = true,
                Labels = new Dictionary<string, string> { ["safety_identifier"] = "user_1" },
                AvatarConfig = new AvatarConfig
                {
                    AvatarName = "avatar-1",
                    CustomizedAvatar = new CustomizedAvatar { ImageMimeType = "image/jpeg", ImageData = "AAAA" },
                    AudioBitrateBps = 64000,
                    VideoBitrateBps = 1000000
                },
                InputAudioTranscription = new AudioTranscriptionConfig
                {
                    LanguageCodes = ["en-US", "es-ES"],
                    CustomVocabulary = ["Gemini"],
                    WordTimestamp = true,
                    Diarization = true,
                    Mode = AudioTranscriptionConfigMode.SMART,
                    SourceText = "draft"
                }
            }
        };

        var setup = SerializeToNode(payload)["setup"]!;
        setup["historyConfig"]!["initialHistoryInClientContent"]!.GetValue<bool>().ShouldBeTrue();
        setup["proactivity"]!["proactiveAudio"]!.GetValue<bool>().ShouldBeTrue();
        setup["proactivity"]!["proactiveVideo"]!.GetValue<bool>().ShouldBeTrue();
        setup["explicitVadSignal"]!.GetValue<bool>().ShouldBeTrue();
        setup["labels"]!["safety_identifier"]!.GetValue<string>().ShouldBe("user_1");

        var avatar = setup["avatarConfig"]!;
        avatar["avatarName"]!.GetValue<string>().ShouldBe("avatar-1");
        avatar["customizedAvatar"]!["imageMimeType"]!.GetValue<string>().ShouldBe("image/jpeg");
        avatar["customizedAvatar"]!["imageData"]!.GetValue<string>().ShouldBe("AAAA");
        avatar["audioBitrateBps"]!.GetValue<int>().ShouldBe(64000);
        avatar["videoBitrateBps"]!.GetValue<int>().ShouldBe(1000000);

        var transcription = setup["inputAudioTranscription"]!;
        transcription["languageCodes"]!.AsArray().Select(n => n!.GetValue<string>()).ShouldBe(["en-US", "es-ES"]);
        transcription["customVocabulary"]![0]!.GetValue<string>().ShouldBe("Gemini");
        transcription["wordTimestamp"]!.GetValue<bool>().ShouldBeTrue();
        transcription["diarization"]!.GetValue<bool>().ShouldBeTrue();
        transcription["mode"]!.GetValue<string>().ShouldBe("SMART");
        transcription["sourceText"]!.GetValue<string>().ShouldBe("draft");
    }

    [Fact]
    public void Setup_EmptyAudioTranscriptionConfig_SerializesAsEmptyObject()
    {
        var payload = new LiveClientMessage
        {
            Setup = new LiveClientSetup { OutputAudioTranscription = new AudioTranscriptionConfig() }
        };

        var node = SerializeToNode(payload)["setup"]!["outputAudioTranscription"]!.AsObject();
        node.Count.ShouldBe(0);
    }

    [Fact]
    public void RealtimeInput_SerializesActivitySignalsAndAudioStreamEnd()
    {
        var payload = new LiveClientMessage
        {
            RealtimeInput = new LiveClientRealtimeInput
            {
                AudioStreamEnd = true,
                ActivityStart = new ActivityStart(),
                ActivityEnd = new ActivityEnd()
            }
        };

        var input = SerializeToNode(payload)["realtimeInput"]!;
        input["audioStreamEnd"]!.GetValue<bool>().ShouldBeTrue();
        input["activityStart"]!.AsObject().Count.ShouldBe(0);
        input["activityEnd"]!.AsObject().Count.ShouldBe(0);
    }

    [Fact]
    public void ContextUpdate_SerializesSystemInstructionAndWrappedTools()
    {
        var payload = new LiveClientMessage
        {
            ContextUpdate = new LiveClientContextUpdate
            {
                SystemInstruction = new Content("Be brief.", Roles.User),
                Tools = new ContextUpdateTools { Tools = [] }
            }
        };

        var update = SerializeToNode(payload)["contextUpdate"]!;
        update["systemInstruction"]!["parts"]![0]!["text"]!.GetValue<string>().ShouldBe("Be brief.");
        // An empty tools list inside the wrapper means "clear all tools".
        update["tools"]!["tools"]!.AsArray().Count.ShouldBe(0);
    }

    [Fact]
    public void SetupComplete_DeserializesSessionId()
    {
        var payload = Deserialize("""{"setupComplete":{"sessionId":"session-42"}}""");

        payload.SetupComplete.ShouldNotBeNull();
        payload.SetupComplete.SessionId.ShouldBe("session-42");
    }

    [Fact]
    public void ServerContent_DeserializesInterimTranscriptionAndInteractionStatus()
    {
        const string json = """
            {"serverContent":{
              "turnComplete":true,
              "turnCompleteReason":"GENERATED_AUDIO_SAFETY",
              "interactionStatus":"IDLE",
              "interimInputTranscription":{"text":"hel"},
              "inputTranscription":{
                "text":"hello world","finished":true,"languageCode":"en-US","speakerLabel":"spk_1",
                "startOffset":"0.5s","endOffset":"1.5s",
                "words":[{"word":"hello","startOffset":"0.5s","endOffset":"0.9s"},{"word":"world","startOffset":"1s","endOffset":"1.5s"}]
              },
              "urlContextMetadata":{"urlMetadata":[{"retrievedUrl":"https://example.com","urlRetrievalStatus":"URL_RETRIEVAL_STATUS_PAYWALL"}]}
            }}
            """;

        var content = Deserialize(json).ServerContent;

        content.ShouldNotBeNull();
        content.TurnCompleteReason.ShouldBe(TurnCompleteReason.GENERATED_AUDIO_SAFETY);
        content.InteractionStatus.ShouldBe(InteractionStatus.IDLE);
        content.InterimInputTranscription!.Text.ShouldBe("hel");

        var transcription = content.InputTranscription;
        transcription.ShouldNotBeNull();
        transcription.LanguageCode.ShouldBe("en-US");
        transcription.SpeakerLabel.ShouldBe("spk_1");
        transcription.StartOffset.ShouldBe("0.5s");
        transcription.EndOffset.ShouldBe("1.5s");
        transcription.Words.ShouldNotBeNull();
        transcription.Words.Count.ShouldBe(2);
        transcription.Words[1].Word.ShouldBe("world");
        transcription.Words[1].StartOffset.ShouldBe("1s");
        transcription.Words[1].EndOffset.ShouldBe("1.5s");

        content.UrlContextMetadata!.UrlMetadata![0].UrlRetrievalStatus.ShouldBe(UrlRetrievalStatus.URL_RETRIEVAL_STATUS_PAYWALL);
    }

    [Theory]
    [InlineData("PROHIBITED_INPUT_CONTENT", TurnCompleteReason.PROHIBITED_INPUT_CONTENT)]
    [InlineData("INPUT_TEXT_CONTAIN_PROMINENT_PERSON_PROHIBITED", TurnCompleteReason.INPUT_TEXT_CONTAIN_PROMINENT_PERSON_PROHIBITED)]
    [InlineData("MAX_REGENERATION_REACHED", TurnCompleteReason.MAX_REGENERATION_REACHED)]
    public void ServerContent_DeserializesNewTurnCompleteReasons(string wire, TurnCompleteReason expected)
    {
        var payload = Deserialize("{\"serverContent\":{\"turnCompleteReason\":\"" + wire + "\"}}");

        payload.ServerContent!.TurnCompleteReason.ShouldBe(expected);
    }

    [Fact]
    public void ServerMessage_DeserializesVoiceActivity()
    {
        var payload = Deserialize("""{"voiceActivity":{"type":"ACTIVITY_START","audioOffset":"2.25s"}}""");

        payload.VoiceActivity.ShouldNotBeNull();
        payload.VoiceActivity.VoiceActivityType.ShouldBe(VoiceActivityType.ACTIVITY_START);
        payload.VoiceActivity.AudioOffset.ShouldBe("2.25s");
    }

    [Fact]
    public void ServerMessage_DeserializesVoiceActivityDetectionSignal()
    {
        var payload = Deserialize("""{"voiceActivityDetectionSignal":{"vadSignalType":"VAD_SIGNAL_TYPE_EOS"}}""");

        payload.VoiceActivityDetectionSignal.ShouldNotBeNull();
        payload.VoiceActivityDetectionSignal.VadSignalType.ShouldBe(VadSignalType.VAD_SIGNAL_TYPE_EOS);
    }
}
