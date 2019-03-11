using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    //This class acts as a way to structure queries from the database and provide to the front end.
    public class GetAlerts
    {
        public DateTime Date;
        //How long the event lasted
        public TimeSpan Duration;
        public string EntityName;
        public int EntityId;
        public string EntityType;
        public double Cost;
        //Litres lost per hour based on sensor reading
        public double LitresPerHour;
        public string Severity;
        //Litres related to the Type IE- Leak
        public double TypeLitres;
        //Total Litres used throughout the system during the event timeline
        public double TotalLitres;
        //Resolution status related to all entities
        public EnumResolveStatus Status;

        public GetAlerts(
            DateTime date,
            TimeSpan duration,
            string entityname, 
            int entityid, 
            string entitytype, 
            double cost, 
            double litresperhour,
            string severity, 
            double totalperlitre,
            double totallitre,
            EnumResolveStatus status
        )
        {
            Date = date;
            Duration = duration;
            EntityName = entityname;
            EntityType = entitytype;
            EntityId = entityid;
            Cost = cost;
            LitresPerHour = litresperhour;
            Severity = severity;
            TypeLitres = totalperlitre;
            TotalLitres = totallitre;
            Status = status;
        }
    }
}
