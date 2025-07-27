using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DreamAquascape.Data.Models;
using static DreamAquascape.Data.Common.EntityConstants.UserContestParticipation;

namespace DreamAquascape.Data.Configuration
{
    public class UserContestParticipationConfiguration: IEntityTypeConfiguration<UserContestParticipation>
    {
        public void Configure(EntityTypeBuilder<UserContestParticipation> entity)
        { 
            entity.
                HasKey(ucp => ucp.Id);

            entity
                .Property(ucp => ucp.ContestId)
                .IsRequired();

            entity.Property(ucp => ucp.ParticipationDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(ucp => ucp.HasSubmittedEntry)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(ucp => ucp.HasVoted)
                .IsRequired()
                .HasDefaultValue(false);

            entity
                .Property(ucp => ucp.VotedForEntryId)
                .IsRequired(false);

            entity.Property(ucp => ucp.VotedAt)
                .IsRequired(false);

            entity.Property(ucp => ucp.SubmittedEntryId)
                .IsRequired(false);

            entity.Property(ucp => ucp.EntrySubmittedAt)
                .IsRequired(false);

            entity.HasOne(ucp => ucp.Contest)
                .WithMany(c => c.Participants)
                .HasForeignKey(ucp => ucp.ContestId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ucp => ucp.User)
                .WithMany() // IdentityUser doesn't need navigation back to participations
                .HasForeignKey(ucp => ucp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ucp => ucp.VotedForEntry)
                .WithMany() // ContestEntry doesn't need navigation back to participations
                .HasForeignKey(ucp => ucp.VotedForEntryId)
                .OnDelete(DeleteBehavior.NoAction); // No action to allow multiple votes, handled in business logic

            entity.HasOne(ucp => ucp.SubmittedEntry)
                .WithMany() // ContestEntry doesn't need navigation back to participations
                .HasForeignKey(ucp => ucp.SubmittedEntryId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasQueryFilter(ucp => !ucp.Contest.IsDeleted);
        }
    }
}
