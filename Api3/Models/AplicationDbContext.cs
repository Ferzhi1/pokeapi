using Microsoft.EntityFrameworkCore;
using api3.Models;
using Api3.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<ProductoPokemon> ProductoPokemon { get; set; }
    public DbSet<PedidoPokemon> PedidoPokemon { get; set; }
    public DbSet<PedidoUsuario> PedidoUsuario { get; set; }
    public DbSet<MazoPokemon> MazoPokemon { get; set; }
    public DbSet<UsuariosPokemonApi> UsuariosPokemonApi { get; set; }
    public DbSet<ColeccionPokemon> ColeccionPokemon { get; set; }
   
  

  

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 🔹 Configuración de relaciones
        modelBuilder.Entity<PedidoPokemon>()
            .HasOne(p => p.PedidoUsuario)
            .WithMany(u => u.Pokemons)
            .HasForeignKey(p => p.PedidosUsuariosPokeId); // 🔥 Usa el nombre correcto de la clave foránea

        modelBuilder.Entity<PedidoUsuario>()
            .HasMany(p => p.Pokemons)
            .WithOne(p => p.PedidoUsuario)
            .HasForeignKey(p => p.PedidosUsuariosPokeId);
    }
}
