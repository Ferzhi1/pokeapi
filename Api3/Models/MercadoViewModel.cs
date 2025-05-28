using static api3.Services.ClimaService;

namespace api3.Models
{
    public class MercadoViewModel
    {
        public List<ProductoPokemon> Pokemons { get; set; }
        public ClimaResponse Clima { get; set; }

    
        public string UsuarioEmail { get; set; }
    }
}

