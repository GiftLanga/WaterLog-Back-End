using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class InputSensor
    {
        public int IdIn { get; set; }
        public int valueIn { get; set; }
        public int IdOut { get; set; }
        public int valueOut { get; set; }
    }
}
