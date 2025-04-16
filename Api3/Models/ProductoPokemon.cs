
namespace api3.Models
{
    public class ProductoPokemon
    {
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public string Descripcion { get; set; }
        public string ImagenUrl { get; set; }
        public string Rareza { get; set; }

        public List<StatPokemon> Stats { get; set; } = new List<StatPokemon>();
    }

    public class StatPokemon
    {
        public string Nombre { get; set; } // Ejemplo: "attack", "defense", "speed"
        public int Valor { get; set; } // Ejemplo: 50, 80, 100
    }
}

