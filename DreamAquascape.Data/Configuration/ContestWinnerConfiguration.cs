using DreamAquascape.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamAquascape.Data.Configuration
{
    public class ContestWinnerConfiguration : IEntityTypeConfiguration<ContestWinner>
    {
        public void Configure(EntityTypeBuilder<ContestWinner> entity)
        {
            // Composite Primary Key (all three fields)
            entity.HasKey(cw => new { cw.ContestId, cw.ContestEntryId, cw.Position });

            // Properties
            entity.Property(cw => cw.Position)
                .IsRequired();

            entity.Property(cw => cw.WonAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(cw => cw.AwardTitle)
                .HasMaxLength(200);

            // Relationships
            entity.HasOne(cw => cw.Contest)
                .WithMany(c => c.Winners)
                .HasForeignKey(cw => cw.ContestId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cw => cw.ContestEntry)
                .WithOne(ce => ce.Winner) // Entry can only win once
                .HasForeignKey<ContestWinner>(cw => cw.ContestEntryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Soft delete filter
            entity.HasQueryFilter(cw => !cw.IsDeleted &&
                 !cw.Contest.IsDeleted && 
                 !cw.ContestEntry.IsDeleted);

            // Unique constraints

            // Ensure only one winner per position per contest
            entity.HasIndex(cw => new { cw.ContestId, cw.Position })
                .IsUnique()
                .HasDatabaseName("IX_ContestWinner_Contest_Position_Unique");

            // Ensure one entry can only win once
            entity.HasIndex(cw => cw.ContestEntryId)
                .IsUnique()
                .HasDatabaseName("IX_ContestWinner_Entry_Unique");
        }
    }
}
