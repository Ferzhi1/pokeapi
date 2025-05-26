const connection = new signalR.HubConnectionBuilder()
    .withUrl("/amistadHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Manejo de reconexión automática en caso de error
async function iniciarConexion() {
    try {
        await connection.start();
        console.log("✅ Conectado a SignalR");
    } catch (err) {
        console.error("❌ Error al conectar a SignalR:", err);
        setTimeout(iniciarConexion, 5000); // Reintentar conexión en 5 segundos
    }
}

iniciarConexion();

// Definición de eventos de SignalR
connection.on("RecibirSolicitud", (remitenteEmail) => {
    console.log(`🔔 Nueva solicitud de amistad de ${remitenteEmail}`);
    alert(`Nueva solicitud de amistad de ${remitenteEmail}`);
    actualizarListaSolicitudes();
});

connection.on("SolicitudAceptada", (receptorEmail) => {
    console.log(`✅ ${receptorEmail} aceptó tu solicitud`);
    alert(`${receptorEmail} aceptó tu solicitud`);
    actualizarListaSolicitudes();
});

connection.on("SolicitudRechazada", (receptorEmail) => {
    console.log(`❌ ${receptorEmail} rechazó tu solicitud`);
    alert(`${receptorEmail} rechazó tu solicitud`);
    actualizarListaSolicitudes();
});

// Función para validar correos electrónicos
function esEmailValido(email) {
    const emailRegex = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
    return emailRegex.test(email);
}

// Función para enviar solicitud de amistad
async function enviarSolicitud(receptorEmail) {
    const usuarioAutenticado = document.getElementById("usuarioActual")?.value?.trim(); // Obtén el usuario autenticado

    if (!usuarioAutenticado || !esEmailValido(usuarioAutenticado) || !esEmailValido(receptorEmail)) {
        alert("❌ Error: Email inválido.");
        return;
    }

    console.log(`📨 Enviando solicitud de ${usuarioAutenticado} a ${receptorEmail}`);

    try {
        const response = await fetch('/SolicitudAmistad/EnviarSolicitud', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ receptorEmail })
        });

        const result = await response.text();
        if (!response.ok) {
            throw new Error(`Error al enviar solicitud: ${result}`);
        }

        alert(`✅ Solicitud enviada a ${receptorEmail}`);
        connection.invoke("EnviarSolicitudAmistad", usuarioAutenticado, receptorEmail)
            .catch(err => console.error("❌ Error al enviar solicitud con SignalR:", err));
    } catch (err) {
        console.error("❌ Error al enviar solicitud:", err);
    }
}

// Función para actualizar la lista de solicitudes sin recargar la página
async function actualizarListaSolicitudes() {
    try {
        const response = await fetch('/SolicitudAmistad/ListaSolicitudes');
        const data = await response.json();

        const listaSolicitudes = document.getElementById("listaSolicitudes");
        listaSolicitudes.innerHTML = ""; // Limpiar lista actual

        data.forEach(solicitud => {
            const item = document.createElement("li");
            item.textContent = `Solicitud de ${solicitud.remitenteEmail}`;
            listaSolicitudes.appendChild(item);
        });

    } catch (err) {
        console.error("❌ Error al actualizar solicitudes:", err);
    }
}
