document.addEventListener("DOMContentLoaded", async () => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/amistadHub")
        .configureLogging(signalR.LogLevel.Information)
        .withAutomaticReconnect()
        .build();

    async function iniciarConexion() {
        try {
            await connection.start();

            connection.on("RecibirSolicitud", async (remitenteEmail) => {
                actualizarListaSolicitudes();

                if (confirm(`🔔 Tienes una solicitud de amistad de ${remitenteEmail}. ¿Quieres aceptarla ahora?`)) {
                    try {
                        const response = await fetch(`/SolicitudAmistad/ObtenerSolicitudId?remitenteEmail=${encodeURIComponent(remitenteEmail)}`);
                        if (!response.ok) throw new Error("❌ No se encontró la solicitud.");

                        const data = await response.json();
                        const solicitudId = data.solicitudId;

                        aceptarSolicitud(solicitudId);
                        actualizarListaSolicitudes();
                    } catch (err) { }
                }
            });

            connection.on("SolicitudAceptada", (receptorEmail) => {
                alert(`✅ Tu solicitud ha sido aceptada por ${receptorEmail}`);
                actualizarListaSolicitudes();
            });

            connection.on("SolicitudRechazada", (receptorEmail) => {
                alert(`${receptorEmail} rechazó tu solicitud`);
                actualizarListaSolicitudes();
            });

        } catch (err) {
            setTimeout(iniciarConexion, 5000);
        }
    }

    iniciarConexion();

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
            const response = await fetch('/SolicitudAmistad/EnviarSolicitud', {
                method: 'POST',
                body: formData
            });

            const data = await response.json();
            if (!response.ok) throw new Error(data.error || "❌ No se pudo enviar la solicitud.");

            alert(data.mensaje);

            setTimeout(() => {
                if (connection.state !== signalR.HubConnectionState.Connected) {
                    iniciarConexion();
                }
            }, 1000);
        } catch (err) {
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

        } catch (err) { }
    }

    async function aceptarSolicitud(solicitudId) {
        if (!solicitudId || solicitudId <= 0) {
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
            document.getElementById(`solicitud-${solicitudId}`)?.remove();
            actualizarListaSolicitudes();
        } catch (err) { }
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
        } catch (err) { }
    }

    window.enviarSolicitud = enviarSolicitud;
    window.actualizarListaSolicitudes = actualizarListaSolicitudes;
    window.aceptarSolicitud = aceptarSolicitud;
    window.rechazarSolicitud = rechazarSolicitud;
});
