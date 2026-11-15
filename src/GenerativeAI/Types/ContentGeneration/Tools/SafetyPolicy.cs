using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Computer Use safety policies that can be disabled through <see cref="ComputerUse.DisabledSafetyPolicies"/>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<SafetyPolicy>))]
public enum SafetyPolicy
{
    /// <summary>
    /// The safety policy is unspecified.
    /// </summary>
    SAFETY_POLICY_UNSPECIFIED,

    /// <summary>
    /// Confirmation before financial transactions.
    /// </summary>
    FINANCIAL_TRANSACTIONS,

    /// <summary>
    /// Confirmation before modifying sensitive data.
    /// </summary>
    SENSITIVE_DATA_MODIFICATION,

    /// <summary>
    /// Confirmation before using communication tools (for example sending messages).
    /// </summary>
    COMMUNICATION_TOOL,

    /// <summary>
    /// Confirmation before creating accounts.
    /// </summary>
    ACCOUNT_CREATION,

    /// <summary>
    /// Confirmation before modifying data.
    /// </summary>
    DATA_MODIFICATION,

    /// <summary>
    /// Confirmation before changing user consent settings.
    /// </summary>
    USER_CONSENT_MANAGEMENT,

    /// <summary>
    /// Confirmation before accepting legal terms and agreements.
    /// </summary>
    LEGAL_TERMS_AND_AGREEMENTS
}
