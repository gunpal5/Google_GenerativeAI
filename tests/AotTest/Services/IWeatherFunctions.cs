using System.ComponentModel;
using CSharpToJsonSchema;

namespace AotTest;

[GenerateJsonSchema()]
public interface IWeatherFunctions
{
    [Description("Get the current weather in a given location")]
    public Weather GetCurrentWeather2(
        [Description("The city and state, e.g. San Francisco, CA")]
        string location,
        Unit unit = Unit.Celsius);

    [Description("Get the current weather in a given location")]
    public Task<Weather> GetCurrentWeatherAsync2(
        [Description("The city and state, e.g. San Francisco, CA")]
        string location,
        Unit unit = Unit.Celsius,
        CancellationToken cancellationToken = default);
}