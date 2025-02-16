using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Defines types of the grantee of this permission.
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/permissions#Permission.GranteeType">See Official API Documentation</seealso>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GranteeType
{
    /// <summary>
    /// The default value. This value is unused.
    /// </summary>
    GRANTEE_TYPE_UNSPECIFIED,

    /// <summary>
    /// Represents a user. When set, you must provide <see cref="GenerativeAI.Types.Permission.EmailAddress"/> for the user.
    /// </summary>
    USER,

    /// <summary>
    /// Represents a group. When set, you must provide <see cref="GenerativeAI.Types.Permission.EmailAddress"/> for the group.
    /// </summary>
    GROUP,

    /// <summary>
    /// Represents access to everyone. No extra information is required.
    /// </summary>
    EVERYONE
}