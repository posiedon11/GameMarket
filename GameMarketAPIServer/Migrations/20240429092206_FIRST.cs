using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameMarketAPIServer.Migrations
{
    /// <inheritdoc />
    public partial class FIRST : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GameMarket_GameTitle",
                columns: table => new
                {
                    gameID = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    gameTitle = table.Column<string>(type: "varchar(350)", maxLength: 350, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameMarket_GameTitle", x => x.gameID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Steam_AppIDs",
                columns: table => new
                {
                    appID = table.Column<uint>(type: "int unsigned", nullable: false),
                    lastScanned = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steam_AppIDs", x => x.appID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Steam_PackageIDs",
                columns: table => new
                {
                    packageID = table.Column<uint>(type: "int unsigned", nullable: false),
                    lastScanned = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steam_PackageIDs", x => x.packageID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Xbox_GameGenre",
                columns: table => new
                {
                    modernTitleID = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    genre = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Xbox_GameGenre", x => new { x.modernTitleID, x.genre });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Xbox_GameTitle",
                columns: table => new
                {
                    modernTitleID = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    titleID = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    titleName = table.Column<string>(type: "varchar(350)", maxLength: 350, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    displayImage = table.Column<string>(type: "varchar(600)", maxLength: 600, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    isGamePass = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    groupID = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lastScanned = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Xbox_GameTitle", x => x.modernTitleID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Xbox_GroupData",
                columns: table => new
                {
                    groupID = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    groupName = table.Column<string>(type: "varchar(350)", maxLength: 350, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Xbox_GroupData", x => x.groupID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Xbox_ProductID",
                columns: table => new
                {
                    productID = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lastScanned = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Xbox_ProductID", x => x.productID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Xbox_UserProfile",
                columns: table => new
                {
                    xuid = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gamertag = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lastScanned = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Xbox_UserProfile", x => x.xuid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GameMarket_Developer",
                columns: table => new
                {
                    gameID = table.Column<uint>(type: "int unsigned", nullable: false),
                    developer = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameMarket_Developer", x => new { x.gameID, x.developer });
                    table.ForeignKey(
                        name: "FK_GameMarket_Developer_GameMarket_GameTitle_gameID",
                        column: x => x.gameID,
                        principalTable: "GameMarket_GameTitle",
                        principalColumn: "gameID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GameMarket_Publisher",
                columns: table => new
                {
                    gameID = table.Column<uint>(type: "int unsigned", nullable: false),
                    publisher = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameMarket_Publisher", x => new { x.gameID, x.publisher });
                    table.ForeignKey(
                        name: "FK_GameMarket_Publisher_GameMarket_GameTitle_gameID",
                        column: x => x.gameID,
                        principalTable: "GameMarket_GameTitle",
                        principalColumn: "gameID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Steam_AppDetails",
                columns: table => new
                {
                    appID = table.Column<uint>(type: "int unsigned", nullable: false),
                    appType = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    appName = table.Column<string>(type: "varchar(350)", maxLength: 350, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    msrp = table.Column<double>(type: "double", nullable: true),
                    listPrice = table.Column<double>(type: "double", nullable: true),
                    isFree = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    lastScanned = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValue: new DateTime(2024, 4, 29, 9, 22, 6, 211, DateTimeKind.Utc).AddTicks(6219))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steam_AppDetails", x => x.appID);
                    table.ForeignKey(
                        name: "FK_Steam_AppDetails_Steam_AppIDs_appID",
                        column: x => x.appID,
                        principalTable: "Steam_AppIDs",
                        principalColumn: "appID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Steam_PackageDetails",
                columns: table => new
                {
                    packageID = table.Column<uint>(type: "int unsigned", nullable: false),
                    packageName = table.Column<string>(type: "varchar(350)", maxLength: 350, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    msrp = table.Column<double>(type: "double", nullable: false),
                    listPrice = table.Column<double>(type: "double", nullable: false),
                    lastScanned = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValue: new DateTime(2024, 4, 29, 9, 22, 6, 214, DateTimeKind.Utc).AddTicks(8425))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steam_PackageDetails", x => x.packageID);
                    table.ForeignKey(
                        name: "FK_Steam_PackageDetails_Steam_PackageIDs_packageID",
                        column: x => x.packageID,
                        principalTable: "Steam_PackageIDs",
                        principalColumn: "packageID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GameMarket_XboxLink",
                columns: table => new
                {
                    modernTitleID = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gameID = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameMarket_XboxLink", x => x.modernTitleID);
                    table.ForeignKey(
                        name: "FK_GameMarket_XboxLink_GameMarket_GameTitle_gameID",
                        column: x => x.gameID,
                        principalTable: "GameMarket_GameTitle",
                        principalColumn: "gameID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameMarket_XboxLink_Xbox_GameTitle_modernTitleID",
                        column: x => x.modernTitleID,
                        principalTable: "Xbox_GameTitle",
                        principalColumn: "modernTitleID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Xbox_TitleDevice",
                columns: table => new
                {
                    modernTitleID = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    device = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Xbox_TitleDevice", x => new { x.modernTitleID, x.device });
                    table.ForeignKey(
                        name: "FK_Xbox_TitleDevice_Xbox_GameTitle_modernTitleID",
                        column: x => x.modernTitleID,
                        principalTable: "Xbox_GameTitle",
                        principalColumn: "modernTitleID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Xbox_MarketDetail",
                columns: table => new
                {
                    productID = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    productTitle = table.Column<string>(type: "varchar(350)", maxLength: 350, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    developerName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    publisherName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    currencyCode = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    purchasable = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    posterImage = table.Column<string>(type: "varchar(600)", maxLength: 600, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    msrp = table.Column<double>(type: "double", nullable: true),
                    listPrice = table.Column<double>(type: "double", nullable: true),
                    releaseDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    startDate = table.Column<DateTime>(type: "datetime(6)", maxLength: 15, nullable: false),
                    endDate = table.Column<DateTime>(type: "datetime(6)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Xbox_MarketDetail", x => x.productID);
                    table.ForeignKey(
                        name: "FK_Xbox_MarketDetail_Xbox_ProductID_productID",
                        column: x => x.productID,
                        principalTable: "Xbox_ProductID",
                        principalColumn: "productID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Xbox_TitleDetail",
                columns: table => new
                {
                    productID = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    modernTitleID = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Xbox_TitleDetail", x => x.productID);
                    table.ForeignKey(
                        name: "FK_Xbox_TitleDetail_Xbox_GameTitle_modernTitleID",
                        column: x => x.modernTitleID,
                        principalTable: "Xbox_GameTitle",
                        principalColumn: "modernTitleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Xbox_TitleDetail_Xbox_ProductID_productID",
                        column: x => x.productID,
                        principalTable: "Xbox_ProductID",
                        principalColumn: "productID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GameMarket_SteamLink",
                columns: table => new
                {
                    gameID = table.Column<uint>(type: "int unsigned", nullable: false),
                    appID = table.Column<uint>(type: "int unsigned", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameMarket_SteamLink", x => x.gameID);
                    table.ForeignKey(
                        name: "FK_GameMarket_SteamLink_GameMarket_GameTitle_gameID",
                        column: x => x.gameID,
                        principalTable: "GameMarket_GameTitle",
                        principalColumn: "gameID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameMarket_SteamLink_Steam_AppDetails_appID",
                        column: x => x.appID,
                        principalTable: "Steam_AppDetails",
                        principalColumn: "appID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Steam_AppDevelopers",
                columns: table => new
                {
                    appID = table.Column<uint>(type: "int unsigned", nullable: false),
                    developer = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steam_AppDevelopers", x => new { x.appID, x.developer });
                    table.ForeignKey(
                        name: "FK_Steam_AppDevelopers_Steam_AppDetails_appID",
                        column: x => x.appID,
                        principalTable: "Steam_AppDetails",
                        principalColumn: "appID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Steam_AppDLC",
                columns: table => new
                {
                    appID = table.Column<uint>(type: "int unsigned", nullable: false),
                    dlcID = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steam_AppDLC", x => new { x.appID, x.dlcID });
                    table.ForeignKey(
                        name: "FK_Steam_AppDLC_Steam_AppDetails_appID",
                        column: x => x.appID,
                        principalTable: "Steam_AppDetails",
                        principalColumn: "appID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Steam_AppDLC_Steam_AppIDs_dlcID",
                        column: x => x.dlcID,
                        principalTable: "Steam_AppIDs",
                        principalColumn: "appID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Steam_AppGenres",
                columns: table => new
                {
                    appID = table.Column<uint>(type: "int unsigned", nullable: false),
                    genre = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steam_AppGenres", x => new { x.appID, x.genre });
                    table.ForeignKey(
                        name: "FK_Steam_AppGenres_Steam_AppDetails_appID",
                        column: x => x.appID,
                        principalTable: "Steam_AppDetails",
                        principalColumn: "appID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Steam_AppPlatforms",
                columns: table => new
                {
                    appID = table.Column<uint>(type: "int unsigned", nullable: false),
                    platform = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steam_AppPlatforms", x => new { x.appID, x.platform });
                    table.ForeignKey(
                        name: "FK_Steam_AppPlatforms_Steam_AppDetails_appID",
                        column: x => x.appID,
                        principalTable: "Steam_AppDetails",
                        principalColumn: "appID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Steam_AppPublishers",
                columns: table => new
                {
                    appID = table.Column<uint>(type: "int unsigned", nullable: false),
                    publisher = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steam_AppPublishers", x => new { x.appID, x.publisher });
                    table.ForeignKey(
                        name: "FK_Steam_AppPublishers_Steam_AppDetails_appID",
                        column: x => x.appID,
                        principalTable: "Steam_AppDetails",
                        principalColumn: "appID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Steam_Packages",
                columns: table => new
                {
                    appID = table.Column<uint>(type: "int unsigned", nullable: false),
                    packageID = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steam_Packages", x => new { x.appID, x.packageID });
                    table.ForeignKey(
                        name: "FK_Steam_Packages_Steam_AppDetails_appID",
                        column: x => x.appID,
                        principalTable: "Steam_AppDetails",
                        principalColumn: "appID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Steam_Packages_Steam_PackageIDs_packageID",
                        column: x => x.packageID,
                        principalTable: "Steam_PackageIDs",
                        principalColumn: "packageID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Xbox_ProductPlatform",
                columns: table => new
                {
                    productID = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    platform = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Xbox_ProductPlatform", x => new { x.productID, x.platform });
                    table.ForeignKey(
                        name: "FK_Xbox_ProductPlatform_Xbox_MarketDetail_productID",
                        column: x => x.productID,
                        principalTable: "Xbox_MarketDetail",
                        principalColumn: "productID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Xbox_GameBundle",
                columns: table => new
                {
                    relatedProductID = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    productID = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Xbox_GameBundle", x => new { x.productID, x.relatedProductID });
                    table.ForeignKey(
                        name: "FK_Xbox_GameBundle_Xbox_ProductID_relatedProductID",
                        column: x => x.relatedProductID,
                        principalTable: "Xbox_ProductID",
                        principalColumn: "productID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Xbox_GameBundle_Xbox_TitleDetail_productID",
                        column: x => x.productID,
                        principalTable: "Xbox_TitleDetail",
                        principalColumn: "productID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GameMarket_SteamLink_appID",
                table: "GameMarket_SteamLink",
                column: "appID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameMarket_XboxLink_gameID",
                table: "GameMarket_XboxLink",
                column: "gameID");

            migrationBuilder.CreateIndex(
                name: "IX_Steam_AppDLC_dlcID",
                table: "Steam_AppDLC",
                column: "dlcID");

            migrationBuilder.CreateIndex(
                name: "IX_Steam_Packages_packageID",
                table: "Steam_Packages",
                column: "packageID");

            migrationBuilder.CreateIndex(
                name: "IX_Xbox_GameBundle_relatedProductID",
                table: "Xbox_GameBundle",
                column: "relatedProductID");

            migrationBuilder.CreateIndex(
                name: "IX_Xbox_TitleDetail_modernTitleID",
                table: "Xbox_TitleDetail",
                column: "modernTitleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameMarket_Developer");

            migrationBuilder.DropTable(
                name: "GameMarket_Publisher");

            migrationBuilder.DropTable(
                name: "GameMarket_SteamLink");

            migrationBuilder.DropTable(
                name: "GameMarket_XboxLink");

            migrationBuilder.DropTable(
                name: "Steam_AppDevelopers");

            migrationBuilder.DropTable(
                name: "Steam_AppDLC");

            migrationBuilder.DropTable(
                name: "Steam_AppGenres");

            migrationBuilder.DropTable(
                name: "Steam_AppPlatforms");

            migrationBuilder.DropTable(
                name: "Steam_AppPublishers");

            migrationBuilder.DropTable(
                name: "Steam_PackageDetails");

            migrationBuilder.DropTable(
                name: "Steam_Packages");

            migrationBuilder.DropTable(
                name: "Xbox_GameBundle");

            migrationBuilder.DropTable(
                name: "Xbox_GameGenre");

            migrationBuilder.DropTable(
                name: "Xbox_GroupData");

            migrationBuilder.DropTable(
                name: "Xbox_ProductPlatform");

            migrationBuilder.DropTable(
                name: "Xbox_TitleDevice");

            migrationBuilder.DropTable(
                name: "Xbox_UserProfile");

            migrationBuilder.DropTable(
                name: "GameMarket_GameTitle");

            migrationBuilder.DropTable(
                name: "Steam_AppDetails");

            migrationBuilder.DropTable(
                name: "Steam_PackageIDs");

            migrationBuilder.DropTable(
                name: "Xbox_TitleDetail");

            migrationBuilder.DropTable(
                name: "Xbox_MarketDetail");

            migrationBuilder.DropTable(
                name: "Steam_AppIDs");

            migrationBuilder.DropTable(
                name: "Xbox_GameTitle");

            migrationBuilder.DropTable(
                name: "Xbox_ProductID");
        }
    }
}
