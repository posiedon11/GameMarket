﻿using GameMarketAPIServer.Configuration;
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
                if (true)

                {
                    if (apiTracker.canRequest())
                    {
                        apiTracker.makeRequest();
                        await GetAppList();
                    }

                }

                //Call and Parse All AppIds
#if true
                await ScanAllAppDetailsAsync();
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


#if false

        #region Old stuff
        public async override Task<(bool, List<ITableData>)> ParseJsonAsyncOld(int apiCall, string json)
        {
            switch ((APICalls)apiCall)
            {
                case APICalls.getAppDetails:
                    return await ParseAppDetailsOld(json);
                case APICalls.getAppListv2:
                case APICalls.getAppListv1:
                    return await ParseAppListAsyncOld(json);
                default:
                    return (false, new List<ITableData>());
            }
        }

        public async Task<(bool, List<ITableData>)> ParseAppListAsyncOld(string json)
        {

            try
            {
                //SteamAppListMain can store data for v1 or v2 of the call.
                List<ITableData> returnList = new List<ITableData>();
                SteamAppListMain appList = JsonConvert.DeserializeObject<SteamAppListMain>(json);
                if (appList == null) return (false, returnList);

                //this is used for v1 of the call.
                if (appList.response != null)
                {
                    logger.LogDebug("Null apps: " + appList.response.apps.Count(item => item == null));
                    int nullcount = 0;
                    foreach (var app in appList.response.apps)
                    {

                        if (app == null) continue;
                        SteamAppListData appData = new SteamAppListData();


                        if (app.name == null) { ++nullcount; continue; }
                        appData.appid = app.appID;
                        appData.name = app.name;
                        if (stmSettings.outputSettings.outputDebug)
                            appData.outputData();
                        await dbManager.EnqueQueueAsync(schema.AppIDs, appData, CRUD.Create);
                        returnList.Add(appData);
                    }
                    logger.LogDebug("null app names: " + nullcount);
#if true
                    if (appList.response.have_more_results)
                    {
                        UInt32 maxREsponse = 10000;
                        var paramaters = new Dictionary<string, object>
                        {
                            {"include_games", true },
                           // {"include_dlc", false },
                           // {"include_software", false },
                            //{"include_videos", false },
                           // {"include_hardware", false },
                            {"last_appid", appList.response.last_appid},
                            {"max_results",  50000}
                        };
                        logger.LogDebug("\n\nNested Search for app list. \n\n");
                        string response = await CallAPIAsync((int)APICalls.getAppListv1, JsonConvert.SerializeObject(paramaters));
                        await ParseJsonAsyncOld((int)APICalls.getAppListv1, response);

                    }
                    else
                    {
                        logger.LogDebug("No more results to add");
                        await dbManager.processQueue(schema);
                    }
#endif
                }

                //this is used for v2 of the call
                else if (appList.appList != null)
                {
                    logger.LogDebug("Total apps: " + appList.appList.apps.Count());
                    logger.LogDebug("Null apps: " + appList.appList.apps.Count(item => item == null));

                    int nullcount = 0;
                    foreach (var app in appList.appList.apps)
                    {
                        if (app == null) continue;
                        SteamAppListData appData = new SteamAppListData();


                        if (app.name == null) { ++nullcount; continue; }

                        appData.appid = app.appID;
                        appData.name = app.name;


                        if (stmSettings.outputSettings.outputAll)
                            appData.outputData();
                        await dbManager.EnqueQueueAsync(schema.AppIDs, appData, CRUD.Create);
                        returnList.Add(appData);
                        //app.output();
                    }
                    logger.LogDebug("null app names: " + nullcount);
                    await dbManager.processQueue(schema);
                }



                return (true, returnList);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());
                return (false, new List<ITableData>());
            }
        }

        public async Task<(bool, List<ITableData>)> ParseAppDetailsOld(string json)
        {
            try
            {
                var appDetailsList = JsonConvert.DeserializeObject<Dictionary<string, SteamAppDetails>>(json);
                var returnList = new List<ITableData>();
                if (appDetailsList == null) return (false, returnList);

                foreach (var item in appDetailsList)
                {
                    SteamAppDetails appDetails = item.Value;
                    if (appDetails == null) return (false, returnList);
                    if (appDetails.success == false)
                    {
                        logger.LogDebug("API failed");
                        return (false, returnList);
                    }

                    //Only care about games and dlc for this
                    if (!validAppTypes.Contains(appDetails.data.type)) return (false, returnList);
                    if (appDetails.data.steam_appid == 0) return (false, returnList);
                    if (appDetails.data.name == null) return (false, returnList);

                    //Im pretty sure each app needs a dev or a pub
                    if (appDetails.data.developers == null && appDetails.data.developers == null) return (false, returnList);


                    //If the game isnt out yet, cant doo much with it.
                    //plan on adding a check for it later
                    if (appDetails.data.release_date == null) return (false, returnList);
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



                    SteamAppDetailsData detailsData = new SteamAppDetailsData();

                    //Each app must have these
                    detailsData.appType = appDetails.data.type;
                    detailsData.appName = appDetails.data.name;
                    detailsData.appID = appDetails.data.steam_appid;
                    //not too sure about this one though
                    detailsData.isFree = appDetails.data.is_free;





                    //I dont know if every app has genres, but it shouldnt be a problem
                    detailsData.genres = new List<string>();
                    if (appDetails.data.genres != null)
                        foreach (var genre in appDetails.data.genres)
                            detailsData.genres.Add(genre.description);


                    //Not every app has these
                    if (appDetails.data.packages != null)
                        detailsData.packages = appDetails.data.packages;
                    if (appDetails.data.dlc != null)
                        detailsData.dlcs = appDetails.data.dlc;
                    if (appDetails.data.packages != null)
                        detailsData.packages = appDetails.data.packages;


                    //If its not free, it must have a price. Free games probably wont have this data
                    if (!detailsData.isFree)
                    {
                        detailsData.listprice = appDetails.data.price_overview.final;
                        detailsData.msrp = appDetails.data.price_overview.initial;
                    }

                    //this will check the developers and publishers
                    if (appDetails.data.developers != null && appDetails.data.developers.Count < 1)
                        detailsData.developers = appDetails.data.developers;
                    else
                        detailsData.developers = new List<string>();

                    if (appDetails.data.developers != null && appDetails.data.publishers.Count < 1)
                        detailsData.publishers = appDetails.data.publishers;
                    else
                        detailsData.publishers = new List<string>();


                    //im pretty sure each platforms needs at least one of these
                    detailsData.platforms = new List<string>();
                    if (appDetails.data.platforms.windows)
                        detailsData.platforms.Add("Windows");

                    if (appDetails.data.platforms.mac)
                        detailsData.platforms.Add("Mac");

                    if (appDetails.data.platforms.linux)
                        detailsData.platforms.Add("Linux");

                    if (detailsData.platforms.Count == 0)
                    {
                        logger.LogDebug("App has no platforms. ");
                        return (false, returnList);
                    }




                    //Each valid app will insert into these tables.
                    await dbManager.EnqueQueueAsync(schema.AppDetails, detailsData, CRUD.Create);


                    //Insert into dev and publ
                    if (detailsData.developers.Count > 0)
                        await dbManager.EnqueQueueAsync(schema.AppDevelopers, detailsData, CRUD.Create);
                    if (detailsData.publishers.Count > 0)
                        await dbManager.EnqueQueueAsync(schema.AppPublishers, detailsData, CRUD.Create);

                    //Not all apps will insert into these.
                    if (detailsData.packages.Count() > 0)
                    {
                        await dbManager.EnqueQueueAsync([schema.PackageIDs, schema.Packages], detailsData, CRUD.Create);
                    }

                    if (detailsData.platforms.Count() > 0)
                        await dbManager.EnqueQueueAsync(schema.AppPlatforms, detailsData, CRUD.Create);

                    if (detailsData.genres.Count() > 0)
                        await dbManager.EnqueQueueAsync(schema.AppGenres, detailsData, CRUD.Create);

                    returnList.Add(detailsData);
                }

                return (true, returnList);
            }
            catch (Exception ex) { logger.LogDebug(ex.ToString()); return (false, new List<ITableData>()); }
        }


        public async Task<bool> ScanAllAppDetailsAsyncOld(int maxCalls = 500)
        {
            try
            {
                var now = DateTime.UtcNow;
                var refreshTime = DateTime.UtcNow - stmSettings.allAppUpdateFrequency;

                using var connection = new MySqlConnection(dbManager.connectionString);
                await connection.OpenAsync();


                var table = Database_structure.Steam.AppIDs;
                if (await dbManager.validTableAsync(table))
                {
                    string sql = $@"Select appID from {table.fullPath()} 
                     where lastScanned < @refreshTime or lastScanned IS null 
                     order by lastScanned, appId";
                    var appIDs = new List<UInt32>();

                    using (var command = connection.CreateCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("refreshTime", refreshTime);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                appIDs.Add(reader.GetUInt32(0));
                            }
                        }

                    }
                    await connection.CloseAsync();

                    int currentCalls = 0;
                    int curentCallLimit = 0;
                    int currentLoop = 0;
                    Stopwatch maxCallTimer = new Stopwatch();
                    maxCallTimer.Start();

                    //For steam web api, you can call it at max 200 times every 5 minutes, or once every 1.5 sec;
                    foreach (var appId in appIDs)
                    {
                        //loopDurationTimer.Start();
                        if (++currentCalls > maxCalls)
                        {
                            logger.LogDebug("Reached mas call paramater");
                            break;
                        }
                        //if (maxCallTimer.Elapsed.Minutes > stmSettings.apiRequestTimer.Minutes)
                        //    {
                        //    await dbManager.processQueueAsync();
                        //    maxCallTimer.Restart(); 
                        //}

                        try
                        {
                            if (apiTracker.canRequest())
                            {
                                //check if paramater has 
                                if (++curentCallLimit > stmSettings.maxRequestIn5Minutes)
                                {
                                    await dbManager.processQueue(schema);
                                    // logger.LogDebug("Time to wait: " + (stmSettings.apiRequestTimer - maxCallTimer.Elapsed));
                                    logger.LogDebug("Can Call Again at: " + (DateTime.UtcNow + stmSettings.apiRequestTimer - maxCallTimer.Elapsed));
                                    logger.LogDebug("Current Loop: " + ++currentLoop);
                                    await Task.Delay(stmSettings.apiRequestTimer - maxCallTimer.Elapsed);

                                    maxCallTimer.Restart();
                                    curentCallLimit = 0;
                                }
                                apiTracker.makeRequest();

                                string appDetails = await CallAPIAsync((int)APICalls.getAppDetails, appId.ToString());
                                if (appDetails != httpRequestFail)
                                    await ParseJsonAsyncOld((int)APICalls.getAppDetails, appDetails);
                                else continue;

                                await dbManager.EnqueQueueAsync(schema.AppIDs, new SteamUpdateScannedData { ID = appId }, CRUD.Update); ;

                                //This loop runs at max once every 1.5 sec
#if false
                                loopDurationTimer.Stop();
                                await Task.Delay(rateLimit - loopDurationTimer.Elapsed);
                                loopDurationTimer.Restart(); 
#endif
                            }
                            else
                            {
                                logger.LogDebug("Cannot request for appdetails");
                            }
                        }
                        catch (Exception ex) { logger.LogDebug(ex.ToString()); }
                    }
                }
                else return false;

                return true;
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString()); return false;
            }
        }

        #endregion

#endif


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
                    ICollection<SteamSchema.AppDetailsTable> successList = new List<SteamSchema.AppDetailsTable>();
                    ICollection<SteamSchema.AppIDsTable> badList = new List<SteamSchema.AppIDsTable>();
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
                                await dbService.AddUpdateTables(successList);
                                successList.Clear();
                                await apiTracker.waitForReset();
                            }

                            var appDetails = await ScannAppDetailsAsync(appId.appID);
                            appId.lastScanned = now;
                            if (appDetails != null && appDetails.Any())
                            {
                                var details = appDetails.Cast<SteamSchema.AppDetailsTable>().ToList();
                                foreach (var detail in details)
                                {
                                    successList.Add(detail);
                                }
                            }
                            else
                            {
                                badList.Add(appId);
                            }

                        }
                        catch (Exception ex) { logger.LogDebug(ex.ToString()); }
                    }
                    await dbService.AddUpdateTables(successList);
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString()); return false;
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
