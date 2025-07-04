namespace api3.Models
{
    public class SubastaInfo
    {
        public int PokemonId { get; set; }
        public int PokemonIdOriginal { get; set; }
        public string Nombre { get; set; }
        public string Rareza { get; set; }
        public decimal PrecioInicial { get; set; }
        public string ImagenUrl { get; set; }
        public int DuracionMinutos { get; set; }
        public string Email { get; set; }
        public decimal PujaActual { get; set; }
        public List<StatPokemon> Stats { get; set; }
        public double TiempoRestante { get; set; }
    }
}
