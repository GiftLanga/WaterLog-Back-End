using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using WaterLog_Backend.Models;

namespace WaterLog_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SegmentEventsController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        public SegmentEventsController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        // GET api/events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SegmentEventsEntry>>> GetAllSegmentEvents()
        {
            return await _db.SegmentEvents.ToListAsync();
        }

        // GET api/eventsById/
        [HttpGet("{id}")]
        public async Task<ActionResult<SegmentEventsEntry>> GetSegmentById(int id)
        {
            var segment = await _db.SegmentEvents.FindAsync(id);

            if (segment == null)
            {
                return NotFound();
            }
            return segment;
        }

        //POST api/events
        [HttpPost]
        public async Task AddSegmentEvent([FromBody] SegmentEventsEntry value)
        {
            try
            {
                await _db.SegmentEvents.AddAsync(value);
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("error", e);
            }
        }

        [Route("dailywastage")]
        public async Task<DataPoints<DateTime, double>> GetDailyWastageGraphData()
        {
            var proc = new Procedures(_db, _config);
            var ret = await proc.CalculatePeriodWastageAsync(Procedures.Period.Daily);
            var outV = new DataPoints<DateTime, double>();
            var dummyHours = new List<DateTime>();
            for (int i = 0; i < 24; i++)
            {
                DateTime tempTime = DateTime.Now;
                DateTime returnV = new DateTime(tempTime.Year, tempTime.Month, tempTime.Day, i, 0, 0);
                for (int j =0; j< ret.Length; j++)
                {
                    if(ret.ElementAt(j).dataPoints.Count < 1)
                    {
                        continue;
                    }
                    var dateValue = ret.ElementAt(j).getvalueT();
                    var reading = ret.ElementAt(j).getValueY();
                    if (dateValue.ElementAt(0).Hour == returnV.Hour)
                    {
                        outV.AddPoint(dateValue.ElementAt(0), reading.ElementAt(0));
                    }
                    else
                    {
                        outV.AddPoint(dateValue.ElementAt(0), 0);
                    }
                }              
            }

            return outV;
        }

        [Route("monthlywastage")]
        public async Task<DataPoints<DateTime, double>> GetMonthlyWastageGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            var ret = await proc.CalculatePeriodWastageAsync(Procedures.Period.Monthly);
            return ret.FirstOrDefault();
        }

        [Route("seasonallywastage")]
        public async Task<DataPoints<DateTime, double>[]> GetSeasonallyWastageGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            return await proc.CalculatePeriodWastageAsync(Procedures.Period.Seasonally);
        }

        //Gets Alerts only loading max 10 elements at a time
        [Route("GetAlerts/{id}")]
        public async Task<List<GetAlerts>> GetAlertsByPage(int id = 1)
        {
            List<GetAlerts> ListOfAlerts = new List<GetAlerts>();
            //Get Entries from SegmentLeaks
            var leaksQuery = await _db.SegmentLeaks.OrderByDescending(a => a.OriginalTimeStamp)
                                      .OrderByDescending(b => b.ResolvedStatus).Skip((id - 1) * Globals.NumberItems)
                                      .Take(Globals.NumberItems).ToListAsync();

            if (leaksQuery.Count != 0)
            {
                leaksQuery = leaksQuery.OrderByDescending(a => a.OriginalTimeStamp)
                    .OrderByDescending(a => a.ResolvedStatus).ToList();

                var proc = new Procedures(_db, _config);

                foreach (SegmentLeaksEntry entry in leaksQuery)
                {
                    double totalSystemLitres = await proc.CalculateTotalUsageLitres(entry),
                            litresUsed = await proc.CalculateTotalWastageLitres(entry),
                            perhourwastagelitre = await proc.CalculatePerHourWastageLitre(entry),
                            cost = await proc.CalculatePerHourWastageCost(entry);

                    //Find Litre Usage
                    ListOfAlerts.Add
                     (
                         new GetAlerts
                         (
                             entry.OriginalTimeStamp,
                             (entry
                             .LatestTimeStamp
                             .Subtract(entry
                             .OriginalTimeStamp) < TimeSpan.Zero ? TimeSpan.Zero : entry
                             .LatestTimeStamp.
                             Subtract(entry
                             .OriginalTimeStamp)),
                             Globals.Segment,
                             entry.SegmentsId,
                             Globals.Leak,
                             cost,
                             perhourwastagelitre,
                             entry.Severity,
                             litresUsed,
                             totalSystemLitres,
                             entry.ResolvedStatus
                          )
                     );
                }

                //Find All Sensors that are faulty
                var faultySensors = await _db.SensorHistory.OrderByDescending(a => a.FaultDate)
                    .OrderByDescending(b => b.SensorResolved)
                    .Skip((id - 1) * Globals.NumberItems).Take(Globals.NumberItems).ToListAsync();

                if (faultySensors.Count != 0)
                {
                    foreach (SensorHistoryEntry entry in faultySensors)
                    {

                        var sensorInfo = await _db.Monitors.Where(a => a.Id == entry.SensorId).FirstOrDefaultAsync();
                        var latestReading = await _db.Readings.Where(a => a.MonitorsId == entry.SensorId)
                            .OrderByDescending(a => a.TimesStamp).FirstOrDefaultAsync();

                        ListOfAlerts.Add
                        (
                            new GetAlerts
                            (
                                entry.FaultDate,
                                (entry
                                .AttendedDate
                                .Subtract(entry.AttendedDate) < TimeSpan.Zero ? TimeSpan.Zero : entry
                                .FaultDate.
                                Subtract(entry
                                .AttendedDate)),
                                Globals.Sensor,
                                entry.SensorId,
                                Globals.Faulty,
                                0.0,
                                0.0,
                                "High",
                                latestReading.Value,
                                sensorInfo.Max_flow,
                                entry.SensorResolved
                             )
                         );
                    }
                }
            }
            return ListOfAlerts.OrderByDescending(a => a.Date).OrderByDescending(a => a.Status).ToList();
        }

        [HttpGet("GetAlertsFilter")]
        public async Task<List<GetAlerts>> GetAlertsByPage (
            [FromQuery(Name = "segment")] int segment, 
            [FromQuery(Name = "sensorType")] int sensorType, 
            [FromQuery(Name = "sensorId")] int sensorId, 
            [FromQuery(Name = "severity")] int severity )
        {

            //Get Entries from SegmentLeaks
            //ToDo chnage events table to have enum like filter
            string tableSeverity = "";
            if (severity == 1)
            {
                tableSeverity = "Low";
            }
            else if (severity == 2)
            {
                tableSeverity = "Medium";
            }
            else
            {
                tableSeverity = "High";
            }
           
            List<GetAlerts> ListOfAlerts = new List<GetAlerts>();
            //Get Entries from SegmentLeaks
            //filter for segmentId
            if (segment != 0)
            {
                if (severity != 0)
                {
                    var leaksQuery = await _db.SegmentLeaks
                                          .Where(a => a.SegmentsId == segment && a.Severity == tableSeverity)
                                          .OrderByDescending(a => a.OriginalTimeStamp)
                                          .OrderByDescending(b => b.ResolvedStatus).Skip((1 - 1) * Globals.NumberItems)
                                          .Take(Globals.NumberItems).ToListAsync();
                    if (leaksQuery.Count != 0)
                    {
                        var proc = new Procedures(_db, _config);
                        foreach (SegmentLeaksEntry entry in leaksQuery)
                        {
                            double totalSystemLitres = await proc.CalculateTotalUsageLitres(entry),
                                    litresUsed = await proc.CalculateTotalWastageLitres(entry),
                                    perhourwastagelitre = await proc.CalculatePerHourWastageLitre(entry),
                                    cost = await proc.CalculatePerHourWastageCost(entry);

                            //Find Litre Usage
                            ListOfAlerts.Add
                             (
                                 new GetAlerts
                                 (
                                     entry.OriginalTimeStamp,
                                     (entry
                                     .LatestTimeStamp
                                     .Subtract(entry
                                     .OriginalTimeStamp) < TimeSpan.Zero ? TimeSpan.Zero : entry
                                     .LatestTimeStamp.
                                     Subtract(entry
                                     .OriginalTimeStamp)),
                                     Globals.Segment,
                                     entry.SegmentsId,
                                     Globals.Leak,
                                     cost,
                                     perhourwastagelitre,
                                     entry.Severity,
                                     litresUsed,
                                     totalSystemLitres,
                                     entry.ResolvedStatus
                                  )
                             );
                        }
                    }
                }
                else
                {
                    var leaksQuery = await _db.SegmentLeaks
                                          .Where(a => a.SegmentsId == segment)
                                          .OrderByDescending(a => a.OriginalTimeStamp)
                                          .OrderByDescending(b => b.ResolvedStatus).Skip((1 - 1) * Globals.NumberItems)
                                          .Take(Globals.NumberItems).ToListAsync();
                    if (leaksQuery.Count != 0)
                    {
                        var proc = new Procedures(_db, _config);
                        foreach (SegmentLeaksEntry entry in leaksQuery)
                        {
                            double totalSystemLitres = await proc.CalculateTotalUsageLitres(entry),
                                    litresUsed = await proc.CalculateTotalWastageLitres(entry),
                                    perhourwastagelitre = await proc.CalculatePerHourWastageLitre(entry),
                                    cost = await proc.CalculatePerHourWastageCost(entry);

                            //Find Litre Usage
                            ListOfAlerts.Add
                             (
                                 new GetAlerts
                                 (
                                     entry.OriginalTimeStamp,
                                     (entry
                                     .LatestTimeStamp
                                     .Subtract(entry
                                     .OriginalTimeStamp) < TimeSpan.Zero ? TimeSpan.Zero : entry
                                     .LatestTimeStamp.
                                     Subtract(entry
                                     .OriginalTimeStamp)),
                                     Globals.Segment,
                                     entry.SegmentsId,
                                     Globals.Leak,
                                     cost,
                                     perhourwastagelitre,
                                     entry.Severity,
                                     litresUsed,
                                     totalSystemLitres,
                                     entry.ResolvedStatus
                                  )
                             );
                        }
                    }
                }
            }
            else
            {
                if (severity != 0)
                {
                    var leaksQuery = await _db.SegmentLeaks
                                          .Where(a => a.Severity == tableSeverity)
                                          .OrderByDescending(a => a.OriginalTimeStamp)
                                          .OrderByDescending(b => b.ResolvedStatus).Skip((1 - 1) * Globals.NumberItems)
                                          .Take(Globals.NumberItems).ToListAsync();
                    if (leaksQuery.Count != 0)
                    {
                        var proc = new Procedures(_db, _config);
                        foreach (SegmentLeaksEntry entry in leaksQuery)
                        {
                            double totalSystemLitres = await proc.CalculateTotalUsageLitres(entry),
                                    litresUsed = await proc.CalculateTotalWastageLitres(entry),
                                    perhourwastagelitre = await proc.CalculatePerHourWastageLitre(entry),
                                    cost = await proc.CalculatePerHourWastageCost(entry);

                            //Find Litre Usage
                            ListOfAlerts.Add
                             (
                                 new GetAlerts
                                 (
                                     entry.OriginalTimeStamp,
                                     (entry
                                     .LatestTimeStamp
                                     .Subtract(entry
                                     .OriginalTimeStamp) < TimeSpan.Zero ? TimeSpan.Zero : entry
                                     .LatestTimeStamp.
                                     Subtract(entry
                                     .OriginalTimeStamp)),
                                     Globals.Segment,
                                     entry.SegmentsId,
                                     Globals.Leak,
                                     cost,
                                     perhourwastagelitre,
                                     entry.Severity,
                                     litresUsed,
                                     totalSystemLitres,
                                     entry.ResolvedStatus
                                  )
                             );
                        }
                    }
                }
                else
                {
                    var leaksQuery = await _db.SegmentLeaks
                                          .OrderByDescending(a => a.OriginalTimeStamp)
                                          .OrderByDescending(b => b.ResolvedStatus).Skip((1 - 1) * Globals.NumberItems)
                                          .Take(Globals.NumberItems).ToListAsync();
                    if (leaksQuery.Count != 0)
                    {
                        var proc = new Procedures(_db, _config);
                        foreach (SegmentLeaksEntry entry in leaksQuery)
                        {
                            double totalSystemLitres = await proc.CalculateTotalUsageLitres(entry),
                                    litresUsed = await proc.CalculateTotalWastageLitres(entry),
                                    perhourwastagelitre = await proc.CalculatePerHourWastageLitre(entry),
                                    cost = await proc.CalculatePerHourWastageCost(entry);

                            //Find Litre Usage
                            ListOfAlerts.Add
                             (
                                 new GetAlerts
                                 (
                                     entry.OriginalTimeStamp,
                                     (entry
                                     .LatestTimeStamp
                                     .Subtract(entry
                                     .OriginalTimeStamp) < TimeSpan.Zero ? TimeSpan.Zero : entry
                                     .LatestTimeStamp.
                                     Subtract(entry
                                     .OriginalTimeStamp)),
                                     Globals.Segment,
                                     entry.SegmentsId,
                                     Globals.Leak,
                                     cost,
                                     perhourwastagelitre,
                                     entry.Severity,
                                     litresUsed,
                                     totalSystemLitres,
                                     entry.ResolvedStatus
                                  )
                             );
                        }
                    }
                }
            }
            //end filter segments
            //faulty sensors filter 
            var faultySensors = await _db.SensorHistory
               .OrderByDescending(a => a.FaultDate)
               .OrderByDescending(b => b.SensorResolved)
               .Skip((1 - 1) * Globals.NumberItems).Take(Globals.NumberItems).ToListAsync();

            if (faultySensors.Count != 0)
            {
                foreach (SensorHistoryEntry entry in faultySensors)
                {
                    if (sensorType == 0)
                    {
                        var latestReading = await _db.TankReadings
                            .Where(a => a.TankMonitorsId == entry.SensorId)
                            .OrderByDescending(a => a.TimeStamp).FirstOrDefaultAsync();
                        if (entry.SensorId == sensorId && (int)entry.SensorType == (int)EnumSensorType.WATER_LEVEL_SENSOR)
                        {
                            ListOfAlerts.Add
                            (
                               new GetAlerts
                               (
                                   entry.FaultDate,
                                    (entry
                                    .AttendedDate
                                    .Subtract(entry.AttendedDate) < TimeSpan.Zero ? TimeSpan.Zero : entry
                                    .FaultDate.
                                    Subtract(entry
                                    .AttendedDate)),
                                    Globals.Sensor,
                                    entry.SensorId,
                                    Globals.Faulty,
                                   -1,
                                   -1,
                                   "High",
                                   latestReading.PercentageLevel,
                                   -1,
                                   entry.SensorResolved
                                )
                            );
                        }
                    }
                    else
                    {
                        var sensorInfo = await _db.Monitors.Where(a => a.Id == entry.SensorId).FirstOrDefaultAsync();
                        var latestReading = await _db.Readings
                            .Where(a => a.MonitorsId == entry.SensorId)
                            .OrderByDescending(a => a.TimesStamp).FirstOrDefaultAsync();
                        if (entry.SensorId == sensorId && (int)entry.SensorType == (int)EnumSensorType.WATER_FLOW_SENSOR)
                        {
                            ListOfAlerts.Add(
                                new GetAlerts
                                (
                                    entry.FaultDate,
                                     (entry
                                    .AttendedDate
                                    .Subtract(entry.AttendedDate) < TimeSpan.Zero ? TimeSpan.Zero : entry
                                    .FaultDate.
                                    Subtract(entry
                                    .AttendedDate)),
                                    Globals.Sensor,
                                    entry.SensorId,
                                    Globals.Sensor,
                                    0.0,
                                    0.0,
                                    "High",
                                    latestReading.Value,
                                    sensorInfo.Max_flow,
                                    entry.SensorResolved
                                )
                            );
                        }
                    }
                }
            } // filter faulty sensor end
            return (ListOfAlerts.OrderByDescending(a => a.Date).OrderByDescending(a => a.Status).ToList());
        }
        //Routing to get all currently opened events
        [Route("GetAlerts")]
        public async Task<List<GetAlerts>> GetAlerts()

        {
            try
            {
                //Get all leaks first
                var leaks = await _db.SegmentLeaks.ToListAsync();
                leaks = leaks.OrderByDescending(a => a.OriginalTimeStamp).OrderByDescending(a => a.ResolvedStatus).ToList();
                if (leaks.Count != 0)
                {
                    List<GetAlerts> alerts = new List<GetAlerts>();
                    var proc = new Procedures(_db, _config);
                    foreach (SegmentLeaksEntry entry in leaks)
                    {
                        double totalSystemLitres = -1.0, litresUsed = -1.0,
                            perhourwastagelitre = await proc.CalculatePerHourWastageLitre(entry),
                            cost = await proc.CalculatePerHourWastageCost(entry);

                        //Find Cost
                        if (entry.ResolvedStatus == EnumResolveStatus.UNRESOLVED)
                        {
                            totalSystemLitres = await proc.CalculateTotalUsageLitres(entry);
                            litresUsed = await proc.CalculateTotalWastageLitres(entry);
                        }

                        //Find Litre Usage
                        alerts.Add
                        (
                            new GetAlerts
                            (
                                entry.OriginalTimeStamp, 
                                (entry
                                 .LatestTimeStamp
                                 .Subtract(entry
                                 .OriginalTimeStamp) < TimeSpan.Zero ? TimeSpan.Zero : entry
                                 .LatestTimeStamp.
                                 Subtract(entry
                                 .OriginalTimeStamp)),
                                 Globals.Segment,
                                 entry.SegmentsId,
                                 Globals.Leak,
                                 cost,
                                 perhourwastagelitre,
                                 entry.Severity,
                                 litresUsed,
                                 totalSystemLitres,
                                 entry.ResolvedStatus
                             )
                        );
                    }

                    //Find All Sensors that are faulty
                    var faultySensors = await _db.SensorHistory.ToListAsync();
                    if (faultySensors.Count != 0)
                    {
                        foreach (SensorHistoryEntry entry in faultySensors)
                        {
                            var sensorInfo = await _db.Monitors.Where(a => a.Id == entry.SensorId).FirstOrDefaultAsync();
                            var latestReading = await _db.Readings.Where(a => a.MonitorsId == entry.SensorId)
                                .OrderByDescending(a => a.TimesStamp).FirstOrDefaultAsync();

                            alerts.Add
                            (
                                new GetAlerts
                                (
                                    entry.FaultDate,
                                     (entry
                                    .AttendedDate
                                    .Subtract(entry.AttendedDate) < TimeSpan.Zero ? TimeSpan.Zero : entry
                                    .FaultDate.
                                    Subtract(entry
                                    .AttendedDate)),
                                    Globals.Sensor,
                                    entry.SensorId,
                                    Globals.Faulty,
                                    0.0,
                                    0.0,
                                    "High",
                                    latestReading.Value,
                                    sensorInfo.Max_flow,
                                    entry.SensorResolved
                                 )
                             );
                        }
                    }
                    return (alerts.OrderByDescending(a => a.Date).ToList());
                }
                throw new Exception("ERROR : Null SegmentLeaks");
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        [Route("dailyUsage")]
        public async Task<DataPoints<DateTime, double>> GetDailyUsgaeGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            var ret = await proc.SummaryPeriodUsageAsync(Procedures.Period.Daily);
            var outV = new DataPoints<DateTime, double>();
            var dummyHours = new List<DateTime>();
            for (int i = 0; i < 24; i++)
            {
                DateTime tempTime = DateTime.Now;
                DateTime returnV = new DateTime(tempTime.Year, tempTime.Month, tempTime.Day, i, 0, 0);
                for (int j = 0; j < ret.Length; j++)
                {
                    if (ret.ElementAt(j).dataPoints.Count < 1)
                    {
                        continue;
                    }
                    var dateValue = ret.ElementAt(j).getvalueT();
                    var reading = ret.ElementAt(j).getValueY();
                    if (dateValue.ElementAt(0).Hour == returnV.Hour)
                    {
                        outV.AddPoint(dateValue.ElementAt(0), reading.ElementAt(0));
                    }
                    else
                    {
                        outV.AddPoint(dateValue.ElementAt(0), 0);
                    }
                }
            }

            return outV;
        }
        [Route("monthlyUsage")]
        public async Task<DataPoints<DateTime, double>> GetMonthlyUsageGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            var ret = await proc.SummaryPeriodUsageAsync(Procedures.Period.Monthly);
            return ret.FirstOrDefault();
        }
        [Route("seasonallyUsage")]
        public async Task<DataPoints<DateTime, double>[]> GetSeasonallyUsgaeGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            return await proc.SummaryPeriodUsageAsync(Procedures.Period.Seasonally);
        }
        [Route("dailyCost")]
        public async Task<DataPoints<DateTime, double>> GetDailyCostGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            var ret = await proc.SummaryPeriodCostsAsync(Procedures.Period.Daily);
            var outV = new DataPoints<DateTime, double>();
            var dummyHours = new List<DateTime>();
            for (int i = 0; i < 24; i++)
            {
                DateTime tempTime = DateTime.Now;
                DateTime returnV = new DateTime(tempTime.Year, tempTime.Month, tempTime.Day, i, 0, 0);
                for (int j = 0; j < ret.Count(); j++)
                {
                    if (ret.ElementAt(j).dataPoints.Count < 1)
                    {
                        continue;
                    }
                    var dateValue = ret.ElementAt(j).getvalueT();
                    var reading = ret.ElementAt(j).getValueY();
                    if (dateValue.ElementAt(0).Hour == returnV.Hour)
                    {
                        outV.AddPoint(dateValue.ElementAt(0), reading.ElementAt(0));
                    }
                    else
                    {
                        outV.AddPoint(dateValue.ElementAt(0), 0);
                    }
                }
            }

            return outV;
        }
        [Route("monthlyCost")]
        public async Task<DataPoints<DateTime, double>> GetMonthlyCostGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            var ret = await proc.SummaryPeriodCostsAsync(Procedures.Period.Monthly);
            return ret.FirstOrDefault();
        }
        [Route("seasonallyCost")]
        public async Task<DataPoints<String, double>> GetSeasonallyUsageGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            var ret = await proc.SummaryPeriodCostsSeasonAsync();
            return ret;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task UpdateSegmentEvent(int id, [FromBody] SegmentEventsEntry value)
        {
            try
            {
                var old = await _db.SegmentEvents.FindAsync(id);
                _db.Entry(old).CurrentValues.SetValues(value);
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("error", e);
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task DeleteSegmentEvent(int id)
        {
            var entry = await _db.SegmentEvents.FindAsync(id);
            _db.SegmentEvents.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }
}
