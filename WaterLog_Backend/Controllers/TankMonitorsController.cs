using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using WaterLog_Backend.Models;


namespace WaterLog_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TankMonitorsController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        public TankMonitorsController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }
        // GET api/segments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TankMonitorsEntry>>> GetTankData()
        { 
            return await _db.TankMonitors.ToListAsync();       
        }

        // GET api/TankMonitorById/
        [HttpGet("{id}")]
        public async Task<ActionResult<TankMonitorsEntry>> GetTankDataByID(int id)
        {
            try
            {
              var TankM = await _db.TankMonitors.FindAsync(id);
              if (TankM == null)
                {
                    return NotFound();
                }
                return TankM;
            }
            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
        }
    }
}