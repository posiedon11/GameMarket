using GameMarketAPIServer.Services;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using static GameMarketAPIServer.Services.StmAPIManager;

namespace GameMarketAPIServer.Utilities.Testing.Steam
{
    [Collection("Test Collection")]
    public class TestAPICalls : Test
    {
        private StmAPIManager stmAPIManager;

        public TestAPICalls(ITestOutputHelper output, TestFixture fixture) : base(output, fixture)
        {
            this.stmAPIManager = fixture.serviceProvider.GetRequiredService<StmAPIManager>();

            mockAPICaller.Setup(x => x.CallAPIAsync((int)APICalls.getAppListv2, ""))
                .ReturnsAsync(Tools.ReadFromFile("JsonFiles/SteamAppList.json"));
        }

        [Fact]
        public async void TestGetAppListV1()
        {
            //var paramaters = new Dictionary<string, object>
            //            {
            //                {"include_games", true },
            //                {"include_dlc", true },
            //               // {"include_software", false },
            //                //{"include_videos", false },
            //               // {"include_hardware", false },
            //                {"max_results",  50000}
            //            };
            //logger.LogDebug("\n\nNested Search for app list. \n\n");
            //string response = await stmAPIManager. CallAPIAsync((int)APICalls.getAppListv1, JsonConvert.SerializeObject(paramaters));
            //var (success, newAppList) = await stmAPIManager.ParseJsonAsync((int)APICalls.getAppListv1, response);
            await stmAPIManager.GetAppList(false);
        }

        [Fact]
        public async void TestGetAppListV2()
        {

            //var response = await stmAPIManager.CallAPIAsync((int)APICalls.getAppListv2, "");
            //var (success, data) = await stmAPIManager.ParseJsonAsync((int)APICalls.getAppListv2, response);

            await stmAPIManager.GetAppList();

            // Assert
        }
    }
}
