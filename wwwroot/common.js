const message = document.getElementById("message");

let messageTimer;

async function request(url, options = {}) {
    const response = await fetch(url, {
        headers: { "Content-Type": "application/json" },
        ...options,
    });

    if (!response.ok) {
        const problem = await response.json().catch(() => null);
        const details = problem?.errors ? Object.values(problem.errors).flat().join(" ") : null;
        throw new Error(details || problem?.detail || problem?.title || `Error ${response.status}`);
    }

    if (response.status === 204) {
        return null;
    }

    return response.json();
}

function showMessage(text, type = "error") {
    message.textContent = text;
    message.className = `message message-${type}`;
    message.hidden = false;

    clearTimeout(messageTimer);
    messageTimer = setTimeout(() => (message.hidden = true), 3000);
}

function createButton(text, className, onClick) {
    const button = document.createElement("button");
    button.type = "button";
    button.className = className;
    button.textContent = text;
    button.addEventListener("click", onClick);
    return button;
}

function createElement(tag, className, text) {
    const element = document.createElement(tag);

    if (className) {
        element.className = className;
    }

    if (text !== undefined) {
        element.textContent = text;
    }

    return element;
}
