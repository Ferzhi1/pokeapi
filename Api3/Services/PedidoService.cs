using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using api3.Models;

namespace api3.Services;

public class PedidoService
{
    private readonly HttpClient _httpClient;

    public PedidoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PedidoPokemon> ObtenerPedidoAsync(string nombreMazo, string email)
    {
        string apiUrl = $"https://tu-api-url.com/Pedido/ObtenerPedido?nombreMazo={nombreMazo}&email={email}"; // ✅ Replace with correct base URL

        return await _httpClient.GetFromJsonAsync<PedidoPokemon>(apiUrl);
    }


}
