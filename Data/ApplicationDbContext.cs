using Microsoft.EntityFrameworkCore;
using myapp.Models;

namespace myapp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
        
        // Add this line to print the database path
        Console.WriteLine($"Database path: {Database.GetConnectionString()}");
        
        // Add seed data if the database is empty
        if (!Todos.Any())
        {
            Todos.AddRange(
                new Todo { Title = "Learn C#", IsComplete = false },
                new Todo { Title = "Build a Todo App", IsComplete = false },
                new Todo { Title = "Master Entity Framework", IsComplete = false }
            );
            SaveChanges();
        }
    }

    public DbSet<Todo> Todos { get; set; } = null!;
} 