using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api3.Models;


namespace api3.Services
{
    public class CheckoutService
    {
        private readonly ConcurrentBag<PedidoPokemon> _pedidos;

        public CheckoutService()
        {
            _pedidos = new ConcurrentBag<PedidoPokemon>();
        }

        public Task<string> ProcesarPagoAsync(PedidoPokemon pedido)
        {
            _pedidos.Add(pedido);
            return Task.FromResult($"✅ Pago exitoso para {pedido.NombreMazo}. ¡Gracias por tu compra!");
        }

        public List<PedidoPokemon> ObtenerPedidos()
        {
            return _pedidos.ToList(); 
        }

      
    }
}