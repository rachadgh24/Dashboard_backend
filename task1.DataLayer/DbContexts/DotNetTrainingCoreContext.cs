using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using task1.DataLayer.Entities;

namespace task1.DataLayer.DbContexts
{
    public class DotNetTrainingCoreContext : DbContext
    {
        public DotNetTrainingCoreContext(DbContextOptions<DotNetTrainingCoreContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<RoleClaim> RoleClaims { get; set; }
public DbSet<Tenant> Tenants { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoleClaim>()
                .HasKey(rc => new { rc.RoleId, rc.ClaimId });

            modelBuilder.Entity<RoleClaim>()
                .HasOne(rc => rc.Role)
                .WithMany(r => r.RoleClaims)
                .HasForeignKey(rc => rc.RoleId);

            modelBuilder.Entity<RoleClaim>()
                .HasOne(rc => rc.Claim)
                .WithMany(c => c.RoleClaims)
                .HasForeignKey(rc => rc.ClaimId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Car>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Notification>().HasQueryFilter(n => !n.IsDeleted);

            modelBuilder.Entity<User>().HasIndex(u => new { u.TenantId, u.IsDeleted });
            modelBuilder.Entity<Customer>().HasIndex(c => new { c.TenantId, c.IsDeleted });
            modelBuilder.Entity<Car>().HasIndex(c => new { c.TenantId, c.IsDeleted });
            modelBuilder.Entity<Notification>().HasIndex(n => new { n.TenantId, n.IsDeleted });

            modelBuilder.Entity<User>().HasIndex(u => new { u.TenantId, u.PhoneNumber });
            modelBuilder.Entity<Customer>().HasIndex(c => new { c.TenantId, c.Email });
        }
    }
}
