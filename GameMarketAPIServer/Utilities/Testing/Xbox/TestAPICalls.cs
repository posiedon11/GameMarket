﻿using GameMarketAPIServer.Services;
using System;
using Xunit;
using Xunit.Abstractions;

namespace GameMarketAPIServer.Utilities.Testing.Xbox
{
    public class APITest
    {

    }
    [Collection("Test Collection")]
    public class TestAPICalls : Test
    {

        private XblAPIManager xblAPIManager;
        private Dictionary<XblAPIManager.APICalls, List<string>> testCalls;
        public TestAPICalls(ITestOutputHelper output, TestFixture fixture) : base(output, fixture)
        {
            var xboxLogger = new XUnitLoggerProvider(output).CreateLogger<XblAPIManager>();
            this.xblAPIManager = fixture.serviceProvider.GetRequiredService<XblAPIManager>();
            //xblManager = new XblAPIManager(mockDataBaseManager.Object, settings, new XblAPITracker(settings), xboxLogger, httpClientFactory, dataBaseService);
        }

        [Fact]
        public async void TestCall1()
        {
            //var temp = await xblManager.CallAPIAsync(1);

            string historyRespone = await xblAPIManager.CallAPIAsync((int)XblAPIManager.APICalls.playerTitleHistory, "2533274880644024");
            //await xblManager.scanAllPlayerHistories();
        }

    }
}