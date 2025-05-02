using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api3.Models;

namespace api3.Services
{
    public class CheckoutService
    {
        private readonly ConcurrentDictionary<string, PedidoPokemon> _pedidos = new();

        public Task<string> ProcesarPagoAsync(PedidoPokemon pedido)
        {
            if (pedido == null || string.IsNullOrWhiteSpace(pedido.UsuarioEmail))
                return Task.FromResult("❌ Error: Pedido inválido o sin email.");

            if (pedido.Pokemons is not { Count: > 0 })
                return Task.FromResult("❌ Error: El pedido no tiene Pokémon.");

            _pedidos[pedido.UsuarioEmail] = pedido;
            return Task.FromResult($"✅ Pago exitoso para {pedido.NombreMazo}. ¡Gracias por tu compra!");
        }

        public PedidoPokemon ObtenerPedido(string email) =>
            string.IsNullOrWhiteSpace(email) ? null : _pedidos.TryGetValue(email, out var pedido) ? pedido : null;
    }
}
