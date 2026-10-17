const API_URL = "/api/categories";

const list = document.getElementById("category-list");
const empty = document.getElementById("empty");
const createForm = document.getElementById("create-form");
const createName = document.getElementById("create-name");

function renderItem(category) {
    const item = createElement("li", "item");
    const name = createElement("span", "item-name", category.name);

    const actions = createElement("div", "item-actions");
    actions.append(
        createButton("Edit", "btn", () => startEdit(item, category)),
        createButton("Delete", "btn btn-danger", () => removeCategory(category)),
    );

    item.append(name, actions);
    return item;
}

function startEdit(item, category) {
    const form = createElement("form", "item item-editing");

    const input = document.createElement("input");
    input.type = "text";
    input.value = category.name;
    input.maxLength = 100;
    input.required = true;

    const save = createElement("button", "btn btn-primary", "Save");
    save.type = "submit";

    const cancel = () => form.replaceWith(item);

    const actions = createElement("div", "item-actions");
    actions.append(save, createButton("Cancel", "btn", cancel));

    form.append(input, actions);

    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        try {
            await request(`${API_URL}/${category.id}`, {
                method: "PUT",
                body: JSON.stringify({ name: input.value.trim() }),
            });
            showMessage("Category updated.", "success");
            await loadCategories();
        } catch (error) {
            showMessage(`Could not update the category. ${error.message}`);
        }
    });

    input.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            cancel();
        }
    });

    item.replaceWith(form);
    input.focus();
    input.select();
}

async function removeCategory(category) {
    const confirmed = confirm(`Delete the category "${category.name}"? Its tasks will also be deleted.`);

    if (!confirmed) {
        return;
    }

    try {
        await request(`${API_URL}/${category.id}`, { method: "DELETE" });
        showMessage("Category deleted.", "success");
        await loadCategories();
    } catch {
        showMessage("Could not delete the category.");
    }
}

async function loadCategories() {
    try {
        const categories = await request(API_URL);

        list.replaceChildren(...categories.map(renderItem));
        empty.hidden = categories.length > 0;
    } catch {
        showMessage("Could not load the categories.");
    }
}

createForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    try {
        await request(API_URL, {
            method: "POST",
            body: JSON.stringify({ name: createName.value.trim() }),
        });
        createForm.reset();
        showMessage("Category added.", "success");
        await loadCategories();
    } catch (error) {
        showMessage(`Could not add the category. ${error.message}`);
    }
});

loadCategories();
