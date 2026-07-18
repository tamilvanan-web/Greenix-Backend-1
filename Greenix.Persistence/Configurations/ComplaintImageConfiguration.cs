using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Greenix.Domain.Entities;

namespace Greenix.Persistence.Configurations
{
    public class ComplaintImageConfiguration : IEntityTypeConfiguration<ComplaintImage>
    {
        public void Configure(EntityTypeBuilder<ComplaintImage> builder)
        {
            builder.ToTable("ComplaintImages");

            builder.HasKey(ci => ci.Id);
            builder.Property(ci => ci.Id).UseIdentityColumn();

            builder.Property(ci => ci.ImageUrl)
                .IsRequired()
                .HasMaxLength(2083);

            builder.Property(ci => ci.ImageType)
                .HasConversion<byte>()
                .IsRequired();

            builder.HasIndex(ci => ci.ComplaintId);

            // Relationships
            builder.HasOne(ci => ci.Complaint)
                .WithMany(c => c.Images)
                .HasForeignKey(ci => ci.ComplaintId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ci => ci.UploadedBy)
                .WithMany(u => u.UploadedImages)
                .HasForeignKey(ci => ci.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
