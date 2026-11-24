using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace GenerativeAI.Types.Converters;

/// <summary>
/// Helpers shared by the Interactions JSON converters.
/// </summary>
internal static class InteractionJson
{
    /// <summary>
    /// Gets the strongly typed <see cref="JsonTypeInfo{T}"/> for <typeparamref name="T"/> from <paramref name="options"/>.
    /// </summary>
    internal static JsonTypeInfo<T> GetTypeInfo<T>(JsonSerializerOptions options)
    {
#if NET11_0_OR_GREATER
        return options.GetTypeInfo<T>();
#else
        return (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T));
#endif
    }

    /// <summary>
    /// Reads the string value of <paramref name="propertyName"/> from <paramref name="node"/>, or <c>null</c>.
    /// </summary>
    internal static string? GetString(JsonNode? node, string propertyName)
    {
        if (node is JsonObject obj && obj.TryGetPropertyValue(propertyName, out var value) &&
            value is JsonValue jsonValue && jsonValue.TryGetValue<string>(out var text))
        {
            return text;
        }

        return null;
    }
}

/// <summary>
/// Base converter for the open (discriminated) unions of the Interactions API. The discriminator selects the
/// concrete type; unknown discriminators, and payloads a known type cannot read, become an "unknown" value that
/// keeps the raw JSON so nothing throws and nothing is lost when the API adds new variants.
/// </summary>
/// <typeparam name="TBase">The base type of the union.</typeparam>
internal abstract class InteractionUnionConverter<TBase> : JsonConverter<TBase> where TBase : class
{
    /// <summary>
    /// Gets the name of the discriminator property.
    /// </summary>
    protected virtual string DiscriminatorName => "type";

    /// <summary>
    /// Maps a discriminator value to its concrete type, or returns <c>null</c> for unknown values.
    /// </summary>
    protected abstract Type? ResolveType(string discriminator);

    /// <summary>
    /// Maps a discriminator value and the payload to its concrete type, for unions where one discriminator value has
    /// several shapes. Defaults to <see cref="ResolveType(string)"/>.
    /// </summary>
    protected virtual Type? ResolveType(string discriminator, JsonNode node) => ResolveType(discriminator);

    /// <summary>
    /// Creates the value used for unknown variants.
    /// </summary>
    protected abstract TBase CreateUnknown(string? discriminator, JsonNode raw);

    /// <summary>
    /// Returns the raw JSON of an unknown variant, or <c>null</c> for known variants.
    /// </summary>
    protected abstract JsonNode? GetUnknownRaw(TBase value);

    /// <inheritdoc />
    public override TBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var node = JsonNode.Parse(ref reader);
        if (node is null)
            return null;

        var discriminator = InteractionJson.GetString(node, DiscriminatorName);
        var type = discriminator == null ? null : ResolveType(discriminator, node);
        if (type != null)
        {
            try
            {
                if (node.Deserialize(options.GetTypeInfo(type)) is TBase value)
                    return value;
            }
            catch (JsonException)
            {
                // Fall through: keep the payload as an unknown variant instead of failing the whole response.
            }
            catch (InvalidOperationException)
            {
                // Same as above.
            }
        }

        return CreateUnknown(discriminator, node);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TBase value, JsonSerializerOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);
#else
        if (writer == null) throw new ArgumentNullException(nameof(writer));
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (options == null) throw new ArgumentNullException(nameof(options));
#endif
        var raw = GetUnknownRaw(value);
        if (raw != null)
        {
            raw.WriteTo(writer, options);
            return;
        }

        JsonSerializer.Serialize(writer, value, options.GetTypeInfo(value.GetType()));
    }
}

/// <summary>Converter for <see cref="InteractionContent"/>.</summary>
internal sealed class InteractionContentConverter : InteractionUnionConverter<InteractionContent>
{
    /// <inheritdoc />
    protected override Type? ResolveType(string discriminator) => discriminator switch
    {
        InteractionContentTypes.Text => typeof(InteractionTextContent),
        InteractionContentTypes.Image => typeof(InteractionImageContent),
        InteractionContentTypes.Audio => typeof(InteractionAudioContent),
        InteractionContentTypes.Video => typeof(InteractionVideoContent),
        InteractionContentTypes.Document => typeof(InteractionDocumentContent),
        _ => null
    };

    /// <inheritdoc />
    protected override InteractionContent CreateUnknown(string? discriminator, JsonNode raw) =>
        new UnknownInteractionContent(discriminator, raw);

    /// <inheritdoc />
    protected override JsonNode? GetUnknownRaw(InteractionContent value) => (value as UnknownInteractionContent)?.Raw;
}

/// <summary>Converter for <see cref="InteractionAnnotation"/>.</summary>
internal sealed class InteractionAnnotationConverter : InteractionUnionConverter<InteractionAnnotation>
{
    /// <inheritdoc />
    protected override Type? ResolveType(string discriminator) => discriminator switch
    {
        "url_citation" => typeof(InteractionUrlCitation),
        "file_citation" => typeof(InteractionFileCitation),
        "place_citation" => typeof(InteractionPlaceCitation),
        "speech_metadata" => typeof(InteractionSpeechAnnotation),
        "word_info" => typeof(InteractionWordInfo),
        _ => null
    };

    /// <inheritdoc />
    protected override InteractionAnnotation CreateUnknown(string? discriminator, JsonNode raw) =>
        new UnknownInteractionAnnotation(discriminator, raw);

    /// <inheritdoc />
    protected override JsonNode? GetUnknownRaw(InteractionAnnotation value) =>
        (value as UnknownInteractionAnnotation)?.Raw;
}

/// <summary>Converter for <see cref="InteractionStep"/>.</summary>
internal sealed class InteractionStepConverter : InteractionUnionConverter<InteractionStep>
{
    /// <inheritdoc />
    protected override Type? ResolveType(string discriminator) => discriminator switch
    {
        InteractionStepTypes.UserInput => typeof(InteractionUserInputStep),
        InteractionStepTypes.ModelOutput => typeof(InteractionModelOutputStep),
        InteractionStepTypes.Thought => typeof(InteractionThoughtStep),
        InteractionStepTypes.FunctionCall => typeof(InteractionFunctionCallStep),
        InteractionStepTypes.FunctionResult => typeof(InteractionFunctionResultStep),
        InteractionStepTypes.CodeExecutionCall => typeof(InteractionCodeExecutionCallStep),
        InteractionStepTypes.CodeExecutionResult => typeof(InteractionCodeExecutionResultStep),
        InteractionStepTypes.UrlContextCall => typeof(InteractionUrlContextCallStep),
        InteractionStepTypes.UrlContextResult => typeof(InteractionUrlContextResultStep),
        InteractionStepTypes.GoogleSearchCall => typeof(InteractionGoogleSearchCallStep),
        InteractionStepTypes.GoogleSearchResult => typeof(InteractionGoogleSearchResultStep),
        InteractionStepTypes.GoogleMapsCall => typeof(InteractionGoogleMapsCallStep),
        InteractionStepTypes.GoogleMapsResult => typeof(InteractionGoogleMapsResultStep),
        InteractionStepTypes.FileSearchCall => typeof(InteractionFileSearchCallStep),
        InteractionStepTypes.FileSearchResult => typeof(InteractionFileSearchResultStep),
        InteractionStepTypes.McpServerToolCall => typeof(InteractionMcpServerToolCallStep),
        InteractionStepTypes.McpServerToolResult => typeof(InteractionMcpServerToolResultStep),
        InteractionStepTypes.RetrievalCall => typeof(InteractionRetrievalCallStep),
        InteractionStepTypes.RetrievalResult => typeof(InteractionRetrievalResultStep),
        InteractionStepTypes.ProcessingCall => typeof(InteractionProcessingCallStep),
        InteractionStepTypes.ProcessingResult => typeof(InteractionProcessingResultStep),
        InteractionStepTypes.ToolSearchCall => typeof(InteractionToolSearchCallStep),
        InteractionStepTypes.ToolSearchResult => typeof(InteractionToolSearchResultStep),
        _ => null
    };

    /// <inheritdoc />
    protected override InteractionStep CreateUnknown(string? discriminator, JsonNode raw) =>
        new UnknownInteractionStep(discriminator, raw);

    /// <inheritdoc />
    protected override JsonNode? GetUnknownRaw(InteractionStep value) => (value as UnknownInteractionStep)?.Raw;
}

/// <summary>Converter for <see cref="InteractionTool"/>.</summary>
internal sealed class InteractionToolConverter : InteractionUnionConverter<InteractionTool>
{
    /// <inheritdoc />
    protected override Type? ResolveType(string discriminator) => discriminator switch
    {
        InteractionToolTypes.Function => typeof(InteractionFunctionTool),
        InteractionToolTypes.CodeExecution => typeof(InteractionCodeExecutionTool),
        InteractionToolTypes.UrlContext => typeof(InteractionUrlContextTool),
        InteractionToolTypes.GoogleSearch => typeof(InteractionGoogleSearchTool),
        InteractionToolTypes.GoogleMaps => typeof(InteractionGoogleMapsTool),
        InteractionToolTypes.FileSearch => typeof(InteractionFileSearchTool),
        InteractionToolTypes.ComputerUse => typeof(InteractionComputerUseTool),
        InteractionToolTypes.McpServer => typeof(InteractionMcpServerTool),
        InteractionToolTypes.Retrieval => typeof(InteractionRetrievalTool),
        InteractionToolTypes.ToolSearch => typeof(InteractionToolSearchTool),
        _ => null
    };

    /// <inheritdoc />
    protected override InteractionTool CreateUnknown(string? discriminator, JsonNode raw) =>
        new UnknownInteractionTool(discriminator, raw);

    /// <inheritdoc />
    protected override JsonNode? GetUnknownRaw(InteractionTool value) => (value as UnknownInteractionTool)?.Raw;
}

/// <summary>Converter for <see cref="InteractionAgentConfig"/>.</summary>
internal sealed class InteractionAgentConfigConverter : InteractionUnionConverter<InteractionAgentConfig>
{
    /// <inheritdoc />
    protected override Type? ResolveType(string discriminator) => discriminator switch
    {
        "deep-research" => typeof(DeepResearchAgentConfig),
        "antigravity" => typeof(AntigravityAgentConfig),
        "code-mender" => typeof(CodeMenderAgentConfig),
        "dynamic" => typeof(DynamicAgentConfig),
        _ => null
    };

    /// <inheritdoc />
    protected override InteractionAgentConfig CreateUnknown(string? discriminator, JsonNode raw) =>
        new UnknownInteractionAgentConfig(discriminator, raw);

    /// <inheritdoc />
    protected override JsonNode? GetUnknownRaw(InteractionAgentConfig value) =>
        (value as UnknownInteractionAgentConfig)?.Raw;
}

/// <summary>Converter for <see cref="InteractionResponseFormat"/>.</summary>
internal sealed class InteractionResponseFormatConverter : InteractionUnionConverter<InteractionResponseFormat>
{
    /// <inheritdoc />
    protected override Type? ResolveType(string discriminator) => discriminator switch
    {
        "text" => typeof(InteractionTextResponseFormat),
        "image" => typeof(InteractionImageResponseFormat),
        "audio" => typeof(InteractionAudioResponseFormat),
        "video" => typeof(InteractionVideoResponseFormat),
        _ => null
    };

    /// <inheritdoc />
    protected override InteractionResponseFormat CreateUnknown(string? discriminator, JsonNode raw) =>
        new UnknownInteractionResponseFormat(discriminator, raw);

    /// <inheritdoc />
    protected override JsonNode? GetUnknownRaw(InteractionResponseFormat value) =>
        (value as UnknownInteractionResponseFormat)?.Raw;
}

/// <summary>Converter for <see cref="InteractionSseEvent"/> (discriminated by <c>event_type</c>).</summary>
internal sealed class InteractionSseEventConverter : InteractionUnionConverter<InteractionSseEvent>
{
    /// <inheritdoc />
    protected override string DiscriminatorName => "event_type";

    /// <inheritdoc />
    protected override Type? ResolveType(string discriminator) => discriminator switch
    {
        InteractionEventTypes.InteractionCreated => typeof(InteractionCreatedEvent),
        InteractionEventTypes.InteractionCompleted => typeof(InteractionCompletedEvent),
        InteractionEventTypes.InteractionStatusUpdate => typeof(InteractionStatusUpdateEvent),
        InteractionEventTypes.StepStart => typeof(InteractionStepStartEvent),
        InteractionEventTypes.StepDelta => typeof(InteractionStepDeltaEvent),
        InteractionEventTypes.StepStop => typeof(InteractionStepStopEvent),
        InteractionEventTypes.Error => typeof(InteractionErrorEvent),
        InteractionEventTypes.InteractionConnected => typeof(InteractionConnectedEvent),
        InteractionEventTypes.InteractionCancelled => typeof(InteractionCancelledEvent),
        _ => null
    };

    /// <inheritdoc />
    /// <remarks>
    /// <c>interaction.created</c> and <c>interaction.completed</c> are shared by the streaming (SSE) events, which
    /// embed an <c>interaction</c> object, and the live interaction events, which don't.
    /// </remarks>
    protected override Type? ResolveType(string discriminator, JsonNode node)
    {
        var isLive = node is JsonObject obj && !obj.ContainsKey("interaction");
        return discriminator switch
        {
            InteractionEventTypes.InteractionCreated when isLive => typeof(LiveInteractionCreatedEvent),
            InteractionEventTypes.InteractionCompleted when isLive => typeof(LiveInteractionCompletedEvent),
            _ => ResolveType(discriminator)
        };
    }

    /// <inheritdoc />
    protected override InteractionSseEvent CreateUnknown(string? discriminator, JsonNode raw) =>
        new UnknownInteractionSseEvent(discriminator, raw);

    /// <inheritdoc />
    protected override JsonNode? GetUnknownRaw(InteractionSseEvent value) =>
        (value as UnknownInteractionSseEvent)?.Raw;
}

/// <summary>Converter for <see cref="InteractionStepDeltaData"/>.</summary>
internal sealed class InteractionStepDeltaDataConverter : InteractionUnionConverter<InteractionStepDeltaData>
{
    /// <inheritdoc />
    protected override Type? ResolveType(string discriminator) => discriminator switch
    {
        InteractionDeltaTypes.Text => typeof(InteractionTextDelta),
        InteractionDeltaTypes.Image => typeof(InteractionImageDelta),
        InteractionDeltaTypes.Audio => typeof(InteractionAudioDelta),
        InteractionDeltaTypes.Video => typeof(InteractionVideoDelta),
        InteractionDeltaTypes.Document => typeof(InteractionDocumentDelta),
        InteractionDeltaTypes.TextAnnotation => typeof(InteractionTextAnnotationDelta),
        InteractionDeltaTypes.Arguments => typeof(InteractionArgumentsDelta),
        InteractionDeltaTypes.ThoughtSignature => typeof(InteractionThoughtSignatureDelta),
        InteractionDeltaTypes.ThoughtSummary => typeof(InteractionThoughtSummaryDelta),
        InteractionDeltaTypes.FunctionResult => typeof(InteractionFunctionResultDelta),
        InteractionDeltaTypes.CodeExecutionCall => typeof(InteractionCodeExecutionCallDelta),
        InteractionDeltaTypes.CodeExecutionResult => typeof(InteractionCodeExecutionResultDelta),
        InteractionDeltaTypes.UrlContextCall => typeof(InteractionUrlContextCallDelta),
        InteractionDeltaTypes.UrlContextResult => typeof(InteractionUrlContextResultDelta),
        InteractionDeltaTypes.GoogleSearchCall => typeof(InteractionGoogleSearchCallDelta),
        InteractionDeltaTypes.GoogleSearchResult => typeof(InteractionGoogleSearchResultDelta),
        InteractionDeltaTypes.GoogleMapsCall => typeof(InteractionGoogleMapsCallDelta),
        InteractionDeltaTypes.GoogleMapsResult => typeof(InteractionGoogleMapsResultDelta),
        InteractionDeltaTypes.FileSearchCall => typeof(InteractionFileSearchCallDelta),
        InteractionDeltaTypes.FileSearchResult => typeof(InteractionFileSearchResultDelta),
        InteractionDeltaTypes.McpServerToolCall => typeof(InteractionMcpServerToolCallDelta),
        InteractionDeltaTypes.McpServerToolResult => typeof(InteractionMcpServerToolResultDelta),
        InteractionDeltaTypes.RetrievalCall => typeof(InteractionRetrievalCallDelta),
        InteractionDeltaTypes.RetrievalResult => typeof(InteractionRetrievalResultDelta),
        InteractionDeltaTypes.ProcessingCall => typeof(InteractionProcessingCallDelta),
        InteractionDeltaTypes.ProcessingResult => typeof(InteractionProcessingResultDelta),
        InteractionDeltaTypes.ToolSearchCall => typeof(InteractionToolSearchCallDelta),
        InteractionDeltaTypes.ToolSearchResult => typeof(InteractionToolSearchResultDelta),
        _ => null
    };

    /// <inheritdoc />
    protected override InteractionStepDeltaData CreateUnknown(string? discriminator, JsonNode raw) =>
        new UnknownInteractionStepDeltaData(discriminator, raw);

    /// <inheritdoc />
    protected override JsonNode? GetUnknownRaw(InteractionStepDeltaData value) =>
        (value as UnknownInteractionStepDeltaData)?.Raw;
}

/// <summary>
/// Converter for <see cref="InteractionInput"/>: a string, a single content object, a list of contents or a list
/// of steps.
/// </summary>
internal sealed class InteractionInputConverter : JsonConverter<InteractionInput>
{
    /// <inheritdoc />
    public override InteractionInput? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var node = JsonNode.Parse(ref reader);
        switch (node)
        {
            case JsonValue value when value.TryGetValue<string>(out var text):
                return new InteractionInput(text);
            case JsonObject:
                return new InteractionInput(node.Deserialize(InteractionJson.GetTypeInfo<InteractionContent>(options))!);
            case JsonArray array:
                // python-genai treats an array whose items are all content blocks as contents, otherwise as steps.
                var allContents = array.All(item => InteractionContentTypes.IsKnown(InteractionJson.GetString(item, "type")));
                if (allContents && array.Count > 0)
                    return InteractionInput.FromContents(
                        array.Deserialize(InteractionJson.GetTypeInfo<List<InteractionContent>>(options)) ?? new());
                return InteractionInput.FromSteps(
                    array.Deserialize(InteractionJson.GetTypeInfo<List<InteractionStep>>(options)) ?? new());
            default:
                throw new JsonException("Unsupported JSON for interaction input.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, InteractionInput value, JsonSerializerOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);
#else
        if (writer == null) throw new ArgumentNullException(nameof(writer));
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (options == null) throw new ArgumentNullException(nameof(options));
#endif
        if (value.Text != null)
            writer.WriteStringValue(value.Text);
        else if (value.Content != null)
            JsonSerializer.Serialize(writer, value.Content, InteractionJson.GetTypeInfo<InteractionContent>(options));
        else if (value.Contents != null)
            JsonSerializer.Serialize(writer, value.Contents, InteractionJson.GetTypeInfo<List<InteractionContent>>(options));
        else if (value.Steps != null)
            JsonSerializer.Serialize(writer, value.Steps, InteractionJson.GetTypeInfo<List<InteractionStep>>(options));
        else
            writer.WriteNullValue();
    }
}

/// <summary>
/// Converter for <see cref="InteractionEnvironmentReference"/>: either an environment ID string or an
/// <see cref="InteractionEnvironment"/> object.
/// </summary>
internal sealed class InteractionEnvironmentReferenceConverter : JsonConverter<InteractionEnvironmentReference>
{
    /// <inheritdoc />
    public override InteractionEnvironmentReference? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        if (reader.TokenType == JsonTokenType.String)
            return new InteractionEnvironmentReference(reader.GetString()!);
        var environment = JsonSerializer.Deserialize(ref reader, InteractionJson.GetTypeInfo<InteractionEnvironment>(options));
        return environment == null ? null : new InteractionEnvironmentReference(environment);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, InteractionEnvironmentReference value,
        JsonSerializerOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
#else
        if (writer == null) throw new ArgumentNullException(nameof(writer));
        if (value == null) throw new ArgumentNullException(nameof(value));
#endif
        if (value.EnvironmentId != null)
            writer.WriteStringValue(value.EnvironmentId);
        else if (value.Environment != null)
            JsonSerializer.Serialize(writer, value.Environment, InteractionJson.GetTypeInfo<InteractionEnvironment>(options));
        else
            writer.WriteNullValue();
    }
}

/// <summary>
/// Converter for <see cref="InteractionToolChoice"/>: either a mode string or an object with <c>allowed_tools</c>.
/// </summary>
internal sealed class InteractionToolChoiceConverter : JsonConverter<InteractionToolChoice>
{
    /// <inheritdoc />
    public override InteractionToolChoice? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        if (reader.TokenType == JsonTokenType.String)
            return new InteractionToolChoice { Mode = reader.GetString() };

        var node = JsonNode.Parse(ref reader);
        var allowed = node is JsonObject obj && obj.TryGetPropertyValue("allowed_tools", out var allowedNode)
            ? allowedNode?.Deserialize(InteractionJson.GetTypeInfo<InteractionAllowedTools>(options))
            : null;
        return new InteractionToolChoice { AllowedTools = allowed };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, InteractionToolChoice value, JsonSerializerOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
#else
        if (writer == null) throw new ArgumentNullException(nameof(writer));
        if (value == null) throw new ArgumentNullException(nameof(value));
#endif
        if (value.AllowedTools == null)
        {
            if (value.Mode == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value.Mode);
            return;
        }

        writer.WriteStartObject();
        writer.WritePropertyName("allowed_tools");
        JsonSerializer.Serialize(writer, value.AllowedTools, InteractionJson.GetTypeInfo<InteractionAllowedTools>(options));
        writer.WriteEndObject();
    }
}

/// <summary>
/// Converter for <c>response_format</c>, which the API accepts as a single format object or as a list of them.
/// A single format is written as an object, several as an array.
/// </summary>
internal sealed class InteractionResponseFormatListConverter : JsonConverter<List<InteractionResponseFormat>>
{
    /// <inheritdoc />
    public override List<InteractionResponseFormat>? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = new List<InteractionResponseFormat>();
            var itemInfo = InteractionJson.GetTypeInfo<InteractionResponseFormat>(options);
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                var item = JsonSerializer.Deserialize(ref reader, itemInfo);
                if (item != null)
                    list.Add(item);
            }

            return list;
        }

        var single = JsonSerializer.Deserialize(ref reader, InteractionJson.GetTypeInfo<InteractionResponseFormat>(options));
        return single == null ? new List<InteractionResponseFormat>() : new List<InteractionResponseFormat> { single };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, List<InteractionResponseFormat> value,
        JsonSerializerOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
#else
        if (writer == null) throw new ArgumentNullException(nameof(writer));
        if (value == null) throw new ArgumentNullException(nameof(value));
#endif
        var itemInfo = InteractionJson.GetTypeInfo<InteractionResponseFormat>(options);
        if (value.Count == 1)
        {
            JsonSerializer.Serialize(writer, value[0], itemInfo);
            return;
        }

        writer.WriteStartArray();
        foreach (var item in value)
            JsonSerializer.Serialize(writer, item, itemInfo);
        writer.WriteEndArray();
    }
}

/// <summary>Converter for <see cref="InteractionClientEvent"/> (discriminated by <c>event_type</c>).</summary>
internal sealed class InteractionClientEventConverter : InteractionUnionConverter<InteractionClientEvent>
{
    /// <inheritdoc />
    protected override string DiscriminatorName => "event_type";

    /// <inheritdoc />
    protected override Type? ResolveType(string discriminator) => discriminator switch
    {
        InteractionClientEventTypes.InteractionCreate => typeof(CreateInteractionEvent),
        InteractionClientEventTypes.InteractionConnect => typeof(ConnectInteractionEvent),
        InteractionClientEventTypes.InteractionCancel => typeof(CancelInteractionEvent),
        InteractionClientEventTypes.InteractionComplete => typeof(CompleteInteractionEvent),
        InteractionClientEventTypes.InteractionDisconnect => typeof(InteractionDisconnectEvent),
        InteractionClientEventTypes.Input => typeof(InteractionInputEvent),
        InteractionClientEventTypes.UserAction => typeof(InteractionUserActionInput),
        InteractionClientEventTypes.WorldSetup => typeof(InteractionWorldSetupEvent),
        _ => null
    };

    /// <inheritdoc />
    protected override InteractionClientEvent CreateUnknown(string? discriminator, JsonNode raw) =>
        new UnknownInteractionClientEvent(discriminator, raw);

    /// <inheritdoc />
    protected override JsonNode? GetUnknownRaw(InteractionClientEvent value) =>
        (value as UnknownInteractionClientEvent)?.Raw;
}

/// <summary>
/// Base converter for values that are either a string or an object of type <typeparamref name="TObject"/>.
/// </summary>
/// <typeparam name="TUnion">The union wrapper type.</typeparam>
/// <typeparam name="TObject">The object variant.</typeparam>
internal abstract class InteractionStringOrObjectConverter<TUnion, TObject> : JsonConverter<TUnion>
    where TUnion : class
    where TObject : class
{
    /// <summary>Creates the union from its string variant.</summary>
    protected abstract TUnion FromString(string value);

    /// <summary>Creates the union from its object variant.</summary>
    protected abstract TUnion FromObject(TObject value);

    /// <summary>Gets the string variant, or <c>null</c>.</summary>
    protected abstract string? GetString(TUnion value);

    /// <summary>Gets the object variant, or <c>null</c>.</summary>
    protected abstract TObject? GetObject(TUnion value);

    /// <inheritdoc />
    public override TUnion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        if (reader.TokenType == JsonTokenType.String)
            return FromString(reader.GetString()!);
        var obj = JsonSerializer.Deserialize(ref reader, InteractionJson.GetTypeInfo<TObject>(options));
        return obj == null ? null : FromObject(obj);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TUnion value, JsonSerializerOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
#else
        if (writer == null) throw new ArgumentNullException(nameof(writer));
        if (value == null) throw new ArgumentNullException(nameof(value));
#endif
        var text = GetString(value);
        var obj = GetObject(value);
        if (text != null)
            writer.WriteStringValue(text);
        else if (obj != null)
            JsonSerializer.Serialize(writer, obj, InteractionJson.GetTypeInfo<TObject>(options));
        else
            writer.WriteNullValue();
    }
}

/// <summary>Converter for <see cref="InteractionAudioProcessing"/>.</summary>
internal sealed class InteractionAudioProcessingConverter
    : InteractionStringOrObjectConverter<InteractionAudioProcessing, InteractionAudioStaticMediaProcessing>
{
    /// <inheritdoc />
    protected override InteractionAudioProcessing FromString(string value) => new(value);

    /// <inheritdoc />
    protected override InteractionAudioProcessing FromObject(InteractionAudioStaticMediaProcessing value) => new(value);

    /// <inheritdoc />
    protected override string? GetString(InteractionAudioProcessing value) => value.Mode;

    /// <inheritdoc />
    protected override InteractionAudioStaticMediaProcessing? GetObject(InteractionAudioProcessing value) => value.Options;
}

/// <summary>Converter for <see cref="InteractionDocumentProcessing"/>.</summary>
internal sealed class InteractionDocumentProcessingConverter
    : InteractionStringOrObjectConverter<InteractionDocumentProcessing, InteractionDocumentStaticMediaProcessing>
{
    /// <inheritdoc />
    protected override InteractionDocumentProcessing FromString(string value) => new(value);

    /// <inheritdoc />
    protected override InteractionDocumentProcessing FromObject(InteractionDocumentStaticMediaProcessing value) => new(value);

    /// <inheritdoc />
    protected override string? GetString(InteractionDocumentProcessing value) => value.Mode;

    /// <inheritdoc />
    protected override InteractionDocumentStaticMediaProcessing? GetObject(InteractionDocumentProcessing value) =>
        value.Options;
}

/// <summary>Converter for <see cref="InteractionWorldPrompt"/>.</summary>
internal sealed class InteractionWorldPromptConverter
    : InteractionStringOrObjectConverter<InteractionWorldPrompt, InteractionWorldPromptContent>
{
    /// <inheritdoc />
    protected override InteractionWorldPrompt FromString(string value) => new(value);

    /// <inheritdoc />
    protected override InteractionWorldPrompt FromObject(InteractionWorldPromptContent value) => new(value);

    /// <inheritdoc />
    protected override string? GetString(InteractionWorldPrompt value) => value.Text;

    /// <inheritdoc />
    protected override InteractionWorldPromptContent? GetObject(InteractionWorldPrompt value) => value.Content;
}

/// <summary>
/// Converter for <see cref="InteractionClientEventBatch"/>: an object with an <c>events</c> array (and no
/// <c>event_type</c>) is an <see cref="InteractionClientEventList"/>, anything else a single client event.
/// </summary>
internal sealed class InteractionClientEventBatchConverter : JsonConverter<InteractionClientEventBatch>
{
    /// <inheritdoc />
    public override InteractionClientEventBatch? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var node = JsonNode.Parse(ref reader);
        if (node is JsonObject obj && !obj.ContainsKey("event_type") &&
            obj.TryGetPropertyValue("events", out var events) && events is JsonArray)
        {
            var list = node.Deserialize(InteractionJson.GetTypeInfo<InteractionClientEventList>(options));
            return list == null ? null : new InteractionClientEventBatch(list);
        }

        var single = node?.Deserialize(InteractionJson.GetTypeInfo<InteractionClientEvent>(options));
        return single == null ? null : new InteractionClientEventBatch(single);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, InteractionClientEventBatch value, JsonSerializerOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
#else
        if (writer == null) throw new ArgumentNullException(nameof(writer));
        if (value == null) throw new ArgumentNullException(nameof(value));
#endif
        if (value.Event != null)
            JsonSerializer.Serialize(writer, value.Event, InteractionJson.GetTypeInfo<InteractionClientEvent>(options));
        else if (value.EventList != null)
            JsonSerializer.Serialize(writer, value.EventList, InteractionJson.GetTypeInfo<InteractionClientEventList>(options));
        else
            writer.WriteNullValue();
    }
}
