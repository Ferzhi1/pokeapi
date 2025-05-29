namespace api3.Services
{
    public class SubastaService
    {
        private readonly ApplicationDbContext _context;

        public SubastaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegistrarOfertaAsync(int pokemonId, string usuario, decimal monto)
        {
            var pokemon = await _context.ProductoPokemon.FindAsync(pokemonId);
            if (pokemon == null || monto <= pokemon.PujaActual) return false;

            pokemon.PujaActual = monto;
            _context.ProductoPokemon.Update(pokemon);
            await _context.SaveChangesAsync();

            return true;
        }


    }
}
