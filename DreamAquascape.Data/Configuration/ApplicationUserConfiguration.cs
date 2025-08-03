using DreamAquascape.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamAquascape.Data.Configuration
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> entity)
        {
            // Configure custom properties
            entity.Property(u => u.FirstName)
                .HasMaxLength(50)
                .IsRequired(false);

            entity.Property(u => u.LastName)
                .HasMaxLength(50)
                .IsRequired(false);

            entity.Property(u => u.DisplayName)
                .HasMaxLength(100)
                .IsRequired(false);

            // Indexes for performance
            entity.HasIndex(u => u.DisplayName);
        }
    }
}
