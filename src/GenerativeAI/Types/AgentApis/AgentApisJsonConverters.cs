using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types.Converters;

/// <summary>Converter for <see cref="DecisionQuestion"/>.</summary>
internal sealed class DecisionQuestionConverter : InteractionUnionConverter<DecisionQuestion>
{
    /// <inheritdoc />
    protected override Type? ResolveType(string discriminator) => discriminator switch
    {
        DecisionQuestionTypes.Noul => typeof(NoulDecisionQuestion),
        DecisionQuestionTypes.Choice => typeof(ChoiceDecisionQuestion),
        DecisionQuestionTypes.Score => typeof(ScoreDecisionQuestion),
        _ => null
    };

    /// <inheritdoc />
    protected override DecisionQuestion CreateUnknown(string? discriminator, JsonNode raw) =>
        new UnknownDecisionQuestion(discriminator, raw);

    /// <inheritdoc />
    protected override JsonNode? GetUnknownRaw(DecisionQuestion value) => (value as UnknownDecisionQuestion)?.Raw;
}

/// <summary>Converter for <see cref="DecisionAnswer"/>.</summary>
internal sealed class DecisionAnswerConverter : InteractionUnionConverter<DecisionAnswer>
{
    /// <inheritdoc />
    protected override Type? ResolveType(string discriminator) => discriminator switch
    {
        DecisionQuestionTypes.Noul => typeof(NoulDecisionAnswer),
        DecisionQuestionTypes.Choice => typeof(ChoiceDecisionAnswer),
        DecisionQuestionTypes.Score => typeof(ScoreDecisionAnswer),
        _ => null
    };

    /// <inheritdoc />
    protected override DecisionAnswer CreateUnknown(string? discriminator, JsonNode raw) =>
        new UnknownDecisionAnswer(discriminator, raw);

    /// <inheritdoc />
    protected override JsonNode? GetUnknownRaw(DecisionAnswer value) => (value as UnknownDecisionAnswer)?.Raw;
}

/// <summary>Converter for <see cref="CredentialConfig"/>.</summary>
internal sealed class CredentialConfigConverter : InteractionUnionConverter<CredentialConfig>
{
    /// <inheritdoc />
    protected override Type? ResolveType(string discriminator) => discriminator switch
    {
        CredentialTypes.EnvironmentVariable => typeof(EnvironmentVariableCredentialConfig),
        CredentialTypes.BearerToken => typeof(BearerTokenCredentialConfig),
        CredentialTypes.OAuth2 => typeof(OAuth2CredentialConfig),
        _ => null
    };

    /// <inheritdoc />
    protected override CredentialConfig CreateUnknown(string? discriminator, JsonNode raw) =>
        new UnknownCredentialConfig(discriminator, raw);

    /// <inheritdoc />
    protected override JsonNode? GetUnknownRaw(CredentialConfig value) => (value as UnknownCredentialConfig)?.Raw;
}

/// <summary>
/// Reads a string or an array of strings into a list; always writes an array.
/// </summary>
internal sealed class StringOrStringListConverter : JsonConverter<List<string>>
{
    /// <inheritdoc />
    public override List<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.String:
                return new List<string> { reader.GetString()! };
            case JsonTokenType.StartArray:
                var list = new List<string>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    if (reader.TokenType == JsonTokenType.String)
                        list.Add(reader.GetString()!);
                    else
                        reader.Skip();
                }

                return list;
            default:
                throw new JsonException("Expected a string or an array of strings.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
#else
        if (writer == null) throw new ArgumentNullException(nameof(writer));
        if (value == null) throw new ArgumentNullException(nameof(value));
#endif
        writer.WriteStartArray();
        foreach (var item in value)
            writer.WriteStringValue(item);
        writer.WriteEndArray();
    }
}
