using DreamAquascape.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static DreamAquascape.Data.Common.EntityConstants.ContestEntry;

namespace DreamAquascape.Data.Configuration
{
    public class ContestEntryConfiguration : IEntityTypeConfiguration<ContestEntry>
    {
        public void Configure(EntityTypeBuilder<ContestEntry> entity)
        {
            entity
                .HasKey(ce => ce.Id);

            entity
                .Property(ce => ce.Id)
                .ValueGeneratedOnAdd();

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
                .HasDefaultValueSql("GETDATE()");

            entity
                .Property(ce => ce.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity
                .Property(ce => ce.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Configure relationship with Contest
            entity.HasOne(ce => ce.Contest)
                .WithMany(c => c.Entries)
                .HasForeignKey(ce => ce.ContestId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ce => ce.Participant)
                .WithMany()
                .HasForeignKey(ce => ce.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(ce => ce.EntryImages)
                .WithOne(ei => ei.ContestEntry)
                .HasForeignKey(ei => ei.ContestEntryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(ce => ce.Votes)
                .WithOne(v => v.ContestEntry)
                .HasForeignKey(v => v.ContestEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasIndex(ce => ce.ContestId);

            // Global query filter to exclude soft deleted contest entries
            entity
                .HasQueryFilter(ce => !ce.IsDeleted);
        }
    }
}
