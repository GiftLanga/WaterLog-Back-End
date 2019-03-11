using NUnit.Framework;
using CheckValues;
using System.Linq;
using WaterLog_Backend.Models;
using WaterLog_Backend;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Tests
{
    public class Tests
    {
        [Test]
        public void TestCalculateDailyWastage()
        {
            var build = new ConfigurationBuilder();
            var _config = build.Build();
            var proc = new Procedures(new DatabaseContext(), _config);
            var expected = GetTestExpectedDaily();
            var dataset = GetTestDataDaily();
            Assert.IsTrue(expected.Equals((proc.CalculateDailyWastage(dataset))[0]));
        }

        [Test]
        public void TestCalculateMonthlyWastage()
        {
            var build = new ConfigurationBuilder();
            var _config = build.Build();
            var proc = new Procedures(new DatabaseContext(), _config);
            var expected = GetTestExpectedMonthly();
            var dataset = GetTestDataMonthly();
            Assert.IsTrue(expected.Equals((proc.CalculateMonthlyWastage(dataset))[0]));
        }

        [Test]
        public void TestCalculateSeasonallyWastage()
        {
            var build = new ConfigurationBuilder();
            IConfiguration _config = build.Build();
            var expected = GetTestExpectedSummer();
            var dataset = GetTestDataSummer();
            var empty = new List<SegmentEventsEntry>();
            Procedures proc = new Procedures(new DatabaseContext(), _config);
            Assert.IsTrue(expected.Equals((proc.CalculateSeasonallyWastage(dataset,empty,empty,empty))[0]));
        }

        private List<SegmentEventsEntry> GetTestDataSummer()
        {
            var lst = new List<SegmentEventsEntry>();
            lst.Add(GetSegmentEventsObject("leak", 120, 0, 1, "2/12/2019 12:04:00 PM"));
            lst.Add(GetSegmentEventsObject("leak", 240, 120, 1, "1 / 1 / 2019 12:10:15 PM"));
            lst.Add(GetSegmentEventsObject("leak", 120, 0, 2, "1/1/2019 12:20:0 PM"));
            lst.Add(GetSegmentEventsObject("leak", 240, 120, 2, "1/1/2019 12:50:15 PM"));
            lst.Add(GetSegmentEventsObject("leak", 600, 300, 1, "1/1/2019 13:10:15 PM"));
            lst.Add(GetSegmentEventsObject("leak", 120, 60, 3, "1/1/2019 13:20:00 PM"));
            lst.Add(GetSegmentEventsObject("leak", 240, 0, 1, "1/1/2019 13:40:00 PM"));
            lst.Add(GetSegmentEventsObject("leak", 60, 0, 3, "1/1/2019 14:05:15 PM"));
            lst.Add(GetSegmentEventsObject("leak", 60, 0, 1, "1/1/2019 14:10:15 PM"));
            lst.Add(GetSegmentEventsObject("leak", 120, 0, 1, "1/1/2019 14:55:15 PM"));

            return lst;
        }

        private DataPoints<DateTime, double> GetTestExpectedSummer()
        {
            //Build data
            var list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("2/12/2019 12:04:00 PM")), 2.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:10:15 PM")), 20.0);
            return list;
        }

        public DataPoints<DateTime, double> GetTestExpectedMonthly()
        {
            //Build data
            var list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:04:00 PM")), 8.0);
            list.AddPoint((Convert.ToDateTime("2/1/2019 13:10:15 PM")), 10.0);
            list.AddPoint((Convert.ToDateTime("3/1/2019 14:05:15 PM")), 4.0);
            list.AddPoint((Convert.ToDateTime("2000/4/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/5/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/6/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/7/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/8/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/9/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/10/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/11/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/12/1")), 0.0);
            return list;
        }

        public List<IGrouping<int, SegmentEventsEntry>> GetTestDataMonthly()
        {
            var lst = new List<SegmentEventsEntry>();
            lst.Add(GetSegmentEventsObject("leak", 120, 0, 1, "1/1/2019 12:04:00 PM"));
            lst.Add(GetSegmentEventsObject("leak", 240, 120, 1, "1 / 1 / 2019 12:10:15 PM"));
            lst.Add(GetSegmentEventsObject("leak", 120, 0, 2, "1/1/2019 12:20:0 PM"));
            lst.Add(GetSegmentEventsObject("leak", 240, 120, 2, "1/1/2019 12:50:15 PM"));
            lst.Add(GetSegmentEventsObject("leak", 600, 300, 1, "2/1/2019 13:10:15 PM"));
            lst.Add(GetSegmentEventsObject("leak", 120, 60, 3, "2/1/2019 13:20:00 PM"));
            lst.Add(GetSegmentEventsObject("leak", 240, 0, 1, "2/1/2019 13:40:00 PM"));
            lst.Add(GetSegmentEventsObject("leak", 60, 0, 3, "3/1/2019 14:05:15 PM"));
            lst.Add(GetSegmentEventsObject("leak", 60, 0, 1, "3/1/2019 14:10:15 PM"));
            lst.Add(GetSegmentEventsObject("leak", 120, 0, 1, "3/1/2019 14:55:15 PM"));
            return lst.GroupBy(a => a.TimeStamp.Hour).ToList();
        }

        public DataPoints<DateTime,double> GetTestExpectedDaily()
        {
            //Build data
            var list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:04:00 PM")),8.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 13:10:15 PM")), 10.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 14:05:15 PM")), 4.0);

            return list;
        }

        public List<IGrouping<int, SegmentEventsEntry>> GetTestDataDaily()
        {
            var lst = new List<SegmentEventsEntry>();
            lst.Add(GetSegmentEventsObject("leak", 120, 0, 1, "1/1/2019 12:04:00 PM"));
            lst.Add(GetSegmentEventsObject("leak", 240, 120, 1, "1 / 1 / 2019 12:10:15 PM"));
            lst.Add(GetSegmentEventsObject("leak", 120, 0, 2, "1/1/2019 12:20:0 PM"));
            lst.Add(GetSegmentEventsObject("leak", 240, 120, 2, "1/1/2019 12:50:15 PM"));
            lst.Add(GetSegmentEventsObject("leak", 600, 300, 1, "1/1/2019 13:10:15 PM"));
            lst.Add(GetSegmentEventsObject("leak", 120, 60, 3, "1/1/2019 13:20:00 PM"));
            lst.Add(GetSegmentEventsObject("leak", 240, 0, 1, "1/1/2019 13:40:00 PM"));
            lst.Add(GetSegmentEventsObject("leak", 60, 0, 3, "1/1/2019 14:05:15 PM"));
            lst.Add(GetSegmentEventsObject("leak", 60, 0, 1, "1/1/2019 14:10:15 PM"));
            lst.Add(GetSegmentEventsObject("leak", 120, 0, 1, "1/1/2019 14:55:15 PM"));
            return lst.GroupBy(a => a.TimeStamp.Hour).ToList();    
        }

        public SegmentEventsEntry GetSegmentEventsObject(string eve,double inn, double outt,int segid,string time)
        {
            var obj = new SegmentEventsEntry();
            obj.EventType = eve;
            obj.FlowIn = inn;
            obj.FlowOut = outt;
            obj.SegmentsId = segid;
            obj.TimeStamp = Convert.ToDateTime(time);
            return obj;
        }

        [Test]
        public void TestsummaryUsageDaily()
        {
            var build = new ConfigurationBuilder();
            var _config = build.Build();
            var proc = new Procedures(new DatabaseContext(), _config);
            var expected = GetTestExpectedSummaryDaily();
            var dataset = GetTestDataSummaryDaily();
            Assert.IsTrue(expected.Equals((proc.SummaryDailyUsage(dataset))[0]));
        }

        [Test]
        public void TestsummaryUsageMonthly()
        {
            var build = new ConfigurationBuilder();
            var _config = build.Build();
            var proc = new Procedures(new DatabaseContext(), _config);
            var expected = GetTestExpectedSummaryMonthly();
            var dataset = GetTestDataSummaryMonthly();
            Assert.IsTrue(expected.Equals((proc.SummaryMonthlyUsage(dataset))[0]));
        }

        [Test]
        public void TestSummarySeasonallyUsage()
        {
            var build = new ConfigurationBuilder();
            var _config = build.Build();
            var proc = new Procedures(new DatabaseContext(), _config);
            var expected = GetTestExpectedSummarySummer();
            var dataset = GetTestDataSummarySummer();
            var empty = new List<SegmentEventsEntry>();
            Assert.IsTrue(expected.Equals((proc.SummarySeasonallyUsage(dataset, empty, empty, empty))[0]));
        }

        [Test]
        public void TestsummaryUsageCost()
        {
            var build = new ConfigurationBuilder();
            var _config = build.Build();
            var proc = new Procedures(new DatabaseContext(), _config);
            var expected = GetTestExpectedSummaryDailyCost();
            var dataset = GetTestDataSummaryDailyCost();
            Assert.IsTrue(expected.Equals((proc.SummaryDailyCost(dataset))[0]));
        }

        [Test]
        public void TestsummaryMonthlyCost()
        {
            var build = new ConfigurationBuilder();
            var _config = build.Build();
            var proc = new Procedures(new DatabaseContext(), _config);
            var expected = GetTestExpectedSummaryMonthlyCost();
            var dataset = GetTestDataSummaryMonthly();
            Assert.IsTrue(expected.Equals((proc.SummaryMonthlyCost(dataset))[0]));
        }

        [Test]
        public void TestSummarySeasonallyCost()
        {
            var build = new ConfigurationBuilder();
            var _config = build.Build();
            var proc = new Procedures(new DatabaseContext(), _config);
            var expected = GetTestExpectedSummarySummerCost();
            var dataset = GetTestDataSummarySummer();
            var empty = new List<SegmentEventsEntry>();
            var seasons = proc.SummarySeasonallyUsage(dataset, empty, empty, empty);
            DataPoints<DateTime, double> list = new DataPoints<DateTime, double>();
        }
        private List<SegmentEventsEntry> GetTestDataSummarySummer()
        {
            List<SegmentEventsEntry> lst = new List<SegmentEventsEntry>();
            lst.Add(GetSegmentObject("leak", 120, 0, 1, "2/12/2019 12:04:00 PM"));
            lst.Add(GetSegmentObject("leak", 240, 120, 1, "1 / 1 / 2019 12:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 0, 2, "1/1/2019 12:20:0 PM"));
            lst.Add(GetSegmentObject("leak", 240, 120, 2, "1/1/2019 12:50:15 PM"));
            lst.Add(GetSegmentObject("leak", 600, 300, 1, "1/1/2019 13:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 60, 3, "1/1/2019 13:20:00 PM"));
            lst.Add(GetSegmentObject("leak", 240, 0, 1, "1/1/2019 13:40:00 PM"));
            lst.Add(GetSegmentObject("leak", 60, 0, 3, "1/1/2019 14:05:15 PM"));
            lst.Add(GetSegmentObject("leak", 60, 0, 1, "1/1/2019 14:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 0, 1, "1/1/2019 14:55:15 PM"));

            return lst;
        }

        private DataPoints<DateTime, double> GetTestExpectedSummarySummer()
        {
            //Build data
            DataPoints<DateTime, double> list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("2/12/2019 12:04:00 PM")), 2.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:10:15 PM")), 30.0);
            return list;
        }
        private DataPoints<String, double> GetTestExpectedSummarySummerCost()
        {
            //Build data
            DataPoints<String, double> list = new DataPoints<String, double>();
            list.AddPoint("Summer", 1184.0);
            list.AddPoint("Winter", 0.0);
            list.AddPoint("Spring", 0.0);
            list.AddPoint("Autum", 0.0);
            return list;
        }

        public DataPoints<DateTime, double> GetTestExpectedSummaryMonthlyCost()
        {
            //Build data
            DataPoints<DateTime, double> list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:04:00 PM")), 444.0);
            list.AddPoint((Convert.ToDateTime("2/1/2019 13:10:15 PM")), 592.0);
            list.AddPoint((Convert.ToDateTime("3/1/2019 14:05:15 PM")), 148.0);
            list.AddPoint((Convert.ToDateTime("2000/4/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/5/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/6/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/7/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/8/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/9/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/10/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/11/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/12/1")), 0.0);

            return list;
        }
        public DataPoints<DateTime, double> GetTestExpectedSummaryMonthly()
        {
            //Build data
            DataPoints<DateTime, double> list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:04:00 PM")), 12.0);
            list.AddPoint((Convert.ToDateTime("2/1/2019 13:10:15 PM")), 16.0);
            list.AddPoint((Convert.ToDateTime("3/1/2019 14:05:15 PM")), 4.0);
            list.AddPoint((Convert.ToDateTime("2000/4/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/5/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/6/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/7/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/8/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/9/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/10/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/11/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/12/1")), 0.0);

            return list;
        }
        public List<IGrouping<int, SegmentEventsEntry>> GetTestDataSummaryMonthly()
        {
            List<SegmentEventsEntry> lst = new List<SegmentEventsEntry>();
            lst.Add(GetSegmentObject("leak", 120, 0, 1, "1/1/2019 12:04:00 PM"));
            lst.Add(GetSegmentObject("leak", 240, 120, 1, "1 / 1 / 2019 12:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 0, 2, "1/1/2019 12:20:0 PM"));
            lst.Add(GetSegmentObject("leak", 240, 120, 2, "1/1/2019 12:50:15 PM"));
            lst.Add(GetSegmentObject("leak", 600, 300, 1, "2/1/2019 13:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 60, 3, "2/1/2019 13:20:00 PM"));
            lst.Add(GetSegmentObject("leak", 240, 0, 1, "2/1/2019 13:40:00 PM"));
            lst.Add(GetSegmentObject("leak", 60, 0, 3, "3/1/2019 14:05:15 PM"));
            lst.Add(GetSegmentObject("leak", 60, 0, 1, "3/1/2019 14:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 0, 1, "3/1/2019 14:55:15 PM"));

            return lst.GroupBy(a => a.TimeStamp.Hour).ToList();
        }

        public DataPoints<DateTime, double> GetTestExpectedSummaryDailyCost()
        {
            //Build data
            DataPoints<DateTime, double> list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:04:00 PM")), 444.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 13:10:15 PM")), 592.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 14:05:15 PM")), 148.0);

            return list;
        }

        public DataPoints<DateTime, double> GetTestExpectedSummaryDaily()
        {
            //Build data
            DataPoints<DateTime, double> list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:04:00 PM")), 12.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 13:10:15 PM")), 16.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 14:05:15 PM")), 4.0);

            return list;
        }

        public List<IGrouping<int, SegmentEventsEntry>> GetTestDataSummaryDailyCost()
        {
            List<SegmentEventsEntry> lst = new List<SegmentEventsEntry>();
            lst.Add(GetSegmentObject("leak", 120, 0, 1, "1/1/2019 12:04:00 PM"));
            lst.Add(GetSegmentObject("leak", 240, 120, 1, "1 / 1 / 2019 12:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 0, 2, "1/1/2019 12:20:0 PM"));
            lst.Add(GetSegmentObject("leak", 240, 120, 2, "1/1/2019 12:50:15 PM"));
            lst.Add(GetSegmentObject("leak", 600, 300, 1, "1/1/2019 13:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 60, 3, "1/1/2019 13:20:00 PM"));
            lst.Add(GetSegmentObject("leak", 240, 0, 1, "1/1/2019 13:40:00 PM"));
            lst.Add(GetSegmentObject("leak", 60, 0, 3, "1/1/2019 14:05:15 PM"));
            lst.Add(GetSegmentObject("leak", 60, 0, 1, "1/1/2019 14:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 0, 1, "1/1/2019 14:55:15 PM"));

            return lst.GroupBy(a => a.TimeStamp.Hour).ToList();
        }
        public List<IGrouping<int, SegmentEventsEntry>> GetTestDataSummaryDaily()
        {
            List<SegmentEventsEntry> lst = new List<SegmentEventsEntry>();
            lst.Add(GetSegmentObject("leak", 120, 0, 1, "1/1/2019 12:04:00 PM"));
            lst.Add(GetSegmentObject("leak", 240, 120, 1, "1 / 1 / 2019 12:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 0, 2, "1/1/2019 12:20:0 PM"));
            lst.Add(GetSegmentObject("leak", 240, 120, 2, "1/1/2019 12:50:15 PM"));
            lst.Add(GetSegmentObject("leak", 600, 300, 1, "1/1/2019 13:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 60, 3, "1/1/2019 13:20:00 PM"));
            lst.Add(GetSegmentObject("leak", 240, 0, 1, "1/1/2019 13:40:00 PM"));
            lst.Add(GetSegmentObject("leak", 60, 0, 3, "1/1/2019 14:05:15 PM"));
            lst.Add(GetSegmentObject("leak", 60, 0, 1, "1/1/2019 14:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 0, 1, "1/1/2019 14:55:15 PM"));

            return lst.GroupBy(a => a.TimeStamp.Hour).ToList();
        }

        public SegmentEventsEntry GetSegmentObject(string eve, double inn, double outt, int segid, string time)
        {
            SegmentEventsEntry obj = new SegmentEventsEntry();
            obj.EventType = eve;
            obj.FlowIn = inn;
            obj.FlowOut = outt;
            obj.SegmentsId = segid;
            obj.TimeStamp = Convert.ToDateTime(time);

            return obj;
        }
    }
}