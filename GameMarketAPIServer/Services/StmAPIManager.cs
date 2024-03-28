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

namespace GameMarketAPIServer.Services
{
    public class StmAPIManager : APIManager
    {
        protected SteamSettings stmSettings;
        protected StmAPITracker apiTracker;
        protected readonly List<string> validAppTypes = new List<string>
        {
            "game",
            "dlc"
        };




        public enum APICalls
        {
            getAppListv1,
            getAppListv2,
            getAppDetails,
            getPackageDetaisl
        }





        public StmAPIManager(IDataBaseManager dbManager, IOptions<MainSettings> settings, StmAPITracker apiTracker) : base(dbManager, settings, "steam")
        {

            stmSettings = settings.Value.steamSettings;
            this.apiTracker = apiTracker;
        }


        protected override async Task RunAsync()
        {
            while (running && !mainCTS.Token.IsCancellationRequested)
            {
#if false
                UInt32 maxREsponse = 50000;
                var paramaters = new Dictionary<string, object>
                {
                    {"include_games", true },
                   // {"include_dlc", false },
                   // {"include_software", false },
                    //{"include_videos", false },
                   // {"include_hardware", false },
                    //{"last_appid", 501310},
                    {"max_results", maxREsponse }
                };
                string response = await CallAPIAsync((int)APICalls.getAppListv1, JsonConvert.SerializeObject(paramaters));
                await ParseJsonAsync((int)APICalls.getAppListv1, response); 
#endif
                //Fetch all app list
#if false
                string response = await CallAPIAsync((int)APICalls.getAppListv2);
                await ParseJsonAsync((int)APICalls.getAppListv2, response); 
#endif

                //Single app Testing
#if false
                string response = await CallAPIAsync((int)APICalls.getAppDetails, "440");
                await ParseJsonAsync((int)APICalls.getAppDetails, response);
                await dbManager.processQueueAsync();
#endif
                //Call and Parse All AppIds
#if true
                await ScanAllAppDetailsAsync();
                await dbManager.processSteamQueueAsync();
#endif
                try
                {
                    Console.WriteLine("\n\nSteam manager Sleeping");
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
                                            url += "&max_results==" + item.Value;
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

        public async override Task<(bool, List<TableData>)> ParseJsonAsync(int apiCall, string json)
        {
            switch ((APICalls)apiCall)
            {
                case APICalls.getAppDetails:
                    return await ParseAppDetails(json);
                case APICalls.getAppListv2:
                case APICalls.getAppListv1:
                    return await ParseAppListAsync(json);
                default:
                    return (false, new List<TableData>());
            }
        }

        public async Task<(bool, List<TableData>)> ParseAppListAsync(string json)
        {

            try
            {
                //SteamAppListMain can store data for v1 or v2 of the call.
                List<TableData> returnList = new List<TableData>();
                SteamAppListMain appList = JsonConvert.DeserializeObject<SteamAppListMain>(json);
                if (appList == null) return (false, returnList);

                //this is used for v1 of the call.
                if (appList.response != null)
                {
                    Console.WriteLine("Null apps: " + appList.response.apps.Count(item => item == null));
                    int nullcount = 0;
                    foreach (var app in appList.response.apps)
                    {

                        if (app == null) continue;
                        SteamAppListData appData = new SteamAppListData();


                        if (app.name == null) { ++nullcount; continue; }
                        appData.appid = app.appid;
                        appData.name = app.name;
                        if (stmSettings.outputSettings.outputDebug)
                            appData.outputData();
                        await dbManager.EnqueueSteamQueueAsync(Tables.SteamAppIDs, appData, CRUD.Create);
                        returnList.Add(appData);
                    }
                    Console.WriteLine("null app names: " + nullcount);
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
                        Console.WriteLine("\n\nNested Search for app list. \n\n");
                        string response = await CallAPIAsync((int)APICalls.getAppListv1, JsonConvert.SerializeObject(paramaters));
                        await ParseJsonAsync((int)APICalls.getAppListv1, response);

                    }
                    else
                    {
                        Console.WriteLine("No more results to add");
                        await dbManager.processSteamQueueAsync();
                    }
#endif
                }

                //this is used for v2 of the call
                else if (appList.appList != null)
                {
                    Console.WriteLine("Total apps: " + appList.appList.apps.Count());
                    Console.WriteLine("Null apps: " + appList.appList.apps.Count(item => item == null));

                    int nullcount = 0;
                    foreach (var app in appList.appList.apps)
                    {
                        if (app == null) continue;
                        SteamAppListData appData = new SteamAppListData();


                        if (app.name == null) { ++nullcount; continue; }

                        appData.appid = app.appid;
                        appData.name = app.name;


                        if (stmSettings.outputSettings.outputAll)
                            appData.outputData();
                        await dbManager.EnqueueSteamQueueAsync(Tables.SteamAppIDs, appData, CRUD.Create);
                        returnList.Add(appData);
                        //app.output();
                    }
                    Console.WriteLine("null app names: " + nullcount);
                    await dbManager.processSteamQueueAsync();
                }



                return (true, returnList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (false, new List<TableData>());
            }
        }

        public async Task<(bool, List<TableData>)> ParseAppDetails(string json)
        {
            try
            {
                var appDetailsList = JsonConvert.DeserializeObject<Dictionary<string, SteamAppDetails>>(json);
                var returnList = new List<TableData>();
                if (appDetailsList == null) return (false, returnList);

                foreach (var item in appDetailsList)
                {
                    SteamAppDetails appDetails = item.Value;
                    if (appDetails == null) return (false, returnList);
                    if (appDetails.success == false)
                    {
                        Console.WriteLine("API failed");
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
                        Console.WriteLine("Game isnt out yet. ");
                        continue;
                    }

                    //only free games should not have a price listed. otherwise, its probably some test/early app.
                    if (!appDetails.data.is_free && appDetails.data.price_overview == null)
                    {
                        Console.WriteLine("App not free, cant find price. ");
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
                        Console.WriteLine("App has no platforms. ");
                        return (false, returnList);
                    }




                    //Each valid app will insert into these tables.
                    await dbManager.EnqueueSteamQueueAsync(Tables.SteamAppDetails, detailsData, CRUD.Create);


                    //Insert into dev and publ
                    if (detailsData.developers.Count > 0)
                        await dbManager.EnqueueSteamQueueAsync(Tables.SteamAppDevelopers, detailsData, CRUD.Create);
                    if (detailsData.publishers.Count > 0)
                        await dbManager.EnqueueSteamQueueAsync(Tables.SteamAppPublishers, detailsData, CRUD.Create);

                    //Not all apps will insert into these.
                    if (detailsData.packages.Count() > 0)
                    {
                        await dbManager.EnqueueSteamQueueAsync([Tables.SteamPackageIDs, Tables.SteamPackages], detailsData, CRUD.Create);
                    }

                    if (detailsData.platforms.Count() > 0)
                        await dbManager.EnqueueSteamQueueAsync(Tables.SteamAppPlatforms, detailsData, CRUD.Create);

                    if (detailsData.genres.Count() > 0)
                        await dbManager.EnqueueSteamQueueAsync(Tables.SteamAppGenres, detailsData, CRUD.Create);

                    returnList.Add(detailsData);
                }

                return (true, returnList);
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); return (false, new List<TableData>()); }
        }


        public async Task<bool> ScanAllAppDetailsAsync(int maxCalls = 500)
        {
            try
            {
                var now = DateTime.UtcNow;
                var refreshTime = DateTime.UtcNow - stmSettings.allAppUpdateFrequency;

                using var connection = new MySqlConnection(dbManager.connectionString);
                await connection.OpenAsync();


                Tables table = Tables.SteamAppIDs;
                if (await dbManager.validTableAsync(table))
                {
                    string sql = $@"Select appID from {DataBaseManager.Schemas.steam}.{table.To_String()} 
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
                            Console.WriteLine("Reached mas call paramater");
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
                                    await dbManager.processSteamQueueAsync();
                                    // Console.WriteLine("Time to wait: " + (stmSettings.apiRequestTimer - maxCallTimer.Elapsed));
                                    Console.WriteLine("Can Call Again at: " + (DateTime.UtcNow + stmSettings.apiRequestTimer - maxCallTimer.Elapsed));
                                    Console.WriteLine("Current Loop: " + ++currentLoop);
                                    await Task.Delay(stmSettings.apiRequestTimer - maxCallTimer.Elapsed);

                                    maxCallTimer.Restart();
                                    curentCallLimit = 0;
                                }
                                apiTracker.makeRequest();

                                string appDetails = await CallAPIAsync((int)APICalls.getAppDetails, appId.ToString());
                                if (appDetails != httpRequestFail)
                                    await ParseJsonAsync((int)APICalls.getAppDetails, appDetails);
                                else continue;

                                await dbManager.EnqueueSteamQueueAsync(Tables.SteamAppIDs, new SteamUpdateScannedData { ID = appId }, CRUD.Update); ;

                                //This loop runs at max once every 1.5 sec
#if false
                                loopDurationTimer.Stop();
                                await Task.Delay(rateLimit - loopDurationTimer.Elapsed);
                                loopDurationTimer.Restart(); 
#endif
                            }
                            else
                            {
                                Console.WriteLine("Cannot request for appdetails");
                            }
                        }
                        catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                    }
                }
                else return false;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); return false;
            }
        }

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
