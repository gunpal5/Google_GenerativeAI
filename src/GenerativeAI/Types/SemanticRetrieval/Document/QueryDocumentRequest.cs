using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Request body for performing semantic search over a <see cref="Document"/>.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/documents#request-body_1">See Official API Documentation</seealso>
public class QueryDocumentRequest
{
    /// <summary>
    /// Query string to perform semantic search. Required.
    /// </summary>
    [JsonPropertyName("query")]
    public string Query { get; set; }

    /// <summary>
    /// The maximum number of <see cref="RelevantChunk"/>s to return. The service may return fewer <see cref="RelevantChunk"/>s.
    /// If unspecified, at most 10 <see cref="RelevantChunk"/>s will be returned. The maximum specified result count is 100.
    /// </summary>
    [JsonPropertyName("resultsCount")]
    public int? ResultsCount { get; set; }

    /// <summary>
    /// Filter for <see cref="RelevantChunk"/> metadata. Each <see cref="MetadataFilter"/> object should correspond to a unique key. Multiple <see cref="MetadataFilter"/> objects are joined by logical "AND"s.
    /// <br/>
    /// Note: <see cref="Document"/>-level filtering is not supported for this request because a <see cref="Document"/> name is already specified.
    /// <br/>
    /// Example query: (year &gt;= 2020 OR year &lt; 2010) AND (genre = drama OR genre = action)
    /// <br/>
    /// <see cref="MetadataFilter"/> object list: metadataFilters = [ {key = "chunk.custom_metadata.year" conditions = [{int_value = 2020, operation = GREATER_EQUAL}, {int_value = 2010, operation = LESS}}, {key = "chunk.custom_metadata.genre" conditions = [{stringValue = "drama", operation = EQUAL}, {stringValue = "action", operation = EQUAL}}]
    /// <br/>
    /// Example query for a numeric range of values: (year &gt; 2015 AND year &lt;= 2020)
    /// <br/>
    /// <see cref="MetadataFilter"/> object list: metadataFilters = [ {key = "chunk.custom_metadata.year" conditions = [{int_value = 2015, operation = GREATER}]}, {key = "chunk.custom_metadata.year" conditions = [{int_value = 2020, operation = LESS_EQUAL}]}]
    /// <br/>
    /// Note: "AND"s for the same key are only supported for numeric values. String values only support "OR"s for the same key.
    /// </summary>
    [JsonPropertyName("metadataFilters")]
    public List<MetadataFilter>? MetadataFilters { get; set; }
}