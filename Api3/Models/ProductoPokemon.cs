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
        public bool EnVenta { get; set; } = false;
        public decimal PrecioInicial { get; set; }
        public DateTime TiempoExpiracion { get; set; } 
        public List<Puja2> HistorialPujas { get; set; } = new List<Puja2>(); 



        public List<StatPokemon> Stats { get; set; } = new List<StatPokemon>();
    }

    public class StatPokemon
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Valor { get; set; }
    }

}


