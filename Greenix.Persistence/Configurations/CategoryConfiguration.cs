using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Greenix.Domain.Entities;

namespace Greenix.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).UseIdentityColumn();

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.HasIndex(c => c.Name)
                .IsUnique();

            builder.HasOne(c => c.Department)
                .WithMany(d => d.Categories)
                .HasForeignKey(c => c.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(
                new Category
                {
                    Id = 1,
                    DepartmentId = 1,
                    Name = "Sanitation & Waste",
                    Description = "Trash collection and road cleaning",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CreatedBy = "System"
                },
                new Category
                {
                    Id = 2,
                    DepartmentId = 2,
                    Name = "Water & Sewage",
                    Description = "Water leakage and sewage issues",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CreatedBy = "System"
                }
            );
        }
    }
}