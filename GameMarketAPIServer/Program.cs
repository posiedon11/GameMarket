
using GameMarketAPIServer.Configuration;
using GameMarketAPIServer.Models;
using GameMarketAPIServer.Models.Contexts;
using GameMarketAPIServer.Services;
using GameMarketAPIServer.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Pomelo.EntityFrameworkCore.MySql;
using Newtonsoft;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
//builder.Logging.AddEventSourceLogger();
//builder.Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));





// Add services to the container.
builder.Services.Configure<MainSettings>(builder.Configuration.GetSection("MainSettings"));
var mainSettings = builder.Configuration.GetRequiredSection("MainSettings").Get<MainSettings>();
var connectionString = mainSettings.sqlServerSettings.getConnectionString();

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddRazorPages();
builder.Services.AddDbContext<DatabaseContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    //options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "GameMarketAPIServer", Version = "v1" });
    options.CustomSchemaIds(type => type.FullName.Replace("+", "."));
});
builder.Services.AddHttpClient();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddScoped<DataBaseService>();

builder.Services.AddSingleton<XblAPITracker>();
builder.Services.AddSingleton<StmAPITracker>();

builder.Services.AddSingleton<XblAPIManager>();
builder.Services.AddSingleton<StmAPIManager>();
builder.Services.AddSingleton<GameMergerManager>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
               builder =>
               {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});


var app = builder.Build();

var xblManager = app.Services.GetRequiredService<XblAPIManager>();
var stmManager = app.Services.GetRequiredService<StmAPIManager>();
var mergerManager = app.Services.GetRequiredService<GameMergerManager>();


var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStarted.Register( async () =>
{
    if (mainSettings.ManagerSettings.runXbox)
    {
        xblManager.Start();
    }
    if (mainSettings.ManagerSettings.runSteam)
    {
        stmManager.Start();
    }
    if (mainSettings.ManagerSettings.runGameMarket)
    {
        mergerManager.Start();
    }
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
if (app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    context.Database.Migrate();

    if (!context.xboxUsers.Any())
    {
        var dbService = scope.ServiceProvider.GetRequiredService<DataBaseService>();
        await dbService.InsertDefaultXboxUsers();
    }
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthorization();


app.MapControllers();
app.MapRazorPages();

app.Run();
