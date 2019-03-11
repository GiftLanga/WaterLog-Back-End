using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class Globals
    {
        public static double RandPerLitre = 0.01068;
        public static double MinuteToHour = 60.00;
        //Number of elements per page
        public static int NumberItems = 5;
        //Number of continuous readings needed to be a leak to constitue a leak event.
        public static string BASE_URL = "https://iot.retrotest.co.za/alert";
        public static int LeakThreshold = 1;
        public static string Sensor = "Sensor";
        public static string Faulty = "faulty";
        public static string Segment = "Segment";
        public static string Leak = "leak";
        public enum COMPONENT_TYPES
        {
            SEGMENT = 1,
            SENSOR = 2,
            TANK = 3
        }
    }
}
