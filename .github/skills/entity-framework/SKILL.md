---
name: entity-framework
description: "Use when: changing EF entity classes, DbContext mappings, navigation properties, data annotations, repositories that use EF, generating migrations, updating the database, or reviewing relationship configuration. Trigger phrases: EF, Entity Framework, DbContext, migration, foreign key, navigation property, data annotation, relationship mapping, database update."
---

# Entity Framework Skill

Use this skill when working on Entity Framework Core code in FishingBuddy.

## Scope

- Update model classes for EF compatibility.
- Adjust one-to-one, one-to-many, many-to-many, and owned-type mappings.
- Change `Data/FishingBuddyDbContext.cs`.
- Update EF-backed repository code in `Repositories/EfFishingRepository.cs`.
- Generate migrations and SQL scripts.
- Apply migrations to the configured SQLite database.

## Workflow

1. Start from the concrete EF surface being changed: model class, `DbContext`, migration, or repository method.
2. Read the related navigation properties and the matching `OnModelCreating` rules before editing.
3. Make the smallest schema/model change needed.
4. If the schema changes, generate a new migration.
5. Validate with `dotnet build` and, when needed, `dotnet tool run dotnet-ef database update`.

## Repo-Specific Notes

- Database provider: SQLite.
- Connection string key: `FishingBuddyDb`.
- `Technique`, `Bait`, `Fish`, `CatchRecord`, `User`, `FishingLicense`, and `FishingSpot` are the core entities.
- The equipment graph on `Fish` is mapped as EF owned types.
- Many-to-many joins are configured for user favorite fish and fishing spot fish.

## Common Commands

```powershell
dotnet build
dotnet tool run dotnet-ef migrations add <MigrationName> --output-dir Data/Migrations
dotnet tool run dotnet-ef migrations script -o Data/Migrations/<MigrationName>.sql
dotnet tool run dotnet-ef database update
```

## Example In This Repo

- EF mapping: `Data/FishingBuddyDbContext.cs`
- EF repository: `Repositories/EfFishingRepository.cs`
- Migration output: `Data/Migrations/`