using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;

var builder = WebApplication.CreateBuilder(args);

// Basic services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable static files for index.html
app.UseDefaultFiles();
app.UseStaticFiles();

// Move this BEFORE app.UseSwagger() and app.UseSwaggerUI()
app.MapGet("/api/debug/connection", (ApplicationDbContext db) =>
{
    var dbPath = db.Database.GetConnectionString();
    var todoCount = db.Todos.Count();
    return new { DatabasePath = dbPath, NumberOfTodos = todoCount };
});

// Then your existing Swagger setup
app.UseSwagger();
app.UseSwaggerUI();

// Basic CRUD endpoints
app.MapGet("/api/todos", async (ApplicationDbContext db) =>
    await db.Todos.ToListAsync());

app.MapGet("/api/todos/{id}", async (int id, ApplicationDbContext db) =>
    await db.Todos.FindAsync(id));

app.MapPost("/api/todos", async (Todo todo, ApplicationDbContext db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return todo;
});

// Add PUT endpoint for updating todo status
app.MapPut("/api/todos/{id}", async (int id, Todo inputTodo, ApplicationDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo == null) return Results.NotFound();
    
    todo.IsComplete = inputTodo.IsComplete;
    todo.Title = inputTodo.Title;
    await db.SaveChangesAsync();
    return Results.Ok(todo);
});

app.MapDelete("/api/todos/{id}", async (int id, ApplicationDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo != null)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
    }
    return todo;
});

app.Run();