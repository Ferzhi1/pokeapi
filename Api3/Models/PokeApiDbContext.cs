using Microsoft.EntityFrameworkCore;
using api3.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<ProductoPokemon> ProductoPokemon { get; set; }
    public DbSet<PedidoPokemon> PedidoPokemon { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuración opcional de relaciones o restricciones
    }
}

