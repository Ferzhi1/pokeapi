using api3.Models;

namespace api3.Models;
public class PedidoUsuario
{
    public int Id { get; set; }
   
    public string Email { get; set; }
    public string MazoSeleccionado { get; set; }
    public List<PedidoPokemon> Pokemons { get; set; }
}
