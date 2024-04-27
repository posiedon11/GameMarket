﻿using Xunit;
using Moq;
using GameMarketAPIServer.Services;
using Xunit.Abstractions;
using GameMarketAPIServer.Configuration;
using static SteamKit2.Internal.CMsgCellList;
using GameMarketAPIServer.Models;
using Microsoft.Extensions.Options;
using System.Reflection;
using SteamKit2;
using GameMarketAPIServer.Utilities.Testing;
using Microsoft.AspNetCore.Components.Web;
using static GameMarketAPIServer.Models.DataBaseSchemas;
using Microsoft.VisualBasic;

namespace GameMarketAPIServer.Utilities.Testing.Xbox
{

    [Collection("Test Collection")]
    public class TestXboxTitles : Test
    {
        private ILogger<JsonData> jsonLogger;
        private readonly int call = (int)XblAPIManager.APICalls.gameTitle;
        private readonly XblAPIManager xblAPIManager;
        public TestXboxTitles(ITestOutputHelper output, TestFixture fixture) : base(output, fixture)
        {
            jsonLogger = fixture.serviceProvider.GetService<ILogger<JsonData>>();
            var xboxLogger = new XUnitLoggerProvider(output).CreateLogger<XblAPIManager>();


            xblAPIManager = fixture.serviceProvider.GetRequiredService<XblAPIManager>();
            if (xblAPIManager == null)
                logger.LogDebug("Error");


            //xblAPIManager = new XblAPIManager(mockDataBaseManager.Object, settings, new XblAPITracker(settings), xboxLogger, httpClientFactory, dataBaseService);
            //cant really do player histories

            //Game Titles:
            //tiny troupers
            mockAPICaller.Setup(x => x.CallAPIAsync((int)XblAPIManager.APICalls.gameTitle, "2099832516"))
                .ReturnsAsync(Tools.ReadFromFile("JsonFiles/Tiny Troupers Title.json"));
            //hell let loose
            mockAPICaller.Setup(x => x.CallAPIAsync((int)XblAPIManager.APICalls.gameTitle, "2111829961"))
                .ReturnsAsync(Tools.ReadFromFile("JsonFiles/Hell Let Loose.json"));

            //modern warfare
            mockAPICaller.Setup(x => x.CallAPIAsync((int)XblAPIManager.APICalls.gameTitle, "609700427"))
                .ReturnsAsync(Tools.ReadFromFile("JsonFiles/ModernWarfareTitle.json"));
            //IDARB
            mockAPICaller.Setup(x => x.CallAPIAsync((int)XblAPIManager.APICalls.gameTitle, "1019936697"))
                .ReturnsAsync(Tools.ReadFromFile("JsonFiles/Idarb.json"));
            //Halo Wars PC
            mockAPICaller.Setup(x => x.CallAPIAsync((int)XblAPIManager.APICalls.gameTitle, "1030018025"))
                .ReturnsAsync(Tools.ReadFromFile("JsonFiles/HaloWarsPCtitle.json"));
            //Flight Sim
            mockAPICaller.Setup(x => x.CallAPIAsync((int)XblAPIManager.APICalls.gameTitle, "1777860928"))
               .ReturnsAsync(Tools.ReadFromFile("JsonFiles/Microsoft Flight Sim Title.json"));
            //ByeByeBrain
            mockAPICaller.Setup(x => x.CallAPIAsync((int)XblAPIManager.APICalls.gameTitle, "1997868610"))
               .ReturnsAsync(Tools.ReadFromFile("JsonFiles/ByeByeBraintitle.json"));

            mockAPICaller.Setup(x => x.CallAPIAsync((int)XblAPIManager.APICalls.gameTitle, "756981192"))
               .ReturnsAsync(Tools.ReadFromFile("JsonFiles/borderlands3.json"));

        }

        [Fact]
        public async void TestTitleHellLetLoose()
        {
            await TestTitle("2111829961");

        }

        [Fact]
        public async void TestScanAllTitles()
        {

            await xblAPIManager.scanAllGameTitles(300);
            //var response = mockAPICaller.Object.CallAPIAsync(call, "0").Result;
            //var (success, data) = await xblAPIManager.ParseJsonAsync(call, response);
            //output.WriteLine(response);

            //Assert.True(success);
            //Assert.NotNull(data);
            //Assert.True(data.Count > 1000);
        }
        [Fact]
        public async void TestTitleTinyTroupers()
        {

            var response = mockAPICaller.Object.CallAPIAsync(call, "2099832516").Result;
            var (success, data) = await xblAPIManager.ParseJsonAsync(call, response);
            //output.WriteLine(response);

            Assert.True(success);
            Assert.NotNull(data);
            Assert.Equal(1, data.Count);
        }
        [Fact]
        public async void TestTitleHaloWarsAndModernWarfare()
        {

            await TestTitles(new List<string> { "1030018025", "609700427" });
        }

        [Fact]
        public async void TestTitleHaloWarsPC()
        {
            await TestTitle("1030018025");
        }

        [Fact]
        public async void TestTitleModernWarfare()
        {
            await TestTitle("609700427");

        }
        [Fact]
        public async void TestBorderlands3()
        {
            await TestTitle("756981192");
        }

        private async Task TestTitle(string titleID)
        {
            var response = mockAPICaller.Object.CallAPIAsync(call, titleID).Result;
            var (success, tableData) = await xblAPIManager.ParseJsonAsync(call, response);

            Assert.True(success);
            Assert.NotNull(tableData);
            Assert.NotEmpty(tableData);

            using var scope = scopeFactory.CreateScope();
            {
                var dbService = scope.ServiceProvider.GetRequiredService<DataBaseService>();

                var gameTitles = await dbService.SelectAll<XboxSchema.GameTitleTable>();
                Assert.NotNull(gameTitles);

                var gameTitle = gameTitles.FirstOrDefault(e => e.titleID == titleID);
                Assert.NotNull(gameTitle);

                ICollection<DataBaseSchemas.XboxSchema.TitleDetailTable> successfulList = new List<XboxSchema.TitleDetailTable>();
                gameTitle.lastScanned = DateTime.UtcNow;

                var selectIDs = await dbService.SelectAll<XboxSchema.ProductIDTable>();
                Dictionary<string, XboxSchema.ProductIDTable> productIDs = new Dictionary<string, XboxSchema.ProductIDTable>();
                if (selectIDs != null && selectIDs.Any())
                    productIDs = selectIDs.ToDictionary(e => e.productID);


                if (xblAPIManager.processGameTitleReturn(gameTitle, success, tableData))
                {
                    foreach (var title in tableData)
                    {
                        if (title is XboxSchema.TitleDetailTable titleTable)
                        {
                            successfulList.Add(titleTable);
                        }
                    }
                }

                await dbService.AddUpdateTables(successfulList);
            }
        }

        private async Task TestTitles(ICollection<string> titleIDs)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                {
                    var dbService = scope.ServiceProvider.GetRequiredService<DataBaseService>();
                    var gameTitles = await dbService.SelectAll<XboxSchema.GameTitleTable>();
                    Assert.NotNull(gameTitles);

                    ICollection<DataBaseSchemas.XboxSchema.TitleDetailTable> successfulList = new List<XboxSchema.TitleDetailTable>();
                    foreach (var titleID in titleIDs)
                    {
                        var response = mockAPICaller.Object.CallAPIAsync(call, titleID).Result;
                        var (success, tableData) = await xblAPIManager.ParseJsonAsync(call, response);

                        var gameTitle = gameTitles.FirstOrDefault(e => e.titleID == titleID);
                        Assert.NotNull(gameTitle);

                        gameTitle.lastScanned = DateTime.UtcNow;

                        var selectIDs = await dbService.SelectAll<XboxSchema.ProductIDTable>();
                        Dictionary<string, XboxSchema.ProductIDTable> productIDs = new Dictionary<string, XboxSchema.ProductIDTable>();
                        if (selectIDs != null && selectIDs.Any())
                            productIDs = selectIDs.ToDictionary(e => e.productID);


                        if (xblAPIManager.processGameTitleReturn(gameTitle, success, tableData))
                        {
                            foreach (var title in tableData)
                            {
                                if (title is XboxSchema.TitleDetailTable titleTable)
                                {
                                    successfulList.Add(titleTable);
                                }
                            }
                        }
                    }

                    await dbService.AddUpdateTables(successfulList);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
            }
        }
    }



}

