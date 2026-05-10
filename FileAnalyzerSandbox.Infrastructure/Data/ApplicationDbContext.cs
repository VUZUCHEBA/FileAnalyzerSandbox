using FileAnalyzerSandbox.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;

namespace FileAnalyzerSandbox.Infrastructure.Data;

/// <summary>
/// Контекст базы данных для работы с Entity Framework Core
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Инициализирует новый экземпляр контекста базы данных
    /// </summary>
    /// <param name="options">Настройки подключения к БД</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>Коллекция анализов файлов</summary>
    public DbSet<FileAnalysis> FileAnalyses { get; set; }
    /// <summary>Коллекция пользователей</summary>
    public DbSet<User> Users { get; set; }
    /// <summary> Коллекция логов анализа</summary>
    public DbSet<AnalysisLog> AnalysisLogs { get; set; }

    /// <summary>
    /// Настройка моделей и отношений с помощью Fluent API
    /// </summary>
    /// <param name="modelBuilder">Построитель модели</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);

            entity.HasMany(e => e.FileAnalyses)
                  .WithOne(e => e.User)
                  .HasForeignKey(e => e.UserId);
        });

        // FileAnalysis configuration
        modelBuilder.Entity<FileAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FileHash);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FileHash).IsRequired().HasMaxLength(64);

            entity.HasMany(e => e.AnalysisLogs)
                  .WithOne(e => e.FileAnalysis)
                  .HasForeignKey(e => e.FileAnalysisId);
        });

        // AnalysisLog configuration
        modelBuilder.Entity<AnalysisLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LogLevel).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
        });
    }
}

// ЭТОТ КЛАСС ОБЯЗАТЕЛЕН ДЛЯ МИГРАЦИЙ!
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Для миграций используем SQLite
        optionsBuilder.UseSqlite("Data Source=fileanalyzer.db");

        // Опционально: включить логирование
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}