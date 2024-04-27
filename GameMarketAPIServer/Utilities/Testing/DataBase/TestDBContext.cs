﻿using GameMarketAPIServer.Models;
using GameMarketAPIServer.Services;
using Xunit;
using Xunit.Abstractions;
using static GameMarketAPIServer.Models.DataBaseSchemas;

namespace GameMarketAPIServer.Utilities.Testing.Database
{

    [Collection("Test Collection")]
    public class TestDBContext : Test
    {

        public TestDBContext(ITestOutputHelper output, TestFixture fixture) : base(output, fixture)
        {
            logger.LogDebug("TestDBContext");
        }
        [Fact]
        public void StartDocker()
        {
            DockerService dockerService = new DockerService(logger);
            settings.Value.sqlServerSettings.serverPort = 3307;
            dockerService.StartContainer("test", settings.Value.sqlServerSettings.serverPassword, settings.Value.sqlServerSettings.serverPort);
        }
        [Fact]
        public async void TestThing()
        {
            logger.LogDebug("TestThing");
            //StartDocker();
            var userProfile = new DataBaseSchemas.XboxSchema.UserProfileTable() { xuid = "123434", gamertag = "Tester3" };
            await dataBaseService.AddUpdateTable(userProfile);
            //await dataBaseService.CreateUpdate(tem2);

        }
        [Fact]
        public async void TestBulkMerge()
        {
            try
            {

                var context = dataBaseService.getContext();
                var productIDs = new List<DataBaseSchemas.XboxSchema.ProductIDTable>()
                {
                    new DataBaseSchemas.XboxSchema.ProductIDTable("9NBLGGH40DCM"),
                    new DataBaseSchemas.XboxSchema.ProductIDTable("9NBLGGH4WBZ5")
                };
                context.BulkMerge(productIDs);
                await context.BulkSaveChangesAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
            }
        }
    }
}
