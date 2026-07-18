using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Greenix.Domain.Entities;

namespace Greenix.Persistence.Configurations
{
    public class ComplaintStatusHistoryConfiguration : IEntityTypeConfiguration<ComplaintStatusHistory>
    {
        public void Configure(EntityTypeBuilder<ComplaintStatusHistory> builder)
        {
            builder.ToTable("ComplaintStatusHistory");

            builder.HasKey(csh => csh.Id);
            builder.Property(csh => csh.Id).UseIdentityColumn();

            builder.Property(csh => csh.Status)
                .HasConversion<byte>()
                .IsRequired();

            builder.Property(csh => csh.Comments)
                .HasMaxLength(1000);

            builder.HasIndex(csh => csh.ComplaintId);

            // Relationships
            builder.HasOne(csh => csh.Complaint)
                .WithMany(c => c.StatusHistories)
                .HasForeignKey(csh => csh.ComplaintId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(csh => csh.ChangedBy)
                .WithMany(u => u.StatusChanges)
                .HasForeignKey(csh => csh.ChangedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
