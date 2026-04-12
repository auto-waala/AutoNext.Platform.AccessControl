using AutoNext.Platform.AccessControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoNext.Platform.AccessControl.API.Data.Context
{
    public class IdentityDbContext : DbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<UserOrganization> UserOrganizations => Set<UserOrganization>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
        public DbSet<UserSession> UserSessions => Set<UserSession>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UserRole composite key
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId, ur.OrganizationId });

            // RolePermission composite key
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // UserOrganization composite key
            modelBuilder.Entity<UserOrganization>()
                .HasKey(uo => new { uo.UserId, uo.OrganizationId });

            // Relationships
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            modelBuilder.Entity<UserOrganization>()
                .HasOne(uo => uo.User)
                .WithMany(u => u.UserOrganizations)
                .HasForeignKey(uo => uo.UserId);

            modelBuilder.Entity<UserOrganization>()
                .HasOne(uo => uo.Organization)
                .WithMany(o => o.UserOrganizations)
                .HasForeignKey(uo => uo.OrganizationId);

            // Seed default roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Super Admin", Code = "super_admin", RoleType = "System", IsSystemRole = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow },
                new Role { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Admin", Code = "admin", RoleType = "System", IsSystemRole = true, DisplayOrder = 2, CreatedAt = DateTime.UtcNow },
                new Role { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Dealer Admin", Code = "dealer_admin", RoleType = "Organization", IsSystemRole = true, DisplayOrder = 3, CreatedAt = DateTime.UtcNow },
                new Role { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Dealer Staff", Code = "dealer_staff", RoleType = "Organization", IsSystemRole = true, DisplayOrder = 4, CreatedAt = DateTime.UtcNow },
                new Role { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Buyer", Code = "buyer", RoleType = "System", IsSystemRole = true, DisplayOrder = 5, CreatedAt = DateTime.UtcNow },
                new Role { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "Seller", Code = "seller", RoleType = "System", IsSystemRole = true, DisplayOrder = 6, CreatedAt = DateTime.UtcNow }
            );

            // Seed default permissions
            var permissions = new List<Permission>();
            var actions = new[] { "create", "read", "update", "delete", "manage" };
            var resources = new[] { "users", "roles", "permissions", "organizations", "vehicles", "documents", "warranties" };

            var counter = 1;
            foreach (var resource in resources)
            {
                foreach (var action in actions)
                {
                    permissions.Add(new Permission
                    {
                        Id = Guid.Parse($"00000000-0000-0000-0000-{counter:D12}"),
                        Name = $"{action.ToUpper()} {resource}",
                        Code = $"{resource}:{action}",
                        Resource = resource,
                        Action = action,
                        CreatedAt = DateTime.UtcNow
                    });
                    counter++;
                }
            }

            modelBuilder.Entity<Permission>().HasData(permissions);
        }
    }
}
