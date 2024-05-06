using System;
using System.IO;

namespace GameMarketAPIServer.Utilities
{
    public class SteamAllAppLastRun
    {
        private static string filePath = "steam_all_app_last_run.txt";

        public static void SaveLastRun()
        {
            File.WriteAllText(filePath, DateTime.UtcNow.ToString());
        }

        public static DateTime? GetLastRun()
        {
            if (File.Exists(filePath))
            {

                if (DateTime.TryParse(File.ReadAllText(filePath), out var lastRun))
                {
                    return lastRun;
                }
            }
            return null;
        }

        public static bool ShouldRun(DateTime refreshTime)
        {
            var lastRun = GetLastRun();

            return lastRun == null || lastRun < refreshTime;
        }
    }
}
