using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class SegmentLeaksEntry
    {
        public int Id { get; set; }
        public int SegmentsId { get; set; }
        public string Severity { get; set; }
        public DateTime OriginalTimeStamp { get; set; }
        public DateTime LatestTimeStamp { get; set; }
        public DateTime LastNotificationDate { get; set; }
        public EnumResolveStatus ResolvedStatus { get; set; }
    }
}
