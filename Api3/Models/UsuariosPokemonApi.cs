

namespace api3.Models
{
    public class UsuariosPokemonApi
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
  

        public string Password { get; set; }
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }
        public string PreguntaSeguridad { get; set; }

        public string RespuestaSeguridad { get; set; }
        public bool CorreoValidado { get; set; } = true;
        public decimal Monedero { get; set; } = 0;

    }
}
