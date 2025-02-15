namespace GenerativeAI.Core;

/// <summary>
/// Represents the behavior and capability configuration for function-calling mechanisms
/// within the system.
/// </summary>
/// <remarks>
/// This class provides toggles to control the automatic execution and handling of
/// functions, as well as enabling or disabling specific function-call features.
/// </remarks>
public class FunctionCallingBehaviour
{
    // Used to show or hide function-call capabilities
    /// <summary>
    /// Determines whether function-call capabilities are automatically invoked in the generative process.
    /// This property enables or disables the auto-execution of predefined functions as part of the model's response generation.
    /// </summary>
    /// <remarks>
    /// Can be used to enhance workflow automation by allowing predefined functions to operate without manual invocation.
    /// </remarks>
    public bool AutoCallFunction { get; set; } = true;

    /// <summary>
    /// Enables or disables automatic function replies. When enabled, the model automatically generates responses
    /// for functions that are invoked during an interaction without requiring explicit prompts.
    /// </summary>
    /// <remarks>
    /// Useful for streamlining automated workflows involving generative AI functions.
    /// </remarks>
    public bool AutoReplyFunction { get; set; } = true;

    /// <summary>
    /// Determines whether function-based tools and capabilities are enabled within the generative model.
    /// These tools can include processing mechanisms that enhance content generation or facilitate specific operations.
    /// </summary>
    /// <remarks>
    /// Disabling this property prevents the model from utilizing any function-based features.
    /// </remarks>
    public bool FunctionEnabled { get; set; } = true;

    /// <summary>
    /// Determines whether the system automatically resolves issues caused by improper or unrecognized function calls.
    /// When enabled, it helps ensure smoother operation by handling invalid or malformed function requests.
    /// </summary>
    /// <remarks>
    /// Useful in scenarios where user-defined functions or external integrations might encounter unexpected failures.
    /// </remarks>
    public bool AutoHandleBadFunctionCalls { get; set; } = false;
}