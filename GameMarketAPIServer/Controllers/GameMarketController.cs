using GameMarketAPIServer.Models;
using GameMarketAPIServer.Models.Contexts;
using GameMarketAPIServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using SteamKit2.Internal;
using static SteamKit2.Internal.CMsgBluetoothDevicesData;
using static GameMarketAPIServer.Models.DataBaseSchemas;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GameMarketAPIServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameMarketController : ControllerBase
    {


        private readonly ILogger<GameMarketController> logger;
        private readonly DatabaseContext context;
        private readonly IServiceScopeFactory scopeFactory;

        public GameMarketController(ILogger<GameMarketController> logger, IServiceScopeFactory scopeFactory, DatabaseContext contex)
        {
            this.logger = logger;
            this.context = contex;
            this.scopeFactory = scopeFactory;
        }


        // GET: api/<GameMarketController>/342
        [HttpGet("FullMergedGames")]
        public ActionResult<IEnumerable<GameMarketDTO>> GetFullMergedGames()
        {
            try
            {
                var query = context.gameMarketTitles.AsQueryable();
                query = query
                    .Include(gt => gt.Developers)
                    .Include(gt => gt.Publishers);
                query = query
                    .Include(gt => gt.XboxLinks)
                        .ThenInclude(xl => xl.XboxTitle)
                            .ThenInclude(xt => xt.TitleDetails)
                                .ThenInclude(pd => pd.ProductIDNavig)
                                    .ThenInclude(pid => pid.MarketDetails);
                query = query
                    .Include(gt => gt.SteamLinks)
                        .ThenInclude(sl => sl.AppDetails);
                var result = query.ToList();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return NoContent();
            }

        }

        [HttpGet("MergedGamesList")]
        public ActionResult<IEnumerable<GameMarketListDTO>> GetMergedGamesList([FromQuery] string? platforms = null, [FromQuery] string? devices = null, [FromQuery] string? genres = null)
        {
            try
            {
                var platformList = platforms?.Split(',');
                var genreList = genres?.Split(',');
                var deviceList = devices?.Split(',');

                var query = context.gameMarketTitles.AsQueryable();

                if (platformList != null && platformList.Any())
                {
                    query = query.Where(gt =>
                        (platformList.Contains("Xbox") && gt.XboxLinks != null && gt.XboxLinks.Any()) ||
                        (platformList.Contains("Steam") && gt.SteamLinks != null && gt.SteamLinks.Any()));
                }

                if (deviceList != null && deviceList.Any())
                {
                    query = query.Where(gt => 
                        gt.XboxLinks.Any(xl => xl.XboxTitle.TitleDevices.Any(td => deviceList.Contains(td.device))) ||
                        gt.SteamLinks.Any(sl => sl.AppDetails.Platforms.Any(p => deviceList.Contains(p.platform)))
                    );

                }
                //Need to add the genre and device filter, not implemented yet
                //if (query == null) return NoContent();

                //Base Query
                query = query.Include(gt => gt.Developers)
                        .Include(gt => gt.Publishers);


                //Xbox Query
                query = query
                    .Include(gt => gt.XboxLinks)
                        .ThenInclude(xl => xl.XboxTitle)
                            .ThenInclude(xt => xt.TitleDetails)
                                .ThenInclude(pd => pd.ProductIDNavig)
                                    .ThenInclude(pid => pid.MarketDetails)
                                        .ThenInclude(pp => pp.ProductPlatforms);

                var temp = query.ToList();
                //steam Query
                query = query
                    .Include(gt => gt.SteamLinks)
                        .ThenInclude(sl => sl.AppDetails)
                        .ThenInclude(Ad => Ad.Platforms);

                var result = query
                    .Select(gt => new GameMarketListDTO
                    {
                        GameID = gt.gameID,
                        GameTitle = gt.gameTitle,

                        XboxPriceDetails = MappingProfile.MaptToXboxPriceDTO(gt.XboxLinks.SelectMany(xl => xl.XboxTitle.TitleDetails.Select(xx => xx.ProductIDNavig)).Select(gg => gg.MarketDetails).ToList()),
                        SteamPriceDetails = MappingProfile.MapToSteamPriceDTO(gt.SteamLinks.Select(s => s.AppDetails).ToList())
                    })
                    .ToList();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return NoContent();
            }

        }

        [HttpGet("MergedGames/{gameID}")]
        public ActionResult<GameMarketTitleDTO> GetMergedGameDetails(UInt32 gameID = 12806)
        {
            try
            {
                var query = context.gameMarketTitles.AsQueryable().Where(g=>g.gameID==gameID);

                //Base Query
                query = query.Include(gt => gt.Developers)
                        .Include(gt => gt.Publishers);

                //Xbox Query
                query = query
                    .Include(gt => gt.XboxLinks)
                        .ThenInclude(xl => xl.XboxTitle)
                            .ThenInclude(xt => xt.TitleDetails)
                                .ThenInclude(pd => pd.ProductIDNavig)
                                    .ThenInclude(pid => pid.MarketDetails)
                                        .ThenInclude(pp => pp.ProductPlatforms);

                query = query
                    .Include(gt => gt.XboxLinks)
                        .ThenInclude(xl => xl.XboxTitle)
                            .ThenInclude(xt => xt.TitleDetails)
                                .ThenInclude(xx => xx.GameBundles)
                                    .ThenInclude(pd => pd.ProductIDNavig)
                                        .ThenInclude(pid => pid.MarketDetails)
                                            .ThenInclude(pp => pp.ProductPlatforms);


                //steam Query
                query = query
                    .Include(gt => gt.SteamLinks)
                        .ThenInclude(sl => sl.AppDetails)
                            .ThenInclude(ad => ad.Platforms);
                query = query
                    .Include(gt => gt.SteamLinks)
                        .ThenInclude(sl => sl.AppDetails)
                            .ThenInclude(ad => ad.Packeges);

                var result = query
                    .Select(gt => new GameMarketTitleDTO
                    {
                        GameID = gt.gameID,
                        GameTitle = gt.gameTitle,
                        Developers = gt.Developers.Select(d => d.developer).ToList(),
                        Publishers = gt.Publishers.Select(p => p.publisher).ToList(),
                        XboxDetails = MappingProfile.MapToXboxDetailDTO(gt.XboxLinks.SelectMany(xl => xl.XboxTitle.TitleDetails).ToList()),
                        SteamDetails = MappingProfile.MapToSteamDetailDTO(gt.SteamLinks.Select(s => s.AppDetails).ToList())
                    }).FirstOrDefault();

                if (result != null)
                    return Ok(result);
                else return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return NoContent();
            }

        }

        
    }
}
