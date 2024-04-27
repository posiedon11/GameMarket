﻿// <auto-generated />
using System;
using GameMarketAPIServer.Models.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GameMarketAPIServer.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20240427075554_TestMigration")]
    partial class TestMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+GameMarketSchema+DeveloperTable", b =>
                {
                    b.Property<uint>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<uint>("ID"));

                    b.Property<string>("developer")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<uint>("gameID")
                        .HasColumnType("int unsigned");

                    b.HasKey("ID");

                    b.HasIndex("gameID", "developer")
                        .IsUnique();

                    b.ToTable("GameMarket_Developer", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+GameMarketSchema+GameTitleTable", b =>
                {
                    b.Property<uint>("gameID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<uint>("gameID"));

                    b.Property<string>("gameTitle")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.HasKey("gameID");

                    b.ToTable("GameMarket_GameTitle", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+GameMarketSchema+PublisherTable", b =>
                {
                    b.Property<uint>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<uint>("ID"));

                    b.Property<uint>("gameID")
                        .HasColumnType("int unsigned");

                    b.Property<string>("publisher")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.HasKey("ID");

                    b.HasIndex("gameID", "publisher")
                        .IsUnique();

                    b.ToTable("GameMarket_Publisher", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+GameMarketSchema+SteamLinkTable", b =>
                {
                    b.Property<uint>("gameID")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("appID")
                        .HasMaxLength(15)
                        .HasColumnType("int unsigned");

                    b.HasKey("gameID");

                    b.HasIndex("appID")
                        .IsUnique();

                    b.ToTable("GameMarket_SteamLink", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+GameMarketSchema+XboxLinkTable", b =>
                {
                    b.Property<string>("modernTitleID")
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.Property<uint>("gameID")
                        .HasColumnType("int unsigned");

                    b.HasKey("modernTitleID");

                    b.HasIndex("gameID");

                    b.ToTable("GameMarket_XboxLink", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppDLCTable", b =>
                {
                    b.Property<uint>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<uint>("ID"));

                    b.Property<uint>("appID")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("dlcID")
                        .HasColumnType("int unsigned");

                    b.HasKey("ID");

                    b.HasIndex("dlcID")
                        .IsUnique();

                    b.HasIndex("appID", "dlcID")
                        .IsUnique();

                    b.ToTable("Steam_AppDLC", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppDetailsTable", b =>
                {
                    b.Property<uint>("appID")
                        .HasColumnType("int unsigned");

                    b.Property<string>("appName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("appType")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.Property<bool>("isFree")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.Property<DateTime>("lastScanned")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValue(new DateTime(2024, 4, 27, 7, 55, 54, 737, DateTimeKind.Utc).AddTicks(1515));

                    b.Property<double?>("listPrice")
                        .HasColumnType("double");

                    b.Property<double?>("msrp")
                        .HasColumnType("double");

                    b.HasKey("appID");

                    b.ToTable("Steam_AppDetails", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppDevelopersTable", b =>
                {
                    b.Property<uint>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<uint>("ID"));

                    b.Property<uint>("appID")
                        .HasColumnType("int unsigned");

                    b.Property<string>("developer")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.HasKey("ID");

                    b.HasIndex("appID", "developer")
                        .IsUnique();

                    b.ToTable("Steam_AppDevelopers", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppGenresTable", b =>
                {
                    b.Property<uint>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<uint>("ID"));

                    b.Property<uint>("appID")
                        .HasColumnType("int unsigned");

                    b.Property<string>("genre")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.HasKey("ID");

                    b.HasIndex("appID", "genre")
                        .IsUnique();

                    b.ToTable("Steam_AppGenres", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppIDsTable", b =>
                {
                    b.Property<uint>("appID")
                        .HasColumnType("int unsigned");

                    b.Property<DateTime?>("lastScanned")
                        .HasColumnType("datetime(6)");

                    b.HasKey("appID");

                    b.ToTable("Steam_AppIDs", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppPlatformsTable", b =>
                {
                    b.Property<uint>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<uint>("ID"));

                    b.Property<uint>("appID")
                        .HasColumnType("int unsigned");

                    b.Property<string>("platform")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.HasKey("ID");

                    b.HasIndex("appID", "platform")
                        .IsUnique();

                    b.ToTable("Steam_AppPlatforms", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppPublishersTable", b =>
                {
                    b.Property<uint>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<uint>("ID"));

                    b.Property<uint>("appID")
                        .HasColumnType("int unsigned");

                    b.Property<string>("publisher")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.HasKey("ID");

                    b.HasIndex("appID", "publisher")
                        .IsUnique();

                    b.ToTable("Steam_AppPublishers", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+PackageDetailsTable", b =>
                {
                    b.Property<uint>("packageID")
                        .HasColumnType("int unsigned");

                    b.Property<DateTime>("lastScanned")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValue(new DateTime(2024, 4, 27, 7, 55, 54, 740, DateTimeKind.Utc).AddTicks(185));

                    b.Property<double>("listPrice")
                        .HasColumnType("double");

                    b.Property<double>("msrp")
                        .HasColumnType("double");

                    b.Property<string>("packageName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.HasKey("packageID");

                    b.ToTable("Steam_PackageDetails", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+PackageIDsTable", b =>
                {
                    b.Property<uint>("packageID")
                        .HasColumnType("int unsigned");

                    b.Property<DateTime?>("lastScanned")
                        .HasColumnType("datetime(6)");

                    b.HasKey("packageID");

                    b.ToTable("Steam_PackageIDs", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+PackagesTable", b =>
                {
                    b.Property<uint>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<uint>("ID"));

                    b.Property<uint>("appID")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("packageID")
                        .HasColumnType("int unsigned");

                    b.HasKey("ID");

                    b.HasIndex("packageID");

                    b.HasIndex("appID", "packageID")
                        .IsUnique();

                    b.ToTable("Steam_Packages", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+GameBundleTable", b =>
                {
                    b.Property<string>("relatedProductID")
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.Property<string>("productID")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.HasKey("relatedProductID");

                    b.HasIndex("productID");

                    b.ToTable("Xbox_GameBundle", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+GameGenreTable", b =>
                {
                    b.Property<uint>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<uint>("ID"));

                    b.Property<string>("genre")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.Property<string>("titleID")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.HasKey("ID");

                    b.HasIndex("titleID", "genre")
                        .IsUnique();

                    b.ToTable("Xbox_GameGenre", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+GameTitleTable", b =>
                {
                    b.Property<string>("modernTitleID")
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.Property<string>("displayImage")
                        .HasMaxLength(600)
                        .HasColumnType("varchar(600)");

                    b.Property<string>("groupID")
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.Property<bool>("isGamePass")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime?>("lastScanned")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("titleID")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.Property<string>("titleName")
                        .IsRequired()
                        .HasMaxLength(130)
                        .HasColumnType("varchar(130)");

                    b.HasKey("modernTitleID");

                    b.ToTable("Xbox_GameTitle", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+GroupDataTable", b =>
                {
                    b.Property<string>("groupID")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("groupName")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("varchar(150)");

                    b.HasKey("groupID");

                    b.ToTable("Xbox_GroupData", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+MarketDetailTable", b =>
                {
                    b.Property<string>("productID")
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.Property<string>("currencyCode")
                        .HasMaxLength(5)
                        .HasColumnType("varchar(5)");

                    b.Property<string>("developerName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<DateTime>("endDate")
                        .HasMaxLength(15)
                        .HasColumnType("datetime(6)");

                    b.Property<double?>("listPrice")
                        .HasColumnType("double");

                    b.Property<double?>("msrp")
                        .HasColumnType("double");

                    b.Property<string>("posterImage")
                        .HasMaxLength(350)
                        .HasColumnType("varchar(350)");

                    b.Property<string>("productTitle")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("varchar(300)");

                    b.Property<string>("publisherName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<bool>("purchasable")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(true);

                    b.Property<DateTime>("releaseDate")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("startDate")
                        .HasMaxLength(15)
                        .HasColumnType("datetime(6)");

                    b.HasKey("productID");

                    b.ToTable("Xbox_MarketDetail", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+ProductIDTable", b =>
                {
                    b.Property<string>("productID")
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.Property<DateTime?>("lastScanned")
                        .HasColumnType("datetime(6)");

                    b.HasKey("productID");

                    b.ToTable("Xbox_ProductID", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+ProductPlatformTable", b =>
                {
                    b.Property<uint>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<uint>("ID"));

                    b.Property<string>("platform")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("varchar(40)");

                    b.Property<string>("productID")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.HasKey("ID");

                    b.HasIndex("productID", "platform")
                        .IsUnique();

                    b.ToTable("Xbox_ProductPlatform", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+TitleDetailTable", b =>
                {
                    b.Property<string>("productID")
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.Property<string>("modernTitleID")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.HasKey("productID");

                    b.HasIndex("modernTitleID");

                    b.ToTable("Xbox_TitleDetail", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+TitleDeviceTable", b =>
                {
                    b.Property<uint>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<uint>("ID"));

                    b.Property<string>("device")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.Property<string>("modernTitleID")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.HasKey("ID");

                    b.HasIndex("modernTitleID", "device")
                        .IsUnique();

                    b.ToTable("Xbox_TitleDevice", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+UserProfileTable", b =>
                {
                    b.Property<string>("xuid")
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<string>("gamertag")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("varchar(16)");

                    b.Property<DateTime?>("lastScanned")
                        .HasColumnType("datetime(6)");

                    b.HasKey("xuid");

                    b.ToTable("Xbox_UserProfile", (string)null);
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+GameMarketSchema+DeveloperTable", b =>
                {
                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+GameMarketSchema+GameTitleTable", "GameTitle")
                        .WithMany("Developers")
                        .HasForeignKey("gameID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GameTitle");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+GameMarketSchema+PublisherTable", b =>
                {
                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+GameMarketSchema+GameTitleTable", "GameTitle")
                        .WithMany("Publishers")
                        .HasForeignKey("gameID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GameTitle");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+GameMarketSchema+SteamLinkTable", b =>
                {
                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppDetailsTable", "AppDetails")
                        .WithOne("GameMarketLink")
                        .HasForeignKey("GameMarketAPIServer.Models.DataBaseSchemas+GameMarketSchema+SteamLinkTable", "appID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+GameMarketSchema+GameTitleTable", "GameTitle")
                        .WithMany("SteamLinks")
                        .HasForeignKey("gameID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AppDetails");

                    b.Navigation("GameTitle");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+GameMarketSchema+XboxLinkTable", b =>
                {
                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+GameMarketSchema+GameTitleTable", "GameTitle")
                        .WithMany("XboxLinks")
                        .HasForeignKey("gameID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+GameTitleTable", "XboxTitle")
                        .WithOne("GameMarketLink")
                        .HasForeignKey("GameMarketAPIServer.Models.DataBaseSchemas+GameMarketSchema+XboxLinkTable", "modernTitleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GameTitle");

                    b.Navigation("XboxTitle");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppDLCTable", b =>
                {
                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppDetailsTable", "AppDetails")
                        .WithMany("DLCs")
                        .HasForeignKey("appID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppIDsTable", "AppIDs")
                        .WithOne("AppDLC")
                        .HasForeignKey("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppDLCTable", "dlcID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AppDetails");

                    b.Navigation("AppIDs");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppDetailsTable", b =>
                {
                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppIDsTable", "AppIDNavigation")
                        .WithOne("AppDetails")
                        .HasForeignKey("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppDetailsTable", "appID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AppIDNavigation");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppDevelopersTable", b =>
                {
                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppDetailsTable", "AppDetails")
                        .WithMany("Developers")
                        .HasForeignKey("appID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AppDetails");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppGenresTable", b =>
                {
                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppDetailsTable", "AppDetails")
                        .WithMany("Genres")
                        .HasForeignKey("appID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AppDetails");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppPlatformsTable", b =>
                {
                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppDetailsTable", "AppDetails")
                        .WithMany("Platforms")
                        .HasForeignKey("appID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AppDetails");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppPublishersTable", b =>
                {
                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppDetailsTable", "AppDetails")
                        .WithMany("Publishers")
                        .HasForeignKey("appID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AppDetails");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+PackageDetailsTable", b =>
                {
                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+PackageIDsTable", "packageIds")
                        .WithOne("PackageDetails")
                        .HasForeignKey("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+PackageDetailsTable", "packageID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("packageIds");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+PackagesTable", b =>
                {
                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppDetailsTable", "AppDetails")
                        .WithMany("Packeges")
                        .HasForeignKey("appID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+PackageIDsTable", "PackageIDs")
                        .WithMany("Packages")
                        .HasForeignKey("packageID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AppDetails");

                    b.Navigation("PackageIDs");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+GameBundleTable", b =>
                {
                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+TitleDetailTable", "TitleDetails")
                        .WithMany("GameBundles")
                        .HasForeignKey("productID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+ProductIDTable", "ProductIDNavig")
                        .WithMany("GameBundles")
                        .HasForeignKey("relatedProductID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ProductIDNavig");

                    b.Navigation("TitleDetails");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+MarketDetailTable", b =>
                {
                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+ProductIDTable", "ProductIDNavig")
                        .WithOne("MarketDetails")
                        .HasForeignKey("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+MarketDetailTable", "productID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ProductIDNavig");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+ProductPlatformTable", b =>
                {
                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+MarketDetailTable", "MarketDetail")
                        .WithMany("ProductPlatforms")
                        .HasForeignKey("productID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MarketDetail");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+TitleDetailTable", b =>
                {
                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+GameTitleTable", "GameTitle")
                        .WithMany("TitleDetails")
                        .HasForeignKey("modernTitleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+ProductIDTable", "ProductIDNavig")
                        .WithMany("TitleDetails")
                        .HasForeignKey("productID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GameTitle");

                    b.Navigation("ProductIDNavig");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+TitleDeviceTable", b =>
                {
                    b.HasOne("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+GameTitleTable", "GameTitle")
                        .WithMany("TitleDevices")
                        .HasForeignKey("modernTitleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GameTitle");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+GameMarketSchema+GameTitleTable", b =>
                {
                    b.Navigation("Developers");

                    b.Navigation("Publishers");

                    b.Navigation("SteamLinks");

                    b.Navigation("XboxLinks");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppDetailsTable", b =>
                {
                    b.Navigation("DLCs");

                    b.Navigation("Developers");

                    b.Navigation("GameMarketLink");

                    b.Navigation("Genres");

                    b.Navigation("Packeges");

                    b.Navigation("Platforms");

                    b.Navigation("Publishers");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+AppIDsTable", b =>
                {
                    b.Navigation("AppDLC");

                    b.Navigation("AppDetails");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+SteamSchema+PackageIDsTable", b =>
                {
                    b.Navigation("PackageDetails");

                    b.Navigation("Packages");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+GameTitleTable", b =>
                {
                    b.Navigation("GameMarketLink");

                    b.Navigation("TitleDetails");

                    b.Navigation("TitleDevices");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+MarketDetailTable", b =>
                {
                    b.Navigation("ProductPlatforms");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+ProductIDTable", b =>
                {
                    b.Navigation("GameBundles");

                    b.Navigation("MarketDetails");

                    b.Navigation("TitleDetails");
                });

            modelBuilder.Entity("GameMarketAPIServer.Models.DataBaseSchemas+XboxSchema+TitleDetailTable", b =>
                {
                    b.Navigation("GameBundles");
                });
#pragma warning restore 612, 618
        }
    }
}
