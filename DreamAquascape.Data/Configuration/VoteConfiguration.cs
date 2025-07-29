using DreamAquascape.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static DreamAquascape.Data.Common.EntityConstants.Vote;

namespace DreamAquascape.Data.Configuration
{
    public class VoteConfiguration : IEntityTypeConfiguration<Vote>
    {
        public void Configure(EntityTypeBuilder<Vote> entity)
        {
            // Primary Key
            entity.HasKey(v => v.Id);

            // Properties
            entity.Property(v => v.ContestEntryId)
                .IsRequired();

            entity.Property(v => v.UserId)
                .IsRequired()
                .HasMaxLength(450); // Standard IdentityUser ID length

            entity.Property(v => v.VotedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasConversion(
                    v => v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            entity.Property(v => v.IpAddress)
                .HasMaxLength(45) // IPv6 max length
                .IsRequired(false);

            // Foreign Key Relationships
            entity.HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.ContestEntry)
                .WithMany(ce => ce.Votes)
                .HasForeignKey(v => v.ContestEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(v => new { v.UserId, v.ContestEntryId })
                .IsUnique()
                .HasDatabaseName("IX_Vote_User_Entry_Unique"); // Prevent duplicate votes

            entity.HasIndex(v => v.ContestEntryId);
            entity.HasIndex(v => v.VotedAt);

            // Global query filter
            entity.HasQueryFilter(v => !v.ContestEntry.IsDeleted && !v.ContestEntry.Contest.IsDeleted);
        }
    }
}
