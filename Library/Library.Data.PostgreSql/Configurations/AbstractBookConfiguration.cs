using Library.Data.PostgreSql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Data.PostgreSql.Configurations;

internal sealed class AbstractBookConfiguration : EntityConfigurationBase<AbstractBookEntity>
{
    public override void Configure(EntityTypeBuilder<AbstractBookEntity> builder)
    {
        base.Configure(builder);

        // Используется схема наследования TPT по следующим причинам:
        // 1. Для книги нужна сквозная нумерация Id. У TPC она тоже возможна, но у TPT нагляднее.
        // 2. Для упрощения привязки общих таблиц и возможности использовать FK-ключи и ограничения на уровне БД.
        builder.UseTptMappingStrategy();

        builder.Property(p => p.Title)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.Authors)
            .IsRequired();

        builder.Property(p => p.Year)
            .IsRequired();

        builder.Property(p => p.CoverImagePath)
            .HasMaxLength(500);

        builder.HasMany(c => c.IssuanceRecords)
            .WithOne()
            .HasForeignKey(p => p.BookId);
    }
}