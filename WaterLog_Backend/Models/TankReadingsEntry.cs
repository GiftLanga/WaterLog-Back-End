using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class TankReadingsEntry
    {
        public int Id { get; set; }
        public int TankMonitorsId { get; set; }
        public int PumpId { get; set; }
        public double PercentageLevel { get; set; }
        public double OptimalLevel { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
