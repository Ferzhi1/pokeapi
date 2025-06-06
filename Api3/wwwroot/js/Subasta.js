const connection = new signalR.HubConnectionBuilder()
    .withUrl("/subastaHub")
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

connection.start();

connection.on("ActualizarOferta", (pokemonId, usuario, monto) => {
    const pujaElemento = document.getElementById(`puja-${pokemonId}`);
    if (pujaElemento) {
        pujaElemento.innerText = `${monto} monedas`;
    }
});

connection.on("SubastaFinalizada", (pokemonId, ganador) => {
    alert(`La subasta del Pokémon ${pokemonId} ha terminado. Ganador: ${ganador}`);
});
connection.on("NuevaSubasta", (pokemonId, nombrePokemon, precioInicial) => {
    const contenedor = document.querySelector(".row.row-cols-1.row-cols-md-3.g-4");
    if (!contenedor) return;

    const nuevaCarta = document.createElement("div");
    nuevaCarta.classList.add("col", "pokemon-card");
    nuevaCarta.id = `card-${nombrePokemon}`;
    nuevaCarta.innerHTML = `
        <div class="card shadow-lg">
            <div class="card-body text-center">
                <h5 class="card-title">${nombrePokemon}</h5>
                <p class="card-text">💰 Precio Inicial: <strong>${precioInicial} monedas</strong></p>
            </div>
        </div>
    `;

    contenedor.appendChild(nuevaCarta);
});

async function pujarPokemon(pokemonId) {
    const usuario = document.getElementById("emailUsuario")?.value.trim();
    const montoInput = document.getElementById(`oferta-${pokemonId}`);
    const monto = montoInput ? montoInput.value.trim() : null;
    const emailVendedor = document.querySelector(`#card-${pokemonId} .pokemon-vendedor`)?.innerText.trim();

    if (!usuario || !monto || isNaN(monto) || parseFloat(monto) <= 0) {
        alert("❌ Debes ingresar una oferta válida.");
        return;
    }

   
    if (usuario === emailVendedor) {
        alert("❌ No puedes pujar por tu propio Pokémon.");
        return;
    }

    try {
        const response = await fetch('/Subasta/PujarPokemon', {
            method: 'POST',
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ pokemonId, usuario, monto })
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.error || "Error al ofertar.");
        }

        alert("✅ Oferta realizada.");
    } catch (err) {
        alert(err.message);
    }
}

