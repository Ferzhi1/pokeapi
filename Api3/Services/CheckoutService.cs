using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api3.Models;

namespace api3.Services
{
    public class CheckoutService
    {
        private readonly ConcurrentDictionary<string, PedidoPokemon> _pedidos;

        public CheckoutService()
        {
            _pedidos = new ConcurrentDictionary<string, PedidoPokemon>();
        }

        public Task<string> ProcesarPagoAsync(PedidoPokemon pedido)
        {
            if (pedido == null || string.IsNullOrWhiteSpace(pedido.UsuarioEmail))
                return Task.FromResult("❌ Error: Pedido inválido o sin email.");

            if (pedido.Pokemons == null || !pedido.Pokemons.Any())
                return Task.FromResult("❌ Error: El pedido no tiene Pokémon.");

            _pedidos[pedido.UsuarioEmail] = pedido; // ✅ Guardar en memoria por email del usuario
            return Task.FromResult($"✅ Pago exitoso para {pedido.NombreMazo}. ¡Gracias por tu compra!");
        }

        public PedidoPokemon ObtenerPedido(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            _pedidos.TryGetValue(email, out var pedido);
            return pedido;
        }
    }
}
