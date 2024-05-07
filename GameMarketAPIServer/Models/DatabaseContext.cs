using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using GameMarketAPIServer.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.Infrastructure;
using static SteamKit2.Internal.CChatUsability_ClientUsabilityMetrics_Notification;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;
using static GameMarketAPIServer.Models.DataBaseSchemas;


namespace GameMarketAPIServer.Models.Contexts
{
    public class DatabaseContext : DbContext
    {
        private readonly string connectionString;
        private readonly ILogger<DatabaseContext> logger;
        private readonly MainSettings settings;
        private UInt16 xboxIDMaxLength = 15;
        private readonly UInt16 devPubmaxLength = 150;
        private readonly UInt16 gameTitleMaxLength = 350;
        private readonly UInt16 miscMaxLength = 80;

        #region Tables
        public DbSet<XboxSchema.GameTitleTable> xboxTitles { get; set; }
        public DbSet<XboxSchema.GameGenreTable> xboxGenres { get; set; }
        public DbSet<XboxSchema.GameBundleTable> xboxBundles { get; set; }
        public DbSet<XboxSchema.GroupDataTable> xboxGroups { get; set; }
        public DbSet<XboxSchema.MarketDetailTable> xboxMarketDetails { get; set; }
        public DbSet<XboxSchema.ProductIDTable> xboxProductIDs { get; set; }
        public DbSet<XboxSchema.ProductPlatformTable> xboxProductPlatforms { get; set; }
        public DbSet<XboxSchema.TitleDetailTable> xboxTitleDetails { get; set; }
        public DbSet<XboxSchema.TitleDeviceTable> xboxDevices { get; set; }
        public DbSet<XboxSchema.UserProfileTable> xboxUsers { get; set; }

        public DbSet<SteamSchema.AppDetailsTable> steamAppDetails { get; set; }
        public DbSet<SteamSchema.AppDevelopersTable> steamAppDevelopers { get; set; }
        public DbSet<SteamSchema.AppGenresTable> steamAppGenres { get; set; }
        public DbSet<SteamSchema.AppIDsTable> steamAppIDs { get; set; }
        public DbSet<SteamSchema.AppPlatformsTable> steamAppPlatforms { get; set; }
        public DbSet<SteamSchema.AppPublishersTable> steamAppPublishers { get; set; }
        public DbSet<SteamSchema.AppDLCTable> steamAppDLCs { get; set; }
        public DbSet<SteamSchema.PackageIDsTable> steamPackageIDs { get; set; }
        public DbSet<SteamSchema.PackageDetailsTable> steamPackageDetails { get; set; }
        public DbSet<SteamSchema.PackagesTable> steamPackages { get; set; }


        public DbSet<GameMarketSchema.DeveloperTable> gameMarketDevelopers { get; set; }
        public DbSet<GameMarketSchema.GameTitleTable> gameMarketTitles { get; set; }
        public DbSet<GameMarketSchema.PublisherTable> gameMarketPublishers { get; set; }
        public DbSet<GameMarketSchema.SteamLinkTable> gameMarketSteamLinks { get; set; }
        public DbSet<GameMarketSchema.XboxLinkTable> gameMarketXboxLinks { get; set; }

        #endregion

        public DatabaseContext(DbContextOptions<DatabaseContext> options, IOptions<MainSettings> settings, ILogger<DatabaseContext> logger) : base(options)
        {
            try
            {
                this.logger = logger;
                this.settings = settings.Value;
                if (settings.Value.sqlServerSettings.serverPassword == null || settings.Value.sqlServerSettings.serverPassword == "")
                {
                    throw new Exception("No password found in settings");
                }
                if (settings.Value.sqlServerSettings.serverUserName == null || settings.Value.sqlServerSettings.serverUserName == "")
                {
                    throw new Exception("No User Name found in settings");
                }
                this.connectionString = settings.Value.sqlServerSettings.getConnectionString();
                connectionString = settings.Value.sqlServerSettings.getConnectionString();
                logger.LogInformation(connectionString);
            }catch (Exception e)
            {
                logger.LogError(e, "\nError in DatabaseContext Constructor");
                throw;
            }

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var cc = settings.sqlServerSettings.getConnectionString();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(cc));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ApplySchema<XboxSchema>(modelBuilder, Xbox);

            ApplySchema<SteamSchema>(modelBuilder, Steam);

            ApplySchema<GameMarketSchema>(modelBuilder, GameMarket);

            ModelBuildXbox(modelBuilder);

            ModelBuildSteam(modelBuilder);

            ModelBuildGameMarket(modelBuilder);



        }

        private void ModelBuildXbox(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<XboxSchema.GameTitleTable>(entity =>
            {
                //Properties
                entity.Property(e => e.titleID).IsRequired(true).HasMaxLength(xboxIDMaxLength);
                entity.Property(e => e.titleName).IsRequired(true).HasMaxLength(gameTitleMaxLength);
                entity.Property(e => e.displayImage).IsRequired(false).HasMaxLength(600).HasDefaultValue(null);
                entity.Property(e => e.modernTitleID).IsRequired(true).HasMaxLength(xboxIDMaxLength);
                entity.Property(e => e.groupID).IsRequired(false).HasMaxLength(xboxIDMaxLength).HasDefaultValue(null);
                entity.Property(e => e.lastScanned).IsRequired(false).HasDefaultValue(null);

                //Primary Key
                entity.HasKey(e => e.modernTitleID);


                //Relationships
                //entity.HasMany(e => e.TitleDetails).WithOne(e => e.GameTitle).IsRequired(false).HasForeignKey(e => e.modernTitleID);
                //entity.HasMany(e => e.TitleDevices).WithOne(e => e.GameTitle).IsRequired(false).HasForeignKey(e => e.modernTitleID);
                entity.HasOne(e => e.GameMarketLink).WithOne(e => e.XboxTitle)
                .HasForeignKey<GameMarketSchema.XboxLinkTable>(e => e.modernTitleID).IsRequired(true);
            });


            modelBuilder.Entity<XboxSchema.TitleDeviceTable>(entity =>
            {
                //Properties

                entity.Property(e => e.modernTitleID).IsRequired(true).HasMaxLength(xboxIDMaxLength);
                entity.Property(e => e.device).IsRequired(true).HasMaxLength(30);

                //Primary Key
                entity.HasKey(e => new { e.modernTitleID, e.device });

                //Relationships
                entity.HasOne(e => e.GameTitle).WithMany(e => e.TitleDevices).HasForeignKey(e => e.modernTitleID).IsRequired(true);
            });


            modelBuilder.Entity<XboxSchema.TitleDetailTable>(entity =>
            {
                entity.Property(e => e.modernTitleID).IsRequired(true).HasMaxLength(xboxIDMaxLength);
                entity.Property(e => e.productID).IsRequired(true).HasMaxLength(xboxIDMaxLength);

                entity.HasKey(e => e.productID);

                //entity.HasMany(e=>e.GameBundles).WithOne(e=>e.TitleDetails).IsRequired(false).HasForeignKey(e=>e.productID);
                entity.HasOne(e => e.GameTitle).WithMany(e => e.TitleDetails).IsRequired(true).HasForeignKey(e => e.modernTitleID);
                entity.HasOne(e => e.ProductIDNavig).WithMany(e => e.TitleDetails).IsRequired(true).HasForeignKey(e => e.productID);
            });


            modelBuilder.Entity<XboxSchema.GameBundleTable>(entity =>
            {

                entity.Property(e => e.productID).IsRequired(true).HasMaxLength(xboxIDMaxLength);
                entity.Property(e => e.relatedProductID).IsRequired(true).HasMaxLength(xboxIDMaxLength);

                entity.HasKey(e => new { e.productID, e.relatedProductID });

                entity.HasOne(e => e.TitleDetails).WithMany(e => e.GameBundles).HasForeignKey(e => e.productID).IsRequired(true);
                entity.HasOne(e => e.ProductIDNavig).WithMany(e => e.GameBundles).HasForeignKey(e => e.relatedProductID).IsRequired(true);

            });


            modelBuilder.Entity<XboxSchema.ProductIDTable>(entity =>
            {
                entity.Property(e => e.productID).IsRequired(true).HasMaxLength(xboxIDMaxLength);
                entity.Property(e => e.lastScanned).IsRequired(false).HasDefaultValue(null);


                entity.HasKey(e => e.productID);
            });


            modelBuilder.Entity<XboxSchema.MarketDetailTable>(entity =>
            {

                entity.Property(e => e.productID).IsRequired(true).HasMaxLength(xboxIDMaxLength);
                entity.Property(e => e.productTitle).IsRequired(true).HasMaxLength(gameTitleMaxLength);
                entity.Property(e => e.developerName).IsRequired(false).HasMaxLength(devPubmaxLength).HasDefaultValue(null);
                entity.Property(e => e.publisherName).IsRequired(false).HasMaxLength(devPubmaxLength).HasDefaultValue(null);
                entity.Property(e => e.currencyCode).IsRequired(false).HasMaxLength(5).HasDefaultValue(null);
                entity.Property(e => e.purchasable).IsRequired(true).HasDefaultValue(true);
                entity.Property(e => e.posterImage).IsRequired(false).HasMaxLength(600).HasDefaultValue(null);
                entity.Property(e => e.msrp).IsRequired(false).HasDefaultValue(null);
                entity.Property(e => e.listPrice).IsRequired(false).HasDefaultValue(null);

                entity.Property(e => e.startDate).IsRequired(true).HasMaxLength(xboxIDMaxLength);
                entity.Property(e => e.endDate).IsRequired(true).HasMaxLength(xboxIDMaxLength);
                //string constraintString = $@"({entity.Property(e => e.purchasable)} is {true} and {entity.Property(e => e.msrp)} is not null and {entity.Property(e => e.listPrice)} is not null and {entity.Property(e => e.currencyCode)} is not null) or
                //          ({entity.Property(e => e.purchasable)} is {false} and {entity.Property(e => e.msrp)} is null and {entity.Property(e => e.listPrice)} is null and {entity.Property(e => e.currencyCode)} is null)\r\n)";


#if false
                entity.ToTable(e => e.HasCheckConstraint("CK_MarketDetail_PurchasableTrue",
                           "(purchasable is false or (msrp is not null and listPrice is not null and currencyCode is not null))"));
                entity.ToTable(e => e.HasCheckConstraint("CK_MarketDetail_PurchasableFalse",
                      "(purchasable is true or (msrp is null and listPrice is null and currencyCode is null))")); 
#endif



                //Primary Key
                entity.HasKey(e => e.productID);
                //Relationships
                entity.HasOne(e => e.ProductIDNavig).WithOne(e => e.MarketDetails).HasPrincipalKey<XboxSchema.ProductIDTable>(e => e.productID).HasForeignKey<XboxSchema.MarketDetailTable>(e => e.productID).IsRequired(true);
            });



            modelBuilder.Entity<XboxSchema.ProductPlatformTable>(entity =>
            {

                entity.Property(e => e.productID).IsRequired(true).HasMaxLength(xboxIDMaxLength);
                entity.Property(e => e.platform).IsRequired(true).HasMaxLength(miscMaxLength);

                entity.HasKey(e => new { e.productID, e.platform });


                entity.HasOne(e => e.MarketDetail).WithMany(e => e.ProductPlatforms).IsRequired(true).HasForeignKey(e => e.productID);


            });


            modelBuilder.Entity<XboxSchema.GroupDataTable>(entity =>
            {
                entity.Property(e => e.groupID).IsRequired(true).HasMaxLength(miscMaxLength);
                entity.Property(e => e.groupName).IsRequired(true).HasMaxLength(gameTitleMaxLength);
                entity.HasKey(entity => entity.groupID);

            });


            modelBuilder.Entity<XboxSchema.UserProfileTable>(entity =>
            {

                entity.Property(e => e.xuid).IsRequired(true).HasMaxLength(20);
                entity.Property(e => e.gamertag).IsRequired(true).HasMaxLength(16);
                entity.Property(e => e.lastScanned).IsRequired(false).HasDefaultValue(null);
                entity.HasKey(e => e.xuid);

            });

            modelBuilder.Entity<XboxSchema.GameGenreTable>(entity =>
            {

                entity.Property(e => e.modernTitleID).IsRequired(true).HasMaxLength(xboxIDMaxLength);
                entity.Property(e => e.genre).IsRequired(true).HasMaxLength(miscMaxLength);

                entity.HasKey(e => new { e.modernTitleID, e.genre });

                //entity.HasOne(e => e.).WithMany(e => e.Genres).IsRequired(true).HasForeignKey(e => e);

            });
        }


        private void ModelBuildSteam(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<SteamSchema.AppIDsTable>(entity =>
            {
                entity.Property(e => e.appID).IsRequired(true).ValueGeneratedNever();
                entity.Property(e => e.lastScanned).IsRequired(false).HasDefaultValue(null);
                entity.HasKey(e => e.appID);


                // entity.HasOne(e => e.AppDetails).WithOne(e => e.AppIDNavigation).HasForeignKey<SteamSchema.AppDetailsTable>().IsRequired();
            });

            modelBuilder.Entity<SteamSchema.AppDetailsTable>(entity =>
            {
                entity.Property(e => e.appID).IsRequired();
                entity.Property(e => e.appType).IsRequired(true).HasMaxLength(miscMaxLength);
                entity.Property(e => e.appName).IsRequired(true).HasMaxLength(gameTitleMaxLength);
                entity.Property(e => e.msrp).IsRequired(false).HasDefaultValue(null);
                entity.Property(e => e.listPrice).IsRequired(false).HasDefaultValue(null);
                entity.Property(e => e.isFree).IsRequired(true).HasDefaultValue(false);
                entity.Property(e => e.lastScanned).IsRequired(true).HasDefaultValue(DateTime.UtcNow);
                entity.Property(e=>e.imageURL).IsRequired(false).HasMaxLength(600).HasDefaultValue(null);

                entity.HasKey(e => e.appID);

                entity.HasOne(e => e.AppIDNavigation).WithOne(e => e.AppDetails)
                .HasForeignKey<SteamSchema.AppDetailsTable>(e => e.appID)
                .HasPrincipalKey<SteamSchema.AppIDsTable>(e => e.appID)
                .IsRequired(true);

                //entity.HasMany(e => e.Developers).WithOne(e => e.AppDetails).IsRequired(false).HasForeignKey(e => e.appID);
                //entity.HasMany(e=>e.Publishers).WithOne(e=>e.AppDetails).IsRequired(false).HasForeignKey(e=>e.appID);
                //entity.HasMany(e=>e.Packeges).WithOne(e=>e.AppDetails).IsRequired(false).HasForeignKey(e=>e.appID);
                //entity.HasMany(e=>e.Genres).WithOne(e=>e.AppDetails).IsRequired(false).HasForeignKey(e=>e.appID);

                //entity.HasMany(e=>e.Platforms).WithOne(e=>e.AppDetails).IsRequired(true).HasForeignKey(e=>e.appID);

                //entity.HasOne(e=>e.GameMarketLink).WithOne(e=>e.AppDetails).IsRequired(false).HasForeignKey<GameMarketSchema.SteamLinkTable>(e=>e.appID);

            });

            modelBuilder.Entity<SteamSchema.AppDevelopersTable>(entity =>
            {

                entity.Property(e => e.appID).IsRequired(true);
                entity.Property(e => e.developer).IsRequired(true).HasMaxLength(devPubmaxLength);

                entity.HasKey(e => new { e.appID, e.developer });

                entity.HasOne(e => e.AppDetails).WithMany(e => e.Developers).HasForeignKey(e => e.appID).HasPrincipalKey(e => e.appID).IsRequired(true);
            });

            modelBuilder.Entity<SteamSchema.AppPublishersTable>(entity =>
            {

                entity.Property(e => e.appID).IsRequired(true);
                entity.Property(e => e.publisher).IsRequired(true).HasMaxLength(devPubmaxLength);

                entity.HasKey(e => new { e.appID, e.publisher });

                entity.HasOne(e => e.AppDetails).WithMany(e => e.Publishers).HasForeignKey(e => e.appID).HasPrincipalKey(e => e.appID).IsRequired(true);
            });

            modelBuilder.Entity<SteamSchema.AppGenresTable>(entity =>
            {
                entity.Property(e => e.appID).IsRequired(true);
                entity.Property(e => e.genre).IsRequired(true).HasMaxLength(miscMaxLength);

                entity.HasKey(e => new { e.appID, e.genre });

                entity.HasOne(e => e.AppDetails).WithMany(e => e.Genres).HasForeignKey(e => e.appID).HasPrincipalKey(e => e.appID).IsRequired(true);
            });

            modelBuilder.Entity<SteamSchema.AppPlatformsTable>(entity =>
            {

                entity.Property(e => e.appID).IsRequired(true);
                entity.Property(e => e.platform).IsRequired(true).HasMaxLength(miscMaxLength);

                entity.HasKey(e => new { e.appID, e.platform });

                entity.HasOne(e => e.AppDetails).WithMany(e => e.Platforms).HasForeignKey(e => e.appID).HasPrincipalKey(e => e.appID).IsRequired(true);
            });

            modelBuilder.Entity<SteamSchema.AppDLCTable>(entity =>
            {

                entity.Property(e => e.appID).IsRequired(true);
                entity.Property(e => e.dlcID).IsRequired(true);


                entity.HasKey(e => new { e.appID, e.dlcID });

                entity.HasOne(e => e.AppDetails).WithMany(e => e.DLCs).HasForeignKey(e => e.appID).HasPrincipalKey(e => e.appID).IsRequired(true);
                entity.HasOne(e => e.AppIDNavigation).WithMany(e => e.AppDLC).HasForeignKey(e => e.dlcID).HasPrincipalKey(e => e.appID).IsRequired(true);
            });

            modelBuilder.Entity<SteamSchema.PackagesTable>(entity =>
            {

                entity.Property(e => e.appID).IsRequired(true).ValueGeneratedNever();
                entity.Property(e => e.packageID).IsRequired(true).ValueGeneratedNever();

                
                entity.HasKey(e => new { e.appID, e.packageID });

                entity.HasOne(e => e.AppDetails).WithMany(e => e.Packeges).HasForeignKey(e => e.appID).HasPrincipalKey(e => e.appID).IsRequired(true);
                entity.HasOne(e => e.PackageIDs).WithMany(e => e.Packages).HasForeignKey(e => e.packageID).HasPrincipalKey(e => e.packageID).IsRequired(true);
            });

            modelBuilder.Entity<SteamSchema.PackageIDsTable>(entity =>
            {
                entity.Property(e => e.packageID).IsRequired(true).ValueGeneratedNever();
                entity.Property(e => e.lastScanned).IsRequired(false).HasDefaultValue(null);

                entity.HasKey(e => e.packageID);

                //entity.HasMany(e => e.Packages).WithOne(e => e.PackageIDs).IsRequired(false).HasForeignKey(e => e.packageID);
                //entity.HasOne(e=>e.PackageDetails).WithOne(e=>e.packageIds).IsRequired(false).HasForeignKey<SteamSchema.PackageDetailsTable>(e=>e.packageID);
            });

            modelBuilder.Entity<SteamSchema.PackageDetailsTable>(entity =>
            {
                entity.Property(e => e.packageID).IsRequired(true).ValueGeneratedNever();
                entity.Property(e => e.packageName).IsRequired(true).HasMaxLength(gameTitleMaxLength);
                entity.Property(e => e.msrp).IsRequired(true);
                entity.Property(e => e.listPrice).IsRequired(true);
                entity.Property(e => e.lastScanned).IsRequired(true).HasDefaultValue(DateTime.UtcNow);

                entity.HasKey(e => e.packageID);

                entity.HasOne(e => e.packageIds).WithOne(e => e.PackageDetails)
                .HasForeignKey<SteamSchema.PackageDetailsTable>(e => e.packageID)
                .HasPrincipalKey<SteamSchema.PackageIDsTable>(e => e.packageID).IsRequired(true);
            });
        }
        private void ModelBuildGameMarket(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GameMarketSchema.GameTitleTable>(entity =>
            {
                entity.Property(e => e.gameID).IsRequired(true).ValueGeneratedOnAdd();
                entity.Property(e => e.gameTitle).IsRequired(true).HasMaxLength(gameTitleMaxLength);

                entity.HasKey(e => e.gameID);

                //entity.HasMany(e=>e.Developers).WithOne(e=>e.GameTitle).IsRequired(false).HasForeignKey(e=>e.gameID);
                //entity.HasMany(e=>e.Publishers).WithOne(e=>e.GameTitle).IsRequired(false).HasForeignKey(e=>e.gameID);
                //entity.HasMany(e=>e.SteamLinks).WithOne(e=>e.GameTitle).IsRequired(false).HasForeignKey(e=>e.gameID);
                //entity.HasMany(e=>e.XboxLinks).WithOne(e=>e.GameTitle).IsRequired(false).HasForeignKey(e=>e.gameID);
            });

            modelBuilder.Entity<GameMarketSchema.DeveloperTable>(entity =>
            {

                entity.Property(e => e.developer).IsRequired(true).HasMaxLength(devPubmaxLength);
                entity.Property(e => e.gameID).IsRequired(true).ValueGeneratedNever();


                entity.HasKey(e => new { e.gameID, e.developer });

                entity.HasOne(e => e.GameTitle).WithMany(e => e.Developers).HasForeignKey(e => e.gameID).HasPrincipalKey(e => e.gameID).IsRequired(true);
            });

            modelBuilder.Entity<GameMarketSchema.PublisherTable>(entity =>
            {
                entity.Property(e => e.publisher).IsRequired(true).HasMaxLength(devPubmaxLength);
                entity.Property(e => e.gameID).IsRequired(true).ValueGeneratedNever();
                
                entity.HasKey(e => new { e.gameID, e.publisher }    );

                entity.HasOne(e => e.GameTitle).WithMany(e => e.Publishers).HasForeignKey(e => e.gameID).HasPrincipalKey(e => e.gameID).IsRequired(true);
            });


            modelBuilder.Entity<GameMarketSchema.XboxLinkTable>(entity =>
            {

                entity.Property(e => e.modernTitleID).IsRequired(true).HasMaxLength(xboxIDMaxLength);
                entity.Property(e => e.gameID).IsRequired(true);

                entity.HasKey(e => e.modernTitleID);

                entity.HasOne(e => e.GameTitle).WithMany(e => e.XboxLinks).HasForeignKey(e => e.gameID).IsRequired(true);

                entity.HasOne(e => e.XboxTitle).WithOne(e => e.GameMarketLink)
                .HasForeignKey<GameMarketSchema.XboxLinkTable>(e => e.modernTitleID)
                .IsRequired(true);
            });

            modelBuilder.Entity<GameMarketSchema.SteamLinkTable>(entity =>
            {
                entity.Property(e => e.appID).IsRequired(true).HasMaxLength(xboxIDMaxLength);
                entity.Property(e => e.gameID).IsRequired(true);

                entity.HasKey(e => e.gameID);

                entity.HasOne(e => e.GameTitle).WithMany(e => e.SteamLinks).HasForeignKey(e => e.gameID).IsRequired(true);

                entity.HasOne(e => e.AppDetails).WithOne(e => e.GameMarketLink)
                .HasForeignKey<GameMarketSchema.SteamLinkTable>(e => e.appID)
                .HasPrincipalKey<SteamSchema.AppDetailsTable>(e => e.appID);
            });
        }
        private void ApplySchema<T>(ModelBuilder modelBuilder, ISchema schema) where T : ISchema
        {
            var entityTypes = typeof(T).GetNestedTypes().Where(t => t.IsClass);
            if (settings.sqlServerSettings.schemaLessDB)
                foreach (var entityType in entityTypes)
                {
                    var name = entityType.Name;
                    var schemaName = schema.GetName();
                    var fullName = $"{schemaName}_{Regex.Replace(entityType.Name, "Table", "", RegexOptions.IgnoreCase)}";
                    modelBuilder.Entity(entityType).ToTable(fullName);
                }
            else
            {
                foreach (var entityType in entityTypes)
                {
                    var name = entityType.Name;
                    var schemaName = schema.GetName();
                    modelBuilder.Entity(entityType).ToTable(Regex.Replace(name, "Table", "", RegexOptions.IgnoreCase), schemaName);
                }
            }
        }
    }
}
