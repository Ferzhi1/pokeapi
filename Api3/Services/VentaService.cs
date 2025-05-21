using api3.Models;
using Microsoft.EntityFrameworkCore;

namespace api3.Services
{
    public class VentaService
    { 
        private readonly ApplicationDbContext _context;
        private readonly Dictionary<string, Decimal> _monederos = new Dictionary<string, decimal>();
        public VentaService(ApplicationDbContext context)
        {
            _context = context;
        }
        public void AgregarPokemonAVenta(string email, ProductoPokemon pokemon)
        {
            pokemon.Descripcion ??= "Sin descripción";
            _context.ProductoPokemon.Add(pokemon);
            _context.SaveChanges();
        }
        public List<ProductoPokemon> ObtenerVentaPokemon(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email no puede ser nulo ni vacío.");

            return _context.ProductoPokemon
                .Include(p => p.Stats)
                .Where(p => p.Email == email)
                .ToList();
        }
        public bool VenderCarta(string emailUsuario, ProductoPokemon pokemon)
        {
            var usuario = _context.UsuariosPokemonApi.FirstOrDefault(u => u.Email == emailUsuario);
            if (usuario == null) return false;

            usuario.Monedero += pokemon.Precio; // 💰 Aumentar saldo
            _context.SaveChanges(); // 🏦 Guardar cambios en la BD

            return true;
        }





        public decimal ObtenerSaldo(string emailUsuario)
        {
            return _monederos.ContainsKey(emailUsuario) ? _monederos[emailUsuario] : 0;
        
        
        }
        
    }
}
