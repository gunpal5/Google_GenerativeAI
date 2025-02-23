namespace GenerativeAI.Types;

/// <summary>
/// Represents the allowed generation of people by the model.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/imagen-api">See Official API Documentation</seealso>
public enum PersonGeneration
{
    /// <summary>
    /// Disallow the inclusion of people or faces in images.
    /// </summary>
    dont_allow,
    /// <summary>
    /// Allow generation of adults only.
    /// </summary>
    allow_adult,
    /// <summary>
    /// Allow generation of people of all ages.
    /// </summary>
    allow_all
}