using GameMarketAPIServer.Configuration;
using GameMarketAPIServer.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit.Abstractions;
using Xunit;
using Moq;
using GameMarketAPIServer.Services;
using Xunit.Abstractions;
using System.Reflection;
using System.Collections.Immutable;
using FuzzySharp;
using GameMarketAPIServer.Models.Enums;
using GameMarketAPIServer.Models;
using GameMarketAPIServer.Utilities.Testing;

namespace GameMarketAPIServer.Utilities.Testing.GameMarket
{

    [Collection("Test Collection")]
    public class TestGameMergerManager : Test
    {

        private GameMergerManager mergerManager;
        protected SortedDictionary<(string, GamePlatformTitle), (string, GamePlatformTitle)> xboxNormalizeTest;
        public TestGameMergerManager(ITestOutputHelper output, TestFixture fixture) : base(output, fixture)
        {
            try
            {
                // var mergerLogger = new XUnitLoggerProvider(output).CreateLogger<GameMergerManager>();
                // var dbLogger = new XUnitLoggerProvider(output).CreateLogger<DataBaseManager>();
                mergerManager = fixture.serviceProvider.GetRequiredService<GameMergerManager>();
                xboxNormalizeTest = new SortedDictionary<(string, GamePlatformTitle), (string, GamePlatformTitle)>{
                {
                   ("1653884944", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Alchemist Simulator", developers = { "Art Games Studio S.A." }, publishers = { "Art Games Studio S.A./Polyslash S.A." } }),
                   ("1653884944", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Alchemist Simulator", developers = { "Art Games Studio" }, publishers = { "Art Games Studio", "Polyslash" } })
                },
                {
                   ("1642353875", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Craftopia", developers = { "POCKET PAIR, Inc." }, publishers = { "POCKET PAIR, Inc." } }),
                   ("1642353875", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Craftopia", developers = { "POCKET PAIR" }, publishers = { "POCKET PAIR" } })
                },

                {
                    ("164371165", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "LEGO Star Wars: TCS", developers = { "Traveller's Tales (UK), Ltd." }, publishers = { "Disney Interactive Studios" } }),
                    ("164371165", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "LEGO Star Wars: TCS", developers = { "Traveller's Tales (UK)" }, publishers = { "Disney Interactive Studios" } })
                },
                {
                    ("1639060910", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Minecraft Legends", developers = { "Mojang Studios / Blackbird Interactive" }, publishers = { "Xbox Game Studios" } }),
                    ("1639060910", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Minecraft Legends", developers = { "Mojang Studios", "Blackbird Interactive" }, publishers = { "Xbox Game Studios" } })
                },
                {
                    ("1628688048", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Atomic Heart", developers = { "Mundfish" }, publishers = { "Focus Entertainment" } }),
                    ("1628688048", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Atomic Heart", developers = { "Mundfish" }, publishers = { "Focus Entertainment" } })
                },
                {
                    ("1626525581", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "The Bridge Curse: Road to Salvation", developers = { "Eastasiasoft Limited, Softstar Entertainment Inc." }, publishers = { "Eastasiasoft Limited" } }),
                    ("1626525581", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "The Bridge Curse: Road to Salvation", developers = { "Eastasiasoft Limited", "Softstar Entertainment" }, publishers = { "Eastasiasoft Limited" } })
                },
                {
                    ("1616046491", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Gears Tactics - Base Game", developers = { "Splash Damage | The Coalition" }, publishers = { "Xbox Game Studios" } }),
                    ("1616046491", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Gears Tactics - Base Game", developers = { "Splash Damage", "The Coalition" }, publishers = { "Xbox Game Studios" } } )
                },
                {
                    ("1144039928", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Halo: The Master Chief Collection", developers = { "343 Industries" }, publishers = { "Xbox Game Studios" } }),
                    ("1144039928", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Halo: The Master Chief Collection", developers = { "343 Industries" }, publishers = { "Xbox Game Studios" } } )
                },
                {
                    ("1677025209", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Wolfenstein: The New Order (PC)", developers = { "MachineGames" }, publishers = { "Bethesda Softworks" } }),
                    ("1677025209", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Wolfenstein: The New Order", developers = { "MachineGames" }, publishers = { "Bethesda Softworks" } } )
                },
                {
                    ("1981403899", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Wolfenstein: The New Order", developers = { "MachineGames" }, publishers = { "Bethesda Softworks" } } ),
                    ("1981403899", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Wolfenstein: The New Order", developers = { "MachineGames" }, publishers = { "Bethesda Softworks" } } )
                },
                    {
                    ("2008811924", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Wolfenstein II: The New Colossus", developers = { "MachineGames" }, publishers = { "Bethesda Softworks" } } ),
                    ("2008811924", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Wolfenstein® II: The New Colossus", developers = { "MachineGames" }, publishers = { "Bethesda Softworks" } } )
                },
                    {
                    ("2132785815", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Wolfenstein® II: The New Colossus™", developers = { "MachineGames" }, publishers = { "Bethesda Softworks" } } ),
                    ("2132785815", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Wolfenstein II: The New Colossus", developers = { "MachineGames" }, publishers = { "Bethesda Softworks" } } )
                },
                    {
                    ("756981192", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Borderlands 3", developers = { "Gearbox Software" }, publishers = { "2K" } } ),
                    ("756981192", new GamePlatformTitle(logger, Database_structure.Xbox) { titleName = "Borderlands 3", developers = { "Gearbox Software" }, publishers = { "2K" } } )
                    }

            };
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }


        [Fact]
        public void TestFuzzy()
        {

            List<(string, string)> testList = new List<(string, string)>()
            {
                ( "ballotron", "ballotron coolbox"),

                ("call of duty 1", "call of duty 1"),
                ("halo 1", "call of duty 1"),
                ("zombie derby 2", "zombie derby"),
                ("the jackbox party pack 8", "the jackbox party pack 3")
            };
            foreach (var test in testList)
            {
                logger.LogDebug($"Different Title: {GameTitle.isSequel(test.Item1, test.Item2)}");
                logger.LogDebug($"Testing {test.Item1} with {test.Item2}");
                logger.LogDebug($"\tToken Sort Score: {Fuzz.TokenSortRatio(test.Item1, test.Item2)}");
                logger.LogDebug($"\tPartial Ratio Sort Score: {Fuzz.PartialRatio(test.Item1, test.Item2)}");
                logger.LogDebug($"\tRatio Sort Score: {Fuzz.Ratio(test.Item1, test.Item2)}");
                logger.LogDebug($"\tWeighted Sort Score: {Fuzz.WeightedRatio(test.Item1, test.Item2)}");

                logger.LogDebug("\n");
            }
        }


        [Fact]
        public async void TestGameMarketMerger()
        {
            List<string> testing = new List<string>() { "1677025209", "1981403899", "2008811924", "2132785815" };
            var temp = new SortedDictionary<string, GamePlatformTitle>();

            foreach (var item in xboxNormalizeTest.Keys)
            {
                //if (item.Item1 == "1981403899" || item.Item1 == "1677025209")
                //if(testing.Contains(item.Item1))
                temp.Add(item.Item1, item.Item2);
            }
            //var me = await mergerManager.MergeXboxGamesAsync(temp);
            //await mergerManager.mergeToGameMarket(DataBaseManager.Schemas.xbox, temp);
            await mergerManager.mergeToGameMarket(DataBaseSchemas.Xbox, temp);
        }
        [Fact]
        public async void TestNewGameMarketMerger()
        {
            List<string> testing = new List<string>() { "1677025209", "1981403899", "2008811924", "2132785815" };
            var temp = new SortedDictionary<string, GamePlatformTitle>();

            foreach (var item in xboxNormalizeTest.Keys)
            {
                //if (item.Item1 == "1981403899" || item.Item1 == "1677025209")
                //if(testing.Contains(item.Item1))
                temp.Add(item.Item1, item.Item2);
            }
            await mergerManager.mergeToGameMarket(DataBaseSchemas.Xbox, temp);
        }
        [Fact]
        public async void TestXboxMerger()
        {
            List<string> testing = new List<string>() { "1677025209", "1981403899", "2008811924", "2132785815" };
            var temp = new SortedDictionary<string, GamePlatformTitle>();

            foreach (var item in xboxNormalizeTest.Keys)
            {
                //if (item.Item1 == "1981403899" || item.Item1 == "1677025209")
                //if(testing.Contains(item.Item1))
                temp.Add(item.Item1, item.Item2);
            }

            var me = await mergerManager.MergeXboxGamesAsync(null);
            foreach (var item in me)
            {
                //item.output();
            }
        }

        [Fact]
        public async void TestNewXboxMerge()
        {
            var temp = new SortedDictionary<string, GamePlatformTitle>();

            foreach (var item in xboxNormalizeTest.Keys)
            {
                //if (item.Item1 == "1981403899" || item.Item1 == "1677025209")
                //if(testing.Contains(item.Item1))
                temp.Add(item.Item1, item.Item2);
            }

            var me = await mergerManager.MergeXboxGamesAsync(null);

            //await mergerManager.MergeXboxGamesAsync(null);
        }

        public async void TestNewGameMarketMerge()
        {
            var temp = new SortedDictionary<string, GamePlatformTitle>();

            foreach (var item in xboxNormalizeTest.Keys)
            {
                //if (item.Item1 == "1981403899" || item.Item1 == "1677025209")
                //if(testing.Contains(item.Item1))
                temp.Add(item.Item1, item.Item2);
            }
            //var me = await mergerManager.MergeXboxGamesAsync(temp);
            //await mergerManager.mergeToGameMarket(DataBaseManager.Schemas.xbox, temp);
            await mergerManager.mergeToGameMarket(DataBaseSchemas.Xbox, temp);
        }


        [Fact]
        public void TestTitleNormalize()
        {

            foreach (var title in xboxNormalizeTest)
            {
                title.Key.Item2.Normalize();
                title.Key.Item2.output();
                Assert.Equivalent(title.Value, title.Key);
            }

        }
    }
}
