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
            entity.Property(v => v.ContestId)
                .IsRequired();

            entity.Property(v => v.ContestEntryId)
                .IsRequired();

            entity.Property(v => v.VotedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(v => v.IpAddress)
                .HasMaxLength(IpAddressMaxLength)
                .IsRequired(false);

            // Foreign Key Relationships
            entity.HasOne(v => v.User)
                       .WithMany()
                       .HasForeignKey(v => v.UserId)
                       .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.Contest)
                .WithMany(c => c.Votes)
                .HasForeignKey(v => v.ContestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.ContestEntry)
                .WithMany(ce => ce.Votes)
                .HasForeignKey(v => v.ContestEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(v => !v.Contest.IsDeleted);
        }
    }
}
