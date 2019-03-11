using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class TankMonitorsEntry
    {
        public int Id { get; set; }
        public double Long { get; set; }
        public double Lat { get; set; }
        public string Status { get; set; } 
        public int FaultCount { get; set; }
        public int connectedMonitorID { get; set; }
        public int connectedMonitorType { get; set; }
    }
}

