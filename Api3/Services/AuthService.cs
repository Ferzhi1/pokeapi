using Microsoft.EntityFrameworkCore;
using api3.Models;
using BCrypt.Net;

namespace api3.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UsuariosPokemonApi?> AuthenticateAsync(string email, string password)
        {
            var user = await _context.UsuariosPokemonApi.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || BCrypt.Net.BCrypt.Verify(password, user.Password))
                return null;

            return user;
        }

        public async Task RegisterAsync(string email, string nombre, string password)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var newUser = new UsuariosPokemonApi { Email = email, Nombre = nombre, Password = hashedPassword };

            _context.UsuariosPokemonApi.Add(newUser);
            await _context.SaveChangesAsync();
        }
    }
}
