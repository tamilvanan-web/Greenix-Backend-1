using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Greenix.Domain.Entities;

namespace Greenix.Persistence.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("Departments");

            builder.HasKey(d => d.Id);
            builder.Property(d => d.Id).UseIdentityColumn();

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.Description)
                .HasMaxLength(500);

            builder.HasIndex(d => d.Name)
                .IsUnique();

            builder.HasData(
                new Department
                {
                    Id = 1,
                    Name = "Sanitation",
                    Description = "Sanitation and waste management",
                    CreatedAt = new DateTime(2026, 1, 1),
                    CreatedBy = "System"
                },
                new Department
                {
                    Id = 2,
                    Name = "Water & Sewage",
                    Description = "Water supply and sewage management",
                    CreatedAt = new DateTime(2026, 1, 1),
                    CreatedBy = "System"
                }
            );
        }
    }
}