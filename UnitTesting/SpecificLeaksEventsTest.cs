using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WaterLog_Backend;
using WaterLog_Backend.Controllers;
using WaterLog_Backend.Models;
using WebApplication1;

namespace UnitTesting
{
    class SpecificLeaksEventsTest
    {
        private IQueryable<SegmentLeaksEntry> mockData = new List<SegmentLeaksEntry>
        {
            new SegmentLeaksEntry
            {
                Id = 1, SegmentsId = 1, Severity = "High", ResolvedStatus = EnumResolveStatus.UNRESOLVED,
                LatestTimeStamp = DateTime.Now, OriginalTimeStamp = DateTime.Today.Date
            },

            new SegmentLeaksEntry
            {
                Id = 2, SegmentsId = 1, Severity = "Low", ResolvedStatus = EnumResolveStatus.RESOLVED,
                LatestTimeStamp = DateTime.Now, OriginalTimeStamp = DateTime.Today.Date
            }
        }.AsQueryable();
        
        [TestCase(1)]
        public async Task GetValue(int id) {
            var mockSet = new Mock<DbSet<SegmentLeaksEntry>>();
            mockSet.As<IQueryable<SegmentLeaksEntry>>().Setup(m => m.GetEnumerator()).Returns(mockData.GetEnumerator());
            var mockContext = new Mock<DatabaseContext>();
            mockContext.Setup(s => s.SegmentLeaks).Returns(mockSet.Object);
            var LeaksController = new SegmentLeaksController(mockContext.Object, null, null);
            var SegmentHistory = LeaksController.GetSegmentHistory(id);
            mockContext.Verify(x => x.SegmentLeaks, Times.Once);
        }
    }
}
