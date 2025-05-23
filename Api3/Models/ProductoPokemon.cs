using System.ComponentModel.DataAnnotations;



namespace api3.Models
{
    public class ProductoPokemon
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public decimal PujaActual { get; set; } = 0;
        public string Descripcion { get; set; }
        public string ImagenUrl { get; set; }
        public string Rareza { get; set; }
        public string Email { get; set; }
        public bool EnVenta { get; set; } = true;
        public decimal PrecioInicial { get; set; }
        public DateTime TiempoExpiracion { get; set; } 
        public List<Puja> HistorialPujas { get; set; } = new List<Puja>(); 



        public List<StatPokemon> Stats { get; set; } = new List<StatPokemon>();
    }

    public class StatPokemon
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Valor { get; set; }
    }
    public class Puja
    {
        public int Id { get; set; }
        public int PokemonId { get; set; } // Pokémon en subasta
        public string UsuarioEmail { get; set; } // Usuario que realizó la puja
        public decimal CantidadMonedas { get; set; } // Monto apostado
        public DateTime FechaPuja { get; set; } // Registro de la fecha
    }
}


