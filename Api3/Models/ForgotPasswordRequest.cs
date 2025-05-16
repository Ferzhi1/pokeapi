using System.ComponentModel.DataAnnotations;

namespace api3.Models
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "Por favor, introduce un correo electrónico válido.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "La respuest de seguridad es requerida.")]
        public string Respuesta {  get; set; }
    }
}
