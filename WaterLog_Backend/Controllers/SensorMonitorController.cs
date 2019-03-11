using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using WaterLog_Backend.Models;

namespace WaterLog_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorMonitorController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;

        public SensorMonitorController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        // GET api/Sensormonitor
        [HttpGet("{id}/{date}")]
        public async Task<ActionResult> Get(int id, DateTime date)
        {
            return Ok("date: " + date + "id: " + id);
        }

        // GET api/Sensormonitor
        [HttpPut("resolveSensor/{id}")]
        public async Task<ActionResult> Put(int id)
        {
            var oldMonitor = await _db.Monitors
                .Where(monitor => monitor.Id == id)
                .FirstOrDefaultAsync();
            if (oldMonitor == null)
            {
                return NotFound();
            }
            var updateMonitor = oldMonitor;
            updateMonitor.Status = "normal";
            _db.Entry(oldMonitor).CurrentValues.SetValues(updateMonitor);
            await _db.SaveChangesAsync();
            var oldhistories = await _db.SensorHistory.Where(history => history.SensorId == id).ToListAsync();
            if (oldhistories != null)
            {
                var latesthistory = oldhistories.Where(history => history.SensorResolved == EnumResolveStatus.UNRESOLVED)
                    .OrderByDescending(history => history.FaultDate).FirstOrDefault();
                if(latesthistory != null)
                {
                    SensorHistoryEntry newHistory = latesthistory;
                    newHistory.SensorResolved = latesthistory.SensorResolved == EnumResolveStatus.UNRESOLVED
                        ? EnumResolveStatus.RESOLVED
                        : EnumResolveStatus.UNRESOLVED;
                    newHistory.AttendedDate = DateTime.Now;
                    _db.Entry(latesthistory).CurrentValues.SetValues(newHistory);
                    await _db.SaveChangesAsync();
                }
            }

            return Ok(updateMonitor);
        }
    }
}
