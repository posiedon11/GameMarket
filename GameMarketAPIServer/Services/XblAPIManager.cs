
using GameMarketAPIServer.Configuration;
using GameMarketAPIServer.Models;
using GameMarketAPIServer.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using static GameMarketAPIServer.Services.XblAPIManager;
using static SteamKit2.Internal.CMsgCellList;
using GameMarketAPIServer.Models.Enums;
using Extensions = GameMarketAPIServer.Models.Enums.Extensions;
using System.Linq;
using MySqlConnector;
using System.Diagnostics;
using SteamKit2.Internal;
using Microsoft.Extensions.Options;
using GameMarketAPIServer.Utilities;
using AutoMapper;
using Microsoft.Extensions.Logging;
using static SteamKit2.Internal.CMsgBluetoothDevicesData;
using GameMarketAPIServer.Models.Contexts;
using System;
using ProtoBuf.Meta;
using static GameMarketAPIServer.Models.DataBaseSchemas;
using static Microsoft.EntityFrameworkCore.Scaffolding.TableSelectionSet;

namespace GameMarketAPIServer.Services
{
    public class XblAPIManager : APIManager<DataBaseSchemas.XboxSchema>
    {
        //For Games on xbox, use https://www.xbox.com/en-US/games/store/somenamedoesntmatter/productid
        //for games that arent on a xbox device, so pc/mobile, use https://apps.microsoft.com/detail/productid?hl=en-us&gl=U

        private readonly IServiceScopeFactory scopeFactory;
        //private readonly DataBaseService dbService;
        protected XboxSettings xboxSettings;
        protected XblAPITracker apiTracker;
        private readonly IMapper mapper;

        private Dictionary<string, XboxGameTitleData> titleDataQueue = new Dictionary<string, XboxGameTitleData>();
        private Dictionary<string, XboxTitleDetailsData> groupDetailsQueue = new Dictionary<string, XboxTitleDetailsData>();
        private Dictionary<string, DataBaseSchemas.XboxSchema.GameTitleTable> gameTitlesQueue = new Dictionary<string, DataBaseSchemas.XboxSchema.GameTitleTable>();


        private readonly SemaphoreSlim titleDataLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim groupDetailsLock = new SemaphoreSlim(1, 1);

        public enum APICalls
        {
            checkAPILimit,
            newGames,
            topGames,
            bestGames,
            comingSoonGames,
            freeGames,
            deals,
            mostPlayedGames,
            marketDetails,
            gameTitle,
            playerTitleHistory,
            searchPlayer
        }

        public XblAPIManager(IOptions<MainSettings> settings, XblAPITracker apiTracker,
            ILogger<XblAPIManager> logger, IHttpClientFactory httpClientFactory, IServiceScopeFactory scopeFactory, IMapper mapper) : base(settings, logger, DataBaseSchemas.Xbox, httpClientFactory)
        {
            this.xboxSettings = settings.Value.xboxSettings;
            this.apiTracker = apiTracker;
            this.apiTracker.resetHourlyRequest();
            //this.dbService = dbService;
            this.mapper = mapper;
            this.scopeFactory = scopeFactory;

            //this.xboxSettings.resetHourlyRequest();


        }




        //custom funcitons
        private async Task<bool> checkAPILimit()
        {
            string response = await CallAPIAsync((int)APICalls.checkAPILimit, checkHeaders);
            apiTracker.makeRequest(XblAPITracker.hourlyAPICallsRemaining.extra);
            return (apiTracker.remainingAPIRequests > 0);
            //return await ParseJsonAsync((int)APICalls.checkAPILimit, response);
        }
        //Overridefunctions
        protected override async Task RunAsync()
        {
            try
            {

                await checkAPILimit();
                int loopRan = 0;
                Stopwatch apiResetStopWatch = new Stopwatch();
                while (running && !mainCTS.Token.IsCancellationRequested)
                {
                    apiTracker.repartitionHourlyRequests();
                    apiTracker.outputRemainingRequests();


                    await scanAllPlayerHistories();

                    await scanAllGameTitles();


                    await scanAllProductIds();


                    //Sleep Loop
                    try
                    {
                        logger.LogInformation("\n\nXbox Manager sleeping");
                        logger.LogTrace("loop number: " + loopRan++);
                        apiResetStopWatch.Stop();
                        logger.LogInformation("Will Start Again at: " + (DateTime.Now + TimeSpan.FromHours(1) - apiResetStopWatch.Elapsed));
                        await Task.Delay(TimeSpan.FromHours(1) - apiResetStopWatch.Elapsed, mainCTS.Token);
                        apiTracker.resetHourlyRequest();

                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                throw;
            }

        }

        public override string ToStringAsync(int apiCall)
        {
            throw new NotImplementedException();
        }

        #region New Stuff
        public async override Task<(bool, ICollection<ITable>?)> ParseJsonAsync(int apiCall, string json)
        {

            switch ((APICalls)apiCall)
            {
                case APICalls.newGames:
                case APICalls.topGames:
                case APICalls.bestGames:
                case APICalls.comingSoonGames:
                case APICalls.freeGames:
                case APICalls.deals:
                case APICalls.mostPlayedGames:
                    return await parseGenericAsync(json);
                case APICalls.marketDetails:
                    return await parseMarketDetailsAsync(json);
                case APICalls.gameTitle:
                    return await parseGameTitleAsync(json);
                case APICalls.playerTitleHistory:
                    return await parsePlayerHistoryAsync(json);
                case APICalls.searchPlayer:

                case APICalls.checkAPILimit:
                    //return parsePlayerAccout(document, operation);
                    break;
            }
            return (false, null);
        }


        private async Task<(bool, ICollection<ITable>?)> parseGenericAsync(string json)
        {
            try
            {
                GenericData? genericData = JsonConvert.DeserializeObject<GenericData>(json);
                if (genericData == null) { return (false, null); }

                foreach (var item in genericData.items)
                {
                    item.output();
                }
                return (false, null);
            }
            catch (Exception ex) { logger.LogDebug(ex.ToString()); return (false, null); }
        }
        private async Task<(bool, ICollection<ITable>?)> parsePlayerHistoryAsync(string json)
        {
            try
            {


                XboxTitleHistory? history = JsonConvert.DeserializeObject<XboxTitleHistory>(json);
                if (history == null) return (false, null);

                history.InitializeJsonData(logger);
                ICollection<ITable> returnList = new List<ITable>();


                foreach (var title in history.titles.Where(title => title.modernTitleId != null && !title.devices.Contains("Win32")))
                {
                    var gameTitle = mapper.Map<DataBaseSchemas.XboxSchema.GameTitleTable>(title);
                    lock (gameTitlesQueue)
                    {
                        gameTitlesQueue.TryAdd(gameTitle.modernTitleID, gameTitle);
                    }
                    returnList.Add(gameTitle);
                }
                return (true, returnList);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());
                throw;
            }
        }

        private async Task<(bool, ICollection<ITable>?)> parseGameTitleAsync(string json)
        {
            try
            {

                XboxMarketDetails? details = JsonConvert.DeserializeObject<XboxMarketDetails>(json);
                if (details == null) { return (false, null); }

                details.InitializeJsonData(logger);
                ICollection<ITable> returnList = new List<ITable>();

                foreach (var product in details.products)
                {
                    //find the titleID
                    if (product.alternateIds.FirstOrDefault(ai => ai.idType == "XboxTitleId") == null)
                    {
                        logger.LogDebug($"No Title ID found for {product.productId}");
                        continue;
                    }


                    var titleDetails = mapper.Map<DataBaseSchemas.XboxSchema.TitleDetailTable>(product);
                    returnList.Add(titleDetails);
                }

                return (true, returnList);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());
                throw;
            }
        }
        private async Task<(bool, ICollection<ITable>?)> parseMarketDetailsAsync(string json)
        {
            try
            {

                XboxMarketDetails? details = JsonConvert.DeserializeObject<XboxMarketDetails>(json);
                if (details == null) { return (false, null); }

                details.InitializeJsonData(logger);
                ICollection<ITable> returnList = new List<ITable>();

                foreach (var product in details.products)
                {
                    //check to see if info is null
                    if (product.localizedProperties == null) continue;
                    if (product.marketProperties == null) continue;
                    if (product.properties == null) continue;
                    if (product.displaySkuAvailabilities == null) continue;
                    if (product.alternateIds == null) continue;

                    //i dont feel like dealing with these
                    if (product.localizedProperties.First().developerName.Count() > 80 || product.localizedProperties.First().publisherName.Count() > 80) continue;


                    var marketDetails = MappingProfile.MapXboxProductToMarketDetail(product);
                    returnList.Add(marketDetails);

                    //await dbService.AddUpdateTable(marketDetails);
                }
                return (true, returnList);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());
                return (false, null);
            }
        }

        public async Task scanAllPlayerHistories(int maxCalls = 5000)
        {
            try
            {
                if (!apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.profile)) { return; }

                var now = DateTime.UtcNow;
                var refreshTime = DateTime.UtcNow - xboxSettings.userProfileUpdateFrequency;

                using var scope = scopeFactory.CreateScope();
                {
                    var dbService = scope.ServiceProvider.GetRequiredService<DataBaseService>();
                    ICollection<DataBaseSchemas.XboxSchema.UserProfileTable>? users = await dbService.SelectXboxUsers(refreshTime);
                    if (users == null || !users.Any()) return;

                    int currentCalls = 0;
                    foreach (var user in users)
                    {
                        if (++currentCalls > maxCalls)
                        {
                            logger.LogInformation("Reached Max call paramater");
                            break;
                        }
                        try
                        {

                            if (apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.profile))
                            {
                                apiTracker.makeRequest(XblAPITracker.hourlyAPICallsRemaining.profile);
                                string historyRespone = await CallAPIAsync((int)APICalls.playerTitleHistory, user.xuid);
                                if (historyRespone == httpRequestFail) break;
                                var (success, tables) = await ParseJsonAsync((int)APICalls.playerTitleHistory, historyRespone);

                                if (!success || tables == null || !tables.Any()) continue;
                                user.lastScanned = now;
                            }
                            else
                            {
                                logger.LogWarning("Cannot request for player history");
                                break;
                            }
                        }
                        catch { logger.LogWarning($"Failed to call for {user.gamertag}  ID: {user.xuid}"); }
                    }

                    List<XboxSchema.GameTitleTable>? titleList;
                    lock (gameTitlesQueue)
                    {
                        titleList = gameTitlesQueue.Values.ToList();

                    }
                    if (titleList == null) return;
                    await dbService.AddUpdateTables(titleList);
                    await dbService.AddUpdateTables(users);
                }


            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());
            }
        }
        public async Task scanAllGameTitles(int maxCalls = 5000)
        {
            if (!apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.title)) { return; }
            try
            {
                var now = DateTime.UtcNow;
                var refreshTime = now - xboxSettings.userProfileUpdateFrequency;

                using var scope = scopeFactory.CreateScope();
                {
                    var dbService = scope.ServiceProvider.GetRequiredService<DataBaseService>();
                    ICollection<DataBaseSchemas.XboxSchema.GameTitleTable>? gameTitles = await dbService.SelectXboxTitles(refreshTime);

                    if (gameTitles == null || !gameTitles.Any()) return;

                    int count = 0;
                    int apiCall = (int)APICalls.gameTitle;
                    ICollection<XboxSchema.TitleDetailTable> successfulList = new List<XboxSchema.TitleDetailTable>();
                    var selectIDs = await dbService.SelectAll<XboxSchema.ProductIDTable>();
                    Dictionary<string, XboxSchema.ProductIDTable> productIDs = new Dictionary<string, XboxSchema.ProductIDTable>();

                    if (selectIDs != null && selectIDs.Any())
                        productIDs = selectIDs.ToDictionary(e => e.productID);
                    foreach (var gameTitle in gameTitles)
                    {
                        if (++count > maxCalls)
                        {
                            logger.LogInformation("Reached Max call paramater");
                            break;
                        }

                        try
                        {
                            if (apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.title))
                            {
                                if (gameTitle == null) continue;
                                apiTracker.makeRequest(XblAPITracker.hourlyAPICallsRemaining.title);
                                string historyRespone = await CallAPIAsync(apiCall, gameTitle.modernTitleID);
                                if (historyRespone == httpRequestFail) break;
                                var (success, tableData) = await ParseJsonAsync(apiCall, historyRespone);

                                if (processGameTitleReturn(gameTitle, success, tableData))
                                {
                                    foreach (var table in tableData)
                                    {
                                        if (table is XboxSchema.TitleDetailTable titleTable)
                                        {
                                            if (productIDs.ContainsKey(titleTable.productID))
                                                titleTable.ProductIDNavig = productIDs[titleTable.productID];
                                            if (titleTable.GameBundles != null && titleTable.GameBundles.Any())
                                            {
                                                titleTable.GameBundles.ToList().ForEach(e =>
                                                {
                                                    e.TitleDetails = titleTable;
                                                    if (productIDs.ContainsKey(e.relatedProductID))
                                                        e.ProductIDNavig = productIDs[e.relatedProductID];
                                                    else
                                                        e.ProductIDNavig = new XboxSchema.ProductIDTable() { productID = e.relatedProductID};
                                                });
                                            }
                                            successfulList.Add(titleTable);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                logger.LogWarning("Cannot Request for Title Details.");
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning($"Failed to call for {gameTitle.titleName}  ID: {gameTitle.titleID}");
                        }
                    }
                    await dbService.AddUpdateTables(successfulList);
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());

            }
        }
        public async Task scanAllProductIds(int maxCalls = 5000)
        {
            if (!apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.market)) { return; }
            try
            {
                var now = DateTime.UtcNow;
                var refreshTime = now - xboxSettings.marketDetailsUpdateFrequency;

                using (var scope = scopeFactory.CreateScope())
                {
                    var dbService = scope.ServiceProvider.GetRequiredService<DataBaseService>();
                    ICollection<XboxSchema.ProductIDTable>? productIDs = await dbService.SelectProductIDs(refreshTime);
                    //var table = dbService.GetTable(DataBaseSchemas.XboxSchema.ProductIDs);
                    if (productIDs == null || !productIDs.Any()) return;

                    int currentCount = 0;
                    var paramaters = new Dictionary<string, object>();

                    ICollection<XboxSchema.ProductIDTable> currentProductIds = new List<XboxSchema.ProductIDTable>();
                    HashSet<XboxSchema.MarketDetailTable> successfulList = new HashSet<XboxSchema.MarketDetailTable>();

                    while (productIDs.Count() > 0)
                    {
                        if (currentCount >= maxCalls)
                        {
                            logger.LogInformation("Reached max call limit:");
                            return;
                        }

                        if (!apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.market)) return;

                        //currentProductIds.que(productIDs);
                        currentProductIds.Add(productIDs.First());
                        productIDs.Remove(productIDs.First());
                        if (currentProductIds.Count() == xboxSettings.maxProductsForMarketDetails || productIDs.Count() == 0)
                        {
                            try
                            {
                                apiTracker.makeRequest(XblAPITracker.hourlyAPICallsRemaining.market);
                                paramaters.Add("products", currentProductIds.Select(e => e.productID).ToList());

                                string historyRespone = await CallAPIAsync((int)APICalls.marketDetails, JsonConvert.SerializeObject(paramaters));

                                if (historyRespone == httpRequestFail)
                                {
                                    currentProductIds.Clear();
                                    paramaters.Clear();
                                    break;
                                }


                                var (success, returnList) = await ParseJsonAsync((int)APICalls.marketDetails, historyRespone);

                                if (!success || returnList == null || !returnList.Any()) continue;

                                if (processMarketDetails(currentProductIds, success, returnList))
                                {
                                    if (!returnList.All(d => d is XboxSchema.MarketDetailTable)) continue;
                                    var marketData = returnList.Cast<XboxSchema.MarketDetailTable>().ToList();

                                    await dbService.AddUpdateTables(marketData);

                                }
                                currentProductIds.Clear();
                                paramaters.Clear();
                                ++currentCount;
                            }
                            catch (Exception ex)
                            {
                                logger.LogWarning(ex.ToString());
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                throw;
            }
        }

        #endregion

        public bool processGameTitleReturn(XboxSchema.GameTitleTable gameTitle, bool success, ICollection<ITable>? tableData)
        {
            if (!success || tableData == null || !tableData.Any())
            { return false; }

            ICollection<ITable> successfulList = new List<ITable>();

            gameTitle.lastScanned = DateTime.UtcNow;
            foreach (var table in tableData)
            {
                if (table == null) continue;

                if (table is XboxSchema.TitleDetailTable titleTable)
                {
                    titleTable.GameTitle = gameTitle;
                    successfulList.Add(titleTable);
                }
            }
            return true;
        }

        public bool processMarketDetails(ICollection<XboxSchema.ProductIDTable> productIDs, bool success, ICollection<ITable>? tableData)
        {
            if (!success || tableData == null || !tableData.Any())
            { return false; }

            ICollection<ITable> successfulList = new List<ITable>();
            if (!tableData.All(d => d is XboxSchema.MarketDetailTable)) return false;
            var marketData = tableData.Cast<XboxSchema.MarketDetailTable>().ToList();
            foreach (var table in marketData)
            {
                if (table == null) continue;

                table.ProductIDNavig = productIDs.Where(p => p.productID == table.productID).First();
                successfulList.Add(table);
            }
            productIDs.Select(c => c.lastScanned = DateTime.UtcNow).ToList();
            return true;
        }


        #region XboxURLStuff


        public override string GetUrlAsync(int apiCall, string paramaters = "")
        {
            APICalls call = (APICalls)apiCall;
            switch (call)
            {
                //no possible paramaters
                case APICalls.newGames:
                case APICalls.topGames:
                case APICalls.bestGames:
                case APICalls.comingSoonGames:
                case APICalls.freeGames:
                case APICalls.deals:
                case APICalls.mostPlayedGames:
                    return call.To_String();

                //possible paramaters
                case APICalls.playerTitleHistory: return paramaters == "" ? call.To_String() : call.To_String() + "/" + paramaters;

                // necessary paramaters
                case APICalls.marketDetails: return "https://xbl.io/api/v2/marketplace/details";

                case APICalls.gameTitle:
                case APICalls.searchPlayer:
                    return paramaters == "" ? errorURLNoParam : call.To_String() + "/" + paramaters;
                case APICalls.checkAPILimit:
                    return call.To_String();
                default:
                    return errorURLCallDoesntExist;

            }

        }
        protected override void CreateDefaultHeaders(IOptions<MainSettings> settings)
        {
            httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            httpClient.DefaultRequestHeaders.Add("x-authorization", settings.Value.xboxSettings.apiKey);
        }

        protected override void AddAdditionalHeaders(HttpRequestMessage request, int apiCall, string paramaters, string payload)
        {
            if (xboxSettings.outputSettings.outputDebug)
                logger.LogTrace("Adding Headers");

            if (paramaters == "" || paramaters == checkHeaders)
                return;

            if (paramaters != noPayload)
            {
                //request.Headers.Add("Content-Type", "application/json");
            }
            return;
        }

        protected override void HandleHttpClientHeaders(HttpHeaders headers, int apiCall, string paramaters)
        {
            if (settings.outputHTTPResponse)
            {
                foreach (var header in headers)
                {
                    logger.LogTrace(header.Key);
                }
            }

            //IEnumerable<string> values;


            if (paramaters == checkHeaders)
            {
                if ((headers.TryGetValues("X-RateLimit-Limit", out var rateMax)) &&
                        (headers.TryGetValues("X-RateLimit-Used", out var rateUsed)))
                {
                    if (int.TryParse(rateMax.FirstOrDefault(), out var max))
                    {
                        apiTracker.setMaxCAlls(max);
                    }
                    if (int.TryParse(rateUsed.FirstOrDefault(), out var used))
                    {
                        apiTracker.setRemaingCalls(max - used);
                    }

                }
            }

        }

        protected override string PostPayload(int apiCall, string paramaters)
        {
            APICalls call = (APICalls)apiCall;
            if (!call.IsPost()) { return noPayload; }
            if (paramaters == "" || paramaters == checkHeaders)
                return noPayload;
            else
            {

                try
                {
                    var deserializedParamaters = JsonConvert.DeserializeObject<Dictionary<string, object>>(paramaters);
                    if (deserializedParamaters == null) return noPayload;

                    string payload = "";
                    var payloadDicitonary = new Dictionary<string, object>();
                    foreach (var paramater in deserializedParamaters)
                    {
                        if (paramater.Value is Newtonsoft.Json.Linq.JArray jsonArray)
                        {
                            // string paramatersString = "";
                            switch (paramater.Key)
                            {
                                case "products":

                                    string productsString = "";
                                    var productsList = jsonArray.ToObject<List<string>>();
                                    if (productsList == null) break;
                                    foreach (var product in productsList)
                                    {
                                        productsString += product;
                                        if (product != productsList.Last()) productsString += ",";
                                    }
                                    //payloadDicitonary.Add(paramater.Key, jsonArray);
                                    payloadDicitonary.Add(paramater.Key, productsString);
                                    break;
                                default: break;
                            }

                        }
                    }
                    payload = JsonConvert.SerializeObject(payloadDicitonary);
                    if (payload == null) return noPayload;
                    return payload;
                }
                catch (Exception)
                {

                    return "{\"products\" : \"" + paramaters + "\"}";
                }

            }
        }
        #endregion
    }
}
