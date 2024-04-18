using FuzzySharp;
using FuzzySharp.SimilarityRatio;
using FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;
using GameMarketAPIServer.Configuration;
using GameMarketAPIServer.Models;
using GameMarketAPIServer.Models.Enums;
using GameMarketAPIServer.Utilities;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static SteamKit2.GC.CSGO.Internal.CGameServers_AggregationQuery_Response;

namespace GameMarketAPIServer.Services
{

    public class GameTitle
    {
        protected ILogger logger { get; }
        public string titleName { get; set; }
        public SortedSet<string> developers { get; set; }
        public SortedSet<string> publishers { get; set; }

        public GameTitle(ILogger logger)
        {
            this.logger = logger;
            titleName = string.Empty;
            developers = new SortedSet<string>();
            publishers = new SortedSet<string>();
        }
        public static bool isSequel(string title1, string title2)
        {
            var regex = new Regex(@"\d+$");
            var match1 = regex.Match(title1);
            var match2 = regex.Match(title2);
            if (title1 == "zombie derby 2" && title2 == "zombie derby")
            {
                Console.WriteLine();
            }
            if (match1.Success && match2.Success)
                return match1.Value != match2.Value;
            else
            {
                return match1.Success != match2.Success;
            }
        }
        public bool isSequel(string title1)
        {
            return isSequel(titleName, title1);
        }
    }
    public class GamePlatformTitle : GameTitle
    {
        public SortedSet<string> ids { get; set; }
        public DataBaseManager.Schemas schema { get; set; }
        public GamePlatformTitle(ILogger logger, DataBaseManager.Schemas schema) : base(logger)
        {
            this.schema = schema;
            ids = new SortedSet<string>();
        }

        protected List<string> removeReg = new List<string>()
        {
            //removing anything like (something)
            @" \((.*?)\)$",
            //remove all nonletters/numbers
            "[^\\p{L}\\p{Nd}:?;.' ]",
            @"\\s{2,|"
        };
        public void Normalize()
        {

            //replace stuff
            titleName = Regex.Replace(titleName, "\\s+|[-]", " ", RegexOptions.IgnoreCase);

            switch (schema)
            {
                case DataBaseManager.Schemas.xbox: NormalizeXbox(); break;
                case DataBaseManager.Schemas.steam: NormalizeSteam(); break;
            }
        }
        /// <summary>
        /// Outputs data in different contexs
        /// </summary>
        /// <param name="mode">0: all<para> 1: normalTest</para><para>2: mergeTest.</para></param>
        public void output(int mode = 0)
        {
            string devOutput = "\t\t";
            string pubOutput = "\t\t";
            string idOutput = "\t\t";
            foreach (var dev in developers)
                devOutput += $"{dev}; ";
            foreach (var pub in publishers)
                pubOutput += $"{pub}; ";
            foreach (var id in ids)
                idOutput += $"{id}; ";

            logger.LogDebug(titleName);
            logger.LogDebug($"\tDevelopers:\n{devOutput}");
            logger.LogDebug($"\tPublishers:\n{pubOutput}\n");
            logger.LogDebug($"\tIDs:\n{idOutput}");
        }

        private void NormalizeXbox()
        {
            List<string> xboxTitleRemoveFromName = new List<string>()
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
            string combinedRegExp = $"{string.Join("|", xboxTitleRemoveFromName.ConvertAll(Regex.Escape))}|{string.Join("|", removeReg)}";
            //remove stuff
            titleName = Regex.Replace(titleName, combinedRegExp, "", RegexOptions.IgnoreCase).ToLower().Trim();
            var delimiters = new string[] { "/", ",", "|", ";" };
            string pattern = @"\s*(,|\s)\s*(LLC|LTD|INC|S\.?A\.?|INC\.|CORPORATION|CORP|PTY\.?|L\.?P\.?|GMBH)\.?([\/,|;]\s*|$)\s*";
            titleName = Regex.Replace(titleName, pattern, "", RegexOptions.IgnoreCase);

            var normalDevelopers = new SortedSet<string>(developers.Select(dev => Regex.Replace(dev, pattern, "$3", RegexOptions.IgnoreCase)));
            var normalPublishers = new SortedSet<string>(publishers.Select(pub => Regex.Replace(pub, pattern, "$3", RegexOptions.IgnoreCase)));

            developers.Clear();
            publishers.Clear();
            foreach (string normalDev in normalDevelopers)
            {
                foreach (string part in normalDev.Split(delimiters, StringSplitOptions.RemoveEmptyEntries))
                {
                    developers.Add(part.Trim());
                }
            }
            foreach (string normalPub in normalPublishers)
            {
                foreach (string part in normalPub.Split(delimiters, StringSplitOptions.RemoveEmptyEntries))
                {
                    publishers.Add(part.Trim());
                }
            }
        }

        private void NormalizeSteam() { }

    };

    public class GameMarketTitle : GameTitle
    {
        public Int32 gameID { get; private set; }
        public SortedSet<string>? xboxIds { get; set; }
        public SortedSet<string>? steamIds { get; set; }

        public Dictionary<DataBaseManager.Schemas, SortedSet<string>> platformIds;

        public GameMarketTitle(ILogger logger, Int32 gameId) : base(logger)
        {
            this.gameID = gameID;
            xboxIds = new SortedSet<string>();
            steamIds = new SortedSet<string>();
            platformIds = new Dictionary<DataBaseManager.Schemas, SortedSet<string>>();
        }
        public GameMarketTitle(ILogger logger, GamePlatformTitle platformTitle) : base(logger)
        {
            gameID = 0;
            xboxIds = new SortedSet<string>();
            steamIds = new SortedSet<string>();
            platformIds = new Dictionary<DataBaseManager.Schemas, SortedSet<string>>();
        }

        public bool hasValidIds()
        {
            return platformIds.Any(p=>p.Value != null && p.Value.Count > 0);
        }
        public void JoinPlatformTitle(GamePlatformTitle platformTitle)
        {
            if (titleName == "") titleName = platformTitle.titleName;
            if (platformTitle.schema == DataBaseManager.Schemas.gamemarket) { return; }
            developers.UnionWith(platformTitle.developers);
            publishers.UnionWith(platformTitle.publishers);

            if (!platformIds.ContainsKey(platformTitle.schema))
                platformIds.Add(platformTitle.schema, new SortedSet<string>());
            platformIds[platformTitle.schema].UnionWith(platformTitle.ids);


            switch (platformTitle.schema)
            {
                case DataBaseManager.Schemas.xbox:
                    {
                        xboxIds.UnionWith(platformTitle.ids);
                        break;
                    }
                case DataBaseManager.Schemas.steam:
                    {
                        steamIds.UnionWith(platformTitle.ids);
                        break;
                    }
                default:
                    break;
            }
        }


    }
    public class GameMarketTitleComparer : IComparer<GameMarketTitle>
    {
        private readonly int threshold;
        public GameMarketTitleComparer(int similarityThreshold = 90)
        {
            threshold = similarityThreshold;
        }

        public int Compare(GameMarketTitle x, GameMarketTitle y)
        {
            int ratioScore = Fuzz.Ratio(x.titleName, y.titleName);
            int partialRatioScore = Fuzz.PartialRatio(x.titleName, y.titleName);
            int comp = string.Compare(x.titleName, y.titleName, StringComparison.Ordinal);

            if (ratioScore == 100)
            {
                //if the title is the same, and it has the same dev/pubs, most likely same game
                if (x.developers.Any(d => y.developers.Contains(d)) || x.publishers.Any(d => y.publishers.Contains(d)))
                {
                    return 0;
                }
                else
                    return 1;
            }
            else if (ratioScore > threshold && partialRatioScore > threshold)
            {
                int devScore = Fuzz.TokenSortRatio(string.Join(" ", x.developers), string.Join(" ", y.developers));
                int pubScore = Fuzz.TokenSortRatio(string.Join(" ", x.publishers), string.Join(" ", y.publishers));

                if (devScore > threshold && pubScore > threshold) return 0;
                return 1;

            }
            else
                return comp;
        }
    }
    public class GamePlatformTitleComparer : IComparer<GamePlatformTitle>
    {
        private readonly int threshold;
        public GamePlatformTitleComparer(int similarityThreshold = 90)
        {
            threshold = similarityThreshold;
        }
        public int Compare(GamePlatformTitle x, GamePlatformTitle y)
        {

            if (GameTitle.isSequel(x.titleName, y.titleName))
                return string.Compare(x.titleName, y.titleName, StringComparison.Ordinal);
            int ratioScore = Fuzz.Ratio(x.titleName, y.titleName);
            int partialRatioScore = Fuzz.PartialRatio(x.titleName, y.titleName);
            if (ratioScore == 100)
            {
                //check to see if ids match
                if (x.ids == y.ids)
                {
                    if (x.developers == y.developers && x.publishers == y.publishers)
                    {
                        return 0;
                    }
                }
                else
                {
                    return string.Compare(x.ids.First(), y.ids.First());
                }
            }

            if (ratioScore > threshold && partialRatioScore > threshold)
            {
                int devScore = Fuzz.TokenSortRatio(string.Join(" ", x.developers), string.Join(" ", y.developers));
                int pubScore = Fuzz.TokenSortRatio(string.Join(" ", x.publishers), string.Join(" ", y.publishers));

                if (devScore > threshold && pubScore > threshold) return 0;
                return 0;
            }
            return string.Compare(x.titleName, y.titleName, StringComparison.Ordinal);
        }

    }
    public class GameMergerManager
    {
        private IDataBaseManager dbManager;
        private ILogger logger;
        private MainSettings settings;

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
        protected List<string> xboxIgnoreInName = new List<string>()
        {
            "Beta$",
            "Test$",
            "B.E.T.A.$"
        };


        public GameMergerManager(IDataBaseManager dbManager, IOptions<MainSettings> settings, ILogger<GameMergerManager> logger)
        {
            this.dbManager = dbManager;
            this.settings = settings.Value;
            this.logger = logger;
        }

        public async Task<SortedSet<GamePlatformTitle>> MergeXboxGamesAsync(SortedDictionary<string, GamePlatformTitle>? testTitles = null)
        {
            try
            {
                //title / list of ids
                SortedDictionary<string, List<string>> mergedXbox = new SortedDictionary<string, List<string>>();

                SortedSet<GamePlatformTitle> mergedTitles = new SortedSet<GamePlatformTitle>(new GamePlatformTitleComparer());
                //id / title
                //Dictionary<string, string> xboxTitles = new Dictionary<string, string>();

                SortedDictionary<string, GamePlatformTitle> xboxTitles = new SortedDictionary<string, GamePlatformTitle>();

                if (testTitles == null)
                {


                    using var connection = new MySqlConnection(dbManager.connectionString);
                    DataBaseManager.Schemas schema = DataBaseManager.Schemas.xbox;
                    await connection.OpenAsync();

                    string gameTitlesSQL = $"Select modernTitleID, titleName from {schema}.{Tables.XboxGameTitles.To_String()} order by modernTitleID";
                    //get the list of xbox titles
                    using (var command = connection.CreateCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = gameTitlesSQL;
                        using (var reader = command.ExecuteReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                xboxTitles.Add(reader.GetString(0), new GamePlatformTitle(logger, DataBaseManager.Schemas.xbox) { titleName = reader.GetString(1) });

                            }
                            reader.Close();
                        }
                    }

                    //get the dev and pubs from each productID directly linked to titleID
                    string gameDevPub = $@"Select modernTitleID, developerName, publisherName from {schema}.{Tables.XboxMarketDetails.To_String()} 
                    inner join {schema}.{Tables.XboxTitleDetails.To_String()} on {schema}.{Tables.XboxTitleDetails.To_String()}.productID
                    = {schema}.{Tables.XboxMarketDetails.To_String()}.productID order by modernTitleID";
                    using (var command = connection.CreateCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = gameDevPub;
                        using (var reader = command.ExecuteReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                var titleID = reader.GetString(0);
                                var developers = reader.GetString(1);
                                var publishers = reader.GetString(2);
                                //foreach(var developer in developers.Split("/"))
                                xboxTitles[titleID].developers.Add(developers);
                                //foreach(var publisher in publishers.Split("/"))
                                xboxTitles[titleID].publishers.Add(publishers);
                            }
                            reader.Close();
                        }
                    }
                    connection.Close();
                }
                else
                {
                    xboxTitles = testTitles;
                }
                logger.LogTrace($"OriginalSize: {xboxTitles.Keys.Count()}");
                logger.LogTrace($"Merged Size: {mergedTitles.Count()}");

                const int scoreThreshold1 = 95;
                const int scoreThreshold2 = 90;
                const int scoreThreshold3 = 80;

                //create dev and pub list
                Dictionary<string, List<GamePlatformTitle>> developerTitles = new Dictionary<string, List<GamePlatformTitle>>();
                Dictionary<string, List<GamePlatformTitle>> publisherTitles = new Dictionary<string, List<GamePlatformTitle>>();

                //populate dev and pub list
                foreach (var game in xboxTitles.Values)
                {
                    game.Normalize();
                    foreach (var dev in game.developers)
                    {
                        if (!developerTitles.ContainsKey(dev))
                            developerTitles[dev] = new List<GamePlatformTitle>();
                        developerTitles[dev].Add(game);
                    }
                    foreach (var pub in game.publishers)
                    {
                        if (!publisherTitles.ContainsKey(pub))
                            publisherTitles[pub] = new List<GamePlatformTitle>();
                        publisherTitles[pub].Add(game);
                    }
                }

                foreach (var titleID in xboxTitles.Keys)
                {
                    var title = xboxTitles[titleID];
                    bool devMatch = false, pubMatch = false;

                    //title will be ignored
                    if (Regex.IsMatch(title.titleName, string.Join("|", xboxIgnoreInName)))
                    {
                        logger.LogTrace($"{title.titleName}::{titleID} is beta or something\n");
                        continue;
                    }



                    //try to find a match for developers
                    foreach (var dev in title.developers)
                    {
                        if (developerTitles.ContainsKey(dev))
                        {
                            var validDevs = developerTitles[dev].Where(t => t != title).ToList();
                            //find the top matching developers
                            var topDevs = Process.ExtractTop(title.titleName, validDevs.Select(t => t.titleName).ToList(), scorer: ScorerCache.Get<DefaultRatioScorer>(), limit: 3);

                            if (topDevs.Any())
                            {

                                //foreach (var topDev in topDevs)
                                //{
                                //    var temp = developerTitles[topDev.Value];
                                //}
                            }

                            devMatch = topDevs.Any(td => td.Score > 95);
                            if (devMatch) break;
                        }

                    }
                    //try to find a match for publishers
                    foreach (var pub in title.publishers)
                    {
                        if (publisherTitles.ContainsKey(pub))
                        {
                            var validPubs = publisherTitles[pub].Where(t => t != title).ToList();
                            //find the top publishers
                            var topPubs = Process.ExtractTop(title.titleName, validPubs.Select(t => t.titleName).ToList(), scorer: ScorerCache.Get<DefaultRatioScorer>(), limit: 3);
                            pubMatch = topPubs.Any(td => td.Score > 95);
                            if (pubMatch) break;
                        }
#if false
                        if (true)
                        {
                            pubMatch = true;
                        }
                        else { pubMatch = false; } 
#endif
                    }

                    bool matchFound = false;
                    //pretty much the same game
                    if (devMatch && pubMatch)
                    {

                        //GameTitle found = mergedTitle.FirstOrDefault(gt =>
                        //Fuzz.Ratio(gt.titleName, title.titleName) > 90 &&
                        //!GameTitleComparer.isSequel(gt.titleName,title.titleName) &&
                        //gt.developers.Any(d => title.developers.Contains(d)) &&
                        //gt.publishers.Any(p => title.publishers.Contains(p)));


                        GamePlatformTitle found = mergedTitles
                            .Where(gt => !gt.isSequel(title.titleName) &&
                            Fuzz.Ratio(gt.titleName, title.titleName) > 90 &&
                             gt.developers.Any(d => title.developers.Contains(d)) &&
                             gt.publishers.Any(p => title.publishers.Contains(p)))
                            .OrderByDescending(gt => Fuzz.Ratio(gt.titleName, title.titleName))  // Order by descending match quality
                            .FirstOrDefault();


                        if (found != null)
                        {
                            logger.LogDebug($"Dev & Pub Match for:  {titleID}. Merging: ");
                            title.output(2);
                            found.output(2);
                            matchFound = true;
                            found.developers.UnionWith(title.developers);
                            found.publishers.UnionWith(title.publishers);
                            found.ids.Add(titleID);
                        }
                    }
                    //most likely the same game
                    else if (devMatch)
                    {
                        GamePlatformTitle found = mergedTitles.FirstOrDefault(gt =>
                        Fuzz.Ratio(gt.titleName, title.titleName) > 90 &&
                        gt.developers.Any(d => title.developers.Contains(d)));


                        if (found != null)
                        {
                            logger.LogDebug($"Dev Match for:  {titleID}. Merging:");
                            title.output(2);
                            found.output(2);
                            matchFound = true;
                            found.developers.UnionWith(title.developers);
                            found.publishers.UnionWith(title.publishers);
                            found.ids.Add(titleID);
                        }
                    }
                    else if (pubMatch)
                    {
                        GamePlatformTitle found = mergedTitles.FirstOrDefault(gt =>
                        Fuzz.PartialRatio(gt.titleName, title.titleName) > 90 &&
                        gt.publishers.Any(p => title.publishers.Contains(p)));
                        if (found != null)
                        {
                            logger.LogDebug($"Pub Match for:  {titleID}. Merging: ");
                            title.output(2);
                            found.output(2);
                            matchFound = true;
                            found.developers.UnionWith(title.developers);
                            found.publishers.UnionWith(title.publishers);
                            found.ids.Add(titleID);
                        }

                    }
                    //unlikely to have a match.
                    if (!matchFound)
                    {
                        title.ids.Add(titleID);
                        mergedXbox[title.titleName] = [titleID];
                        mergedTitles.Add(title);
                    }
                }
                logger.LogTrace($"OriginalSize: {xboxTitles.Keys.Count()}");
                logger.LogTrace($"Merged Size: {mergedTitles.Count()}");
                logger.LogTrace($"Titles Merged: {xboxTitles.Keys.Count() - mergedTitles.Count()}");
                return mergedTitles;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
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



                string combinedRegExp = $"";
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

        public async Task mergeToGameMarket(DataBaseManager.Schemas mergeSchema, SortedDictionary<string, GamePlatformTitle>? testTitles = null)
        {
            try
            {
                SortedSet<GamePlatformTitle> gameMergeTitles = new SortedSet<GamePlatformTitle>();
                switch (mergeSchema)
                {
                    case DataBaseManager.Schemas.xbox:
                        gameMergeTitles = await MergeXboxGamesAsync(testTitles);
                        break;
                    case DataBaseManager.Schemas.steam:
                        break;

                    //Cant merge a gamemarket game
                    case DataBaseManager.Schemas.gamemarket:
                        return;
                }

                if (gameMergeTitles == null || gameMergeTitles.Count == 0)
                {
                    logger.LogDebug("Titles to merge is empty");
                    return;
                }

                if (!await dbManager.validTableAsync(Tables.GameMarketGameTitles)) return;

                Dictionary<int, string> gameMarketGames = new Dictionary<int, string>();
                SortedSet<GameMarketTitle> marketTitles = new SortedSet<GameMarketTitle>(new GameMarketTitleComparer());


                using var connection = new MySqlConnection(dbManager.connectionString);
                await connection.OpenAsync();
                //string sql = $"Select gameId, gameTitle from {DataBaseManager.Schemas.gamemarket}.{Tables.GameMarketGameTitles.To_String()}";
                var schema = DataBaseManager.Schemas.gamemarket;
                string titlesTable = Tables.GameMarketGameTitles.To_String(), publisherTable = Tables.GameMarketPublishers.To_String(), developerTable = Tables.GameMarketDevelopers.To_String();
                string sql = $@"Select {titlesTable}.gameID, {titlesTable}.gametitle, {developerTable}.developer, {publisherTable}.publisher
                    from {schema}.{titlesTable}
                    inner join {schema}.{developerTable} on {titlesTable}.gameID = {developerTable}.gameID
                    inner join {schema}.{publisherTable} on {titlesTable}.gameID = {publisherTable}.gameID";
                using (var command = connection.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sql;

                    using (var reader = command.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            gameMarketGames.Add(reader.GetInt32(0), reader.GetString(1).ToLower());
                            marketTitles.Add(new GameMarketTitle(logger, reader.GetInt32(0)) { titleName = reader.GetString(1).ToLower() });
                        }
                    }
                }


                connection.Close();
                Dictionary<string, List<GameMarketTitle>> developerTitles = new Dictionary<string, List<GameMarketTitle>>();
                Dictionary<string, List<GameMarketTitle>> publisherTitles = new Dictionary<string, List<GameMarketTitle>>();

                foreach (var marketTitle in marketTitles)
                {
                    foreach (var dev in marketTitle.developers)
                    {
                        if (!developerTitles.ContainsKey(dev))
                        {
                            developerTitles[dev] = new List<GameMarketTitle>();
                        }
                        developerTitles[dev].Add(marketTitle);
                    }
                    foreach (var pub in marketTitle.publishers)
                    {
                        if (!publisherTitles.ContainsKey(pub))
                        {
                            publisherTitles[pub] = new List<GameMarketTitle>();
                        }
                        publisherTitles[pub].Add(marketTitle);
                    }
                }


                foreach (var title in gameMergeTitles)
                {
                    bool devMatch = false, pubMatch = false;

                    //try to find a match for developers
                    foreach (var dev in title.developers)
                    {
                        if (developerTitles.ContainsKey(dev))
                        {
                            var validDevs = developerTitles[dev].Where(t => Fuzz.Ratio(t.titleName, title.titleName) > 80).ToList();
                            //find the top matching developers
                            var topDevs = Process.ExtractTop(title.titleName, validDevs.Select(t => t.titleName).ToList(), scorer: ScorerCache.Get<DefaultRatioScorer>(), limit: 3);

                            if (topDevs.Any())
                            {

                                //foreach (var topDev in topDevs)
                                //{
                                //    var temp = developerTitles[topDev.Value];
                                //}
                            }

                            devMatch = topDevs.Any(td => td.Score > 95);
                            if (devMatch) break;
                        }

                    }

                    //try to find a match for publishers
                    foreach (var pub in title.publishers)
                    {
                        var fda = Process.ExtractTop(pub, publisherTitles.Keys, scorer: ScorerCache.Get<PartialRatioScorer>(), limit: 3, cutoff: 90);
                        if (publisherTitles.ContainsKey(pub))
                        {
                            var validPubs = publisherTitles[pub].Where(t => Fuzz.Ratio(t.titleName, title.titleName) > 80).ToList();
                            //find the top publishers
                            var topPubs = Process.ExtractTop(title.titleName, validPubs.Select(t => t.titleName).ToList(), scorer: ScorerCache.Get<DefaultRatioScorer>(), limit: 3);
                            pubMatch = topPubs.Any(td => td.Score > 95);
                            if (pubMatch) break;
                        }
#if false
                        if (true)
                        {
                            pubMatch = true;
                        }
                        else { pubMatch = false; } 
#endif
                    }


                    bool matchFound = false;
                    if (devMatch || pubMatch)
                    {
                        var matchList = marketTitles.Where(gt => !gt.isSequel(title.titleName) &&
                            Fuzz.Ratio(gt.titleName, title.titleName) > 90)
                            .OrderByDescending(gt => Fuzz.Ratio(gt.titleName, title.titleName));
                        if (devMatch)
                        {
                            matchList = matchList.Where(gt => gt.developers.Any(d => title.developers.Contains(d)))
                                .OrderByDescending(gt => Fuzz.Ratio(gt.titleName, title.titleName));
                        }
                        if (pubMatch)
                        {
                            matchList = matchList.Where(gt => gt.publishers.Any(d => title.publishers.Contains(d)))
                                .OrderByDescending(gt => Fuzz.Ratio(gt.titleName, title.titleName));
                        }

                        var found = matchList.FirstOrDefault();


                        if (found != null)
                        {
                            matchFound = true;
                            found.JoinPlatformTitle(title);
                        }
                    }

                    if (!matchFound)
                    {
                        var temp = new GameMarketTitle(logger, 0);
                        temp.titleName=title.titleName;
                        temp.JoinPlatformTitle(title);
                        marketTitles.Add(temp);
                    }

                }

                await insertIntoDB(marketTitles);
            }
            catch (Exception ex) { logger.LogError(ex.ToString());
                return; }


            await dbManager.processGameMarketQueueAsync();
        }


        private async Task insertIntoDB(SortedSet<GameMarketTitle> marketTitles)
        {
            foreach(var title in marketTitles)
            {
                if (!title.hasValidIds()) continue;
                var mergeData = new GameMarketMergedData(title.gameID)
                {
                    developers = title.developers,
                    publishers = title.publishers,
                    xboxIds = title.xboxIds,
                    steamIds = title.steamIds,
                    platformIds = title.platformIds
                };
                if (mergeData.getGameID() == 0)
                    await dbManager.EnqueueGameMarketQueueAsync(Tables.GameMarketGameTitles, mergeData, CRUD.Create);
                else
                    await dbManager.EnqueueGameMarketQueueAsync(Tables.GameMarketGameTitles, mergeData, CRUD.Update);

                if (mergeData.developers != null && mergeData.developers.Any())
                    await dbManager.EnqueueGameMarketQueueAsync(Tables.GameMarketDevelopers, mergeData, CRUD.Create);
                
                if (mergeData.publishers != null && !mergeData.publishers.Any())
                    await dbManager.EnqueueGameMarketQueueAsync(Tables.GameMarketPublishers, mergeData, CRUD.Create);

                foreach(var kvp in mergeData.platformIds.Where(s=>s.Value.Any()))
                {
                    switch(kvp.Key)
                    {
                        case DataBaseManager.Schemas.xbox:
                            await dbManager.EnqueueGameMarketQueueAsync(Tables.GameMarketXboxLink, mergeData, CRUD.Create);
                            break;
                        case DataBaseManager.Schemas.steam:
                            await dbManager.EnqueueGameMarketQueueAsync(Tables.GameMarketSteamLink, mergeData, CRUD.Create);
                            break;
                    }
                    
                }
                //if xboxIds and steam ids are empty, dont add it
                //add to xbox
                if (mergeData.xboxIds != null && mergeData.xboxIds.Count() > 0)
                {
                    await dbManager.EnqueueGameMarketQueueAsync(Tables.GameMarketGameTitles, mergeData, CRUD.Update);
                    await dbManager.EnqueueGameMarketQueueAsync(Tables.GameMarketXboxLink, mergeData, CRUD.Create);
                    await dbManager.processGameMarketQueueAsync();
                }
            }
        }
        public async Task mergeXboxToGameMarketGames()
        {
            try
            {
                Tables table = Tables.GameMarketGameTitles;
                if (!await dbManager.validTableAsync(Tables.GameMarketGameTitles))
                {
                    return;
                }
                Dictionary<int, string> gameMarketGames = new Dictionary<int, string>();

                using var connection = new MySqlConnection(dbManager.connectionString);
                await connection.OpenAsync();
                string sql = $"Select gameId, gameTitle from {DataBaseManager.Schemas.gamemarket}.{Tables.GameMarketGameTitles.To_String()}";

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



                var xboxGames = await MergeXboxGamesAsync(null);
                //Search for existing
                var minMatchThreshold = 80;

                int queueSize = 4;
#if false
                foreach (var xboxGame in xboxGames)
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
#endif
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
