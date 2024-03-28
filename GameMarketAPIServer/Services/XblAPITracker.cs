using GameMarketAPIServer.Configuration;
using Microsoft.Extensions.Options;

namespace GameMarketAPIServer.Services
{
    public class XblAPITracker
    {
        private readonly XboxSettings xbSettings;


        public enum hourlyAPICallsRemaining
        {
            title,
            market,
            profile,
            extra
        };

        public int maxHourlyAPIRequests { get; private set; } = 150;
        public int remainingAPIRequests { get; private set; } = 150;


        private int hourlyTitleRemaining = 0;
        private int hourlyMarketRemaining = 0;
        private int hourlyProfileRemaining = 0;
        private int hourlyExtraRemaining = 0;

        public XblAPITracker(IOptions<MainSettings> settings)
        {
            xbSettings = settings.Value.xboxSettings;
        }

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
            else if (xbSettings.autoUseExtraCalls && hourlyExtraRemaining > 0)
            {
                if (xbSettings.outputSettings.outputDebug)
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
                        if (xbSettings.outputSettings.outputDebug)
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
                case hourlyAPICallsRemaining.profile:
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
            remainingAPIRequests = maxHourlyAPIRequests;
            hourlyMarketRemaining = (int)Math.Round(maxHourlyAPIRequests * xbSettings.hourlyMarketRequestPercent);
            hourlyProfileRemaining = (int)Math.Round(maxHourlyAPIRequests * xbSettings.hourlyProfileRequestPercent);
            hourlyTitleRemaining = (int)Math.Round(maxHourlyAPIRequests * xbSettings.hourlyTitleRequestPercent);
            hourlyExtraRemaining = maxHourlyAPIRequests - hourlyMarketRemaining - hourlyProfileRemaining - hourlyTitleRemaining;
        }
        public void repartitionHourlyRequests()
        {
            hourlyMarketRemaining = (int)Math.Round(remainingAPIRequests * xbSettings.hourlyMarketRequestPercent);
            hourlyProfileRemaining = (int)Math.Round(remainingAPIRequests * xbSettings.hourlyProfileRequestPercent);
            hourlyTitleRemaining = (int)Math.Round(remainingAPIRequests * xbSettings.hourlyTitleRequestPercent);
            hourlyExtraRemaining = remainingAPIRequests - hourlyMarketRemaining - hourlyProfileRemaining - hourlyTitleRemaining;
        }
        public void outputRemainingRequests()
        {
            Console.WriteLine("Max Calls: " + maxHourlyAPIRequests);
            Console.WriteLine("Remaing Calls: " + remainingAPIRequests);
            Console.WriteLine("Profile calls Remaining: " + hourlyProfileRemaining + ".   Percentage of Max: " + xbSettings.hourlyProfileRequestPercent * 100 + "%");
            Console.WriteLine("Market calls Remaining: " + hourlyMarketRemaining + ".   Percentage of Max: " + xbSettings.hourlyMarketRequestPercent * 100 + "%");
            Console.WriteLine("Title calls Remaining: " + hourlyTitleRemaining + ".   Percentage of Max: " + xbSettings.hourlyTitleRequestPercent * 100 + "%");
            Console.WriteLine("Extra calls Remaining: " + hourlyExtraRemaining + ".   Percentage of Max: " + Math.Round(1 - xbSettings.hourlyProfileRequestPercent - xbSettings.hourlyMarketRequestPercent - xbSettings.hourlyTitleRequestPercent) * 100 + "%");
            Console.WriteLine("\n\n");
        }
        public void setRemaingCalls(int value)
        {
            remainingAPIRequests = value;
        }
        public void setMaxCAlls(int value)
        {
            maxHourlyAPIRequests = value;
        }
    }
}
