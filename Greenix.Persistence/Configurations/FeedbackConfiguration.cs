using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Greenix.Domain.Entities;

namespace Greenix.Persistence.Configurations
{
    public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
    {
        public void Configure(EntityTypeBuilder<Feedback> builder)
        {
            builder.ToTable("Feedbacks");

            builder.HasKey(f => f.Id);
            builder.Property(f => f.Id).UseIdentityColumn();

            builder.Property(f => f.Rating)
                .IsRequired();

            builder.Property(f => f.Comments)
                .HasMaxLength(1000);

            // Force unique index on ComplaintId to establish a strict 1-to-1 relation
            builder.HasIndex(f => f.ComplaintId)
                .IsUnique();

            builder.HasIndex(f => f.CitizenId);

            // Relationships
            builder.HasOne(f => f.Complaint)
                .WithOne(c => c.Feedback)
                .HasForeignKey<Feedback>(f => f.ComplaintId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.Citizen)
                .WithMany(u => u.Feedbacks)
                .HasForeignKey(f => f.CitizenId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
