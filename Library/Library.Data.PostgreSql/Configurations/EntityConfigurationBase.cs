using Library.Abstraction.Storage.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Data.PostgreSql.Configurations;

internal abstract class EntityConfigurationBase<TEntity> : IEntityTypeConfiguration<TEntity>
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