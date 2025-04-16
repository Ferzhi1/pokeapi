using api3.Models;

namespace api3.Models;
public class PedidoUsuario
{
    public string MazoSeleccionado { get; set; }
    public List<PedidoPokemon> Pokemons { get; set; }
}
