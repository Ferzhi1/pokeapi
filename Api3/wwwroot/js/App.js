document.addEventListener("DOMContentLoaded", function () {
    const emailUsuario = document.getElementById("emailUsuario")?.value?.trim();
    if (!emailUsuario) return console.error("❌ ERROR: Email del usuario no definido.");

    document.querySelectorAll(".guardar-btn").forEach((boton) => {
        boton.removeEventListener("click", boton.dataset.eventListener);
        const eventListener = function () {
            guardarPokemon(boton, emailUsuario);
        };
        boton.dataset.eventListener = eventListener;
        boton.addEventListener("click", eventListener);
    });
});

function guardarPokemon(boton, emailUsuario) {
    const card = boton.closest(".pokemon-card");
    if (!card) return mostrarAlerta("⚠ Error: No se pudo obtener la información del Pokémon.", "danger");

    const { nombrePokemon, imagenUrl, rarezaPokemon, stats } = obtenerDatosPokemon(card);
    if (!nombrePokemon || !imagenUrl || !rarezaPokemon || stats.length === 0)
        return mostrarAlerta("⚠ No se pudo guardar porque los datos están incompletos.", "danger");

    fetch("/Pokemon/GuardarFavorito", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ Nombre: nombrePokemon, ImagenUrl: imagenUrl, Rareza: rarezaPokemon, Stats: stats, Email: emailUsuario })
    })
        .then(response => response.ok ? response.json() : response.text().then(text => { throw new Error(text); }))
        .then(() => {
            mostrarAlerta(`✅ ${nombrePokemon} guardado satisfactoriamente`, "success");
            card.remove();
        })
        .catch(error => mostrarAlerta(`⚠ Error al guardar: ${error.message}`, "danger"));
}



function irAColeccion() {
    const emailUsuario = document.getElementById("emailUsuario")?.value?.trim();

    if (!emailUsuario) {
        console.error("❌ ERROR: Email del usuario no definido.");
        mostrarAlerta("⚠ No se pudo acceder a la colección porque el email no está definido.", "danger");
        return;
    }

    window.location.href = `https://localhost:7200/Pokemon/Coleccion?email=${encodeURIComponent(emailUsuario)}`;
}






function obtenerDatosPokemon(card) {
    return {
        nombrePokemon: card.querySelector(".pokemon-nombre")?.textContent?.trim() || "Desconocido",
        rarezaPokemon: card.querySelector(".pokemon-rareza")?.textContent?.replace("Rareza: ", "").trim() || "N/A",
        imagenUrl: card.querySelector(".pokemon-img")?.src || "/images/default-pokemon.png",
        stats: Array.from(card.querySelectorAll(".list-group-item"))
            .map(stat => {
                const statData = stat.textContent.split(": ");
                return { nombre: statData[0]?.trim(), valor: statData[1]?.trim() || "N/A" };
            })
            .filter(stat => stat.nombre)
    };
}

function mostrarAlerta(mensaje, tipo) {
    let alertDiv = document.createElement("div");
    alertDiv.className = `alert alert-${tipo} position-fixed top-0 start-50 translate-middle-x fade show`;
    alertDiv.style.zIndex = "9999";
    alertDiv.style.padding = "10px 20px";
    alertDiv.innerHTML = mensaje;

    document.body.appendChild(alertDiv);
    setTimeout(() => {
        alertDiv.style.transition = "opacity 0.5s";
        alertDiv.style.opacity = "0";
        setTimeout(() => alertDiv.remove(), 500);
    }, 3000);
}






function mostrarAlerta(mensaje, tipo) {
    let alertDiv = document.createElement("div");
    alertDiv.className = `alert alert-${tipo} position-fixed top-0 start-50 translate-middle-x fade show`;
    alertDiv.style.zIndex = "9999";
    alertDiv.style.padding = "10px 20px";
    alertDiv.innerHTML = mensaje;

    document.body.appendChild(alertDiv);
    setTimeout(() => {
        alertDiv.style.transition = "opacity 0.5s";
        alertDiv.style.opacity = "0";
        setTimeout(() => alertDiv.remove(), 500);
    }, 3000);
}
