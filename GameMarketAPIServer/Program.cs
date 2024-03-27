//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();


using GameMarketAPIServer.Configuration;
using GameMarketAPIServer.Services;
using MySqlConnector;

public class Program
{
    public static async Task<int> Main()
    {
        Settings ss = Settings.Instance;
        ss.xboxSettings.repartitionHourlyRequests();
        //ss.xboxSettings.outputRemainingRequests();


        DataBaseManager dataBaseManager = new DataBaseManager(ss);
        XblAPIManager xblAPIManager = new XblAPIManager(dataBaseManager, ss);
        StmAPIManager stmAPIManager = new StmAPIManager(dataBaseManager, ss);
        GameMergerManager mergerManager = new GameMergerManager(dataBaseManager);
        xblAPIManager.Start();
        //stmAPIManager.Start();

       // await mergerManager.mergeXboxToGameMarketGames();
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