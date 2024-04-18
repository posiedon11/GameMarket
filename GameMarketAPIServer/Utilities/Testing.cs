using GameMarketAPIServer.Configuration;
using GameMarketAPIServer.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
using GameMarketAPIServer.Models;
using Xunit;
using System.Reflection;
namespace GameMarketAPIServer.Utilities
{
    public static class TestHelper
    {
        public static IOptions<MainSettings> CreateMockSettings()
        {
            var settings = new MainSettings
            {

            };
            return Options.Create(settings);
        }
    }

    public static class ConfigurationHelper
    {
        public static IConfigurationRoot GetConfiguration(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json",optional:true)
                .AddEnvironmentVariables()
                .Build();
        }
    }

    public class TestFixture : IDisposable
    {
        public ServiceProvider serviceProvider { get; private set; }

        public TestFixture()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddSingleton<ILogger<JsonData>>(provider =>
                provider.GetRequiredService<ILoggerFactory>().CreateLogger<JsonData>());
            services.AddSingleton<XblAPITracker>();
            serviceProvider = services.BuildServiceProvider();
        }
        public void Dispose()
        {
            if (serviceProvider is IDisposable)
            {
                ((IDisposable)serviceProvider).Dispose();
            }
        }
    }

    [CollectionDefinition("Test Collection")]
    public class TestCollection : ICollectionFixture<TestFixture> { }
    public abstract class Test
    {
        protected readonly ServiceProvider serviceProvider;
        protected readonly ITestOutputHelper output;
        protected readonly Mock<IAPIManager> mockAPICaller;
        protected readonly Mock<IDataBaseManager> mockDataBaseManager;
        protected readonly IOptions<MainSettings> settings;
        protected readonly Microsoft.Extensions.Logging.ILogger<Test> logger;
        protected Test(ITestOutputHelper output, TestFixture fixture, string outputPath = "")
        {
            this.logger = new XUnitLoggerProvider(output).CreateLogger<Test>();
            if(outputPath=="")
                outputPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            serviceProvider = fixture.serviceProvider;
            settings = Options.Create(ConfigurationHelper.GetConfiguration(outputPath).GetSection("MainSettings").Get<MainSettings>());
            mockAPICaller = new Mock<IAPIManager>();
            mockDataBaseManager = new Mock<IDataBaseManager>();
            settings.Value.sqlServerSettings.serverUserName = "root";
            settings.Value.sqlServerSettings.serverPassword = "GamePasswordMarket11";
        }
    }
}
