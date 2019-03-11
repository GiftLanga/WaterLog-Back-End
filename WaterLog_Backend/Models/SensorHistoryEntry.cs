using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class SensorHistoryEntry
    {
        public int Id { get; set; }
        public int SensorId { get; set; }
        public EnumResolveStatus SensorResolved { get; set; }
        public DateTime FaultDate { get; set; }
        public DateTime AttendedDate { get; set; }
        public DateTime EmailSentDate { get; set; } 
        public EnumSensorType SensorType { get; set; }
    }
}
