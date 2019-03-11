using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WaterLog_Backend.Models;

namespace WaterLog_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CostForecastController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        public CostForecastController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        [Route("daily")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LinearRegressionModel>>> GenerateLinearCostForecast()
        {
            Forecast forecast = new Forecast();
            //get the cost data and dates
            Procedures proc = new Procedures(_db, _config);
            var results = (await proc.SummaryPeriodCostsAsync(Procedures.Period.Daily)).FirstOrDefault();

            LinearRegressionModel data = new LinearRegressionModel();
            if (results.dataPoints.Count == 0)
            {
                var _day = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                var _date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, _day);
                data.rSquared = 0;
                data.yIntercept = 0;
                data.slope = 0;
                data.start = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
                data.end = new DateTimeOffset(_date).ToUnixTimeSeconds();
                data.numOfElements = 0;

                return new List<LinearRegressionModel> { data };
            }
            if (results.dataPoints.Count == 1)
            {
                var _day = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                var _date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, _day);
                data.rSquared = 0;
                data.yIntercept = results.dataPoints.FirstOrDefault().y;
                data.slope = 0;
                data.start = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
                data.end = new DateTimeOffset(_date).ToUnixTimeSeconds();
                data.numOfElements = 1;

                return new List<LinearRegressionModel> { data };
            }
            var x = results.dataPoints.Select(row => row.x); //datetime data
            var y = results.dataPoints.Select(row => row.y); //cost data <double>

            DateTime start = x.ElementAt(0);
            DateTime second = x.ElementAt(1);
            DateTime end = x.Last();

            var epochDates = forecast.generateUnixEpochFromDatetime(start, end, x.Count());

            double rSquared, yIntercept, slope;

            forecast.LinearRegression(epochDates.ToArray(), y.ToArray(), out rSquared, out yIntercept, out slope);

            data.rSquared = rSquared;
            data.yIntercept = yIntercept;
            data.slope = slope;
            data.start = epochDates.First();
            data.end = epochDates.Last();
            data.numOfElements = epochDates.Count();

            return new List<LinearRegressionModel> { data };
        }

        [Route("monthly/{id}")]
        [HttpGet]
        public async Task<ActionResult<double>> GetMonthCostForecast(int id)
        {
            Procedures P = new Procedures();
            var thisMonthsEvents = P.SummaryDailyCost(await _db
                    .SegmentEvents.Where(a => a.TimeStamp.Month == id && a.TimeStamp.Year == DateTime.Now.Year)
                    .GroupBy(b => b.TimeStamp.Hour)
                    .ToListAsync()).FirstOrDefault();

            if (thisMonthsEvents.dataPoints.Count == 0)
            {
                return 0;
            }
            if (thisMonthsEvents.dataPoints.Count == 1)
            {
                return thisMonthsEvents.dataPoints.FirstOrDefault().y;
            }
            var orderedEvents = thisMonthsEvents.dataPoints.OrderBy(d => d.x.Date);
            var x = (orderedEvents.ToArray()).Select(row => row.x); //datetime data
            var y = (orderedEvents.ToArray()).Select(row => row.y); //cost data <double>

            DateTime start = x.ElementAt(0);
            DateTime second = x.ElementAt(1);
            DateTime end = x.Last();

            Forecast forecast = new Forecast();
            var epochDates = forecast.generateUnixEpochFromDatetime(start, end, y.Count());

            if (thisMonthsEvents.dataPoints.Count == 2)
            {
                var _slope = (y.ElementAt(1) - y.ElementAt(0)) / (epochDates[1] - epochDates[0]);
                var _day = DateTime.DaysInMonth(DateTime.Now.Year, id);
                var _date = new DateTime(DateTime.Now.Year, id, _day);
                var _yint = y.ElementAt(1) - _slope * epochDates[1];
                return _slope * (new DateTimeOffset(_date).ToUnixTimeSeconds()) + _yint;
            }

            double rSquared, yIntercept, slope;

            forecast.LinearRegression(epochDates.ToArray(), y.ToArray(), out rSquared, out yIntercept, out slope);
            double sum = 0;
            var day = DateTime.DaysInMonth(DateTime.Now.Year, id);

            for (int i = 1; i <= day; i++)
            {
                var date = new DateTime(DateTime.Now.Year, id, i);
                var toAdd = slope * (new DateTimeOffset(date).ToUnixTimeSeconds()) + yIntercept;

                sum += toAdd >= 0 ? toAdd : 0;
            }

            return sum;
        }

        [Route("falsePoints")]
        public List<DateTime> getDummyPointsHours()
        {
            List<DateTime> dummyHours = new List<DateTime>();
            for (int i = 0; i < 24; i++)
            {
                DateTime tempTime = DateTime.Now;
                DateTime returnV = new DateTime(tempTime.Year, tempTime.Month, tempTime.Day, i, 0, 0);
                dummyHours.Add(returnV);
            }

            return dummyHours;
        }

    }
}
