
using GameMarketAPIServer.Configuration;
using GameMarketAPIServer.Models;
using GameMarketAPIServer.Services;
using GameMarketAPIServer.Utilities;
using Microsoft.Extensions.Options;
using MySqlConnector;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();
builder.Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<MainSettings>(builder.Configuration.GetSection("MainSettings"));
builder.Services.AddSingleton<XblAPITracker>();
builder.Services.AddSingleton<StmAPITracker>();
builder.Services.AddSingleton<IDataBaseManager, DataBaseManager>();
builder.Services.AddSingleton<XblAPIManager>();
builder.Services.AddSingleton<StmAPIManager >();
builder.Services.AddSingleton<GameMergerManager>();

var app = builder.Build();


var xblManager = app.Services.GetRequiredService<XblAPIManager>();
var mergerManager = app.Services.GetRequiredService<GameMergerManager>();


var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStarted.Register(async () =>
{
    //settings.xboxSettings.xblAPIKey = builder.Configuration["MyApiKeys:XblAPIKey"];
    //settings.steamSettings.apiKey = builder.Configuration["MyApiKeys:SteamWebAPIKey"];
    //Console.WriteLine(settings.xboxSettings.xblAPIKey);
   // Console.WriteLine(settings.steamSettings.apiKey);
    //settings.xboxSettings.outputRemainingRequests();
    
     //xblManager.Start();
    await mergerManager.MergeXboxGamesAsync(); 
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


#if false
public class Program
{
    public static async Task<int> Main()
    {

        //var builder = new ConfigurationBuilder()
        //    .SetBasePath(Directory.GetCurrentDirectory())
        //    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        //    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        //    .AddEnvironmentVariables();
        //IConfigurationRoot configuration = builder.Build();

        Settings ss = Settings.Instance;
        ss.xboxSettings.repartitionHourlyRequests();
        //ss.xboxSettings.outputRemainingRequests();


        DataBaseManager dataBaseManager = new DataBaseManager(ss);
        XblAPIManager xblAPIManager = new XblAPIManager(dataBaseManager, ss);
        StmAPIManager stmAPIManager = new StmAPIManager(dataBaseManager, ss);
        GameMergerManager mergerManager = new GameMergerManager(dataBaseManager);
        //xblAPIManager.Start();
        //stmAPIManager.Start();

        //var settin = configuration["MyApiKeys:XblAPIKey"];
        //Console.WriteLine();
        await mergerManager.mergeXboxToGameMarketGames();
        // await mergerManager.mergeSteamToGameMarketGames();

        //string connectionString = "Server=" + ss.sqlServerSettings.serverName + ";Port=" + ss.sqlServerSettings.serverPort + ";User ID=" + ss.sqlServerSettings.serverUserName + ";Password=" + ss.sqlServerSettings.serverPassword;

        //await using var connection = new MySqlConnection(connectionString);
        //await connection.OpenAsync();


        while (true)
        {

        }
        xblAPIManager.Stop();
    }
} 
#endif