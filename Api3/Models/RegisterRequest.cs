﻿namespace api3.Models
{
    public class RegisterRequest
    {
        public string Email { get; set; }

        public string Nombre { get; set; }
        public string Password { get; set; }

        public string Pregunta { get; set; }

        public string Respuesta { get; set; }
    }
}
