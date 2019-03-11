using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WaterLog_Backend.Models;

namespace WaterLog_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryLogController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        public HistoryLogController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        // GET api/eventhistory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HistoryLogEntry>>> Get()
        {
            return await _db.HistoryLogs.ToListAsync();
        }
    }
}