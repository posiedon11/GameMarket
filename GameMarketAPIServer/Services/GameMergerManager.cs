using AutoMapper;
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
using static GameMarketAPIServer.Models.DataBaseSchemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.FileSystemGlobbing.Internal;

namespace GameMarketAPIServer.Services
{
    #region GameTitleClasses

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
        public DBSchema schema { get; set; }
        public GamePlatformTitle(ILogger logger, DBSchema schema) : base(logger)
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
                case Database_structure.XboxSchema: NormalizeXbox(); break;
                case Database_structure.SteamSchema: NormalizeSteam(); break;
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
            //titleName = Regex.Replace(titleName, pattern, "", RegexOptions.IgnoreCase);

            var normalDevelopers = new SortedSet<string>(developers.Select(dev => Regex.Replace(dev, pattern, "$3", RegexOptions.IgnoreCase)));
            var normalPublishers = new SortedSet<string>(publishers.Select(pub => Regex.Replace(pub, pattern, "$3", RegexOptions.IgnoreCase)));

            developers.Clear();
            publishers.Clear();
            foreach (string normalDev in normalDevelopers)
            {
                foreach (string part in normalDev.Split(delimiters, StringSplitOptions.RemoveEmptyEntries))
                {
                    developers.Add(part.Trim().ToLower());
                }
            }
            foreach (string normalPub in normalPublishers)
            {
                foreach (string part in normalPub.Split(delimiters, StringSplitOptions.RemoveEmptyEntries))
                {
                    publishers.Add(part.Trim().ToLower());
                }
            }
        }
        private void NormalizeSteam()
        {
            string combinedRegExp = $"{string.Join("|", removeReg)}";

            titleName = Regex.Replace(titleName, combinedRegExp, "", RegexOptions.IgnoreCase).ToLower().Trim();

            string pattern = @"\s*(,|\s)\s*(LLC|LTD|INC|S\.?A\.?|INC\.|CORPORATION|CORP|PTY\.?|L\.?P\.?|GMBH)\.?([\/,|;]\s*|$)\s*";
            var delimiters = new string[] { "/", ",", "|", ";" };
            var normalDevelopers = new SortedSet<string>(developers.Select(dev => Regex.Replace(dev, pattern, "$3", RegexOptions.IgnoreCase)));
            var normalPublishers = new SortedSet<string>(publishers.Select(pub => Regex.Replace(pub, pattern, "$3", RegexOptions.IgnoreCase)));

            developers.Clear();
            publishers.Clear();
            foreach (string normalDev in normalDevelopers)
            {
                foreach (string part in normalDev.Split(delimiters, StringSplitOptions.RemoveEmptyEntries))
                {
                    developers.Add(part.Trim().ToLower());
                }
            }
            foreach (string normalPub in normalPublishers)
            {
                foreach (string part in normalPub.Split(delimiters, StringSplitOptions.RemoveEmptyEntries))
                {
                    publishers.Add(part.Trim().ToLower());
                }
            }
        }

    };
    public class GameMarketTitle : GameTitle
    {
        public UInt32 gameID { get; private set; }
        public SortedSet<string>? xboxIds { get; set; }
        public SortedSet<string>? steamIds { get; set; }

        public Dictionary<DBSchema, SortedSet<string>> platformIds;

        public GameMarketTitle(ILogger logger, UInt32 gameID) : base(logger)
        {
            this.gameID = gameID;
            xboxIds = new SortedSet<string>();
            steamIds = new SortedSet<string>();
            platformIds = new Dictionary<DBSchema, SortedSet<string>>();
        }
        public GameMarketTitle(ILogger logger, GamePlatformTitle platformTitle) : base(logger)
        {
            gameID = 0;
            xboxIds = new SortedSet<string>();
            steamIds = new SortedSet<string>();
            platformIds = new Dictionary<DBSchema, SortedSet<string>>();
        }

        public bool hasValidIds()
        {
            return platformIds.Any(p => p.Value != null && p.Value.Count > 0);
        }
        public void JoinPlatformTitle(GamePlatformTitle platformTitle)
        {
            if (titleName == "") titleName = platformTitle.titleName;
            if (platformTitle.schema == Database_structure.GameMarket) { return; }
            developers.UnionWith(platformTitle.developers);
            publishers.UnionWith(platformTitle.publishers);

            if (!platformIds.ContainsKey(platformTitle.schema))
                platformIds.Add(platformTitle.schema, new SortedSet<string>());
            platformIds[platformTitle.schema].UnionWith(platformTitle.ids);


            switch (platformTitle.schema)
            {
                case Database_structure.XboxSchema:
                    {
                        xboxIds.UnionWith(platformTitle.ids);
                        break;
                    }
                case Database_structure.SteamSchema:
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


    #endregion



    public class GameMergerManager
    {
        private ILogger logger;
        private MainSettings settings;
        private IMapper mapper;
        private IServiceScopeFactory scopeFactory;

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


        protected bool running = false;
        protected CancellationTokenSource mainCTS = new CancellationTokenSource();
        protected Task mainTask;

        public GameMergerManager(IOptions<MainSettings> settings, ILogger<GameMergerManager> logger,
            IServiceScopeFactory scopeFactory, IMapper mapper)
        {
            this.settings = settings.Value;
            this.logger = logger;
            this.mapper = mapper;
            this.scopeFactory = scopeFactory;
        }

        public void Start()
        {
            running = true;

            logger.LogTrace($"MergerManager is starting");
            mainTask = Task.Run(RunAsync);
        }


        public void Stop()
        {
            running = false;
            mainTask?.Wait();
        }
        protected async Task RunAsync()
        {
            try
            {
                while (running && !mainCTS.Token.IsCancellationRequested)
                {
                    try
                    {
                        await mergeToGameMarket(DataBaseSchemas.Xbox);
                        await mergeToGameMarket(DataBaseSchemas.Steam);

                        logger.LogInformation("Game Merger will merge again in 1 hour");
                        await Task.Delay(TimeSpan.FromHours(1));
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.ToString());
                    }
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                throw;
            }
        }



        #region newstuff
        public async Task<SortedSet<GamePlatformTitle>> MergeXboxGamesAsync(SortedDictionary<string, GamePlatformTitle>? testTitles = null)
        {
            try
            {
                SortedSet<GamePlatformTitle> mergedTitles = new SortedSet<GamePlatformTitle>(new GamePlatformTitleComparer());
                //id / title
                SortedDictionary<string, GamePlatformTitle> xboxTitles = new SortedDictionary<string, GamePlatformTitle>();

                if (testTitles == null)
                {
                    using var scope = scopeFactory.CreateScope();
                    {
                        var dbService = scope.ServiceProvider.GetRequiredService<DataBaseService>();

                        var gameTitles = await dbService.SelectAll<XboxSchema.GameTitleTable>();

                        var context = dbService.getContext();


                        var titleQuery = context.xboxTitles?
                            .Where(gt => gt.lastScanned != null)
                            ?.Include(xt => xt.TitleDetails)
                            .ThenInclude(td => td.ProductIDNavig)
                            .ThenInclude(pn => pn.MarketDetails)
                            .ToList();


                        foreach (var title in titleQuery)
                        {
                            if (title.TitleDetails == null) continue;
                            var titleID = title.modernTitleID;
                            var newPlatformTitle = new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = title.titleName };
                            bool valid = false;
                            foreach (var titleDetails in title.TitleDetails)
                            {
                                if (titleDetails?.ProductIDNavig.MarketDetails != null)
                                {
                                    valid = true;
                                    newPlatformTitle.developers.Add(titleDetails.ProductIDNavig.MarketDetails.developerName);
                                    newPlatformTitle.publishers.Add(titleDetails.ProductIDNavig.MarketDetails.publisherName);
                                }
                            }
                            if (valid)
                                xboxTitles.Add(titleID, newPlatformTitle);
                        }
                        xboxTitles.Count();

                    }
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
        public async Task<SortedSet<GamePlatformTitle>> MergeSteamGamesAsync(SortedDictionary<string, GamePlatformTitle>? testTitles = null)
        {

            try
            {
                SortedSet<GamePlatformTitle> mergedTitles = new SortedSet<GamePlatformTitle>(new GamePlatformTitleComparer());

                //id / title
                //Dictionary<int, string> steamTitles = new Dictionary<int, string>();
                SortedDictionary<string, GamePlatformTitle> steamTitles = new SortedDictionary<string, GamePlatformTitle>();

                if (testTitles == null)
                {
                    using var scope = scopeFactory.CreateScope();
                    {
                        var dbService = scope.ServiceProvider.GetRequiredService<DataBaseService>();

                        var context = dbService.getContext();

                        var titleQuery = context.steamAppDetails
                            .Where(sad => sad.appType == "game")
                            .Include(ds => ds.Developers)
                            .Include(pb => pb.Publishers)
                            .ToList();

                        if (titleQuery != null && titleQuery.Any())
                        {
                            foreach (var title in titleQuery)
                            {
                                var titleID = title.appID;

                                var newPlatformTitle = new GamePlatformTitle(logger, Database_structure.Steam) { titleName = title.appName };
                                bool valid = false;
                                if (title.Developers != null && title.Developers.Any())
                                {
                                    title.Developers.Select(d => d.developer).ToList().ForEach(d => newPlatformTitle.developers.Add(d));
                                    valid = true;
                                }
                                if (title.Publishers != null && title.Publishers.Any())
                                {
                                    title.Publishers.Select(d => d.publisher).ToList().ForEach(p => newPlatformTitle.publishers.Add(p));
                                    valid = true;
                                }
                                if (valid)
                                    steamTitles.Add(titleID.ToString(), newPlatformTitle);
                            }
                        }
                    }
                }
                else
                {
                    steamTitles = testTitles;
                }


                string combinedRegExp = $"";
                const int scoreThreshold1 = 95;
                const int scoreThreshold2 = 90;
                const int scoreThreshold3 = 80;

                //do stuff for each title
                foreach (var title in steamTitles.Values)
                {
                    string titleName = title.titleName;
                    title.Normalize();

                    //replace stuff
                    titleName = Regex.Replace(titleName, "\\s+|[-]", " ", RegexOptions.IgnoreCase);

                    //remove stuff
                    titleName = Regex.Replace(titleName, combinedRegExp, "", RegexOptions.IgnoreCase);

                    titleName = titleName.ToLower().Trim();
                }


                //add to merged
                foreach (var titleID in steamTitles.Keys)
                {
                    var title = steamTitles[titleID];
                    title.ids.Add(titleID);
                    mergedTitles.Add(title);
                }

                return mergedTitles;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); throw;
            }
        }


        public async Task mergeToGameMarket(ISchema mergeSchema, SortedDictionary<string, GamePlatformTitle>? testTitles = null)
        {
            try
            {
                logger.LogInformation($"Merging {mergeSchema.ToString()}");
                SortedSet<GamePlatformTitle> gameMergeTitles = new SortedSet<GamePlatformTitle>();
                SortedSet<GameMarketTitle> marketTitles = new SortedSet<GameMarketTitle>(new GameMarketTitleComparer());
                logger.LogInformation($"Starting GameMerger for :  {mergeSchema.GetName()}");
                switch (mergeSchema)
                {
                    case DataBaseSchemas.XboxSchema:
                        gameMergeTitles = await MergeXboxGamesAsync(testTitles);
                        break;
                    case DataBaseSchemas.SteamSchema:
                        gameMergeTitles = await MergeSteamGamesAsync(testTitles);
                        break;

                    //Cant merge a gamemarket game
                    case DataBaseSchemas.GameMarketSchema:
                        return;
                }

                if (gameMergeTitles == null || gameMergeTitles.Count == 0)
                {
                    logger.LogDebug("Titles to merge is empty");
                    return;
                }
                logger.LogDebug($"Titles to merge: {gameMergeTitles.Count()}");
                using var scope = scopeFactory.CreateScope();
                {
                    var dbService = scope.ServiceProvider.GetRequiredService<DataBaseService>();
                    var context = dbService.getContext();
                    var gameMarketTitles = context.gameMarketTitles.AsNoTracking()
                        .Include(gmt => gmt.Developers)
                        .Include(gmt => gmt.Publishers)
                        .OrderBy(gmt => gmt.gameID).ToList();


                    foreach (var marketTitle in gameMarketTitles)
                    {
                        var newMarketTitle = new GameMarketTitle(logger, marketTitle.gameID);
                        newMarketTitle.titleName = marketTitle.gameTitle;
                        if (marketTitle?.Developers != null)
                        {
                            foreach (var dev in marketTitle.Developers)
                            {
                                newMarketTitle.developers.Add(dev.developer);
                            }
                        }
                        if (marketTitle?.Publishers != null)
                        {
                            foreach (var pub in marketTitle.Publishers)
                            {
                                newMarketTitle.publishers.Add(pub.publisher);
                            }
                        }

                        marketTitles.Add(newMarketTitle);

                    }

                    var (developerTitles, publisherTitles) = getDevPubTitles(marketTitles);

                    foreach (var title in gameMergeTitles)
                    {
                        //See if a match can be found
                        var (devMatch, pubMatch) = FindDevPubMatch(developerTitles, publisherTitles, title);

                        bool matchFound = false;
                        if (devMatch || pubMatch)
                        {
                            if (title.schema == Database_structure.Xbox)
                            {
                                if (title.titleName=="borderlands 3")
                                title.titleName = title.titleName.Replace("SEJ_", "");
                            }
                            if (title.ids.Count > 1)
                            {
                                logger.LogDebug($"Multiple IDs for: {title.titleName}");
                            }
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

                            temp.titleName = title.titleName;
                            temp.JoinPlatformTitle(title);
                            marketTitles.Add(temp);
                        }

                    }


                    marketTitles.Count();
                    ICollection<GameMarketSchema.GameTitleTable> gameTitleTables = new List<GameMarketSchema.GameTitleTable>();
                    foreach (var title in marketTitles)
                    {
                        gameTitleTables.Add(MappingProfile.MapTitleTalbe(title));


                    }
                    await dbService.AddUpdateTables(gameTitleTables);
                }


            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return;
            }
        }

        #endregion

        private (Dictionary<string, List<GameMarketTitle>>, Dictionary<string, List<GameMarketTitle>>) getDevPubTitles(SortedSet<GameMarketTitle> marketTitles)
        {
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
            return (developerTitles, publisherTitles);


        }

        private (bool, bool) FindDevPubMatch(Dictionary<string, List<GameMarketTitle>> developerTitles, Dictionary<string, List<GameMarketTitle>> publisherTitles, GamePlatformTitle title)
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
            }

            return (devMatch, pubMatch);
        }
    }
}
