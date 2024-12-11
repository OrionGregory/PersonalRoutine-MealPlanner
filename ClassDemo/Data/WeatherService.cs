using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class WeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public WeatherService(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
    }

    private async Task<(double lat, double lon, string city)> GetCoordinatesAsync(string zipCode)
    {
        var url = $"http://api.openweathermap.org/geo/1.0/zip?zip={zipCode},us&appid={_apiKey}";
        var response = await _httpClient.GetStringAsync(url);
        var json = JObject.Parse(response);
        var lat = json["lat"]?.Value<double>() ?? 0;
        var lon = json["lon"]?.Value<double>() ?? 0;
        var city = json["name"]?.Value<string>() ?? "Unknown";
        return (lat, lon, city);
    }

    public async Task<(string temperature, string city)> GetTemperatureAsync(string zipCode)
    {
        var (lat, lon, city) = await GetCoordinatesAsync(zipCode);
        var url = $"https://api.openweathermap.org/data/3.0/onecall?lat={lat}&lon={lon}&exclude=minutely,hourly,daily,alerts&appid={_apiKey}&units=imperial";
        var response = await _httpClient.GetStringAsync(url);
        var json = JObject.Parse(response);
        var temperature = json["current"]?["temp"]?.ToString();
        return (temperature, city);
    }
}