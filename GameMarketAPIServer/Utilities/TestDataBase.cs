using GameMarketAPIServer.Models;
using Xunit;
using Xunit.Abstractions;

namespace GameMarketAPIServer.Utilities
{
    [Collection("Test Collection")]
    public class TestDataBase : Test
    {
        public TestDataBase(ITestOutputHelper output, TestFixture fixture) : base(output, fixture)
        {
        }

        [Fact]
        public void TestDBStructure()
        {
            try
            {

                var bbox = Database_structure.Xbox;
                var ffs=bbox.MarketDetails.endDate.fullPath();
                //logger.LogDebug(Database_structure.Xbox.GameBundles.var1.fullPath());
                //var ff = Database_structure.XboxSchema.UserProfiles.Name;
                //var fdfd = Database_structure.XboxSchema;
              // var dfasfas= Database_structure.XboxSchema.GameBundlesTable.var1;
               // var fdasf = bbox.GameBundles;

            }catch (Exception ex) { logger.LogError(ex.Message); }
            
        }
    }
}
