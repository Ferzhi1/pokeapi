using api3.Models;

namespace Api3.Models
{
    public class ColeccionPokemon
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string ImagenUrl { get; set; }
        public string Rareza { get; set; }
        public string EmailUsuario { get; set; } // Relación con el usuario
        public List<StatPokemon> Stats { get; set; } = new List<StatPokemon>();
    }

}
