using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// An individual SharePointSource.
/// </summary>
public class SharePointSource
{
    /// <summary>
    /// The Application ID for the app registered in Microsoft Azure Portal. The application must also be configured with MS Graph permissions "Files.ReadAll", "Sites.ReadAll" and BrowserSiteLists.Read.All.
    /// </summary>
    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; } 

    /// <summary>
    /// The application secret for the app registered in Azure.
    /// </summary>
    [JsonPropertyName("clientSecret")]
    public ApiAuthApiKeyConfig? ClientSecret { get; set; } 

    /// <summary>
    /// The ID of the drive to download from.
    /// </summary>
    [JsonPropertyName("driveId")]
    public string? DriveId { get; set; } 

    /// <summary>
    /// The name of the drive to download from.
    /// </summary>
    [JsonPropertyName("driveName")]
    public string? DriveName { get; set; } 

    /// <summary>
    /// Output only. The SharePoint file id. Output only.
    /// </summary>
    [JsonPropertyName("fileId")]
    public string? FileId { get; set; } 

    /// <summary>
    /// The ID of the SharePoint folder to download from.
    /// </summary>
    [JsonPropertyName("sharepointFolderId")]
    public string? SharepointFolderId { get; set; } 

    /// <summary>
    /// The path of the SharePoint folder to download from.
    /// </summary>
    [JsonPropertyName("sharepointFolderPath")]
    public string? SharepointFolderPath { get; set; } 

    /// <summary>
    /// The name of the SharePoint site to download from. This can be the site name or the site id.
    /// </summary>
    [JsonPropertyName("sharepointSiteName")]
    public string? SharepointSiteName { get; set; } 

    /// <summary>
    /// Unique identifier of the Azure Active Directory Instance.
    /// </summary>
    [JsonPropertyName("tenantId")]
    public string? TenantId { get; set; } 
}