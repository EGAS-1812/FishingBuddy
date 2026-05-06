using FishingBuddy.Data;
using FishingBuddy.Repositories;
using Microsoft.EntityFrameworkCore;

internal class FBuddy
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();
        builder.Services.AddDbContext<FishingBuddyDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("FishingBuddyDb")));
        builder.Services.AddScoped<IFishingRepository, EfFishingRepository>();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<FishingBuddyDbContext>();
            dbContext.Database.Migrate();
            DbInitializer.Seed(dbContext);
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

        app.Run();
    }
}
