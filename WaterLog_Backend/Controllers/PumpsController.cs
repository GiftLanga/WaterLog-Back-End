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
    public class PumpsController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        public PumpsController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        // GET api/pumps
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PumpEntry>>> Get()
        {
            return await _db.Pumps.ToListAsync();
        }

        // GET api/pumpsById/
        [HttpGet("{id}")]
        public async Task<ActionResult<PumpEntry>> Get(int id)
        {
            return await _db.Pumps.FindAsync(id);
        }

        // POST api/pump
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PumpEntry value)
        {
            if (ModelState.IsValid)
            {
                await _db.Pumps.AddAsync(value);
                await _db.SaveChangesAsync();
                return Ok("Ok");
            }
            else
            {
                return new BadRequestObjectResult("Not Found");
            }
        }

        // PUT api/pumps/
        [HttpPost("{id}")]
        public async Task<PumpEntry> TogglePump(int id)
        {
            try
            {
                var old = await _db.Pumps.FindAsync(id);
                if (old.Status == "on")
                {
                    old.Status = "off";
                }
                else
                {
                    old.Status = "on";
                }
                _db.Entry(old).CurrentValues.SetValues(old.Status);
                await _db.SaveChangesAsync();
                return old;
            }
            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
        }

        // DELETE api/pumps/
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            try
            {
                var entry = await _db.Pumps.FindAsync(id);
                _db.Pumps.Remove(entry);
                await _db.SaveChangesAsync();
            }
            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
        }
    }
}

