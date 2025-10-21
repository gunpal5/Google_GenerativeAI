using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a customer-managed encryption key spec that can be applied to a top-level resource.
/// </summary>
public class EncryptionSpec
{
    /// <summary>
    /// Required. The Cloud KMS resource identifier of the customer managed encryption key used to protect a resource.
    /// Has the form: projects/my-project/locations/my-region/keyRings/my-kr/cryptoKeys/my-key.
    /// The key needs to be in the same region as where the compute resource is created.
    /// </summary>
    [JsonPropertyName("kmsKeyName")]
    public string? KmsKeyName { get; set; }
}
