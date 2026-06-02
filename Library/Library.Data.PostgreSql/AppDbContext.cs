using Library.Abstraction.Storage.Entity;
using Library.Data.PostgreSql.Entities;
using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Data.PostgreSql;

/// <summary>
/// Контекст EF Core для приложения
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    #region Set UpdateDate on SaveChanges

    // Данные перегрузки выбраны потому, что оставшиеся две вызывают эти методы:

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetUpdateDates();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetUpdateDates();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void SetUpdateDates()
    {
        var updateDate = DateTime.UtcNow;

        var updatedEntries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in updatedEntries)
        {
            if (entry.Entity is IEntity entity)
                entity.UpdatedAt = updateDate;
        }
    }

    #endregion

    public DbSet<AbstractBookEntity> Books { get; set; }
    internal DbSet<EducationalBookEntity> EducationalBooks { get; set; }
    internal DbSet<FictionBookEntity> FictionBooks { get; set; }
    internal DbSet<ScientificBookEntity> ScientificBooks { get; set; }
    public DbSet<ReaderEntity> Readers { get; set; }
    public DbSet<BookBorrowEntity> BookBorrows { get; set; }
    public DbSet<NotificationLogEntity> NotificationLogs { get; set; }
}