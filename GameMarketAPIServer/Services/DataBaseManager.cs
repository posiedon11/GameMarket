

using GameMarketAPIServer.Configuration;
using MySqlConnector;
using System.Numerics;
using GameMarketAPIServer.Utilities;
using GameMarketAPIServer.Models.Enums;
using GameMarketAPIServer.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static SteamKit2.DepotManifest;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Extensions.Options;

namespace GameMarketAPIServer.Services
{

    public interface IDataBaseManager
    {
        Task EnqueueGameMarketQueueAsync(Tables table, TableData data, CRUD operation = CRUD.Read);
        Task EnqueueGameMarketQueueAsync(IEnumerable<Tables> tables, TableData data, CRUD operation = CRUD.Read);
        Task EnqueueSteamQueueAsync(Tables table, TableData data, CRUD operation = CRUD.Read);
        Task EnqueueSteamQueueAsync(IEnumerable<Tables> tables, TableData data, CRUD operation = CRUD.Read);
        Task EnqueueXboxQueueAsync(Tables table, TableData data, CRUD operation = CRUD.Read);
        Task EnqueueXboxQueueAsync(IEnumerable<Tables> tables, TableData data, CRUD operation = CRUD.Read);

        Task processGameMarketQueueAsync();
        Task processSteamQueueAsync();
        Task processXboxQueueAsync();

        Task<bool> validTableAsync(Tables table);

        public string connectionString { get; set; }

    }
    public class DataBaseManager : IDataBaseManager
    {
        private ILogger<DataBaseManager> logger;
        protected MainSettings mainSettings;
        protected SQLServerSettings sqlserverSettings;
        public string connectionString { get; set; }
        private readonly Queue<(Tables, TableData, CRUD)> xboxQueue = new Queue<(Tables, TableData, CRUD)>();
        private readonly Queue<(Tables, TableData, CRUD)> steamQueue = new Queue<(Tables, TableData, CRUD)>();
        private readonly Queue<(Tables, TableData, CRUD)> gameMarketQueue = new Queue<(Tables, TableData, CRUD)>();


        private static Mutex insertXboxQueueMutex = new Mutex();
        private readonly SemaphoreSlim xboxQueueLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim steamQueueLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim gameMarketQueueLock = new SemaphoreSlim(1, 1);

        private List<Tables> foundTables = new List<Tables>();

        public enum Schemas
        {
            xbox,
            steam,
            gamemarket
        }


        public DataBaseManager(IOptions<MainSettings> settings, ILogger<DataBaseManager> logger)
        {
            this.logger = logger;
            mainSettings = settings.Value;
            sqlserverSettings = settings.Value.sqlServerSettings;

            connectionString = "Server=" + sqlserverSettings.serverName + ";Port=" + sqlserverSettings.serverPort + ";User=" + sqlserverSettings.serverUserName + ";Password=" + sqlserverSettings.serverPassword + ";";
        }


        #region QueueManagement

        public async Task EnqueueGameMarketQueueAsync(Tables table, TableData data, CRUD operation = CRUD.Read)
        {
            await EnqueueGameMarketQueueAsync([table], data, operation);
        }
        public async Task EnqueueGameMarketQueueAsync(IEnumerable<Tables> tables, TableData data, CRUD operation = CRUD.Read)
        {
            await gameMarketQueueLock.WaitAsync();
            try
            {
                foreach (Tables table in tables)
                    gameMarketQueue.Enqueue((table, data, operation));
            }
            finally
            {
                gameMarketQueueLock.Release();
            }
        }


        public async Task EnqueueSteamQueueAsync(Tables table, TableData data, CRUD operation = CRUD.Read)
        {
            await EnqueueSteamQueueAsync([table], data, operation);
        }
        public async Task EnqueueSteamQueueAsync(IEnumerable<Tables> tables, TableData data, CRUD operation = CRUD.Read)
        {
            await steamQueueLock.WaitAsync();
            try
            {
                foreach (Tables table in tables)
                    steamQueue.Enqueue((table, data, operation));
            }
            finally
            {
                steamQueueLock.Release();
            }
        }


        public async Task EnqueueXboxQueueAsync(Tables table, TableData data, CRUD operation = CRUD.Read)
        {
            await EnqueueXboxQueueAsync([table], data, operation);
        }
        public async Task EnqueueXboxQueueAsync(IEnumerable<Tables> tables, TableData data, CRUD operation = CRUD.Read)
        {
            await xboxQueueLock.WaitAsync();
            try
            {
                foreach (Tables table in tables)
                    xboxQueue.Enqueue((table, data, operation));
            }
            finally
            {
                xboxQueueLock.Release();
            }
        }





        public async Task processSteamQueueAsync()
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            while (steamQueue.Any())
            {
                try
                {
                    await steamQueueLock.WaitAsync();
                    var queueData = steamQueue.Dequeue();
                    steamQueueLock.Release();
                    var table = queueData.Item1;
                    var data = queueData.Item2;

                    switch (queueData.Item3)
                    {
                        case CRUD.Create:
                            {
                                await InsertIntoTableAsync(table, data, connection);
                                break;
                            }
                        case CRUD.Read:
                            {
                                break;
                            }
                        case CRUD.Update:
                            {
                                await UpdateTableAsync(table, data, connection);
                                break;
                            }
                        case CRUD.Delete:
                            {
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw;
                }

            }
        }

        public async Task processGameMarketQueueAsync()
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            while (gameMarketQueue.Any())
            {
                try
                {
                    await gameMarketQueueLock.WaitAsync();
                    var queueData = gameMarketQueue.Dequeue();
                    gameMarketQueueLock.Release();
                    var table = queueData.Item1;
                    var data = queueData.Item2;

                    switch (queueData.Item3)
                    {
                        case CRUD.Create:
                            {
                                await InsertIntoTableAsync(table, data, connection);
                                break;
                            }
                        case CRUD.Read:
                            {
                                break;
                            }
                        case CRUD.Update:
                            {
                                await UpdateTableAsync(table, data, connection);
                                break;
                            }
                        case CRUD.Delete:
                            {
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw;
                }

            }
        }

        public async Task processXboxQueueAsync()
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            while (xboxQueue.Any())
            {
                try
                {
                    await xboxQueueLock.WaitAsync();
                    var queueData = xboxQueue.Dequeue();
                    xboxQueueLock.Release();
                    var table = queueData.Item1;
                    var data = queueData.Item2;

                    switch (queueData.Item3)
                    {
                        case CRUD.Create:
                            {
                                await InsertIntoTableAsync(table, data, connection);
                                break;
                            }
                        case CRUD.Read:
                            {
                                break;
                            }
                        case CRUD.Update:
                            {
                                await UpdateTableAsync(table, data, connection);
                                break;
                            }
                        case CRUD.Delete:
                            {
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw;
                }

            }
        }

        #endregion



        private async Task<bool> InsertIntoTableAsync(Tables table, TableData data, MySqlConnection connection)
        {

            //using var transaction = await connection.BeginTransactionAsync();
            bool success = false;
            try
            {
                if (!await validTableAsync(table))
                {
                    //await transaction.RollbackAsync();
                    Console.WriteLine("Table not valid for insertion");
                    return false;
                }
                switch(table.ToSchema())
                {
                    case Schemas.xbox:
                        success = await InsertXbox(table,data,connection);
                        break;
                    case Schemas.steam:
                        success = await InsertSteam(table,data,connection);
                        break;
                    case Schemas.gamemarket:
                        success = await InsertGameMarket(table,data,connection);
                        break;
                    default:
                        break;
                }

                // await transaction.CommitAsync();
                return success;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Failed to insert for: ");
                data.outputData();
               // Console.WriteLine("Sql Error: " + ex.ToString());
                return false;
            }
            catch (Exception ex)
            {
                // If something goes wrong, roll back the transaction
                //await transaction.RollbackAsync();
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private async Task<bool> UpdateTableAsync(Tables table, TableData data, MySqlConnection connection)
        {
            //using var transaction = await connection.BeginTransactionAsync();
            bool success = false;
            try
            {
                
                if (!await validTableAsync(table))
                {
                    //await transaction.RollbackAsync();
                    return success;
                }

                switch (table.ToSchema())
                {
                    case Schemas.xbox:
                        success = await UpdateXbox(table, data, connection);
                        break;
                    case Schemas.steam:
                        success = await UpdateSteam(table, data, connection);
                        break;
                    case Schemas.gamemarket:
                        success = await UpdateGameMarket(table, data, connection);
                        break;
                    default:
                        break;
                }
                // await transaction.CommitAsync();
                return success;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Update failed for: ");
                data.outputData();
                return false;
            }
            catch (Exception ex)
            {
                // If something goes wrong, roll back the transaction
                //await transaction.RollbackAsync();
                Console.WriteLine(ex.ToString());
                
                throw;
            }
        }


        private async Task<bool> InsertXbox(Tables table, TableData data, MySqlConnection connection)
        {
            try
            {
                bool success = false;
                switch(table)
                {
                    case Tables.XboxUserProfiles:
                        {
                            break;
                        }
                    case Tables.XboxGameTitles:
                        {
                            if (data is XboxGameTitleData titleData)
                            {

                                string sqlQuery = $@"INSERT INTO  {Schemas.xbox}.{table.To_String()} 
                                 (titleID, titleName, displayImage, modernTitleID, isGamePass) 
                                 VALUES(@titleID, @titleName, @displayImage, @modernTitleID, @isGamePass) 
                                 ON DUPLICATE KEY UPDATE titleName = VALUES(titleName), displayImage = VALUES(displayImage), modernTitleId=VALUES(modernTitleId), isGamePass = VALUES(isGamePass)";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("titleID", titleData.titleID);
                                    command.Parameters.AddWithValue("titleName", titleData.titleName);
                                    command.Parameters.AddWithValue("displayImage", titleData.displayImage);
                                    command.Parameters.AddWithValue("modernTitleID", titleData.modernTitleID);
                                    command.Parameters.AddWithValue("isGamePass", titleData.isGamePass);
                                    await command.ExecuteNonQueryAsync();
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.XboxTitleDevices:
                        {
                            if (data is XboxGameTitleData titleData)
                            {

                                //Delete all devices for id
                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = $@"Delete from  {Schemas.xbox}.{table.To_String()} where modernTitleId = @modernTitleId";
                                    command.Parameters.AddWithValue("modernTitleId", titleData.modernTitleID);
                                    await command.ExecuteNonQueryAsync();
                                }
                                string sqlQuery = $@"INSERT INTO {Schemas.xbox}.{table.To_String()} 
                                 (modernTitleID, device) 
                                 VALUES(@modernTitleID, @device) 
                                 ON DUPLICATE KEY UPDATE modernTitleID = VALUES(modernTitleID), device = VALUES(device)";

                                foreach (var device in titleData.devices)
                                {
                                    using (var command = new MySqlCommand())
                                    {
                                        command.Connection = connection;
                                        command.CommandText = sqlQuery;

                                        command.Parameters.AddWithValue("modernTitleID", titleData.modernTitleID);
                                        command.Parameters.AddWithValue("device", device);
                                        await command.ExecuteNonQueryAsync();
                                    }
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.XboxProductIds:
                        {
                            if (data is XboxTitleDetailsData titleData)
                            {
                                string sqlQuery = $@"INSERT INTO {Schemas.xbox}.{table.To_String()} 
                                 (productID) 
                                 VALUES(@productID) 
                                 ON DUPLICATE KEY UPDATE productID = VALUES(productID)";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("productID", titleData.productID);
                                    await command.ExecuteNonQueryAsync();
                                }

                                foreach (var bundleID in titleData.bundleIDs)
                                {
                                    using (var command = new MySqlCommand())
                                    {
                                        command.Connection = connection;
                                        command.CommandText = sqlQuery;

                                        command.Parameters.AddWithValue("productID", bundleID);
                                        await command.ExecuteNonQueryAsync();
                                    }
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.XboxTitleDetails:
                        {
                            if (data is XboxTitleDetailsData titleData)
                            {
                                string sqlQuery = $@"INSERT INTO {Schemas.xbox}.{table.To_String()} 
                                 (modernTitleID, productID) 
                                 VALUES(@modernTitleID, @productID) 
                                 ON DUPLICATE KEY UPDATE modernTitleID = VALUES(modernTitleID), productID = VALUES(productID)";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("modernTitleID", titleData.modernTitleID);
                                    command.Parameters.AddWithValue("productID", titleData.productID);
                                    await command.ExecuteNonQueryAsync();
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.XboxGameBundles:
                        {
                            if (data is XboxTitleDetailsData titleData)
                            {
                                //remove all bundles for ID
                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = $@"Delete from {Schemas.xbox}.{table.To_String()} where productID = @productID";

                                    command.Parameters.AddWithValue("productID", titleData.productID);
                                    await command.ExecuteNonQueryAsync();
                                }
                                string sqlQuery = $@"INSERT INTO {Schemas.xbox}.{table.To_String()} 
                                 (relatedProductID, productID) 
                                 VALUES(@relatedProductID, @productID) On Duplicate key update productID = Values(productID)";

                                //insert new bundles
                                foreach (var bundleID in titleData.bundleIDs)
                                {

                                    using (var command = new MySqlCommand())
                                    {
                                        command.Connection = connection;
                                        command.CommandText = sqlQuery;

                                        command.Parameters.AddWithValue("relatedProductID", bundleID);
                                        command.Parameters.AddWithValue("productID", titleData.productID);
                                        await command.ExecuteNonQueryAsync();
                                    }
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.XboxGameGenres:
                        {
                            if (data is XboxTitleDetailsData titleData)
                            {
                                string sqlQuery = $@"INSERT INTO {Schemas.xbox}.{table.To_String()} 
                                 (productID) 
                                 VALUES(@productID) 
                                 ON DUPLICATE KEY UPDATE productID = VALUES(productID)";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("productID", titleData.productID);
                                    await command.ExecuteNonQueryAsync();
                                }

                                foreach (var bundleID in titleData.bundleIDs)
                                {
                                    using (var command = new MySqlCommand())
                                    {
                                        command.Connection = connection;
                                        command.CommandText = sqlQuery;

                                        command.Parameters.AddWithValue("productID", bundleID);
                                        await command.ExecuteNonQueryAsync();
                                    }
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.XboxGroupData:
                        {
                            if (data is XboxTitleDetailsData titleData)
                            {

                                string sqlQuery = $@"INSERT INTO {Schemas.xbox}.{table.To_String()}
                                 (groupID, groupName) 
                                 VALUES(@groupID, @groupName) 
                                 ON DUPLICATE KEY UPDATE groupName = VALUES(groupName)";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("groupID", titleData.groupID);
                                    command.Parameters.AddWithValue("groupName", titleData.groupName);
                                    await command.ExecuteNonQueryAsync();
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.XboxMarketDetails:
                        {
                            if (data is XboxGameMarketData marketData)
                            {
                                string sqlQuery = $@"INSERT INTO {Schemas.xbox}.{table.To_String()} 
                                 (productID, productTitle, developerName, publisherName, currencyCode, purchasable, posterImage, msrp, listPrice, startDate, endDate) 
                                 VALUES(@productID,@productTitle, @developerName,@publisherName,@currencyCode,@purchasable,@posterImage,@msrp,@listPrice,@startDate,@endDate) 
                                 ON DUPLICATE KEY UPDATE productTitle = VALUES(productTitle), 
                                 developerName = VALUES(developerName), publisherName = VALUES(publisherName), currencyCode = VALUES(currencyCode), purchasable = VALUES(purchasable), 
                                 posterImage = VALUES(posterImage), msrp = VALUES(msrp), listPrice = VALUES(listPrice), startDate = VALUES(startDate), endDate = VALUES(endDate)";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;


                                    //They should all have this
                                    command.Parameters.AddWithValue("productID", marketData.productID);
                                    command.Parameters.AddWithValue("productTitle", marketData.productTitle);
                                    command.Parameters.AddWithValue("developerName", marketData.devName);
                                    command.Parameters.AddWithValue("publisherName", marketData.pubName);
                                    command.Parameters.AddWithValue("startDate", marketData.startDate);
                                    command.Parameters.AddWithValue("endDate", marketData.endDate);
                                    command.Parameters.AddWithValue("purchasable", marketData.purchasable);
                                    command.Parameters.AddWithValue("posterImage", marketData.posterImage);

                                    if (marketData.purchasable)
                                    {
                                        command.Parameters.AddWithValue("msrp", marketData.msrp);
                                        command.Parameters.AddWithValue("listPrice", marketData.ListPrice);
                                        command.Parameters.AddWithValue("currencyCode", marketData.currencyCode);
                                    }
                                    else
                                    {
                                        command.Parameters.AddWithValue("msrp", null);
                                        command.Parameters.AddWithValue("listPrice", null);
                                        command.Parameters.AddWithValue("currencyCode", null);
                                    }

                                    await command.ExecuteNonQueryAsync();
                                }

                                success = true;
                            }
                            break;
                        }
                    case Tables.XboxProductPlatforms:
                        {
                            if (data is XboxGameMarketData titleData)
                            {
                                //remove all bundles for ID
                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = $@"Delete from {Schemas.xbox}.{table.To_String()} where productID = @productID";

                                    command.Parameters.AddWithValue("productID", titleData.productID);
                                    await command.ExecuteNonQueryAsync();
                                }
                                string sqlQuery = $@"INSERT INTO {Schemas.xbox}.{table.To_String()} 
                                 (productID, platform) 
                                 VALUES(@productId, @platform) On Duplicate key update platform = Values(platform)";

                                //insert new bundles
                                foreach (var platform in titleData.platforms)
                                {

                                    using (var command = new MySqlCommand())
                                    {
                                        command.Connection = connection;
                                        command.CommandText = sqlQuery;

                                        command.Parameters.AddWithValue("platform", platform);
                                        command.Parameters.AddWithValue("productID", titleData.productID);
                                        await command.ExecuteNonQueryAsync();
                                    }
                                }
                                success = true;
                            }
                            break;
                        }


                    default:
                        logger.LogWarning($"DB insert not Implemented for {table.ToString()}");
                        break;
                       
                }

                return success;
            }catch (MySqlException ex) { return false; }
        }
        private async Task<bool> InsertSteam(Tables table, TableData data, MySqlConnection connection)
        {
            try
            {
                bool success = false;
                switch(table)
                {
                    case Tables.SteamAppIDs:
                        {
                            if (data is SteamAppListData appListData)
                            {


                                string sqlQuery = $@"INSERT INTO {Schemas.steam}.{table.To_String()}
                                 (appID) 
                                 VALUES(@appID) 
                                 ON DUPLICATE KEY UPDATE appID = VALUES(appID)";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("appID", appListData.appid);
                                    await command.ExecuteNonQueryAsync();
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.SteamAppDetails:
                        {
                            if (data is SteamAppDetailsData appDetailsData)
                            {


                                string sqlQuery = $@"INSERT INTO {Schemas.steam}.{table.To_String()} 
                                 (appID, appType,appName,msrp,listprice,isFree) 
                                 VALUES(@appID, @appType,@appName,@msrp,@listprice,@isFree) 
                                 ON DUPLICATE KEY UPDATE appType = VALUES(appType), appName = VALUES(appName), msrp = VALUES(msrp), listprice = VALUES(listprice), isFree = VALUES(isFree)  ";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("appID", appDetailsData.appID);
                                    command.Parameters.AddWithValue("appType", appDetailsData.appType);
                                    command.Parameters.AddWithValue("appName", appDetailsData.appName);
                                    if (!appDetailsData.isFree)
                                    {
                                        command.Parameters.AddWithValue("listprice", appDetailsData.listprice);
                                        command.Parameters.AddWithValue("msrp", appDetailsData.msrp);
                                    }
                                    else
                                    {
                                        command.Parameters.AddWithValue("listprice", null);
                                        command.Parameters.AddWithValue("msrp", null);
                                    }
                                    command.Parameters.AddWithValue("isFree", appDetailsData.isFree);

                                    await command.ExecuteNonQueryAsync();
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.SteamPackageIDs:
                        {
                            if (data is SteamAppDetailsData appDetailsData)
                            {
                                string sqlQuery = $@"INSERT INTO {Schemas.steam}.{table.To_String()} 
                                 (packageID) 
                                 VALUES(@packageID) 
                                 ON DUPLICATE KEY UPDATE packageID = VALUES(packageID)";

                                foreach (var package in appDetailsData.packages)
                                {
                                    using (var command = new MySqlCommand())
                                    {
                                        command.Connection = connection;
                                        command.CommandText = sqlQuery;

                                        command.Parameters.AddWithValue("packageID", package);

                                        await command.ExecuteNonQueryAsync();
                                    }
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.SteamPackages:
                        {
                            if (data is SteamAppDetailsData appDetailsData)
                            {

                                //remove all packages for ID
                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = $@"Delete from {Schemas.steam}.{table.To_String()} where appID = @appID";

                                    command.Parameters.AddWithValue("appID", appDetailsData.appID);
                                    await command.ExecuteNonQueryAsync();
                                }

                                string sqlQuery = $@"INSERT INTO {Schemas.steam}.{table.To_String()}
                                 (appID, packageID) 
                                 VALUES(@appID, @packageID) 
                                 ON DUPLICATE KEY UPDATE appID = VALUES(appID)";

                                foreach (var package in appDetailsData.packages)
                                {
                                    using (var command = new MySqlCommand())
                                    {
                                        command.Connection = connection;
                                        command.CommandText = sqlQuery;

                                        command.Parameters.AddWithValue("appID", appDetailsData.appID);
                                        command.Parameters.AddWithValue("packageID", package);

                                        await command.ExecuteNonQueryAsync();
                                    }
                                }

                                success = true;
                            }
                            break;
                        }
                    case Tables.SteamAppDevelopers:
                        {
                            if (data is SteamAppDetailsData appDetailsData)
                            {
                                string sqlQuery = $@"INSERT INTO {Schemas.steam}.{table.To_String()} 
                                 (appID, developer) 
                                 VALUES(@appID, @developer) 
                                 ON DUPLICATE KEY UPDATE appID = VALUES(appID), developer = Values(developer)";

                                foreach (var developer in appDetailsData.developers)
                                {
                                    using (var command = new MySqlCommand())
                                    {
                                        command.Connection = connection;
                                        command.CommandText = sqlQuery;

                                        command.Parameters.AddWithValue("appID", appDetailsData.appID);
                                        command.Parameters.AddWithValue("developer", developer);

                                        await command.ExecuteNonQueryAsync();
                                    }
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.SteamAppPublishers:
                        {
                            if (data is SteamAppDetailsData appDetailsData)
                            {

                                string sqlQuery = $@"INSERT INTO {Schemas.steam}.{table.To_String()}
                                 (appID, publisher) 
                                 VALUES(@appID, @publisher) 
                                 ON DUPLICATE KEY UPDATE appID = VALUES(appID), publisher = Values(publisher)";

                                foreach (var publisher in appDetailsData.publishers)
                                {
                                    using (var command = new MySqlCommand())
                                    {
                                        command.Connection = connection;
                                        command.CommandText = sqlQuery;

                                        command.Parameters.AddWithValue("appID", appDetailsData.appID);
                                        command.Parameters.AddWithValue("publisher", publisher);

                                        await command.ExecuteNonQueryAsync();
                                    }
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.SteamAppPlatforms:
                        {
                            if (data is SteamAppDetailsData appDetailsData)
                            {

                                //remove all packages for ID
                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = $@"Delete from {Schemas.steam}.{table.To_String()} where appID = @appID";
                                    command.Parameters.AddWithValue("appID", appDetailsData.appID);
                                    await command.ExecuteNonQueryAsync();
                                }
                                string sqlQuery = $@"INSERT INTO {Schemas.steam}.{table.To_String()} 
                                 (appID, platform) 
                                 VALUES(@appID, @platform) 
                                 ON DUPLICATE KEY UPDATE appID = VALUES(appID), platform = VAlUES(platform)";

                                foreach (var platform in appDetailsData.platforms)
                                {
                                    using (var command = new MySqlCommand())
                                    {
                                        command.Connection = connection;
                                        command.CommandText = sqlQuery;

                                        command.Parameters.AddWithValue("appID", appDetailsData.appID);
                                        command.Parameters.AddWithValue("platform", platform);

                                        await command.ExecuteNonQueryAsync();
                                    }
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.SteamAppGenres:
                        {
                            if (data is SteamAppDetailsData appDetailsData)
                            {
                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = $@"Delete from {Schemas.steam}.{table.To_String()}  where appID = @appID";

                                    command.Parameters.AddWithValue("appID", appDetailsData.appID);
                                    await command.ExecuteNonQueryAsync();
                                }

                                string sqlQuery = $@"INSERT INTO {Schemas.steam}.{table.To_String()}
                                 (appID, genre) 
                                 VALUES(@appID, @genre) 
                                 ON DUPLICATE KEY UPDATE appID = VALUES(appID), genre = VALUES(genre)";

                                foreach (var genre in appDetailsData.genres)
                                {
                                    using (var command = new MySqlCommand())
                                    {
                                        command.Connection = connection;
                                        command.CommandText = sqlQuery;

                                        command.Parameters.AddWithValue("appID", appDetailsData.appID);
                                        command.Parameters.AddWithValue("genre", genre);

                                        await command.ExecuteNonQueryAsync();
                                    }
                                }
                                success = true;
                            }
                            break;
                        }

                    default:
                        logger.LogWarning($"DB insert not Implemented for {table.ToString()}");
                        break;
                }



                return success;

            }catch (MySqlException ex) { return false;}
        }
        private async Task<bool> InsertGameMarket(Tables table, TableData data, MySqlConnection connection)
        {
            try
            {
                bool success = false;

                switch(table)
                {
                    case Tables.GameMarketGameTitles:
                        {
                            if (data is GameMarketMergedXboxData mergedData)
                            {

                                string sqlQuery = $@"INSERT INTO {Schemas.gamemarket}.{table.To_String()}
                                 (gameTitle) 
                                 VALUES(@gameTitle)";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("gameTitle", mergedData.gameTitle);
                                    await command.ExecuteNonQueryAsync();
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.GameMarketXboxLink:
                        {
                            if (data is GameMarketMergedXboxData mergedData)
                            {
                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = $@"Delete from {Schemas.gamemarket}.{table.To_String()} where gameId = @gameId";

                                    command.Parameters.AddWithValue("gameId", mergedData.gameId);
                                    await command.ExecuteNonQueryAsync();
                                }
                                string sqlQuery2 = $@"INSERT INTO {Schemas.gamemarket}.{table.To_String()}
                                 (gameID, modernTitleID) 
                                 VALUES(LAST_INSERT_ID(), @modernTitleID)";
                                string sqlQuery1 = $@"INSERT INTO {Schemas.gamemarket}.{table.To_String()}
                                 (gameID, modernTitleID) 
                                 VALUES(@gameID, @modernTitleID)";

                                foreach (var titleId in mergedData.xboxIds)
                                {
                                    if (mergedData.gameId >= 1000)
                                    {
                                        using (var command = new MySqlCommand())
                                        {
                                            command.Connection = connection;
                                            command.CommandText = sqlQuery1;
                                            command.Parameters.AddWithValue("gameID", mergedData.gameId);
                                            command.Parameters.AddWithValue("modernTitleID", titleId);
                                            await command.ExecuteNonQueryAsync();
                                        }
                                    }
                                    else
                                    {
                                        using (var command = new MySqlCommand())
                                        {
                                            command.Connection = connection;
                                            command.CommandText = sqlQuery2;
                                            command.Parameters.AddWithValue("modernTitleID", titleId);
                                            await command.ExecuteNonQueryAsync();
                                        }
                                    }

                                }
                                success = true;
                            }
                            break;
                        }

                    default:
                        logger.LogWarning($"DB insert not Implemented for {table.ToString()}");
                        break;
                }


                return success;

            }
            catch (MySqlException ex) { return false; }
        }


        private async Task<bool> UpdateXbox(Tables table, TableData data, MySqlConnection connection)
        {
            try
            {
                bool success = false;
                switch(table)
                {
                    case Tables.XboxUserProfiles:
                        {
                            if (data is XboxUpdateScannedData updateData)
                            {
                                var tableCommand = new MySqlCommand("Use " + sqlserverSettings.xboxSchema + ";", connection);
                                await tableCommand.ExecuteNonQueryAsync();

                                string sqlQuery = $@"Update {Schemas.xbox}.{table.To_String()}
                                 set lastScanned = @lastScanned 
                                 where xuid = @xuid";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("xuid", updateData.ID);
                                    command.Parameters.AddWithValue("lastScanned", updateData.lastScanned);
                                    await command.ExecuteNonQueryAsync();
                                }
                                success = true;
                            }

                            break;
                        }
                    case Tables.XboxGameTitles:
                        {
                            if (data is XboxTitleDetailsData titleDetailsData)
                            {
                                string sqlQuery = $@"Update {Schemas.xbox}.{table.To_String()} 
                                 set groupID = @groupID, lastScanned = @lastScanned 
                                 where modernTitleID = @modernTitleID";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("groupID", titleDetailsData.groupID);
                                    command.Parameters.AddWithValue("modernTitleID", titleDetailsData.modernTitleID);
                                    command.Parameters.AddWithValue("lastScanned", titleDetailsData.lastScanned);

                                    await command.ExecuteNonQueryAsync();
                                }
                                success = true;
                            }
                            else if (data is XboxUpdateScannedData updateData)
                            {

                                string sqlQuery = $@"Update {Schemas.xbox}.{table.To_String()}
                                 set lastScanned = @lastScanned 
                                 where modernTitleID = @modernTitleID";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("modernTitleID", updateData.ID);
                                    command.Parameters.AddWithValue("lastScanned", updateData.lastScanned);
                                    await command.ExecuteNonQueryAsync();
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.XboxProductIds:
                        {
                            if (data is XboxUpdateScannedData updateData)
                            {
                                string sqlQuery = $@"Update {Schemas.xbox}.{table.To_String()} 
                                     set lastScanned = @lastScanned
                                     where productID = @productID";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("productID", updateData.ID);
                                    command.Parameters.AddWithValue("lastScanned", updateData.lastScanned);
                                    await command.ExecuteNonQueryAsync();
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.XboxTitleDetails:
                        {
                            if (data is XboxTitleDetailsData titleData)
                            {
                                string sqlQuery = $@"Update {Schemas.xbox}.{table.To_String()}
                                     set productID = @productID 
                                     where modernTitleID = @modernTitleID";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("modernTitleID", titleData.modernTitleID);
                                    command.Parameters.AddWithValue("productID", titleData.productID);
                                    await command.ExecuteNonQueryAsync();
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.XboxGameBundles:
                        {
                            if (data is XboxTitleDetailsData titleData)
                            {
                                //remove all bundles for ID
                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = $@"Delete from {Schemas.xbox}.{table.To_String()} where productID = @productID";

                                    command.Parameters.AddWithValue("productID", titleData.productID);
                                    await command.ExecuteNonQueryAsync();
                                }
                                string sqlQuery = @"INSERT INTO {Schemas.xbox}.{table.To_String()} 
                                 (relatedProductID, productID) 
                                 VALUES(@relatedProductID, @productID) ";

                                //insert new bundles
                                foreach (var bundleID in titleData.bundleIDs)
                                {

                                    using (var command = new MySqlCommand())
                                    {
                                        command.Connection = connection;
                                        command.CommandText = sqlQuery;

                                        command.Parameters.AddWithValue("relatedProductID", bundleID);
                                        command.Parameters.AddWithValue("productID", titleData.productID);
                                        await command.ExecuteNonQueryAsync();
                                    }
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.XboxGameGenres:
                        {
                            if (data is XboxTitleDetailsData titleData)
                            {
                                string sqlQuery = $@"INSERT INTO {Schemas.xbox}.{table.To_String()} 
                                 (productID) 
                                 VALUES(@productID) 
                                 ON DUPLICATE KEY UPDATE productID = VALUES(productID)";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("productID", titleData.productID);
                                    await command.ExecuteNonQueryAsync();
                                }

                                foreach (var bundleID in titleData.bundleIDs)
                                {
                                    using (var command = new MySqlCommand())
                                    {
                                        command.Connection = connection;
                                        command.CommandText = sqlQuery;
                                        command.Parameters.AddWithValue("productID", bundleID);
                                        await command.ExecuteNonQueryAsync();
                                    }
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.XboxGroupData:
                        {
                            if (data is XboxTitleDetailsData titleData)
                            {

                                string sqlQuery = $@"INSERT INTO {Schemas.xbox}.{table.To_String()}
                                 (groupID, groupName) 
                                 VALUES(@groupID, @groupName) 
                                 ON DUPLICATE KEY UPDATE groupName = VALUES(groupName)";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("groupID", titleData.groupID);
                                    command.Parameters.AddWithValue("groupName", titleData.groupName);
                                    await command.ExecuteNonQueryAsync();
                                }

                                success = true;
                            }
                            break;
                        }

                    default:
                        logger.LogWarning($"DB Update not Implemented for {table.ToString()}");
                        break;
                }

                return success;

            }catch (MySqlException ex) { return false;}
            catch (Exception ex) { return false;}
        }
        private async Task<bool> UpdateSteam(Tables table, TableData data, MySqlConnection connection)
        {
            try
            {
                bool success = false;
                switch (table)
                {
                    case Tables.SteamAppIDs:
                        {
                            if (data is SteamUpdateScannedData updateData)
                            {


                                string sqlQuery = $@"Update {Schemas.steam}.{table.To_String()}
                                 set lastScanned = @lastScanned 
                                 where appID = @appID";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("appID", updateData.ID);
                                    command.Parameters.AddWithValue("lastScanned", updateData.lastScanned);
                                    await command.ExecuteNonQueryAsync();
                                }

                                success = true;
                            }
                            break;
                        }

                    default:
                        logger.LogWarning($"DB Update not Implemented for {table.ToString()}");
                        break;
                }

                return success;

            }
            catch (MySqlException ex) { return false; }
            catch (Exception ex) { return false; }
        }
        private async Task<bool> UpdateGameMarket(Tables table, TableData data, MySqlConnection connection)
        {
            try
            {
                bool success = false;
                switch (table)
                {
                    case Tables.GameMarketGameTitles:
                        {
                            if (data is GameMarketMergedXboxData mergedData)
                            {

                                string sqlQuery = $@"Update {Schemas.gamemarket}.{table.To_String()}
                                 set gameTitle = @gameTitle 
                                 where gameID = @gameID";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("gameTitle", mergedData.gameTitle);
                                    command.Parameters.AddWithValue("gameID", mergedData.gameId);
                                    await command.ExecuteNonQueryAsync();
                                }
                                success = true;
                            }
                            break;
                        }
                    case Tables.GameMarketXboxLink:
                        {
                            if (data is GameMarketMergedXboxData mergedData)
                            {

                                string sqlQuery = $@"Update {Schemas.gamemarket}.{table.To_String()}
                                 set groupID =@groupID
                                 where gameID = @gameID";

                                using (var command = new MySqlCommand())
                                {
                                    command.Connection = connection;
                                    command.CommandText = sqlQuery;

                                    command.Parameters.AddWithValue("groupID", mergedData.groupID);
                                    command.Parameters.AddWithValue("gameID", mergedData.groupID);
                                    await command.ExecuteNonQueryAsync();
                                }
                                success = true;
                            }
                            break;
                        }

                    default:
                        logger.LogWarning($"DB Update not Implemented for {table.ToString()}");
                        break;
                }

                return success;

            }
            catch (MySqlException ex) { return false; }
            catch (Exception ex) { return false; }
        }

        public async Task<bool> validTableAsync(Tables table)
        {
            if (foundTables.Exists(x => x == table))
            {
                return true;
            }
            else
            {
                if (await TableExistsAsync(table))
                {
                    foundTables.Add(table);
                    return true;
                }
                else
                    return false;
            }
        }

        private async Task<bool> TableExistsAsync(Tables table)
        {
            int schemaNum = -1;
            string sqlQuery = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @databaseName AND table_name = @tableName";

            string tableEnum = table.ToString().ToLower();

            if (tableEnum.Contains("xbox")) schemaNum = (int)Schemas.xbox;
            else if (tableEnum.Contains("steam")) schemaNum = (int)Schemas.steam;
            else if (tableEnum.Contains("gamemarket")) schemaNum = (int)Schemas.gamemarket;
            //find the schema of the table



            //Check to see if the table exists in the db
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(sqlQuery, connection))
                {
                    switch (schemaNum)
                    {
                        case (int)Schemas.xbox:
                            command.Parameters.AddWithValue("@databaseName", Schemas.xbox.ToString());
                            break;
                        case (int)Schemas.steam:
                            command.Parameters.AddWithValue("@databaseName", Schemas.steam.ToString());
                            break;
                        case (int)Schemas.gamemarket:
                            command.Parameters.AddWithValue("@databaseName", Schemas.gamemarket.ToString());
                            break;
                        default:
                            command.Parameters.AddWithValue("@databaseName", Schemas.xbox.ToString());
                            break;
                    }

                    command.Parameters.AddWithValue("@tableName", TablesExtension.To_String(table));

                    var result = await command.ExecuteScalarAsync();
                    if (Convert.ToInt32(result) > 0)
                    {
                        Console.WriteLine("Found " + table.To_String() + " in the Database");
                        return true;
                    }

                }
            }



            string sqlFilePath = "";
            switch (table)
            {
                case Tables.XboxUserProfiles:
                    sqlFilePath = "sqlQueries/create xboxuserprofiles.sql";
                    break;
                case Tables.XboxGameTitles:
                    sqlFilePath = "sqlQueries/create xboxgametitles.sql";
                    break;
                case Tables.XboxGameBundles:
                    sqlFilePath = "sqlQueries/create xboxgamebundles.sql";
                    break;
                case Tables.XboxProductIds:
                    sqlFilePath = "sqlQueries/create xboxproductID.sql";
                    break;
                case Tables.XboxGameGenres:
                    sqlFilePath = "sqlQueries/create xboxgamegenres.sql";
                    break;
                case Tables.XboxTitleDetails:
                    sqlFilePath = "sqlQueries/create xboxtitledetails.sql";
                    break;
                case Tables.XboxMarketDetails:
                    sqlFilePath = "sqlQueries/create xboxmarketdetails.sql";
                    break;
                case Tables.XboxGroupData:
                    sqlFilePath = "sqlQueries/create xboxgroupdata.sql";
                    break;
                case Tables.XboxTitleDevices:
                    sqlFilePath = "sqlQueries/create xboxtitledevices.sql";
                    break;


                //steam
                case Tables.SteamAppIDs:
                    sqlFilePath = "sqlQueries/create steamappids.sql";
                    break;
                case Tables.SteamAppGenres:
                    sqlFilePath = "sqlQueries/create steamappgenres.sql";
                    break;
                case Tables.SteamAppDetails:
                    sqlFilePath = "sqlQueries/create steamappdetails.sql";
                    break;
                case Tables.SteamAppDevelopers:
                case Tables.SteamAppPublishers:
                    sqlFilePath = "sqlQueries/create steamappdev-pub.sql";
                    break;
                case Tables.SteamAppPlatforms:
                    sqlFilePath = "sqlQueries/create steamappplatforms.sql";
                    break;
                case Tables.SteamPackageDetails:
                    sqlFilePath = "sqlQueries/create steampackegedetails.sql";
                    break;
                case Tables.SteamPackageIDs:
                    sqlFilePath = "sqlQueries/create steampackageids.sql";
                    break;
                case Tables.SteamPackages:
                    sqlFilePath = "sqlQueries/create steampackages.sql";
                    break;


                case Tables.GameMarketGameTitles:
                    sqlFilePath = "sqlQueries/create gamemarkettitles.sql";
                    break;
                case Tables.GameMarketXboxLink:
                    sqlFilePath = "sqlQueries/create gamemarketxboxlink.sql";
                    break;
                default:
                    sqlFilePath = "";
                    break;
            }



            if (sqlFilePath != "")
            {
                if (await executeSQLFile(sqlFilePath))
                {
                    Console.WriteLine("Created " + table.To_String() + "\n\n");
                    return true;
                }
                else
                {
                    Console.WriteLine("could not create " + table.To_String());
                }
            }
            Console.WriteLine("Invalid SQL FilePath");
            return false;
        }
        private async Task<bool> executeSQLFile(string filePath)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    List<string> sqlQueries = Tools.ReadFromSQLFile(filePath);



                    foreach (var querie in sqlQueries)
                    {
                        using (var command = new MySqlCommand(querie, connection, transaction))
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    await transaction.CommitAsync();
                    if (sqlserverSettings.outputSettings.outputDebug)
                        Console.WriteLine("Sql Ran Successfully");
                    return true;
                }
                catch (MySqlException)
                {
                    Console.WriteLine("Error executing sql");
                    transaction.Rollback();
                    return false;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }


        }

    }
}
