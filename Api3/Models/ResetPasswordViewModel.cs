using System.ComponentModel.DataAnnotations;

namespace api3.Models
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "La nueva contraseña es requerida.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nueva Contraseña")]
        [Compare("NewPassword", ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string? ConfirmNewPassword { get; set; }

        public string? ResetToken { get; set; }
    }
}