namespace DreamAquascape.Data.Models
{
    /// <summary>
    /// Base entity class providing simple soft deletion capabilities.
    /// </summary>
    public class SoftDeletableEntity
    {
        /// <summary>
        /// Timestamp when the entity was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// User ID who created the entity
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty; // User ID or username

        /// <summary>
        /// Timestamp when the entity was updated.
        /// Null if the entity is not updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// User ID who last updated the entity (optional).
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Indicates whether the entity is soft deleted.
        /// Soft deleted entities are not physically removed from the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp when the entity was soft deleted.
        /// Null if the entity is not deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// User ID who performed the soft deletion (optional).
        /// Useful for audit trails and accountability.
        /// </summary>
        public string? DeletedBy { get; set; }
    }
}
