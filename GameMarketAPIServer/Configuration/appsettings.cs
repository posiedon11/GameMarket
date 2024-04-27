using Microsoft.VisualBasic;
using SteamKit2.GC.Dota.Internal;

namespace GameMarketAPIServer.Configuration
{
    public class OutputSettings
    {
        public bool outputAll { get; set; }
        public bool outputDebug { get; set; }
        public bool outputSQLErrors { get; set; }

        public bool outputHTTP { get; set; }
    }
    public class SQLServerSettings
    {
        public string serverName { get; set; }
        public int serverPort { get; set; }
        public string serverUserName { get; set; }
        public string serverPassword { get; set; }



        public string xboxSchema { get; set; }
        public string steamSchema { get; set; }
        public string gamemarketSchema { get; set; }
        public bool schemaLessDB { get; set; }
        public string defaultSchema { get; set; }
        public OutputSettings outputSettings { get; set; }

        public string getConnectionString()
        {
            return $"Server={serverName};Port={serverPort};database={defaultSchema};User={serverUserName};Password={serverPassword};";
        }
        public void printServerSettings()
        {

        }
        public SQLServerSettings() { }
    }

    public class XboxSettings
    {
        public string apiKey { get; set; }
        public bool autoUseExtraCalls { get; set; }
        public OutputSettings outputSettings { get; set; }

        public int maxProductsForMarketDetails { get; set; }
        public double hourlyTitleRequestPercent { get; set; }
        public double hourlyMarketRequestPercent { get; set; }
        public double hourlyProfileRequestPercent { get; set; }

        public TimeSpan userProfileUpdateFrequency { get; set; }
        public TimeSpan gameTitleUpdateFrequency { get; set; }
        public TimeSpan marketDetailsUpdateFrequency { get; set; }
    }

    public class SteamSettings
    {
        public string apiKey { get; set; }
        public int steamAPIDailyCallLimit { get; set; }
        public int maxRequestIn5Minutes { get; set; }

        public OutputSettings outputSettings { get; set; }

        public TimeSpan allAppUpdateFrequency { get; set; }
        public TimeSpan gameAppUpdateFrequency { get; set; }
        public TimeSpan dlcUpdateFrequency { get; set; }
        public TimeSpan apiRequestTimer { get; set; }



    }

    public class MainSettings
    {
        public SQLServerSettings sqlServerSettings { get; set; }
        public XboxSettings xboxSettings { get; set; }
        public SteamSettings steamSettings { get; set; }


        public bool outputHTTPResponse = false;
        public bool outputHTTP = true;
    }
}
