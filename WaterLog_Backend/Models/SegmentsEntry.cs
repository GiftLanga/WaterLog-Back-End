using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class SegmentsEntry
    {
        public int Id { get; set; }
        public int SenseIDOut { get; set; }
        public int SenseIDIn { get; set; }
        public int FaultCount { get; set; }
    }
}
