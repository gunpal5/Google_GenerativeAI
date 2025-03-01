using GenerativeAI.Types.RagEngine;

namespace GenerativeAI;

/// <summary>
/// Extension methods for adding various data sources to an <see cref="ImportRagFilesRequest"/>.
/// </summary>
public static class ImportRagFilesRequestExtensions
{
    /// <summary>
    /// Adds a Jira source to the <see cref="ImportRagFilesRequest"/>.
    /// </summary>
    /// <param name="request">The request to modify.</param>
    /// <param name="source">The Jira source to add.</param>
    public static void AddSource(this ImportRagFilesRequest request, JiraSource source)
    {
        if (request.ImportRagFilesConfig == null)
            request.ImportRagFilesConfig = new ImportRagFilesConfig();
        request.ImportRagFilesConfig.JiraSource = source;
    }

    /// <summary>
    /// Adds a Google Cloud Storage (GCS) source to the <see cref="ImportRagFilesRequest"/>.
    /// </summary>
    /// <param name="request">The request to modify.</param>
    /// <param name="source">The GCS source to add.</param>
    public static void AddSource(this ImportRagFilesRequest request, GcsSource source)
    {
        if (request.ImportRagFilesConfig == null)
            request.ImportRagFilesConfig = new ImportRagFilesConfig();
        request.ImportRagFilesConfig.GcsSource = source;
    }

    /// <summary>
    /// Adds a Google Drive source to the <see cref="ImportRagFilesRequest"/>.
    /// </summary>
    /// <param name="request">The request to modify.</param>
    /// <param name="source">The Google Drive source to add.</param>
    public static void AddSource(this ImportRagFilesRequest request, GoogleDriveSource source)
    {
        if (request.ImportRagFilesConfig == null)
            request.ImportRagFilesConfig = new ImportRagFilesConfig();
        request.ImportRagFilesConfig.GoogleDriveSource = source;
    }

    /// <summary>
    /// Adds a Slack channel to the <see cref="ImportRagFilesRequest"/>.
    /// </summary>
    /// <param name="request">The request to modify.</param>
    /// <param name="slackChannel">The Slack channel to add.</param>
    public static void AddSource(this ImportRagFilesRequest request, SlackSourceSlackChannels slackChannel)
    {
        AddSource(request, new[] { slackChannel });
    }

    /// <summary>
    /// Adds multiple Slack channels to the <see cref="ImportRagFilesRequest"/>.
    /// </summary>
    /// <param name="request">The request to modify.</param>
    /// <param name="slackChannels">The Slack channels to add.</param>
    public static void AddSource(this ImportRagFilesRequest request, IEnumerable<SlackSourceSlackChannels> slackChannels)
    {
        if (request.ImportRagFilesConfig == null)
            request.ImportRagFilesConfig = new ImportRagFilesConfig();
        request.ImportRagFilesConfig.SlackSource = new SlackSource()
        {
            Channels = slackChannels.ToList()
        };
    }

    /// <summary>
    /// Adds a Slack source to the <see cref="ImportRagFilesRequest"/>.
    /// </summary>
    /// <param name="request">The request to modify.</param>
    /// <param name="slackSource">The Slack source to add.</param>
    public static void AddSource(this ImportRagFilesRequest request, SlackSource slackSource)
    {
        if (request.ImportRagFilesConfig == null)
            request.ImportRagFilesConfig = new ImportRagFilesConfig();
        request.ImportRagFilesConfig.SlackSource = slackSource;
    }

    /// <summary>
    /// Adds a SharePoint source to the <see cref="ImportRagFilesRequest"/>.
    /// </summary>
    /// <param name="request">The request to modify.</param>
    /// <param name="source">The SharePoint source to add.</param>
    public static void AddSource(this ImportRagFilesRequest request, SharePointSource source)
    {
        AddSource(request, new[] { source });
    }

    /// <summary>
    /// Adds multiple SharePoint sources to the <see cref="ImportRagFilesRequest"/>.
    /// </summary>
    /// <param name="request">The request to modify.</param>
    /// <param name="source">The SharePoint sources to add.</param>
    public static void AddSource(this ImportRagFilesRequest request, IEnumerable<SharePointSource> source)
    {
        if (request.ImportRagFilesConfig == null)
            request.ImportRagFilesConfig = new ImportRagFilesConfig();
        request.ImportRagFilesConfig.SharePointSources = new SharePointSources()
        {
            SharePointSourceCollection = source.ToList()
        };
    }

    /// <summary>
    /// Adds a SharePoint source to the <see cref="ImportRagFilesRequest"/>.
    /// </summary>
    /// <param name="request">The request to modify.</param>
    /// <param name="sources">The SharePoint sources to add.</param>
    public static void AddSource(this ImportRagFilesRequest request, SharePointSources sources)
    {
        if (request.ImportRagFilesConfig == null)
            request.ImportRagFilesConfig = new ImportRagFilesConfig();
        request.ImportRagFilesConfig.SharePointSources = sources;
    }

    /// <summary>
    /// Adds multiple GCS URIs to the <see cref="ImportRagFilesRequest"/>.
    /// </summary>
    /// <param name="request">The request to modify.</param>
    /// <param name="gcsUris">The list of GCS URIs to add.</param>
    public static void AddGcsSource(this ImportRagFilesRequest request, IEnumerable<string> gcsUris)
    {
        if (request.ImportRagFilesConfig == null)
            request.ImportRagFilesConfig = new ImportRagFilesConfig();
        request.ImportRagFilesConfig.GcsSource = new GcsSource()
        {
            Uris = gcsUris.ToList()
        };
    }

    /// <summary>
    /// Adds a single GCS URI to the <see cref="ImportRagFilesRequest"/>.
    /// </summary>
    /// <param name="request">The request to modify.</param>
    /// <param name="gcsUri">The GCS URI to add.</param>
    public static void AddGcsSource(this ImportRagFilesRequest request, string gcsUri)
    {
        if (request.ImportRagFilesConfig == null)
            request.ImportRagFilesConfig = new ImportRagFilesConfig();
        request.ImportRagFilesConfig.GcsSource = new GcsSource()
        {
            Uris = new List<string>() { gcsUri }
        };
    }

    /// <summary>
    /// Adds a Google Drive resource ID to the <see cref="ImportRagFilesRequest"/>.
    /// </summary>
    /// <param name="request">The request to modify.</param>
    /// <param name="resourceId">The Google Drive resource ID to add.</param>
    public static void AddGooglDriveSource(this ImportRagFilesRequest request, GoogleDriveSourceResourceId resourceId)
    {
        AddGooglDriveSource(request, new[] { resourceId });
    }

    /// <summary>
    /// Adds multiple Google Drive resource IDs to the <see cref="ImportRagFilesRequest"/>.
    /// </summary>
    /// <param name="request">The request to modify.</param>
    /// <param name="resourceIds">The Google Drive resource IDs to add.</param>
    public static void AddGooglDriveSource(this ImportRagFilesRequest request, IEnumerable<GoogleDriveSourceResourceId> resourceIds)
    {
        if (request.ImportRagFilesConfig == null)
            request.ImportRagFilesConfig = new ImportRagFilesConfig();
        request.ImportRagFilesConfig.GoogleDriveSource = new GoogleDriveSource()
        {
            ResourceIds = resourceIds.ToList()
        };
    }
    
    
}