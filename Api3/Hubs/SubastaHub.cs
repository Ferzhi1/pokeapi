﻿using api3.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace api3.Hubs
{
    public class SubastaHub : Hub
    {
        private static ConcurrentDictionary<string, string> UsuariosSubasta = new();

        public override async Task OnConnectedAsync()
        {
            var usuario = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(usuario))
            {
                UsuariosSubasta.AddOrUpdate(usuario, Context.ConnectionId, (key, oldValue) => Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var usuario = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(usuario))
            {
                UsuariosSubasta.TryRemove(usuario, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task NotificarActualizarOferta(int pokemonId, string usuario, decimal monto)
        {
            if (pokemonId > 0 && !string.IsNullOrEmpty(usuario) && monto > 0)
            {
                await Clients.All.SendAsync("ActualizarOferta", pokemonId, usuario, monto);
            }
        }

        public async Task NotificarNuevaSubasta(int pokemonId, string nombrePokemon, string rareza, decimal precioInicial, string imagenUrl, int duracionMinutos, string emailVendedor, decimal pujaActual, List<StatPokemon> stats)
        {
            if (pokemonId > 0 && !string.IsNullOrEmpty(nombrePokemon) && !string.IsNullOrEmpty(emailVendedor) && precioInicial >= 0)
            {
                await Clients.All.SendAsync("NuevaSubasta", pokemonId, nombrePokemon, rareza, precioInicial, imagenUrl, duracionMinutos, emailVendedor, pujaActual, stats);
            }
        }

        public async Task ActualizarTiempoSubasta(int pokemonId, string emailVendedor, int tiempoRestante)
        {
            if (pokemonId > 0 && !string.IsNullOrEmpty(emailVendedor) && tiempoRestante >= 0)
            {
                await Clients.All.SendAsync("ActualizarTiempoSubasta", pokemonId, emailVendedor, tiempoRestante);
            }
        }

    
        public async Task ActualizarMonedero(string usuarioEmail, decimal nuevoSaldo)
        {
            await Clients.User(usuarioEmail).SendAsync("ActualizarMonedero", nuevoSaldo);
        }
    



    }
}
