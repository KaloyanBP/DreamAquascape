using DreamAquascape.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamAquascape.Data.Configuration
{
    public class ContestsCategoriesConfiguration: IEntityTypeConfiguration<ContestsCategories>
    {
        public void Configure(EntityTypeBuilder<ContestsCategories> entity)
        {
            entity
                .HasKey(cc => new { cc.ContestId, cc.CategoryId });
            
            entity.HasOne(cc => cc.Contest)
                .WithMany(c => c.Categories)
                .HasForeignKey(cc => cc.ContestId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(cc => cc.Category)
                .WithMany(c => c.Contests)
                .HasForeignKey(cc => cc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(cc => !cc.Contest.IsDeleted);
        }
    }
}
