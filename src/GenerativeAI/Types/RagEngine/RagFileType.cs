using System.Runtime.Serialization;

namespace GenerativeAI.Types.RagEngine;

public enum RagFileType
{

    [EnumMember(Value = @"RAG_FILE_TYPE_UNSPECIFIED")]
    RAG_FILE_TYPE_UNSPECIFIED = 0,

    [EnumMember(Value = @"RAG_FILE_TYPE_TXT")]
    RAG_FILE_TYPE_TXT = 1,

    [EnumMember(Value = @"RAG_FILE_TYPE_PDF")]
    RAG_FILE_TYPE_PDF = 2,

}