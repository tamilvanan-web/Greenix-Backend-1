using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Greenix.Domain.Entities;
using Greenix.Domain.Common;
using Greenix.Application.Interfaces;

namespace Greenix.Persistence.Context
{
    public class ApplicationDbContext : IdentityDbContext<User>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Complaint> Complaints => Set<Complaint>();
        public DbSet<ComplaintImage> ComplaintImages => Set<ComplaintImage>();
        public DbSet<WorkerAssignment> WorkerAssignments => Set<WorkerAssignment>();
        public DbSet<ComplaintStatusHistory> ComplaintStatusHistories => Set<ComplaintStatusHistory>();
        public DbSet<Feedback> Feedbacks => Set<Feedback>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        // In a real-world scenario, you'd fetch the current user's name/ID from an ICurrentUserService.
                        // We will add a fallback for now.
                        entry.Entity.CreatedBy ??= "System";
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedBy ??= "System";
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Scan and apply Fluent API configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
