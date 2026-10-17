const TASK_URL = "/api/tasks";
const CATEGORY_URL = "/api/categories";

const list = document.getElementById("task-list");
const empty = document.getElementById("empty");
const noCategories = document.getElementById("no-categories");
const createForm = document.getElementById("create-form");
const createName = document.getElementById("create-name");
const createDescription = document.getElementById("create-description");
const createCategory = document.getElementById("create-category");
const filterCategory = document.getElementById("filter-category");
const segments = document.querySelectorAll(".segment");

let categories = [];
let tasks = [];
let statusFilter = "all";

function fillCategorySelect(select, selectedId) {
    const options = categories.map((category) => {
        const option = createElement("option", null, category.name);
        option.value = category.id;
        option.selected = category.id === selectedId;
        return option;
    });

    select.replaceChildren(...options);
}

function toPayload(task) {
    return JSON.stringify({
        name: task.name,
        description: task.description,
        done: task.done,
        categoryId: task.categoryId,
    });
}

function renderItem(task) {
    const item = createElement("li", task.done ? "item task done" : "item task");

    const checkbox = document.createElement("input");
    checkbox.type = "checkbox";
    checkbox.className = "task-check";
    checkbox.checked = task.done;
    checkbox.setAttribute("aria-label", `Mark "${task.name}" as done`);
    checkbox.addEventListener("change", () => toggleDone(task, checkbox));

    const content = createElement("div", "task-content");
    content.append(createElement("span", "item-name task-name", task.name));

    if (task.description) {
        content.append(createElement("span", "task-description", task.description));
    }

    content.append(createElement("span", "badge", task.categoryName));

    const actions = createElement("div", "item-actions");
    actions.append(
        createButton("Edit", "btn", () => startEdit(item, task)),
        createButton("Delete", "btn btn-danger", () => removeTask(task)),
    );

    item.append(checkbox, content, actions);
    return item;
}

async function toggleDone(task, checkbox) {
    try {
        await request(`${TASK_URL}/${task.id}`, {
            method: "PUT",
            body: toPayload({ ...task, done: checkbox.checked }),
        });
        await loadTasks();
    } catch {
        checkbox.checked = task.done;
        showMessage("Could not update the task.");
    }
}

function startEdit(item, task) {
    const form = createElement("form", "item item-editing task-edit");

    const name = document.createElement("input");
    name.type = "text";
    name.value = task.name;
    name.maxLength = 100;
    name.required = true;

    const description = document.createElement("input");
    description.type = "text";
    description.value = task.description;
    description.maxLength = 500;
    description.placeholder = "Description (optional)";

    const category = document.createElement("select");
    fillCategorySelect(category, task.categoryId);

    const save = createElement("button", "btn btn-primary", "Save");
    save.type = "submit";

    const cancel = () => form.replaceWith(item);

    const actions = createElement("div", "item-actions");
    actions.append(save, createButton("Cancel", "btn", cancel));

    const row = createElement("div", "task-form-row");
    row.append(category, actions);

    form.append(name, description, row);

    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        try {
            await request(`${TASK_URL}/${task.id}`, {
                method: "PUT",
                body: toPayload({
                    name: name.value.trim(),
                    description: description.value.trim(),
                    done: task.done,
                    categoryId: Number(category.value),
                }),
            });
            showMessage("Task updated.", "success");
            await loadTasks();
        } catch (error) {
            showMessage(`Could not update the task. ${error.message}`);
        }
    });

    form.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            cancel();
        }
    });

    item.replaceWith(form);
    name.focus();
}

async function removeTask(task) {
    if (!confirm(`Delete the task "${task.name}"?`)) {
        return;
    }

    try {
        await request(`${TASK_URL}/${task.id}`, { method: "DELETE" });
        showMessage("Task deleted.", "success");
        await loadTasks();
    } catch {
        showMessage("Could not delete the task.");
    }
}

function render() {
    const categoryId = Number(filterCategory.value);

    const visible = tasks
        .filter((task) => !categoryId || task.categoryId === categoryId)
        .filter((task) => statusFilter === "all" || task.done === (statusFilter === "done"));

    list.replaceChildren(...visible.map(renderItem));
    empty.hidden = visible.length > 0;
}

async function loadTasks() {
    try {
        tasks = await request(TASK_URL);
        render();
    } catch {
        showMessage("Could not load the tasks.");
    }
}

async function loadCategories() {
    try {
        categories = await request(CATEGORY_URL);
    } catch {
        showMessage("Could not load the categories.");
        return;
    }

    fillCategorySelect(createCategory);

    const all = createElement("option", null, "All categories");
    all.value = "";
    filterCategory.replaceChildren(all, ...categories.map((category) => {
        const option = createElement("option", null, category.name);
        option.value = category.id;
        return option;
    }));

    const hasCategories = categories.length > 0;
    noCategories.hidden = hasCategories;
    createForm.hidden = !hasCategories;
}

createForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    try {
        await request(TASK_URL, {
            method: "POST",
            body: toPayload({
                name: createName.value.trim(),
                description: createDescription.value.trim(),
                done: false,
                categoryId: Number(createCategory.value),
            }),
        });
        createName.value = "";
        createDescription.value = "";
        createName.focus();
        showMessage("Task added.", "success");
        await loadTasks();
    } catch (error) {
        showMessage(`Could not add the task. ${error.message}`);
    }
});

filterCategory.addEventListener("change", render);

segments.forEach((segment) => {
    segment.addEventListener("click", () => {
        segments.forEach((item) => item.classList.toggle("active", item === segment));
        statusFilter = segment.dataset.status;
        render();
    });
});

loadCategories().then(loadTasks);
