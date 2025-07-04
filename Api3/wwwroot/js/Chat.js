document.addEventListener("DOMContentLoaded", function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .configureLogging(signalR.LogLevel.Information)
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveMessage", function (user, message) {
        const li = document.createElement("li");
        li.textContent = `${user} dice: ${message}`;
        document.getElementById("messagesList").appendChild(li);
    });

    connection.start()
        .then(function () {
            const sendButton = document.getElementById("sendButton");
            if (sendButton) sendButton.disabled = false;
        })
        .catch(function (err) {
            console.error("❌ Error conectando al chat:", err.toString());
        });


    const sendButton = document.getElementById("sendButton");
    if (sendButton) {
        sendButton.addEventListener("click", function (event) {
            event.preventDefault();

            const user = document.getElementById("userEmail").value.trim();

     
            const message = document.getElementById("messageInput").value.trim();

            if (user === "" || message === "") {
                console.warn("⚠️ Usuario o mensaje vacío.");
                return;
            }

            if (connection.state === signalR.HubConnectionState.Connected) {
                connection.invoke("SendMessage", user, message)
                    .catch(function (err) {
                        console.error("❌ Error enviando mensaje:", err.toString());
                    });
            } else {
                console.warn("⚠️ Conexión no activa.");
            }
        });
    }
});




