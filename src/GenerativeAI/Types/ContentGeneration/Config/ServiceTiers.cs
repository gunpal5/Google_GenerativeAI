namespace GenerativeAI.Types;

/// <summary>
/// Values for <see cref="GenerateContentRequest.ServiceTier"/>.
/// </summary>
public static class ServiceTiers
{
    /// <summary>
    /// Standard processing (the default).
    /// </summary>
    public const string Standard = "standard";

    /// <summary>
    /// Flex processing: lower cost, with higher and more variable latency.
    /// </summary>
    public const string Flex = "flex";

    /// <summary>
    /// Priority processing: lower latency at a higher price.
    /// </summary>
    public const string Priority = "priority";
}
