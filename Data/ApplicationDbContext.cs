using Microsoft.EntityFrameworkCore;
using myapp.Models;

namespace myapp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
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

    public DbSet<Query> Queries { get; set; } = null!;
} 