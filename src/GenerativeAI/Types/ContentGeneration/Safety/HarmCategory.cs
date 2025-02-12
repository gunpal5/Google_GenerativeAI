using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The category of a rating.
/// These categories cover various kinds of harms that developers may wish to adjust.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#harmcategory">See Official API Documentation</seealso>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HarmCategory
{
    /// <summary>
    /// Category is unspecified.
    /// </summary>
    HARM_CATEGORY_UNSPECIFIED = 0,

    /// <summary>
    /// <b>PaLM</b> - Negative or harmful comments targeting identity and/or protected attribute.
    /// </summary>
    HARM_CATEGORY_DEROGATORY = 1,

    /// <summary>
    /// <b>PaLM</b> - Content that is rude, disrespectful, or profane.
    /// </summary>
    HARM_CATEGORY_TOXICITY = 2,

    /// <summary>
    /// <b>PaLM</b> - Describes scenarios depicting violence against an individual or group,
    /// or general descriptions of gore.
    /// </summary>
    HARM_CATEGORY_VIOLENCE = 3,

    /// <summary>
    /// <b>PaLM</b> - Contains references to sexual acts or other lewd content.
    /// </summary>
    HARM_CATEGORY_SEXUAL = 4,

    /// <summary>
    /// <b>PaLM</b> - Promotes unchecked medical advice.
    /// </summary>
    HARM_CATEGORY_MEDICAL = 5,

    /// <summary>
    /// <b>PaLM</b> - Dangerous content that promotes, facilitates, or encourages harmful acts.
    /// </summary>
    HARM_CATEGORY_DANGEROUS = 6,

    /// <summary>
    /// <b>Gemini</b> - Harassment content.
    /// </summary>
    HARM_CATEGORY_HARASSMENT = 7,

    /// <summary>
    /// <b>Gemini</b>  - Hate speech and content.
    /// </summary>
    HARM_CATEGORY_HATE_SPEECH = 8,

    /// <summary>
    /// <b>Gemini</b>  - Sexually explicit content.
    /// </summary>
    HARM_CATEGORY_SEXUALLY_EXPLICIT = 9,

    /// <summary>
    /// <b>Gemini</b> - Dangerous content.
    /// </summary>
    HARM_CATEGORY_DANGEROUS_CONTENT = 10,

    /// <summary>
    /// <b>Gemini</b>  - Content that may be used to harm civic integrity.
    /// </summary>
    HARM_CATEGORY_CIVIC_INTEGRITY = 11,
}