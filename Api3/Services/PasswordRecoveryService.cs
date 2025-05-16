using api3.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net; 
using System;
using System.Threading.Tasks;

namespace api3.Services
{
    public class PasswordRecoveryService
    {
        private readonly ApplicationDbContext _context;

        public PasswordRecoveryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UsuariosPokemonApi?> FindUserByEmailAsync(string email,string respuestaSeguridad)
        {
            var usuario=await _context.UsuariosPokemonApi.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario != null && BCrypt.Net.BCrypt.Verify(respuestaSeguridad, usuario.RespuestaSeguridad))
            {
                return usuario;
            }
            return null;
        }

        public async Task GenerateAndSetResetTokenAsync(UsuariosPokemonApi user)
        {
            user.ResetPasswordToken = Guid.NewGuid().ToString();
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1); 
            _context.UsuariosPokemonApi.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<UsuariosPokemonApi?> FindUserByResetTokenAsync(string token)
        {
            return await _context.UsuariosPokemonApi
                .FirstOrDefaultAsync(u => u.ResetPasswordToken == token && u.ResetPasswordTokenExpiry > DateTime.UtcNow);
        }


        public async Task ResetPasswordAsync(UsuariosPokemonApi user, string newPassword)
        {
            if (user == null)
            {
                Console.WriteLine("Error: Usuario es null.");
                return;
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                Console.WriteLine("Error: La nueva contraseña está vacía.");
                return;
            }

         

            _context.Attach(user);
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;

            _context.Entry(user).Property(p => p.Password).IsModified = true;
            _context.Entry(user).Property(p => p.ResetPasswordToken).IsModified = true;
            _context.Entry(user).Property(p => p.ResetPasswordTokenExpiry).IsModified = true;

       
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                Console.WriteLine($"Entidad: {entry.Entity.GetType().Name}, Estado: {entry.State}");
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("Contraseña actualizada correctamente.");

            
            await _context.Entry(user).ReloadAsync();
            Console.WriteLine($"Nueva contraseña en memoria después de la recarga: {user.Password}");
        }


    }
}