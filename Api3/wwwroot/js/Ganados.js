document.addEventListener("DOMContentLoaded", function () {
    const emailUsuario = document.getElementById("emailUsuario")?.value?.trim();
    console.log("📧 emailUsuario:", emailUsuario);

    if (!emailUsuario) {
        mostrarAlerta("⚠ No se detectó el correo del usuario.", "danger");
        return;
    }

    function mostrarAlerta(mensaje, tipo) {
        const alerta = document.createElement("div");
        alerta.className = `alert alert-${tipo} position-fixed top-0 start-50 translate-middle-x fade show`;
        alerta.style.zIndex = "9999";
        alerta.style.padding = "10px 20px";
        alerta.innerHTML = mensaje;

        document.body.appendChild(alerta);
        setTimeout(() => {
            alerta.style.transition = "opacity 0.5s";
            alerta.style.opacity = "0";
            setTimeout(() => alerta.remove(), 500);
        }, 3000);
    }

  
    document.querySelectorAll(".guardar-btn").forEach(boton => {
        boton.addEventListener("click", function () {
            const card = boton.closest(".pokemon-card");
            if (!card) return;

            const pokemonId = card.getAttribute("data-id");
            const nombre = card.querySelector(".pokemon-nombre")?.textContent?.trim();
            const rareza = card.querySelector(".pokemon-rareza")?.textContent?.replace("Rareza: ", "").trim();
            const imagenUrl = card.querySelector(".pokemon-img")?.src;

            const stats = Array.from(card.querySelectorAll(".list-group-item"))
                .map(stat => {
                    const valores = stat.textContent.split(/:\s+| |\t/);
                    return valores.length >= 2
                        ? { nombre: valores[0].trim(), valor: valores[1].trim() }
                        : null;
                })
                .filter(e => e !== null);

            const data = {
                PokemonIdOriginal: parseInt(pokemonId),
                Nombre: nombre,
                ImagenUrl: imagenUrl,
                Rareza: rareza,
                Stats: stats,
                Email: emailUsuario
            };

            console.log("📦 Datos enviados a /Pokemon/GuardarFavorito:", data);

            fetch("/Pokemon/GuardarFavorito", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(data)
            })
                .then(res => {
                    console.log("📨 Respuesta /GuardarFavorito:", res);
                    if (res.ok) return res.json();
                    return res.text().then(text => { throw new Error(text); });
                })
                .then(result => {
                    console.log("✅ Guardado exitoso:", result);
                    actualizarCard(card, "✅ Pokémon guardado ✔️", "success");
                })
                .catch(error => {
                    console.error("❌ Error al guardar:", error);
                    mostrarAlerta(`❌ Error al guardar ${nombre}: ${error.message}`, "danger");
                });
        });
    });


 
    function actualizarCard(card, mensaje, tipo) {
        card.querySelectorAll(".guardar-btn, .vender-btn").forEach(btn => btn.remove());

        const estado = document.createElement("div");
        estado.className = `mt-2 text-${tipo} fw-bold`;
        estado.innerHTML = mensaje;

        card.querySelector(".card-body").appendChild(estado);
        card.classList.add("disabled-card");


        setTimeout(() => card.remove(), 1200);
    }
});
