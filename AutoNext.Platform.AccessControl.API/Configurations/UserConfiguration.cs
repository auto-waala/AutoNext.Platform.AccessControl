using AutoNext.Platform.AccessControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNext.Platform.AccessControl.API.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.UserType);
            builder.Property(u => u.Metadata).HasColumnType("jsonb");
        }
    }
}
