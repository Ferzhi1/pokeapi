using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using api3.Models;

public class PokemonVentaService
{
    private readonly string _archivoJson = "pokemons_venta.json";
    private List<ProductoPokemon> _pokemonsEnVenta;

    public PokemonVentaService()
    {
        Console.WriteLine("🔄 Iniciando carga de datos desde archivo...");
        _pokemonsEnVenta = new List<ProductoPokemon>();
        CargarDatosDesdeArchivo();
        Console.WriteLine("✅ Carga de datos completada.");
    }

    // ✅ Agregar un Pokémon a la lista de venta
    public bool AgregarPokemonAVenta(string email, ProductoPokemon pokemon)
    {
        Console.WriteLine($"🛠 Recibido para venta: {pokemon.Nombre} - Rareza antes de asignar: {pokemon.Rareza}");

        pokemon.Rareza = string.IsNullOrWhiteSpace(pokemon.Rareza) ? "Desconocida" : pokemon.Rareza;

        if (string.IsNullOrWhiteSpace(email) || pokemon == null || string.IsNullOrWhiteSpace(pokemon.Nombre))
        {
            Console.WriteLine("⚠️ Error: Información inválida.");
            return false;
        }

        if (_pokemonsEnVenta.Any(p => p.Nombre == pokemon.Nombre))
        {
            Console.WriteLine($"⚠️ Pokémon {pokemon.Nombre} ya está en la lista de venta.");
            return false;
        }

        Console.WriteLine($"🛠 Antes de agregar a colección/venta: {pokemon.Nombre} - Rareza asignada: {pokemon.Rareza}");

        _pokemonsEnVenta.Add(new ProductoPokemon
        {
            Nombre = pokemon.Nombre,
            ImagenUrl = pokemon.ImagenUrl,
            Rareza = pokemon.Rareza
        });

        GuardarDatosEnArchivo();
        Console.WriteLine($"✅ Pokémon {pokemon.Nombre} agregado a la lista de venta con rareza: {pokemon.Rareza}");
        return true;
    }

    // ✅ Obtener lista de Pokémon en venta
    public List<ProductoPokemon> ObtenerPokemonsEnVenta()
    {
        Console.WriteLine("📌 Obteniendo lista de Pokémon en venta...");
        return _pokemonsEnVenta;
    }

    // ✅ Remover un Pokémon de la lista de venta
    public bool RemoverPokemonDeVenta(string nombre)
    {
        Console.WriteLine($"❌ Intentando eliminar: {nombre}");
        var pokemonAEliminar = _pokemonsEnVenta.FirstOrDefault(p => p.Nombre == nombre);
        if (pokemonAEliminar == null)
        {
            Console.WriteLine($"❌ Pokémon {nombre} no está en la lista de venta.");
            return false;
        }

        _pokemonsEnVenta.Remove(pokemonAEliminar);
        GuardarDatosEnArchivo();
        Console.WriteLine($"✅ Pokémon {nombre} eliminado de la lista de venta.");
        return true;
    }

    // ✅ Guardar datos en archivo JSON
    private void GuardarDatosEnArchivo()
    {
        Console.WriteLine("💾 Guardando datos en JSON...");
        var jsonData = JsonSerializer.Serialize(_pokemonsEnVenta, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_archivoJson, jsonData);
        Console.WriteLine("✅ Datos guardados correctamente.");
    }

    // ✅ Cargar datos desde archivo JSON
    private void CargarDatosDesdeArchivo()
    {
        Console.WriteLine("📂 Cargando datos desde archivo JSON...");
        if (File.Exists(_archivoJson))
        {
            string jsonData = File.ReadAllText(_archivoJson);
            _pokemonsEnVenta = JsonSerializer.Deserialize<List<ProductoPokemon>>(jsonData) ?? new List<ProductoPokemon>();

            Console.WriteLine("✅ Datos cargados con éxito. Pokémon en venta:");
            foreach (var pokemon in _pokemonsEnVenta)
            {
                Console.WriteLine($"🔍 Cargado: {pokemon.Nombre} - Rareza: {pokemon.Rareza}");
            }
        }
        else
        {
            Console.WriteLine("⚠️ No se encontró el archivo JSON, se crea uno nuevo.");
            _pokemonsEnVenta = new List<ProductoPokemon>();
        }
    }
}
