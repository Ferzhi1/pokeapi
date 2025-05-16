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
            if (user == null)
                return null; 
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                return null; 

            return user; 
        }

        public async Task RegisterAsync(string email, string nombre, string password, string pregunta, string respuesta)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var hashedRespuesta = BCrypt.Net.BCrypt.HashPassword(respuesta); 
            var newUser = new UsuariosPokemonApi
            {
                Email = email,
                Nombre = nombre,
                Password = hashedPassword,
                PreguntaSeguridad = pregunta, 
                RespuestaSeguridad = hashedRespuesta 
            };

            _context.UsuariosPokemonApi.Add(newUser);
            await _context.SaveChangesAsync();
        }

    }
}
