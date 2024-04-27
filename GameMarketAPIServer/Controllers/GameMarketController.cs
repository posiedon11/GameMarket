﻿using GameMarketAPIServer.Models;
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
        private readonly DatabaseContext contex;
        private readonly IServiceScopeFactory scopeFactory;

        public GameMarketController(ILogger<GameMarketController> logger, IServiceScopeFactory scopeFactory, DatabaseContext contex)
        {
            this.logger = logger;
            this.contex = contex;
            this.scopeFactory = scopeFactory;
        }


        //GET: api/<GameMarketController>/GetAll
        //GET: api/<GameMarketController>/GetAll
        [HttpGet, Route("HJef")]
        public string Get()
        {
            var temp = contex.xboxTitles.First();
            return temp.titleName.ToString();
        }

        // GET: api/<GameMarketController>/342
        [HttpGet, Route("afdas")]
        public ActionResult<IEnumerable<XboxSchema.GameTitleTable>> GetGameTitle()
        {
            var temp = contex.xboxTitles
                .Where(gt => gt.TitleDetails != null && gt.TitleDetails.Count() > 0)
                .Include(gt => gt.TitleDetails).ToList();
            return temp;
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