using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Greenix.Domain.Entities;

namespace Greenix.Persistence.Configurations
{
    public class WorkerAssignmentConfiguration : IEntityTypeConfiguration<WorkerAssignment>
    {
        public void Configure(EntityTypeBuilder<WorkerAssignment> builder)
        {
            builder.ToTable("WorkerAssignments");

            builder.HasKey(wa => wa.Id);
            builder.Property(wa => wa.Id).UseIdentityColumn();

            builder.Property(wa => wa.Notes)
                .HasMaxLength(500);

            builder.HasIndex(wa => wa.ComplaintId);
            builder.HasIndex(wa => wa.WorkerId);

            // Relationships
            builder.HasOne(wa => wa.Complaint)
                .WithMany(c => c.Assignments)
                .HasForeignKey(wa => wa.ComplaintId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(wa => wa.Worker)
                .WithMany(u => u.WorkerAssignments)
                .HasForeignKey(wa => wa.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(wa => wa.AssignedBy)
                .WithMany(u => u.CreatedAssignments)
                .HasForeignKey(wa => wa.AssignedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
