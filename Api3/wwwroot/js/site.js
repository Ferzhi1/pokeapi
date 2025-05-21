document.addEventListener("DOMContentLoaded", function () {
    fetch('/Auth/ObtenerEmail')
        .then(response => response.json())
        .then(data => {
            if (!data.email) {
                console.error("❌ ERROR: Email del usuario no definido.");
                return;
            }

            console.log('Correo del usuario:', data.email);

       
            const emailInput = document.getElementById("emailUsuario");
            if (emailInput) {
                emailInput.value = data.email;
            }

           
            document.querySelectorAll(".vender-btn").forEach((boton) => {
                boton.removeEventListener("click", boton.dataset.eventListener);
                const eventListener = function () {
                    VenderPokemon(boton, data.email);
                };
                boton.dataset.eventListener = eventListener;
                boton.addEventListener("click", eventListener);
            });
        })
        .catch(error => {
            console.error('Error al obtener el correo:', error);
        });
});


function VenderPokemon(boton, emailUsuario) {
    const card = boton.closest(".pokemon-card");
    if (!card) return mostrarAlerta("⚠ Error: No se pudo obtener la información del Pokémon.", "danger");

    const { nombrePokemon, imagenUrl, rarezaPokemon, stats } = obtenerDatosPokemon(card);
    if (!nombrePokemon || !imagenUrl || !rarezaPokemon || stats.length === 0) {
        return mostrarAlerta("⚠ No se pudo guardar porque los datos están incompletos.", "danger");
    }

    if (!emailUsuario) {
        return mostrarAlerta("⚠ Error: Email del usuario no definido.", "danger");
    }

    console.log("Enviando datos:", { Nombre: nombrePokemon, ImagenUrl: imagenUrl, Rareza: rarezaPokemon, Stats: stats, Email: emailUsuario });

    fetch("/Venta/GuardarVenta", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ Nombre: nombrePokemon, ImagenUrl: imagenUrl, Rareza: rarezaPokemon, Stats: stats, Email: emailUsuario })
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => { throw new Error(text); });
            }
            return response.json();
        })
        .then(() => {
            mostrarAlerta(`✅ ${nombrePokemon} guardado satisfactoriamente`, "success");
            card.remove();
        })
        .catch(error => {
            console.error("Error al guardar:", error);
            mostrarAlerta(`⚠ Error al guardar: ${error.message}`, "danger");
        });
}
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".vender-btn").forEach(boton => {
        boton.addEventListener("click", function () {
            venderCarta(boton);
        });
    });
});

function venderCarta(boton) {
    const card = boton.closest(".card");

    if (!card) {
        mostrarAlerta("⚠ Error: No se pudo obtener la información del Pokémon.", "danger");
        return;
    }

    // Extraer datos ANTES de eliminar la tarjeta
    const nombrePokemon = card.querySelector("h5")?.textContent.trim();
    const imagenUrl = card.querySelector("img")?.src;
    const rarezaPokemon = card.querySelector(".fw-bold")?.textContent.trim();
    const precioPokemon = card.querySelector("input")?.value;
    const emailUsuario = document.getElementById("emailUsuario").value;

    if (!nombrePokemon || !imagenUrl || !rarezaPokemon || !precioPokemon || isNaN(precioPokemon) || precioPokemon <= 0) {
        return mostrarAlerta("⚠ Error: Datos del Pokémon incompletos o precio inválido.", "danger");
    }

    // ENVIAR LA VENTA ANTES DE ELIMINAR LA TARJETA
    fetch("/Venta/VenderCarta", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ Nombre: nombrePokemon, Email: emailUsuario, Precio: parseInt(precioPokemon, 10) })
    })
        .then(response => response.json())
        .then(data => {
            mostrarAlerta("✅ Venta realizada con éxito.", "success");

            // 🔄 Verifica que el monedero haya cambiado antes de actualizarlo
            const monederoElement = document.querySelector("#monederoUsuario");
            if (monederoElement && data.nuevoSaldo !== undefined) {
                monederoElement.textContent = data.nuevoSaldo;
            } else {
                mostrarAlerta("⚠ Error: No se pudo actualizar el monedero.", "danger");
                return; // ⛔ Evita eliminar la tarjeta si el saldo no cambió
            }

            // 🔄 Ahora sí, eliminar la tarjeta
            // 🔄 Ahora sí, eliminar la tarjeta
            card.remove();

            // 📢 Actualizar el contador
            const totalPokemonElement = document.querySelector("h6"); // O busca un ID específico si lo tienes
            if (totalPokemonElement) {
                let totalActual = parseInt(totalPokemonElement.textContent.match(/\d+/)[0], 10);
                totalPokemonElement.textContent = `📢 Total de Pokémon para venta: ${totalActual - 1}`;
            }
        }
}
function irAVenta() {
    const emailUsuario = document.getElementById("emailUsuario")?.value?.trim();

    if (!emailUsuario) {
        console.error("❌ ERROR: Email del usuario no definido.");
        mostrarAlerta("⚠ No se pudo acceder a la colección porque el email no está definido.", "danger");
        return;
    }

    window.location.href = "/Venta/Venta";
}


