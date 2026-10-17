using Microsoft.EntityFrameworkCore;
using TasksAPI.Data;
using TasksAPI.Services;
using TasksAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

string? connectionString = builder.Configuration.GetConnectionString("ConnectionString");

if(connectionString is null)
{
    throw new Exception("The connection string wasn't defined in appsettings.json.");
}

builder.Services.AddDbContext<TasksApiContext>(opt => opt.UseNpgsql(connectionString));

// Addition of repositories by ID
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<TaskRepository>();

// Addition of services by ID
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<TaskService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.Run();
