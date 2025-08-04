using staj_proje.Models;

namespace staj_proje.Models
{
    public interface IWeatherService
    {
        Task<WeatherApiResponse> GetWeatherAsync(string city);
        Task<string> ExecutePythonScriptAsync(string scriptPath, string arguments);
    }
}