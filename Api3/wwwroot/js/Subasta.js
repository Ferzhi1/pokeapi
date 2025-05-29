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

