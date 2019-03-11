using System;
using System.ComponentModel;
using System.Net.Mail;
using System.Web;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.IO;
using WaterLog_Backend.Models;
using System.Collections.Generic;

namespace EmailNotifications
{
    public class Email
    {
        static bool mailSent = false;
        string[] values;
        IConfiguration _conf;
        public Email(string[] emailValue,IConfiguration config)
        {
            values = emailValue;
            _conf = config;        
        }
        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            String token = (string)e.UserState;
            if (e.Cancelled)
            {
                Console.WriteLine("[{0}] Send canceled.", token);
            }
            if (e.Error != null)
            {
                Console.WriteLine("[{0}] {1}", token, e.Error.ToString());
            }
            else
            {
                Console.WriteLine("Message sent.");
            }
            mailSent = true;
        }

        public void SendMail(Recipient[] recipient)
        {
            if (recipient != null)
            {
                string email = ConstructEmail();
                if (email == "ERROR")
                {
                    throw new Exception("Error : No Values Given");
                }
                else if (recipient.Length < 1)
                {
                    throw new Exception("Error : No Address Given");
                }
                else
                {
                    try
                    {
                        var client = new SmtpClient("smtp.gmail.com", 587)
                        {
                            //TODO: Move email address to config file
                            Credentials = new NetworkCredential("nmotsumi@retrorabbit.co.za", _conf.GetSection("Password").Value),
                            EnableSsl = true
                        };

                        //Set From
                        MailAddress from = new MailAddress(_conf.GetSection("Sender").Value, _conf.GetSection("SenderName").Value);

                        //Set To
                        MailAddress to = new MailAddress(recipient[0].Address, recipient[0].Name);
                        MailMessage message = new MailMessage(from, to);
                        for (int i = 1; i < recipient.Length; i++)
                        {
                            message.CC.Add(new MailAddress(recipient[i].Address, recipient[i].Name));
                        }

                        message.Subject = "WaterLog Notification";
                        message.IsBodyHtml = true;
                        message.Body = email;
                        client.Send(message);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("{0} Exception caught.", e);
                    }
                }
            }
            else
            {
                throw new Exception("ERROR : Recipients Cannot Be Null");
            }
        }

        public string ConstructEmail()
        {
            //Define Dictionary
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            dictionary.Add("entityFullName",0);
            dictionary.Add("entityEvent", 1);
            dictionary.Add("entitySeverity", 2);
            dictionary.Add("entityDuration", 3);
            dictionary.Add("entityTotalCost", 4);
            dictionary.Add("entityPerHourWastageCost", 5);
            dictionary.Add("entityPerHourWastageLitre", 6);
            dictionary.Add("entityURL", 7);
           
            if (values.Length > 0)
            {
                string[] styleProperties = {
                GetSeverityColor(values[dictionary["entitySeverity"]],
                values[dictionary["entityEvent"]])+";padding-top: 40px;",
                GetSeverityColor(values[dictionary["entitySeverity"]],
                values[dictionary["entityEvent"]]),
                "color:black;padding-top: 11px;",
                GetSeverityColor(values[dictionary["entitySeverity"]],
                values[dictionary["entityEvent"]])+ ";padding-top: 35px;",
                "color:black;",
                "color:black;padding-top: 20px;",
                "color:grey;" ,
                "color:grey;",
                "color:red;padding-top: 24px;",
                GetLogItStyle(values[dictionary["entityEvent"]]),
                "color:grey;padding-top: 9px;" };
                string[] fontSizeProperties = { "6", "3", "3", "6", "4", "6", "4", "4", "4", "4", "2" };
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                using (TableStructure.Table table = new TableStructure.Table(sb, id: "some-id", align: "center"))
                {
                    table.StartBody();
                    int count = 0;
                    string[] items =
                    {
                        "<b>" + values[dictionary["entityFullName"]] + 
                            " is " + BuildVerb(values[dictionary["entityEvent"]]) + "</b>",
                        "<b>" + GetSeverityDescription(values[dictionary["entitySeverity"]],
                        values[dictionary["entityEvent"]])+ "</b>",
                            "This problem has been <b>unresolved for " +
                            GetRelevantUnit(values[dictionary["entityDuration"]])+ "</b>",
                        "<b>" + GetRelevantRand(values[dictionary["entityFullName"]],
                        values[dictionary["entityEvent"]],
                        values[dictionary["entityTotalCost"]],
                        values[dictionary["entityPerHourWastageCost"]]) + "</b>",
                        "<b>"+ GetRelevantDescription(values[dictionary["entityFullName"]],
                        values[dictionary["entityEvent"]])+"</b>",
                        "<b>" + GetRelevantLossPH(values[dictionary["entityFullName"]],
                        values[dictionary["entityEvent"]],
                        values[dictionary["entityPerHourWastageLitre"]],
                        values[dictionary["entityPerHourWastageCost"]]) + "</b>",
                        GetRelevantLossDescriptionLine1(values[dictionary["entityFullName"]],
                        values[dictionary["entityEvent"]]),
                        GetRelevantLossDescriptionLine2(values[dictionary["entityFullName"]],
                        values[dictionary["entityEvent"]]),"",
                        (values[dictionary["entityEvent"]].ToLower() == "resolved" ? 
                            "" : values[dictionary["entityURL"]]),
                        GetPhoneHelp(values[dictionary["entityEvent"]])
                    };
                    foreach (var alert in items)
                    {
                        using (var tr = table.AddRow(classAttributes: "someattributes"))
                        {
                            if (count == 9)
                            {
                                tr.AddCell(alert, align: "center", style: styleProperties[count], fontSize: fontSizeProperties[count], url: true);
                            }
                            else
                            {
                                tr.AddCell(alert, align: "center", style: styleProperties[count], fontSize: fontSizeProperties[count]);
                            }
                            count++;
                        }
                    }
                    using (var tr = table.AddRow(classAttributes: "someattributes2"))
                    {
                        tr.AddImage("https://res.cloudinary.com/retro-rabbit/image/upload/v1549634899/logo.png", align: "center", style: "padding-left: 70px", sizeX: "100", sizeY: "100");

                    }
                    table.EndBody();
                }

                return sb.ToString();
            }
            else
            {
                //Something went wrong
                return "ERROR";
            }
        }

        private string GetSeverityDescription(string severity, string entityType)
        {
            switch (entityType.ToLower())
            {
                case "resolved":
                    return "";
                default:
                    return "(" + severity + ")";
            }
        }

        private string GetPhoneHelp(string entityEvent)
        {
            switch (entityEvent.ToLower())
            {
                case "resolved":
                    return "";
                default:
                    return "call third party help: <u>&zwj;011111929292</u>";
            }
        }

        private string GetLogItStyle(string status)
        {
            switch (status.ToLower())
            {
                case "resolved":
                    return "pointer-events: none;";
                default:
                    return "opacity:1;";
            }
        }

        private string GetSeverityColor(string severity)
        {
            switch (severity.ToLower())
            {
                case "low":
                    return "color:#ffea00";
                case "medium":
                    return "color:#ffab00";
                case "high":
                    return "color:#ff1744";
            }
            return "color:#ffea00";
        }

        private string GetSeverityColor(string severity,string entityEvent)
        {
            if(entityEvent.ToLower() == "resolved")
            {
                return "color:#56CCF7";
            }
            else
            {
                return GetSeverityColor(severity);
            }
        }

        private string BuildVerb(string entityEventType)
        {
            if(entityEventType == "leak")
            {
                return "leaking";
            }
            if(entityEventType == "sufficient")
            {
                return "at a Sufficient Level";
            }
            else
            {
                return entityEventType;
            }
        }

        private string GetRelevantLossDescriptionLine2(string entity, string entityType)
        {
            string[] split = entity.Split(" ");
            switch ((split[0]).ToLower())
            {
                case "segment":
                    return "per hour";
                case "tank":
                    switch (entityType.ToLower())
                    {
                        case "leak":
                            return "reduce wastage";
                        case "sufficient":
                            return "reduce wastage";
                        case "empty":
                            return "";
                    }
                    break;
                default:
                    return "";
            }
            return "";
        }

        private string GetRelevantLossDescriptionLine1(string entity, string entityType)
        {
            string[] split = entity.Split(" ");
            switch ((split[0]).ToLower())
            {
                case "segment":
                    switch (entityType.ToLower())
                    {
                        case "resolved":
                            return "was being lost";
                        default:
                            return "is currently being lost";
                    }
                case "tank":
                    switch (entityType.ToLower())
                    {
                        case "leak":
                            return "fix the leak to";
                        case "sufficient":
                            return "switch off the pump to";
                        case "empty":
                            return "switch on the pump to";
                    }
                    break;
                default:
                    return "";
            }
            return "";
        }

        private string GetRelevantLossPH(string entity, string entityType, string entityLitres, string entityRand)
        {
            string[] split = entity.Split(" ");
            if(split[0].ToLower() == "segment" && entityType.ToLower() == "leak")
            {
                return Math.Round(Double.Parse(entityLitres),1) + "L or R " + Math.Round(Double.Parse(entityRand),1);
            }
            else if(entityType.ToLower() == "resolved")
            {
                return "R " + Math.Round(Double.Parse(entityRand), 1);
            }
            else
            {
                return "";
            }
        }

        private string GetRelevantDescription(string entity, string entityType)
        {
            string[] split = entity.Split(" ");
            switch ((split[0]).ToLower())
            {
                case "segment":
                    return "Has been lost!";
                case "tank":
                    if(entityType.ToLower() != "empty")
                    {
                        return "Will be lost per hour";
                    }
                    break;
                case "sensor":
                    //TODO: Return needed text
                    break;
                default:
                    return "";
            }
            return "";
        }

        //Displays the element based on what entity it is
        private string GetRelevantRand(string entity, string entityType, string randValue, string randPH)
        {
            //Get string before " "
            string[] split = entity.Split(" ");
            if( (split[0]).ToLower() == "sensor" || (split[0]).ToLower() == "tank" && entityType.ToLower() == "empty")
            {
                return "";
            }
            else
            {
                if ((split[0]).ToLower() == "tank")
                {
                    double randPh = Double.Parse(randPH);
                    return "R " + Math.Max(Math.Round(randPh,1),0.1);
                }
                else
                {
                    double rand = Double.Parse(randValue);
                    return "R " + Math.Max(Math.Round(rand,1),0.1);
                }
            }
        }

        //Receive minutes and convert into relevant
        private string GetRelevantUnit(string v)
        {
            double value = Double.Parse(v);
            if(value < 60.00)
            {
                //Can continue in minutes
                return (Math.Round(value) + " minutes");
            }
            else if(value < 1440)
            {
                //Can continue in hours
                return (Math.Round((value / 60.00)) + " hours");
            }
            else
            {
                //Can continue in days
                return (Math.Round((value / 1440.00)) + " days");
            }
        }
    }
}
