document.addEventListener("DOMContentLoaded", function () {
    const emailUsuario = document.getElementById("emailUsuario")?.value?.trim();
    if (!emailUsuario) return;

    document.querySelectorAll(".vender-btn").forEach((boton) => {
        if (boton.eventListener) {
            boton.removeEventListener("click", boton.eventListener);
        }

        boton.eventListener = function () {
            guardarVenta(boton, emailUsuario);
        };

        boton.addEventListener("click", boton.eventListener);
    });
});

function guardarVenta(boton, emailUsuario) {
    const card = boton.closest(".pokemon-card");
    if (!card) return;

    const { nombrePokemon, imagenUrl, rarezaPokemon, stats } = obtenerDatosPokemon(card);
    if (!nombrePokemon || !imagenUrl || !rarezaPokemon || stats.length === 0) return;

    const pokemonIdOriginal = parseInt(card.getAttribute("data-id")); // 👈 DEBE contener el ID original real

    if (!pokemonIdOriginal || isNaN(pokemonIdOriginal) || pokemonIdOriginal <= 0) {
        console.error("❌ ID original inválido:", pokemonIdOriginal);
        alert("⚠ El ID original del Pokémon no está definido correctamente.");
        return;
    }

    fetch("/Venta/GuardarVenta", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            PokemonIdOriginal: pokemonIdOriginal,
            Nombre: nombrePokemon,
            ImagenUrl: imagenUrl,
            Rareza: rarezaPokemon,
            Stats: stats,
            Email: emailUsuario
        })
    })
        .then(response => response.ok ? response.json() : response.text().then(text => { throw new Error(text); }))
        .then(() => {
            card.remove(); // o actualizarCard(...) si deseas dejarla visualmente bloqueada
        })
        .catch(error => {
            console.error("❌ Error al guardar venta:", error);
        });
}




function irAVenta() {
    const emailUsuario = document.getElementById("emailUsuario")?.value?.trim();
    if (!emailUsuario) return;

    window.location.href = "/Venta/Venta";
}


