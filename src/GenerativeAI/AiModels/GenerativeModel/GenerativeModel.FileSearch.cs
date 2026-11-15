using GenerativeAI.Types;

namespace GenerativeAI;

public partial class GenerativeModel
{
    /// <summary>
    /// Grounds every request on the given File Search stores by adding a <see cref="FileSearch"/> tool
    /// (replacing any previously configured <see cref="RetrievalTool"/>).
    /// </summary>
    /// <param name="fileSearchStoreNames">Store names, either <c>fileSearchStores/{id}</c> or just <c>{id}</c>.</param>
    /// <param name="topK">Optional number of chunks to retrieve.</param>
    /// <param name="metadataFilter">Optional AIP-160 filter over document custom metadata, e.g. <c>author = "Robert Graves"</c>.</param>
    /// <exception cref="NotSupportedException">The model is configured for Vertex AI, which does not support File Search stores.</exception>
    /// <exception cref="ArgumentException">No store names were given.</exception>
    public void UseFileSearch(IEnumerable<string> fileSearchStoreNames, int? topK = null, string? metadataFilter = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(fileSearchStoreNames);
#else
        if (fileSearchStoreNames == null) throw new ArgumentNullException(nameof(fileSearchStoreNames));
#endif
        if (this.Platform is VertextPlatformAdapter)
            throw new NotSupportedException("File Search stores are only supported by the Gemini Developer API, not Vertex AI.");

        var names = fileSearchStoreNames
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.StartsWith("fileSearchStores/", StringComparison.Ordinal) ? n : "fileSearchStores/" + n)
            .ToList();
        if (names.Count == 0)
            throw new ArgumentException("At least one File Search store name is required.", nameof(fileSearchStoreNames));

        this.RetrievalTool = new Tool
        {
            FileSearch = new FileSearch
            {
                FileSearchStoreNames = names,
                TopK = topK,
                MetadataFilter = metadataFilter
            }
        };
    }

    /// <summary>
    /// Grounds every request on the given File Search stores by adding a <see cref="FileSearch"/> tool.
    /// </summary>
    /// <param name="fileSearchStoreNames">Store names, either <c>fileSearchStores/{id}</c> or just <c>{id}</c>.</param>
    public void UseFileSearch(params string[] fileSearchStoreNames)
    {
        UseFileSearch((IEnumerable<string>)fileSearchStoreNames);
    }
}
