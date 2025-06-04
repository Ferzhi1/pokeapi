using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Puja2
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("ProductoPokemon")]
    public int PokemonId { get; set; }

    public string UsuarioEmail { get; set; }
    public decimal CantidadMonedas { get; set; }
    public DateTime FechaPuja { get; set; }
}
