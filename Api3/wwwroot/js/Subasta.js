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

connection.on("ActualizarMonedero", function (nuevoSaldo) {
    document.getElementById("monedero").innerText = nuevoSaldo;
});

connection.on("NuevaSubasta", (pokemonId, nombrePokemon, rareza, precioInicial, imagenUrl, duracionMinutos, emailVendedor, pujaActual, stats) => {
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

    let tiempoRestante = duracionMinutos * 60;

    nuevaCarta.innerHTML = `
    <div class="card shadow-lg">
        <div class="d-flex align-items-center p-3">
            <div class="flex-shrink-0">
                <img src="${imagenUrl}" class="img-fluid rounded pokemon-img" alt="${nombrePokemon}" style="width: 180px; height: auto;" onerror="this.src='/images/default-pokemon.png';" />
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
            <p class="card-text">⏳ Tiempo Restante: <strong id="tiempo-restante-${pokemonId}">${tiempoRestante} segundos</strong></p>
            <input type="number" id="oferta-${pokemonId}" min="${pujaActual}" placeholder="Monedas a ofertar" required />
            <button class="btn btn-success mt-3" onclick="pujarPokemon(${pokemonId})">💰 Pujar</button>
            ${emailVendedor ? `<button onclick="enviarSolicitud('${emailVendedor}')" class="btn btn-secondary mt-2">➕ Agregar a Amigos (${emailVendedor})</button>` : '<p class="text-muted">⚠ Email no disponible para solicitud.</p>'}
        </div>
    </div>
    `;

    contenedor.appendChild(nuevaCarta);
});

function actualizarTiempoRestante() {
    document.querySelectorAll("[id^='tiempo-restante-']").forEach(tiempoElemento => {
        const expiracionStr = tiempoElemento.getAttribute("data-expiracion");
        if (!expiracionStr) return;

        let tiempoExpiracion = new Date(expiracionStr).getTime();
        let ahora = new Date().getTime();
        let tiempoRestante = Math.max(Math.floor((tiempoExpiracion - ahora) / 1000), 0);

        if (tiempoRestante > 0) {
            tiempoElemento.innerText = `${tiempoRestante} segundos`;
        } else {
            tiempoElemento.innerText = "⏳ Finalizando...";

            const pokemonCard = tiempoElemento.closest(".pokemon-card");
            if (!pokemonCard || pokemonCard.getAttribute("data-finalizado") === "true") return;

            const pokemonId = pokemonCard.querySelector("[name='pokemonId']")?.value;
            if (!pokemonId) return;

            fetch("/Subasta/FinalizarSubasta", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ pokemonId: parseInt(pokemonId) })
            })
                .then(response => response.json())
                .then(data => {
                    pokemonCard.setAttribute("data-finalizado", "true");
                    if (data.sinPujas) {
                        pokemonCard.style.display = "none";
                    }
                })
                .catch(err => { });
        }
    });
}

setInterval(actualizarTiempoRestante, 1000);

connection.on("FinalizarSubasta", (pokemonId, nombrePokemon, Id, ganador) => {
    const cardElement = document.getElementById(`card-${Id}`);
    if (cardElement) {
        cardElement.innerHTML += `<p class="text-success">✅ Subasta finalizada. Ganador: <strong>${ganador}</strong></p>`;
    }
});
