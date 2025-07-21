using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Specifies the type of file in the RAG engine.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<RagFileType>))]
public enum RagFileType
{

    /// <summary>
    /// The file type is unspecified.
    /// </summary>
    [EnumMember(Value = @"RAG_FILE_TYPE_UNSPECIFIED")]
    RAG_FILE_TYPE_UNSPECIFIED = 0,

    /// <summary>
    /// The file is a text file.
    /// </summary>
    [EnumMember(Value = @"RAG_FILE_TYPE_TXT")]
    RAG_FILE_TYPE_TXT = 1,

    /// <summary>
    /// The file is a PDF document.
    /// </summary>
    [EnumMember(Value = @"RAG_FILE_TYPE_PDF")]
    RAG_FILE_TYPE_PDF = 2,

}