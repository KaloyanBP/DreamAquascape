using Microsoft.EntityFrameworkCore;
using DreamAquascape.Data.Models;
using DreamAquascape.Data.Seeding.Interfaces;

namespace DreamAquascape.Data.Seeding
{
    public class CategoriesSeeder : ICategoriesSeeder
    {
        private readonly ApplicationDbContext context;

        public CategoriesSeeder(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task SeedCategoriesAsync()
        {
            // Check if categories already exist
            if (await context.ContestCategories.AnyAsync())
            {
                return; // Categories already seeded
            }

            var categories = new List<ContestCategory>
            {
                new ContestCategory
                {
                    Name = "Nano Tank",
                    Description = "Small tanks under 30 liters",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Nature Style",
                    Description = "Inspired by natural landscapes",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Iwagumi",
                    Description = "Minimalist rock-focused layout",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Dutch Style",
                    Description = "Dense, colorful plant arrangements",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Biotope",
                    Description = "Mimics a real-world ecosystem",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Beginner",
                    Description = "For new aquascapers",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Hardscape Only",
                    Description = "Focus on rocks and wood, no plants",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Themed Tank",
                    Description = "Creative or fantasy-inspired setups",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Paludarium",
                    Description = "Combo of water and land elements",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Large Tank",
                    Description = "Over 200 liters of aquascaping",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Low Tech",
                    Description = "No CO2 or high-tech equipment needed",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Jungle Style",
                    Description = "Wild, overgrown plant arrangements",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.ContestCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}
