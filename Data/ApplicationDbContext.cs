using Microsoft.EntityFrameworkCore;
using myapp.Models;

namespace myapp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
<<<<<<< HEAD
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Query>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SqlQuery).IsRequired();
            entity.Property(e => e.Result).IsRequired();
            entity.Property(e => e.ExecutedAt).IsRequired();
        });
    }

=======
        Database.EnsureCreated();
    }

>>>>>>> 9559f54c25a82f0b45e6d89035f8ebb26480d09b
    public DbSet<Query> Queries { get; set; } = null!;
} 