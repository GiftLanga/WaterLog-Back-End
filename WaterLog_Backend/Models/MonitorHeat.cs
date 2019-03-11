using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class MonitorHeat
    {
        public int Id { get; set; }
        public double Long { get; set; }
        public double Lat { get; set; }
        public string FaultLevel { get; set; }
    }
}
