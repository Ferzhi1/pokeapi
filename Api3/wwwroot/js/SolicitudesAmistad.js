document.addEventListener("DOMContentLoaded", async () => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/amistadHub")
        .configureLogging(signalR.LogLevel.Information)
        .withAutomaticReconnect()
        .build();

    async function iniciarConexion() {
        try {
            await connection.start();
            console.log("Conectado a SignalR");
        } catch (err) {
            console.error("Error de conexión:", err);
            setTimeout(iniciarConexion, 5000);
        }
    }

    connection.on("RecibirSolicitud", async (remitenteEmail) => {
        try {
            actualizarListaSolicitudes(); 

            const response = await fetch(`/SolicitudAmistad/ObtenerSolicitudId?remitenteEmail=${encodeURIComponent(remitenteEmail)}`);
            if (!response.ok) throw new Error("Error obteniendo solicitud.");

            const data = await response.json();
            if (confirm(`Tienes una solicitud de amistad de ${remitenteEmail}. ¿Quieres aceptarla ahora?`)) {
                aceptarSolicitud(data.solicitudId);
                actualizarListaSolicitudes(); 
            }
        } catch (err) {
            console.error("Error procesando solicitud:", err);
        }
    });


    connection.on("SolicitudAceptada", (receptorEmail) => {
        alert(`Tu solicitud ha sido aceptada por ${receptorEmail}`);
        actualizarListaSolicitudes();
    });

    iniciarConexion();

    async function enviarSolicitud(receptorEmail) {
        const usuarioAutenticado = document.getElementById("emailUsuario")?.value.trim();
        if (!usuarioAutenticado) return;

        const formData = new FormData();
        formData.append("remitenteEmail", usuarioAutenticado);
        formData.append("receptorEmail", receptorEmail);

        try {
            const response = await fetch('/SolicitudAmistad/EnviarSolicitud', { method: 'POST', body: formData });
            const data = await response.json();
            if (!response.ok) throw new Error(data.error);

            alert(data.mensaje);
        } catch (err) {
            console.error("Error enviando solicitud:", err);
            alert(err.message);
        }
    }

    async function actualizarListaSolicitudes() {
        const usuarioAutenticado = document.getElementById("emailUsuario")?.value.trim();
        if (!usuarioAutenticado) return;

        try {
            const response = await fetch(`/SolicitudAmistad/ListaSolicitudes?usuarioEmail=${usuarioAutenticado}`, { headers: { "X-Requested-With": "XMLHttpRequest" } });
            if (!response.ok) throw new Error("Error obteniendo lista de solicitudes.");

            const data = await response.json();
            const listaSolicitudes = document.getElementById("listaSolicitudes");
            if (!listaSolicitudes) return;

            listaSolicitudes.innerHTML = data.length
                ? data.map(solicitud => `<li id="solicitud-${solicitud.id}" class="list-group-item">
                        Solicitud de <strong>${solicitud.remitenteEmail}</strong> 
                        <button onclick="aceptarSolicitud(${solicitud.id})" class="btn btn-success btn-sm mx-1">Aceptar</button>  
                        <button onclick="rechazarSolicitud(${solicitud.id})" class="btn btn-danger btn-sm">Rechazar</button>
                    </li>`).join("")
                : "<li class='list-group-item'>No tienes solicitudes pendientes.</li>";
        } catch (err) {
            console.error("Error actualizando lista:", err);
        }
    }

    async function aceptarSolicitud(solicitudId) {
        if (!solicitudId || solicitudId <= 0) return;

        try {
            const response = await fetch('/SolicitudAmistad/AceptarSolicitud', { method: 'POST', headers: { "Content-Type": "application/json" }, body: JSON.stringify({ solicitudId }) });
            if (!response.ok) throw new Error("Error aceptando solicitud.");

            alert("Solicitud aceptada.");
            document.getElementById(`solicitud-${solicitudId}`)?.remove();
            actualizarListaSolicitudes();
        } catch (err) {
            console.error("Error aceptando solicitud:", err);
        }
    }

    window.enviarSolicitud = enviarSolicitud;
    window.actualizarListaSolicitudes = actualizarListaSolicitudes;
    window.aceptarSolicitud = aceptarSolicitud;
});
