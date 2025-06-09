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


/*connection.on("NuevaSubasta", (pokemonId, nombrePokemon,rareza, precioInicial, imagenUrl, duracionMinutos, emailVendedor, pujaActual, stats) => {
  

    const contenedor = document.querySelector(".row.row-cols-1.row-cols-md-3.g-4");
    if (!contenedor) return;

    const nuevaCarta = document.createElement("div");
    nuevaCarta.classList.add("col", "pokemon-card");
    nuevaCarta.id = `card-${pokemonId}`;

    let statsHTML = '<ul class="list-group">';
    if (Array.isArray(stats) && stats.length > 0) {
        stats.forEach(stat => {
            statsHTML += `<li class="list-group-item small">${stat.nombre}: <strong>${stat.valor}</strong></li>`;
        });
    } else {
        statsHTML += '<li class="list-group-item small text-muted">No hay estadísticas disponibles.</li>';
    }
    statsHTML += '</ul>';

  
    nuevaCarta.innerHTML = `
    <div class="card shadow-lg">
        <div class="d-flex align-items-center p-3">
            <div class="flex-shrink-0">
                <img src="${imagenUrl}" class="img-fluid rounded pokemon-img"
                     alt="${nombrePokemon}" style="width: 180px; height: auto;"
                     onerror="this.src='/images/default-pokemon.png';" />
            </div>
            <div class="stats-section ms-3">
                <h6> 📊 Estadísticas:</h6>
                ${statsHTML}
            </div>
        </div>

        <div class="card-body text-center">
            <h5 class="card-title pokemon-nombre">${nombrePokemon}</h5>
            <p class="card-text"><strong>Rareza:</strong> ${rareza}</p>
            <p class="card-text">💰 Precio Inicial: <strong>${precioInicial} monedas</strong></p>
            <p class="card-text">🏅 Puja Actual: <strong id="puja-${pokemonId}">${pujaActual} monedas</strong></p>
            <p class="card-text">Email del vendedor:<strong>${emailVendedor}</strong></p>
            <p class="card-text">
                ⏳ Tiempo Restante:
                <strong id="tiempo-restante"
                        data-expiracion="${duracionMinutos * 60}">
                    ${duracionMinutos} minutos
                </strong>
            </p>

            <input type="number" id="oferta-${pokemonId}" min="${pujaActual}" placeholder="Monedas a ofertar" required />
            <button class="btn btn-success mt-3" onclick="pujarPokemon(${pokemonId})">💰 Pujar</button>

            <!-- Agregar botón de solicitud de amistad al final -->
            ${emailVendedor ?
            `<button onclick="enviarSolicitud('${emailVendedor}')" class="btn btn-secondary mt-2">
                    ➕ Agregar a Amigos (${emailVendedor})
                </button>`
            :
            '<p class="text-muted">⚠ Email no disponible para solicitud.</p>'}
        </div>
    </div>
    `;

    contenedor.appendChild(nuevaCarta);
});*/






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


