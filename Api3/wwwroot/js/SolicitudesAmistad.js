document.addEventListener("DOMContentLoaded", () => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/amistadHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    async function iniciarConexion(reintentos = 0) {
        try {
            await connection.start();
            console.log("✅ Conectado a SignalR");
        } catch (err) {
            console.error(`❌ Error al conectar a SignalR (Intento ${reintentos}):`, err);
            if (reintentos < 5) {
                setTimeout(() => iniciarConexion(reintentos + 1), 5000);
            }
        }
    }

   
    connection.onclose(() => {
        console.warn("⚠ Conexión perdida. Intentando reconectar...");
        iniciarConexion();
    });

    iniciarConexion();


    connection.on("RecibirSolicitud", async (remitenteEmail) => {
        console.log(`🔔 Nueva solicitud de amistad recibida de ${remitenteEmail}`);

       
        actualizarListaSolicitudes();

        if (confirm(`🔔 Tienes una solicitud de amistad de ${remitenteEmail}. ¿Quieres aceptarla ahora?`)) {
            try {
                const response = await fetch(`/SolicitudAmistad/ObtenerSolicitudId?remitenteEmail=${remitenteEmail}`);
                if (!response.ok) throw new Error("❌ No se encontró la solicitud.");

                const data = await response.json();
                const solicitudId = data.solicitudId;

                console.log(`📡 ID de solicitud obtenido: ${solicitudId}`);
                aceptarSolicitud(solicitudId);

                // 🔹 Volver a actualizar la lista después de aceptar
                actualizarListaSolicitudes();
            } catch (err) {
                console.error("❌ Error al obtener solicitudId:", err);
            }
        }
    });


    connection.on("SolicitudAceptada", (receptorEmail) => {
        console.log(`✅ Solicitud aceptada por: ${receptorEmail}`);
        alert(`✅ Tu solicitud ha sido aceptada por ${receptorEmail}`);
        actualizarListaSolicitudes();
    });

    connection.on("SolicitudRechazada", (receptorEmail) => {
        console.log(`❌ ${receptorEmail} rechazó tu solicitud`);
        alert(`${receptorEmail} rechazó tu solicitud`);
        actualizarListaSolicitudes();
    });


    async function enviarSolicitud(receptorEmail) {
        const usuarioAutenticado = document.getElementById("emailUsuario")?.value.trim();
        if (!usuarioAutenticado) {
            alert("❌ Error: No se pudo obtener tu email.");
            return;
        }

        const formData = new FormData();
        formData.append("remitenteEmail", usuarioAutenticado);
        formData.append("receptorEmail", receptorEmail);

        try {
            console.log("📡 Enviando solicitud...");
            const response = await fetch('/SolicitudAmistad/EnviarSolicitud', {
                method: 'POST',
                body: formData
            });

            const data = await response.json();
            if (!response.ok) throw new Error(data.error || "❌ No se pudo enviar la solicitud.");

            alert(data.mensaje);

            // 🔹 Verificar si la conexión de SignalR se perdió y reconectar automáticamente
            setTimeout(() => {
                if (connection.state !== signalR.HubConnectionState.Connected) {
                    console.warn("⚠ Conexión perdida después de enviar solicitud. Intentando reconectar...");
                    iniciarConexion();
                }
            }, 1000);

        } catch (err) {
            console.error("❌ Error al enviar solicitud:", err);
            alert(err.message);
        }
    }




    async function actualizarListaSolicitudes() {
        const usuarioAutenticado = document.getElementById("emailUsuario")?.value.trim();
        if (!usuarioAutenticado) return;

        try {
            const response = await fetch(`/SolicitudAmistad/ListaSolicitudes?usuarioEmail=${usuarioAutenticado}`, {
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });

            if (!response.ok) return;

            const data = await response.json();
            const listaSolicitudes = document.getElementById("listaSolicitudes");
            if (!listaSolicitudes) return;

            listaSolicitudes.innerHTML = data.length
                ? data.map(solicitud =>
                    `<li id="solicitud-${solicitud.id}" class="list-group-item">Solicitud de <strong>${solicitud.remitenteEmail}</strong> 
                <button onclick="aceptarSolicitud(${solicitud.id})" class="btn btn-success btn-sm mx-1">✅ Aceptar</button>  
                <button onclick="rechazarSolicitud(${solicitud.id})" class="btn btn-danger btn-sm">❌ Rechazar</button>
            </li>`
                ).join("")
                : "<li class='list-group-item'>No tienes solicitudes pendientes.</li>";

 
            setTimeout(() => window.location.reload(), 1000);

        } catch (err) {
            console.error("❌ Error al actualizar lista de solicitudes:", err);
        }
    }


    async function aceptarSolicitud(solicitudId) {
        console.log(`📡 Capturando solicitud con ID: ${solicitudId}`);

        if (!solicitudId || solicitudId <= 0) {
            console.error("❌ ID de solicitud inválido.");
            return;
        }

        try {
            const response = await fetch('/SolicitudAmistad/AceptarSolicitud', {
                method: 'POST',
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ solicitudId })
            });

            if (!response.ok) throw new Error("❌ No se pudo aceptar la solicitud.");

            alert("✅ Solicitud aceptada.");

            // 🔹 Eliminar solicitud de la lista en tiempo real
            document.getElementById(`solicitud-${solicitudId}`)?.remove();

            // 🔹 Actualizar la lista de solicitudes para que refleje los cambios
            actualizarListaSolicitudes();

        } catch (err) {
            console.error("❌ Error al aceptar solicitud:", err);
        }
    }



    async function rechazarSolicitud(solicitudId) {
        try {
            const response = await fetch('/SolicitudAmistad/RechazarSolicitud', {
                method: 'POST',
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ solicitudId })
            });

            if (!response.ok) throw new Error("❌ No se pudo rechazar la solicitud.");

            alert("❌ Solicitud rechazada.");
            document.getElementById(`solicitud-${solicitudId}`)?.remove();
        } catch (err) {
            console.error("❌ Error al rechazar solicitud:", err);
        }
    }

    window.enviarSolicitud = enviarSolicitud;
    window.actualizarListaSolicitudes = actualizarListaSolicitudes;
    window.aceptarSolicitud = aceptarSolicitud;
    window.rechazarSolicitud = rechazarSolicitud;
});
