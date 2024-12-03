using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using System.Data;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Basic services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure database and tables are created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // This will create the database and all tables
        context.Database.EnsureDeleted(); // Remove old database
        context.Database.EnsureCreated(); // Create fresh database
        
        // Create Queries table explicitly if needed
        using var connection = new SqliteConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
        connection.Open();
        using var command = new SqliteCommand(@"
            CREATE TABLE IF NOT EXISTS Queries (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SqlQuery TEXT NOT NULL,
                Result TEXT NOT NULL,
                ExecutedAt TEXT NOT NULL
            )", connection);
        command.ExecuteNonQuery();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the database.");
    }
}

// Enable static files for index.html
app.UseDefaultFiles();
app.UseStaticFiles();

// Move this BEFORE app.UseSwagger() and app.UseSwaggerUI()
app.MapGet("/api/debug/connection", (ApplicationDbContext db) =>
{
    var dbPath = db.Database.GetConnectionString();
    return new { DatabasePath = dbPath };
});

// Then your existing Swagger setup
app.UseSwagger();
app.UseSwaggerUI();

// Ensure the SQL query endpoints are correctly set up
app.MapPost("/api/execute-query", async (QueryRequest request, ApplicationDbContext db) =>
{
    try
    {
        var result = new List<Dictionary<string, object>>();
        using (var connection = new SqliteConnection(builder.Configuration.GetConnectionString("DefaultConnection")))
        {
            await connection.OpenAsync();
            using var command = new SqliteCommand(request.SqlQuery, connection);
            
            // For CREATE TABLE and other non-query commands
            if (request.SqlQuery.Trim().ToUpper().StartsWith("CREATE") || 
                request.SqlQuery.Trim().ToUpper().StartsWith("INSERT") || 
                request.SqlQuery.Trim().ToUpper().StartsWith("UPDATE") || 
                request.SqlQuery.Trim().ToUpper().StartsWith("DELETE"))
            {
                var rowsAffected = await command.ExecuteNonQueryAsync();
                result.Add(new Dictionary<string, object> { 
                    { "message", $"Command executed successfully. Rows affected: {rowsAffected}" } 
                });
            }
            else // For SELECT queries
            {
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i) ?? DBNull.Value;
                    }
                    result.Add(row);
                }
            }
        }

        // Save query history
        var queryRecord = new Query
        {
            SqlQuery = request.SqlQuery,
            Result = System.Text.Json.JsonSerializer.Serialize(result),
            ExecutedAt = DateTime.UtcNow
        };
        db.Queries.Add(queryRecord);
        await db.SaveChangesAsync();

        return Results.Ok(new { data = result });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/query-history", async (ApplicationDbContext db) =>
{
    try 
    {
        var history = await db.Queries
            .OrderByDescending(q => q.ExecutedAt)
            .Take(10)
            .ToListAsync();
        return Results.Ok(history);
    }
    catch (Exception)
    {
        // Return empty list if table doesn't exist
        return Results.Ok(new List<Query>());
    }
});

app.MapDelete("/api/query-history", async (ApplicationDbContext db) =>
{
    try 
    {
        // Delete all records from the Queries table
        db.Queries.RemoveRange(db.Queries);
        await db.SaveChangesAsync();
        return Results.Ok(new { message = "History deleted successfully" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Add this endpoint before app.Run()
app.MapGet("/api/tables", async (ApplicationDbContext db) =>
{
    try
    {
        var result = new List<Dictionary<string, object>>();
        using (var connection = new SqliteConnection(builder.Configuration.GetConnectionString("DefaultConnection")))
        {
            await connection.OpenAsync();
            using var command = new SqliteCommand(
                @"SELECT name FROM sqlite_master 
                  WHERE type='table' 
                  AND name NOT LIKE 'sqlite_%' 
                  AND name NOT LIKE 'Queries'", connection);
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new Dictionary<string, object>
                {
                    { "tableName", reader.GetString(0) }
                });
            }
        }
        return Results.Ok(new { data = result });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();