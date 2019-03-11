using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class EventsList
    {
        public int Id { get; set; }
        public DateTime OriginalTimeStamp { get; set; }
        public string EventType = "Leak";
        public string Severity { get; set; }
        public float Cost = 0;
    }
}
