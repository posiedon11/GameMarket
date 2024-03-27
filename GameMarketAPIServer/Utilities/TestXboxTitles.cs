using Xunit;
using Moq;
using GameMarketAPIServer.Services;
using Xunit.Abstractions;
using GameMarketAPIServer.Configuration;
using static SteamKit2.Internal.CMsgCellList;
using GameMarketAPIServer.Models;


namespace GameMarketAPIServer.Utilities
{
    public abstract class Test
    {
        protected readonly ITestOutputHelper output;
        protected readonly Mock<IAPIManager> mockAPICaller;
        protected readonly Mock<IDataBaseManager> mockDataBaseManager;

        protected Test(ITestOutputHelper output)
        {
            this.output = output;
            mockAPICaller = new Mock<IAPIManager>();
            mockDataBaseManager = new Mock<IDataBaseManager>();
        }
    }
    public class TestXboxTitles : Test
    {
        
        private XblAPIManager xblManager;
        private readonly int call = (int)XblAPIManager.APICalls.gameTitle;
        public TestXboxTitles(ITestOutputHelper output) : base(output)         {
            xblManager = new XblAPIManager(mockDataBaseManager.Object, Settings.Instance);
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
        }

        [Fact]
        public async void TestTitleHellLetLoose()
        {

            var response = mockAPICaller.Object.CallAPIAsync(call, "2111829961").Result;
            var (success, data) = await xblManager.ParseJsonAsync(call, response);
            //output.WriteLine(response);

            Assert.True(success);
            Assert.NotNull(data);
            Assert.Equal(1, data.Count);
            if (success)
            {
                using (var writer = new StringWriter())
                {
                    Console.SetOut(writer); ;
                    foreach (var item in data)
                        item.outputData();

                    var temp = writer.ToString();
                    output.WriteLine(temp);
                }

            }
        }
        [Fact]
        public async void TestTitleTinyTroupers()
        {
            
            var response = mockAPICaller.Object.CallAPIAsync(call, "2099832516").Result;
            var (success, data) = await xblManager.ParseJsonAsync(call, response);
            //output.WriteLine(response);

            Assert.True(success);
            Assert.NotNull(data);
            Assert.Equal(1, data.Count);
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer); ;
                foreach (var item in data)
                    item.outputData();

                var temp = writer.ToString();
                output.WriteLine(temp);
            }
        }

        [Fact]
        public async void TestTitleHaloWarsPC()
        {
            var response = mockAPICaller.Object.CallAPIAsync(call, "1030018025").Result;
            var (success, data) = await xblManager.ParseJsonAsync(call, response);
            //output.WriteLine(response);

            Assert.True(success);
            Assert.NotNull(data);
            Assert.Equal(2, data.Count);
            if (success)
            {
                using (var writer = new StringWriter())
                {
                    Console.SetOut(writer); ;
                    foreach (var item in data)
                        item.outputData();

                    var temp = writer.ToString();
                    output.WriteLine(temp);
                }

            }
        }

    }

    public class TestXboxProducts : Test
    {
        private readonly int call = (int)XblAPIManager.APICalls.marketDetails;
        private XblAPIManager xblManager;
        public TestXboxProducts(ITestOutputHelper output) : base(output) 
        {
            xblManager = new XblAPIManager(mockDataBaseManager.Object, Settings.Instance);
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
        public async void TestMWStandard()
        {
            var response = mockAPICaller.Object.CallAPIAsync(call, "9NVQBQ3F6W9W").Result;
            var (success, data) = await xblManager.ParseJsonAsync(call, response);
            XboxGameMarketData expected = new XboxGameMarketData() {
                productID = "9NVQBQ3F6W9W",
                devName = "Infinity Ward",
                pubName = "Activision",
                releaseDate = DateTime.Parse("10/24/2019 6:00:00 PM"),
                purchasable = true
            };

            Assert.True(success);
            var actual = Assert.IsType<XboxGameMarketData>(expected);

            Assert.NotNull(data);
            Assert.NotEqual(0, data.Count);
            Assert.Equal(expected.productID, actual.productID);
            Assert.Equal(expected.devName, actual.devName);
            Assert.Equal(expected.pubName, actual.pubName);
            Assert.Equal(expected.releaseDate, actual.releaseDate);
            Assert.Equal(expected.purchasable, actual.purchasable);

            using (var writer = new StringWriter())
                {
                    Console.SetOut(writer); ;

                //expected.outputData();
                //output.WriteLine("\n\n");
                    foreach (var item in data)
                        item.outputData();

                    var temp = writer.ToString();
                    output.WriteLine(temp);
                }
        }

        [Fact]
        public async void TestMWBase()
        {
            var response = mockAPICaller.Object.CallAPIAsync(call, "C5DTJ99626K3").Result;
            var (success, data) = await xblManager.ParseJsonAsync(call, response);
            //output.WriteLine(response);
            XboxGameMarketData expected = new XboxGameMarketData()
            {
                productID = "C5DTJ99626K3",
                devName = "Infinity Ward",
                pubName = "Activision",
                releaseDate = DateTime.Parse("10/24/2019 6:00:00 PM"),
                purchasable = false
            };

            Assert.True(success);
            var actual = Assert.IsType<XboxGameMarketData>(expected);

            Assert.NotNull(data);
            Assert.NotEqual(0, data.Count);
            Assert.Equal(expected.productID, actual.productID);
            Assert.Equal(expected.devName, actual.devName);
            Assert.Equal(expected.pubName, actual.pubName);
            Assert.Equal(expected.releaseDate, actual.releaseDate);

            Assert.Equal(expected.purchasable, actual.purchasable);
           // Assert.NotEqual(DateTime.MinValue, actual.startDate);
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer); ;
                foreach (var item in data)
                    item.outputData();

                var temp = writer.ToString();
                output.WriteLine(temp);
            }
        }

        [Fact]
        public async void TestTinyTroupers()
        {
            var response = mockAPICaller.Object.CallAPIAsync(call, "9WZDNCRDLJW7").Result;
            var (success, data) = await xblManager.ParseJsonAsync(call, response);
            //output.WriteLine(response);

            Assert.True(success);
            Assert.NotNull(data);
            Assert.NotEqual(0, data.Count);
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer); ;
                foreach (var item in data)
                    item.outputData();

                var temp = writer.ToString();
                output.WriteLine(temp);
            }
        }
    }


}
