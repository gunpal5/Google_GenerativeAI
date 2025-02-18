namespace GenerativeAI;

/// <summary>
/// Defines constant role identifiers used in the Generative AI system.
/// </summary>
public class Roles
{
    /// <summary>
    /// Represents the role of a user interacting with the system.
    /// </summary>
    public const string User = "user";

    /// <summary>
    /// Represents the role assigned to the AI model in the system.
    /// </summary>
    public const string Model = "model";

    /// <summary>
    /// Represents the role for functions invoked during the system's operation.
    /// </summary>
    public const string Function = "function";

    /// <summary>
    /// Represents the system's internal role for handling instructions or operations.
    /// </summary>
    public const string System = "system";
}