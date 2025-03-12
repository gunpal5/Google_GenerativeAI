using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace AotTest
{
    [Description("Weather Functions")]
    public class WeatherService : IWeatherFunctions
    {
        [Description("Get the current weather in a given location")]
        public Weather GetCurrentWeather2(string location, Unit unit = Unit.Celsius)
        {
            return new Weather
            {
                Location = location,
                Temperature = 30.0,
                Unit = unit,
                Description = "Sunny",
            };
        }

        public Task<Weather> GetCurrentWeatherAsync2(string location, Unit unit = Unit.Celsius,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Weather
            {
                Location = location,
                Temperature = 22.0,
                Unit = unit,
                Description = "Sunny",
            });
        }
    }
}