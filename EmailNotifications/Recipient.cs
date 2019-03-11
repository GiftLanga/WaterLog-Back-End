using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    //Class to hold the Recipient for emails
    public class Recipient
    {
        public string Address;
        public string Name;

        public Recipient(string address,string name)
        {
            Address = address;
            Name = name;
        }

        public Recipient()
        {

        }
    }
}
