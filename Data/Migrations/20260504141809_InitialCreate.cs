using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FishingBuddy.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Baits",
                columns: table => new
                {
                    BaitID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BaitName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BaitType = table.Column<int>(type: "INTEGER", nullable: false),
                    PreparationMethod = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AveragePriceEur = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Baits", x => x.BaitID);
                });

            migrationBuilder.CreateTable(
                name: "FishingSpots",
                columns: table => new
                {
                    SpotID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SpotName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Region = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    HasPiers = table.Column<bool>(type: "INTEGER", nullable: false),
                    BoatAccess = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FishingSpots", x => x.SpotID);
                });

            migrationBuilder.CreateTable(
                name: "Techniques",
                columns: table => new
                {
                    TechniqueID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TechniqueName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    PerformanceNote = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TutorialUrl = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Techniques", x => x.TechniqueID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Fish",
                columns: table => new
                {
                    FishID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SpeciesName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    CatchSeason = table.Column<int>(type: "INTEGER", nullable: false),
                    FavouriteBaitID = table.Column<int>(type: "INTEGER", nullable: false),
                    FleshColor = table.Column<int>(type: "INTEGER", nullable: false),
                    PreferredMethodID = table.Column<int>(type: "INTEGER", nullable: false),
                    Equipment_FReel_Size = table.Column<int>(type: "INTEGER", nullable: false),
                    Equipment_FReel_Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Equipment_FRod_LengthMeters = table.Column<decimal>(type: "TEXT", nullable: false),
                    Equipment_FRod_Action = table.Column<int>(type: "INTEGER", nullable: false),
                    Equipment_FRod_MinWeightRatingGrams = table.Column<int>(type: "INTEGER", nullable: false),
                    Equipment_FRod_MaxWeightRatingGrams = table.Column<int>(type: "INTEGER", nullable: false),
                    Equipment_FLine_Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Equipment_FLine_ThicknessMm = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fish", x => x.FishID);
                    table.ForeignKey(
                        name: "FK_Fish_Baits_FavouriteBaitID",
                        column: x => x.FavouriteBaitID,
                        principalTable: "Baits",
                        principalColumn: "BaitID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fish_Techniques_PreferredMethodID",
                        column: x => x.PreferredMethodID,
                        principalTable: "Techniques",
                        principalColumn: "TechniqueID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FishingLicenses",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "INTEGER", nullable: false),
                    BeginDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FishingLicenses", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_FishingLicenses_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatchRecords",
                columns: table => new
                {
                    CatchID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserID = table.Column<int>(type: "INTEGER", nullable: false),
                    FishID = table.Column<int>(type: "INTEGER", nullable: false),
                    CatchDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Weight = table.Column<double>(type: "REAL", nullable: false),
                    LengthCm = table.Column<double>(type: "REAL", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatchRecords", x => x.CatchID);
                    table.ForeignKey(
                        name: "FK_CatchRecords_Fish_FishID",
                        column: x => x.FishID,
                        principalTable: "Fish",
                        principalColumn: "FishID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatchRecords_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FishingSpotFish",
                columns: table => new
                {
                    MostLikelyCatchFishID = table.Column<int>(type: "INTEGER", nullable: false),
                    PossibleSpotsSpotID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FishingSpotFish", x => new { x.MostLikelyCatchFishID, x.PossibleSpotsSpotID });
                    table.ForeignKey(
                        name: "FK_FishingSpotFish_Fish_MostLikelyCatchFishID",
                        column: x => x.MostLikelyCatchFishID,
                        principalTable: "Fish",
                        principalColumn: "FishID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FishingSpotFish_FishingSpots_PossibleSpotsSpotID",
                        column: x => x.PossibleSpotsSpotID,
                        principalTable: "FishingSpots",
                        principalColumn: "SpotID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFavoriteFish",
                columns: table => new
                {
                    FavoriteFishFishID = table.Column<int>(type: "INTEGER", nullable: false),
                    FavoritedByUsersUserID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoriteFish", x => new { x.FavoriteFishFishID, x.FavoritedByUsersUserID });
                    table.ForeignKey(
                        name: "FK_UserFavoriteFish_Fish_FavoriteFishFishID",
                        column: x => x.FavoriteFishFishID,
                        principalTable: "Fish",
                        principalColumn: "FishID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavoriteFish_Users_FavoritedByUsersUserID",
                        column: x => x.FavoritedByUsersUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatchRecords_FishID",
                table: "CatchRecords",
                column: "FishID");

            migrationBuilder.CreateIndex(
                name: "IX_CatchRecords_UserID",
                table: "CatchRecords",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Fish_FavouriteBaitID",
                table: "Fish",
                column: "FavouriteBaitID");

            migrationBuilder.CreateIndex(
                name: "IX_Fish_PreferredMethodID",
                table: "Fish",
                column: "PreferredMethodID");

            migrationBuilder.CreateIndex(
                name: "IX_FishingSpotFish_PossibleSpotsSpotID",
                table: "FishingSpotFish",
                column: "PossibleSpotsSpotID");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteFish_FavoritedByUsersUserID",
                table: "UserFavoriteFish",
                column: "FavoritedByUsersUserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatchRecords");

            migrationBuilder.DropTable(
                name: "FishingLicenses");

            migrationBuilder.DropTable(
                name: "FishingSpotFish");

            migrationBuilder.DropTable(
                name: "UserFavoriteFish");

            migrationBuilder.DropTable(
                name: "FishingSpots");

            migrationBuilder.DropTable(
                name: "Fish");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Baits");

            migrationBuilder.DropTable(
                name: "Techniques");
        }
    }
}
