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
using Newtonsoft.Json;
using WaterLog_Backend.Models;

namespace WaterLog_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorHistoryController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        private IControllerService _service;

        public SensorHistoryController(DatabaseContext context, IConfiguration config, IControllerService service)
        {
            _db = context;
            _config = config;
            _service = service;
        } 
        // GET api/sensor history
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SensorHistoryEntry>>> GetAllHistory()
        {
            return await _db.SensorHistory.ToListAsync();
        } 
        // GET api/sensorHistory/
        [HttpGet("{id}")]
        public async Task<ActionResult<SensorHistoryEntry>> GetSensor(int id)
        {
            return await _db.SensorHistory.FindAsync(id);
        }
        // POST api/sensorHistory
        [HttpPost]
        public async Task<IActionResult> PostHistory([FromBody] SensorHistoryEntry value)
        {
            if (ModelState.IsValid)
            {
                await _db.SensorHistory.AddAsync(value);
                await _db.SaveChangesAsync();
                return Ok("Ok");
            }
            else
            {
                return new BadRequestObjectResult("Not Found");
            }
        }
        // DELETE api/sensorHistory/
        [HttpDelete("{id}")]
        public async Task DeleteHistory(int id)
        {
            try
            {
                var entry = await _db.SensorHistory.FindAsync(id);
                _db.SensorHistory.Remove(entry);
                await _db.SaveChangesAsync();
            }
            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
        }
        
        [Route("sensor/{id}/{date}")]
        public async Task<List<GetAlerts>> GetSensorByDate(int id, DateTime date)
        {
            var alert = await _db.SensorHistory
                .Where(a => a.FaultDate == date && a.SensorId == id)
                .FirstOrDefaultAsync();

            List<GetAlerts> alerts = new List<GetAlerts>();
            if (alert != null)
            {
                var sensorInfo = await _db.Monitors.Where(a => a.Id == alert.SensorId).FirstOrDefaultAsync();
                var latestReading = await _db.Readings.Where(a => a.MonitorsId == alert.SensorId)
                    .OrderByDescending(a => a.TimesStamp).FirstOrDefaultAsync();

                alerts.Add
                (
                    new GetAlerts
                    (
                        alert.FaultDate,
                        (alert.FaultDate.Subtract(alert.AttendedDate) < TimeSpan.Zero ? 
                            TimeSpan.Zero : 
                            alert.FaultDate.Subtract(alert.AttendedDate
                        )),
                        Globals.Sensor,
                        alert.SensorId,
                        Globals.Faulty,
                        0.0,
                        0.0,
                        "High",
                        latestReading.Value,
                        sensorInfo.Max_flow,
                        alert.SensorResolved
                     )
                 );
            }
            return alerts;
        }
    }
}
