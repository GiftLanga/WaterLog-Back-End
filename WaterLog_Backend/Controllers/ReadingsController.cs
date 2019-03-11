using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class ReadingsController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        private IControllerService _service;

        public ReadingsController(DatabaseContext context, IConfiguration config, IControllerService service)
        {
            _db = context;
            _config = config;
            _service = service;
        }

        // GET api/readings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReadingsEntry>>> Get()
        {
            return await _db.Readings.ToListAsync();
        }

        // GET api/readingsById/
        [HttpGet("{id}")]
        public async Task<ActionResult<ReadingsEntry>> Get(int id)
        {
            return await _db.Readings.FindAsync(id);
        }

        // POST api/readings
        [HttpPost]
        public async Task Post([FromBody] ReadingsEntry value)
        {
            value.TimesStamp = DateTime.Now;
            await _db.Readings.AddAsync(value);
            await _db.SaveChangesAsync();
            Procedures procedure = new Procedures(_db, _config);
            await procedure.TriggerInsert(value);
        
        }

        [HttpPost("{value}")]
        public async Task Post([FromBody] InputSensor values)
        {
            if (values.valueIn == 0 && values.valueOut != 0)
            {
                MonitorsEntry mon = await _db.Monitors.FindAsync(values.IdIn);
                MonitorsEntry old = await _db.Monitors.FindAsync(values.IdIn);
                mon.Status = "faulty";
                mon.FaultCount = mon.FaultCount + 1;
                _db.Entry(old).CurrentValues.SetValues(mon);
                await _db.SaveChangesAsync();
                var sensorHistory = await _db.SensorHistory
                    .Where(h => h.SensorId == mon.Id)
                    .OrderByDescending(h => h.FaultDate)
                    .FirstOrDefaultAsync();
                if (sensorHistory == null)
                {
                    SensorHistoryEntry history = new SensorHistoryEntry();
                    history.SensorId = values.IdIn;
                    history.SensorType = EnumSensorType.WATER_FLOW_SENSOR;
                    history.SensorResolved = EnumResolveStatus.UNRESOLVED;
                    history.FaultDate = DateTime.Now;
                    history.EmailSentDate = DateTime.Now;
                    history.AttendedDate = DateTime.MinValue;
                    await _db.SensorHistory.AddAsync(history);
                }
                else
                {
                    var updatedHistory = sensorHistory;
                    updatedHistory.SensorResolved = EnumResolveStatus.UNRESOLVED;
                    updatedHistory.EmailSentDate = DateTime.Now;
                    _db.Entry(sensorHistory).CurrentValues.SetValues(updatedHistory);
                    await _db.SaveChangesAsync();
                }

            }
            else if(values.valueOut == 0 && values.valueIn != 0 || values.valueOut > values.valueIn)
            {
                MonitorsEntry mon = await _db.Monitors.FindAsync(values.IdOut);
                MonitorsEntry old = await _db.Monitors.FindAsync(values.IdOut);
                mon.Status = "faulty";
                mon.FaultCount = mon.FaultCount + 1;
                _db.Entry(old).CurrentValues.SetValues(mon);
                await _db.SaveChangesAsync();
                var sensorHistory = await _db.SensorHistory
                    .Where(h => h.SensorId == mon.Id)
                    .OrderByDescending(h => h.FaultDate)
                    .FirstOrDefaultAsync();
                if (sensorHistory == null)
                {
                    SensorHistoryEntry history = new SensorHistoryEntry();
                    history.SensorId = values.IdOut;
                    history.SensorType = EnumSensorType.WATER_FLOW_SENSOR;
                    history.SensorResolved = EnumResolveStatus.UNRESOLVED;
                    history.FaultDate = DateTime.Now;
                    history.EmailSentDate = DateTime.Now;
                    history.AttendedDate = DateTime.MinValue;
                    await _db.SensorHistory.AddAsync(history);
                } else
                {
                    var updatedHistory = sensorHistory;
                    updatedHistory.SensorResolved = EnumResolveStatus.UNRESOLVED;
                    updatedHistory.EmailSentDate = DateTime.Now;
                    _db.Entry(sensorHistory).CurrentValues.SetValues(updatedHistory);
                    await _db.SaveChangesAsync();
                }
            }

            ReadingsEntry reading = new ReadingsEntry();
            ReadingsEntry reading2 = new ReadingsEntry();
            reading.TimesStamp = DateTime.Now;
            reading2.TimesStamp = DateTime.Now;
            reading.Value = (values.valueIn)*Globals.MinuteToHour;
            reading2.Value = (values.valueOut)*Globals.MinuteToHour;
            reading.MonitorsId = values.IdIn;
            reading2.MonitorsId = values.IdOut;
            await _db.Readings.AddAsync(reading);
            await _db.SaveChangesAsync();
            await _db.Readings.AddAsync(reading2);
            await _db.SaveChangesAsync();
            Procedures procedure = new Procedures(_db, _config);
            await procedure.TriggerInsert(reading);
        }

        // PUT api/readings/
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] ReadingsEntry value)
        {
            try
            {
                var old = await _db.Readings.FindAsync(id);
            _db.Entry(old).CurrentValues.SetValues(value);
            await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("error", e);
            }
        }

        // DELETE api/readingsById/
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var entry = await _db.Readings.FindAsync(id);
            _db.Readings.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }
}
