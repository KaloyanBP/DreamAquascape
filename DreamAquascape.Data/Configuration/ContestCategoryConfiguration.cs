using DreamAquascape.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamAquascape.Data.Configuration
{
    public class ContestCategoryConfiguration : IEntityTypeConfiguration<ContestCategory>
    {
        public void Configure(EntityTypeBuilder<ContestCategory> entity)
        {
            entity.HasKey(cc => cc.Id);

            entity.Property(cc => cc.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(cc => cc.Description)
                .HasMaxLength(500);

            entity.HasIndex(cc => cc.Name)
                .IsUnique();

            // One-to-Many with relationship entity
            entity.HasMany(cc => cc.ContestsCategories)
                .WithOne(cr => cr.Category)
                .HasForeignKey(cr => cr.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable("ContestCategories");
        }
    }
}
