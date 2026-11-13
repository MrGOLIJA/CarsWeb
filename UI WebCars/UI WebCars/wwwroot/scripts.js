const URL = "http://localhost:5204"

export async function loadCars() {
    const response = await fetch(`${URL}/api/Cars`);
    if (!response.ok) {
        throw new Error("Запрос завершился с кодом " + response.status);
    }
    return await response.json();
}

export async function loadCarsByID(ID) {
    const response = await fetch(`${URL}/api/Cars/${encodeURIComponent(ID)}`);
    if (!response.ok) {
        throw new Error("Запрос завершился с кодом " + response.status);
    }
    return await response.json();
}

export async function putChangesByCar(car) {
    const response = await fetch(`${URL}/api/cars/${encodeURIComponent(car.id)}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(car),
    });
    if (!response.ok) {
        throw new Error("Запрос завершился с кодом " + response.status);
    }
}

export async function deleteCarByID(ID) {
    const response = await fetch(`${URL}/api/cars/${encodeURIComponent(ID)}`, {
        method: "DELETE",
    });
    if (!response.ok) {
        throw new Error("Запрос завершился с кодом " + response.status);
    }
}