using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Exilesoft.Models;

namespace Exilesoft.TimeManager
{
    public class CloudDeployment
    {
        public TextBox ColudSyncLogTextBox { get; set; }
        public TextBox LocalTimeRecordLogTextBox { get; set; }
        private SyncSessionLog currentSyncSessionLog;

        private static DateTime? _lastDeviceClearedDate;

        /*
         * This method reads time records from card reader machine's and add time records to local databse
         * Tables updated by this method : TimeRecords, SyncMachineLog, SyncSessionLog
         * SyncMachineLog, SyncSessionLog : this tables are updated only when there is an error in connecting to card reader diveces.
         * so that this log entries will be used to sync to cloud database about the failures
         */
        public void SyncLocalTimeRecords(ITimeRecordsReader timeRecordsReader)
        {
            bool deleteLogsForToday = (_lastDeviceClearedDate == null || _lastDeviceClearedDate.Value.Date != DateTime.Now.Date);
            var connections = ConfigurationManager.ConnectionStrings;
            var connectionString = connections["Context"].ConnectionString;
            using (var dbContext = new LocalContext(connectionString))
            {
                // Need to avoid on site locations with for the sync
                var locationList = dbContext.Locations.Where(a => a.OnSiteLocation == false).ToList();

                // Main audit log for the sysnchronizing session.
                var syncSessionLog = new SyncSessionLog { StartAt = DateTime.Now, IsSynced = 1 };
                currentSyncSessionLog = syncSessionLog;

                var syncMachineLogList = new List<SyncMachineLog>();

                #region --- Update service status info ---

                if (UpdateServiceStatusStart(dbContext, syncSessionLog,
                    string.Format("Synchronization form time devices to local db started at : {0}", DateTime.Now), LocalTimeRecordLogTextBox)) return;


                #endregion

                // Sysnchronization on each floor
                foreach (var location in locationList)
                {
                    var syncMachineLog = new SyncMachineLog { StartAt = DateTime.Now };
                    syncMachineLog.Description = string.Format("{0} Sysnchronization from FingerTec to MyTime at {1}",
                        location.Floor, syncMachineLog.StartAt.ToString(CultureInfo.InvariantCulture));
                    syncMachineLog.LocationID = location.Id;
                    syncMachineLog.Status = SyncStatus.Successful.ToString();
                    syncMachineLog.SyncSessionLog = syncSessionLog;

                    #region --- Update service status info ---

                    UpdateServiceStatusAboutLocation(dbContext, location, syncMachineLog);

                    #endregion

                    // Connecting and veryfing the connection to the device
                    // Connect_TCPIP Returns 0 if the connection is successful
                    if (timeRecordsReader.Connect_TCPIP(location.DeviceName, location.DeviceNo,
                        location.DeviceIP, location.Port, location.CommKey) == 0)
                    {
                        bool hasNewRecords = false;

                        #region --- Local Variable Decleration ---

                        var enrollNumber = new int();
                        int logSize = 0, year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0, verifyMode = 0, inOutMode = 0, workCode = 0;

                        int successfullCount = 0;
                        int unsuccessfullCount = 0;

                        #endregion

                        // Gets all transaction logs from device memory
                        // Returns 0 for success or -1 for fail.
                        if (timeRecordsReader.ReadGeneralLog(ref logSize) == 0)
                        {
                            // Gets a single transaction log from device memory
                            // Returns 0 for success or -1 for fail.
                            while (timeRecordsReader.GetGeneralLog(ref enrollNumber, ref year, ref month, ref day,
                                ref hour, ref minute,
                                ref second, ref verifyMode, ref inOutMode, ref workCode) == 0)
                            {
                                if (!dbContext.TimeRecords.Any(a => a.CardNo == enrollNumber &&
                                                                    a.Year == year && a.Month == month && a.Day == day &&
                                                                    a.Hour == hour && a.Minute == minute &&
                                                                    a.Second == second &&
                                                                    a.VerifyMode == verifyMode &&
                                                                    a.InOutMode == inOutMode && a.WorkCode == workCode))
                                {
                                    hasNewRecords = true;

                                    var timeRecord = new TimeRecord
                                    {
                                        CardNo = enrollNumber,
                                        Year = year,
                                        Month = month,
                                        Day = day,
                                        Hour = hour,
                                        Minute = minute,
                                        Second = second,
                                        VerifyMode = verifyMode,
                                        InOutMode = inOutMode,
                                        WorkCode = workCode,
                                        LocationId = location.Id,
                                        IsSynced = 0
                                    };

                                    dbContext.TimeRecords.Add(timeRecord);

                                    try
                                    {
                                        dbContext.SaveChanges();
                                        successfullCount++;
                                    }
                                    catch (Exception ex)
                                    {
                                        unsuccessfullCount++;
                                        LogException(ex, "Error occurred when creating the attendance entry",
                                            EventLogEntryType.Error);
                                    }
                                }

                                syncMachineLog.NumberOfEmployeeRecords++;
                            }
                        }

                        #region --- Updating Machine Log Status  ---

                        syncMachineLog.CompletedAt = DateTime.Now;
                        if (unsuccessfullCount > 0)
                        {
                            if (successfullCount > 0)
                            {
                                syncMachineLog.Status = SyncStatus.PartiallyCompleted.ToString();
                                syncMachineLog.Notes = string.Format("{0} Records synchronized successfully. {1} Records Failed.", successfullCount, unsuccessfullCount);
                            }
                            else
                            {
                                syncMachineLog.Status = SyncStatus.Failed.ToString();
                                syncMachineLog.Notes = "All records filed to synchronize.";
                            }
                        }
                        else
                        {
                            syncMachineLog.Notes = "All records synchronized successfully.";
                        }

                        #endregion

                        #region --- Clear the machine logs ---
                        ClearMachineLogs(timeRecordsReader, location, hasNewRecords, deleteLogsForToday, syncMachineLog);

                        #endregion

                        syncMachineLog.IsSynced = 1;
                    }
                    else
                    {
                        // If the connection was not successful with the device ?
                        // Write the event to the event log and continue with the next device....
                        string errorMessage = string.Format(@"Could not connect to device on {0} at {1} (IP Address :{2},Device Name:{3}, Device  Number:{4}, Port: {5}, Comm Key:{6})",
                        location.Floor, syncMachineLog.StartAt, location.DeviceIP, location.DeviceName, location.DeviceNumber, location.Port, location.CommKey);

                        syncMachineLog.Status = SyncStatus.Failed.ToString();
                        syncMachineLog.Notes = errorMessage;
                        syncMachineLog.CompletedAt = DateTime.Now;

                        syncMachineLog.IsSynced = 0;
                        syncSessionLog.IsSynced = 0;

                        Logger.Log(errorMessage, EventLogEntryType.Error);
                    }
                    syncMachineLogList.Add(syncMachineLog);
                }

                #region --- Updating Sync Session Log Status  ---

                UpdateSyncSessionLogStatus(syncSessionLog, syncMachineLogList);

                #endregion

                #region --- Saving Finalized Log Entries ---

                dbContext.SyncSessionLogs.Add(syncSessionLog);

                foreach (var machineLog in syncMachineLogList)
                    dbContext.SyncMachineLogs.Add(machineLog);

                try
                {
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    LogException(ex, "Error occurred when saving the sync entries", EventLogEntryType.Error);
                }

                #endregion

                #region --- Update service status info ---

                UpdateServiceStatusFinished(dbContext, syncSessionLog, string.Format("Synchronization form time devices to local db completed at : {0}", DateTime.Now), LocalTimeRecordLogTextBox);

                #endregion
            }
        }

        private static void UpdateSyncSessionLogStatus(SyncSessionLog syncSessionLog, List<SyncMachineLog> syncMachineLogList)
        {
            syncSessionLog.CompletedAt = DateTime.Now;
            syncSessionLog.Status = SyncStatus.Successful.ToString();
            syncSessionLog.Description = string.Format("Synchronization of MyTime With FingerTec at {0}", syncSessionLog.StartAt);
            if (syncMachineLogList.Any(a => a.Status == SyncStatus.Failed.ToString()))
            {
                if (syncMachineLogList.Any(a => a.Status == SyncStatus.Successful.ToString()))
                {
                    int successfullCount = syncMachineLogList.Count(a => a.Status == SyncStatus.Successful.ToString());
                    int unsuccessfullCount = syncMachineLogList.Count(a => a.Status == SyncStatus.Failed.ToString());

                    syncSessionLog.Status = SyncStatus.PartiallyCompleted.ToString();
                    syncSessionLog.Notes = string.Format("{0} Records synchronized successfully. {1} Records Failed.",
                        successfullCount, unsuccessfullCount);
                }
                else
                {
                    syncSessionLog.Status = SyncStatus.Failed.ToString();
                    syncSessionLog.Notes = "All records filed to synchronize.";
                }
            }
            else
            {
                syncSessionLog.Notes = "All records synchronized successfully.";
            }
        }

        private void UpdateServiceStatusFinished(LocalContext dbContext, SyncSessionLog syncSessionLog, string message,
            TextBox textBox)
        {
            SyncServiceStatusInfo syncStatusInfo = dbContext.SyncServiceStatusInfo.FirstOrDefault();
            int syncInterval =
                int.Parse(ConfigurationManager.AppSettings["SynchronizationInterval"].ToString(CultureInfo.InvariantCulture));

            if (syncStatusInfo != null)
            {
                syncStatusInfo.SynchronizationStatus = SyncProcessStatus.Idle.ToString();
                syncStatusInfo.LastServiceStoppedAt = syncSessionLog.CompletedAt;
                syncStatusInfo.NextServiceScheduledAt = DateTime.Now.AddMinutes(syncInterval);
                syncStatusInfo.CurrentSynchronizingMessage = string.Format("Synchronization completed on all floors at: {0}",
                    syncSessionLog.CompletedAt);
                try
                {
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    LogException(ex, "Error occurred when updating service status info", EventLogEntryType.Error);
                }
            }
            AppendToTextBox(message, textBox);
        }

        private void UpdateServiceStatusAboutLocation(LocalContext dbContext, Location location, SyncMachineLog syncMachineLog)
        {
            SyncServiceStatusInfo syncStatusInfo = dbContext.SyncServiceStatusInfo.FirstOrDefault();
            if (syncStatusInfo != null)
            {
                syncStatusInfo.SynchronizationStatus = SyncProcessStatus.Synchronizing.ToString();
                syncStatusInfo.CurrentSynchronizingLocationID = location.Id;
                syncStatusInfo.CurrentSynchronizingMessage = string.Format("Synchronizing location: {0}", location.Floor);

                try
                {
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    LogException(ex, "Error occurred when updating service status info", EventLogEntryType.Error);
                    syncMachineLog.Status = SyncStatus.Failed.ToString();
                }
            }
        }

        private bool UpdateServiceStatusStart(LocalContext dbContext, SyncSessionLog syncSessionLog, string message,
            TextBox textBox)
        {
            SyncServiceStatusInfo syncStatusInfo = dbContext.SyncServiceStatusInfo.FirstOrDefault();
            if (syncStatusInfo != null)
            {
                syncStatusInfo.SynchronizationStatus = SyncProcessStatus.Synchronizing.ToString();
                syncStatusInfo.LastServiceStartedAt = syncSessionLog.StartAt;
                syncStatusInfo.CurrentSynchronizingMessage = message;

                try
                {
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    LogException(ex, "Error occurred when updating service status info", EventLogEntryType.Error);
                    return true;
                }
            }
            AppendToTextBox(message, textBox);
            return false;
        }

        /*
         * Card reader machine records are deleted once a day if this service is running continously.
         * 
         */
        private static void ClearMachineLogs(ITimeRecordsReader timeRecordsReader, Location location, bool hasNewRecords,
            bool deleteLogsForToday, SyncMachineLog syncMachineLog)
        {
            //1 -default, delete card reader machine logs, 0- don't delete
            int deleteMachineLogs = int.Parse(ConfigurationManager.AppSettings["DeleteMachineLogs"]);
            //If the location is set as to clear logs automatically from the database
            if (location.ClearLogsAutomatically && deleteMachineLogs == 1)
            {
                if (!hasNewRecords && deleteLogsForToday)
                {
                    _lastDeviceClearedDate = DateTime.Now.Date;
                    Logger.Log(
                        string.Format("Clear logs on location :{0} started at:{1}.", location.Floor, DateTime.Now),
                        EventLogEntryType.SuccessAudit);

                    //If all the logs are completed successfuly then cleat the logs on device
                    if (syncMachineLog.Status.Equals(SyncStatus.Successful.ToString()))
                    {
                        // Returns 0 for success or -1 for fail.
                        if (timeRecordsReader.DeleteGeneralLog() == 0)
                        {
                            string clearMessage = string.Format("All logs clear on {0} at : {1}", location.Floor,
                                DateTime.Now);
                            syncMachineLog.Notes += clearMessage;
                            Logger.Log(clearMessage, EventLogEntryType.SuccessAudit);
                        }
                        else
                        {
                            string clearMessage = string.Format("Clear logs failed on {0} at : {1}", location.Floor,
                                DateTime.Now);
                            syncMachineLog.Notes += clearMessage;
                            Logger.Log(clearMessage, EventLogEntryType.Error);
                        }
                    }
                }
            }
        }

        public void CreateCloudAttendanceRecords()
        {
            var connections = ConfigurationManager.ConnectionStrings;
            var connectionString = connections["CloudContext"].ConnectionString;

            using (var dbContext = new Context(connectionString))
            {
                var locationList = dbContext.Locations.Where(a => a.OnSiteLocation == false).ToList();
                var employeeEnrollmentList = dbContext.EmployeeEnrollment.ToList();
                var visitorPassAllocations = dbContext.VisitorPassAllocations.Where(a => a.IsActive);

                #region --- Update service status info ---
                AppendToTextBox(string.Format("Synchronization to cloud started at : {0}", DateTime.Now), ColudSyncLogTextBox);
                #endregion

                List<TimeRecord> localTimeRecords;
                connections = ConfigurationManager.ConnectionStrings;
                connectionString = connections["Context"].ConnectionString;
                using (var dbContextLocal = new LocalContext(connectionString))
                {
                    dbContextLocal.Configuration.AutoDetectChangesEnabled = false;
                    localTimeRecords = dbContextLocal.TimeRecords.Where(a => a.IsSynced == 0).ToList();

                }

                var recordedLocationList = (from r in localTimeRecords
                                            group r by new { r.LocationId }
                                                into resultSet
                                                orderby resultSet.Key.LocationId
                                                select new
                                                {
                                                    resultSet.Key.LocationId
                                                });

                foreach (var recordedLocation in recordedLocationList)
                {

                    var notSyncedTimeRecordIds = new Dictionary<int, int>();
                    var recordedLocationCopy = recordedLocation;
                    var localTimeRecordsToUpdate = localTimeRecords.Where(a => a.LocationId == recordedLocationCopy.LocationId);

                    Location location = locationList.FirstOrDefault(a => a.Id == recordedLocation.LocationId);
                    if (location == null) continue;


                    int recordCount = 0;
                    foreach (TimeRecord localTimeRecord in localTimeRecordsToUpdate)
                    {
                        recordCount++;
                        //first set to errornus status. if the record processed successfully, correct status will be set in related places
                        localTimeRecord.IsSynced = 2;

                        int enrollNumber = localTimeRecord.CardNo;
                        int year = localTimeRecord.Year;
                        int month = localTimeRecord.Month;
                        int day = localTimeRecord.Day;
                        int hour = localTimeRecord.Hour;
                        int minute = localTimeRecord.Minute;
                        int second = localTimeRecord.Second;
                        int verifyMode = localTimeRecord.VerifyMode;
                        int inOutMode = localTimeRecord.InOutMode;
                        int workCode = localTimeRecord.WorkCode;

                        EmployeeEnrollment employeeEnrollment =
                            employeeEnrollmentList.FirstOrDefault(a => a.CardNo == enrollNumber);

                        string mode = inOutMode == 0 ? "in" : "out";

                        // If the employee is available for the enrollment number ?
                        // Consider the log as an attendance entry. If Not ?
                        // Consider all as visitors, Includes all employees from ProAccount as well
                        if (employeeEnrollment != null)
                        {
                            EmployeeEnrollment employee =
                                dbContext.EmployeeEnrollment.Single(e => e.EmployeeId == employeeEnrollment.EmployeeId);
                            if (!dbContext.Attendances.Any(a => a.CardNo == enrollNumber &&
                                                                a.Year == year && a.Month == month && a.Day == day &&
                                                                a.Hour == hour && a.Minute == minute &&
                                                                a.Second == second &&
                                                                a.VerifyMode == verifyMode && a.InOutMode == mode &&
                                                                a.WorkCode == workCode))
                            {
                                #region --- Updating Attendance For Employees ---

                                var attendance = new Attendance();
                                var syncLogEntry = new SyncLogEntry();

                                attendance.CardNo = enrollNumber;
                                attendance.Year = year;
                                attendance.Month = month;
                                attendance.Day = day;
                                attendance.Hour = hour;
                                attendance.Minute = minute;
                                attendance.Second = second;
                                attendance.VerifyMode = verifyMode;
                                attendance.InOutMode = mode;
                                attendance.WorkCode = workCode;
                                attendance.Employee = employee;
                                attendance.Location = location;
                                dbContext.Attendances.Add(attendance);


                                #region --- Check with visitor pass allocation ---

                                DateTime? entryDate = TryParseDate(year, month, day, hour, minute, second);
                                if (entryDate != null)
                                {
                                    DateTime entryDateValue = entryDate.Value.Date;

                                    foreach (var visitorAllocation in visitorPassAllocations)
                                    {
                                        if (visitorAllocation.AssignDate <= entryDateValue && visitorAllocation.IsActive
                                            && visitorAllocation.EmployeeId == employee.EmployeeId)
                                        {
                                            visitorAllocation.IsActive = false;
                                            visitorAllocation.Notes =
                                                string.Format(
                                                    "Visitor pass allocation deactivated with the use of enrollment number: {0} at :{1}",
                                                    enrollNumber,
                                                    entryDate.Value);
                                        }
                                    }
                                }

                                #endregion

                                try
                                {
                                    //dbContext.SaveChanges();
                                    syncLogEntry.Status = SyncStatus.Successful.ToString();
                                }
                                catch (Exception ex)
                                {
                                    LogException(ex, "Error occurred when creating the attendance entry",
                                    EventLogEntryType.Error);
                                }
                                localTimeRecord.IsSynced = 1;

                                #endregion
                            }
                        }
                        else
                        {
                            DateTime? entryDate = TryParseDate(year, month, day, hour, minute, second);
                            var entryDateValue = new DateTime();
                            if (entryDate != null)
                                entryDateValue = entryDate.Value.Date;

                            if (!dbContext.VisitorAttendances.Any(a => a.CardNo == enrollNumber &&
                                                                       a.DateTime == entryDate &&
                                                                       a.VerifyMode == verifyMode &&
                                                                       a.InOutMode == mode && a.WorkCode == workCode))
                            {
                                #region --- Updating Attendance For Visitor And External Employees

                                var visitorAttendance = new VisitorAttendance();
                                var syncLogEntry = new SyncLogEntry();
                                if (entryDate != null)
                                {
                                    visitorAttendance.CardNo = enrollNumber;
                                    visitorAttendance.DateTime = entryDate.Value;
                                    visitorAttendance.VerifyMode = verifyMode;
                                    visitorAttendance.InOutMode = mode;
                                    visitorAttendance.WorkCode = workCode;
                                    visitorAttendance.isTransferred = false;
                                    visitorAttendance.Location = location;


                                    #region --- Check with visitor pass allocation ---

                                    var visitorPassAllocation =
                                        visitorPassAllocations.FirstOrDefault(a => a.CardNo == enrollNumber
                                                                                   && a.AssignDate == entryDateValue &&
                                                                                   a.IsActive);
                                    if (visitorPassAllocation != null)
                                    {
                                        if (visitorPassAllocation.EmployeeEnrollment != null)
                                        {
                                            EmployeeEnrollment employee =
                                                dbContext.EmployeeEnrollment.Single(
                                                    e =>
                                                        e.EmployeeId ==
                                                        visitorPassAllocation.EmployeeEnrollment.EmployeeId);

                                            var attendance = new Attendance
                                            {
                                                CardNo = enrollNumber,
                                                Year = year,
                                                Month = month,
                                                Day = day,
                                                Hour = hour,
                                                Minute = minute,
                                                Second = second,
                                                VerifyMode = verifyMode,
                                                InOutMode = mode,
                                                WorkCode = workCode,
                                                Employee = employee,
                                                Location = location
                                            };
                                            dbContext.Attendances.Add(attendance);

                                            visitorAttendance.isTransferred = true;
                                            visitorAttendance.TransferredDate = DateTime.Today;
                                            visitorAttendance.Note =
                                                string.Format("Transferred to employee : {0} by {1} at {2}",
                                                    visitorPassAllocation.EmployeeEnrollment.CardNo,
                                                    "Sync Service", DateTime.Now);
                                        }
                                        visitorAttendance.VisitorPassAllocationId = visitorPassAllocation.Id;
                                        visitorAttendance.VisitInformationId = visitorPassAllocation.VisitInformationId;
                                        visitorAttendance.EmployeeId = visitorPassAllocation.EmployeeId;
                                    }

                                    #endregion

                                    dbContext.VisitorAttendances.Add(visitorAttendance);

                                    try
                                    {
                                        //dbContext.SaveChanges();
                                        syncLogEntry.Status = SyncStatus.Successful.ToString();
                                    }
                                    catch (Exception ex)
                                    {
                                        LogException(ex, "Error occurred when creating the visitor attendance entry",
                                        EventLogEntryType.Error);
                                    }


                                    localTimeRecord.IsSynced = 1;
                                }

                                #endregion
                            }
                        }


                        dbContext.TimeRecords.Add(localTimeRecord);
                        notSyncedTimeRecordIds.Add(localTimeRecord.Id, localTimeRecord.IsSynced);

                        try
                        {
                            if (recordCount % 100 == 0)
                            {
                                dbContext.SaveChanges();

                                using (var dbContextLocal = new Context())
                                {
                                    foreach (var timeRecordId in notSyncedTimeRecordIds)
                                    {
                                        TimeRecord locaTimeRecord = dbContextLocal.TimeRecords.FirstOrDefault(a => a.Id == timeRecordId.Key);
                                        if (locaTimeRecord != null)
                                            locaTimeRecord.IsSynced = timeRecordId.Value;
                                    }
                                    try
                                    {
                                        dbContextLocal.SaveChanges();
                                    }
                                    catch (Exception ex)
                                    {
                                        LogException(ex, "Error occurred when sync failed machine log ", EventLogEntryType.Error);
                                    }
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            LogException(ex, "Error occurred when adding records to cloud DB",
                                EventLogEntryType.Error);
                        }
                    }
                    try
                    {
                        dbContext.SaveChanges();


                        using (var dbContextLocal = new Context())
                        {
                            foreach (var timeRecordId in notSyncedTimeRecordIds)
                            {
                                TimeRecord locaTimeRecord = dbContextLocal.TimeRecords.FirstOrDefault(a => a.Id == timeRecordId.Key);
                                if (locaTimeRecord != null)
                                    locaTimeRecord.IsSynced = timeRecordId.Value;
                            }
                            try
                            {
                                dbContextLocal.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                LogException(ex, "Error occurred when sync failed machine log ", EventLogEntryType.Error);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        LogException(ex, "Error occurred when adding records to cloud DB",
                            EventLogEntryType.Error);
                    }

                }

                #region --- Update service status info ---
                AppendToTextBox(string.Format("Synchronization to cloud completed at : {0}", DateTime.Now), ColudSyncLogTextBox);

                #endregion

            }
        }

        private void LogException(Exception ex, string prefix, EventLogEntryType eventType)
        {
            var exceptionMessage = ex.Message;
            if (ex.InnerException != null)
            {
                exceptionMessage = ex.InnerException.Message;
                if (ex.InnerException.InnerException != null)
                    exceptionMessage = ex.InnerException.InnerException.Message;
            }

            Logger.Log(string.Format("{0} : {1}", prefix, exceptionMessage), eventType);
            AppendToTextBox(string.Format("{0} : {1}", prefix, exceptionMessage), LocalTimeRecordLogTextBox);
        }

        private void AppendToTextBox(string value, TextBox messageBox)
        {
            if (messageBox.InvokeRequired)
            {
                messageBox.Invoke(new Action<string, TextBox>(AppendToTextBox), new object[] { value, messageBox });
                return;
            }
            messageBox.Text += Environment.NewLine + value;
        }

        private DateTime? TryParseDate(int year, int month, int date, int hour, int minute, int second)
        {
            DateTime? convertedDate;
            try
            {
                convertedDate = new DateTime(year, month, date, hour, minute, second);
            }
            catch (Exception)
            {
                convertedDate = null;
            }
            return convertedDate;
        }
    }
}
