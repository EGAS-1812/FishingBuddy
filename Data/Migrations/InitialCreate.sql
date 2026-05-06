CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Baits" (
    "BaitID" INTEGER NOT NULL CONSTRAINT "PK_Baits" PRIMARY KEY AUTOINCREMENT,
    "BaitName" TEXT NOT NULL,
    "BaitType" INTEGER NOT NULL,
    "PreparationMethod" TEXT NOT NULL,
    "AveragePriceEur" TEXT NOT NULL
);

CREATE TABLE "FishingSpots" (
    "SpotID" INTEGER NOT NULL CONSTRAINT "PK_FishingSpots" PRIMARY KEY AUTOINCREMENT,
    "SpotName" TEXT NOT NULL,
    "Region" TEXT NOT NULL,
    "HasPiers" INTEGER NOT NULL,
    "BoatAccess" INTEGER NOT NULL
);

CREATE TABLE "Techniques" (
    "TechniqueID" INTEGER NOT NULL CONSTRAINT "PK_Techniques" PRIMARY KEY AUTOINCREMENT,
    "TechniqueName" TEXT NOT NULL,
    "PerformanceNote" TEXT NOT NULL,
    "TutorialUrl" TEXT NOT NULL
);

CREATE TABLE "Users" (
    "UserID" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "Username" TEXT NOT NULL,
    "Email" TEXT NOT NULL
);

CREATE TABLE "Fish" (
    "FishID" INTEGER NOT NULL CONSTRAINT "PK_Fish" PRIMARY KEY AUTOINCREMENT,
    "SpeciesName" TEXT NOT NULL,
    "CatchSeason" INTEGER NOT NULL,
    "FavouriteBaitID" INTEGER NOT NULL,
    "FleshColor" INTEGER NOT NULL,
    "PreferredMethodID" INTEGER NOT NULL,
    "Equipment_FReel_Size" INTEGER NOT NULL,
    "Equipment_FReel_Type" INTEGER NOT NULL,
    "Equipment_FRod_LengthMeters" TEXT NOT NULL,
    "Equipment_FRod_Action" INTEGER NOT NULL,
    "Equipment_FRod_MinWeightRatingGrams" INTEGER NOT NULL,
    "Equipment_FRod_MaxWeightRatingGrams" INTEGER NOT NULL,
    "Equipment_FLine_Type" INTEGER NOT NULL,
    "Equipment_FLine_ThicknessMm" TEXT NOT NULL,
    CONSTRAINT "FK_Fish_Baits_FavouriteBaitID" FOREIGN KEY ("FavouriteBaitID") REFERENCES "Baits" ("BaitID") ON DELETE RESTRICT,
    CONSTRAINT "FK_Fish_Techniques_PreferredMethodID" FOREIGN KEY ("PreferredMethodID") REFERENCES "Techniques" ("TechniqueID") ON DELETE RESTRICT
);

CREATE TABLE "FishingLicenses" (
    "UserID" INTEGER NOT NULL CONSTRAINT "PK_FishingLicenses" PRIMARY KEY,
    "BeginDate" TEXT NOT NULL,
    "ExpirationDate" TEXT NOT NULL,
    CONSTRAINT "FK_FishingLicenses_Users_UserID" FOREIGN KEY ("UserID") REFERENCES "Users" ("UserID") ON DELETE CASCADE
);

CREATE TABLE "CatchRecords" (
    "CatchID" INTEGER NOT NULL CONSTRAINT "PK_CatchRecords" PRIMARY KEY AUTOINCREMENT,
    "UserID" INTEGER NOT NULL,
    "FishID" INTEGER NOT NULL,
    "CatchDate" TEXT NOT NULL,
    "Weight" REAL NOT NULL,
    "LengthCm" REAL NOT NULL,
    "Location" TEXT NOT NULL,
    CONSTRAINT "FK_CatchRecords_Fish_FishID" FOREIGN KEY ("FishID") REFERENCES "Fish" ("FishID") ON DELETE CASCADE,
    CONSTRAINT "FK_CatchRecords_Users_UserID" FOREIGN KEY ("UserID") REFERENCES "Users" ("UserID") ON DELETE CASCADE
);

CREATE TABLE "FishingSpotFish" (
    "MostLikelyCatchFishID" INTEGER NOT NULL,
    "PossibleSpotsSpotID" INTEGER NOT NULL,
    CONSTRAINT "PK_FishingSpotFish" PRIMARY KEY ("MostLikelyCatchFishID", "PossibleSpotsSpotID"),
    CONSTRAINT "FK_FishingSpotFish_Fish_MostLikelyCatchFishID" FOREIGN KEY ("MostLikelyCatchFishID") REFERENCES "Fish" ("FishID") ON DELETE CASCADE,
    CONSTRAINT "FK_FishingSpotFish_FishingSpots_PossibleSpotsSpotID" FOREIGN KEY ("PossibleSpotsSpotID") REFERENCES "FishingSpots" ("SpotID") ON DELETE CASCADE
);

CREATE TABLE "UserFavoriteFish" (
    "FavoriteFishFishID" INTEGER NOT NULL,
    "FavoritedByUsersUserID" INTEGER NOT NULL,
    CONSTRAINT "PK_UserFavoriteFish" PRIMARY KEY ("FavoriteFishFishID", "FavoritedByUsersUserID"),
    CONSTRAINT "FK_UserFavoriteFish_Fish_FavoriteFishFishID" FOREIGN KEY ("FavoriteFishFishID") REFERENCES "Fish" ("FishID") ON DELETE CASCADE,
    CONSTRAINT "FK_UserFavoriteFish_Users_FavoritedByUsersUserID" FOREIGN KEY ("FavoritedByUsersUserID") REFERENCES "Users" ("UserID") ON DELETE CASCADE
);

CREATE INDEX "IX_CatchRecords_FishID" ON "CatchRecords" ("FishID");

CREATE INDEX "IX_CatchRecords_UserID" ON "CatchRecords" ("UserID");

CREATE INDEX "IX_Fish_FavouriteBaitID" ON "Fish" ("FavouriteBaitID");

CREATE INDEX "IX_Fish_PreferredMethodID" ON "Fish" ("PreferredMethodID");

CREATE INDEX "IX_FishingSpotFish_PossibleSpotsSpotID" ON "FishingSpotFish" ("PossibleSpotsSpotID");

CREATE INDEX "IX_UserFavoriteFish_FavoritedByUsersUserID" ON "UserFavoriteFish" ("FavoritedByUsersUserID");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260504141809_InitialCreate', '9.0.4');

COMMIT;

