
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
using static GameMarketAPIServer.Services.DataBaseManager;
using Microsoft.Extensions.Options;

namespace GameMarketAPIServer.Services
{
    public class XblAPIManager : APIManager
    {
        //For Games on xbox, use https://www.xbox.com/en-US/games/store/somenamedoesntmatter/productid
        //for games that arent on a xbox device, so pc/mobile, use https://apps.microsoft.com/detail/productid?hl=en-us&gl=U
        
        protected XboxSettings xboxSettings;
        protected XblAPITracker apiTracker;
        private Dictionary<string, XboxGameTitleData> titleDataQueue = new Dictionary<string, XboxGameTitleData>();
        private Dictionary<string, XboxTitleDetailsData> groupDetailsQueue = new Dictionary<string, XboxTitleDetailsData>();


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



        public XblAPIManager(IDataBaseManager dbManager, IOptions<MainSettings> settings, XblAPITracker apiTracker) : base(dbManager, settings, "xbox")
        {
            this.xboxSettings = settings.Value.xboxSettings;
            this.apiTracker = apiTracker;
            this.apiTracker.resetHourlyRequest();
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
            int loopRan = 0;
            Stopwatch apiResetStopWatch = new Stopwatch();
            while (running && !mainCTS.Token.IsCancellationRequested)
            {

#if false
                apiResetStopWatch.Restart();
                if (!await checkAPILimit())
                {
                    Console.WriteLine("Xbox out of API calls");
                    xboxSettings.setRemaingCalls(0);
                }
                xboxSettings.repartitionHourlyRequests();
                xboxSettings.outputRemainingRequests(); 
#endif


#if true

                await scanAllGameTitles(2);
                await dbManager.processXboxQueueAsync();

                apiTracker.outputRemainingRequests();
#endif
#if true
                await scanAllProductIds(2);
                await dbManager.processXboxQueueAsync();
#endif


                //Sleep Loop
                try
                {
                    Console.WriteLine("\n\nXbox Manager sleeping");
                    Console.WriteLine("loop number: " + loopRan++);
                    apiResetStopWatch.Stop();
                    Console.WriteLine("Will Start Again at: " + (DateTime.UtcNow + TimeSpan.FromHours(1) - apiResetStopWatch.Elapsed));
                    await Task.Delay(TimeSpan.FromHours(1) - apiResetStopWatch.Elapsed, mainCTS.Token);

                }
                catch (TaskCanceledException)
                {
                    break;
                }

            }

        }

        public override string ToStringAsync(int apiCall)
        {
            throw new NotImplementedException();
        }


        public async override Task<(bool, List<TableData>)> ParseJsonAsync(int apiCall, string json)
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
                    break;
                case APICalls.gameTitle:
                    return await parseGameTitleAsync(json);
                case APICalls.playerTitleHistory:
                    return await parsePlayerHistoryAsync(json);
                case APICalls.searchPlayer:

                case APICalls.checkAPILimit:
                    //return parsePlayerAccout(document, operation);
                    break;
            }
            return (false, new List<TableData>());
        }


        private async Task<(bool, List<TableData>)> parseGenericAsync(string json)
        {
            try
            {
                GenericData genericData = JsonConvert.DeserializeObject<GenericData>(json);
                if (genericData == null) { return (false, new List<TableData>()); }

                foreach (var item in genericData.items)
                {
                    item.output();
                }
                return (false, new List<TableData>());
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); return (false, new List<TableData>()); }
        }

        private async Task<(bool, List<TableData>)> parseGameTitleAsync(string json)
        {
            try
            {

                XboxMarketDetails details = JsonConvert.DeserializeObject<XboxMarketDetails>(json);
                List<TableData> returnList = new List<TableData>();
                if (details == null) { return (false, returnList); }
                foreach (var product in details.products)
                {
                    XboxTitleDetailsData data = new XboxTitleDetailsData();

#if false
                    if (!product.IsSandboxedProduct)
                    {
                        Console.WriteLine("Product Not Sandboxed");
                        return true;
                    }
                    else if (product.SandboxId != "RETAIL")
                    {
                        Console.WriteLine("Product not Retail");
                        return true;
                    }


#endif

                    foreach (var displaySku in details.products)
                    {

                    }
                    foreach(var localProp in product.localizedProperties)
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
                    //find the titleID
                    foreach (var altID in product.alternateIds)
                    {
                        if (altID.idType == "XboxTitleId")
                        {
                            data.modernTitleID = altID.value;
                        }
                    }


                    //valid data
                    //data.outputData();

                    //add to details queue
                    if (data.groupName != null && data.groupID != null)
                    {
                        if (data.groupName.Length > 60)
                            Console.WriteLine(data.groupName);
                        await groupDetailsLock.WaitAsync();
                        groupDetailsQueue.TryAdd(data.groupID, data);
                        groupDetailsLock.Release();
                    }


                    //await dbManager.InsertXboxQueueAsync(Tables.XboxProductIds, data, CRUD.Create);
                    //await dbManager.InsertXboxQueueAsync(Tables.XboxTitleDetails, data, CRUD.Create);
                    returnList.Add(data);
                    await dbManager.EnqueueXboxQueueAsync([Tables.XboxProductIds, Tables.XboxTitleDetails, Tables.XboxGameBundles], data, CRUD.Create);
                    
                    await dbManager.EnqueueXboxQueueAsync(Tables.XboxGameTitles, data, CRUD.Update);
                }
                //details.output(9);

                return (true, returnList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
        private async Task<(bool, List<TableData>)> parseMarketDetailsAsync(string json)
        {
            try
            {

                XboxMarketDetails details = JsonConvert.DeserializeObject<XboxMarketDetails>(json);
                List<TableData> returnList = new List<TableData>();
                if (details == null) { return (false, new List<TableData>()); }
                foreach (var product in details.products)
                {
                    XboxGameMarketData data = new XboxGameMarketData();

#if false
                    if (!product.IsSandboxedProduct)
                    {
                        Console.WriteLine("Product Not Sandboxed");
                        continue;
                    }
                    else if (product.SandboxId != "RETAIL")
                    {
                        Console.WriteLine("Product not Retail");
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
                    if (product.localizedProperties.Count > 1) Console.WriteLine("\n\n\nMultiple Local Properties??\n\n");
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
                        Console.WriteLine($"{data.productID} has no poster image");
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
                                            Console.WriteLine("multiple skus valid??");
                                        }
                                        data.purchasable = true;
                                        data.ListPrice = availability.orderManagementData.price.listPrice;
                                        data.msrp = availability.orderManagementData.price.msrp;

                                        if (availability.conditions.clientConditions.allowedPlatforms != null)
                                        foreach(var platform in availability.conditions.clientConditions.allowedPlatforms)
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
                        Console.WriteLine($"{data.productID} is not purchasable and not sandboxed");
                    }
                    returnList.Add(data);
                    await dbManager.EnqueueXboxQueueAsync(Tables.XboxMarketDetails, data, CRUD.Create);
                }
                //details.output(9);

                return (true, returnList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (false, new List<TableData>());
            }
        }

        private async Task<(bool, List<TableData>)> parsePlayerHistoryAsync(string json)
        {
            try
            {
                //JObject oo = JObject.Parse(json);
                // Console.WriteLine(oo.ToString());

                if (xboxSettings.outputSettings.outputDebug)
                    Console.WriteLine("Parsing Player history");
                List<TableData> returnList = new List<TableData>();

                XboxTitleHistory history = JsonConvert.DeserializeObject<XboxTitleHistory>(json);
                if (history == null) return (false, returnList);
                foreach (var title in history.titles.Where(title => title.modernTitleId != null && !title.devices.Contains("Win32")))
                {
                    XboxGameTitleData titleData = new XboxGameTitleData();
                    titleData.titleID = title.titleId;
                    titleData.titleName = title.name;
                    titleData.modernTitleID = title.modernTitleId;
                    titleData.displayImage = title.displayImage;
                    titleData.isGamePass = title.gamePass.isGamePass;
                    titleData.devices = title.devices;



                    await titleDataLock.WaitAsync();
                    titleDataQueue.TryAdd(titleData.modernTitleID, titleData);
                    titleDataLock.Release();
                    returnList.Add(titleData);
                }
                // await dbManager.processQueueAsync();
                // history.output(5);


                return (true,returnList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task scanAllPlayerHistories(int maxCalls = 5000)
        {
            try
            {
                if (!apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.profile)) { return; }

                var now = DateTime.UtcNow;
                var refreshTime = DateTime.UtcNow - xboxSettings.userProfileUpdateFrequency;
                using var connection = new MySqlConnection(dbManager.connectionString);
                await connection.OpenAsync();

                Tables table = Tables.XboxUserProfiles;
                if (await dbManager.validTableAsync(table))
                {
                    string sql = $@"Select xuid from {Schemas.xbox}.{table.To_String()} 
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
                            Console.WriteLine("Reached Max call paramater");
                            break;
                        }
                        try
                        {

                            if (apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.profile))
                            {
                                apiTracker.makeRequest(XblAPITracker.hourlyAPICallsRemaining.profile);
                                string historyRespone = await CallAPIAsync((int)APICalls.playerTitleHistory, xuid);
                                await ParseJsonAsync((int)APICalls.playerTitleHistory, historyRespone);
                                await dbManager.EnqueueXboxQueueAsync(Tables.XboxUserProfiles, new XboxUpdateScannedData() { ID = xuid}, CRUD.Update);
                            }
                            else
                            {
                                Console.WriteLine("Cannot request for player history");
                                break;
                            }
                        }
                        catch { Console.WriteLine("Failed to call for " + xuid); }
                    }



                    await processQueueAsync(titleDataQueue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        public async Task scanAllGameTitles(int maxCalls = 5000)
        {
            if (!apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.title)) { return; }
            try
            {

                var now = DateTime.UtcNow;
                var refreshTime = DateTime.UtcNow - xboxSettings.userProfileUpdateFrequency;
                using var connection = new MySqlConnection(dbManager.connectionString);


                Tables table = Tables.XboxGameTitles;
                if (await dbManager.validTableAsync(table))
                {
                    await connection.OpenAsync();
                    string sql = $@"Select modernTitleId from {Schemas.xbox}.{table.To_String()} 
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
                            Console.WriteLine("Reached Max call paramater");
                            break;
                        }
                        try
                        {
                            if (apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.title))
                            {
                                apiTracker.makeRequest(XblAPITracker.hourlyAPICallsRemaining.title);
                                string historyRespone = await CallAPIAsync((int)APICalls.gameTitle, modernTitleId);
                                if (historyRespone == httpRequestFail) continue;
                                var (success, tableData) = await ParseJsonAsync((int)APICalls.gameTitle, historyRespone);
                                
                                if (success)
                                {
                                    await dbManager.EnqueueXboxQueueAsync(Tables.XboxGameTitles, new XboxUpdateScannedData() { ID = modernTitleId }, CRUD.Update);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Cannot Request for Title Details.");
                                break;
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Failed to call for " + modernTitleId);
                        }
                    }

                    //await dbManager.processQueueAsync();


                    //add group data to db
                    await processQueueAsync(groupDetailsQueue);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

            }
        }

        public async Task scanAllProductIds(int maxCalls = 5000)
        {
            if (!apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.market)) { return; }
            try
            {
                var now = DateTime.UtcNow;
                var refreshTime = DateTime.UtcNow - xboxSettings.marketDetailsUpdateFrequency;
                using var connection = new MySqlConnection(dbManager.connectionString);


                Tables table = Tables.XboxProductIds;
                if (await dbManager.validTableAsync(table))
                {
                    //get the list of ids needing to be updated
                    await connection.OpenAsync();
                    string sql = $@"Select productID from {Schemas.xbox}.{table.To_String()}
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
                            Console.WriteLine("Reached max call limit:");
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
                                var (success, returnList) = await ParseJsonAsync((int)APICalls.marketDetails, historyRespone);
                                if (!apiTracker.canRequest(XblAPITracker.hourlyAPICallsRemaining.market)) return;
                                if (success)
                                {
                                    foreach (var id in currentProductIds)
                                    {
                                        await dbManager.EnqueueXboxQueueAsync(Tables.XboxProductIds, new XboxUpdateScannedData { ID = id }, CRUD.Update);
                                    }
                                }
                                currentProductIds.Clear();
                                paramaters.Clear();
                                ++currentCount;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                continue;
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        protected async Task processQueueAsync<T>(Dictionary<string, T> tableQueue) where T : TableData
        {
            try
            {
                foreach (var tableData in tableQueue)
                {
                    //for the group data
                    if (tableData.Value is XboxTitleDetailsData)
                    {

                        await dbManager.EnqueueXboxQueueAsync(Tables.XboxGroupData, tableData.Value, CRUD.Create);

                    }
                    else if (tableData.Value is XboxGameTitleData)
                    {
                        await dbManager.EnqueueXboxQueueAsync([Tables.XboxGameTitles, Tables.XboxTitleDevices], tableData.Value, CRUD.Create);
                    }
                }
            }
            catch
            {
            }
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
                Console.WriteLine("Adding Headers");

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
            if (xboxSettings.outputSettings.outputDebug)
                Console.WriteLine("Handleing Headers");
            if (settings.outputHTTPResponse)
            {
                foreach (var header in headers)
                {
                    Console.WriteLine(header);
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
                        apiTracker.setRemaingCalls( max - used);
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
