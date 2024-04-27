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

using static GameMarketAPIServer.Models.DataBaseSchemas;
using ProtoBuf.Meta;

namespace GameMarketAPIServer.Utilities.Testing.Xbox
{
    [Collection("Test Collection")]
    public class TestProducts : Test
    {
        private readonly int call = (int)XblAPIManager.APICalls.marketDetails;
        private XblAPIManager xblAPIManager;
        public TestProducts(ITestOutputHelper output, TestFixture fixture) : base(output, fixture)
        {
            xblAPIManager = fixture.serviceProvider.GetRequiredService<XblAPIManager>();


            //ProductIDs
            //Control Standard
            mockAPICaller.Setup(x => x.CallAPIAsync((int)XblAPIManager.APICalls.marketDetails, "9PL1J8PJKH29"))
               .ReturnsAsync(Tools.ReadFromFile("JsonFiles/HaloWarsPCtitle.json"));
            //MW Standard/can purchase
            mockAPICaller.Setup(x => x.CallAPIAsync((int)XblAPIManager.APICalls.marketDetails, "9NVQBQ3F6W9W"))
               .ReturnsAsync(Tools.ReadFromFile("JsonFiles/MWStandard.json"));
            //mw /base game/cant purchase
            mockAPICaller.Setup(x => x.CallAPIAsync((int)XblAPIManager.APICalls.marketDetails, "C5DTJ99626K3"))
               .ReturnsAsync(Tools.ReadFromFile("JsonFiles/MWBase.json"));

            //Tiny Troupers / mobile game
            mockAPICaller.Setup(x => x.CallAPIAsync((int)XblAPIManager.APICalls.marketDetails, "9WZDNCRDLJW7"))
               .ReturnsAsync(Tools.ReadFromFile("JsonFiles/Tiny Troupers Product.json"));
        }
        [Fact]
        public async void ScanAllProducts()
        {
            await xblAPIManager.scanAllProductIds(500);
        }

        [Fact]
        public async void TestMWStandard()
        {
            await TestProduct(new List<string>() { "9NVQBQ3F6W9W" });
            //var response = mockAPICaller.Object.CallAPIAsync(call, "9NVQBQ3F6W9W").Result;
            //var (success, data) = await xblAPIManager.ParseJsonAsync(call, response);
            //XboxSchema.MarketDetailTable expected = new XboxSchema.MarketDetailTable()
            //{
            //    productID = "9NVQBQ3F6W9W",
            //    developerName = "Infinity Ward",
            //    publisherName = "Activision",
            //    releaseDate = DateTime.Parse("10/24/2019 6:00:00 PM"),
            //    purchasable = true
            //};

            //Assert.True(success);
            //Assert.NotNull(data);
            //Assert.Single(data);

            //var actual = Assert.IsType<XboxSchema.MarketDetailTable>(data.First());

            //Assert.Equal(expected.productID, actual.productID);
            //Assert.Equal(expected.developerName, actual.developerName);
            //Assert.Equal(expected.publisherName, actual.publisherName);
            //Assert.Equal(expected.releaseDate, actual.releaseDate);
            //Assert.Equal(expected.purchasable, actual.purchasable);
        }

        [Fact]
        public async void TestNull()
        {
            XboxSchema.MarketDetailTable expected = new XboxSchema.MarketDetailTable()
            {
                productID = "C5DTJ99626K3",
                developerName = "Infinity Ward",
                publisherName = "Activision",
                productTitle = "Call of Duty: Modern Warfare",
                releaseDate = DateTime.Parse("10/24/2019 5:00:00 PM"),
                startDate = DateTime.Parse("12/31/1752 4:00:00 PM"),
                endDate = DateTime.Parse("12/29/9998 4:00:00 PM"),
                currencyCode = null,
                purchasable = false,
                msrp = null,
                listPrice = null,
                posterImage = "//store-images.s-microsoft.com/image/apps.50528.67185831113154542.823e899c-d262-40a0-91f6-eee04bdc3713.dd7010c6-9641-4c5a-b8b5-7eb16ddc67ea"
            };

            using (var scope = scopeFactory.CreateScope())
            {
                var dbService = scope.ServiceProvider.GetRequiredService<DataBaseService>();

                List<XboxSchema.MarketDetailTable> listt = new List<XboxSchema.MarketDetailTable>() { expected };
                await dbService.AddUpdateTables(listt);
            }


        }
        [Fact]
        public async void TestNotNull()
        {
            XboxSchema.MarketDetailTable expected = new XboxSchema.MarketDetailTable()
            {
                productID = "C5DTJ99626K3",
                developerName = "Infinity Ward",
                publisherName = "Activision",
                productTitle = "Call of Duty: Modern Warfare",
                releaseDate = DateTime.Parse("10/24/2019 5:00:00 PM"),
                startDate = DateTime.Parse("12/31/1752 4:00:00 PM"),
                endDate = DateTime.Parse("12/29/9998 4:00:00 PM"),
                currencyCode = "USD",
                purchasable = true,
                msrp = 12,
                listPrice = 12,
                posterImage = "//store-images.s-microsoft.com/image/apps.50528.67185831113154542.823e899c-d262-40a0-91f6-eee04bdc3713.dd7010c6-9641-4c5a-b8b5-7eb16ddc67ea"
            };

            using (var scope = scopeFactory.CreateScope())
            {
                var dbService = scope.ServiceProvider.GetRequiredService<DataBaseService>();

                List<XboxSchema.MarketDetailTable> listt = new List<XboxSchema.MarketDetailTable>() { expected };
                await dbService.AddUpdateTables(listt);
            }


        }
        [Fact]
        public async void TestMWBase()
        {
            await TestProduct(new List<string>() { "C5DTJ99626K3" });
            //var response = mockAPICaller.Object.CallAPIAsync(call, "C5DTJ99626K3").Result;
            //var (success, data) = await xblAPIManager.ParseJsonAsync(call, response);
            ////output.WriteLine(response);
            //XboxSchema.MarketDetailTable expected = new XboxSchema.MarketDetailTable()
            //{
            //    productID = "C5DTJ99626K3",
            //    developerName = "Infinity Ward",
            //    publisherName = "Activision",
            //    releaseDate = DateTime.Parse("2019-10-25T00:00:00.0000000Z"),
            //    purchasable = false
            //};

            //Assert.True(success);
            //Assert.NotNull(data);
            //Assert.Single(data);

            //var actual = Assert.IsType<XboxSchema.MarketDetailTable>(data.First());

            //Assert.Equal(expected.productID, actual.productID);
            //Assert.Equal(expected.developerName, actual.developerName);
            //Assert.Equal(expected.publisherName, actual.publisherName);
            //Assert.Equal(expected.releaseDate, actual.releaseDate);
            //Assert.Equal(expected.purchasable, actual.purchasable);
        }

        [Fact]
        public async void TestTinyTroupers()
        {
            var response = mockAPICaller.Object.CallAPIAsync(call, "9WZDNCRDLJW7").Result;
            var (success, data) = await xblAPIManager.ParseJsonAsync(call, response);

            XboxSchema.MarketDetailTable expected = new XboxSchema.MarketDetailTable()
            {
                productID = "9WZDNCRDLJW7",
                developerName = "GAME TROOPERS",
                publisherName = "GAME TROOPERS",
                releaseDate = DateTime.Parse("2014-12-03T10:26:26.3870000Z"),
                purchasable = true
            };

            Assert.True(success);
            Assert.NotNull(data);
            Assert.Single(data);

            var actual = Assert.IsType<XboxSchema.MarketDetailTable>(data.First());

            Assert.Equal(expected.productID, actual.productID);
            Assert.Equal(expected.developerName, actual.developerName);
            Assert.Equal(expected.publisherName, actual.publisherName);
            Assert.Equal(expected.releaseDate, actual.releaseDate);
            Assert.Equal(expected.purchasable, actual.purchasable);
        }


        private async Task TestProduct(List<string> testIDs)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbService = scope.ServiceProvider.GetRequiredService<DataBaseService>();
                ICollection<XboxSchema.ProductIDTable>? productIDs = await dbService.SelectProductIDs(DateTime.UtcNow);

                HashSet<XboxSchema.MarketDetailTable> successList = new HashSet<XboxSchema.MarketDetailTable>();

                if (productIDs == null || !productIDs.Any()) return;

                ICollection<XboxSchema.ProductIDTable>? currentProductIDs = productIDs.Where(c => testIDs.Contains(c.productID)).ToList();
                foreach (var productID in testIDs)
                {
                    var response = mockAPICaller.Object.CallAPIAsync(call, productID).Result;
                    var (success, data) = await xblAPIManager.ParseJsonAsync(call, response);

                    if (xblAPIManager.processMarketDetails(currentProductIDs, success, data))
                    {
                        if (!data.All(d => d is XboxSchema.MarketDetailTable)) continue;
                        var marketData = data.Cast<XboxSchema.MarketDetailTable>().ToList();
                        successList.UnionWith(marketData);
                        //await dbService.AddUpdateTables(returnList);
                    }
                }
                await dbService.AddUpdateTables(successList);
            }
        }
    }
}