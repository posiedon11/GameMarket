using GameMarketAPIServer.Services;
using ProtoBuf.WellKnownTypes;
using System.Numerics;

namespace GameMarketAPIServer.Models
{
    public interface ITableData
    {
        void outputData();
    };
    public class AllTableData
    {
        public class Xbox
        {

        }
    }
    class genericXboxData : ITableData
    {
        public string itemType { get; set; }
        public string productID { get; set; }
        public void outputData()
        {
            Console.WriteLine("Xbox Generic Data");
            Console.WriteLine("ProductID: " + productID);
            Console.WriteLine("Item Type: " + itemType);
        }

    };
    class XboxUpdateScannedData : ITableData
    {
        public string ID { get; set; }

        public DateTime lastScanned { get; set; } = DateTime.UtcNow;

        public void outputData()
        {
            Console.WriteLine("Update ID: " + ID);
        }
    };

    class SteamUpdateScannedData : ITableData
    {
        public UInt32 ID;
        public DateTime lastScanned = DateTime.UtcNow;
        public void outputData()
        {
            Console.WriteLine("ID: " + ID);
        }
    }
    class XboxGameMarketData : ITableData
    {
        public List<string> productIDs = new List<string>();
        public List<string> platforms = new List<string>();
        public string productID, devName, pubName, productTitle, productDesc, posterImage, currencyCode;
        public double ListPrice, msrp;

        public DateTime startDate, endDate, releaseDate;
        public bool isAccessable, isDemo, purchasable;
        public void outputData()
        {
            Console.WriteLine("Xbox Market Data");

            Console.WriteLine($"ProductID: {productID}");
            Console.WriteLine($"Developer: {devName}");
            Console.WriteLine($"Publisher: {pubName}");
            Console.WriteLine($"Title: {productTitle}");
            Console.WriteLine($"Desc: {productDesc}");
            Console.WriteLine($"CCode: {currencyCode}");
            Console.WriteLine($"ListPrice: {ListPrice}");
            Console.WriteLine($"MSRP: {msrp}");


            Console.WriteLine($"Release Date: {releaseDate}");
            Console.WriteLine($"Start Date: {startDate}");
            Console.WriteLine($"End Date: {endDate}");

            Console.WriteLine($"Accessable: {isAccessable}");
            Console.WriteLine($"Demo: {isDemo}");
            Console.WriteLine($"Purchasable: {purchasable}");



            Console.WriteLine("Platforms: ");
            foreach (var platform in platforms)
            {
                Console.Write($"\t{platform},");
            }
        }
    }

    class XboxProfileData : ITableData
    {
        public string xuID, gamertag;
        public void outputData()
        {

            Console.WriteLine("XUID: " + xuID);
            Console.WriteLine("Gamertag: " + gamertag);
        }
    }
    class XboxTitleDetailsData : ITableData
    {
        public string productTitle { get; set; }
        public string modernTitleID { get; set; }
        public string productID { get; set; }
        public string groupName { get; set; }
        public string groupID { get; set; }



        public List<string> bundleIDs { get; set; } = new List<string>();

        public DateTime lastScanned { get; set; } = DateTime.UtcNow;
        public void outputData()
        {
            Console.WriteLine("Xbox Title Detail Data");
            Console.WriteLine("Product Title: " + productTitle);
            Console.WriteLine("TitleID: " + modernTitleID);
            Console.WriteLine("ProductID: " + productID);
            Console.WriteLine("Group Name: " + groupName);
            Console.WriteLine("Group ID: " + groupID);
            Console.Write("Bundle IDS: ");
            foreach (var bundleID in bundleIDs)
            {
                Console.Write(bundleID + "  ");
            }
            Console.WriteLine("\n");
        }
    }
    class XboxGameTitleData : ITableData
    {
        public string titleID, modernTitleID, titleName, displayImage;
        public List<string> devices = new List<string>();
        public bool isGamePass;
        public void outputData()
        {
            Console.WriteLine();
        }
    }

    class XboxProductGroupData : ITableData
    {
        public string groupID, groupName, productID, titleID;
        public void outputData()
        {
            Console.WriteLine("Xbox Product Group Data");
            Console.WriteLine("GroupID: " + groupID);
            Console.WriteLine("GroupName: " + groupName);
            Console.WriteLine("ProductID: " + productID);
            Console.WriteLine("TitleID: " + titleID);
        }
    }

    class SteamAppListData : ITableData
    {
        public string name;
        public UInt32 appid;
        public void outputData()
        {
            Console.WriteLine("\nSteam App List Data");
            Console.WriteLine("App Name: " + name);
            Console.WriteLine("App ID: " + appid);
        }
    }

    class SteamAppDetailsData : ITableData
    {
        public string appName, appType;
        public List<UInt32> dlcs, packages;
        public List<string> developers, publishers, platforms, genres;
        public UInt32 appID;

        public int msrp, listprice;

        public bool isFree;
        public void outputData()
        {
            throw new NotImplementedException();
        }
    }



    class GameMarketMergedXboxData : ITableData
    {

        public int gameId;
        public string groupID, gameTitle;

        public List<string> xboxIds { get; set; }
        public void outputData()
        {
            throw new NotImplementedException();
        }
    }

    class GameMarketMergedData : ITableData
    {
        private readonly object dataLock = new object();
        private UInt32 gameID;
        public string gameTitle;
        public SortedSet<string>? developers;
        public SortedSet<string>? publishers;
        public SortedSet<string>? xboxIds;
        public SortedSet<string>? steamIds;
        public Dictionary<DBSchema, SortedSet<string>> platformIds;

        public GameMarketMergedData(UInt32 gameID)
        {
            this.gameID = gameID;
        }

        public void insertPlatformIds(DBSchema schema, SortedSet<string> ids)
        {
            lock (dataLock)
            {
                if (platformIds.ContainsKey(schema))
                {
                    platformIds[schema].UnionWith(ids);
                }
                else
                {
                    platformIds.TryAdd(schema, ids);
                }
            }
        }
        public void updateGameID(UInt32 gameID)
        {
            lock (dataLock)
            {
                this.gameID = gameID;
            }
        }
        public UInt32 getGameID()
        {
            lock (dataLock)
            {
                return this.gameID;
            }
        }
        public void outputData() { }
    }
}
