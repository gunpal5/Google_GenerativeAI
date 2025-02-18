namespace GenerativeAI;

/// <summary>
/// Represents a static class containing string constants for different versions of the API.
/// These versions are to be used across the application for interacting with services
/// or constructing API request URLs where versioning is required.
/// </summary>
public static class ApiVersions
{
    /// <summary>
    /// Represents the version string "v1", which can be used to specify the first stable version
    /// of the API when interacting with services or constructing API request URLs.
    /// </summary>
    public const string v1 = "v1";

    /// <summary>
    /// Represents the version string "v1beta", which can be used to specify
    /// the beta iteration of the version 1 API when interacting with services
    /// or constructing API request URLs.
    /// </summary>
    public const string v1Beta = "v1beta";

    /// <summary>
    /// Represents the version string "v1beta1", which can be used to specify the first beta version
    /// of the API's features when interacting with services or constructing API request URLs.
    /// </summary>
    public const string v1Beta1 = "v1beta1";
}