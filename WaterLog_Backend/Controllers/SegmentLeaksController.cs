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
    public class SegmentLeaksController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        private IControllerService _service;

        public SegmentLeaksController(DatabaseContext context, IConfiguration config, IControllerService service)
        {
            _db = context;
            _config = config;
            _service = service;
        }

        // GET api/segmentleaks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SegmentLeaksEntry>>> Get()
        {
            return await _db.SegmentLeaks.ToListAsync();
        }

        [Route("costs/{id}")]
        public async Task<ActionResult<string>> GetCost(int id)
        {
            SegmentLeaksEntry leaks = await _db.SegmentLeaks.FindAsync(id);
            if (leaks == null)
            {
                return NotFound();
            }
            Procedures procedures = new Procedures(_db, _config);
            return (JsonConvert.SerializeObject(
                (procedures.CalculateTotalCostAsync(leaks),
                procedures.CalculatePerHourWastageCost(leaks)))
                );
        }

        [Route("litres/{id}")]
        public async Task<ActionResult<string>> GetLitres(int id)
        {
            SegmentLeaksEntry leaks = await _db.SegmentLeaks.FindAsync(id);
            if (leaks == null)
            {
                return NotFound();
            }
            Procedures procedures = new Procedures(_db, _config);
            return (JsonConvert.SerializeObject((procedures.CalculateTotalWastageLitres(leaks), procedures.CalculatePerHourWastageLitre(leaks))));
        }

        //Gets segment based on id and date
        [Route("segment/{id}/{date}")]
        public async Task<List<GetAlerts>> GetSegmentLeakByDate(int id, DateTime date)
        {
            var alert = await _db.SegmentLeaks
                .Where(a => a.OriginalTimeStamp == date && a.SegmentsId == id)
                .FirstOrDefaultAsync();

            List<GetAlerts> alerts = new List<GetAlerts>();
            if (alert != null)
            {
                Procedures proc = new Procedures(_db, _config);
                double totalSystemLitres = await proc.CalculateTotalUsageLitres(alert),
                           litresUsed = await proc.CalculateTotalWastageLitres(alert),
                           perhourwastagelitre = await proc.CalculatePerHourWastageLitre(alert),
                           cost = await proc.CalculatePerHourWastageCost(alert);

                alerts.Add(new GetAlerts(alert.OriginalTimeStamp,
                    (alert.LatestTimeStamp.Subtract(alert.OriginalTimeStamp) < TimeSpan.Zero ? TimeSpan.Zero :
                        alert.LatestTimeStamp.Subtract(alert.OriginalTimeStamp)),
                    "Segment", alert.SegmentsId, "leak", cost, perhourwastagelitre,
                    alert.Severity, litresUsed, totalSystemLitres, alert.ResolvedStatus));
            }
            return alerts;
        }

        // GET api/Sensormonitor
        [HttpPut("segment/{id}/{date}")]
        public async Task<ActionResult> Put(int id, DateTime date, [FromBody] SegmentLeaksEntry value)
        {
            var old = await _db.SegmentLeaks
                 .OrderByDescending(b => b.LatestTimeStamp)
                 .Where(a => a.OriginalTimeStamp == date)
                 .FirstOrDefaultAsync();
            _db.Entry(old).CurrentValues.SetValues(value);
            await _db.SaveChangesAsync();
            return Ok(value);
        }
        // GET api/segmentById/
        [HttpGet("{id}")]
        public async Task<ActionResult<SegmentLeaksEntry>> GetBySegmentId(int id)
        {
            //System makes assumption that segment and resolved status make entry distinct
            var leaks = await _db.SegmentLeaks
                .Where(a => a.SegmentsId == id && a.ResolvedStatus == EnumResolveStatus.UNRESOLVED)
                .FirstOrDefaultAsync();

            if (leaks == null)
            {
                return NotFound();
            }
            return leaks;
        }

        // GET api/segment
        [HttpGet("segment/{Id}")]
        public async Task<ActionResult<IEnumerable<SegmentLeaksEntry>>> GetSegmentHistory(int Id)
        {
            return await _db.SegmentLeaks.Where(row => row.SegmentsId == Id).ToListAsync();
        }

        // POST api/segment
        [HttpPost]
        public async Task Post([FromBody] SegmentLeaksEntry value)
        {
            await _db.SegmentLeaks.AddAsync(value);
            await _db.SaveChangesAsync();
        }

        // PUT api/segmentLeak
        [HttpPut("{id}")]
        public async Task Put(int id)
        {
            try
            {
                var old = await _db.SegmentLeaks.Where(leak => leak.SegmentsId == id)
                    .OrderByDescending(leak => leak.OriginalTimeStamp).FirstOrDefaultAsync();
                var updated = old;
                updated.ResolvedStatus = EnumResolveStatus.RESOLVED;
                _db.Entry(old).CurrentValues.SetValues(updated);
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("error", e);
            }
        }

        // DELETE api/segmentLeak/
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var entry = await _db.SegmentLeaks.FindAsync(id);
            _db.SegmentLeaks.Remove(entry);
            await _db.SaveChangesAsync();
        }

        [HttpPatch("{id}")]
        public async Task Patch([FromBody] SegmentLeaksEntry value)
        {
            var entry = _db.SegmentLeaks.FirstOrDefault(segL => segL.Id == value.Id);
            if (entry != null)
            {
                entry.LatestTimeStamp = value.LatestTimeStamp;
                await _db.SaveChangesAsync();
            }
        }
    }
}