﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class SegmentLeaksEntry
    {
        public int Id { get; set; }
        public int SegmentId { get; set; }

        [ForeignKey("SegmentId")]
        public SegmentsEntry SegmentsEntry { get; set; }

        public string Severity { get; set; }
        public DateTime OriginalTimeStamp { get; set; }
        public DateTime LatestTimeStamp { get; set; }
        public string ResolvedStatus { get; set; }
    }
}
