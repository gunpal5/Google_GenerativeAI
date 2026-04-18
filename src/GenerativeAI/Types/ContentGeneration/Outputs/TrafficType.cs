using System.Text.Json.Serialization;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Types;

/// <summary>
/// Output only. Traffic type. This shows whether a request consumes Pay-As-You-Go or Provisioned Throughput quota.
/// </summary>
/// <remarks>
/// This enum uses a lenient JSON converter that gracefully handles unknown values
/// by falling back to <see cref="TRAFFIC_TYPE_UNSPECIFIED"/> instead of throwing an exception.
/// This prevents crashes when Google adds new TrafficType values to their API.
/// </remarks>
[JsonConverter(typeof(LenientTrafficTypeConverter))]
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
    PROVISIONED_THROUGHPUT = 2,

    /// <summary>
    /// Type for Pay-As-You-Go priority traffic.
    /// </summary>
    ON_DEMAND_PRIORITY = 3,

    /// <summary>
    /// Type for Pay-As-You-Go flex traffic.
    /// </summary>
    ON_DEMAND_FLEX = 4
}
