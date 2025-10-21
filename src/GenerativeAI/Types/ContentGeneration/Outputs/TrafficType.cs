using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Output only. Traffic type. This shows whether a request consumes Pay-As-You-Go or Provisioned Throughput quota.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<TrafficType>))]
public enum TrafficType
{
    /// <summary>
    /// Unspecified request traffic type.
    /// </summary>
    TRAFFIC_TYPE_UNSPECIFIED = 0,

    /// <summary>
    /// Type for Pay-As-You-Go traffic.
    /// </summary>
    ON_DEMAND = 1,

    /// <summary>
    /// Type for Provisioned Throughput traffic.
    /// </summary>
    PROVISIONED_THROUGHPUT = 2
}
