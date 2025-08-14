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
            if (await context.ContestCategories.AnyAsync())
            {
                return;
            }

            var categories = new List<ContestCategory>
            {
                new ContestCategory
                {
                    Name = "Nano Aquascapes",
                    Description = "Small aquascapes in tanks under 30 liters. Perfect for desktop setups and intimate aquatic landscapes.",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Planted Tanks",
                    Description = "Aquascapes featuring live aquatic plants as the primary focus. Includes all planting styles and techniques.",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Nature Style",
                    Description = "Aquascapes inspired by natural landscapes, following the principles of Takashi Amano and natural aquascaping.",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Dutch Style",
                    Description = "Traditional Dutch aquascaping with terraced plant arrangements, vibrant colors, and meticulous plant management.",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Iwagumi",
                    Description = "Minimalist aquascaping style focusing on stone arrangements with simple plant compositions.",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Biotope",
                    Description = "Aquascapes replicating specific natural aquatic environments with region-appropriate plants and hardscape.",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Hardscape Only",
                    Description = "Aquascapes featuring only rocks, driftwood, and substrate without live plants. Focus on composition and layout.",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Low-Tech",
                    Description = "Aquascapes maintained without CO2 injection or high-intensity lighting. Emphasis on simple, sustainable setups.",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "High-Tech",
                    Description = "Advanced aquascapes with CO2 injection, high-intensity lighting, and sophisticated filtration systems.",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Aquatic Gardens",
                    Description = "Large-scale planted aquariums resembling underwater gardens with diverse plant species and complex layouts.",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Paludarium",
                    Description = "Half-aquatic, half-terrestrial setups combining underwater and above-water plant and hardscape elements.",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Minimalist",
                    Description = "Simple, clean aquascapes with few elements. Focus on negative space, simplicity, and elegant composition.",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Jungle Style",
                    Description = "Dense, wild-looking aquascapes with abundant plant growth creating a lush, overgrown appearance.",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Beginner Friendly",
                    Description = "Aquascapes designed for newcomers to the hobby, featuring easy-to-care-for plants and simple maintenance.",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new ContestCategory
                {
                    Name = "Innovative Design",
                    Description = "Creative and experimental aquascapes pushing the boundaries of traditional aquascaping concepts.",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.ContestCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}
