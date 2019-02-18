﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailNotifications;
using WaterLog_Backend.Models;

namespace WaterLog_Backend
{
    public class Procedures
    {
        DatabaseContext _db;
        IConfiguration _config;
        public Procedures() {

        }
        public Procedures(DatabaseContext db,IConfiguration cfg)
        {
            _db = db;
            _config = cfg;
        }

        public async Task triggerInsert(ReadingsEntry value)
        {
            IEnumerable<SegmentsEntry> allSegments = _db.Segments;
            int segmentInid = -1;
            int segmentOutid = -1;
            int segmentid = -1;
            foreach (SegmentsEntry seg in allSegments)
            {
                if (seg.SenseIDIn == value.MonitorsId)
                {
                    segmentInid = seg.SenseIDIn;
                    segmentOutid = seg.SenseIDOut;
                    segmentid = seg.Id;
                    break;
                }
            }

            ReadingsEntry reading1 = await _db.Readings
            .Where(r => r.MonitorsId == segmentInid)
            .OrderByDescending(r => r.TimesStamp)
            .FirstAsync();

            ReadingsEntry reading2 = await _db.Readings
            .Where(re => re.MonitorsId == segmentOutid)
            .OrderByDescending(re => re.TimesStamp)
            .FirstAsync();

            if (isLeakage(reading1.Value, reading2.Value))
            {
                //Updateleakagestatus
                IEnumerable<SegmentLeaksEntry> allLeaks = _db.SegmentLeaks;
                if (allLeaks.Any(leak => leak.SegmentsId == segmentid))
                {
                    SegmentLeaksEntry latestEntry = await _db.SegmentLeaks
                    .Where(leak => leak.SegmentsId == segmentid && leak.ResolvedStatus == "unresolved")
                    .OrderByDescending(lk => lk.LatestTimeStamp)
                    .FirstAsync();
                    //Check in SegmentEntry if latest event related to entry has been resolved.
                    if (latestEntry != null)
                    {
                        SegmentEventsEntry entry = (await _db.SegmentEvents
                        .Where(leak => leak.SegmentsId == segmentid)
                        .OrderByDescending(lks => lks.TimeStamp)
                        .FirstAsync());

                        if (entry.EventType == "leak")
                        {
                            await updateSegmentLeaksAsync(latestEntry.Id, segmentid, calculateSeverity(latestEntry), latestEntry.OriginalTimeStamp, entry.TimeStamp, "unresolved",latestEntry.LastNotificationDate);
                        }
                    }
                }
                else
                {
                    //Normal Add
                    await createSegmentLeaksAsync(segmentid, "unresolved");
                    string[] template = populateEmail(segmentid);
                    Email email = new Email(template,_config);
                    email.sendEmail();
                }
            }
            else
            {
                //Updatewithoutleakagestatus
                await updateSegmentsEventAsync(segmentid, "normal", reading1.Value, reading2.Value);
            }
        }

        public string calculateSeverity(SegmentLeaksEntry entry)
        {
            
            return calculateSeverityGivenValue(calculateTotaLitres(entry));

            
        }

        public string calculateSeverityGivenValue(double value)
        {
            if (value >= 100)
            {
                return "High";
            }
            else if (value < 50)
            {
                return "Low";
            }
            else
            {
                return "Medium";
            }
        }

        public async Task createSegmentLeaksAsync(int segId, string resolvedStatus)
        {
            SegmentLeaksEntry entry = new SegmentLeaksEntry();
            entry.SegmentsId = segId;
            entry.LatestTimeStamp = DateTime.Now;
            entry.OriginalTimeStamp = DateTime.Now;
            entry.LatestTimeStamp = DateTime.Now;
            entry.ResolvedStatus = resolvedStatus;
            entry.Severity = calculateSeverity(entry);
            await _db.SegmentLeaks.AddAsync(entry);
            await _db.SaveChangesAsync();

        }
        
        public async Task updateSegmentLeaksAsync(int leakId, int segId, string severity, DateTime original, DateTime updated, string resolvedStatus,DateTime lastEmail)
        {
            bool toSend = false;
            if((DateTime.Now - lastEmail).Days >= 1)
            {
                toSend = true;
                lastEmail = DateTime.Now;
            }
            SegmentLeaksEntry entry = new SegmentLeaksEntry();
            entry.SegmentsId = segId;
            entry.OriginalTimeStamp = original;
            entry.LatestTimeStamp = updated;
            entry.ResolvedStatus = resolvedStatus;
            entry.Id = leakId;
            entry.LastNotificationDate = lastEmail;
            var old = await _db.SegmentLeaks
            .FindAsync(leakId);

            _db.Entry(old)
            .CurrentValues
            .SetValues(entry);

            await _db.SaveChangesAsync();
            if (toSend)
            {
                string[] template = populateEmail(segId);
                Email email = new Email(template, _config);
                email.sendEmail();
            }
        }

        public async Task updateSegmentsEventAsync(int id, string status, double inv, double outv)
        {
            SegmentEventsEntry entry = new SegmentEventsEntry();
            entry.TimeStamp = DateTime.Now;
            entry.SegmentsId = id;
            entry.FlowIn = inv;
            entry.FlowOut = outv;
            entry.EventType = status;
            await _db.SegmentEvents
            .AddAsync(entry);

            await _db.SaveChangesAsync();
        }


        public Boolean isLeakage(double first, double second)
        {
            double margin = 2;
            if ((first - second) > margin)
            {
                return true;
            }
            return false;
        }

        public string[] populateEmail(int sectionid)
        {
            var leaks = _db.SegmentLeaks;
            var leak = leaks
            .Where(sudo => sudo.SegmentsId == sectionid)
            .Single();

            string[] template = { "Segment " + leak.SegmentsId, getSegmentStatus(leak.SegmentsId), leak.Severity, getLeakPeriod(leak), calculateTotalCost(leak).ToString(), calculatePerHourCost(leak).ToString(), calculateLitresPerHour(leak).ToString(), buildUrl(leak.SegmentsId) };
            return template;
        }

        private string getLeakPeriod(SegmentLeaksEntry leak)
        {

            if (((leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalHours) < 1)
            {
                return "1";
            }
            else
            {
                return ((leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalDays.ToString());
            }
        }

        public double calculateTotalCost(SegmentLeaksEntry leak)
        {
            var list = _db.SegmentEvents;
            var entry = list
            .Where(inlist => inlist.SegmentsId == leak.SegmentsId)
            .Last();

            var timebetween = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalHours;
            if (timebetween < 1)
            {
                return calculatePerHourCost(leak)/60;
            }
            else
            {
                timebetween = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalHours;
            }
            var perhour = calculatePerHourCost(leak);
            return (timebetween * perhour);
        }

        private string buildUrl(int segmentId)
        {
            return "https://iot.retrotest.co.za/alert/segment/" + segmentId;
        }

        public double calculatePerHourCost(SegmentLeaksEntry leak)
        {
            var list = _db.SegmentEvents;
            var entry = list
            .Where(inlist => inlist.SegmentsId == leak.SegmentsId)
            .Last();

            double currentTariff = 37.5;
            double usageperpoll = (entry.FlowIn - entry.FlowOut);
            return (usageperpoll * currentTariff);
        }

        public double calculateLitresPerHour(SegmentLeaksEntry leak)
        {
            var list = _db.SegmentEvents;
            var entry = list
            .Where(inlist => inlist.SegmentsId == leak.SegmentsId)
            .Last();

            double usageperpoll = (entry.FlowIn - entry.FlowOut);
            return (usageperpoll);
        }

        public double calculateTotaLitres(SegmentLeaksEntry leak)
        {
            var list = _db.SegmentEvents;
            var entry = list
            .Where(inlist => inlist.SegmentsId == leak.SegmentsId)
            .Last();

            var timebetween = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalHours;
            var perhour = calculateLitresPerHour(leak);
            return (timebetween * perhour);
        }

        private string getSegmentStatus(int segmentId)
        {
            var list = _db.SegmentEvents;
            var entry = list
            .Where(inlist => inlist.SegmentsId == segmentId)
            .Last();

            return entry.EventType;
        }

        //Calculates the data points of the wastage based on period
        public async Task<DataPoints<DateTime, double>[]> CalculatePeriodWastageAsync(Period timeframe)
        {
            switch (timeframe)
            {
                case Period.Daily:
                    return CalculateDailyWastage(await _db
                    .SegmentEvents.Where(a => a.EventType == "leak" && a.TimeStamp.Month == DateTime.Now.Month && a.TimeStamp.Day == DateTime.Now.Day && a.TimeStamp.Year == DateTime.Now.Year)
                    .GroupBy(b => b.TimeStamp.Hour)
                    .ToListAsync());

                case Period.Monthly:
                    return (CalculateMonthlyWastage(await _db.SegmentEvents.Where(a => a.EventType == "leak")
                    .GroupBy(b => b.TimeStamp.Month)
                    .ToListAsync()));

                case Period.Seasonally:
                    var summerList = await _db.SegmentEvents
                    .Where(a => a.EventType == "leak" && getSeason(a.TimeStamp, true) == 1)
                    .ToListAsync();

                    var winterList = await _db.SegmentEvents
                    .Where(a => a.EventType == "leak" && getSeason(a.TimeStamp,true) == 3)
                    .ToListAsync();

                    var autumnList = await _db.SegmentEvents
                    .Where(a => a.EventType == "leak" && getSeason(a.TimeStamp,true) == 2)
                    .ToListAsync();

                    var springList = await _db.SegmentEvents
                    .Where(a => a.EventType == "leak" && getSeason(a.TimeStamp,true) == 0)
                    .ToListAsync();

                    return CalculateSeasonallyWastage(summerList, winterList, autumnList, springList);
                default:
                    return null;
            }
        }

        private int getSeason(DateTime date, bool ofSouthernHemisphere)
        {
            int hemisphereConst = (ofSouthernHemisphere ? 2 : 0);
            Func<int, int> getReturn = (northern) => {
                return (northern + hemisphereConst) % 4;
            };
            float value = (float)date.Month + date.Day / 100f;  // <month>.<day(2 digit)>
            if (value < 3.21 || value >= 12.22) return getReturn(3);    // 3: Winter
            if (value < 6.21) return getReturn(0);  // 0: Spring
            if (value < 9.23) return getReturn(1);  // 1: Summer
            return getReturn(2);    // 2: Autumn
        }
        //Returns an array of yearly sorted data
        // 0 - summer
        // 1 - winter
        // 2 - spring
        // 3 - autumn
        public DataPoints<DateTime, double>[] CalculateSeasonallyWastage(List<SegmentEventsEntry> summer, List<SegmentEventsEntry> winter, List<SegmentEventsEntry> autumn, List<SegmentEventsEntry> spring)
        {
            try
            {
                //Get Summer
                var sortedSummer = CalculateYearlyWastage(summer.GroupBy(a => a.TimeStamp.Month).ToList());
                //Get Winter
                var sortedWinter = CalculateYearlyWastage(winter.GroupBy(a => a.TimeStamp.Month).ToList());
                //Get Autumn
                var sortedAutumn = CalculateYearlyWastage(autumn.GroupBy(a => a.TimeStamp.Month).ToList());
                //Get Spring
                var sortedSpring = CalculateYearlyWastage(spring.GroupBy(a => a.TimeStamp.Month).ToList());

                DataPoints<DateTime, double>[] arrayOfSeasons = new DataPoints<DateTime, double>[4];

                arrayOfSeasons[0] = sortedSummer[0];
                arrayOfSeasons[1] = sortedWinter[0];
                arrayOfSeasons[2] = sortedSpring[0];
                arrayOfSeasons[3] = sortedAutumn[0];

                return arrayOfSeasons;
            }
            catch(Exception error)
            {
                throw new Exception(error.Message);
            }
        }

        public DataPoints<DateTime, double>[] CalculateYearlyWastage(List<IGrouping<int, SegmentEventsEntry>> list)
        {
            try
            {
                DataPoints<DateTime, double> yearly = new DataPoints<DateTime, double>();
                var totalForMonth = 0.0;
                for (int i = 0; i < list.Count; i++)
                {
                    //We have a list per hour of current day.
                    //Group these groups by segmentId
                    totalForMonth = 0.0;
                    var segments = list.ElementAt(i).GroupBy(a => a.SegmentsId);
                    foreach (IGrouping<int, SegmentEventsEntry> lst in segments)
                    {
                        foreach (SegmentEventsEntry lst2 in lst)
                        {
                            totalForMonth += ((lst2.FlowIn - lst2.FlowOut) / 60);

                        }

                    }
                    yearly.AddPoint(list.ElementAt(i).ElementAt(0).TimeStamp, totalForMonth);
                }
                DataPoints<DateTime, double>[] ret = new DataPoints<DateTime, double>[1];

                ret[0] = yearly;
                return ret;
            }
            catch(Exception error)
            {
                throw new Exception(error.Message);
            }
        }

        public DataPoints<DateTime, double>[] CalculateMonthlyWastage(List<IGrouping<int, SegmentEventsEntry>> list)
        {
            try
            {
                DataPoints<DateTime, double> monthly = new DataPoints<DateTime, double>();
                var totalForDay = 0.0;
                for (int i = 0; i < list.Count; i++)
                {
                    //We have a list per hour of current day.
                    //Group these groups by segmentId
                    totalForDay = 0.0;
                    var segments = list.ElementAt(i).GroupBy(a => a.SegmentsId);
                    foreach (IGrouping<int, SegmentEventsEntry> lst in segments)
                    {
                        foreach (SegmentEventsEntry lst2 in lst)
                        {
                            totalForDay += ((lst2.FlowIn - lst2.FlowOut) / 60);

                        }

                    }
                    monthly.AddPoint(list.ElementAt(i).ElementAt(0).TimeStamp, totalForDay);
                }

                //See what months to add
                List<int> monthsAlreadyThere = new List<int>();
                foreach (DataNode<DateTime, double> var in monthly.dataPoints)
                {
                    monthsAlreadyThere.Add(var.x.Month);
                }

                for (int i = 1; i <= 12; i++)
                {
                    if (!(monthsAlreadyThere.Contains(i)))
                    {
                        monthly.AddPoint(new DateTime(2000, i, 1), 0.0);
                    }
                }
                monthly.dataPoints = monthly.dataPoints.OrderBy(a => a.x.Month).ToList();
                DataPoints<DateTime, double>[] ret = new DataPoints<DateTime, double>[1];
                ret[0] = monthly;
                return ret;
            }
            catch(Exception error)
            {
                throw new Exception(error.Message);
            }
        }

        public DataPoints<DateTime, double>[] CalculateDailyWastage(List<IGrouping<int, SegmentEventsEntry>> list)
        {
            try
            {
                DataPoints<DateTime, double> daily = new DataPoints<DateTime, double>();
                var totalForHour = 0.0;
                for (int i = 0; i < list.Count; i++)
                {
                    //We have a list per hour of current day.
                    //Group these groups by segmentId
                    totalForHour = 0.0;
                    var segments = list.ElementAt(i).GroupBy(a => a.SegmentsId);
                    foreach (IGrouping<int, SegmentEventsEntry> lst in segments)
                    {
                        foreach (SegmentEventsEntry lst2 in lst)
                        {
                            totalForHour += ((lst2.FlowIn - lst2.FlowOut) / 60);

                        }

                    }
                    daily.AddPoint(list.ElementAt(i).ElementAt(0).TimeStamp, totalForHour);
                }
                DataPoints<DateTime, double>[] ret = new DataPoints<DateTime, double>[1];
                ret[0] = daily;
                return ret;
            }
            catch(Exception error)
            {
                throw new Exception(error.Message);
            }
        }

        public enum Period
        {
            Daily,
            Seasonally,
            Monthly
        }
    }
}
