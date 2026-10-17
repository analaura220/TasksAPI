# TasksAPI

A REST API for managing tasks grouped by categories. It is built with ASP.NET Core 10 and Entity Framework Core on PostgreSQL, and it ships with a small web frontend.

## Table of contents

- [Tech stack](#tech-stack)
- [Project structure](#project-structure)
- [Getting started](#getting-started)
- [Entity Framework Core](#entity-framework-core)
- [Dependency injection](#dependency-injection)
- [REST API](#rest-api)
- [Frontend](#frontend)
- [What was addressed](#what-was-addressed)
- [Known limitations](#known-limitations)

## Tech stack

| Layer        | Technology                                                  |
| ------------ | ----------------------------------------------------------- |
| Runtime      | .NET 10 / ASP.NET Core (controllers)                        |
| Data access  | Entity Framework Core 10 + Npgsql provider                  |
| Database     | PostgreSQL 17 (Docker)                                      |
| Frontend     | Plain HTML, CSS and JavaScript served from `wwwroot`        |

## Project structure

```
TasksAPI/
├── Controllers/    HTTP layer: routes, status codes, request validation
├── Services/       Business layer: rules and mapping between entities and DTOs
├── Repositories/   Data layer: every Entity Framework query lives here
├── Dtos/           Request and response contracts exposed by the API
├── Models/         Entity classes mapped to database tables
├── Data/           TasksApiContext (the EF Core DbContext)
├── Migrations/     Generated EF Core migrations
├── wwwroot/        Frontend (tasks.html, index.html, JS and CSS)
├── Program.cs      Service registration and HTTP pipeline
└── TasksAPI.http   Ready-to-send requests for every endpoint
```

A request always flows through the same three layers:

```
HTTP request → Controller → Service → Repository → TasksApiContext → PostgreSQL
                  ↑ DTOs ↑      ↑ entities ↑
```

## Getting started

### 1. Start PostgreSQL

```bash
docker run -d --name tasks-db \
  -e POSTGRES_PASSWORD=<your-password> \
  -e POSTGRES_DB=tasks-db \
  -p 5432:5432 \
  postgres:17
```

The connection string lives in `appsettings.json` under `ConnectionStrings:ConnectionString`.

### 2. Install the EF Core CLI

```bash
dotnet tool install --global dotnet-ef
export PATH="$PATH:$HOME/.dotnet/tools"
```

### 3. Create the database schema

```bash
dotnet ef database update
```

### 4. Run the application

```bash
dotnet run
```

| URL                                | What it is              |
| ---------------------------------- | ----------------------- |
| https://localhost:5000/tasks.html  | Tasks page              |
| https://localhost:5000/            | Categories page         |
| https://localhost:5000/api/...     | REST API                |
| https://localhost:5000/openapi/v1.json | OpenAPI document (Development only) |

### 5. Inspect the database from the terminal

```bash
docker exec tasks-db psql -U postgres -d tasks-db -c '\dt'
docker exec tasks-db psql -U postgres -d tasks-db -c 'SELECT * FROM "Tasks";'
```

## Entity Framework Core

### DbContext

`Data/TasksApiContext.cs` is the unit of work. It exposes one `DbSet<T>` per table:

```csharp
public DbSet<Category> Categories { get; set; }
public DbSet<Task> Tasks { get; set; }
```

### Entities and relationships

- `Category` has many `Task`s (`List<Task> Tasks`).
- `Task` belongs to one `Category`. It has a foreign key (`int CategoryId`) and an optional navigation property (`Category? Category`).

EF Core infers the following from these conventions:

- **Primary keys.** A property named `Id` becomes the primary key, with an identity column.
- **A required relationship.** Because `CategoryId` is a non-nullable `int`, every task must belong to a category.
- **Cascade delete.** Deleting a category also deletes its tasks (`ReferentialAction.Cascade` in the migration).

The navigation property is nullable on purpose. The client only sends `categoryId`, and EF Core loads the `Category` object only when the code asks for it.

### Migrations

The schema is created from the C# model. No SQL is written by hand.

```bash
dotnet ef migrations add <Name>    # generate a migration from model changes
dotnet ef database update          # apply pending migrations
dotnet ef migrations list          # see applied and pending migrations
dotnet ef migrations has-pending-model-changes   # check if the model and migrations are in sync
```

Applied migrations are recorded in the `__EFMigrationsHistory` table.

### Query practices used in the repositories

| Practice | Where | Why |
| --- | --- | --- |
| `async` everywhere (`ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`) | All repositories | Requests don't hold a thread while they wait on the database |
| `AsNoTracking()` | List and get-by-id queries | Read-only results don't need change tracking, which makes them cheaper |
| `Include(task => task.Category)` | `TaskRepository` reads | Loads the category in the same SQL query (a JOIN), so the response can include `categoryName` without N+1 queries |
| `OrderBy` / `ThenBy` | List queries | Sorting happens in SQL instead of in memory |
| `AnyAsync` | `CategoryRepository.ExistsAsync` | Checks existence with `SELECT EXISTS` and doesn't load the row |
| `FindAsync` + property assignment + `SaveChangesAsync` | Update methods | The change tracker detects which columns changed and issues a minimal `UPDATE` |
| `Entry(...).Reference(...).LoadAsync()` | `TaskRepository` add/update | Explicitly loads the category after a write, so the response reflects the new `CategoryId` |

## Dependency injection

Every dependency is registered in `Program.cs` and resolved by the built-in ASP.NET Core container:

```csharp
builder.Services.AddDbContext<TasksApiContext>(opt => opt.UseNpgsql(connectionString));

builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<TaskRepository>();

builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<TaskService>();
```

### Lifetimes

| Service | Lifetime | Reason |
| --- | --- | --- |
| `TasksApiContext` | Scoped (default of `AddDbContext`) | A `DbContext` is not thread-safe and tracks entities. One instance per HTTP request is the recommended lifetime. |
| Repositories | Scoped | They depend on the scoped `DbContext`. A longer lifetime (singleton) would capture a context that has already been disposed. |
| Services | Scoped | They depend on scoped repositories. |
| Controllers | Created per request by the framework | They receive services through their constructor. |

### Constructor injection with primary constructors

Classes declare their dependencies as C# 12 primary constructor parameters, and the container supplies them:

```csharp
public class TaskService(TaskRepository taskRepository, CategoryRepository categoryRepository)
```

`TaskService` depends on two repositories. This shows that the container resolves the whole dependency graph automatically: controller → service → repositories → `DbContext`.

### Configuration

The connection string is read from configuration (`appsettings.json`, environment variables, and so on) through `builder.Configuration`. The app fails fast at startup if it is missing.

## REST API

### Endpoints

Base URL: `https://localhost:5000/api`

| Method | Route | Body | Success | Errors |
| --- | --- | --- | --- | --- |
| `GET` | `/categories` | – | `200` list | – |
| `GET` | `/categories/{id}` | – | `200` item | `404` |
| `POST` | `/categories` | `CategoryRequest` | `201` + `Location` header | `400` |
| `PUT` | `/categories/{id}` | `CategoryRequest` | `200` item | `400`, `404` |
| `DELETE` | `/categories/{id}` | – | `204` | `404` |
| `GET` | `/tasks` | – | `200` list | – |
| `GET` | `/tasks/{id}` | – | `200` item | `404` |
| `POST` | `/tasks` | `TaskRequest` | `201` + `Location` header | `400` |
| `PUT` | `/tasks/{id}` | `TaskRequest` | `200` item | `400`, `404` |
| `DELETE` | `/tasks/{id}` | – | `204` | `404` |

`TasksAPI.http` contains one ready-made request for each endpoint. Send them with the VS Code REST Client extension or with the built-in Visual Studio / Rider HTTP client.

### Contracts (DTOs)

The API never exposes entity classes directly. It accepts and returns records from `Dtos/`:

```jsonc
// POST /api/tasks  (TaskRequest)
{ "name": "Write the README", "description": "Optional", "done": false, "categoryId": 1 }

// 201 Created, Location: /api/tasks/7  (TaskResponse)
{ "id": 7, "name": "Write the README", "description": "Optional", "done": false, "categoryId": 1, "categoryName": "Work" }
```

DTOs separate the public contract from the database model:

- Clients cannot set server-owned fields such as `id`, or send navigation collections.
- Responses contain only what clients need, for example `categoryName` instead of a nested object or an empty `tasks` array.
- There are no JSON reference cycles (`Category.Tasks` ↔ `Task.Category`).
- The database schema can change without breaking API consumers.

Mapping happens in the service layer (`FromEntity` and `ToEntity` methods on the records). Controllers only see DTOs, and repositories only see entities.

### Validation

- **Data annotations** on the request DTOs: `[Required]`, `[StringLength]` and `[Range]`.
- **`[ApiController]`** validates these annotations automatically and returns `400` before the action runs.
- **Business validation** in `TaskController` checks that the category exists. It adds the error to `ModelState`, so the response has the same shape as automatic validation errors.
- **Route constraints** (`{id:int}`) reject non-numeric ids at the routing level.

### Consistent errors (RFC 9110 / ProblemDetails)

Every error response uses the standard `application/problem+json` format:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "CategoryId": ["The category 9999 doesn't exist."] }
}
```

This behavior is configured in `Program.cs`:

- `AddProblemDetails()` registers the ProblemDetails service.
- `UseExceptionHandler()` turns unhandled exceptions into a `500` ProblemDetails response instead of a stack trace.
- `UseStatusCodePages()` gives empty error responses a ProblemDetails body.

### OpenAPI

`[Produces("application/json")]` and `[ProducesResponseType(...)]` describe every possible status code. `AddOpenApi()` and `MapOpenApi()` publish the specification at `/openapi/v1.json` in Development.

## Frontend

The ASP.NET Core app serves the static frontend itself (`UseDefaultFiles()` + `UseStaticFiles()`). The frontend and the API share the same origin, so no CORS configuration is needed.

| File | Purpose |
| --- | --- |
| `tasks.html` / `tasks.js` | Create, edit, complete and delete tasks; filter by category and status |
| `index.html` / `categories.js` | Create, rename and delete categories |
| `common.js` | Shared `fetch` wrapper that reads ProblemDetails errors and shows them to the user |
| `styles.css` | Styles with light and dark mode |


