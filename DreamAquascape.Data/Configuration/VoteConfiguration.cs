using DreamAquascape.Data.Configuration.Base;
using DreamAquascape.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamAquascape.Data.Configuration
{
    public class VoteConfiguration : SoftDeletableEntityConfiguration<Vote>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Vote> entity)
        {
            // Properties specific to Vote entity
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
                .HasDatabaseName("IX_Vote_User_Entry_Unique") // Prevent duplicate votes
                .HasFilter("[IsDeleted] = 0"); // Only active votes should be unique

            entity.HasIndex(v => v.ContestEntryId);
            entity.HasIndex(v => v.VotedAt);
        }

        protected override void ConfigureSoftDeletion(EntityTypeBuilder<Vote> builder)
        {
            // Call base configuration first
            base.ConfigureSoftDeletion(builder);

            // Override the global query filter to include related entity checks
            builder.HasQueryFilter(v => !v.IsDeleted && !v.ContestEntry.IsDeleted && !v.ContestEntry.Contest.IsDeleted);
        }
    }
}
