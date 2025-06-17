using Microsoft.Extensions.Caching.Memory;
using api3.Models;

namespace api3.Services
{
    public class PedidoService
    {
        private readonly IMemoryCache _cache;

        public PedidoService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public PedidoPokemon ObtenerPedidoDesdeCache(string nombreMazo, string email, int cantidadPokemons)
        {
            if (string.IsNullOrWhiteSpace(nombreMazo) || string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El nombre del mazo y el email son obligatorios.");

            string cacheKeyMazo = $"PokemonMazo_{cantidadPokemons}";
            string cacheKeyPedido = $"Pedido_{email}_{nombreMazo}";

            if (!_cache.TryGetValue(cacheKeyMazo, out List<ProductoPokemon> pokemons))
            {
              
                throw new InvalidOperationException("El mazo no existe en caché.");
            }

            if (_cache.TryGetValue(cacheKeyPedido, out PedidoPokemon cachedPedido))
            {
               
                return cachedPedido;
            }

            
            var pedido = new PedidoPokemon(nombreMazo, 25.99m, email)
            {
                Pokemons = pokemons
            };

            _cache.Set(cacheKeyPedido, pedido, TimeSpan.FromMinutes(30));

 

            return pedido;
        }
    }
}
