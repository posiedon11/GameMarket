using FuzzySharp;
using FuzzySharp.SimilarityRatio;
using FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;
using GameMarketAPIServer.Configuration;
using GameMarketAPIServer.Models;
using GameMarketAPIServer.Models.Enums;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static SteamKit2.GC.CSGO.Internal.CGameServers_AggregationQuery_Response;

namespace GameMarketAPIServer.Services
{
    public class GameMergerManager
    {
        IDataBaseManager dbManager;
        MainSettings settings;

        public enum SpecialGroupCases
        {
            fission,
            SEJ,
            PCGAMEPass,

        }
        protected List<string> xboxGroupRemoveName = new List<string>()
        {
            "[Fission] ",
            "PC & Game Pass",
            "SEJ_",
        };
        protected List<string> xboxTitleRemoveFromName = new List<string>()
            {
                "for Xbox Series X|S",
                "Xbox Series X|S",
                "for Xbox One",
                "Xbox One Edition",
                "Xbox One",
                "for Windows 10",
                "for Windows",
                "- Windows 10",
                "Windows 10 Edition",
                "PC"
            };
        protected List<string> xboxIgnoreInName = new List<string>()
        {
            "Beta$",
            "Test$",
            "B.E.T.A.$"
        };
        protected List<string> removeReg = new List<string>()
        {
            //removing anything like (something)
            @" \((.*?)\)$",
            //remove all nonletters/numbers
            "[^\\p{L}\\p{Nd}:?;.' ]",
            @"\\s{2,|"
        };
        public GameMergerManager(IDataBaseManager dbManager, IOptions<MainSettings> settings)
        {
            this.dbManager = dbManager;
            this.settings = settings.Value;
        }

        public async Task<Dictionary<string, List<string>>> MergeXboxGamesAsync()
        {
            try
            {
                //title / list of ids
                Dictionary<string, List<string>> mergedXbox = new Dictionary<string, List<string>>();

                //id / title
                Dictionary<string, string> xboxTitles = new Dictionary<string, string>();


                using var connection = new MySqlConnection(dbManager.connectionString);
                await connection.OpenAsync();
                string sql = $"Select modernTitleID, titleName from {DataBaseManager.Schemas.xbox}.{Tables.XboxGameTitles.To_String()}";


                //get the list of xbox titles
                using (var command = connection.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sql;
                    using (var reader = command.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            xboxTitles.Add(reader.GetString(0), reader.GetString(1));
                        }
                        reader.Close();
                    }
                }
                connection.Close();


                Console.WriteLine("OriginalSize: " + xboxTitles.Keys.Count());
                Console.WriteLine($"Merged Size: {mergedXbox.Keys.Count()}");
                string combinedRegExp = $"{string.Join("|", xboxTitleRemoveFromName.ConvertAll(Regex.Escape))}|{string.Join("|", removeReg)}";
                const int scoreThreshold1 = 95;
                const int scoreThreshold2 = 90;
                const int scoreThreshold3 = 80;

                foreach (var titleId in xboxTitles.Keys)
                {
                    string titleName = xboxTitles[titleId];

                    if (Regex.IsMatch(titleName, string.Join("|", xboxIgnoreInName)))
                    {
                        Console.WriteLine($"{titleName}::{titleId} is beta or something\n");
                        continue;
                    }
                    //replace stuff
                    titleName = Regex.Replace(titleName, "\\s+|[-]", " ", RegexOptions.IgnoreCase);

                    //remove stuff
                    titleName = Regex.Replace(titleName, combinedRegExp, "", RegexOptions.IgnoreCase);

                    titleName = titleName.ToLower().Trim();

                    //after normalizing the titles for xbox games.
                    if (!mergedXbox.ContainsKey(titleName))
                        mergedXbox[titleName] = new List<string>();
                    mergedXbox[titleName].Add(titleId);

                    var topMatches = Process.ExtractTop(titleName, mergedXbox.Keys.ToList(), scorer: ScorerCache.Get<DefaultRatioScorer>(), limit: 1);

#if false

                    if (topMatches.Any())
                    {
                        var bestKey = topMatches.First().Value;
                        var topScore = topMatches.First().Score;
                        // if (topScore > scoreThreshold3)
                        //Console.WriteLine($"{titleName} matches with {bestKey} score:{topScore}");
                        switch (topScore)
                        {

                            //perfect match
                            case 100:
                                mergedXbox[bestKey].Add(titleId);
                                Console.WriteLine($"{titleName} matches with {bestKey} score:{topScore}");
                                break;
                            case > scoreThreshold1:
                                mergedXbox[bestKey].Add(titleId);
                                //Console.WriteLine($"{titleName} matches with {bestKey} score:{topScore}");
                                break;
                            default:
                                if (!mergedXbox.ContainsKey(titleName))
                                    mergedXbox[titleName] = new List<string>();
                                mergedXbox[titleName].Add(titleId);
                                break;


                        }
                        if (!mergedXbox[bestKey].Contains(xboxTitles[titleId]))
                        {
                            mergedXbox[bestKey].Add(titleId);
                        }
                    }
                    else
                    {

                        mergedXbox[titleName] = new List<string>();
                        mergedXbox[titleName].Add(titleId);
                    }

#endif



                }

#if false
                foreach (var title in mergedXbox.Keys)
                {
                    if (mergedXbox[title].Count == 1)
                        continue;
                    Console.Write($"Xbox Title: {title}\n\tModernIDs: ");
                    foreach (var id in mergedXbox[title])
                    {
                        Console.Write($"{id}\t");
                    }
                    Console.WriteLine("\n\n");
                }
#endif

                //output each title and similar games.
#if false
                foreach (var title in mergedXbox.Keys)
                {
                    var topMatches1 = Process.ExtractTop(title, mergedXbox.Keys.ToList(), scorer: ScorerCache.Get<DefaultRatioScorer>(), limit: 5);
                    if (topMatches1.First().Score > 95 && topMatches1.First().Score != 100)
                    {
                        Console.Write($"Xbox Title: {title}\n\tModernIDs: ");
                        foreach (var id in mergedXbox[title])
                        {
                            Console.Write($"{id}\t");
                        }
                        Console.WriteLine("");
                    }
                    foreach (var match in topMatches1)
                    {
                        if (match.Score == 100 && match.Value != title)
                        {
                            Console.WriteLine($"{title.Length},  {match.Value.Length}");
                        }
                        if (match.Score > 90 && match.Value != title)
                        {
                            Console.WriteLine($"{title} matches with {match.Value} score:{match.Score}");
                        }
                    }

                } 
#endif


                Console.WriteLine($"OriginalSize: {xboxTitles.Count}");
                Console.WriteLine($"Merged Size: {mergedXbox.Count}");

                return mergedXbox;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

        }
        public async Task<Dictionary<string, List<int>>> MergeSteamGamesAsync()
        {

            try
            {
                Dictionary<string, List<int>> mergedSteam = new Dictionary<string, List<int>>();

                //id / title
                Dictionary<int, string> steamTitles = new Dictionary<int, string>();


                using var connection = new MySqlConnection(dbManager.connectionString);
                await connection.OpenAsync();
                string sql = $"Select appId, appName from {DataBaseManager.Schemas.steam}.{Tables.SteamAppDetails.To_String()} where appType = @appType";


                //get the list of xbox titles
                using (var command = connection.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("appType", $"game");
                    using (var reader = command.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            steamTitles.Add(reader.GetInt32(0), reader.GetString(1));
                        }
                        reader.Close();
                    }
                }
                connection.Close();



                string combinedRegExp = $"{string.Join("|", removeReg)}";
                const int scoreThreshold1 = 95;
                const int scoreThreshold2 = 90;
                const int scoreThreshold3 = 80;


                foreach (var appId in steamTitles.Keys)
                {
                    string titleName = steamTitles[appId];


                    //replace stuff
                    titleName = Regex.Replace(titleName, "\\s+|[-]", " ", RegexOptions.IgnoreCase);

                    //remove stuff
                    titleName = Regex.Replace(titleName, combinedRegExp, "", RegexOptions.IgnoreCase);

                    titleName = titleName.ToLower().Trim();

                    //after normalizing the titles for xbox games.
                    if (!mergedSteam.ContainsKey(titleName))
                        mergedSteam[titleName] = new List<int>();
                    mergedSteam[titleName].Add(appId);

                    //var topMatches = Process.ExtractTop(titleName, mergedSteam.Keys.ToList(), scorer: ScorerCache.Get<DefaultRatioScorer>(), limit: 1);

                }
#if false
                foreach (var title in mergedSteam.Keys)
                {
                    Console.Write($"{title}: ");
                    foreach (var id in mergedSteam[title])
                    {
                        Console.Write($"\t{id}");
                    }
                    Console.WriteLine("\n");
                } 
#endif


                return mergedSteam;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); throw;
            }
        }
        public async Task<Dictionary<string, string>> standardizeGroupName(Dictionary<string, string> groups = null)
        {
            try
            {
                Tables groupTable = Tables.XboxGroupData;
                Tables titleTable = Tables.XboxGroupData;

                using var connection = new MySqlConnection(dbManager.connectionString);
                //Get the list from xbox schema
                if (groups == null) groups = new Dictionary<string, string>();
                if (groups == null || groups.Count == 0)
                {
                    await connection.OpenAsync();
                    string sql = $"Select groupID, groupName from @tableName where groupId is not null";

                    using (var command = connection.CreateCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("tableName", $"{DataBaseManager.Schemas.xbox}.{groupTable.To_String()}");
                        using (var reader = command.ExecuteReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                groups.Add(reader.GetString(0), reader.GetString(1));
                            }
                        }
                    }
                    connection.Close();
                }



                Dictionary<string, string> returnGroups = new Dictionary<string, string>();
                foreach (var groupID in groups.Keys)
                {
                    string groupName = groups[groupID];
                    //remove all reg commands in it;
                    foreach (SpecialGroupCases thing in Enum.GetValues(typeof(SpecialGroupCases)))
                    {
                        if (Regex.IsMatch(groupName, Regex.Escape(thing.ToString()), RegexOptions.IgnoreCase))
                        {
                            switch (thing)
                            {
                                //Some group names are weird
                                case SpecialGroupCases.PCGAMEPass:
                                case SpecialGroupCases.SEJ:
                                    {
                                        await connection.OpenAsync();
                                        using (var command = connection.CreateCommand())
                                        {
                                            command.Connection = connection;
                                            command.CommandText = $"Select titleName from {DataBaseManager.Schemas.xbox}.{Tables.XboxGameTitles.To_String()} where groupId = @groupId";
                                            command.Parameters.AddWithValue("groupID", groupID);

                                            using (var reader = command.ExecuteReader())
                                            {
                                                while (await reader.ReadAsync())
                                                {
                                                    groupName = reader.GetString(0);
                                                }
                                            }

                                            connection.Close();
                                        }
                                        break;
                                    }

                                //remove fission
                                case SpecialGroupCases.fission:
                                    {
                                        groupName = Regex.Replace(groupName, Regex.Escape(thing.To_String()), "", RegexOptions.IgnoreCase);
                                        groupName = Regex.Replace(groupName, @" \((.*?)\)$", "");
                                    }
                                    break;
                            }
                        }
                    }



                    if (groupName != "")
                        groups[groupID] = groupName;

                    foreach (var reg in removeReg)
                    {
                        groupName = Regex.Replace(groupName, reg, "", RegexOptions.IgnoreCase);
                    }

                    groupName = groupName.ToLower();

                    foreach (var substr in xboxGroupRemoveName)
                    {
                        //IDK why this games group is this
                        groupName = groupName.Replace(substr.ToLower(), "");

                    }
                    returnGroups.Add(groupID, groupName);


                }






                return returnGroups;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Dictionary<string, string>();
            }
        }

        public async Task mergeXboxToGameMarketGames()
        {
            try
            {
                Tables table = Tables.GameMarketGameTitles;
                if (!await dbManager.validTableAsync(table))
                {
                    return;
                }
                Dictionary<int, string> gameMarketGames = new Dictionary<int, string>();

                using var connection = new MySqlConnection(dbManager.connectionString);
                await connection.OpenAsync();
                string sql = $"Select gameId, gameTitle from {DataBaseManager.Schemas.gamemarket}.{table.To_String()}";

                using (var command = connection.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sql;

                    using (var reader = command.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            gameMarketGames.Add(reader.GetInt32(0), reader.GetString(1).ToLower());
                        }
                    }
                }



                Dictionary<string, List<string>> xboxGames = await MergeXboxGamesAsync();
                //Search for existing
                var minMatchThreshold = 80;

                int queueSize = 4;
                foreach (var xboxGame in xboxGames.Keys)
                {

                    PriorityQueue<int, int> topRe = new PriorityQueue<int, int>(queueSize, Comparer<int>.Create((x, y) => x.CompareTo(y)));
                    // var topResults = Process.ExtractTop(xboxGame, gameMarketGames.Values, scorer: ScorerCache.Get<PartialRatioScorer>() );
                    //get the top relusts for fuzz match.
                    foreach (var marketGame in gameMarketGames.Keys)
                    {
                        var score = Fuzz.PartialRatio(xboxGame, gameMarketGames[marketGame]);
                        topRe.Enqueue(marketGame, score);
                        while (topRe.Count > queueSize)
                        {
                            topRe.Dequeue();
                        }
                    }
                    var topMa = topRe.UnorderedItems.OrderByDescending(x => x.Priority).ToList();
                    ;

                    //foreach(var match in topMa)
                    //{
                    //    Console.WriteLine($"{xboxGame} matches with {gameMarketGames[match.Element]} with score: {match.Priority}");
                    //}

                    if (topMa.Any() && topMa.First().Priority > minMatchThreshold)
                    {
                        foreach (var match in topMa)
                        {
                            switch (match.Priority)
                            {
                                case 100:
                                    var mergeData = new GameMarketMergedXboxData()
                                    {
                                        gameId = match.Element,
                                        gameTitle = gameMarketGames[match.Element],
                                        titleIds = xboxGames[xboxGame]
                                    };
                                    await dbManager.EnqueueGameMarketQueueAsync(Tables.GameMarketGameTitles, mergeData, CRUD.Update);
                                    await dbManager.EnqueueGameMarketQueueAsync(Tables.GameMarketXboxLink, mergeData, CRUD.Create);
                                    await dbManager.processGameMarketQueueAsync();
                                    break;
                                default:
                                    await dbManager.EnqueueGameMarketQueueAsync([Tables.GameMarketGameTitles, Tables.GameMarketXboxLink], new GameMarketMergedXboxData() { gameId = 0, gameTitle = xboxGame, titleIds = xboxGames[xboxGame] }, CRUD.Create);
                                    await dbManager.processGameMarketQueueAsync();
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"No Mathes found for {xboxGame}");
                        await dbManager.EnqueueGameMarketQueueAsync([Tables.GameMarketGameTitles, Tables.GameMarketXboxLink], new GameMarketMergedXboxData() { gameId = 0, gameTitle = xboxGame, titleIds = xboxGames[xboxGame] }, CRUD.Create);
                        await dbManager.processGameMarketQueueAsync();
                    }
#if false
                    //If there are results
                    if (topResults.Any() && topResults.First().Score > 70)
                    {
                        foreach (var result in topResults)
                        {
                            if (result.Score > 80)
                            {
                                // Console.WriteLine($"{xboxGame} matches with {result.Value} with score: {result.Score}");
                            }
                            switch (result.Score)
                            {
                                case 100:
                                    await dbManager.InsertGameMarketQueueAsync([Tables.GameMarketGameTitles, Tables.GameMarketXboxLink], new GameMarketMergedXboxData() { gameId = 0, gameTitle = xboxGame, titleIds = xboxGames[xboxGame] }, CRUD.Create);
                                    await dbManager.processGameMarketQueueAsync();
                                    break;
                                case 95:
                                    break;
                                case 90:
                                    break;
                                case 80:

                                    break;
                            }
                        }
                    }
                    //no results
                    else
                    {
                        Console.WriteLine($"No Mathes found for {xboxGame}");
                        await dbManager.InsertGameMarketQueueAsync([Tables.GameMarketGameTitles, Tables.GameMarketXboxLink], new GameMarketMergedXboxData() { gameId = 0, gameTitle = xboxGame, titleIds = xboxGames[xboxGame] }, CRUD.Create);
                        await dbManager.processGameMarketQueueAsync();
                    } 
#endif
                }
                //Add to it
                //Fuzzy match found nothing
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        }

        public async Task mergeSteamToGameMarketGames()
        {
            Tables table = Tables.GameMarketGameTitles;
            if (!await dbManager.validTableAsync(table))
            {
                return;
            }

            Dictionary<int, string> gameMarketGames = new Dictionary<int, string>();

            using var connection = new MySqlConnection(dbManager.connectionString);
            await connection.OpenAsync();
            string sql = $"Select gameId, gameTitle from {DataBaseManager.Schemas.gamemarket}.{table.To_String()}";

            using (var command = connection.CreateCommand())
            {
                command.Connection = connection;
                command.CommandText = sql;

                using (var reader = command.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        gameMarketGames.Add(reader.GetInt32(0), reader.GetString(1).ToLower());
                    }
                }
            }

            connection.Close();



            var steamGames = await MergeSteamGamesAsync();
            var minMatchThreshold = 80;

            int queueSize = 4;

            foreach (var steamgame in steamGames.Keys)
            {


                PriorityQueue<int, int> topResults = new PriorityQueue<int, int>(queueSize, Comparer<int>.Create((x, y) => x.CompareTo(y)));
                // var topResults = Process.ExtractTop(xboxGame, gameMarketGames.Values, scorer: ScorerCache.Get<PartialRatioScorer>() );
                //get the top relusts for fuzz match.
                foreach (var marketGame in gameMarketGames.Keys)
                {
                    var score = Fuzz.PartialRatio(steamgame, gameMarketGames[marketGame]);
                    topResults.Enqueue(marketGame, score);
                    while (topResults.Count > queueSize)
                    {
                        topResults.Dequeue();
                    }
                }
                var topMatcjes = topResults.UnorderedItems.OrderByDescending(x => x.Priority).ToList();

                foreach (var match in topMatcjes)
                {
                    Console.WriteLine($"{steamgame} matches with {gameMarketGames[match.Element]} with score: {match.Priority}");
                }
                Console.WriteLine("\n");
            }
        }
    }
}
