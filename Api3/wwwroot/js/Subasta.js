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


    document.querySelectorAll(".mazo-card.card-puja-activa").forEach(card => {
        card.classList.remove("card-puja-activa");
        card.classList.remove("card-apostada");
    });

    const cartaPujada = document.querySelector(`#card-${pokemonId}`);
    if (cartaPujada) {
        cartaPujada.classList.add("card-puja-activa");

    
        cartaPujada.classList.add("card-apostada");
        void cartaPujada.offsetWidth;
        setTimeout(() => {
            cartaPujada.classList.remove("card-apostada");
        }, 4000);
    }
});

connection.on("NuevaSubasta", (subasta) => {
    const {
        pokemonId,
        pokemonIdOriginal,
        nombre,
        rareza,
        precioInicial,
        imagenUrl,
        duracionMinutos,
        email,
        pujaActual,
        stats,
        tiempoRestante
    } = subasta;

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

    let tiempoRestanteSegundos = Math.floor(tiempoRestante * 60);

    nuevaCarta.innerHTML = `
    <div class="card shadow-lg">
        <div class="d-flex align-items-center p-3">
            <div class="flex-shrink-0">
                <img src="${imagenUrl}" class="img-fluid rounded pokemon-img"
                     alt="${nombre}" style="width: 180px; height: auto;"
                     onerror="this.src='/images/default-pokemon.png';" />
            </div>
            <div class="stats-section ms-3">
                <h6> 📊 Estadísticas:</h6>
                ${statsHTML}
            </div>
        </div>

        <div class="card-body text-center">
            <h5 class="card-title pokemon-nombre">${nombre}</h5>
            <p class="card-text"><strong>Rareza:</strong> ${rareza}</p>
             <p class="card-text">💰 Precio Inicial: <strong>${precioInicial} monedas</strong></p>
            <p class="card-text">🏅 Puja Actual: <strong id="puja-${pokemonId}">${pujaActual} monedas</strong></p>
             <p class="card-text text-muted small">🆔 ID Original: <strong>${pokemonIdOriginal}</strong></p>
            <p class="card-text">Email del vendedor:<strong>${email}</strong></p>
            <p class="card-text">
                ⏳ Tiempo Restante:
                <strong id="tiempo-restante-${pokemonId}">
                    ${tiempoRestanteSegundos} segundos
                </strong>
            </p>

            <input type="number" id="oferta-${pokemonId}" min="${pujaActual}" placeholder="Monedas a ofertar" required />
            <button class="btn btn-success mt-3" onclick="pujarPokemon(${pokemonId})">💰 Pujar</button>

            ${email ?
            `<button onclick="enviarSolicitud('${email}')" class="btn btn-secondary mt-2">
                ➕ Agregar a Amigos (${email})
            </button>` :
            '<p class="text-muted">⚠ Email no disponible para solicitud.</p>'}
        </div>
    </div>
    `;

    contenedor.appendChild(nuevaCarta);

    setTimeout(() => {
        const tiempoElemento = document.getElementById(`tiempo-restante-${pokemonId}`);
        if (!tiempoElemento) return;

        const intervalo = setInterval(() => {
            if (tiempoRestanteSegundos > 0) {
                tiempoRestanteSegundos--;
                tiempoElemento.innerText = `${tiempoRestanteSegundos} segundos`;
            } else {
                tiempoElemento.innerText = "⏳ Finalizando...";
                clearInterval(intervalo);

                setTimeout(() => {
                    const cartaElemento = document.getElementById(`card-${pokemonId}`);
                    if (cartaElemento) {
                        cartaElemento.remove();
                    }
                }, 2000);
            }
        }, 1000);
    }, 500);
});



function pujarPokemon(pokemonId) {
    const emailUsuario = document.getElementById("userEmail").value;

    const oferta = document.getElementById(`oferta-${pokemonId}`).value.trim();
   
    debugger;
    const monto = parseFloat(oferta);

    if (!emailUsuario || oferta === "" || isNaN(monto) || monto <= 0) {
        console.warn("❌ Oferta inválida");
        return;
    }
    debugger;


    fetch("/Subasta/PujarPokemon", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ pokemonId, usuario: emailUsuario, monto: oferta })
    })
        .then(response => response.json())
        .then(data => {
            if (data.error) {
                console.warn(data.error);
            } else {
           
                const cartaElemento = document.querySelector(`#card-${pokemonId}`);
                if (cartaElemento) {
                    cartaElemento.setAttribute("data-usuario", emailUsuario);
                    cartaElemento.setAttribute("data-monto", oferta);
                    cartaElemento.classList.add("card-puja-activa");

               
                    cartaElemento.classList.add("card-apostada");
                    void cartaElemento.offsetWidth;
                    setTimeout(() => {
                        cartaElemento.classList.remove("card-apostada");
                    }, 4000);
                }
            }
        })
        .catch(err => console.error("❌ Error al enviar la oferta."));
}





function actualizarTiempoRestante() {
    document.querySelectorAll("[id^='tiempo-restante-']").forEach(tiempoElemento => {
        const expiracionStr = tiempoElemento.getAttribute("data-expiracion");
        if (!expiracionStr) return;

        const tiempoExpiracion = new Date(expiracionStr).getTime();
        const ahora = Date.now();
        const tiempoRestante = Math.max(Math.floor((tiempoExpiracion - ahora) / 1000), 0);

        if (tiempoRestante > 0) {
            tiempoElemento.textContent = `${tiempoRestante} segundos`;
        } else {
            tiempoElemento.textContent = "⏳ Finalizando...";

            const cartaElemento = tiempoElemento.closest(".pokemon-card");
            if (!cartaElemento || cartaElemento.getAttribute("data-finalizado") === "true") return;

            const pokemonId = parseInt(cartaElemento.querySelector("[name='pokemonId']")?.value);
            const usuario = cartaElemento.getAttribute("data-usuario");
            const monto = parseFloat(cartaElemento.getAttribute("data-monto"));
            
            if (!pokemonId || !usuario || isNaN(monto)) return;

            fetch("/Subasta/FinalizarSubasta", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ pokemonId, usuario, monto })
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`Error HTTP: ${response.status}`);
                    }
                    return response.json();
                })
                .then(data => {
                    cartaElemento.setAttribute("data-finalizado", "true");

                    if (data.sinPujas) {
                        console.log("No hubo pujas.");
                    } else {
                        console.log("✅ Subasta finalizada con pujas.");
                    }

                    
                    cartaElemento.remove();
                })

        }
    });
}

setInterval(actualizarTiempoRestante, 1000);








connection.on("ActualizarTiempoSubasta", (pokemonId, emailVendedor, tiempoRestante) => {
    const tiempoElemento = document.getElementById(`tiempo-restante-${pokemonId}`);
    if (tiempoElemento)
    {
        tiempoElemento.setAttribute("data-expiracion", new Date().getTime() + tiempoRestante * 1000);

        if (tiempoRestante > 0) {
            tiempoElemento.innerText = `${tiempoRestante} segundos`;
        } else {
            tiempoElemento.innerText = "⏳ Finalizando...";

        }

    }
});



connection.on("FinalizarSubasta", (pokemonId) => {
    const cardElement = document.getElementById(`card-${pokemonId}`);
    if (cardElement) {
        cardElement.remove();
    }
});



connection.on("EliminarCarta", function (pokemonId) {
    const carta = document.getElementById(`card-${pokemonId}`); 
    if (carta) carta.remove();
    else console.warn(`⚠️ No se encontró card-${pokemonId}`);
});



connection.on("ActualizarMonedero", function (nuevoSaldo) {
    const monederoElement = document.getElementById("monedero");
    if (monederoElement) {
        const saldoFormateado = Number(nuevoSaldo).toFixed(2);
        monederoElement.textContent = `$${saldoFormateado}`;
        console.log("✅ Monedero actualizado: $" + saldoFormateado);
    } else {
        console.warn("⚠️ Elemento del monedero no encontrado.");
    }
});


































