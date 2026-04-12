using AutoNext.Platform.AccessControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNext.Platform.AccessControl.API.Configurations
{
    public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.HasIndex(o => o.Code).IsUnique();
            builder.Property(o => o.Metadata).HasColumnType("jsonb");
        }
    }
}
