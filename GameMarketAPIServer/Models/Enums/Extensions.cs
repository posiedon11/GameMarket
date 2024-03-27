using GameMarketAPIServer.Services;
using static GameMarketAPIServer.Services.XblAPIManager;
using static GameMarketAPIServer.Services.DataBaseManager;
using GameMarketAPIServer.Configuration;

namespace GameMarketAPIServer.Models.Enums
{
    public static class Extensions
    {
        public static string To_String(this XblAPIManager.APICalls apiCall)
        {
            switch (apiCall)
            {
                case APICalls.newGames: return "https://xbl.io/api/v2/marketplace/new";
                case APICalls.topGames: return "https://xbl.io/api/v2/marketplace/top-paid";
                case APICalls.bestGames: return "https://xbl.io/api/v2/marketplace/best-rated";
                case APICalls.comingSoonGames: return "https://xbl.io/api/v2/marketplace/coming-soon";
                case APICalls.freeGames: return "https://xbl.io/api/v2/marketplace/top-free";
                case APICalls.deals: return "https://xbl.io/api/v2/marketplace/deals";
                case APICalls.mostPlayedGames: return "https://xbl.io/api/v2/marketplace/most-played";
                case APICalls.marketDetails: return "https://xbl.io/api/v2/marketplace/details";
                case APICalls.gameTitle: return "https://xbl.io/api/v2/marketplace/title";
                case APICalls.playerTitleHistory: return "https://xbl.io/api/v2/player/titleHistory";
                case APICalls.searchPlayer: return "https://xbl.io/api/v2/search";
                case APICalls.checkAPILimit: return "https://xbl.io/api/v2/account";
                default: return "";
            }
        }
        public static bool IsPost(this XblAPIManager.APICalls apicall)
        {
            switch(apicall)
            {
                case APICalls.marketDetails:
                    return true;
                default: return false;
            }
        }

        public static string To_String(this StmAPIManager.APICalls apiCall) 
        {
            switch (apiCall)
            {
                case StmAPIManager.APICalls.getAppListv1: return "https://api.steampowered.com/IStoreService/GetAppList/v1/?key=";
                case StmAPIManager.APICalls.getAppListv2: return "https://api.steampowered.com/ISteamApps/GetAppList/v2/?key=";
                case StmAPIManager.APICalls.getAppDetails: return "https://store.steampowered.com/api/appdetails?cc=us&appids=";


                default: return "";
            }
        }
        public static string To_String(this GameMergerManager.SpecialGroupCases specialCase)
        {
            switch(specialCase)
            {
                case GameMergerManager.SpecialGroupCases.fission: return "[Fission] ";
                case GameMergerManager.SpecialGroupCases.SEJ: return "SEJ_";
                case GameMergerManager.SpecialGroupCases.PCGAMEPass: return "PC & Game Pass";
                default:return "";
            }
        }

        private static string To_String(this DataBaseManager.Schemas schema)
        {
            switch(schema)
            {
                case Schemas.xbox: return Settings.Instance.sqlServerSettings.xboxSchema;
                case Schemas.steam: return Settings.Instance.sqlServerSettings.steamSchema;
                case Schemas.gamemarket: return Settings.Instance.sqlServerSettings.gamemarketSchema;

                default: return "";
            }
        }
    }
}
