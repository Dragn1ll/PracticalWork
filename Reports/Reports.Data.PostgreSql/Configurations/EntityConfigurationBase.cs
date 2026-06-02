using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reports.Abstractions.Storage.Entity;

namespace Reports.Data.PostgreSql.Configurations;

public abstract class EntityConfigurationBase<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class, IEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone") // То есть, UTC в современном Postgres.
            .HasDefaultValueSql("NOW()") // `now()` Генерит значение типа `timestamp with timezone` == UTC.
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone");
    }
}