using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reports.Data.PostgreSql.Entities;

namespace Reports.Data.PostgreSql.Configurations;

public class ReportConfiguration : EntityConfigurationBase<ReportEntity>
{
    public override void Configure(EntityTypeBuilder<ReportEntity> builder)
    {
        base.Configure(builder);
        
        builder.Property(p => p.Name)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.HasIndex(p => p.Name)
            .IsUnique();
        
        builder.Property(p => p.FilePath)
            .HasMaxLength(500)
            .IsRequired();
        
        builder.Property(b => b.PeriodFrom)
            .IsRequired();
        
        builder.Property(b => b.PeriodTo)
            .IsRequired();
        
        builder.Property(b => b.Status)
            .IsRequired();
    }
}