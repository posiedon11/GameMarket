using GameMarketAPIServer.Models;
using GameMarketAPIServer.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
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
            //await dataBaseService.AddUpdateTable(userProfile);
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

        [Fact]
        public async void TestQuery()
        {
            try
            {
                var context = dataBaseService.getContext();
                var qqq = context.gameMarketTitles
                .Include(gt => gt.Developers)
                .Include(gt => gt.Publishers)
                .Include(gt => gt.XboxLinks)
                    .ThenInclude(xl => xl.XboxTitle)
                        .ThenInclude(xt => xt.TitleDetails)
                            .ThenInclude(pd => pd.ProductIDNavig)
                                .ThenInclude(pid => pid.MarketDetails)
                .Include(gt => gt.SteamLinks)
                    .ThenInclude(sl => sl.AppDetails)
                .Select(gt => new
                {
                    GameTitle = gt.gameTitle,
                    Developers = gt.Developers.Select(d => d.developer).ToList(),
                    Publishers = gt.Publishers.Select(p => p.publisher).ToList(),
                    XboxMarketDetails = gt.XboxLinks.SelectMany(xl => xl.XboxTitle.TitleDetails.Select(xx=>xx.ProductIDNavig)).Select(gg=>gg.MarketDetails).ToList()

                })
                .ToList();
                var fdfda = qqq.Where(gg => gg.XboxMarketDetails.Any()).ToList();
                var fdfdaa = qqq.Where(gg => gg.XboxMarketDetails.Count >1).ToList();
                logger.LogDebug(qqq.ToString());

            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
            }
        }

        [Fact]
        public async void TestQuery1()
        {
            try
            {
                var context = dataBaseService.getContext();
                var temp = context.gameMarketTitles.Include(gt => gt.Developers)
                .Include(gt => gt.Publishers)
                .Include(gt => gt.XboxLinks)
                .ThenInclude(g => g.XboxTitle.TitleDevices)
                .Include(gt => gt.SteamLinks)
                .ThenInclude(s => s.AppDetails)
                .ToList();
                var hiii = temp.Where(gt=>gt.XboxLinks.Count>1).ToList();
                logger.LogDebug(temp.ToString());

            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
            }
        }
    }
}
