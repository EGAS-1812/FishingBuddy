using FishingBuddy.Data;
using FishingBuddy.Models;
using FishingBuddy.Repositories;
using FishingBuddy.Services.Ai;
using FishingBuddy.Services.Search;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

public partial class FBuddy
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        builder.Services.AddDbContext<FishingBuddyDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("FishingBuddyDb")));

        builder.Services
            .AddDefaultIdentity<AppUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<FishingBuddyDbContext>();

        var authenticationBuilder = builder.Services.AddAuthentication();
        var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
        var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        var facebookAppId = builder.Configuration["Authentication:Facebook:AppId"];
        var facebookAppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];

        if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
        {
            authenticationBuilder.AddGoogle(options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
            });
        }

        if (!string.IsNullOrWhiteSpace(facebookAppId) && !string.IsNullOrWhiteSpace(facebookAppSecret))
        {
            authenticationBuilder.AddFacebook(options =>
            {
                options.AppId = facebookAppId;
                options.AppSecret = facebookAppSecret;
            });
        }

        builder.Services.AddScoped<IFishingRepository, EfFishingRepository>();
        builder.Services.AddScoped<IGlobalSearchService, GlobalSearchService>();
        builder.Services.AddHttpClient<IAiFishDraftService, AiFishDraftService>();
        builder.Services.AddSingleton<IEmailSender, FishingBuddy.Data.NoOpEmailSender>();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseSerilogRequestLogging();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<FishingBuddyDbContext>();
            if (dbContext.Database.IsRelational())
            {
                dbContext.Database.Migrate();
            }
            else
            {
                dbContext.Database.EnsureCreated();
            }

            if (builder.Configuration.GetValue("SeedData", true))
            {
                DbInitializer.Seed(dbContext);
            }

            IdentitySeeder.SeedRoles(scope.ServiceProvider).GetAwaiter().GetResult();
        }

        app.MapControllerRoute(
            name: "fish-catalog",
            pattern: "catalog/fish/{action=Index}/{id?}",
            defaults: new { controller = "Fish" });

        app.MapControllerRoute(
            name: "bait-catalog",
            pattern: "catalog/baits/{action=Index}/{id?}",
            defaults: new { controller = "Bait" });

        app.MapControllerRoute(
            name: "angler-hub",
            pattern: "community/anglers/{action=Index}/{id?}",
            defaults: new { controller = "User" });

        app.MapControllerRoute(
            name: "spot-guide",
            pattern: "destinations/spots/{action=Index}/{id?}",
            defaults: new { controller = "FishingSpot" });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();

        app.Run();
    }
}
