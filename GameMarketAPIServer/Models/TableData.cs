using GameMarketAPIServer.Services;
using ProtoBuf.WellKnownTypes;
using System.Numerics;

namespace GameMarketAPIServer.Models
{
    public interface  TableData
    {
        void outputData();
    };

    class genericXboxData : TableData
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
    class XboxUpdateScannedData : TableData
    {
        public string ID { get; set; }

        public DateTime lastScanned { get; set; } = DateTime.UtcNow;

        public void outputData()
        {
            Console.WriteLine("Update ID: " + ID);
        }
    };

    class SteamUpdateScannedData : TableData
    {
        public UInt32 ID;
        public DateTime lastScanned = DateTime.UtcNow;
        public void outputData()
        {
            Console.WriteLine("ID: " + ID);
        }
    }
    class XboxGameMarketData : TableData
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

    class XboxProfileData : TableData
    {
        public string xuID, gamertag;
        public void outputData()
        {

            Console.WriteLine("XUID: " + xuID);
            Console.WriteLine("Gamertag: " + gamertag);
        }
    }
    class XboxTitleDetailsData : TableData
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
    class XboxGameTitleData : TableData
    {
        public string titleID, modernTitleID, titleName, displayImage;
        public List<string> devices = new List<string>();
        public bool isGamePass;
        public void outputData()
        {
            Console.WriteLine();
        }
    }

    class XboxProductGroupData : TableData
    {
        public string groupID, groupName, productID, titleID;
        public  void outputData()
        {
            Console.WriteLine("Xbox Product Group Data");
            Console.WriteLine("GroupID: " + groupID);
            Console.WriteLine("GroupName: " + groupName);
            Console.WriteLine("ProductID: " + productID);
            Console.WriteLine("TitleID: " + titleID);
        }
    }

    class SteamAppListData : TableData
    {
        public string name;
        public UInt32 appid;
        public  void outputData()
        {
            Console.WriteLine("\nSteam App List Data");
            Console.WriteLine("App Name: " + name);
            Console.WriteLine("App ID: " + appid);
        }
    }

    class SteamAppDetailsData : TableData
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



    class GameMarketMergedXboxData : TableData
    {

        public int gameId;
        public string groupID, gameTitle;

        public List<string> xboxIds { get; set; }
        public void outputData()
        {
            throw new NotImplementedException();
        }
    }

    class GameMarketMergedData : TableData
    {
        private readonly object dataLock = new object();
        private Int32 gameID;
        public SortedSet<string>? developers;
        public SortedSet<string>? publishers;
        public Dictionary<DataBaseManager.Schemas, SortedSet<string>>? platformIds;
        public SortedSet<string>? xboxIds;
        public SortedSet<string>? steamIds;

        public GameMarketMergedData(int gameID)
        {
            this.gameID = gameID;
        }
        public void updateGameID(int gameID)
        {
            lock(dataLock)
            {
                this.gameID = gameID;
            }
        }
        public Int32 getGameID()
        {
            lock(dataLock)
            {
                return this.gameID;
            }
        }
        public void outputData() { }
    }
}
