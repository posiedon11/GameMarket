using GameMarketAPIServer.Configuration;
using System.Net.Http.Headers;
using SteamKit2;
using GameMarketAPIServer.Models;
using Newtonsoft.Json;
using GameMarketAPIServer.Models.Enums;
using Microsoft.VisualBasic;
using System.Reflection.Metadata;
using MySqlConnector;
using System.Diagnostics;
using System.Data;
using Microsoft.Extensions.Options;
using AutoMapper;
using System.Linq;
using static GameMarketAPIServer.Models.DataBaseSchemas;
using Microsoft.Extensions.Logging;
using static GameMarketAPIServer.Utilities.SteamAllAppLastRun;
namespace GameMarketAPIServer.Services
{
    public class StmAPIManager : APIManager<DataBaseSchemas.SteamSchema>
    {
        protected SteamSettings stmSettings;
        protected StmAPITracker apiTracker;
        protected readonly List<string> validAppTypes = new List<string>
        {
            "game",
            "dlc"
        };
        private readonly IMapper mapper;
        private readonly IServiceScopeFactory scopeFactory;



        public enum APICalls
        {
            getAppListv1,
            getAppListv2,
            getAppDetails,
            getPackageDetaisl
        }

        public StmAPIManager(IOptions<MainSettings> settings, StmAPITracker apiTracker,
            ILogger<StmAPIManager> apiLogger, IHttpClientFactory httpClientFactory, IServiceScopeFactory scopeFactory, IMapper mapper) : base(settings, apiLogger, DataBaseSchemas.Steam, httpClientFactory)
        {

            stmSettings = settings.Value.steamSettings;
            this.apiTracker = apiTracker;
            this.mapper = mapper;
            this.scopeFactory = scopeFactory;
        }


        protected override async Task RunAsync()
        {
            while (running && !mainCTS.Token.IsCancellationRequested)
            {
                var refreshTime = DateTime.UtcNow - stmSettings.allAppUpdateFrequency;
                if (ShouldRun(refreshTime))
                {
                    SaveLastRun();
                    if (apiTracker.canRequest())
                    {
                        apiTracker.makeRequest();
                        await GetAppList();
                    }

                }

                //Call and Parse All AppIds
#if true
                await ScanAllAppDetailsAsync(50000);
#endif

                try
                {
                    logger.LogDebug("\n\nSteam manager Sleeping");
                    await Task.Delay(TimeSpan.FromHours(1), mainCTS.Token);
                }
                catch (TaskCanceledException) { break; }
            }
        }
        public override string GetUrlAsync(int apiCall, string paramater = "")
        {
            APICalls call = (APICalls)apiCall;
            switch (call)
            {
                case APICalls.getAppListv1:
                    {
                        if (paramater == "")
                            return call.To_String() + stmSettings.apiKey;
                        else
                        {
                            var paramaters = JsonConvert.DeserializeObject<Dictionary<string, object>>(paramater);
                            if (paramaters == null)
                                return call.To_String() + stmSettings.apiKey;

                            string url = call.To_String() + stmSettings.apiKey;

                            foreach (var item in paramaters)
                            {
                                if (item.Value is bool valueBool)
                                {
                                    switch (item.Key)
                                    {
                                        case "include_games":
                                            url += "&include_games=" + valueBool;
                                            break;
                                        case "include_dlc":
                                            url += "&include_dlc=" + valueBool;
                                            break;
                                        case "include_software":
                                            url += "&include_software=" + valueBool;
                                            break;
                                        case "include_videos":
                                            url += "&include_videos=" + valueBool;
                                            break;
                                        case "include_hardware":
                                            url += "&include_hardware=" + valueBool;

                                            break;
                                    }
                                }
                                else if (item.Value is string valueString)
                                {

                                }

                                else if (Information.IsNumeric(item.Value))
                                {
                                    //long valueInt = Convert.ToInt64(item.Value);
                                    switch (item.Key)
                                    {
                                        case "last_appid":
                                            url += "&last_appid=" + item.Value;
                                            break;
                                        case "max_results":
                                            url += "&max_results=" + item.Value;
                                            break;

                                    }
                                }
                            }
                            return url;
                        }
                    }
                case APICalls.getAppListv2:
                    return call.To_String() + paramater;
                case APICalls.getAppDetails:

                    return paramater == "" ? "" : call.To_String() + paramater;
                default:
                    return "";
            }
        }


        #region New Stuff

        public async override Task<(bool, ICollection<ITable>?)> ParseJsonAsync(int apiCall, string json)
        {
            switch ((APICalls)apiCall)
            {
                case APICalls.getAppDetails:
                    return await ParseAppDetails(json);
                case APICalls.getAppListv2:
                case APICalls.getAppListv1:
                    return await ParseAppListAsync(json);
                default:
                    return (false, null);
            }
        }

        private async Task<(bool, ICollection<ITable>?)> ParseAppListAsync(string json)
        {

            try
            {
                //SteamAppListMain can store data for v1 or v2 of the call.
                SteamAppListMain? appListMain = JsonConvert.DeserializeObject<SteamAppListMain>(json);
                ICollection<ITable> returnList = new HashSet<ITable>();
                ICollection<SteamApp> appList = new List<SteamApp>();

                if (appListMain == null) return (false, returnList);

                //this is used for v1 of the call.
                if (appListMain.response != null)
                {
                    logger.LogDebug("Null apps: " + appListMain.response.apps.Count(item => item == null));

                    appList = appListMain.response.apps;
#if true
                    if (appListMain.response.have_more_results)
                    {
                        var paramaters = new Dictionary<string, object>
                        {
                            {"include_games", true },
                            {"include_dlc", true },
                           // {"include_software", false },
                            //{"include_videos", false },
                           // {"include_hardware", false },
                            {"last_appid", appListMain.response.last_appid},
                            {"max_results",  50000}
                        };
                        logger.LogDebug("\n\nNested Search for app list. \n\n");
                        string response = await CallAPIAsync((int)APICalls.getAppListv1, JsonConvert.SerializeObject(paramaters));
                        var (success, newAppList) = await ParseJsonAsync((int)APICalls.getAppListv1, response);
                        if (success && newAppList != null && newAppList.Any())
                        {
                            returnList = newAppList;
                        }
                    }
                    else
                    {
                        logger.LogDebug("No more results to add");
                    }
#endif
                }




                //this is used for v2 of the call
                else if (appListMain.appList != null)
                {
                    logger.LogDebug("Total apps: " + appListMain.appList.apps.Count());
                    logger.LogDebug("Null apps: " + appListMain.appList.apps.Count(item => item == null));
                    appList = appListMain.appList.apps;
                }

                int nullcount = 0;
                foreach (var app in appList)
                {
                    if (app == null) continue;
                    if (app.appID == 292030)
                    {
                        logger.LogDebug("fad");
                    }
                    if (app.name == null || app.name == string.Empty || app.name == "")
                    {
                        ++nullcount;
                        continue;
                    }


                    var appID = mapper.Map<DataBaseSchemas.SteamSchema.AppIDsTable>(app);
                    returnList.Add(appID);
                }
                //returnList.OrderBy(e => ((DataBaseSchemas.SteamSchema.AppIDsTable)e).appID);
                return (true, returnList);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return (false, null);
            }
        }


        private async Task<(bool, ICollection<ITable>?)> ParseAppDetails(string json)
        {
            try
            {
                var appDetailsList = JsonConvert.DeserializeObject<Dictionary<string, SteamAppDetails>>(json);
                ICollection<ITable> returnList = new List<ITable>();
                if (appDetailsList == null) return (false, null);

                //There should only be one app in the list
                foreach (var item in appDetailsList)
                {
                    SteamAppDetails appDetails = item.Value;
                    if (appDetails == null) return (false, returnList);
                    if (appDetails.success == false)
                    {
                        logger.LogDebug("API failed");
                        return (false, null);
                    }

                    //Only care about games and dlc for this
                    if (!validAppTypes.Contains(appDetails.data.type)) return (false, null);
                    if (appDetails.data.steam_appid == 0) return (false, null);
                    if (appDetails.data.name == null || appDetails.data.name == string.Empty) return (false, null);

                    //Im pretty sure each app needs a dev or a pub
                    if (appDetails.data.developers == null && appDetails.data.developers == null) return (false, null);

                    //If the game isnt out yet, cant doo much with it.
                    //plan on adding a check for it later
                    if (appDetails.data.release_date == null) return (false, null);
                    if (appDetails.data.release_date.coming_soon == true)
                    {
                        logger.LogDebug("Game isnt out yet. ");
                        continue;
                    }
                    //only free games should not have a price listed. otherwise, its probably some test/early app.
                    if (!appDetails.data.is_free && appDetails.data.price_overview == null)
                    {
                        logger.LogDebug("App not free, cant find price. ");
                        continue;
                    }


                    var detailsData = MappingProfile.MapSteamAppDetails(appDetails.data);


                    returnList.Add(detailsData);
                }

                return (true, returnList);
            }
            catch (Exception ex) { logger.LogDebug(ex.ToString()); return (false, null); }
        }


        public async Task<bool> ScanAllAppDetailsAsync(int maxCalls = 500)
        {
            try
            {
                var now = DateTime.UtcNow;
                var refreshTime = DateTime.UtcNow - stmSettings.allAppUpdateFrequency;

                using var scope = scopeFactory.CreateScope();
                {
                    var dbService = scope.ServiceProvider.GetRequiredService<DataBaseService>();

                    var appIDs = await dbService.SelectAppIDs(refreshTime);
                    if (appIDs == null || !appIDs.Any()) return false;


                    int currentCalls = 0;
                    //For steam web api, you can call it at max 200 times every 5 minutes, or once every 1.5 sec;
                    ICollection<SteamSchema.AppDetailsTable> successList = new HashSet<SteamSchema.AppDetailsTable>();
                    ICollection<SteamSchema.AppIDsTable> badList = new HashSet<SteamSchema.AppIDsTable>();
                    Dictionary<UInt32, SteamSchema.AppDetailsTable> successDict = new Dictionary<UInt32, SteamSchema.AppDetailsTable>();

                    foreach (var appId in appIDs)
                    {
                        //loopDurationTimer.Start();
                        if (++currentCalls > maxCalls)
                        {
                            logger.LogDebug("Reached mas call paramater");
                            break;
                        }
                        try
                        {
                            if (!apiTracker.canRequest())
                            {
                                await dbService.AddUpdateTables(successDict.Values.ToList());
                                successDict.Clear();
                                //await dbService.AddUpdateTables(successList);
                                //successList.Clear();
                                await apiTracker.waitForReset();
                            }

                            var appDetails = await ScannAppDetailsAsync(appId.appID);
                            appId.lastScanned = now;
                            if (appDetails != null && appDetails.Any())
                            {
                                var details = appDetails.Cast<SteamSchema.AppDetailsTable>().ToList();
                                foreach (var detail in details)
                                {
                                    if (detail.appID == appId.appID)
                                        detail.AppIDNavigation = appId;
                                    else
                                        detail.AppIDNavigation = new SteamSchema.AppIDsTable() { appID = detail.appID };

                                    detail.DLCs.ToList().ForEach(dlc => dlc.AppIDNavigation = appId);
                                    //successList.Add(detail);
                                    if (!successDict.ContainsKey(detail.appID))
                                        successDict.Add(detail.appID, detail);
                                    else
                                    {
                                        logger.LogDebug("Duplicate appID: " + detail.appID);
                                    }


                                }
                            }
                            else
                            {
                                badList.Add(appId);
                            }

                        }
                        catch (Exception ex) 
                        { 
                            logger.LogDebug(ex.ToString());
                        }
                    }
                    await dbService.AddUpdateTables(successDict.Values.ToList());
                    //await dbService.AddUpdateTables(successList);
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());
                return false;
            }
        }

        public async Task<ICollection<ITable>?> ScannAppDetailsAsync(UInt32 appID)
        {
            try
            {
                if (apiTracker.canRequest())
                {
                    apiTracker.makeRequest();

                    string appDetails = await CallAPIAsync((int)APICalls.getAppDetails, appID.ToString());
                    if (appDetails != httpRequestFail)
                    {
                        var (success, retunList) = await ParseJsonAsync((int)APICalls.getAppDetails, appDetails);
                        if (success && retunList != null && retunList.Any())
                            return retunList;
                    }
                    else 
                        return null;

                }
                else
                {
                    logger.LogDebug("Cannot request for appdetails");
                }
                return null;
            }
            catch (Exception ex) { logger.LogDebug(ex.ToString()); return null; }
        }

        public async Task GetAppList(bool useV2 = true)
        {
            try
            {
                var now = DateTime.UtcNow;
                var refreshTime = DateTime.UtcNow - stmSettings.allAppUpdateFrequency;

                using var scope = scopeFactory.CreateScope();
                {
                    var dbService = scope.ServiceProvider.GetRequiredService<DataBaseService>();

                    string response;
                    if (!useV2)
                    {
                        var paramaters = new Dictionary<string, object>
                        {
                            {"include_games", true },
                            {"include_dlc", true },
                           // {"include_software", false },
                            //{"include_videos", false },
                           // {"include_hardware", false },
                            {"max_results",  50000}
                        };
                        response = await CallAPIAsync((int)APICalls.getAppListv1, JsonConvert.SerializeObject(paramaters));
                        var (success, data) = await ParseJsonAsync((int)APICalls.getAppListv1, response);
                        if (success && data != null && data.Any())
                        {
                            var appIds = data.Cast<SteamSchema.AppIDsTable>().ToList();
                            await dbService.AddUpdateTables(appIds);
                        }
                    }
                    else
                    {
                        response = await CallAPIAsync((int)APICalls.getAppListv2);
                        var (success, data) = await ParseJsonAsync((int)APICalls.getAppListv2, response);
                        if (success && data != null && data.Any())
                        {
                            var appIds = data.Cast<SteamSchema.AppIDsTable>().ToList();
                            await dbService.AddUpdateTables(appIds);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }
        #endregion




        public override string ToStringAsync(int apiCall)
        {
            throw new NotImplementedException();
        }

        protected override void AddAdditionalHeaders(HttpRequestMessage request, int apiCall, string paramater, string payload)
        {

        }

        protected override void CreateDefaultHeaders(IOptions<MainSettings> settings)
        {

        }

        protected override void HandleHttpClientHeaders(HttpHeaders headers, int apiCall, string paramater)
        {

        }


    }
}
