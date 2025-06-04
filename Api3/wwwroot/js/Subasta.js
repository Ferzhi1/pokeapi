const connection = new signalR.HubConnectionBuilder()
    .withUrl("/subastaHub")
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

connection.start().catch(err => console.error("❌ Error de conexión:", err));

connection.on("ActualizarOferta", (pokemonId, usuario, monto) => {
    console.log(`🔔 ${usuario} ha ofertado $${monto}`);

    const pujaElemento = document.getElementById(`puja-${pokemonId}`);
    if (pujaElemento) {
        pujaElemento.innerText = `${monto} monedas`;
    }
});
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/subastaHub")
    .build();

connection.on("ActualizarPagina", function () {
    location.reload(); 
});

connection.on("SubastaFinalizada", function (pokemonId, ganador) {
    alert(`La subasta del Pokémon ${pokemonId} ha terminado. Ganador: ${ganador}`);
    location.reload();
});
document.addEventListener("DOMContentLoaded", function () {
    setInterval(actualizarTiempo, 60000); 
    actualizarTiempo(); 
});


connection.start().catch(function (err) {
    console.error(err.toString());
});




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

        alert("✅ Oferta realizada.");
    } catch (err) {
        console.error("❌", err);
        alert(err.message);
    }
}
function actualizarTiempoSubasta() {
    document.querySelectorAll(".tiempo-restante").forEach(function (elemento) {
        let fechaExpiracion = new Date(elemento.getAttribute("data-expiracion"));
        let ahora = new Date();
        let diferenciaMinutos = Math.max(Math.floor((fechaExpiracion - ahora) / 60000), 0);

        // Actualiza el texto del tiempo restante en la tarjeta
        elemento.innerText = diferenciaMinutos > 0 ? `${diferenciaMinutos} minutos` : "⏳ Finalizado";

        // Cambia el estilo si ha expirado
        if (diferenciaMinutos === 0) {
            elemento.classList.add("text-danger");
        }
    });
}

// Ejecutar la función automáticamente cada minuto
setInterval(actualizarTiempoSubasta, 60000);

// Ejecutar al cargar la página para mostrar tiempos correctos desde el inicio
document.addEventListener("DOMContentLoaded", actualizarTiempoSubasta);



