using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class TankObject
    {
        public int Id { get; set; }
        public double PercentageLevel { get; set; }
        public double OptimalLevel { get; set; }
        public string PumpStatus { get; set; }
    }
}
