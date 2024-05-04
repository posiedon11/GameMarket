
using GameMarketAPIServer.Services;
using Xunit;
using Xunit.Abstractions;
using static GameMarketAPIServer.Models.DataBaseSchemas;

namespace GameMarketAPIServer.Utilities.Testing.Steam
{
    [Collection("Test Collection")]
    public class TestAppDetails : Test
    {
        private StmAPIManager stmAPIManager;
        public TestAppDetails(ITestOutputHelper output, TestFixture fixture) : base(output, fixture)
        {
            logger.LogDebug("TestAppDetails");
            this.stmAPIManager = fixture.serviceProvider.GetRequiredService<StmAPIManager>();
        }

        [Fact]
        public async void TestAllAppDetails()
        {
            await stmAPIManager.ScanAllAppDetailsAsync(210);
        }

        [Fact]
        public async void TestWitcher3WildHunt()
        {
            //wait stmAPIManager.ScanAppDetailsAsync();
            await TestAppDetail(292030);
        }
        [Fact]
        public async void TestKillingFloor()
        {
            await TestAppDetail(1250);
        }
        [Fact]
        public async void TestNomadica()
        {
            await TestAppDetail(2835380);
        }
        [Fact]
        public async void TestBorderlands3()
        {
            await TestAppDetail(397540);
        }
        [Fact]
        public async void TestCS2()
        {
            await TestAppDetail(730);
        }
        [Fact]
        public async void TestEVEOnline()
        {
            await TestAppDetail(8500);
        }

        private async Task TestAppDetail(UInt32 appID)
        {


            using var scope = scopeFactory.CreateScope();
            {
                var dbService = scope.ServiceProvider.GetRequiredService<DataBaseService>();
                var appIDS = await dbService.SelectAll<SteamSchema.AppIDsTable>();
                if (appIDS is null) return;
                var IDNav = appIDS.FirstOrDefault(a => a.appID == appID);
                var details = await stmAPIManager.ScannAppDetailsAsync(appID);
                if (details is null) return;

                var appDetails = details.Cast<SteamSchema.AppDetailsTable>().ToList();
                await dbService.AddUpdateTables(appDetails);
            }
        }
    }

}
