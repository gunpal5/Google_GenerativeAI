using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a whole or partial calendar date, such as a birthday.
/// </summary>
public class GoogleTypeDate
{
    /// <summary>
    /// Year of the date. Must be from 1 to 9999, or 0 to specify a date without a year.
    /// </summary>
    [JsonPropertyName("year")]
    public int? Year { get; set; }

    /// <summary>
    /// Month of the year. Must be from 1 to 12, or 0 to specify a year without a month and day.
    /// </summary>
    [JsonPropertyName("month")]
    public int? Month { get; set; }

    /// <summary>
    /// Day of the month. Must be from 1 to 31 and valid for the year and month,
    /// or 0 to specify a year by itself or a year and month where the day isn't significant.
    /// </summary>
    [JsonPropertyName("day")]
    public int? Day { get; set; }
}
