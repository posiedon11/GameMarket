﻿
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

        public XblAPIManager( IOptions<MainSettings> settings, XblAPITracker apiTracker,
            ILogger<XblAPIManager> logger, IHttpClientFactory httpClientFactory, IServiceScopeFactory scopeFactory, IMapper mapper) : base( settings, logger, DataBaseSchemas.Xbox, httpClientFactory)
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
                    await scanAllPlayerHistories();

                    await scanAllGameTitles(20);

                    apiTracker.outputRemainingRequests();

                    await scanAllProductIds(20);


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







#if false
        #region Old Stuff
        public async override Task<(bool, List<ITableData>)> ParseJsonAsyncOld(int apiCall, string json)
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
                    return await parseGenericAsyncOld(json);
                case APICalls.marketDetails:
                    return await parseMarketDetailsAsyncOld(json);
                    break;
                case APICalls.gameTitle:
                    return await parseGameTitleAsyncOld(json);
                case APICalls.playerTitleHistory:
                    return await parsePlayerHistoryAsyncOld(json);
                case APICalls.searchPlayer:

                case APICalls.checkAPILimit:
                    //return parsePlayerAccout(document, operation);
                    break;
            }
            return (false, new List<ITableData>());
        }
        private async Task<(bool, List<ITableData>)> parseGenericAsyncOld(string json)
        {
            try
            {
                GenericData genericData = JsonConvert.DeserializeObject<GenericData>(json);
                if (genericData == null) { return (false, new List<ITableData>()); }

                foreach (var item in genericData.items)
                {
                    item.output();
                }
                return (false, new List<ITableData>());
            }
            catch (Exception ex) { logger.LogDebug(ex.ToString()); return (false, new List<ITableData>()); }
        }
        private async Task<(bool, List<ITableData>)> parseGameTitleAsyncOld(string json)
        {
            try
            {
                var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();

                XboxMarketDetails details = JsonConvert.DeserializeObject<XboxMarketDetails>(json);
                List<ITableData> returnList = new List<ITableData>();
                if (details == null) { return (false, returnList); }
                details.InitializeJsonData(logger);
                details.output(2);
                foreach (var product in details.products)
                {
                    XboxTitleDetailsData data = new XboxTitleDetailsData();


                    foreach (var displaySku in details.products)
                    {

                    }
                    foreach (var localProp in product.localizedProperties)
                    {
                        data.productTitle = localProp.productTitle;
                    }
                    data.productID = product.productId;
                    data.groupID = product.properties.ProductGroupId;
                    data.groupName = product.properties.ProductGroupName;

                    foreach (var marketProperty in product.marketProperties)
                    {
                        foreach (var relatedProduct in marketProperty.relatedProducts)
                        {
                            if (relatedProduct.relationshipType == "Bundle")
                            {
                                data.bundleIDs.Add(relatedProduct.relatedProductId);
                            }
                        }
                    }
                    bool foundID = false;
                    //find the titleID
                    foreach (var altID in product.alternateIds)
                    {
                        if (altID.idType == "XboxTitleId")
                        {
                            foundID = true;
                            data.modernTitleID = altID.value;
                        }
                    }
                    if (foundID == false)
                    {

                        logger.LogDebug("No Title ID found for " + data.productID);
                        continue;
                    }


                    //valid data
                    //data.outputData();

                    //add to details queue
                    if (data.groupName != null && data.groupID != null)
                    {
                        if (data.groupName.Length > 60)
                            logger.LogDebug(data.groupName);
                        await groupDetailsLock.WaitAsync();
                        groupDetailsQueue.TryAdd(data.groupID, data);
                        groupDetailsLock.Release();
                    }

                    var titleDetails = mapper.Map<DataBaseSchemas.XboxSchema.TitleDetailTable>(product);
                    //await dbManager.InsertXboxQueueAsync(Tables.XboxProductIds, data, CRUD.Create);
                    //await dbManager.InsertXboxQueueAsync(Tables.XboxTitleDetails, data, CRUD.Create);
                    returnList.Add(data);
                    await dbManager.EnqueQueueAsync([schema.ProductIDs, schema.TitleDetails, schema.GameBundles], data, CRUD.Create);

                    await dbManager.EnqueQueueAsync(schema.GameTitles, data, CRUD.Update);
                }
                //details.output(9);

                return (true, returnList);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());
                throw;
            }
        }
        private async Task<(bool, List<ITableData>)> parseMarketDetailsAsyncOld(string json)
        {
            try
            {

                XboxMarketDetails? details = JsonConvert.DeserializeObject<XboxMarketDetails>(json);
                List<ITableData> returnList = new List<ITableData>();
                if (details == null) { return (false, new List<ITableData>()); }
                foreach (var product in details.products)
                {
                    XboxGameMarketData data = new XboxGameMarketData();

#if false
                    if (!product.IsSandboxedProduct)
                    {
                        logger.LogDebug("Product Not Sandboxed");
                        continue;
                    }
                    else if (product.SandboxId != "RETAIL")
                    {
                        logger.LogDebug("Product not Retail");
                        continue;
                    } 
#endif

                    //check to see if info is null
                    if (product.localizedProperties == null) continue;
                    if (product.marketProperties == null) continue;
                    if (product.properties == null) continue;
                    if (product.displaySkuAvailabilities == null) continue;

                    if (product.alternateIds == null) continue;

                    data.productID = product.productId;


                    //handle local. Should only be one but idk
                    if (product.localizedProperties.Count > 1) logger.LogDebug("\n\n\nMultiple Local Properties??\n\n");
                    foreach (var localProp in product.localizedProperties)
                    {
                        data.devName = localProp.developerName;
                        data.pubName = localProp.publisherName;
                        data.productTitle = localProp.productTitle;

                        if (localProp.Images != null)
                            foreach (var image in localProp.Images)
                            {
                                //find the poster image
                                if (image == null) continue;
                                if (image.imagePurpose == "Poster")
                                {
                                    data.posterImage = image.uri;
                                }
                            }
                        //might add description later
                    }


                    if (data.posterImage == null)
                    {
                        logger.LogDebug($"{data.productID} has no poster image");
                    }

                    data.isDemo = product.properties.isDemo;
                    //Only thing in market properties i might need is release date
#if true
                    foreach (var marketProperty in product.marketProperties)
                    {
                        if (DateTime.TryParse(marketProperty.OriginalReleaseDate, out var origRelease))
                        {
                            data.releaseDate = origRelease;
                        }
                    }
#endif
                    //valid data
                    //data.outputData();


                    data.purchasable = false;
                    foreach (var displaySkuAvail in product.displaySkuAvailabilities)
                    {
                        //Im only going to deal with the first purchasable sku i find. if its only gamepass, ill add null's to the prices
                        if (data.purchasable)
                            break;
                        //Sku suff if i need it


                        //availabilities
                        int validAvails = 0;
                        foreach (var availability in displaySkuAvail.availabilities)
                        {
                            //find the date for each sku;
                            //if no date range is give, idk what to do with it.
                            if (availability.conditions.startDate == null || availability.conditions.endDate == null) continue;


                            //Get at the first date.
                            if (data.startDate == DateTime.MinValue)
                                if (DateTime.TryParse(availability.conditions.startDate, out var start))
                                    data.startDate = start;

                            if (data.endDate == DateTime.MinValue)
                                if (DateTime.TryParse(availability.conditions.endDate, out var end))
                                    data.endDate = end;

                            //if you cant purchase it, this avail is pointless
                            //There are some gamepass games that cant be purchased though, I will have to deal with that later.
                            if (!availability.actions.Contains("Purchase") && !data.purchasable)
                            {
                                continue;
                            }
                            else
                            {
                                //most likely a gamepass game or some other service
                                if (availability.remediationRequired == true)
                                {
                                    continue;
                                }
                                //This sku should be purchasable
                                else
                                {

                                    if (DateTime.TryParse(availability.conditions.startDate, out var start))
                                        data.startDate = start;

                                    if (DateTime.TryParse(availability.conditions.endDate, out var end))
                                        data.endDate = end;

                                    //The current sale is ongoing.
                                    if (DateTime.UtcNow > data.startDate && DateTime.UtcNow < data.endDate)
                                    {
                                        //The current sku is purchasable
                                        if (availability.orderManagementData.price.currencyCode != null)
                                            data.currencyCode = availability.orderManagementData.price.currencyCode;

                                        if (++validAvails > 1)
                                        {
                                            logger.LogDebug("multiple skus valid??");
                                        }
                                        data.purchasable = true;
                                        data.ListPrice = availability.orderManagementData.price.listPrice;
                                        data.msrp = availability.orderManagementData.price.msrp;

                                        if (availability.conditions.clientConditions.allowedPlatforms != null)
                                            foreach (var platform in availability.conditions.clientConditions.allowedPlatforms)
                                                data.platforms.Add(platform.platformName);
                                        //Only finding the first skew
                                        break;
                                    }

                                }
                            }
                        }
                    }

                    if (!data.purchasable && !product.IsSandboxedProduct)
                    {
                        logger.LogDebug($"{data.productID} is not purchasable and not sandboxed");
                    }
                    returnList.Add(data);
                    await dbManager.EnqueQueueAsync(schema.MarketDetails, data, CRUD.Create);
                }
                //details.output(9);

                return (true, returnList);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());
                return (false, new List<ITableData>());
            }
        }

        private async Task<(bool, List<ITableData>)> parsePlayerHistoryAsyncOld(string json)
        {
            try
            {
                //JObject oo = JObject.Parse(json);
                // logger.LogDebug(oo.ToString());

                if (xboxSettings.outputSettings.outputDebug)
                    logger.LogDebug("Parsing Player history");
                List<ITableData> returnList = new List<ITableData>();

                XboxTitleHistory? history = JsonConvert.DeserializeObject<XboxTitleHistory>(json);
                if (history == null) return (false, returnList);
                history.InitializeJsonData(logger);
                foreach (var title in history.titles.Where(title => title.modernTitleId != null && !title.devices.Contains("Win32")))
                {


                    XboxGameTitleData titleData = new XboxGameTitleData();
                    titleData.titleID = title.titleId;
                    titleData.titleName = title.name;
                    titleData.modernTitleID = title.modernTitleId;
                    titleData.displayImage = title.displayImage;
                    titleData.isGamePass = title.gamePass.isGamePass;
                    titleData.devices = title.devices;


                    var gameTitle = mapper.Map<DataBaseSchemas.XboxSchema.GameTitleTable>(title);
                    await titleDataLock.WaitAsync();
                    titleDataQueue.TryAdd(titleData.modernTitleID, titleData);
                    titleDataLock.Release();


                    returnList.Add(titleData);
                }
                // await dbManager.processQueueAsync();
                //history.output(5);


                return (true, returnList);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());
                throw;
            }
        }
        public async Task scanAllPlayerHistoriesOld(int maxCalls = 5000)
        {
            try
            {
                if (!apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.profile)) { return; }

                var now = DateTime.UtcNow;
                var refreshTime = now - xboxSettings.userProfileUpdateFrequency;
                using var connection = new MySqlConnection(dbManager.connectionString);
                await connection.OpenAsync();

                var table = schema.UserProfiles;
                if (await dbManager.validTableAsync(table))
                {
                    string sql = $@"Select xuid from {table.fullPath()} 
                     where lastScanned < @refreshTime or lastScanned IS null 
                     order by lastScanned";
                    var xuids = new List<string>();

                    using (var command = connection.CreateCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("refreshTime", refreshTime);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                xuids.Add(reader.GetString(0));
                            }
                        }

                    }

                    await connection.CloseAsync();

                    int currentCalls = 0;
                    foreach (var xuid in xuids)
                    {
                        if (++currentCalls > maxCalls)
                        {
                            logger.LogDebug("Reached Max call paramater");
                            break;
                        }
                        try
                        {

                            if (apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.profile))
                            {
                                apiTracker.makeRequest(XblAPITracker.hourlyAPICallsRemaining.profile);
                                string historyRespone = await CallAPIAsync((int)APICalls.playerTitleHistory, xuid);
                                await ParseJsonAsyncOld((int)APICalls.playerTitleHistory, historyRespone);
                                //await dbManager.EnqueueXboxQueueAsync(Tables.XboxUserProfiles, new XboxUpdateScannedData() { ID = xuid}, CRUD.Update);
                                await dbManager.EnqueQueueAsync(Database_structure.Xbox.UserProfiles, new XboxUpdateScannedData() { ID = xuid }, CRUD.Update);
                            }
                            else
                            {
                                logger.LogDebug("Cannot request for player history");
                                break;
                            }
                        }
                        catch { logger.LogDebug("Failed to call for " + xuid); }
                    }



                    await processQueueAsync(titleDataQueue);
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());
            }
        }
        public async Task scanAllGameTitlesOld(int maxCalls = 5000)
        {
            if (!apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.title)) { return; }
            try
            {

                var now = DateTime.UtcNow;
                var refreshTime = DateTime.UtcNow - xboxSettings.userProfileUpdateFrequency;
                using var connection = new MySqlConnection(dbManager.connectionString);


                var table = schema.GameTitles;
                if (await dbManager.validTableAsync(table))
                {
                    await connection.OpenAsync();
                    string sql = $@"Select modernTitleId from {table.fullPath()} 
                     where lastScanned < @refreshTime or lastScanned IS null 
                     order by lastScanned";
                    var modernTitleIds = new List<string>();

                    using (var command = connection.CreateCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("refreshTime", refreshTime);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                modernTitleIds.Add(reader.GetString(0));
                            }
                        }

                    }

                    await connection.CloseAsync();


                    int count = 0;
                    foreach (var modernTitleId in modernTitleIds)
                    {
                        if (++count > maxCalls)
                        {
                            logger.LogDebug("Reached Max call paramater");
                            break;
                        }
                        try
                        {
                            if (apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.title))
                            {
                                apiTracker.makeRequest(XblAPITracker.hourlyAPICallsRemaining.title);
                                string historyRespone = await CallAPIAsync((int)APICalls.gameTitle, modernTitleId);
                                if (historyRespone == httpRequestFail) continue;
                                var (success, tableData) = await ParseJsonAsyncOld((int)APICalls.gameTitle, historyRespone);

                                if (success)
                                {
                                    await dbManager.EnqueQueueAsync(schema.GameTitles, new XboxUpdateScannedData() { ID = modernTitleId }, CRUD.Update);
                                }
                            }
                            else
                            {
                                logger.LogDebug("Cannot Request for Title Details.");
                                break;
                            }
                        }
                        catch
                        {
                            logger.LogDebug("Failed to call for " + modernTitleId);
                        }
                    }

                    //await dbManager.processQueueAsync();


                    //add group data to db
                    await processQueueAsync(groupDetailsQueue);

                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());

            }
        }
        public async Task scanAllProductIdsOld(int maxCalls = 5000)
        {
            if (!apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.market)) { return; }
            try
            {
                var now = DateTime.UtcNow;
                var refreshTime = DateTime.UtcNow - xboxSettings.marketDetailsUpdateFrequency;
                using var connection = new MySqlConnection(dbManager.connectionString);


                var table = schema.ProductIDs;
                if (await dbManager.validTableAsync(table))
                {
                    //get the list of ids needing to be updated
                    await connection.OpenAsync();
                    string sql = $@"Select productID from {table.fullPath()}
                     where lastScanned < @refreshTime or lastScanned IS null
                     order by lastScanned";
                    var productIDS = new List<string>();


                    using (var command = connection.CreateCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("refreshTime", refreshTime);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                productIDS.Add(reader.GetString(0));
                            }
                        }

                    }

                    await connection.CloseAsync();


                    //THe loop
                    int currentCount = 0;
                    var paramaters = new Dictionary<string, object>();
                    var currentProductIds = new List<string>();
                    while (productIDS.Count > 0)
                    {
                        if (currentCount >= maxCalls)
                        {
                            logger.LogDebug("Reached max call limit:");
                            return;
                        }
                        if (!apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.market)) return;
                        currentProductIds.Add(productIDS[0]);
                        productIDS.RemoveAt(0);
                        if (currentProductIds.Count == xboxSettings.maxProductsForMarketDetails || productIDS.Count == 0)
                        {

                            try
                            {
                                apiTracker.makeRequest(XblAPITracker.hourlyAPICallsRemaining.market);
                                paramaters.Add("products", currentProductIds);
                                string historyRespone = await CallAPIAsync((int)APICalls.marketDetails, JsonConvert.SerializeObject(paramaters));

                                if (historyRespone == httpRequestFail)
                                {
                                    currentProductIds.Clear();
                                    paramaters.Clear();
                                    continue;
                                }
                                var (success, returnList) = await ParseJsonAsyncOld((int)APICalls.marketDetails, historyRespone);
                                if (!apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.market)) return;
                                if (success)
                                {
                                    foreach (var id in currentProductIds)
                                    {
                                        await dbManager.EnqueQueueAsync(schema.ProductIDs, new XboxUpdateScannedData { ID = id }, CRUD.Update);
                                    }
                                }
                                currentProductIds.Clear();
                                paramaters.Clear();
                                ++currentCount;
                            }
                            catch (Exception ex)
                            {
                                logger.LogDebug(ex.ToString());
                                continue;
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());
                throw;
            }
        }
        #endregion

#endif

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
                                if (historyRespone == httpRequestFail) continue;
                                var (success, tableData) = await ParseJsonAsync(apiCall, historyRespone);

                                if (processGameTitleReturn(gameTitle, success, tableData))
                                {
                                    foreach (var table in tableData)
                                    {
                                        if (table is XboxSchema.TitleDetailTable titleTable)
                                        {
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
                                    continue;
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
