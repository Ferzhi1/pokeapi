using api3.Models;



namespace api3.Models
{
    public class ProductoPokemon
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public string Descripcion { get; set; }
        public string ImagenUrl { get; set; }
        public string Rareza { get; set; }
        public string Email { get; set; }
        public bool EnVenta { get; set; }
    

        public List<StatPokemon> Stats { get; set; } = new List<StatPokemon>();
    }

    public class StatPokemon
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Valor { get; set; } 
    }
}

