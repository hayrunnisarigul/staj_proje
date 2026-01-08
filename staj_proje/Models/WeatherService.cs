using System.Diagnostics;
using System.Text.Json;

namespace staj_proje.Models
{
    public class WeatherService : IWeatherService
    {
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(ILogger<WeatherService> logger)
        {
            _logger = logger;
        }

        public async Task<WeatherApiResponse> GetWeatherAsync(string city)
        {
            try
            {
                // Python script'ini çalıştır
                var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "weather_service.py");
                var arguments = $"\"{city}\"";
                
                var result = await ExecutePythonScriptAsync(scriptPath, arguments);
                
                if (string.IsNullOrEmpty(result))
                {
                    return new WeatherApiResponse
                    {
                        Success = false,
                        ErrorMessage = "Python script'inden veri alınamadı."
                    };
                }

                // JSON parse et
                var weatherData = JsonSerializer.Deserialize<WeatherApiResponse>(result);
                if (weatherData != null)
                {
                    weatherData.Success = true;
                    weatherData.Timestamp = DateTime.Now;
                    return weatherData;
                }
                
                return new WeatherApiResponse
                {
                    Success = false,
                    ErrorMessage = "JSON parse hatası"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hava durumu verisi alınırken hata oluştu");
                return new WeatherApiResponse
                {
                    Success = false,
                    ErrorMessage = $"Hata: {ex.Message}"
                };
            }
        }

        public async Task<string> ExecutePythonScriptAsync(string scriptPath, string arguments)
        {
            try
            {
                if (!File.Exists(scriptPath))
                {
                    _logger.LogError($"Python script dosyası bulunamadı: {scriptPath}");
                    return string.Empty;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = "python3", // Linux için python3, Windows için python
                    Arguments = $"{scriptPath} {arguments}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    _logger.LogError($"Python script hatası: {error}");
                    return string.Empty;
                }

                return output?.Trim() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Python script çalıştırılırken hata oluştu");
                return string.Empty;
            }
        }
    }
}