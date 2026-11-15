using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Source-generated serialization metadata for the File Search stores and documents types.
/// </summary>
/// <remarks>
/// This is a separate context rather than a partial of <see cref="TypesSerializerContext"/> because the
/// System.Text.Json source generator shipped for net6.0-net10.0/netstandard2.0 fails when
/// <c>[JsonSerializable]</c> attributes are split across partial declarations (duplicate hint names).
/// <see cref="GenerativeAI.Clients.FileSearchStoresClient"/> appends it to its serializer resolver chain.
/// Options mirror those of <see cref="TypesSerializerContext"/>.
/// </remarks>
[JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(FileSearchStore))]
[JsonSerializable(typeof(FileSearchDocument))]
[JsonSerializable(typeof(FileSearchDocumentState))]
[JsonSerializable(typeof(ChunkingConfig))]
[JsonSerializable(typeof(WhiteSpaceConfig))]
[JsonSerializable(typeof(UploadToFileSearchStoreConfig))]
[JsonSerializable(typeof(ImportFileConfig))]
[JsonSerializable(typeof(ImportFileRequest))]
[JsonSerializable(typeof(ListFileSearchStoresResponse))]
[JsonSerializable(typeof(ListFileSearchDocumentsResponse))]
[JsonSerializable(typeof(FileSearchOperation))]
[JsonSerializable(typeof(FileSearchOperationResponse))]
public partial class FileSearchSerializerContext : JsonSerializerContext
{
}
