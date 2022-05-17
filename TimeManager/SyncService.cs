using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Configuration;
using System.Diagnostics;
using Exilesoft.Models;

namespace Exilesoft.TimeManager
{
    public partial class SyncService : Form
    {
        private static DateTime? lastDeviceClearedDate = null;
        private ITimeRecordsReader _timeRecordsReader;
        private CloudDeployment _cloudDeployment;

        public SyncService()
        {
            InitializeComponent();
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            if (!System.Diagnostics.EventLog.SourceExists("MyTimeSource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "MyTimeSource", "MyTimeEventLog");
            }   
            myTimeEventLog.Source = "MyTimeSource";
            myTimeEventLog.Log = "MyTimeEventLog";
            _timeRecordsReader = new TimeRecordsReaderFactory().GetTimeRecordsReader();
            _timeRecordsReader.Initialize(axBioBridgeSDK1);
            Logger.MyTimeEventLog = myTimeEventLog;
            _cloudDeployment = new CloudDeployment
            {
                ColudSyncLogTextBox = txtCloudSyncLog,
                LocalTimeRecordLogTextBox = textBoxLog
            };
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            UpdateStopSyncStatusInfor();
            Logger.Log(string.Format("MyTime synchronization service stoped at : {0}", System.DateTime.Now),
                EventLogEntryType.Information);

            KillExistingProcess();
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
            AppendTextBox(string.Format("{0} : {1}", prefix, exceptionMessage));
        }

        private void SyncNow()
        {
            Logger.Log(string.Format("Synchronization with FingerTex started at : {0}", System.DateTime.Now),
                EventLogEntryType.SuccessAudit);
            AppendTextBox(string.Format("Synchronization with FingerTex started at : {0}", System.DateTime.Now));

            // Create or update the service status on the service.
            CreateAndStartSyncStatusInfo();
            try
            {
                // Do processing on sysncronization
                //LocalServerDeployment();
                _cloudDeployment.SyncLocalTimeRecords(_timeRecordsReader);
            }
            catch (Exception ex)
            {
                LogException(ex, "Error occurred on sync, time records to local db", EventLogEntryType.Error);
            }
            try
            {
                _cloudDeployment.CreateCloudAttendanceRecords();
            }
            catch (Exception ex)
            {
                LogException(ex, "Error occurred on sync, time records to cloud db", EventLogEntryType.Error);
            }

            Logger.Log(string.Format("Synchronization with FingerTex completed at : {0}", System.DateTime.Now),
                EventLogEntryType.SuccessAudit);
            AppendTextBox(string.Format("Synchronization with FingerTex completed at : {0}", System.DateTime.Now));
        }

        private void LocalServerDeployment()
        {
            var dbContext = new Context();
            SyncServiceStatusInfo syncStatusInfo = null;
            bool deleteLogsForToday = (lastDeviceClearedDate == null || lastDeviceClearedDate.Value.Date != DateTime.Now.Date);

            // Main audit log for the sysnchronizing session.
            var syncSessionLog = new SyncSessionLog();
            syncSessionLog.StartAt = System.DateTime.Now;

            // Need to avoid on site locations with for the sync
            var locationList = dbContext.Locations.Where(a => a.OnSiteLocation == false).ToList();
            var employeeEnrollmentList = dbContext.EmployeeEnrollment.ToList();
            var externalEmployeeList = dbContext.EmployeesExternal.ToList();
            var visitorPassAllocations = dbContext.VisitorPassAllocations.Where(a => a.IsActive);

            #region --- Update service status info ---

            syncStatusInfo = dbContext.SyncServiceStatusInfo.FirstOrDefault();
            if (syncStatusInfo != null)
            {
                syncStatusInfo.SynchronizationStatus = SyncProcessStatus.Synchronizing.ToString();
                syncStatusInfo.LastServiceStartedAt = syncSessionLog.StartAt;
                syncStatusInfo.CurrentSynchronizingMessage = "Synchronization started. Waiting for response from physical machine.";
                AppendTextBox("Synchronization started. Waiting for response from physical machine.");

                try
                {
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    LogException(ex, "Error occurred when updating service status info", EventLogEntryType.Error);
                    return;
                }
            }

            #endregion

            List<SyncMachineLog> syncMachineLogList = new List<SyncMachineLog>();
            List<SyncLogEntry> syncLogEntryList = new List<SyncLogEntry>();

            DateTime? lastSyncLogStartTime = null;
            TimeSpan? lastLogUpdatedTimeSpan = null;

            // Sysnchronization on each floor
            foreach (var location in locationList)
            {
                var syncMachineLog = new SyncMachineLog();
                syncMachineLog.StartAt = System.DateTime.Now;
                syncMachineLog.Description = string.Format("{0} Sysnchronization from FingerTec to MyTime at {1}",
                    location.Floor, syncMachineLog.StartAt.ToString());
                syncMachineLog.LocationID = location.Id;
                syncMachineLog.Status = SyncStatus.Successful.ToString();
                syncMachineLog.SyncSessionLog = syncSessionLog;

                #region --- Update service status info ---

                syncStatusInfo = dbContext.SyncServiceStatusInfo.FirstOrDefault();
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

                #endregion

                // Connecting and veryfing the connection to the device
                // Connect_TCPIP Returns 0 if the connection is successful
                if (_timeRecordsReader.Connect_TCPIP(location.DeviceName, location.DeviceNo,
                    location.DeviceIP, location.Port, location.CommKey) == 0)
                {
                    #region --- Local Variable Decleration ---

                    int enrollNumber = new int();
                    int logSize = 0;
                    int year = 0;
                    int month = 0;
                    int day = 0;
                    int hour = 0;
                    int minute = 0;
                    int second = 0;
                    int verifyMode = 0;
                    int inOutMode = 0;
                    int workCode = 0;

                    #endregion

                    // Gets all transaction logs from device memory
                    // Returns 0 for success or -1 for fail.
                    if (_timeRecordsReader.ReadGeneralLog(ref logSize) == 0)
                    {
                        // Gets a single transaction log from device memory
                        // Returns 0 for success or -1 for fail.
                        while (_timeRecordsReader.GetGeneralLog(ref enrollNumber, ref year, ref month, ref day, ref hour, ref minute,
                        ref second, ref verifyMode, ref inOutMode, ref workCode) == 0)
                        {
                            lastSyncLogStartTime = DateTime.Now;
                            EmployeeEnrollment employeeEnrollment = employeeEnrollmentList.FirstOrDefault(a => a.CardNo == enrollNumber);
                            EmployeeExternal externalEmployee = null;
                            if (employeeEnrollment == null)
                                externalEmployee = externalEmployeeList.FirstOrDefault(a => a.CardNo == enrollNumber);

                            string mode = inOutMode == 0 ? "in" : "out";

                            // If the employee is available for the enrollment number ?
                            // Consider the log as an attendance entry. If Not ?
                            // Consider all as visitors, Includes all employees from ProAccount as well
                            if (employeeEnrollment != null)
                            {
                                EmployeeEnrollment employee = dbContext.EmployeeEnrollment.Single(e => e.EmployeeId == employeeEnrollment.EmployeeId);
                                if (!dbContext.Attendances.Any(a => a.CardNo == enrollNumber &&
                                a.Year == year && a.Month == month && a.Day == day &&
                                a.Hour == hour && a.Minute == minute && a.Second == second &&
                                a.VerifyMode == verifyMode && a.InOutMode == mode && a.WorkCode == workCode))
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

                                    string _logDescription = "Sync record Employee user name:{0} Enroll Number:{1}, DateTime:{2}/{3}/{4} {5}:{6}:{7}, Mode:{8}";
                                    syncLogEntry.Description = string.Format(_logDescription, employee.UserName, enrollNumber,
                                        month, day, year, hour, minute, second, mode);
                                    syncLogEntry.EmployeeId = employeeEnrollment.EmployeeId;
                                    syncLogEntry.CardNo = enrollNumber.ToString();
                                    syncLogEntry.LocationId = location.Id;
                                    syncLogEntry.Status = SyncStatus.Successful.ToString();
                                    syncLogEntry.SyncSessionLog = syncSessionLog;
                                    syncLogEntry.SyncTime = System.DateTime.Now;

                                    #region --- Check with visitor pass allocation ---

                                    DateTime? entryDate = TryParseDate(year, month, day, hour, minute, second);
                                    DateTime entryDateValue = entryDate.Value.Date;

                                    foreach (var visitorAllocation in visitorPassAllocations)
                                    {
                                        if (visitorAllocation.AssignDate <= entryDateValue && visitorAllocation.IsActive
                                            && visitorAllocation.EmployeeId == employee.EmployeeId)
                                        {
                                            visitorAllocation.IsActive = false;
                                            visitorAllocation.Notes = string.Format("Visitor pass allocation deactivated with the use of enrollment number: {0} at :{1}", enrollNumber,
                                                entryDate.Value.ToString());
                                        }
                                    }

                                    #endregion

                                    try
                                    {
                                        dbContext.SaveChanges();
                                        syncLogEntry.Status = SyncStatus.Successful.ToString();
                                    }
                                    catch (Exception ex)
                                    {
                                        syncLogEntry.Status = SyncStatus.Failed.ToString();
                                        syncLogEntry.Notes = ex.InnerException.InnerException.Message;
                                        LogException(ex, "Error occurred when creating the attendance entry", EventLogEntryType.Error);
                                    }
                                    syncLogEntryList.Add(syncLogEntry);
                                    syncMachineLog.NumberOfEmployeeRecords++;

                                    #endregion
                                }
                            }
                            else
                            {
                                DateTime? entryDate = TryParseDate(year, month, day, hour, minute, second);
                                DateTime entryDateValue = entryDate.Value.Date;

                                if (!dbContext.VisitorAttendances.Any(a => a.CardNo == enrollNumber &&
                                    a.DateTime == entryDate && a.VerifyMode == verifyMode &&
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

                                        string _logDescription = "Sync visitor record Enroll Number:{0}, DateTime:{1}/{2}/{3} {4}:{5}:{6} Mode:{7}";
                                        syncLogEntry.Description = string.Format(_logDescription, enrollNumber,
                                            month, day, year, hour, minute, second, mode);

                                        syncLogEntry.CardNo = enrollNumber.ToString();
                                        syncLogEntry.LocationId = location.Id;
                                        syncLogEntry.Status = SyncStatus.Successful.ToString();
                                        syncLogEntry.SyncSessionLog = syncSessionLog;
                                        syncLogEntry.SyncTime = System.DateTime.Now;

                                        #region --- Check with visitor pass allocation ---

                                        var visitorPassAllocation = visitorPassAllocations.FirstOrDefault(a => a.CardNo == enrollNumber
                                            && a.AssignDate == entryDateValue && a.IsActive);
                                        if (visitorPassAllocation != null)
                                        {
                                            if (visitorPassAllocation.EmployeeEnrollment != null)
                                            {
                                                EmployeeEnrollment employee =
                                                    dbContext.EmployeeEnrollment.Single(
                                                        e =>
                                                            e.EmployeeId ==
                                                            visitorPassAllocation.EmployeeEnrollment.EmployeeId);

                                                var attendance = new Attendance();
                                                /*
                                                * CardNo in attendance record is corrected to have to real card no which generate the record.  
                                                */
                                                //attendance.CardNo = visitorPassAllocation.EmployeeEnrollment.CardNo;
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

                                                visitorAttendance.isTransferred = true;
                                                visitorAttendance.TransferredDate = DateTime.Today;
                                                visitorAttendance.Note =
                                                    string.Format("Transferred to employee : {0} by {1} at {2}",
                                                        visitorPassAllocation.EmployeeEnrollment.CardNo,
                                                        "Sync Service", DateTime.Now.ToString());
                                            }

                                            //visitorAttendance.EntityId = visitorPassAllocation.EntityId;
                                            //visitorAttendance.EntityType = visitorPassAllocation.EntityType;
                                            visitorAttendance.VisitorPassAllocationId = visitorPassAllocation.Id;
                                            visitorAttendance.VisitInformationId = visitorPassAllocation.VisitInformationId;
                                            visitorAttendance.EmployeeId = visitorPassAllocation.EmployeeId;
                                            //visitorAttendance.TransferredBy_Id
                                        }

                                        #endregion

                                        dbContext.VisitorAttendances.Add(visitorAttendance);

                                        try
                                        {
                                            dbContext.SaveChanges();
                                            syncLogEntry.Status = SyncStatus.Successful.ToString();
                                        }
                                        catch (Exception ex)
                                        {
                                            syncLogEntry.Status = SyncStatus.Failed.ToString();
                                            syncLogEntry.Notes = ex.InnerException.InnerException.Message;
                                            LogException(ex, "Error occurred when creating the visitor attendance entry", EventLogEntryType.Error);
                                        }

                                        syncLogEntryList.Add(syncLogEntry);
                                        syncMachineLog.NumberOfVisitorRecords++;
                                    }

                                    #endregion
                                }
                            }


                            lastLogUpdatedTimeSpan = DateTime.Now - lastSyncLogStartTime;
                        }
                    }

                    #region --- Updating Machine Log Status  ---

                    syncMachineLog.CompletedAt = System.DateTime.Now;   
                    if (syncLogEntryList.Any(a => a.LocationId == location.Id && a.Status == SyncStatus.Failed.ToString()))
                    {
                        if (syncLogEntryList.Any(a => a.LocationId == location.Id && a.Status == SyncStatus.Successful.ToString()))
                        {
                            int _successfullCount = syncLogEntryList.Where(a => a.LocationId == location.Id && a.Status == SyncStatus.Successful.ToString()).Count();
                            int _unsuccessfullCount = syncLogEntryList.Where(a => a.LocationId == location.Id && a.Status == SyncStatus.Failed.ToString()).Count();

                            syncMachineLog.Status = SyncStatus.PartiallyCompleted.ToString();
                            syncMachineLog.Notes = string.Format("{0} Records synchronized successfully. {1} Records Failed.", _successfullCount, _unsuccessfullCount);
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

                    //1 -default, delete card reader machine logs, 0- don't delete
                    int deleteMachineLogs = int.Parse(ConfigurationManager.AppSettings["DeleteMachineLogs"]);

                    // If the location is set as to clear logs automatically from the database
                    //As per seminda he has commented below clearing code for testing purpose and mistakenly checked in. He asked me to correct it
                    if (location.ClearLogsAutomatically && deleteMachineLogs == 1)
                    {
                        if (lastLogUpdatedTimeSpan != null && lastLogUpdatedTimeSpan.Value.TotalMilliseconds < 100 && deleteLogsForToday)
                        {
                            lastDeviceClearedDate = DateTime.Now.Date;
                            Logger.Log(string.Format("Clear logs on location :{0} started at:{1}.", location.Floor, DateTime.Now.ToString()), EventLogEntryType.SuccessAudit);

                            //If all the logs are completed successfuly then cleat the logs on device
                            if (syncMachineLog.Status.Equals(SyncStatus.Successful.ToString()))
                            {
                                // Returns 0 for success or -1 for fail.
                                if (_timeRecordsReader.DeleteGeneralLog() == 0)
                                {
                                    string _clearMessage = string.Format("All logs clear on {0} at : {1}", location.Floor,
                                        System.DateTime.Now.ToString());
                                    syncMachineLog.Notes += _clearMessage;
                                    Logger.Log(_clearMessage, EventLogEntryType.SuccessAudit);
                                }
                                else
                                {
                                    string _clearMessage = string.Format("Clear logs failed on {0} at : {1}", location.Floor,
                                        System.DateTime.Now.ToString());
                                    syncMachineLog.Notes += _clearMessage;
                                    Logger.Log(_clearMessage, EventLogEntryType.Error);
                                }
                            }
                        }
                    }

                    #endregion
                }
                else
                {
                    // If the connection was not successful with the device ?
                    // Write the event to the event log and continue with the next device....
                    string _errorMessage = string.Format(@"Could not connect to device on {0} at {1} (IP Address :{2},Device Name:{3}, Device  Number:{4}, Port: {5}, Comm Key:{6})",
                    location.Floor, syncMachineLog.StartAt.ToString(), location.DeviceIP, location.DeviceName, location.DeviceNumber, location.Port, location.CommKey);

                    syncMachineLog.Status = SyncStatus.Failed.ToString();
                    syncMachineLog.Notes = _errorMessage;
                    syncMachineLog.CompletedAt = System.DateTime.Now;

                    Logger.Log(_errorMessage, EventLogEntryType.Error);
                }

                syncMachineLogList.Add(syncMachineLog);
            }

            #region --- Updating Sync Session Log Status  ---

            syncSessionLog.CompletedAt = System.DateTime.Now;
            syncSessionLog.Status = SyncStatus.Successful.ToString();
            syncSessionLog.Description = string.Format("Synchronization of MyTime With FingerTec at {0}", syncSessionLog.StartAt.ToString());
            if (syncMachineLogList.Any(a => a.Status == SyncStatus.Failed.ToString()))
            {
                if (syncMachineLogList.Any(a => a.Status == SyncStatus.Successful.ToString()))
                {
                    int successfullCount = syncMachineLogList.Where(a => a.Status == SyncStatus.Successful.ToString()).Count();
                    int unsuccessfullCount = syncMachineLogList.Where(a => a.Status == SyncStatus.Failed.ToString()).Count();

                    syncSessionLog.Status = SyncStatus.PartiallyCompleted.ToString();
                    syncSessionLog.Notes = string.Format("{0} Records synchronized successfully. {1} Records Failed.", successfullCount, unsuccessfullCount);
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

            foreach (var _machineLog in syncMachineLogList)
            {
                syncSessionLog.NumberOfEmployeeRecords += _machineLog.NumberOfEmployeeRecords;
                syncSessionLog.NumberOfVisitorRecords += _machineLog.NumberOfVisitorRecords;
                syncSessionLog.NuberOfExternalRecords += _machineLog.NuberOfExternalRecords;
            }

            #endregion

            #region --- Saving Finalized Log Entries ---

            dbContext.SyncSessionLogs.Add(syncSessionLog);

            foreach (var machineLog in syncMachineLogList)
                dbContext.SyncMachineLogs.Add(machineLog);

            foreach (var auditLogEntry in syncLogEntryList)
                dbContext.SyncLogEntries.Add(auditLogEntry);

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

            syncStatusInfo = dbContext.SyncServiceStatusInfo.FirstOrDefault();
            int _syncInterval = int.Parse(ConfigurationManager.AppSettings["SynchronizationInterval"].ToString());

            if (syncStatusInfo != null)
            {
                syncStatusInfo.SynchronizationStatus = SyncProcessStatus.Idle.ToString();
                syncStatusInfo.LastServiceStoppedAt = syncSessionLog.CompletedAt;
                syncStatusInfo.NextServiceScheduledAt = System.DateTime.Now.AddMinutes(_syncInterval);
                syncStatusInfo.CurrentSynchronizingMessage = string.Format("Synchronization completed on all floors at: {0}",
                    syncSessionLog.CompletedAt.ToString());

                try
                {
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    LogException(ex, "Error occurred when updating service status info", EventLogEntryType.Error);
                }
            }

            #endregion
        }
        
        public void AppendTextBox(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextBox), new object[] { value });
                return;
            }
            textBoxLog.Text += Environment.NewLine + value;
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

        private void CreateAndStartSyncStatusInfo()
        {
            try
            {
                var dbContext = new Context();
                var syncStatusInfo = dbContext.SyncServiceStatusInfo.FirstOrDefault();
                if (syncStatusInfo == null)
                {
                    syncStatusInfo = new SyncServiceStatusInfo();
                    syncStatusInfo.ServiceStatus = SyncServiceStatus.Running.ToString();
                    dbContext.SyncServiceStatusInfo.Add(syncStatusInfo);
                }
                else
                {
                    syncStatusInfo.ServiceStatus = SyncServiceStatus.Running.ToString();
                }

                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogException(ex, "Error occurred when creating service status info", EventLogEntryType.Error);
            }
        }

        private void UpdateStopSyncStatusInfor()
        {
            try
            {
                var dbContext = new Context();
                var syncStatusInfo = dbContext.SyncServiceStatusInfo.FirstOrDefault();
                if (syncStatusInfo != null)
                    syncStatusInfo.ServiceStatus = SyncServiceStatus.Stopped.ToString();

                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogException(ex, "Error occurred when updating service status info", EventLogEntryType.Error);
            }
        }

        public void DoPeriodically(int millis)
        {
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        SyncNow();
                    }
                    catch (Exception e)
                    {
                        AppendTextBox(DateTime.Now + " Exception in timer: " + e);
                    }
                    finally
                    {
                        Thread.Sleep(millis);
                    }
                }
            }).Start();
        }

        private void button_sync_Click(object sender, EventArgs e)
        {
             Logger.Log(string.Format("MyTime synchronization service started at : {0}", System.DateTime.Now),
                EventLogEntryType.Information);
            textBoxLog.Text = string.Format("MyTime synchronization service started at : {0}", System.DateTime.Now);
            int syncInterval = int.Parse(ConfigurationManager.AppSettings["SynchronizationInterval"].ToString());
            DoPeriodically((syncInterval * 60 * 1000));
        }

        private void axBioBridgeSDK1_OnConnect(object sender, EventArgs e)
        {

        }
        private void KillExistingProcess()
        {

            foreach (var process in Process.GetProcessesByName("Exilesoft.TimeManager"))
            {
                process.Kill();
            }

        }
        private void SyncService_Load(object sender, EventArgs e)
        {
            

        }
    }
}
