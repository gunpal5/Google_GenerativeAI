using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CSharpToJsonSchema;
using GenerativeAI.Tools;

namespace GenerativeAI.IntegrationTests
{
    [GenerateJsonSchema(GoogleFunctionTool = true)]
    public interface IMultiService
    {
        [Description("Get the current weather for a location")] 
        Task<WeatherInfo> GetWeatherAsync(
            [Description("The city name")] string city,
            [Description("The country code")] string countryCode = "US",
            CancellationToken cancellationToken = default);

        [Description("Get information about a book")]
        Task<BookInfo> GetBookInfoAsync(
            [Description("The title of the book")] string title,
            CancellationToken cancellationToken = default);

        [Description("Get weather forecast for the next days")]
        Task<List<DailyForecast>> GetForecastAsync(
            [Description("The city name")] string city,
            [Description("Number of days to forecast")] int days = 3,
            CancellationToken cancellationToken = default);

        [Description("Search for books by genre")]
        Task<List<BookInfo>> SearchBooksAsync(
            [Description("The genre to search for")] string genre,
            CancellationToken cancellationToken = default);
    }

    public class MultiService : IMultiService
    {
        public Task<WeatherInfo> GetWeatherAsync(string city, string countryCode = "US", CancellationToken cancellationToken = default)
        {
            // Simulate an API call
            return Task.FromResult(new WeatherInfo
            {
                Temperature = 22.5,
                Description = $"Sunny with some clouds in {city}, {countryCode}",
                Humidity = 65,
                WindSpeed = 10.2
            });
        }

        public Task<BookInfo> GetBookInfoAsync(string title, CancellationToken cancellationToken = default)
        {
            // Simulate book lookup
            return Task.FromResult(new BookInfo
            {
                Title = title,
                Author = $"Author of {title}",
                Pages = 250,
                YearPublished = 2022
            });
        }

        public Task<List<DailyForecast>> GetForecastAsync(string city, int days = 3, CancellationToken cancellationToken = default)
        {
            var forecast = new List<DailyForecast>();
            
            for (int i = 0; i < days; i++)
            {
                forecast.Add(new DailyForecast
                {
                    Day = $"Day {i+1}",
                    Temperature = 20 + (i * 2), 
                    Description = i % 2 == 0 ? "Sunny" : "Cloudy"
                });
            }
            
            return Task.FromResult(forecast);
        }

        public Task<List<BookInfo>> SearchBooksAsync(string genre, CancellationToken cancellationToken = default)
        {
            var books = new List<BookInfo>();
            
            for (int i = 0; i < 3; i++)
            {
                books.Add(new BookInfo
                {
                    Title = $"{genre} Book {i+1}",
                    Author = $"Author {i+1}",
                    Pages = 200 + (i * 50),
                    YearPublished = 2020 + i
                });
            }
            
            return Task.FromResult(books);
        }
    }
    
    public class WeatherInfo
    {
        public double Temperature { get; set; }
        public string Description { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
    }
    
    public class BookInfo
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public int Pages { get; set; }
        public int YearPublished { get; set; }
    }
    
    public class DailyForecast
    {
        public string Day { get; set; }
        public double Temperature { get; set; }
        public string Description { get; set; }
    }
}