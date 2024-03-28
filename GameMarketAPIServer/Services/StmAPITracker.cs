using GameMarketAPIServer.Configuration;
using Microsoft.Extensions.Options;

namespace GameMarketAPIServer.Services
{
    public class StmAPITracker
    {
        private readonly SteamSettings stmSettings;
        public int steamCurrentCalls { get; set; } = 0;

        public StmAPITracker(IOptions<MainSettings> settings)
        {
            stmSettings = settings.Value.steamSettings;
        }
        public bool canRequest()
        {
            if (steamCurrentCalls < stmSettings.steamAPIDailyCallLimit) { return true; }
            else return false;
        }
        public void makeRequest()
        {
            ++steamCurrentCalls;
        }
    }
}
