using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class DatabaseContext : DbContext

    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {

        }

        public DatabaseContext() { }
        public DbSet<LocationsEntry> Locations { get; set; }
        public DbSet<LocationSegmentsEntry> LocationSegments { get; set; }
        public DbSet<MonitorsEntry> Monitors { get; set; }
        public DbSet<ReadingsEntry> Readings { get; set; }
        public DbSet<SegmentsEntry> Segments { get; set; }
        public DbSet<SegmentEventsEntry> SegmentEvents { get; set; }
        public virtual DbSet<SegmentLeaksEntry> SegmentLeaks { get; set; } 
        public DbSet<PumpEntry> Pumps { get; set; }
        public virtual DbSet<TankMonitorsEntry> TankMonitors { get; set; }
        public virtual DbSet<TankReadingsEntry> TankReadings { get; set; }
        public DbSet<MailingListEntry> MailingList { get; set; }
        public DbSet<HistoryLogEntry> HistoryLogs { get; set; }
        public DbSet<SensorHistoryEntry> SensorHistory { get; set; }

    }
}
