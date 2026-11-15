namespace GenerativeAI.Types;

/// <summary>
/// Known event types a <see cref="Webhook"/> can subscribe to (see <see cref="Webhook.SubscribedEvents"/>).
/// The API may add new events, so events are exposed as strings.
/// </summary>
public static class WebhookEvents
{
    /// <summary>Batch processing finished successfully.</summary>
    public const string BatchSucceeded = "batch.succeeded";

    /// <summary>The batch has not been processed within the 48h timeframe.</summary>
    public const string BatchExpired = "batch.expired";

    /// <summary>The batch job failed.</summary>
    public const string BatchFailed = "batch.failed";

    /// <summary>The interaction requires action (e.g. function calling).</summary>
    public const string InteractionRequiresAction = "interaction.requires_action";

    /// <summary>The interaction completed successfully.</summary>
    public const string InteractionCompleted = "interaction.completed";

    /// <summary>The interaction failed.</summary>
    public const string InteractionFailed = "interaction.failed";

    /// <summary>Video generation completed.</summary>
    public const string VideoGenerated = "video.generated";
}

/// <summary>
/// Known values of <see cref="Webhook.State"/> and <see cref="WebhookUpdate.State"/>. The API may return values
/// that are not listed here, so the state is exposed as a string.
/// </summary>
public static class WebhookStates
{
    /// <summary>The webhook is enabled and receives events.</summary>
    public const string Enabled = "enabled";

    /// <summary>The webhook was disabled by the user.</summary>
    public const string Disabled = "disabled";

    /// <summary>The webhook was disabled by the service because deliveries kept failing.</summary>
    public const string DisabledDueToFailedDeliveries = "disabled_due_to_failed_deliveries";
}

/// <summary>
/// Known values of <see cref="RotateSigningSecretRequest.RevocationBehavior"/>.
/// </summary>
public static class WebhookRevocationBehaviors
{
    /// <summary>Previous signing secrets are revoked 24 hours after the rotation.</summary>
    public const string RevokePreviousSecretsAfterH24 = "revoke_previous_secrets_after_h24";

    /// <summary>Previous signing secrets are revoked immediately.</summary>
    public const string RevokePreviousSecretsImmediately = "revoke_previous_secrets_immediately";
}
