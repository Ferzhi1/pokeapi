

namespace api3.Models;
public class PedidoPokemon
{
    public int Id { get; set; }
    public string NombreMazo { get; set; }
    public decimal Precio { get; set; }
    public string UsuarioEmail { get; set; }
    public DateTime FechaPedido { get; set; }
    public List<ProductoPokemon> Pokemons { get; set; }
    public int PedidosUsuariosPokeId { get; set; }
    public PedidoUsuario PedidoUsuario { get; set; }
    public PedidoPokemon(string nombreMazo, decimal precio, string usuarioEmail)
    {
        NombreMazo = nombreMazo;
        Precio = precio;
        UsuarioEmail = usuarioEmail;
        FechaPedido = DateTime.UtcNow;
        Pokemons = new List<ProductoPokemon>();
    }
}
