const connection = new signalR.HubConnectionBuilder()
    .withUrl("/subastaHub")
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

connection.start().catch(err => console.error("❌ Error de conexión:", err));

// Evento para actualizar una oferta en tiempo real
connection.on("ActualizarOferta", (pokemonId, usuario, monto) => {
    console.log(`🔔 ${usuario} ha ofertado $${monto}`);

    const pujaElemento = document.getElementById(`puja-${pokemonId}`);
    if (pujaElemento) {
        pujaElemento.innerText = `${monto} monedas`;
    }
});

// Evento para actualizar la página sin recargarla completamente
connection.on("ActualizarPagina", () => {
    document.querySelectorAll(".tiempo-restante").forEach(actualizarTiempoSubasta);
});

// Evento para manejar la finalización de una subasta
connection.on("SubastaFinalizada", (pokemonId, ganador) => {
    alert(`🏆 La subasta del Pokémon ${pokemonId} ha terminado. Ganador: ${ganador}`);
    actualizarTiempoSubasta(); // Evita recargar completamente la página
});

// Función para pujar por un Pokémon
async function pujarPokemon(pokemonId) {
    const usuario = document.getElementById("emailUsuario")?.value.trim();
    const montoInput = document.getElementById(`oferta-${pokemonId}`);
    const monto = montoInput ? montoInput.value.trim() : null;

    if (!usuario || !monto || isNaN(monto) || parseFloat(monto) <= 0) {
        alert("❌ Debes ingresar una oferta válida.");
        return;
    }

    try {
        const response = await fetch('/Subasta/PujarPokemon', {
            method: 'POST',
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ pokemonId, usuario, monto })
        });

        const data = await response.json();
        console.log("✅ Respuesta del servidor:", data.mensaje);

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.error || "❌ Error al ofertar.");
        }

        // Notificar a otros clientes sobre la nueva oferta
        connection.invoke("NotificarOferta", pokemonId, usuario, parseFloat(monto))
            .catch(err => console.error("❌ Error enviando oferta:", err));

        alert("✅ Oferta realizada.");
    } catch (err) {
        console.error("❌", err);
        alert(err.message);
    }
}

// Función para actualizar el tiempo restante de la subasta
function actualizarTiempoSubasta() {
    document.querySelectorAll(".tiempo-restante").forEach((elemento) => {
        let fechaExpiracion = new Date(elemento.getAttribute("data-expiracion"));
        let ahora = new Date();
        let diferenciaMinutos = Math.max(Math.floor((fechaExpiracion - ahora) / 60000), 0);

        elemento.innerText = diferenciaMinutos > 0 ? `${diferenciaMinutos} minutos` : "⏳ Finalizado";

        if (diferenciaMinutos === 0) {
            elemento.classList.add("text-danger");
        }
    });
}

// Ejecutar la actualización de tiempo periódicamente
setInterval(actualizarTiempoSubasta, 60000);
document.addEventListener("DOMContentLoaded", actualizarTiempoSubasta);
