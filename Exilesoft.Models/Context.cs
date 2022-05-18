using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{
    public class Context : DbContext
    {
        public Context(string connectionString)
            :base(connectionString)
        {
        }
        public DbSet<BackgroundMasters> BackgroundMasters { get; set; }
        public DbSet<BackgroundSlaves> BackgroundSlaves { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<PendingAttendance> PendingAttendances { get; set; }
        public DbSet<WorkingFromHomeTask> WorkingFromHomeTasks { get; set; }
        
        public DbSet<EmployeeExternal> EmployeesExternal { get; set; }
        public DbSet<EmployeeEnrollment> EmployeeEnrollment { get; set; }
        public DbSet<EmployeeOnSite> EmployeesOnSite { get; set; }
        public DbSet<Location> Locations { get; set; }        
        public DbSet<VisitorAttendance> VisitorAttendances { get; set; }
        public DbSet<UserTeam> UserTeams { get; set; }

        public DbSet<SyncSessionLog> SyncSessionLogs { get; set; }
        public DbSet<SyncMachineLog> SyncMachineLogs { get; set; }
        public DbSet<SyncLogEntry> SyncLogEntries { get; set; }
        public DbSet<SyncServiceStatusInfo> SyncServiceStatusInfo { get; set; }
        
       
        public DbSet<VisitorPassAllocation> VisitorPassAllocations { get; set; }        
        public DbSet<SpecialEvent> SpecialEvents { get; set; }
        public DbSet<Privilege> Privileges { get; set; }
        public DbSet<CardCategory> CardCategories { get; set; }
        public DbSet<CardAccessLevel> CardAccessLevels { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<VisitInformation> VisitInformation { get; set; }
        public DbSet<TimeRecord> TimeRecords { get; set; }
        public DbSet<EmployeeMissingEntry> EmployeeMissingEntries { get; set; }
        public DbSet<WorkingFromHomeTaskTemplate> WorkingFromHomeTaskTemplates{ get; set; }

        public Context()
        {
            this.Configuration.LazyLoadingEnabled = true;           
			var objectContext = (this as IObjectContextAdapter).ObjectContext;
			objectContext.CommandTimeout = 180;
        }
    }
}