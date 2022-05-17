using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{
    public class LocalContext : DbContext
    {
        public LocalContext(string connectionString)
            : base(connectionString)
        {
        }        
        public DbSet<Location> Locations { get; set; }   
        public DbSet<SyncSessionLog> SyncSessionLogs { get; set; }
        public DbSet<SyncMachineLog> SyncMachineLogs { get; set; }       
        public DbSet<SyncServiceStatusInfo> SyncServiceStatusInfo { get; set; }       
        public DbSet<TimeRecord> TimeRecords { get; set; }

        public LocalContext()
        {
            this.Configuration.LazyLoadingEnabled = true;
            var objectContext = (this as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 180;
        }
    }
}