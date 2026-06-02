using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reports.Data.PostgreSql.Entities;

namespace Reports.Data.PostgreSql.Configurations;

public class ActivityLogConfiguration : EntityConfigurationBase<ActivityLogEntity>
{
    public override void Configure(EntityTypeBuilder<ActivityLogEntity> builder)
    {
        base.Configure(builder);
        
        builder.Property(p => p.EventType)
            .IsRequired();
        
        builder.Property(p => p.EventDate)
            .IsRequired();
        
        builder.Property(b => b.Metadata)
            .HasColumnType("jsonb")
            .IsRequired();
    }
}