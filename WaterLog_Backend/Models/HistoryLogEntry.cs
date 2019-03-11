using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class HistoryLogEntry
    {
        public int Id { get; set; }
        public EnumTypeOfEvent Type { get; set; }
        [DefaultValue("0000-00-00 00:00:00.000")]
        public DateTime CreationDate { get; set; }
        [DefaultValue("0000-00-00 00:00:00.000")]
        public DateTime AutomaticDate { get; set; }
        [DefaultValue("0000-00-00 00:00:00.000")]
        public DateTime ManualDate { get; set; }
        public int EventsId { get; set; }
       
    }
}
