using SteamKit2.GC.Artifact.Internal;

namespace GameMarketAPIServer.Models
{
    public interface IDBModel
    {
        string Name { get; }
        string fullPath();

        DBSchema getSchema();

    }
    public interface IDBObject : IDBModel
    {
        object ObjectType { get; }
    }
    public abstract class DBSchema(string name) : IDBModel
    {
        public string Name { get; private set; } = name;
        public virtual string fullPath() => Name;
        public virtual DBSchema getSchema() => this;
    }

    public abstract class DBTable(string name, DBSchema schema) : IDBModel
    {
        public string Name { get; } = name;
        public DBSchema Schema { get; } = schema;
        public virtual string fullPath() => $"{Schema.fullPath()}.{Name}";
        public virtual DBSchema getSchema() => Schema;


    }
    public class DBObject(string name, DBTable table) : IDBModel
    {
        public string Name { get; } = name;
        public DBTable Table { get; } = table;

        public virtual string fullPath() => $"{Table.fullPath()}.{Name}";

        public virtual DBSchema getSchema() => Table.getSchema();
    }


    public static class Database_structure
    {
        public static readonly XboxSchema Xbox = new XboxSchema();


        public class XboxSchema : DBSchema
        {
            public GameBundlesTable GameBundles { get; private set; }
            public GameTitlesTable GameTitles { get; private set; }
            public GroupDataTable GroupData { get; private set; }
            public MarketDetailsTable MarketDetails { get; private set; }
            public ProductIdsTable ProductIds { get; private set; }
            public ProductPlatformsTable ProductPlatforms { get; private set; }
            public TitleDetailsTable TitleDetails { get; private set; }
            public TitleDevicesTable TitleDevices { get; private set; }
            public UserProfilesTable UserProfiles { get; private set; }


            public XboxSchema() : base("Xbox")
            {
                GameBundles = new GameBundlesTable(this);
                GameTitles = new GameTitlesTable(this);
                GroupData = new GroupDataTable(this);
                MarketDetails = new MarketDetailsTable(this);
                ProductIds = new ProductIdsTable(this);
                ProductPlatforms = new ProductPlatformsTable(this);
                TitleDetails = new TitleDetailsTable(this);
                TitleDevices = new TitleDevicesTable(this);
                UserProfiles = new UserProfilesTable(this);



            }

            public class GameBundlesTable : DBTable
            {
                public DBObject relatedProductID { get; private set; }
                public DBObject productID { get; private set; }
                public GameBundlesTable(DBSchema schema) : base("GameBundles", schema)
                {
                    relatedProductID = new DBObject("relatedProductID", this);
                    productID = new DBObject("productID", this);
                }
            }
            public class GameTitlesTable : DBTable
            {
                public DBObject titleID { get; private set; }
                public DBObject titleName { get; private set; }
                public DBObject displayImage { get; private set; }
                public DBObject modernTitleID { get; private set; }
                public DBObject isGamePass { get; private set; }
                public DBObject groupID { get; private set; }
                public DBObject lastScanned { get; private set; }
                public GameTitlesTable(DBSchema schema) : base("GameTitles", schema)
                {
                    titleID = new DBObject("titleID", this);
                    titleName = new DBObject("titleName", this);
                    displayImage = new DBObject("displayImage", this);
                    modernTitleID = new DBObject("modernTitleID", this);
                    isGamePass = new DBObject("isGamePass", this);
                    groupID = new DBObject("groupID", this);
                    lastScanned = new DBObject("lastScanned", this);
                }
            }
            public class GroupDataTable : DBTable
            {
                public DBObject groupID { get; private set; }
                public DBObject groupName { get; private set; }
                public GroupDataTable(DBSchema schema) : base("GroupData", schema)
                {
                    groupID = new DBObject("groupID", this);
                    groupName = new DBObject("groupName", this);
                }
            }
            public class MarketDetailsTable : DBTable
            {
                public DBObject productID { get; private set; }
                public DBObject productTitle { get; private set; }
                public DBObject developerName { get; private set; }
                public DBObject publisherName { get; private set; }
                public DBObject currencyCode { get; private set; }
                public DBObject purchasable { get; private set; }
                public DBObject posterImage { get; private set; }
                public DBObject msrp { get; private set; }
                public DBObject listPrice { get; private set; }
                public DBObject startDate { get; private set; }
                public DBObject endDate { get; private set; }
                public MarketDetailsTable(DBSchema schema) : base("MarketDetails", schema)
                {
                    productID = new DBObject("productID", this);
                    productTitle = new DBObject("productTitle", this);
                    developerName = new DBObject("developerName", this);
                    publisherName = new DBObject("publisherName", this);
                    currencyCode = new DBObject("currencyCode", this);
                    purchasable = new DBObject("purchasable", this);
                    posterImage = new DBObject("posterImage", this);
                    msrp = new DBObject("msrp", this);
                    listPrice = new DBObject("listPrice", this);
                    startDate = new DBObject("startDate", this);
                    endDate = new DBObject("endDate", this);
                }
            }
            public class ProductIdsTable : DBTable
            {
                public DBObject productID { get; private set; }
                public DBObject lastScanned { get; private set; }
                public ProductIdsTable(DBSchema schema) : base("ProductIds", schema)
                {
                    productID = new DBObject("productID", this);
                    lastScanned = new DBObject("lastScanned", this);
                }
            }
            public class ProductPlatformsTable : DBTable
            {
                public DBObject productID { get; private set; }
                public DBObject platform { get; private set; }
                public ProductPlatformsTable(DBSchema schema) : base("ProductPlatforms", schema)
                {
                    productID = new DBObject("productID", this);
                    platform = new DBObject("platform", this);
                }
            }
            public class TitleDetailsTable : DBTable
            {
                public DBObject modernTitleID { get; private set; }
                public DBObject productID { get; private set; }
                public TitleDetailsTable(DBSchema schema) : base("TitleDetails", schema)
                {
                    modernTitleID = new DBObject("modernTitleID", this);
                    productID = new DBObject("productID", this);
                }
            }
            public class TitleDevicesTable : DBTable
            {
                public DBObject modernTitleID { get; private set; }
                public DBObject device { get; private set; }
                public TitleDevicesTable(DBSchema schema) : base("TitleDevices", schema)
                {
                    modernTitleID = new DBObject("modernTitleID", this);
                    device = new DBObject("device", this);
                }

            }
            public class UserProfilesTable : DBTable
            {
                public DBObject xuid { get; private set; }
                public DBObject gamertag { get; private set; }
                public DBObject lastScanned { get; private set; }
                public UserProfilesTable(DBSchema schema) : base("UserProfiles", schema)
                {
                    xuid = new DBObject("xuid", this);
                    gamertag = new DBObject("gamertag", this);
                    lastScanned = new DBObject("lastScanned", this);
                }

            }

        }


        public class SteamSchema : DBSchema
        {
            public AppIDsTable AppIDs { get; private set; }
            public AppDetailsTable AppDetails { get; private set; }
            public AppDevelopersTable AppDevelopers { get; private set; }
            public AppGenresTable AppGenres { get; private set; }
            public AppPlatformsTable AppPlatforms { get; private set; }
            public AppPublishersTable AppPublishers { get; private set; }
            public PackageDetailsTable PackageDetails { get; private set; }
            public PackageIDsTable PackageIDs { get; private set; }
            public PackagesTable Packages { get; private set; }

            public SteamSchema() : base("Steam")
            {
                AppIDs = new AppIDsTable(this);
                AppDetails = new AppDetailsTable(this);
                AppDevelopers = new AppDevelopersTable(this);
                AppGenres = new AppGenresTable(this);
                AppPlatforms = new AppPlatformsTable(this);
                AppPublishers = new AppPublishersTable(this);
                PackageDetails = new PackageDetailsTable(this);
                PackageIDs = new PackageIDsTable(this);
                Packages = new PackagesTable(this);
            }

            public class AppDetailsTable : DBTable
            {
                public DBObject appID { get; private set; }
                public DBObject appType { get; private set; }
                public DBObject appName { get; private set; }
                public DBObject msrp { get; private set; }
                public DBObject listPrice { get; private set; }
                public DBObject isFree { get; private set; }
                public DBObject lastScanned { get; private set; }
                public AppDetailsTable(DBSchema schema) : base("AppDetails", schema)
                {
                    appID = new DBObject("appID", this);
                    appType = new DBObject("appType", this);
                    appName = new DBObject("appName", this);
                    msrp = new DBObject("msrp", this);
                    listPrice = new DBObject("listPrice", this);
                    isFree = new DBObject("isFree", this);
                    lastScanned = new DBObject("lastScanned", this);
                }
            }

            public class AppDevelopersTable : DBTable
            {
                public DBObject appID { get; private set; }
                public DBObject developer { get; private set; }
                public AppDevelopersTable(DBSchema schema) : base("AppDevelopers", schema)
                {
                    appID = new DBObject("appID", this);
                    developer = new DBObject("developer", this);
                }
            }

            public class AppGenresTable : DBTable
            {
                public DBObject appID { get; private set; }
                public DBObject genre { get; private set; }
                public AppGenresTable(DBSchema schema) : base("AppGenres", schema)
                {
                    appID = new DBObject("appID", this);
                    genre = new DBObject("genre", this);
                }
            }

            public class AppIDsTable : DBTable
            {
                public DBObject appID { get; private set; }
                public DBObject lastScanned { get; private set; }
                public AppIDsTable(DBSchema schema) : base("AppIDs", schema)
                {
                    appID = new DBObject("appID", this);
                    lastScanned = new DBObject("lastScanned", this);
                }
            }

            public class AppPlatformsTable : DBTable
            {
                public DBObject appID { get; private set; }
                public DBObject platform { get; private set; }
                public AppPlatformsTable(DBSchema schema) : base("AppPlatforms", schema)
                {
                    appID = new DBObject("appID", this);
                    platform = new DBObject("platform", this);
                }
            }

            public class AppPublishersTable : DBTable
            {
                public DBObject appID { get; private set; }
                public DBObject publisher { get; private set; }
                public AppPublishersTable(DBSchema schema) : base("AppPublishers", schema)
                {
                    appID = new DBObject("appID", this);
                    publisher = new DBObject("publisher", this);
                }
            }

            public class PackageIDsTable : DBTable
            {
                public DBObject packageID { get; private set; }
                public DBObject lastScanned { get; private set; }
                public PackageIDsTable(DBSchema schema) : base("PackageIDs", schema)
                {
                    packageID = new DBObject("packageID", this);
                    lastScanned = new DBObject("lastScanned", this);
                }
            }
            public class PackageDetailsTable : DBTable
            {
                public DBObject packageID { get; private set; }
                public DBObject lastScanned { get; private set; }
                public PackageDetailsTable(DBSchema schema) : base("PackageDetails", schema)
                {
                    packageID = new DBObject("packageID", this);
                    lastScanned = new DBObject("lastScanned", this);
                }
            }
            public class PackagesTable : DBTable
            {
                public DBObject appID { get; private set; }
                public DBObject packageID { get; private set; }

                public PackagesTable(DBSchema schema) : base("Packages", schema)
                {
                    appID = new DBObject("appID", this);
                    packageID = new DBObject("packageID", this);
                }
            }
        }
    }
}
