using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<TodoTask> Tasks => Set<TodoTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configura índice único no e-mail para evitar duplicatas
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Configura o relacionamento 1:N entre User e TodoTask
        modelBuilder.Entity<TodoTask>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Armazena o enum TaskStatus como string no banco (mais legível)
        modelBuilder.Entity<TodoTask>()
            .Property(t => t.Status)
            .HasConversion<string>();
    }
}
