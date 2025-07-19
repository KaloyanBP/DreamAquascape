using DreamAquascape.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DreamAquascape.Data.Common.EntityConstants.EntryImage;

namespace DreamAquascape.Data.Configuration
{
    public class EntryImageConfiguration : IEntityTypeConfiguration<EntryImage>
    {
        public void Configure(EntityTypeBuilder<EntryImage> entity)
        {
            // Primary Key
            entity.HasKey(ei => ei.Id);

            // Properties
            entity.Property(ei => ei.ContestEntryId)
                .IsRequired();

            entity.Property(ei => ei.ImageUrl)
                .IsRequired()
                .HasMaxLength(ImageUrlMaxLength);

            entity.Property(ei => ei.Caption)
                .HasMaxLength(CaptionMaxLength)
                .IsRequired(false); // Nullable

            entity.Property(ei => ei.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(ei => ei.UploadedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Foreign Key Relationship
            entity.HasOne(ei => ei.ContestEntry)
                .WithMany(ce => ce.EntryImages)
                .HasForeignKey(ei => ei.ContestEntryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
