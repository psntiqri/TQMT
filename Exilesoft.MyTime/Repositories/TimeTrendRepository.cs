// -------------DEVELOPER COMMENT---------------------------//
//
// Filename     : TimeTrendRepository.cs
// Created By   : Harinda Dias
// Date         : 2013-May-10, Fri
// Description  : Repository for the Time Trend Analysis View

//
// Modified By  : 
// Date         : 
// Purpose      : 
//
// ---------------------------------------------------------//

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Exilesoft.Models;
using Exilesoft.MyTime.Helpers;
using Exilesoft.MyTime.ViewModels;

namespace Exilesoft.MyTime.Repositories
{
    /// <summary>
    /// Repository for the Time Trend Analysis View
    /// </summary>
    public class TimeTrendRepository
    {
        private static Context dbContext = new Context();
        private static ViewModels.AttendaceReportStructure attendanceCoverageReportModel = new ViewModels.AttendaceReportStructure();
        private static int plannedMinitsforDay = 0;


        /// <summary>
        /// Gets the data for the graph generation required.
        /// Result would be in the form of required for the Javascript
        /// </summary>
        /// <param name="model">View Model sent from the client with data</param>
        /// <returns>Structured data with the summery</returns>
        internal static ViewModels.AttendaceReportStructure GetEmployeesInOutGraphData(ViewModels.TimeTrendAnalysisViewModel model)
        {
            #region --- Local Variables----

            StringBuilder _sb = new StringBuilder();
            StringBuilder _employeeStringBuilder = new StringBuilder();
            attendanceCoverageReportModel = new ViewModels.AttendaceReportStructure();
            attendanceCoverageReportModel.Duration = string.Format("{0} - {1}", model.FromDate.Value.ToShortDateString(), model.ToDate.Value.ToShortDateString());
            _sb.Append(string.Format("{0},{1},{2}\n", "Date", "In", "Out"));
            decimal _plannedMinitsforDay = decimal.Parse(ConfigurationManager.AppSettings["PlannedMinitsPerDay"].ToString());
            EmployeeEnrollment _selectedEmployee =
                EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(model.SelectedEmployeeID);
            DateTime _tempStartDate = model.FromDate.Value;
            int _totalValidDaysForAverageCalculation = 0;          
        
            #endregion
            
            IList<Leave> leaveList =  new List<Leave>();
            if (model.FromDate < model.ToDate)
            {
                var user = EmployeeRepository.GetEmployee(model.SelectedEmployeeID);
                leaveList = GetLeaveList(model, user);

                var holidayList = HolidayRepository.GetHolidays(model.FromDate.Value, model.ToDate.Value);
                
                do
                {
                    decimal _plannedMinitsForThisDate = _plannedMinitsforDay;
                    bool _validForInOutAverageCalculation = true;

                    #region --- Check for holidays and leave ---

                    var _holiday = HolidayRepository.IsHoliday(holidayList, model.FromDate.Value);
                    var _leave = LeaveRepository.IsLeave(leaveList, model.FromDate.Value);
                    
                    if (_holiday != null)
                    {
                        _validForInOutAverageCalculation = false;
                        if (_holiday.Type != HolidayType.FullDay)
                        {
                            if (_leave == null)
                            {
                                 attendanceCoverageReportModel.TotalPlanned += _plannedMinitsForThisDate / 2;
                                attendanceCoverageReportModel.WorkingDays = attendanceCoverageReportModel.WorkingDays + (decimal)0.5;
                            }
                        }
                    }
                    else
                    {
                        if (_leave != null)
                        {
                            _validForInOutAverageCalculation = false;
                            if (_leave.LeaveType != LeaveType.FullDay)
                            {
                                 attendanceCoverageReportModel.TotalPlanned += _plannedMinitsForThisDate / 2;
                                attendanceCoverageReportModel.WorkingDays = attendanceCoverageReportModel.WorkingDays + (decimal)0.5;
                            }
                        }
                        else
                        {
                            attendanceCoverageReportModel.TotalPlanned += _plannedMinitsForThisDate ;
                            attendanceCoverageReportModel.WorkingDays = attendanceCoverageReportModel.WorkingDays + 1;
                        }
                    }



                    // Not considering the current date for the average in out calculation
                    if (model.FromDate.Value == Utility.GetDateTimeNow().Date)
                        _validForInOutAverageCalculation = false;

                    if (_validForInOutAverageCalculation)
                        _totalValidDaysForAverageCalculation++;

                    #endregion

                    // Transforming the data for the graph.
                    // Required format for the Javascript graph generation.
                    _sb.Append(GetEmployeesInOutGraphData(model, model.FromDate.Value, _validForInOutAverageCalculation));

                   // attendanceCoverageReportModel.TotalPlanned += _plannedMinitsForThisDate;


                    #region --- Generate the holiday list for the graph ----
                    if (_holiday != null)
                    {
                        attendanceCoverageReportModel.HolidayDateList.Add(new ViewModels.HolidayOnView()
                        {
                            FromDate = model.FromDate.Value.ToString("yyyy/MM/dd"),
                            ToDate = model.FromDate.Value.AddDays(1).ToString("yyyy/MM/dd"),
                            Reason = _holiday.Description
                        });
                    }
                    else
                    {
                        if (LeaveRepository.IsLeave(leaveList, model.FromDate.Value) != null)
                        {
                            attendanceCoverageReportModel.HolidayDateList.Add(new ViewModels.HolidayOnView()
                            {
                                FromDate = model.FromDate.Value.ToString("yyyy/MM/dd"),
                                ToDate = model.FromDate.Value.AddDays(1).ToString("yyyy/MM/dd"),
                                Reason = "Planned Leave"
                            });
                        }
                    }

                    #endregion




                    model.FromDate = model.FromDate.Value.AddDays(1);
                } while (model.FromDate <= model.ToDate);

                
                #region --- Update average in out time ---

                int _averageInTimeDevation = 0;
                int _averageOutTimeDevation = 0;

                if (_totalValidDaysForAverageCalculation > 0)
                {
                    _averageInTimeDevation = attendanceCoverageReportModel.TotalDeviationForInTimeAverage / _totalValidDaysForAverageCalculation;
                    _averageOutTimeDevation = attendanceCoverageReportModel.TotalDeviationForOutTimeAverage / _totalValidDaysForAverageCalculation;
                }


                DateTime _standardInTime = new DateTime(Utility.GetDateTimeNow().Year, Utility.GetDateTimeNow().Month, DateTime.Today.Day, 9, 0, 0);
                DateTime _standardOutTime = new DateTime(Utility.GetDateTimeNow().Year, Utility.GetDateTimeNow().Month, DateTime.Today.Day, 18, 0, 0);
                DateTime _averageIn = _standardInTime.AddSeconds((_averageInTimeDevation * -1));
                DateTime _averageOut = _standardOutTime.AddSeconds((_averageOutTimeDevation * -1));


                attendanceCoverageReportModel.AverageInTime = _averageIn.ToLongTimeString();
                attendanceCoverageReportModel.AverageOutTime = _averageOut.ToLongTimeString();

                #endregion
            }

           
            #region --- Calculating the planned leaves within the date range ---

            foreach (var leave in leaveList)
            {
                if (leave.Date.Date >= _tempStartDate.Date && leave.Date.Date <= model.ToDate)
                {
                    if (leave.LeaveType == LeaveType.FullDay)
                        attendanceCoverageReportModel.TotalPlannedLeave = attendanceCoverageReportModel.TotalPlannedLeave + 1;
                    else
                        attendanceCoverageReportModel.TotalPlannedLeave = attendanceCoverageReportModel.TotalPlannedLeave + (decimal)0.5;
                }
            }

            #endregion


            decimal TotalOutOfOffice = attendanceCoverageReportModel.EmployeeOutOfOfficeList.Sum(a => a.OutMinits);
            decimal TotalOutOfWFH = TotalOutOfWFH = attendanceCoverageReportModel.EmployeeOutOfWFHList.Sum(a => a.OutMinits);

            if (attendanceCoverageReportModel.TotalPlanned > 0)
            {              

                attendanceCoverageReportModel.WorkCoverage = Math.Round((((attendanceCoverageReportModel.TotalActual - TotalOutOfOffice) / attendanceCoverageReportModel.TotalPlanned) * 100), 2);
                
                if (!((attendanceCoverageReportModel.TotalActual - TotalOutOfOffice) <= 0))
                {
                    attendanceCoverageReportModel.WFHPercentage = Math.Round((((attendanceCoverageReportModel.TotalActualWFH - TotalOutOfWFH) * 100) / (attendanceCoverageReportModel.TotalActual - TotalOutOfOffice)), 2);
                    if (attendanceCoverageReportModel.WFHPercentage > 100)
                    {
                        attendanceCoverageReportModel.WFHPercentage = 100;
                    }
                }
            }
            
            attendanceCoverageReportModel.ResultGraphData = _sb.ToString();

         
            attendanceCoverageReportModel.TotalPlanned = attendanceCoverageReportModel.TotalPlanned/60;
            attendanceCoverageReportModel.TotalActual = (attendanceCoverageReportModel.TotalActual - TotalOutOfOffice )/ 60;
            attendanceCoverageReportModel.TotalOutOfOffice = TotalOutOfOffice / 60;

            return attendanceCoverageReportModel;
        }

        private static IList<Leave> GetLeaveList(ViewModels.TimeTrendAnalysisViewModel model, EmployeeData user)
        {
            IList<Leave> leaveList = new List<Leave>();
            var leaveListSer = LeaveRepository.GetMyLeaveService(model.FromDate.Value, model.ToDate.Value);
            var leaveListSerForUer = leaveListSer.Where(a => a.user == user.PrimaryEmailAddress).ToList();
            var myLeaveViewModel = leaveListSerForUer.FirstOrDefault();
            if (myLeaveViewModel != null && myLeaveViewModel.Leaves.Any())
            {
                var approvedLeaveList = myLeaveViewModel.Leaves.Where(l => l.status == "Approved" || l.status == "Pending").ToList();

                foreach (var leaveItem in approvedLeaveList)
                {
                    var leaveType = LeaveType.FullDay;
                    if (leaveItem.LeaveType != MyLeaveType.HALFDAY_NO.ToString())
                    {
                        leaveType = LeaveType.HalfDay;
                    }
                    Leave leave = new Leave() { Date = DateTime.Parse(leaveItem.date), LeaveType = leaveType };
                    leaveList.Add(leave);
                }
            }
            return leaveList;
        }


        /// <summary>
        /// Converts the decimal minits in to hours and minits
        /// </summary>
        /// <param name="minits">Total minits to be converted</param>
        /// <returns>hours and minits</returns>
        private static decimal GetMinitsToHours(decimal minits)
        {
            string _hours = string.Format("{0}.{1}", (int)minits / 60, minits % 60);
            return Convert.ToDecimal(_hours);
        }

        /// <summary>
        /// Gets the in out graph data for the alanysis report for the selected date
        /// Result would be in the form of required for the Javascript
        /// </summary>
        /// <param name="model">Viewmodel from the client with parameters</param>
        /// <param name="queryDate">Date for the selection of attendance</param>
        /// <returns>Structure data for the graph</returns>
        internal static string GetEmployeesInOutGraphData(ViewModels.TimeTrendAnalysisViewModel model, DateTime queryDate, bool validateDeviation)
        {
            #region ---- Local variables ----

            string _reportDataElement = string.Empty;
            IQueryable<Attendance> _attendanceSearchQuery = dbContext.Attendances;
            _attendanceSearchQuery = GetAttendanceSearchQuery(model, queryDate);
            List<Attendance> attendanceList =
                _attendanceSearchQuery.ToList()
                                      .OrderBy(a => a.EmployeeId)
                                      .ThenBy(a => a.Year)
                                      .ThenBy(a => a.Month)
                                      .ThenBy(a => a.Day)
                                      .ThenBy(a => a.Hour)
                                      .ThenBy(a => a.Minute)
                                      .ThenBy(a => a.Second).ToList();
            plannedMinitsforDay = int.Parse(ConfigurationManager.AppSettings["PlannedMinitsPerDay"].ToString());
            List<int> _exitLocationList = GetExitLocationMachins();

            #endregion

            //var _firstInAttendance = attendanceList.FirstOrDefault(a => a.EmployeeId == model.SelectedEmployeeID && a.InOutMode == "in");

            var _firstInAttendance = attendanceList.FirstOrDefault(a => a.EmployeeId == model.SelectedEmployeeID );//Get the both in/Out Card pucn records for Time calculation.....
           
            if (_firstInAttendance != null)
            {
                attendanceCoverageReportModel.LoggedDays++;

                // Gets the first attendance record with in
                // consider this record as the employee IN time.
                DateTime _firstInTime = new DateTime(_firstInAttendance.Year, _firstInAttendance.Month, _firstInAttendance.Day,
                                              _firstInAttendance.Hour, _firstInAttendance.Minute, _firstInAttendance.Second);

                #region --- Calculate InTime Deviation ---

                if (validateDeviation)
                {
                    DateTime _standardInTime = new DateTime(queryDate.Year, queryDate.Month, queryDate.Day, 9, 0, 0);
                    TimeSpan _inTimeDeviation = _standardInTime - _firstInTime;
                    attendanceCoverageReportModel.TotalDeviationForInTimeAverage += (int)_inTimeDeviation.TotalSeconds;
                }

                #endregion

                #region --- Generation of graph data ---

                Attendance _lastAttendance = null;
                DateTime? _lastOutTime = null;

                // Gets the last out attendance as the employee OUT time
                var outattendanceList = attendanceList.Where(a => a.EmployeeId == model.SelectedEmployeeID && a.InOutMode == "out");
                if (outattendanceList.Count() > 0)
                    _lastAttendance = attendanceList.Last(a => a.EmployeeId == model.SelectedEmployeeID && a.InOutMode == "out");

                if (_lastAttendance != null)
                {
                    _lastOutTime = new DateTime(_lastAttendance.Year, _lastAttendance.Month, _lastAttendance.Day,
                                               _lastAttendance.Hour, _lastAttendance.Minute, _lastAttendance.Second);
                }
                else
                {
                    if (_firstInTime.Date == Utility.GetDateTimeNow().Date)
                        _lastOutTime = Utility.GetDateTimeNow();
                }

                if (_lastOutTime != null)
                {
                    string _inTimeString = string.Empty;
                    string _outTimeString = string.Empty;

                    string _firstInMinitStr = _firstInTime.Minute.ToString("##");
                    string _lastOutnMinitStr = _lastOutTime.Value.Minute.ToString("##");

                    if (_firstInMinitStr.Length < 2)
                        _firstInMinitStr = "0" + _firstInMinitStr;
                    if (_lastOutnMinitStr.Length < 2)
                        _lastOutnMinitStr = "0" + _lastOutnMinitStr;

                    if (model.ViewInTime)
                        _inTimeString = string.Format("{0}.{1}", _firstInTime.Hour.ToString("##"), _firstInMinitStr);
                    if (model.ViewOutTime)
                        _outTimeString = string.Format("{0}.{1}", _lastOutTime.Value.Hour.ToString("##"), _lastOutnMinitStr);

                    _reportDataElement = string.Format("{0},{1},{2}\n", queryDate.ToString("yyyyMMdd"),
                        _inTimeString, _outTimeString);

                    TimeSpan _ts = _lastOutTime.Value - _firstInTime;
                    attendanceCoverageReportModel.TotalActual += (decimal)_ts.TotalMinutes;
                    if (_firstInAttendance.LocationId == 27)
                    {
                        attendanceCoverageReportModel.TotalActualWFH += (decimal)_ts.TotalMinutes;
                    }
                    #region --- Calculate OutTime Deviation ---

                    if (validateDeviation)
                    {
                        DateTime _standardOutTime = new DateTime(queryDate.Year, queryDate.Month, queryDate.Day, 18, 0, 0);
                        TimeSpan _outTimeDeviation = _standardOutTime - _lastOutTime.Value;
                        attendanceCoverageReportModel.TotalDeviationForOutTimeAverage += (int)_outTimeDeviation.TotalSeconds;
                    }

                    #endregion

                }

                #endregion

                #region --- Out of office time calculation ---

                if (_lastAttendance != null)
                {
                    for (int i = 0; i < attendanceList.Count; i++)
                    {
                        var _outOfficeEntry = attendanceList[i];
                        // Check for the attendance entry is not the first and the last entry for the day.
                        if (_outOfficeEntry.Id != _firstInAttendance.Id && _outOfficeEntry.Id != _lastAttendance.Id
                            && _outOfficeEntry.InOutMode == "out")
                        {

                            var firstInAttendanceForTheDay = attendanceList.FirstOrDefault(a => a.Year == _outOfficeEntry.Year &&
                                a.Month == _outOfficeEntry.Month && a.Day == _outOfficeEntry.Day && a.InOutMode == "in");

                            bool isValidForCalculation = false;
                            if (firstInAttendanceForTheDay != null)
                            {
                                DateTime firstInTime = new DateTime(firstInAttendanceForTheDay.Year, firstInAttendanceForTheDay.Month, firstInAttendanceForTheDay.Day,
                                    firstInAttendanceForTheDay.Hour, firstInAttendanceForTheDay.Minute, firstInAttendanceForTheDay.Second);
                                DateTime outOfOfficeTime = new DateTime(_outOfficeEntry.Year, _outOfficeEntry.Month, _outOfficeEntry.Day,
                                        _outOfficeEntry.Hour, _outOfficeEntry.Minute, _outOfficeEntry.Second);
                                if (_firstInTime < outOfOfficeTime)
                                    isValidForCalculation = true;
                            }

                            // Check if the attendece entry is from any exit location.
                            if (_exitLocationList.IndexOf(_outOfficeEntry.LocationId) != -1 && isValidForCalculation)
                            {
                                // Check the next attendance entry is not the last entry
                                if ((i + 1) < (attendanceList.Count - 1))
                                {
                                    if (attendanceList[i + 1].InOutMode == "in")
                                    {
                                        var _backtoOfficeEntry = attendanceList[i + 1];
                                        DateTime _outOfOfficeTime = new DateTime(_outOfficeEntry.Year, _outOfficeEntry.Month, _outOfficeEntry.Day,
                                               _outOfficeEntry.Hour, _outOfficeEntry.Minute, _outOfficeEntry.Second);
                                        DateTime _backtoOfficeTime = new DateTime(_backtoOfficeEntry.Year, _backtoOfficeEntry.Month, _backtoOfficeEntry.Day,
                                               _backtoOfficeEntry.Hour, _backtoOfficeEntry.Minute, _backtoOfficeEntry.Second);
                                        TimeSpan _tsOutOfOffice = _backtoOfficeTime - _outOfOfficeTime;

                                        attendanceCoverageReportModel.EmployeeOutOfOfficeList.Add(new ViewModels.EmployeeOutOfOffice()
                                        {
                                            EmployeeID = model.SelectedEmployeeID,
                                            OutDate = queryDate.ToString("yyyy/MM/dd"),
                                            FromTime = decimal.Parse(string.Format("{0}.{1}", _outOfOfficeTime.Hour.ToString(), _outOfOfficeTime.Minute.ToString())),
                                            ToTime = decimal.Parse(string.Format("{0}.{1}", _backtoOfficeTime.Hour.ToString(), _backtoOfficeTime.Minute.ToString())),
                                            OutMinits = (decimal)_tsOutOfOffice.TotalMinutes
                                        });

                                        if (_outOfficeEntry.LocationId == 27 && _backtoOfficeEntry.LocationId == 27)
                                        {
                                            attendanceCoverageReportModel.EmployeeOutOfWFHList.Add(new ViewModels.EmployeeOutOfWFH()
                                            {
                                                EmployeeID = model.SelectedEmployeeID,
                                                OutDate = queryDate.ToString("yyyy/MM/dd"),
                                                FromTime = decimal.Parse(string.Format("{0}.{1}", _outOfOfficeTime.Hour.ToString(), _outOfOfficeTime.Minute.ToString())),
                                                ToTime = decimal.Parse(string.Format("{0}.{1}", _backtoOfficeTime.Hour.ToString(), _backtoOfficeTime.Minute.ToString())),
                                                OutMinits = (decimal)_tsOutOfOffice.TotalMinutes
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion
            }

            return _reportDataElement;
        }

        /// <summary>
        /// Creates the dynamic search Query for the attendace.
        /// </summary>
        /// <param name="model">view model from the client with parameter data</param>
        /// <param name="queryDate">slected date for the query</param>
        /// <returns>Returns the search query</returns>
        private static IQueryable<Attendance> GetAttendanceSearchQuery(ViewModels.TimeTrendAnalysisViewModel model, DateTime queryDate)
        {
            IQueryable<Attendance> _attendanceSearchQuery = dbContext.Attendances;
            _attendanceSearchQuery = dbContext.Attendances;
            _attendanceSearchQuery = _attendanceSearchQuery.Where(entity => entity.EmployeeId == model.SelectedEmployeeID);
            _attendanceSearchQuery = _attendanceSearchQuery.Where(a => a.Day == queryDate.Day).
                Where(a => a.Month == queryDate.Month).Where(a => a.Year == queryDate.Year);
            //_attendanceSearchQuery = _attendanceSearchQuery

            return _attendanceSearchQuery;
        }

        /// <summary>
        /// Gets the exit locations from the configuraton file
        /// </summary>
        /// <returns>List of location IDs</returns>
        internal static List<int> GetExitLocationMachins()
        {
            string _exitLocationMachines = ConfigurationManager.AppSettings["ExitLocationMachines"].ToString();
            List<int> _locationIDList = new List<int>();
            foreach (string locationID in _exitLocationMachines.Split(','))
                _locationIDList.Add(int.Parse(locationID));

            return _locationIDList;
        }
    }
}