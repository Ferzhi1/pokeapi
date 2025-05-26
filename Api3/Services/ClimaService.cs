using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using api3.Models;
namespace api3.Services
{
    public class ClimaService
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "c70139fcc15039e8202d1c1cb1857acb";
        private const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";

        public ClimaService(HttpClient httpClient)
        {
            _httpClient = httpClient;

        }
        public async Task<ClimaResponse> ObtenerClimaAsync(string ciudad)
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={ciudad}&appid={ApiKey}&units=metric&lang=es";

            using HttpClient client = new();
            try
            {
                var respuesta = await client.GetStringAsync(url);
                var clima = JsonSerializer.Deserialize<ClimaResponse>(respuesta, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true 
                });

                Console.WriteLine($"✅ JSON Clima: {respuesta}"); 

                return clima ?? new ClimaResponse(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al obtener el clima: {ex.Message}");
                return new ClimaResponse();
            }
        }

        public class ClimaResponse
        {
            [JsonPropertyName("main")]  
            public MainData Main { get; set; }

            [JsonPropertyName("weather")]
            public List<WeatherData> Weather { get; set; }
        }

        public class MainData
        {
            [JsonPropertyName("temp")]
            public float Temp { get; set; }

            [JsonPropertyName("feels_like")]
            public float Feels_Like { get; set; }

            [JsonPropertyName("humidity")]
            public int Humidity { get; set; }
        }

        public class WeatherData
        {
            [JsonPropertyName("description")]
            public string Description { get; set; }
        }

    }
}
