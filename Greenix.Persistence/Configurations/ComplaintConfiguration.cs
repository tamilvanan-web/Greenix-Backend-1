using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Greenix.Domain.Entities;

namespace Greenix.Persistence.Configurations
{
    public class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
    {
        public void Configure(EntityTypeBuilder<Complaint> builder)
        {
            builder.ToTable("Complaints");

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).UseIdentityColumn();

            builder.Property(c => c.ComplaintNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.Description)
                .IsRequired();

            builder.Property(c => c.Latitude)
                .HasPrecision(9, 6);

            builder.Property(c => c.Longitude)
                .HasPrecision(9, 6);

            builder.Property(c => c.Status)
                .HasConversion<byte>()
                .IsRequired();

            builder.Property(c => c.Priority)
                .HasConversion<byte>()
                .IsRequired();

            builder.HasIndex(c => c.ComplaintNumber)
                .IsUnique();

            builder.HasIndex(c => c.CitizenId);
            builder.HasIndex(c => c.CategoryId);
            builder.HasIndex(c => c.Status);

            // Relationships
            builder.HasOne(c => c.Citizen)
                .WithMany(u => u.CitizenComplaints)
                .HasForeignKey(c => c.CitizenId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Category)
                .WithMany(cat => cat.Complaints)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
