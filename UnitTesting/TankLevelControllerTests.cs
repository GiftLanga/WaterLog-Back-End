using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaterLog_Backend.Controllers;
using WaterLog_Backend.Models;

namespace UnitTesting
{
    class TankLevelControllerTests
    {
        private IQueryable<TankReadingsEntry> mockData = new List<TankReadingsEntry>
        {
            new TankReadingsEntry
            {
                TankMonitorsId  = 1,PumpId=1, PercentageLevel = 50, OptimalLevel = 80
            },
            new TankReadingsEntry
            {
                TankMonitorsId  = 2,PumpId=2, PercentageLevel= 20, OptimalLevel = 80
            },

        }.AsQueryable();

        [TestCase(1)]
        public void TestGetById(int id)
        {
            var mockSet = new Mock<DbSet<TankReadingsEntry>>();
            mockSet.As<IOrderedQueryable<TankReadingsEntry>>().Setup(x => x.GetEnumerator())
                .Returns(mockData.GetEnumerator());
            var mockContext = new Mock<DatabaseContext>();
            mockContext.Setup(dbc => dbc.TankReadings).Returns(mockSet.Object);
            var tankReadingsController = new TankReadingsController(mockContext.Object, null);
            mockContext.Setup(x => x.TankReadings.FindAsync(id))
                .Returns(Task.FromResult(mockData.Where(x => x.Id == id).First()));
            var levels = tankReadingsController.GetTankReadingsId(id).Result;
            Assert.IsNotNull(levels);
            Assert.AreEqual(levels.Value.Id, id);
            Assert.AreEqual(levels.Value.PercentageLevel, 50.0);
        }
    }
}
