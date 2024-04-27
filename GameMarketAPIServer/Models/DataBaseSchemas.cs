using Microsoft.AspNetCore.Mvc;
using SteamKit2;
using System.ComponentModel.DataAnnotations;

namespace GameMarketAPIServer.Models
{
    public interface ISchema
    {
        ISchema GetSchema();
        string GetName();
        //void output() { logger.LogDebug(toString()); }
    }
    public interface ITable
    {
        ISchema GetSchema();
        object GetKey();
    }

    public static class DataBaseSchemas
    {
        public static readonly XboxSchema Xbox = new XboxSchema();
        public static readonly SteamSchema Steam = new SteamSchema();
        public static readonly GameMarketSchema GameMarket = new GameMarketSchema();


        public class XboxSchema : ISchema
        {
            private static XboxSchema Instance { get; } = new XboxSchema();
            private static string Name { get; } = "Xbox";
            public string GetName() => Name;
            public ISchema GetSchema() => Instance;

            public Type GameTitles { get; } = typeof(GameTitleTable);

            public class GameTitleTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => modernTitleID;
                public string titleID { get; set; }
                public string titleName { get; set; }
                public string? displayImage { get; set; }
                public string modernTitleID { get; set; }
                public bool isGamePass { get; set; }
                public string groupID { get; set; }
                public DateTime? lastScanned { get; set; }

                public virtual ICollection<TitleDetailTable>? TitleDetails { get; set; } = new List<TitleDetailTable>();
                public virtual ICollection<TitleDeviceTable>? TitleDevices { get; set; } = new List<TitleDeviceTable>();

                public virtual GameMarketSchema.XboxLinkTable? GameMarketLink { get; set; }
                // public virtual GroupDataTable GroupData { get; set; }
                public GameTitleTable()
                {

                }
            }
            public class GameGenreTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => ID;
                public UInt32 ID { get; set; }
                public string titleID { get; set; }
                public string genre { get; set; }
                public GameGenreTable() { }
            }
            public class GroupDataTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => groupID;
                public string groupID { get; set; }
                public string groupName { get; set; }

                //public virtual ICollection<GameTitleTable> GameTitlesTable { get; set; } = new List<GameTitleTable>();
            }
            public class MarketDetailTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => productID;
                public string productID { get; set; }
                public string productTitle { get; set; }
                public string developerName { get; set; }
                public string publisherName { get; set; }
                public string? currencyCode { get; set; }
                public bool purchasable { get; set; }
                public string? posterImage { get; set; }
                public double? msrp { get; set; }
                public double? listPrice { get; set; }
                public DateTime releaseDate { get; set; }
                public DateTime startDate { get; set; }
                public DateTime endDate { get; set; }

                public ProductIDTable ProductIDNavig { get; set; }

                public virtual ICollection<ProductPlatformTable>? ProductPlatforms { get; set; } = new List<ProductPlatformTable>();

                public MarketDetailTable() { }

            }
            public class ProductIDTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => productID;
                [Key]
                public string productID { get; set; }
                public DateTime? lastScanned { get; set; }

                public virtual MarketDetailTable? MarketDetails { get; set; }
                public virtual ICollection<TitleDetailTable>? TitleDetails { get; set; } = new List<TitleDetailTable>();
                public virtual ICollection<GameBundleTable>? GameBundles { get; set; } = new List<GameBundleTable>();

                public ProductIDTable() { }
                public ProductIDTable(string productID)
                {
                    this.productID = productID;
                }

            }

            public class ProductPlatformTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => ID;
                public UInt32 ID { get; set; }
                public string productID { get; set; }
                public string platform { get; set; }

                public virtual MarketDetailTable MarketDetail { get; set; }

                public ProductPlatformTable() { }
                public ProductPlatformTable(string productID, string platform)
                {
                    this.productID = productID;
                    this.platform = platform;
                }

            }
            public class TitleDetailTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => productID;
                public string modernTitleID { get; set; }

                public string productID { get; set; }

                public virtual ICollection<GameBundleTable>? GameBundles { get; set; } = new List<GameBundleTable>();
                public virtual ProductIDTable ProductIDNavig { get; set; }
                public virtual GameTitleTable GameTitle { get; set; }
            }

            public class TitleDeviceTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => ID;
                public UInt32 ID { get; set; }
                public string modernTitleID { get; set; }
                public string device { get; set; }

                public virtual GameTitleTable GameTitle { get; set; }
                public TitleDeviceTable() { }
                public TitleDeviceTable(string modernTitleID, string device)
                {
                    this.modernTitleID = modernTitleID;
                    this.device = device;
                }

            }

            public class GameBundleTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => relatedProductID;

                public string relatedProductID { get; set; }

                public string productID { get; set; }

                public TitleDetailTable TitleDetails { get; set; }
                public ProductIDTable ProductIDNavig { get; set; }

                public GameBundleTable() { }
                public GameBundleTable(string relatedProductID, string productID)
                {
                    this.relatedProductID = relatedProductID;
                    this.productID = productID;
                }
            }

            public class UserProfileTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => xuid;
                public string xuid { get; set; }
                public string gamertag { get; set; }
                public DateTime? lastScanned { get; set; }

                public UserProfileTable() { }

                public UserProfileTable(string xuid, string gamertag)
                {
                    this.xuid = xuid;
                    this.gamertag = gamertag;
                }

            }

        }

        public class SteamSchema : ISchema
        {
            private static SteamSchema Instance { get; } = new SteamSchema();
            public ISchema GetSchema() => Instance;

            private static string Name { get; } = "Steam";
            public string GetName() => Name;
            public class AppDetailsTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => appID;
                public UInt32 appID { get; set; }
                public string appType { get; set; }
                public string appName { get; set; }
                public double? msrp { get; set; }
                public double? listPrice { get; set; }
                public bool isFree { get; set; }
                public DateTime lastScanned { get; set; }

                public virtual AppIDsTable AppIDNavigation { get; set; }

                public ICollection<AppDevelopersTable>? Developers { get; set; } = new List<AppDevelopersTable>();
                public ICollection<AppGenresTable>? Genres { get; set; } = new List<AppGenresTable>();
                public ICollection<AppPublishersTable>? Publishers { get; set; } = new List<AppPublishersTable>();
                public ICollection<AppPlatformsTable> Platforms { get; set; } = new List<AppPlatformsTable>();
                public ICollection<PackagesTable>? Packeges { get; set; } = new List<PackagesTable>();
                public ICollection<AppDLCTable>? DLCs { get; set; } = new List<AppDLCTable>();

                public virtual GameMarketSchema.SteamLinkTable? GameMarketLink { get; set; }

                public AppDetailsTable() { }
                public AppDetailsTable(UInt32 _appID, string _appType, string _appName, double _msrp, double _listPrice, bool _isFree, DateTime _lastScanned)
                {
                    appID = _appID;
                    appType = _appType;
                    appName = _appName;
                    msrp = _msrp;
                    listPrice = _listPrice;
                    isFree = _isFree;
                    lastScanned = _lastScanned;
                }

            }

            public class AppDevelopersTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => ID;

                public UInt32 ID { get; set; }
                public UInt32 appID { get; set; }
                public string developer { get; set; }
                public virtual AppDetailsTable AppDetails { get; set; }
                public AppDevelopersTable() { }
                public AppDevelopersTable(UInt32 _appID, string _developer)
                {
                    appID = _appID;
                    developer = _developer;
                }

            }

            public class AppGenresTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => ID;

                public UInt32 ID { get; set; }
                public UInt32 appID { get; set; }
                public string genre { get; set; }
                public virtual AppDetailsTable AppDetails { get; set; }

                public AppGenresTable() { }
                public AppGenresTable(UInt32 _appID, string _genre)
                {
                    appID = _appID;
                    genre = _genre;
                }

            }

            public class AppIDsTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => appID;
                public UInt32 appID { get; set; }
                public DateTime? lastScanned { get; set; }

                public virtual AppDetailsTable? AppDetails { get; set; }
                public virtual AppDLCTable? AppDLC { get; set; }

                public AppIDsTable() { }
                public AppIDsTable(UInt32 _appID, DateTime _lastScanned)
                {
                    appID = _appID;
                    lastScanned = _lastScanned;
                    this.AppDetails = AppDetails;
                }
            }

            public class AppPlatformsTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => ID;
                public UInt32 ID { get; set; }
                public UInt32 appID { get; set; }
                public string platform { get; set; }

                public AppDetailsTable AppDetails { get; set; }
                // public virtual List<AppDetailsTable> AppDetails { get; set; } = new List<AppDetailsTable>();
                // public virtual List<AppIDsTable> appIDs { get; set; } = new List<AppIDsTable>();

                public AppPlatformsTable() { }
                public AppPlatformsTable(UInt32 _appID, string _platform)
                {
                    appID = _appID;
                    platform = _platform;
                }
            }

            public class AppPublishersTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => ID;
                public UInt32 ID { get; set; }
                public UInt32 appID { get; set; }
                public string publisher { get; set; }
                public virtual AppDetailsTable AppDetails { get; set; }
                //public virtual List<AppIDsTable> appIDs { get; set; } = new List<AppIDsTable>();

                public AppPublishersTable() { }
                public AppPublishersTable(UInt32 _appID, string _publisher)
                {
                    appID = _appID;
                    publisher = _publisher;
                }
            }

            public class AppDLCTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => ID;
                public UInt32 ID { get; set; }
                public UInt32 appID { get; set; }
                public UInt32 dlcID { get; set; }
                public virtual AppDetailsTable AppDetails { get; set; }
                public virtual AppIDsTable AppIDs { get; set; }
                public AppDLCTable() { }
                public AppDLCTable(UInt32 _appID, UInt32 _dlcID)
                {
                    appID = _appID;
                    dlcID = _dlcID;
                }
            }

            public class PackageIDsTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => packageID;
                public UInt32 packageID { get; set; }
                public DateTime? lastScanned { get; set; }

                public virtual List<PackagesTable>? Packages { get; set; } = new List<PackagesTable>();
                public virtual PackageDetailsTable? PackageDetails { get; set; }

                public PackageIDsTable() { }
                public PackageIDsTable(UInt32 _packageID, DateTime? _lastScanned = null)
                {
                    packageID = _packageID;
                    lastScanned = _lastScanned;
                }
            }
            public class PackageDetailsTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => packageID;
                public UInt32 packageID { get; set; }
                public string packageName { get; set; }
                public double msrp { get; set; }
                public double listPrice { get; set; }

                public DateTime lastScanned { get; set; }

                public virtual PackageIDsTable packageIds { get; set; }

                public PackageDetailsTable() { }
                public PackageDetailsTable(UInt32 _packageID, DateTime _lastScanned, PackageIDsTable packages)
                {
                    packageID = _packageID;
                    lastScanned = _lastScanned;
                    this.packageIds = packages;
                }
            }
            public class PackagesTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => ID;

                public UInt32 ID { get; set; }
                public UInt32 appID { get; set; }
                public UInt32 packageID { get; set; }
                public virtual PackageIDsTable PackageIDs { get; set; }
                public virtual AppDetailsTable AppDetails { get; set; }

                public PackagesTable() { }
                public PackagesTable(UInt32 _appID, UInt32 _packageID)
                {
                    appID = _appID;
                    packageID = _packageID;
                }

            }
        }

        public class GameMarketSchema : ISchema
        {
            private static GameMarketSchema Instance { get; } = new GameMarketSchema();

            public ISchema GetSchema() => Instance;

            private static string Name { get; } = "GameMarket";
            public string GetName() => Name;
            public class DeveloperTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => ID;
                public UInt32 ID { get; set; }
                public UInt32 gameID { get; set; }
                public string developer { get; set; }
                public virtual GameTitleTable GameTitle { get; set; }
                public DeveloperTable() { }
                public DeveloperTable(UInt32 _gameID, string _developer)
                {
                    gameID = _gameID;
                    developer = _developer;
                }
            }
            public class GameTitleTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => gameID;
                public UInt32 gameID { get; set; }
                public string gameTitle { get; set; }
                public virtual List<DeveloperTable>? Developers { get; set; } = new List<DeveloperTable>();
                public virtual List<PublisherTable>? Publishers { get; set; } = new List<PublisherTable>();
                public virtual List<XboxLinkTable>? XboxLinks { get; set; } = new List<XboxLinkTable>();
                public virtual List<SteamLinkTable>? SteamLinks { get; set; } = new List<SteamLinkTable>();

                public GameTitleTable() { }
                public GameTitleTable(UInt32 _gameID, string _gameTitle)
                {
                    gameID = _gameID;
                    gameTitle = _gameTitle;
                }

            }
            public class PublisherTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => ID;
                public UInt32 ID { get; set; }
                public UInt32 gameID { get; set; }
                public string publisher { get; set; }
                public virtual GameTitleTable GameTitle { get; set; }
                public PublisherTable() { }
                public PublisherTable(UInt32 _gameID, string _publisher)
                {
                    gameID = _gameID;
                    publisher = _publisher;
                }
            }
            public class XboxLinkTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => modernTitleID;
                public UInt32 gameID { get; set; }
                public string modernTitleID { get; set; }

                public virtual GameTitleTable GameTitle { get; set; }
                public virtual XboxSchema.GameTitleTable XboxTitle { get; set; }


                public XboxLinkTable() { }
                public XboxLinkTable(UInt32 _gameID, string _modernTitleID)
                {
                    gameID = _gameID;
                    modernTitleID = _modernTitleID;
                }
            }
            public class SteamLinkTable : ITable
            {
                public ISchema GetSchema() => Instance;
                public object GetKey() => appID;
                public UInt32 gameID { get; set; }
                public UInt32 appID { get; set; }
                public virtual GameTitleTable GameTitle { get; set; }
                public virtual SteamSchema.AppDetailsTable AppDetails { get; set; }
                public SteamLinkTable() { }
                public SteamLinkTable(UInt32 _gameID, UInt32 _appID)
                {
                    gameID = _gameID;
                    appID = _appID;
                }
            }
        }
    }
}

