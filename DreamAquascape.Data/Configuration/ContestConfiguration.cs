using DreamAquascape.Data.Configuration.Base;
using DreamAquascape.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static DreamAquascape.Data.Common.EntityConstants.Contest;

namespace DreamAquascape.Data.Configuration
{
    public class ContestConfiguration : SoftDeletableEntityConfiguration<Contest>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Contest> entity)
        {
            // Primary Key
            entity
                .HasKey(e => e.Id);

            // Define constraints for the Title column
            entity
                .Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(TitleMaxLength);

            // Define constraints for the Description column
            entity
                .Property(c => c.Description)
                .HasMaxLength(DescriptionMaxLength);

            // Define constraints for the ImageFileUrl column
            entity
                .Property(c => c.ImageFileUrl)
                .HasMaxLength(ImageFileUrlMaxLength);

            // Define constraints for the SubmissionStartDate column
            entity
                .Property(c => c.SubmissionStartDate)
                .IsRequired();

            // Define constraints for the SubmissionEndDate column
            entity
                .Property(c => c.SubmissionEndDate)
                .IsRequired();

            // Define constraints for the VotingStartDate column
            entity
                .Property(c => c.VotingStartDate)
                .IsRequired();

            // Define constraints for the VotingEndDate column
            entity
                .Property(c => c.VotingEndDate)
                .IsRequired();

            // Define constraints for the ResultDate column
            entity
                .Property(c => c.ResultDate);

            // Define constraints for the CreatedBy column
            entity
                .Property(c => c.CreatedBy)
                .IsRequired()
                .HasMaxLength(CreatedByMaxLength);

            // Define constraints for the IsActive column
            entity
                .Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // One-to-zero-or-one relationship
            entity
                .HasMany(c => c.Prizes)
                .WithOne(p => p.Contest)
                .HasForeignKey(p => p.ContestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure one-to-many relationship
            entity.HasMany(c => c.Entries)
                .WithOne(ce => ce.Contest)
                .HasForeignKey(ce => ce.ContestId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-Many with relationship entity
            entity.HasMany(c => c.Categories)
                .WithOne(cc => cc.Contest)
                .HasForeignKey(cc => cc.ContestId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(c => c.Winners)
                .WithOne(cw => cw.Contest)
                .HasForeignKey(cw => cw.ContestId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
