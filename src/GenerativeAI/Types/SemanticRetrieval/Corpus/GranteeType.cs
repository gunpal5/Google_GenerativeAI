namespace GenerativeAI.Types.SemanticRetrieval.Corpus;

/// <summary>
/// Defines types of the grantee of this permission.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/corpora#Permission.GranteeType">See Official API Documentation</seealso>
public enum GranteeType
{
    /// <summary>
    /// The default value. This value is unused.
    /// </summary>
    GRANTEE_TYPE_UNSPECIFIED,

    /// <summary>
    /// Represents a user. When set, you must provide <see cref="Permission.EmailAddress"/> for the user.
    /// </summary>
    USER,

    /// <summary>
    /// Represents a group. When set, you must provide <see cref="Permission.EmailAddress"/> for the group.
    /// </summary>
    GROUP,

    /// <summary>
    /// Represents access to everyone. No extra information is required.
    /// </summary>
    EVERYONE
}