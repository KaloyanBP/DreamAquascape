namespace DreamAquascape.Web.Infrastructure.Extensions
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    using Data.Seeding.Interfaces;
    using Middlewares;

    public static class WebApplicationExtensions
    {
        public static IApplicationBuilder UserAdminRedirection(this IApplicationBuilder app)
        {
            app.UseMiddleware<AdminRedirectionMiddleware>();

            return app;
        }

        public static IApplicationBuilder SeedDefaultIdentity(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            IServiceProvider serviceProvider = scope.ServiceProvider;

            IIdentitySeeder identitySeeder = serviceProvider
                .GetRequiredService<IIdentitySeeder>();
            identitySeeder
                .SeedIdentityAsync()
                .GetAwaiter()
                .GetResult();

            return app;
        }

        public static IApplicationBuilder SeedDefaultCategories(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            IServiceProvider serviceProvider = scope.ServiceProvider;

            ICategoriesSeeder identitySeeder = serviceProvider
                .GetRequiredService<ICategoriesSeeder>();
            identitySeeder
                .SeedCategoriesAsync()
                .GetAwaiter()
                .GetResult();

            return app;
        }
    }
}
