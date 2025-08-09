using DreamAquascape.Data;
using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.Data.Seeding;
using DreamAquascape.Data.Seeding.Interfaces;
using DreamAquascape.Services.Core;
using DreamAquascape.Services.Core.AdminDashboard;
using DreamAquascape.Services.Core.Business.Permissions;
using DreamAquascape.Services.Core.Business.Rules;
using DreamAquascape.Services.Core.Infrastructure;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.Infrastructure.Extensions;
using DreamAquascape.GCommon.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DreamAquascape.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddRepositories(typeof(IContestRepository).Assembly);

            // Register Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddControllersWithViews();

            builder.Services.AddScoped<IContestService, ContestService>();
            builder.Services.AddScoped<IContestQueryService, ContestQueryService>();
            builder.Services.AddScoped<IContestEntryService, ContestEntryService>();
            builder.Services.AddScoped<IContestEntryQueryService, ContestEntryQueryService>();
            builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            builder.Services.AddScoped<IUserDashboardService, UserDashboardService>();

            // Register new abstraction services for better testability
            builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            builder.Services.AddScoped<IContestBusinessRules, ContestBusinessRules>();
            builder.Services.AddScoped<IContestPermissionService, ContestPermissionService>();

            // Register VotingService with all its dependencies
            builder.Services.AddScoped<IVotingService, VotingService>();

            builder.Services.AddTransient<IFileUploadService>(provider =>
            {
                var env = provider.GetRequiredService<IWebHostEnvironment>();
                var logger = provider.GetRequiredService<ILogger<FileUploadService>>();
                var dateTimeProvider = provider.GetRequiredService<IDateTimeProvider>();
                return new FileUploadService(env.WebRootPath, logger, dateTimeProvider);
            });

            // Add background service for automatic winner determination
            builder.Services.AddHostedService<WinnerDeterminationService>();

            builder.Services.AddTransient<IIdentitySeeder, IdentitySeeder>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.SeedDefaultIdentity();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
