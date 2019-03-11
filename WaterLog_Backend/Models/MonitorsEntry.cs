using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class MonitorsEntry
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public double Max_flow { get; set; }
        public double Long { get; set; }
        public double Lat { get; set; }
        public string Status { get; set; } 
        public int FaultCount { get; set; }
    }
}
