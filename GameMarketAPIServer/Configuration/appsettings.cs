using Microsoft.VisualBasic;
using SteamKit2.GC.Dota.Internal;

namespace GameMarketAPIServer.Configuration
{
    public class SQLServerSettings
    {
        public string serverName = "127.0.0.1";
        public int serverPort = 3306;
        public string serverUserName = "root";
        public string serverPassword = "GamePasswordMarket11";

        public string xboxSchema = "Xbox";
        public string steamSchema = "Steam";
        public string gamemarketSchema = "GameMarket";
        public bool outputDebug = true;

        public void printServerSettings()
        {

        }
        public SQLServerSettings() { }
    }

    public class XboxSettings
    {
        public string xblAPIKey = "33c3e09e-754c-47e5-b6fd-c1710f4eccad";
        public int maxHourlyAPIRequests = 150;
        public int remainingAPIRequests = 150;
        public int maxProductsForMarketDetails = 40;
        public bool outputDebug = false, autoUseExtraCalls = true, outputSQLErrors = false;


        private const double hourlyTitleRequestPercent = .20;
        private const double hourlyMarketRequestPercent = .40;
        private const double hourlyProfileRequestPercent = .05;
        private int hourlyTitleRemaining = 0;
        private int hourlyMarketRemaining = 0;
        private int hourlyProfileRemaining = 0;
        private int hourlyExtraRemaining = 0;

        public TimeSpan userProfileUpdateFrequency = TimeSpan.FromDays(3);
        public TimeSpan gameTitleUpdateFrequency = TimeSpan.FromDays(1);
        public TimeSpan marketDetailsUpdateFrequency = TimeSpan.FromDays(2);
        public enum hourlyAPICallsRemaining
        {
            title,
			market,
			profile,
			extra
        };

        public int remaingRequests(hourlyAPICallsRemaining hourlyCalls)
        {
            switch (hourlyCalls)
            {
                case hourlyAPICallsRemaining.market:
                    return hourlyMarketRemaining;

                case hourlyAPICallsRemaining.extra:
                    return hourlyExtraRemaining;

                case hourlyAPICallsRemaining.profile:
                    return hourlyProfileRemaining;

                case hourlyAPICallsRemaining.title:
                    return hourlyTitleRemaining;

                default:
                    return 0;
            }
        }
        public bool canRequest(hourlyAPICallsRemaining hourlyCalls)
        {
            bool remaingCalls = false;
            if (remainingAPIRequests <= 0)
                return false;
            switch (hourlyCalls)
            {
                case hourlyAPICallsRemaining.title:
                    if (hourlyTitleRemaining > 0)
                    {
                        remaingCalls = true;
                    }
                    break;
                case hourlyAPICallsRemaining.market:
                    if (hourlyMarketRemaining > 0)
                    {
                        remaingCalls = true;
                    }
                    break;
                case hourlyAPICallsRemaining.profile:
                    if (hourlyProfileRemaining > 0)
                    {
                        remaingCalls = true;
                    }
                    break;
                case hourlyAPICallsRemaining.extra:
                    if (hourlyExtraRemaining > 0)
                    {
                        remaingCalls = true;
                    }
                    break;

                default:
                    return false;
            }
            if (remaingCalls)
            {
                return true;
            }
            else if (autoUseExtraCalls && hourlyExtraRemaining > 0)
            {
                if (outputDebug)
                    Console.WriteLine("Using Extra");
                return true;
            }
            return false;
        }
        public void makeRequest(hourlyAPICallsRemaining hourlyCalls)
        {
            --remainingAPIRequests;
            switch (hourlyCalls)
            {
                case hourlyAPICallsRemaining.title:
                    if (hourlyTitleRemaining-- <= 0)
                    {
                        if (outputDebug)
                            Console.WriteLine("Taking from extra");
                        --hourlyExtraRemaining;
                    }

                    break;
                case hourlyAPICallsRemaining.market:
                    if (hourlyMarketRemaining-- <= 0)
                    {
                        --hourlyExtraRemaining;
                    }
                    break;
                case hourlyAPICallsRemaining.  profile:
                    if (hourlyProfileRemaining-- <= 0)
                    {
                        --hourlyExtraRemaining;
                    }
                    break;
                case hourlyAPICallsRemaining.extra:
                    hourlyExtraRemaining--;
                    break;
                default:
                    break;
            }
        }
        public void resetHourlyRequest()
        {
            hourlyMarketRemaining = (int)Math.Round( maxHourlyAPIRequests * hourlyMarketRequestPercent);
            hourlyProfileRemaining = (int)Math.Round(maxHourlyAPIRequests * hourlyProfileRequestPercent);
            hourlyTitleRemaining = (int)Math.Round(maxHourlyAPIRequests * hourlyTitleRequestPercent);
            hourlyExtraRemaining = maxHourlyAPIRequests - hourlyMarketRemaining - hourlyProfileRemaining - hourlyTitleRemaining;
        }
        public void repartitionHourlyRequests()
        {
            hourlyMarketRemaining = (int)Math.Round(remainingAPIRequests * hourlyMarketRequestPercent);
            hourlyProfileRemaining = (int)Math.Round(remainingAPIRequests * hourlyProfileRequestPercent);
            hourlyTitleRemaining = (int)Math.Round(remainingAPIRequests * hourlyTitleRequestPercent);
            hourlyExtraRemaining = remainingAPIRequests - hourlyMarketRemaining - hourlyProfileRemaining - hourlyTitleRemaining;
        }
        public void outputRemainingRequests()
        {
            Console.WriteLine("Max Calls: " + maxHourlyAPIRequests);
            Console.WriteLine("Remaing Calls: " + remainingAPIRequests);
            Console.WriteLine("Profile calls Remaining: " + hourlyProfileRemaining + ".   Percentage of Max: " + hourlyProfileRequestPercent * 100 + "%");
            Console.WriteLine("Market calls Remaining: " + hourlyMarketRemaining + ".   Percentage of Max: " + hourlyMarketRequestPercent * 100 + "%");
            Console.WriteLine("Title calls Remaining: " + hourlyTitleRemaining + ".   Percentage of Max: " + hourlyTitleRequestPercent * 100 + "%");
            Console.WriteLine("Extra calls Remaining: " + hourlyExtraRemaining + ".   Percentage of Max: " + Math.Round(1 - hourlyProfileRequestPercent - hourlyMarketRequestPercent - hourlyTitleRequestPercent) * 100 + "%");
            Console.WriteLine("\n\n");
        }
        public void setRemaingCalls(int value)
        {
            remainingAPIRequests = value;
        }
    }

    public class SteamSettings
    {
        public string apiKey = "616722F93C5B8CD52F9322620CE45E92";
        public bool outputData = false;

        public int steamAPIDailyCallLimit = 100000;
        public int steamCurrentCalls = 0;
        public int maxRequestIn5Minutes = 200;


        public TimeSpan allAppUpdateFrequency = TimeSpan.FromDays(14);
        public TimeSpan gameAppUpdateFrequency = TimeSpan.FromDays(7);
        public TimeSpan dlcUpdateFrequency = TimeSpan.FromDays(7);
        public TimeSpan apiRequestTimer = TimeSpan.FromMinutes(5);


        public bool canRequest()
        {
            if (steamCurrentCalls < steamAPIDailyCallLimit) { return true; }
            else return false;
        }
        public void makeRequest()
        {
            ++steamCurrentCalls;
        }
    }

    public class Settings
    {
        public XboxSettings xboxSettings { get; set; }
        public SQLServerSettings sqlServerSettings { get; set; }
        public SteamSettings steamSettings { get; set; }
        public bool outputHTTPResponse = false;
        public bool outputHTTP = true;


        //Singleton
        private static readonly Lazy<Settings> lazy = new Lazy<Settings>(() => new Settings());
        public static Settings Instance { get { return lazy.Value; } }


        private Settings() 
        { 
            xboxSettings = new XboxSettings();
            sqlServerSettings = new SQLServerSettings();
            steamSettings = new SteamSettings();
        }
        private Settings(XboxSettings xboxSettings, SQLServerSettings sqlServerSettings, SteamSettings steaamSettings)
        {
            this.xboxSettings = xboxSettings;
            this.sqlServerSettings = sqlServerSettings;
            this.steamSettings = steamSettings;
        }
    }
}
