using NUnit.Framework;
using CheckValues;
using WaterLog_Backend.Models;
using System;
using System.Collections.Generic;
using WaterLog_Backend;

namespace UnitTesting
{
    class ForecastTest
    {
        [Test]
        public void given_start_second_end_dates_generate_correct_number_of_epochDates()
        {
            Forecast f = new Forecast();
            DateTime start = new DateTime(2019, 02, 12);
            DateTime second = new DateTime(2019, 02, 13);
            DateTime end = new DateTime(2019, 02, 15);
            var arr = f.generateUnixEpochFromDatetime(start, end, 4);

            Assert.AreEqual(4, arr.Count);
        }
    }
}
