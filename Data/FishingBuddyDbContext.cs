using FishingBuddy.Models;
using Microsoft.EntityFrameworkCore;

namespace FishingBuddy.Data;

public class FishingBuddyDbContext(DbContextOptions<FishingBuddyDbContext> options) : DbContext(options)
{
    public DbSet<Technique> Techniques => Set<Technique>();
    public DbSet<Bait> Baits => Set<Bait>();
    public DbSet<Fish> Fish => Set<Fish>();
    public DbSet<CatchRecord> CatchRecords => Set<CatchRecord>();
    public DbSet<FishingLicense> FishingLicenses => Set<FishingLicense>();
    public DbSet<User> Users => Set<User>();
    public DbSet<FishingSpot> FishingSpots => Set<FishingSpot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Fish>()
            .HasOne(f => f.PreferredMethod)
            .WithMany(t => t.FishUsingTechnique)
            .HasForeignKey(f => f.PreferredMethodID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Fish>()
            .HasOne(f => f.FavouriteBait)
            .WithMany(b => b.PreferredByFish)
            .HasForeignKey(f => f.FavouriteBaitID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CatchRecord>()
            .HasOne(cr => cr.User)
            .WithMany(u => u.CatchRecords)
            .HasForeignKey(cr => cr.UserID);

        modelBuilder.Entity<CatchRecord>()
            .HasOne(cr => cr.Fish)
            .WithMany(f => f.CatchRecords)
            .HasForeignKey(cr => cr.FishID);

        modelBuilder.Entity<User>()
            .HasOne(u => u.FishingLicense)
            .WithOne(fl => fl.User)
            .HasForeignKey<FishingLicense>(fl => fl.UserID);

        modelBuilder.Entity<User>()
            .HasMany(u => u.FavoriteFish)
            .WithMany(f => f.FavoritedByUsers)
            .UsingEntity(j => j.ToTable("UserFavoriteFish"));

        modelBuilder.Entity<FishingSpot>()
            .HasMany(s => s.MostLikelyCatch)
            .WithMany(f => f.PossibleSpots)
            .UsingEntity(j => j.ToTable("FishingSpotFish"));

        modelBuilder.Entity<Fish>(entity =>
        {
            entity.OwnsOne(f => f.Equipment, equipment =>
            {
                equipment.OwnsOne(e => e.FReel);
                equipment.OwnsOne(e => e.FRod);
                equipment.OwnsOne(e => e.FLine);
            });
        });
    }
}