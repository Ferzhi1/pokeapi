﻿using System.Collections.Generic;

namespace api3.Models
{
    public class MazoPokemon
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public string ImagenUrl { get; set; }

        
        public List<ProductoPokemon> Pokemons { get; set; }

        public MazoPokemon(string nombre, decimal precio, string imagenUrl)
        {
           
            Nombre = nombre;
            Precio = precio;
            ImagenUrl = imagenUrl;
            Pokemons = new List<ProductoPokemon>(); 
        }
    }
}
