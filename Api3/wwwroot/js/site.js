document.addEventListener("DOMContentLoaded", function () {
    const emailUsuario = document.getElementById("emailUsuario")?.value?.trim();
    if (!emailUsuario) return;

    document.querySelectorAll(".vender-btn").forEach((boton) => {
        if (boton.eventListener) {
            boton.removeEventListener("click", boton.eventListener);
        }

        boton.eventListener = function () {
            guardarVenta(boton, emailUsuario);
        };

        boton.addEventListener("click", boton.eventListener);
    });
});

function guardarVenta(boton, emailUsuario) {
    const card = boton.closest(".pokemon-card");
    if (!card) return;

    const { nombrePokemon, imagenUrl, rarezaPokemon, stats } = obtenerDatosPokemon(card);
    if (!nombrePokemon || !imagenUrl || !rarezaPokemon || stats.length === 0) return;

    fetch("/Venta/GuardarVenta", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ Nombre: nombrePokemon, ImagenUrl: imagenUrl, Rareza: rarezaPokemon, Stats: stats, Email: emailUsuario })
    })
        .then(response => response.ok ? response.json() : response.text().then(text => { throw new Error(text); }))
        .then(() => {
            mostrarAlerta(`✅ ${nombrePokemon} guardado para venta satisfactoriamente.`, "success");
            card.remove();
        })
        .catch(error => mostrarAlerta(`⚠ Error al guardar: ${error.message}`, "danger"));
}


function irAVenta() {
    const emailUsuario = document.getElementById("emailUsuario")?.value?.trim();

    if (!emailUsuario) {
        mostrarAlerta("⚠ No se pudo acceder a la colección porque el correo electrónico no está definido.", "danger");
        return;
    }

    window.location.href = "/Venta/Venta";
}

document.addEventListener("DOMContentLoaded", function () {
    if (window.location.pathname.includes("/Venta")) {
        const ventaContenedor = document.querySelector(".row-cols-1.row-cols-md-3.g-4");

        if (ventaContenedor) {
            setInterval(() => {
                fetch("/Venta/Venta")
                    .then(response => response.text())
                    .then(data => {
                        const nuevaVista = new DOMParser().parseFromString(data, "text/html")
                            .querySelector(".row-cols-1.row-cols-md-3.g-4");

                        if (nuevaVista) {
                            ventaContenedor.replaceChildren(...nuevaVista.children);
                        } else {
                            mostrarAlerta("⚠ La estructura de Venta no se encontró en la respuesta del servidor.", "warning");
                        }
                    })
                    .catch(error => mostrarAlerta("⚠ Error al actualizar la vista de Venta.", "danger"));
            }, 30000);
        } else {
            mostrarAlerta("⚠ La estructura `.row-cols-1.row-cols-md-3.g-4` no se encuentra en Venta.", "warning");
        }
    }
});

function actualizarVistaVenta() {
    fetch("/Venta/Venta")
        .then(response => response.text())
        .then(data => {
            const nuevaVista = new DOMParser().parseFromString(data, "text/html")
                .querySelector(".row-cols-1.row-cols-md-3.g-4");

            if (nuevaVista) {
                document.querySelector(".row-cols-1.row-cols-md-3.g-4").replaceChildren(...nuevaVista.children);
            }
        })
        .catch(error => console.error("❌ Error al actualizar la vista de Venta:", error));
}


setInterval(actualizarVistaVenta, 30000);
function finalizarSubasta(pokemonId) {
    fetch(`/Venta/FinalizarSubasta?pokemonId=${pokemonId}`, { method: "POST" })
        .then(response => response.json())
        .then(() => {
            mostrarAlerta("✅ Subasta finalizada correctamente.", "success");


            setTimeout(actualizarVistaVenta, 2000);
        })
        .catch(error => mostrarAlerta("⚠ Error al finalizar la subasta.", "danger"));
}



