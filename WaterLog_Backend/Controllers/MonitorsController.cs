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
using WaterLog_Backend;

namespace WaterLog_Backend.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class MonitorsController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        public MonitorsController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        // GET api/monitors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MonitorsEntry>>> Get()
        {
            try
            {
                var monitors = await _db.Monitors.ToListAsync();

                if(monitors == null)
                {
                    return NotFound();
                }

                return monitors;
            }
            catch (Exception e)
            {
                return NotFound();
            }
        }

        // GET api/monitors/
        [HttpGet("{id}")]
        public async Task<ActionResult<MonitorsEntry>> Get(int id)
        {
            return await _db.Monitors.FindAsync(id);
        }

        [Route("heat")]
        public  MonitorHeat[] getHeat()
        {
            Procedures procedure = new Procedures(_db, _config);
            List<MonitorHeat> results =  procedure.getMonitorsFaultLevels();
            return results.ToArray();
            
        }

        // POST api/monitors
        [HttpPost]
        public async Task Post([FromBody] MonitorsEntry value)
        {
            await _db.Monitors.AddAsync(value);
            await _db.SaveChangesAsync();
        }

        // PUT api/monitors/
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] MonitorsEntry value)
        {
            var old = await _db.Monitors.FindAsync(id);
            _db.Entry(old).CurrentValues.SetValues(value);
            await _db.SaveChangesAsync();
        }

        // DELETE api/monitors/
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var entry = await _db.Monitors.FindAsync(id);
            _db.Monitors.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }
}
