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
    public class RealTimeController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        public RealTimeController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        [Route("pollnotifications")]
        public async Task<int> GetLatestNumberNotifications()
        {
            var leaksCount = await _db.SegmentLeaks.Where(b => b.ResolvedStatus == EnumResolveStatus.UNRESOLVED).CountAsync();
            var faultyCount = await _db.SensorHistory.Where(b => b.SensorResolved == EnumResolveStatus.UNRESOLVED).CountAsync();

            return (leaksCount + faultyCount);
        }
    }
}
