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




connection.on("SubastaIniciada", (pokemonId, nombrePokemon, precioInicial, duracionMinutos, imagenUrl, emailVendedor, pujaActual) => {
    const contenedor = document.querySelector(".row.row-cols-1.row-cols-md-3.g-4");
    if (!contenedor) return;

    const nuevaCarta = document.createElement("div");
    nuevaCarta.classList.add("col", "pokemon-card");
    nuevaCarta.id = `card-${pokemonId}`;
    nuevaCarta.innerHTML = `
        <div class="card shadow-lg">
            <div class="card-body text-center">
                <img src="${imagenUrl}" class="img-fluid pokemon-img" style="width: 180px;"
                     onerror="this.src='/images/default-pokemon.png';" />
                <h5 class="card-title">${nombrePokemon}</h5>
                <p class="card-text">💰 Precio Inicial: <strong>${precioInicial} monedas</strong></p>
                <p class="card-text">⏳ Tiempo Restante: <strong>${duracionMinutos} minutos</strong></p>
                <p class="card-text">📧 Vendedor: <strong>${emailVendedor}</strong></p>
                <p class="card-text">🏅 Puja Actual: <strong id="puja-${pokemonId}">${pujaActual} monedas</strong></p>

                <!-- Input para pujar -->
                <input type="number" id="oferta-${pokemonId}" min="${pujaActual}" placeholder="Monedas a ofertar" required />
                <button class="btn btn-success mt-3" onclick="pujarPokemon(${pokemonId})">💰 Pujar</button>
            </div>
        </div>
    `;

    contenedor.appendChild(nuevaCarta);
});



function pujarPokemon(pokemonId) {
    const emailUsuario = document.getElementById("emailUsuario").value;
    const oferta = document.getElementById(`oferta-${pokemonId}`).value;

    if (!emailUsuario || oferta === "" || isNaN(oferta) || Number(oferta) <= 0) {
        alert("❌ Debes ingresar una oferta válida.");
        return;
    }

    fetch("/Subasta/PujarPokemon", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ pokemonId, usuario: emailUsuario, monto: oferta })
    })
        .then(response => response.json())
        .then(data => {
            if (data.error) {
                alert(data.error);
            } else {
                alert("✅ Oferta realizada.");
            }
        })
        .catch(err => alert("❌ Error al enviar la oferta."));
}


