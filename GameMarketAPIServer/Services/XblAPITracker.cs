using GameMarketAPIServer.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace GameMarketAPIServer.Services
{
    public class XblAPITracker
    {
        private readonly XboxSettings xbSettings;
        private readonly ILogger logger;
        private readonly Stopwatch stopwatch = new Stopwatch();

        public enum hourlyAPICallsRemaining
        {
            title,
            market,
            profile,
            extra
        };
        private object trackerLock = new object();
        public int maxHourlyAPIRequests { get; private set; } = 150;
        public int remainingAPIRequests { get; private set; } = 150;


        private int hourlyTitleRemaining = 0;
        private int hourlyMarketRemaining = 0;
        private int hourlyProfileRemaining = 0;
        private int hourlyExtraRemaining = 0;

        public XblAPITracker(IOptions<MainSettings> settings, ILogger<XblAPITracker> logger)
        {
            xbSettings = settings.Value.xboxSettings;
            this.logger = logger;
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
        public bool needCheckHeaders()
        {
            if (!stopwatch.IsRunning)
            {
                stopwatch.Start();
                return true;
            }
            else
            {
                if (stopwatch.Elapsed.Minutes >= 60)
                {
                    stopwatch.Restart();
                    return true;
                }
                else
                {
                    return false;
                }
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
                    logger.LogDebug("Using Extra");
                return true;
            }
            return false;
        }
        public void makeRequest(hourlyAPICallsRemaining hourlyCalls)
        {
            lock (trackerLock)
            {
                --remainingAPIRequests;
                switch (hourlyCalls)
                {
                    case hourlyAPICallsRemaining.title:
                        if (hourlyTitleRemaining-- <= 0)
                        {
                            if (xbSettings.outputSettings.outputDebug)
                                logger.LogTrace("Taking from extra");
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
        }
        public void resetHourlyRequest()
        {
            lock (trackerLock)
            {
                remainingAPIRequests = maxHourlyAPIRequests;
                hourlyMarketRemaining = (int)Math.Round(maxHourlyAPIRequests * xbSettings.hourlyMarketRequestPercent);
                hourlyProfileRemaining = (int)Math.Round(maxHourlyAPIRequests * xbSettings.hourlyProfileRequestPercent);
                hourlyTitleRemaining = (int)Math.Round(maxHourlyAPIRequests * xbSettings.hourlyTitleRequestPercent);
                hourlyExtraRemaining = maxHourlyAPIRequests - hourlyMarketRemaining - hourlyProfileRemaining - hourlyTitleRemaining;
            }
        }
        public void repartitionHourlyRequests()
        {
            lock (trackerLock)
            {
                hourlyMarketRemaining = (int)Math.Round(remainingAPIRequests * xbSettings.hourlyMarketRequestPercent);
                hourlyProfileRemaining = (int)Math.Round(remainingAPIRequests * xbSettings.hourlyProfileRequestPercent);
                hourlyTitleRemaining = (int)Math.Round(remainingAPIRequests * xbSettings.hourlyTitleRequestPercent);
                hourlyExtraRemaining = remainingAPIRequests - hourlyMarketRemaining - hourlyProfileRemaining - hourlyTitleRemaining;
            }
        }
        public void outputRemainingRequests()
        {
            logger.LogInformation($"Max Calls: {maxHourlyAPIRequests}\n" +
            $"Remaing Calls: {remainingAPIRequests}\n" +
            $"Profile calls Remaining:  {hourlyProfileRemaining}.   Percentage of Max:  {xbSettings.hourlyProfileRequestPercent * 100}%\n" +
            $"Market calls Remaining:  {hourlyMarketRemaining}.   Percentage of Max:  {xbSettings.hourlyMarketRequestPercent * 100}%\n" +
            $"Title calls Remaining:  {hourlyTitleRemaining}.   Percentage of Max:  {xbSettings.hourlyTitleRequestPercent * 100} %\n" +
            $"Extra calls Remaining:  {hourlyExtraRemaining}.   Percentage of Max:  {Math.Round(1 - xbSettings.hourlyProfileRequestPercent - xbSettings.hourlyMarketRequestPercent - xbSettings.hourlyTitleRequestPercent) * 100} %\n\n");
        }
        public void setRemaingCalls(int value)
        {
            lock (trackerLock)
            {
                remainingAPIRequests = value;
            }
        }
        public void setMaxCAlls(int value)
        {
            lock (trackerLock)
                maxHourlyAPIRequests = value;
        }
    }
}
