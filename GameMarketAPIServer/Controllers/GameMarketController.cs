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


        //GET: api/<GameMarketController>/GetAll
        //GET: api/<GameMarketController>/GetAll
        [HttpGet("HJef")]
        public string Get()
        {
            var temp = context.xboxTitles.First();
            return temp.titleName.ToString();
        }

        // GET: api/<GameMarketController>/342
        [HttpGet("afdas")]
        public ActionResult<IEnumerable<XboxSchema.GameTitleTable>> GetGameTitle()
        {
            var temp = context.xboxTitles
                // .Where(gt => gt.TitleDetails != null && gt.TitleDetails.Count() > 0)
                .Include(gt => gt.TitleDetails).ToList();
            return temp;
        }


        [HttpGet("FullMergedGames")]
        public ActionResult<IEnumerable<GameMarketSchema.GameTitleTable>> GetFullMergedGames()
        {
            var temp = context.gameMarketTitles
                .Include(gt => gt.Developers)
                .Include(gt => gt.Publishers)
                .Include(gt => gt.XboxLinks)
                    .ThenInclude(xl => xl.XboxTitle)
                        .ThenInclude(xt => xt.TitleDetails)
                            .ThenInclude(pd => pd.ProductIDNavig)
                                .ThenInclude(pid => pid.MarketDetails)
                .Include(gt => gt.SteamLinks)
                    .ThenInclude(sl => sl.AppDetails)
                    .ToList();
            return temp;
        }

        // GET: api/<GameMarketController>/342
        [HttpGet("MergedGames")]
        public ActionResult<IEnumerable<GameMarketDTO>> GetMergedGames()
        {
            try
            {
                var temp = context.gameMarketTitles
                    .Include(gt => gt.Developers)
                    .Include(gt => gt.Publishers)
                    .Include(gt => gt.XboxLinks)
                        .ThenInclude(xl => xl.XboxTitle)
                            .ThenInclude(xt => xt.TitleDetails)
                                .ThenInclude(pd => pd.ProductIDNavig)
                                    .ThenInclude(pid => pid.MarketDetails)
                    .Include(gt => gt.SteamLinks)
                        .ThenInclude(sl => sl.AppDetails)
                    .Select(gt => new GameMarketDTO
                    {
                        GameID = gt.gameID,
                        GameTitle = gt.gameTitle,
                        Developers = gt.Developers.Select(d => d.developer).ToList(),
                        Publishers = gt.Publishers.Select(p => p.publisher).ToList(),
                        XboxMarketDetails = gt.XboxLinks.SelectMany(xl => xl.XboxTitle.TitleDetails.Select(xx => xx.ProductIDNavig)).Select(gg => gg.MarketDetails).ToList(),
                        SteamAppDetails = gt.SteamLinks.Select(s => s.AppDetails).ToList()
                    })
                    .ToList();
                return Ok(temp);
            } catch (Exception e)
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
                        (platformList.Contains("Xbox") && gt.XboxLinks!=null && gt.XboxLinks.Any()) ||
                        (platformList.Contains("Steam") && gt.SteamLinks != null && gt.SteamLinks.Any()));
                }
                if (deviceList != null && deviceList.Any())
                {
                    query = query.Where(gt =>gt.XboxLinks.Any(xl=>xl.XboxTitle.TitleDevices.Any(td=>deviceList.Contains(td.device))) ||
                        gt.SteamLinks.Any(sl=>sl.AppDetails.Platforms.Any(p=>deviceList.Contains(p.platform)))
                    );
                                           
                }

                //Need to add the genre and device filter, not implemented yet
                if (query == null) return NoContent();
                var temp = query
                    .Include(gt => gt.Developers)
                    .Include(gt => gt.Publishers)
                    .Include(gt => gt.XboxLinks)
                        .ThenInclude(xl => xl.XboxTitle)
                            .ThenInclude(xt => xt.TitleDetails)
                                .ThenInclude(pd => pd.ProductIDNavig)
                                    .ThenInclude(pid => pid.MarketDetails)
                                        .ThenInclude(pp=> pp.ProductPlatforms)
                    .Include(gt => gt.SteamLinks)
                        .ThenInclude(sl => sl.AppDetails)
                        .ThenInclude(Ad=>Ad.Platforms)
                    .Select(gt => new GameMarketListDTO
                    {
                        GameID = gt.gameID,
                        GameTitle = gt.gameTitle,

                        XboxPriceDetails = MappingProfile.MaptToXboxPriceDTO(gt.XboxLinks.SelectMany(xl => xl.XboxTitle.TitleDetails.Select(xx => xx.ProductIDNavig)).Select(gg => gg.MarketDetails).ToList()),
                        SteamPriceDetails = MappingProfile.MapToSteamPriceDTO(gt.SteamLinks.Select(s => s.AppDetails).ToList())
                    })
                    .ToList();
                return Ok(temp);
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return NoContent();
            }

        }

        [HttpGet("MergedGames/{gameID}")]
        public ActionResult<GameMarketTitleDTO> GetMergedGameDetails(UInt32 gameID = 3)
        {
            try
            {
                var temp = context.gameMarketTitles.Where(g => g.gameID == gameID)
                    .Include(gt => gt.Developers)
                    .Include(gt => gt.Publishers)
                    .Include(gt => gt.XboxLinks)
                        .ThenInclude(xl => xl.XboxTitle)
                            .ThenInclude(xt => xt.TitleDetails)
                                .ThenInclude(pd => pd.ProductIDNavig)
                                    .ThenInclude(pid => pid.MarketDetails)
                                        .ThenInclude(pp => pp.ProductPlatforms)
                    .Include(gt => gt.XboxLinks)
                        .ThenInclude(xl => xl.XboxTitle)
                            .ThenInclude(xt => xt.TitleDetails)
                                .ThenInclude(xx => xx.GameBundles)
                                    .ThenInclude(pd => pd.ProductIDNavig)
                                        .ThenInclude(pid => pid.MarketDetails)
                                            .ThenInclude(pp => pp.ProductPlatforms)
                    .Include(gt => gt.SteamLinks)
                        .ThenInclude(sl => sl.AppDetails)
                            .ThenInclude(ad => ad.Platforms)
                    .Include(gt => gt.SteamLinks)
                        .ThenInclude(sl => sl.AppDetails)
                            .ThenInclude(ad => ad.Packeges)
                    .Select(gt => new GameMarketTitleDTO
                    {
                        GameID = gt.gameID,
                        GameTitle = gt.gameTitle,
                        Developers = gt.Developers.Select(d => d.developer).ToList(),
                        Publishers = gt.Publishers.Select(p => p.publisher).ToList(),
                        XboxDetails = MappingProfile.MapToXboxDetailDTO(gt.XboxLinks.SelectMany(xl => xl.XboxTitle.TitleDetails).ToList()),
                        SteamDetails = MappingProfile.MapToSteamDetailDTO(gt.SteamLinks.Select(s => s.AppDetails).ToList())
                    }).FirstOrDefault();
                    
                if (temp!=null)
                 return Ok(temp);
                else return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return NoContent();
            }

        }
        // GET api/<GameMarketController>/5

        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
        //[HttpGet(Name ="Get all Games")]
        //public async Task<ActionResult<IEnumerable<GameMarketTitle>>> GetTitles()
        //{

        //}
        // POST api/<GameMarketController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<GameMarketController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<GameMarketController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
