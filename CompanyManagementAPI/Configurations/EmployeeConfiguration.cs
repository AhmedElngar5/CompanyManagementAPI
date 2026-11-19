using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CompanyManagementAPI.Models;

namespace CompanyManagementAPI.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            // Unique index on Email
            builder.HasIndex(e => e.Email)
                .IsUnique();

            // Salary precision
            builder.Property(e => e.Salary)
                .HasColumnType("decimal(18,2)");

            builder.Property(e => e.HireDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationship configured in DepartmentConfiguration
        }
    }
}
