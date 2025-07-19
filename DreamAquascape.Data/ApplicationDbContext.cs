namespace DreamAquascape.Data
{
    using DreamAquascape.Data.Models;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using System.Reflection;

    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Contest> Contests { get; set; } = null!;

        public virtual DbSet<ContestEntry> ContestEntries { get; set; } = null!;

        public virtual DbSet<EntryImage> EntryImages { get; set; } = null!;

        public virtual DbSet<Vote> Votes { get; set; } = null!;

        public virtual DbSet<Prize> Prizes { get; set; } = null!;

        public virtual DbSet<UserContestParticipation> UserContestParticipations { get; set; } = null!;

        public virtual DbSet<ContestCategory> ContestCategories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
