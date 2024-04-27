﻿using GameMarketAPIServer.Models;
using GameMarketAPIServer.Services;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Writers;
using Moq;
using Xunit;
using Xunit.Abstractions;
using static GameMarketAPIServer.Models.DataBaseSchemas;
namespace GameMarketAPIServer.Utilities.Testing.Xbox
{
    [Collection("Test Collection")]
    public class TestPlayerHistories : Test
    {
        private ILogger<JsonData> jsonLogger;
        private readonly int apiCall = (int)XblAPIManager.APICalls.playerTitleHistory;
        private readonly XblAPIManager xblAPIManager;
        public TestPlayerHistories(ITestOutputHelper output, TestFixture fixture) : base(output, fixture)
        {
            jsonLogger = fixture.serviceProvider.GetService<ILogger<JsonData>>();

            xblAPIManager = fixture.serviceProvider.GetRequiredService<XblAPIManager>();
            if (xblAPIManager == null)
                logger.LogDebug("Error");


            mockAPICaller.Setup(x => x.CallAPIAsync((int)XblAPIManager.APICalls.playerTitleHistory, "2533274880644024"))
                .ReturnsAsync(Tools.ReadFromFile("JsonFiles/posiedon11.json"));
        }

        [Fact]
        public async void TestScannAllHistory()
        {
            var users = new List<XboxSchema.UserProfileTable>()
                {
                    new XboxSchema.UserProfileTable("2533274792073233", "jimmyhova"),
                    new XboxSchema.UserProfileTable("2533274797744336", "SiegfriedX"),
                    new XboxSchema.UserProfileTable("2533274810558996", "True Marvellous"),
                    new XboxSchema.UserProfileTable("2533274814515397", "RedmptionDenied"),
                    new XboxSchema.UserProfileTable("2533274880644024", "posiedon11"),
                    new XboxSchema.UserProfileTable("2698138705331816", "Riffai"),
                    new XboxSchema.UserProfileTable("2535419822751112", "x51pegasus50"),
                };

            await dataBaseService.InsertDefaultXboxUsers();
            await dataBaseService.AddUpdateTables<XboxSchema.UserProfileTable>(users);
            //await xblAPIManager.scanAllPlayerHistories(1);
            // await dataBaseService.AddUpdateTables<XboxSchema.UserProfileTable>(users);

            //await xblAPIManager.scanAllPlayerHistories();

        }

        [Fact]

        public async void TestPosiedon11()
        {
            string historyRespone = await mockAPICaller.Object.CallAPIAsync(apiCall, "2533274880644024");
            var (success, data) = await xblAPIManager.ParseJsonAsync(apiCall, historyRespone);

            if (data != null && data.All(d => d is XboxSchema.GameTitleTable) && data.Count > 0)
            {
                using (var scope = scopeFactory.CreateScope())
                {

                    var dbService = scope.ServiceProvider.GetRequiredService<DataBaseService>();
                    //var temp = data.Cast<XboxSchema.GameTitleTable>().ToList();
                    await dbService.AddUpdateTables(data);
                }
            }


            Assert.True(success);
            Assert.NotNull(data);
            Assert.True(data.Count > 500);

            logger.LogDebug("historyRespone");
        }
    }
}
