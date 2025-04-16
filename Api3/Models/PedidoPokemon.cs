using api3.Models;

namespace api3.Models;
public class PedidoPokemon
{
    public string NombreMazo { get; set; }
    public decimal Precio { get; set; }
    public string UsuarioEmail { get; set; }
    public DateTime FechaPedido { get; set; }
    public List<ProductoPokemon> Pokemons { get; set; }

    public PedidoPokemon(string nombreMazo, decimal precio, string usuarioEmail)
    {
        NombreMazo = nombreMazo;
        Precio = precio;
        UsuarioEmail = usuarioEmail;
        FechaPedido = DateTime.Now;
        Pokemons = new List<ProductoPokemon>(); // ✅ Evita errores de `null`
    }
}
