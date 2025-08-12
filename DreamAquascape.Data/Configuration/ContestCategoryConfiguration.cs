using DreamAquascape.Data.Configuration.Base;
using DreamAquascape.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamAquascape.Data.Configuration
{
    public class ContestCategoryConfiguration : SoftDeletableEntityConfiguration<ContestCategory>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<ContestCategory> entity)
        {
            entity
                .HasKey(cc => cc.Id);

            entity.Property(cc => cc.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(cc => cc.Description)
                .HasMaxLength(500);

            // Unique index on Name, but only for non-deleted records
            entity.HasIndex(cc => cc.Name)
                .IsUnique()
                .HasFilter("IsDeleted = 0");

            // One-to-Many with relationship entity
            entity.HasMany(cc => cc.ContestsCategories)
                .WithOne(cr => cr.Category)
                .HasForeignKey(cr => cr.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable("ContestCategories");
        }
    }
}
