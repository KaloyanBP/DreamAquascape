using DreamAquascape.Data.Configuration.Base;
using DreamAquascape.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static DreamAquascape.Data.Common.EntityConstants.ContestEntry;

namespace DreamAquascape.Data.Configuration
{
    public class ContestEntryConfiguration : SoftDeletableEntityConfiguration<ContestEntry>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<ContestEntry> entity)
        {
            // Primary Key
            entity
                .HasKey(e => e.Id);

            entity
                .Property(ce => ce.Title)
                .IsRequired()
                .HasMaxLength(TitleMaxLength);

            entity
                .Property(ce => ce.Description)
                .IsRequired()
                .HasMaxLength(DescriptionMaxLength);

            entity
                .Property(ce => ce.SubmittedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity
                .Property(ce => ce.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Configure relationship with Contest
            entity.HasOne(ce => ce.Contest)
                .WithMany(c => c.Entries)
                .HasForeignKey(ce => ce.ContestId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ce => ce.Participant)
                .WithMany()
                .HasForeignKey(ce => ce.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ce => ce.Winner)
                .WithOne(cw => cw.ContestEntry)
                .HasForeignKey<ContestWinner>(cw => cw.ContestEntryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(ce => ce.Votes)
                .WithOne(v => v.ContestEntry)
                .HasForeignKey(v => v.ContestEntryId)
                .OnDelete(DeleteBehavior.Cascade); // When entry is deleted, delete its votes

            entity
                .HasIndex(ce => ce.ContestId);
        }
    }
}
