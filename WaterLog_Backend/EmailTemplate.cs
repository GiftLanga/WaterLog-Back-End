using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WaterLog_Backend
{
    public class EmailTemplate
    {
        string section, status, severity, openperiod, resolveurl;
        double totalcost, perhourcost;
        public EmailTemplate(string section,string status, string severity, string openperiod,double totalcost,double perhourcost,string resolveurl)
        {
            this.section = section;
            this.status = status;
            this.openperiod = openperiod;
            this.perhourcost = perhourcost;
            this.resolveurl = resolveurl;
            this.severity = severity;
            this.totalcost = totalcost;
        }    
    }
}
