using DreamAquascape.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamAquascape.Data.Configuration.Base
{
    /// <summary>
    /// Provides common configuration for soft deletion properties.
    /// </summary>
    public abstract class SoftDeletableEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
        where TEntity : SoftDeletableEntity
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> entity)
        {
            // Configure base entity properties
            ConfigureBaseEntity(entity);

            // Configure soft deletion properties
            ConfigureSoftDeletion(entity);

            // Configure custom entity properties (implemented by derived classes)
            ConfigureEntity(entity);
        }

        /// <summary>
        /// Configure base entity properties (Id, CreatedAt, UpdatedAt)
        /// </summary>
        protected virtual void ConfigureBaseEntity(EntityTypeBuilder<TEntity> entity)
        {
            // Primary Key
            entity.HasKey(e => e.Id);

            // CreatedAt
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasConversion(
                    v => v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            // UpdatedAt
            entity.Property(e => e.UpdatedAt)
                .IsRequired(false)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToUniversalTime() : (DateTime?)null,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);
        }

        /// <summary>
        /// Configure soft deletion properties
        /// </summary>
        protected virtual void ConfigureSoftDeletion(EntityTypeBuilder<TEntity> entity)
        {
            // IsDeleted
            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // DeletedAt
            entity.Property(e => e.DeletedAt)
                .IsRequired(false)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToUniversalTime() : (DateTime?)null,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);

            // DeletedBy
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(450) // Standard IdentityUser ID length
                .IsRequired(false);

            // Index for soft deletion queries
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => e.DeletedAt);

            // Global query filter to exclude soft deleted entities by default
            entity.HasQueryFilter(e => !e.IsDeleted);
        }

        /// <summary>
        /// Configure entity-specific properties. Must be implemented by derived classes.
        /// </summary>
        /// <param name="entity">Entity type entity</param>
        protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> entity);
    }
}
