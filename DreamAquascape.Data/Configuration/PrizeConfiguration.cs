using DreamAquascape.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static DreamAquascape.Data.Common.EntityConstants.Prize;

namespace DreamAquascape.Data.Configuration
{
    public class PrizeConfiguration : IEntityTypeConfiguration<Prize>
    {
        public void Configure(EntityTypeBuilder<Prize> entity)
        {
            entity
                .HasKey(p => p.Id);

            entity.Property(p => p.ContestId)
                .IsRequired();

            entity
                .Property(p => p.Place)
                .IsRequired();
                
            entity
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(NameMaxLength);

            entity
                .Property(p => p.Description)
                .HasMaxLength(DescriptionMaxLength);

            entity
                .Property(p => p.NavigationUrl)
                .HasMaxLength(NavigationUrlMaxLength);

            entity
                .Property(p => p.ImageUrl)
                .HasMaxLength(ImageUrlMaxLength);

            entity
                .Property(p => p.MonetaryValue)
                .HasColumnType("decimal(10,2)");

            entity
                .Property(p => p.SponsorName)
                .HasMaxLength(SponsorNameMaxLength);

            // Relationships
            entity
                .HasOne(p => p.Contest)
                .WithMany(c => c.Prizes)
                .HasForeignKey(p => p.ContestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Soft delete filter
            entity
                .HasQueryFilter(p => !p.IsDeleted);

            // Unique constraints
            entity
                .HasIndex(p => new { p.ContestId, p.Place })
                .IsUnique()
                .HasDatabaseName("IX_Prize_ContestId_Place_Unique");
        }
    }
}