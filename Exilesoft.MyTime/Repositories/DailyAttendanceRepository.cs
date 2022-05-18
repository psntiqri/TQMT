// -------------DEVELOPER COMMENT---------------------------//
//
// Filename     : DailyAttendanceRepository.cs
// Created By   : Harinda Dias
// Date         : 2013-May-10, Fri
// Description  : Repository for the daily attendance view 

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
using System.Web;
using System.Web.Optimization;
using Exilesoft.Models;
using System.Data.Entity;
using Exilesoft.MyTime.Helpers;
using Exilesoft.MyTime.ViewModels;
using Exilesoft.MyTime.Filters;
using System.Net.Mail;
using System.Data.Entity.Core.Objects;

namespace Exilesoft.MyTime.Repositories
{
    /// <summary>
    /// Repository for the daily attendance view 
    /// </summary>
    public class DailyAttendanceRepository
    {
        private static Context dbContext = new Context();
        private static ViewModels.AttendaceReportStructure attendanceCoverageReportModel = new ViewModels.AttendaceReportStructure();
        private static IList<Holiday> holidayList = new List<Holiday>();
        private static IList<Leave> leaveList = new List<Leave>();
        private static int plannedMinitsforDay = 0;
        private static decimal actualOnpremH = 0;
        private static decimal actualOnpremMin = 0;

        private static decimal highlightTotalCoverage = 0;
        private static decimal highlightTotalWFHPercentage = 0;


        /// <summary>
        /// Search option for the employees with a given text
        /// Used with the option for auto complete search
        /// </summary>
        /// <param name="searchText">Employee Search Text</param>
        /// <returns>List of empployees HTML</returns>
        internal static string SearchEmployees(string searchText)
        {
            if (searchText == null)
                return string.Empty;

            var _employeeList = EmployeeRepository.EmployeeSearchByName(searchText);
            return GetEmployeeListView(_employeeList);

        }

        internal static string SearchShareEmployees(string searchText)
        {
            if (searchText == null)
                return string.Empty;

            var _employeeList = EmployeeRepository.EmployeeSearchByName(searchText);
            return GetSharedEmployeeListView(_employeeList);
        }

        /// <summary>
        /// Gets the data for the graph generation required.
        /// Result would be in the form of required for the Javascript
        /// </summary>
        /// <param name="model">View Model sent from the client with data</param>
        /// <returns>Structured data with the summery</returns>
        internal static ViewModels.AttendaceReportStructure GetEmployeesHorsGraphData(ViewModels.DailyAttendanceViewModel model)
        {
            #region --- Local Variables----

            StringBuilder _sb = new StringBuilder();
            StringBuilder _employeeStringBuilder = new StringBuilder();
            attendanceCoverageReportModel = new ViewModels.AttendaceReportStructure();
            _sb.Append(string.Format("{0},{1},{2}\n", "Date", "Planned", "Actual"));
            IList<EmployeeEnrollment> _employeeList = new List<EmployeeEnrollment>();

            decimal _totalActualMinits = 0;
            decimal _totalActualWFHMins = 0;
            int _noOfDays = 0;
            double leaveCount = 0;
            decimal totalTeamCompletedPrecentage = 0;
            decimal totalTeamCompletedWFHPrecentage = 0;
            int _plannedMinitsforDay = int.Parse(ConfigurationManager.AppSettings["PlannedMinitsPerDay"].ToString());
            IList<Leave> leaveListNew = new List<Leave>();

            holidayList = HolidayRepository.GetHolidays(model.FromDate.Value, model.ToDate.Value);

            // Iniating the data structure with the list of employees
            // Selected by the user for the report.
            List<MyLeaveViewModel> employeeLeaves = LeaveRepository.GetMyLeaveService(model.FromDate.Value, model.ToDate.Value);
            foreach (var _checkEemployee in model.SelectedEmployeeList)
            {
                var _employee = EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(_checkEemployee.Id);//dbContext.Employees.FirstOrDefault(e => e.Id == _checkEemployee.Id);
                if (_employee != null)
                {
                    _employeeList.Add(_employee);
                    attendanceCoverageReportModel.EmployeeCoverageList.Add(new ViewModels.EmployeeAttandaceCoverage()
                    {
                        EmployeeID = _checkEemployee.Id,
                        EmployeeName = _checkEemployee.Name
                    });

                    var leaveListForUser = LeaveRepository.GetLeave(model.FromDate.Value, model.ToDate.Value, _employee, holidayList, employeeLeaves);
                    if (leaveListForUser.Any())
                    {
                        leaveListNew = leaveListNew.Concat(leaveListForUser).ToList();
                    }
                }
            }

            #endregion
            leaveListNew = leaveListNew.Where(a => a.Status == "Pending" || a.Status == "Approved").ToList();

            if (model.FromDate < model.ToDate)
            {
                leaveList = leaveListNew;

                do
                {
                    var _holiday = HolidayRepository.IsHoliday(holidayList, model.FromDate.Value);
                    if (_holiday != null)
                    {
                        if (_holiday.Type == HolidayType.FullDay)
                        {
                        }
                        else
                        {
                            attendanceCoverageReportModel.WorkingDays = attendanceCoverageReportModel.WorkingDays + (decimal)0.5;
                        }
                    }
                    else
                    {
                        attendanceCoverageReportModel.WorkingDays = attendanceCoverageReportModel.WorkingDays + 1;
                    }


                    _noOfDays++;
                    // Consider saturday and sunday as holidays
                    UpdatePlannedCoverageOnDate(model.FromDate.Value, _employeeList);

                    // Gets the actual worked hours for the employee
                    // for the selected date in the iteration.
                    _totalActualMinits += GetActualWorkedMinits(model, model.FromDate.Value);

                    // Transforming the data for the graph.
                    // Required format for the Javascript graph generation.
                    _sb.Append(string.Format("{0},{1},{2}\n", model.FromDate.Value.ToString("yyyyMMdd"),
                        (attendanceCoverageReportModel.TotalPlanned / 60).ToString("#.##"), (_totalActualMinits / 60).ToString("#.##")));

                    model.FromDate = model.FromDate.Value.AddDays(1);
                } while (model.FromDate <= model.ToDate);

                // Finalizing the summery by calculating the actual and coverage % 
                // for each employee selected for the report.
                attendanceCoverageReportModel.ResultGraphData = _sb.ToString();

                foreach (var employeeAttendance in attendanceCoverageReportModel.EmployeeCoverageList)
                {
                    decimal _actualMinits = employeeAttendance.ActualMinits;
                    employeeAttendance.ActualHours = (_actualMinits / 60).ToString("#.##");
                    if (employeeAttendance.PlannedMinits > 0)
                    {
                        employeeAttendance.Precentage = ((_actualMinits / employeeAttendance.PlannedMinits) * 100).ToString("#.##");
                    }
                    else
                    {
                        employeeAttendance.Precentage = "0";
                    }


                    if (employeeAttendance.Precentage.Equals(string.Empty))
                    {
                        employeeAttendance.Precentage = "0";
                    }

                    totalTeamCompletedPrecentage += decimal.Parse(employeeAttendance.Precentage);
                }
            }
            foreach (Leave leaveItem in leaveList)
            {
                if (leaveItem.LeaveType == LeaveType.FullDay)
                {
                    leaveCount += 1;
                }
                else
                {
                    leaveCount += 0.5;
                }
            }

            // Calculate the totoal team completion precentage
            // Requested by Anuradha
            attendanceCoverageReportModel.TotalTeamWorkCoverage = "0";
            if (attendanceCoverageReportModel.EmployeeCoverageList.Count > 0)
            {
                attendanceCoverageReportModel.TotalTeamWorkCoverage = (totalTeamCompletedPrecentage / attendanceCoverageReportModel.EmployeeCoverageList.Count).ToString("#.##");
            }
            attendanceCoverageReportModel.TotalLeaveCount = leaveCount.ToString();

            return attendanceCoverageReportModel;
        }

        /// <summary>
        /// Updates the planned working hours on the list of employees
        /// For the selected date 
        /// </summary>
        /// <param name="selectedDate">Date for the update</param>
        /// <param name="employeeList">Selected employee list</param>
        internal static void UpdatePlannedCoverageOnDate(DateTime selectedDate, IList<EmployeeEnrollment> employeeList)
        {
            int _plannedMinitsforDay = int.Parse(ConfigurationManager.AppSettings["PlannedMinitsPerDay"].ToString());
            foreach (var employee in employeeList)
            {
                decimal _plannedMinitsForThisDate = _plannedMinitsforDay;
                var _holiday = HolidayRepository.IsHoliday(holidayList, selectedDate);
                if (_holiday != null)
                {
                    if (_holiday.Type == HolidayType.FullDay)
                        _plannedMinitsForThisDate = 0;
                    else
                        _plannedMinitsForThisDate = _plannedMinitsForThisDate / 2;
                }

                // If not a full day holiday?? then check for leave applied
                if (_plannedMinitsForThisDate > 0)
                {
                    var _leave = LeaveRepository.IsLeave(leaveList, selectedDate, employee.EmployeeId);
                    if (_leave != null)
                    {
                        if (_leave.LeaveType == LeaveType.FullDay)
                            _plannedMinitsForThisDate = 0;
                        else
                            _plannedMinitsForThisDate -= _plannedMinitsforDay / 2;
                    }
                }

                attendanceCoverageReportModel.TotalPlanned += _plannedMinitsForThisDate;
                foreach (var employeeAttendance in attendanceCoverageReportModel.EmployeeCoverageList)
                {
                    if (employeeAttendance.EmployeeID == employee.EmployeeId)
                    {
                        employeeAttendance.PlannedMinits += _plannedMinitsForThisDate;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Get the data for individula employee graph generation required.
        /// Result would be in the form of required for the Javascript
        /// </summary>
        /// <param name="model">View Model sent from the client with data</param>
        /// <returns>Structured data with the summery</returns>
        internal static ViewModels.AttendaceReportStructure GetSelectedEmployeeHorsGraphData(ViewModels.DailyAttendanceViewModel model)
        {
            StringBuilder _sb = new StringBuilder();
            StringBuilder _sb_top = new StringBuilder();
            StringBuilder _employeeStringBuilder = new StringBuilder();
            attendanceCoverageReportModel = new ViewModels.AttendaceReportStructure();
            _sb.Append(string.Format("{0},{1},{2}\n", "Date", "Planned", "Actual"));

            decimal _totalActualMinits = 0;
            decimal _totalPlannedIndividualMinits = 0;
            decimal _totalPlannedMinits = 0;
            int _noOfDays = 0;
            DateTime _tempStartDate = model.FromDate.Value;
            int _plannedMinitsforDay = int.Parse(ConfigurationManager.AppSettings["PlannedMinitsPerDay"].ToString());
            EmployeeData _selectedEmployee = null;
            IList<Leave> leaveListNew = new List<Leave>();


            // Iniating the data structure with the list of employees
            // Selected by the user for the report.
            List<MyLeaveViewModel> employeeLeaves = LeaveRepository.GetMyLeaveService(model.FromDate.Value, model.ToDate.Value);
            foreach (var _employee in model.SelectedEmployeeList)
            {
                _selectedEmployee = _employee;
                var _employeeEnrollment = EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(_employee.Id);
                attendanceCoverageReportModel.EmployeeCoverageList.Add(new ViewModels.EmployeeAttandaceCoverage()
                {
                    EmployeeID = _employee.Id,
                    EmployeeName = _employee.Name
                });

                var leaveListForUser = LeaveRepository.GetLeave(model.FromDate.Value, model.ToDate.Value, _employeeEnrollment, holidayList, employeeLeaves);
                if (leaveListForUser.Any())
                {
                    leaveListNew = leaveListNew.Concat(leaveListForUser).ToList();
                }
            }

            if (model.FromDate < model.ToDate)
            {
                holidayList = HolidayRepository.GetHolidays(model.FromDate.Value, model.ToDate.Value);
                // Add from previous month... for early leave submissions.
                List<MyLeaveViewModel> employeeLeavesList = LeaveRepository.GetMyLeaveService(model.FromDate.Value.AddMonths(-1), model.ToDate.Value);
                leaveList = LeaveRepository.GetLeave(model.FromDate.Value.AddMonths(-1), model.ToDate.Value, EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(_selectedEmployee.Id), holidayList, employeeLeavesList);

                leaveList = leaveListNew;

                do
                {
                    _noOfDays++;
                    decimal _plannedMinitsForThisDate = _plannedMinitsforDay;
                    // Consider saturday and sunday as holidays
                    var _holiday = HolidayRepository.IsHoliday(holidayList, model.FromDate.Value);
                    if (_holiday != null)
                    {
                        if (_holiday.Type == HolidayType.FullDay)
                            _plannedMinitsForThisDate = 0;
                        else
                            _plannedMinitsForThisDate = _plannedMinitsForThisDate / 2;
                    }

                    if (_plannedMinitsForThisDate > 0)
                    {
                        var _leave = LeaveRepository.IsLeave(leaveList, model.FromDate.Value);
                        if (_leave != null)
                        {
                            if (_leave.LeaveType == LeaveType.FullDay)
                                _plannedMinitsForThisDate = 0;
                            else
                                _plannedMinitsForThisDate -= _plannedMinitsforDay / 2;
                        }
                    }

                    _totalPlannedIndividualMinits += _plannedMinitsForThisDate;
                    _totalPlannedMinits += _plannedMinitsForThisDate * model.SelectedEmployeeList.Count;

                    // Gets the actual worked hours for the employee
                    // for the selected date in the iteration.
                    _totalActualMinits += GetActualWorkedMinits(model, model.FromDate.Value);

                    // Transforming the data for the graph.
                    // Required format for the Javascript graph generation.
                    _sb.Append(string.Format("{0},{1},{2}\n", model.FromDate.Value.ToString("yyyyMMdd"),
                        (_totalPlannedIndividualMinits / 60).ToString("#.##"), (_totalActualMinits / 60).ToString("#.##")));

                    model.FromDate = model.FromDate.Value.AddDays(1);
                } while (model.FromDate <= model.ToDate);

                // Finalizing the summery by calculating the actual and coverage % 
                // for each employee selected for the report.
                attendanceCoverageReportModel.ResultGraphData = _sb.ToString();
                foreach (var employeeAttendance in attendanceCoverageReportModel.EmployeeCoverageList)
                {
                    decimal _actualMinits = employeeAttendance.ActualMinits;
                    employeeAttendance.ActualHours = (_actualMinits / 60).ToString("#.##");

                    if (_totalPlannedIndividualMinits > 0)
                        employeeAttendance.Precentage = ((_actualMinits / _totalPlannedIndividualMinits) * 100).ToString("#.##");
                    else
                        employeeAttendance.Precentage = "00";
                }

                #region --- Generate the holiday list for the graph ----

                model.FromDate = _tempStartDate;
                do
                {
                    var _holiday = HolidayRepository.IsHoliday(holidayList, model.FromDate.Value);
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
                    model.FromDate = model.FromDate.Value.AddDays(1);
                } while (model.FromDate <= model.ToDate);

                #endregion
            }

            return attendanceCoverageReportModel;
        }

        /// <summary>
        /// Build the summery report for the selected employees
        /// Generates the HTML for the report to be displayed
        /// </summary>
        /// <param name="model">Viewmodel from the client with data</param>
        /// <returns>HTML Report to be displayed</returns>
        internal static string GetEmployeesHorsSummeryData(ViewModels.DailyAttendanceViewModel model)
        {
            #region --- Local Variables----

            StringBuilder _sb = new StringBuilder();
            StringBuilder _employeeStringBuilder = new StringBuilder();
            attendanceCoverageReportModel = new ViewModels.AttendaceReportStructure();
            DateTime _tempDate = model.FromDate.Value;
            IList<EmployeeEnrollment> _employeeList = new List<EmployeeEnrollment>();

            int _totalActualMinits = 0;
            decimal _totalPlannedMinits = 0;
            decimal _plannedMinitsforDay = decimal.Parse(ConfigurationManager.AppSettings["PlannedMinitsPerDay"].ToString());
            IList<Leave> leaveListNew = new List<Leave>();

            holidayList = HolidayRepository.GetHolidays(model.FromDate.Value, model.ToDate.Value);

            // Iniating the data structure with the list of employees
            // Selected by the user for the report.
            List<MyLeaveViewModel> employeeLeaves = LeaveRepository.GetMyLeaveService(model.FromDate.Value, model.ToDate.Value);
            foreach (var _checkEmployee in model.SelectedEmployeeList)
            {
                var _employee = EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(_checkEmployee.Id);
                var employeeData = EmployeeRepository.GetEmployee(_checkEmployee.Id);
                if (_employee != null)
                {
                    _employeeList.Add(_employee);
                    attendanceCoverageReportModel.EmployeeCoverageList.Add(new ViewModels.EmployeeAttandaceCoverage()
                    {
                        EmployeeID = employeeData.Id,
                        EmployeeName = employeeData.Name
                    });

                    var leaveListForUser = LeaveRepository.GetLeave(model.FromDate.Value, model.ToDate.Value, _employee, holidayList, employeeLeaves);
                    if (leaveListForUser.Any())
                    {
                        leaveListNew = leaveListNew.Concat(leaveListForUser).ToList();
                    }

                }
            }


            #endregion

            if (model.FromDate < model.ToDate)
            {

                leaveList = leaveListNew;

                #region ---- Query List of attendance for all the employeess ----

                // Collect all the attendance reords for all employees 
                // For the reporting period.
                IQueryable<Attendance> _attendanceSearchQuery = dbContext.Attendances;
                List<Attendance> attendanceList = new List<Attendance>();
                do
                {
                    _attendanceSearchQuery = GetAttendanceSearchQuery(model, _tempDate);
                    var attendanceSerach =
                        _attendanceSearchQuery.ToList()
                                              .OrderBy(a => a.EmployeeId)
                                              .ThenBy(a => a.Year)
                                              .ThenBy(a => a.Month)
                                              .ThenBy(a => a.Day)
                                              .ThenBy(a => a.Hour)
                                              .ThenBy(a => a.Minute)
                                              .ThenBy(a => a.Second).ToList();
                    attendanceList.AddRange(attendanceSerach);
                    _tempDate = _tempDate.AddDays(1);
                } while (_tempDate <= model.ToDate);

                #endregion

                #region ---- Generating the hedder ----

                // Implementation of the report hedder part HTML. 
                _tempDate = model.FromDate.Value;
                _sb.Append("<h3>Attendance Summary</h3>");
                _sb.Append("<table class=\"summery_report\"><tr>");
                _sb.Append("<td class=\"employee_clm hedder_clm\">Employee Name</td>");

                do
                {
                    _sb.Append(string.Format("<td class=\"hedder_clm\">{0}<sup>{1}</sup> {2}<br />",
                        _tempDate.Day, GetDateSup(_tempDate.Day), GetMonthAbbr(_tempDate.Month)));

                    decimal _plannedMinitsForCurrentDate = _plannedMinitsforDay;
                    if (HolidayRepository.IsHoliday(holidayList, model.FromDate.Value) != null)
                        _plannedMinitsForCurrentDate = 0;

                    //if (_plannedMinitsForCurrentDate > 0)
                    //    _sb.Append(string.Format("<span class=\"plan_span\">Plan : {0} Hrs</span></td>",
                    //         //(_plannedMinitsForCurrentDate / 60).ToString("#.##")));
                    //         string.Format("{0}:{1}", _plannedMinitsForCurrentDate / 60, _plannedMinitsForCurrentDate % 60)));
                    //else
                    //    _sb.Append("<span class=\"plan_span\">Plan : 0 Hrs</span></td>");

                    _tempDate = _tempDate.AddDays(1);
                } while (_tempDate <= model.ToDate);
                _sb.Append("<td class=\"hedder_clm\">Total </td>");
                _sb.Append("</tr>");

                #endregion

                #region ---- Generating body for the list of employees ----

                IDictionary<DateTime, int> _totalRowData = new Dictionary<DateTime, int>();

                // Generation of body section for all employees
                // Will generate a single row for each and every
                foreach (var employeeAttendance in attendanceCoverageReportModel.EmployeeCoverageList)
                {
                    _sb.Append(string.Format("<tr><td class=\"nametd\">{0}</td>", employeeAttendance.EmployeeName));
                    _tempDate = model.FromDate.Value;
                    int _employeeTotal = 0;

                    do
                    {
                        int _totalWorkedMinits = GetEmployeeWorkedMinits(employeeAttendance.EmployeeID, _tempDate, attendanceList);

                        #region --- Calculate Planned minits for current date ---

                        decimal _plannedMinitsForThisDate = _plannedMinitsforDay;
                        var _holiday = HolidayRepository.IsHoliday(holidayList, _tempDate);
                        if (_holiday != null)
                        {
                            if (_holiday.Type == HolidayType.FullDay)
                                _plannedMinitsForThisDate = 0;
                            else
                                _plannedMinitsForThisDate = _plannedMinitsForThisDate / 2;
                        }

                        if (_plannedMinitsForThisDate > 0)
                        {
                            var _leave = LeaveRepository.IsLeave(leaveList, _tempDate, employeeAttendance.EmployeeID);
                            if (_leave != null)
                            {
                                if (_leave.LeaveType == LeaveType.FullDay)
                                    _plannedMinitsForThisDate = 0;
                                else
                                    _plannedMinitsForThisDate -= _plannedMinitsforDay / 2;
                            }
                        }

                        #endregion

                        // For the final summery
                        _totalPlannedMinits += _plannedMinitsForThisDate;

                        // For the final summery generation purpose
                        _totalActualMinits += _totalWorkedMinits;

                        //add to the total for the employee
                        _employeeTotal += _totalWorkedMinits;

                        string _statusClass = "work_done";
                        if (_totalWorkedMinits < _plannedMinitsForThisDate)
                            _statusClass = "work_incomplete";

                        if (_totalWorkedMinits > 0)
                            _sb.Append(string.Format("<td class=\"hourstd {0}\">{1} </td>", _statusClass,
                                //(_totalWorkedMinits / 60).ToString("#.##")));
                                string.Format("{0}:{1}", _totalWorkedMinits / 60, _totalWorkedMinits % 60)));
                        else
                            _sb.Append(string.Format("<td class=\"hourstd {0}\">0 </td>", _statusClass));

                        //add to summary row
                        var keyExisit = _totalRowData.ContainsKey(_tempDate);
                        if (keyExisit)
                            _totalRowData[_tempDate] += _totalWorkedMinits;
                        else
                            _totalRowData[_tempDate] = _totalWorkedMinits;

                        _tempDate = _tempDate.AddDays(1);
                    } while (_tempDate <= model.ToDate);
                    _sb.Append(string.Format("<td class=\"hourstd\">{0} </td>",
                        _employeeTotal > 0 ? string.Format("{0}:{1}", _employeeTotal / 60, _employeeTotal % 60) : "0"));
                    _sb.Append("</tr>");
                }

                #endregion

                #region ----Generating total row
                //StringBuilder _totoalRowSb = new StringBuilder("<tr><td class=\"nametd\"></td>");
                _sb.Append("<tr><td class=\"nametd\">Total </td>");
                _totalRowData.OrderBy(d => d.Key);
                int _daySumTotal = 0;
                foreach (var daySum in _totalRowData)
                {
                    _sb.Append(string.Format("<td class=\"hourstd\">{0} </td>",
                        daySum.Value > 0 ? string.Format("{0}:{1}", daySum.Value / 60, daySum.Value % 60) : "0"));
                    _daySumTotal += daySum.Value;
                }
                _sb.Append(string.Format("<td class=\"hourstd\">{0} </td>",
                    _daySumTotal > 0 ? string.Format("{0}:{1}", _daySumTotal / 60, _daySumTotal % 60) : "0"));
                _sb.Append("</tr>");

                #endregion

                #region ---- Generating the report summery ----

                // Adding the report summery sections HTML to the reoprt.
                TimeSpan _timeDuration = model.ToDate.Value - model.FromDate.Value;
                string _totalPlannedStrHrs = "0";
                string _totalActualStrHrs = "0";
                string _totalPlannedStrMnts = "0";
                string _totalActualStrMnts = "0";

                if (_totalPlannedMinits > 0)
                {
                    _totalPlannedStrHrs = (_totalPlannedMinits / 60).ToString();
                    _totalPlannedStrMnts = (_totalPlannedMinits % 60).ToString();
                }
                if (_totalActualMinits > 0)
                {
                    _totalActualStrHrs = (_totalActualMinits / 60).ToString();
                    _totalActualStrMnts = (_totalActualMinits % 60).ToString();
                }

                _sb.Append(string.Format("<tr><td colspan=\"{0}\" class=\"summerytd\">", (_timeDuration.Days + 3)));
                _sb.Append("<strong>Report summary</strong><br>");
                _sb.Append(string.Format("Report From : {0} To : {1}<br>", model.FromDate.Value.ToShortDateString(),
                    model.ToDate.Value.ToShortDateString()));
                _sb.Append(string.Format("Total planned hours : {0} Hrs {1} Mnts ", _totalPlannedStrHrs, _totalPlannedStrMnts));
                _sb.Append(string.Format("Total actual hours : {0} Hrs {1} Mnts<br>", _totalActualStrHrs, _totalActualStrMnts));
                _sb.Append("</td></tr></table>");

                highlightTotalCoverage = ((_totalActualMinits * 100) / _totalPlannedMinits);

                actualOnpremMin = _totalActualMinits;

                #endregion
            }

            return _sb.ToString();
        }

        internal static string GetEmployeesTaskHoursSummeryData(ViewModels.DailyAttendanceViewModel model)
        {
            #region --- Local Variables----

            StringBuilder _sb = new StringBuilder();
            attendanceCoverageReportModel = new ViewModels.AttendaceReportStructure();
            IList<EmployeeEnrollment> _employeeList = new List<EmployeeEnrollment>();

            decimal _plannedMinitsforDay = decimal.Parse(ConfigurationManager.AppSettings["PlannedMinitsPerDay"].ToString());

            // Iniating the data structure with the list of employees
            // Selected by the user for the report.
            foreach (var _checkEmployee in model.SelectedEmployeeList)
            {
                var _employee = EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(_checkEmployee.Id);
                if (_employee != null)
                {
                    var employeeData = EmployeeRepository.GetEmployee(_checkEmployee.Id);
                    _employeeList.Add(_employee);
                    attendanceCoverageReportModel.EmployeeCoverageList.Add(new ViewModels.EmployeeAttandaceCoverage()
                    {
                        EmployeeID = employeeData.Id,
                        EmployeeName = employeeData.Name
                    });
                }
            }


            #endregion

            if (model.FromDate < model.ToDate)
            {
                #region ---- Generating the hedder ----

                // Implementation of the report hedder part HTML. 
                DateTime _tempDate = model.FromDate.Value;
                _sb.Append("<h3>Working From Home Task Summary</h3>");
                _sb.Append("<table class=\"summery_report\"><tr>");
                _sb.Append("<td class=\"employee_clm hedder_clm\">Employee Name</td>");

                do
                {
                    _sb.Append(string.Format("<td class=\"hedder_clm\">{0}<sup>{1}</sup> {2}<br />",
                        _tempDate.Day, GetDateSup(_tempDate.Day), GetMonthAbbr(_tempDate.Month)));

                    decimal _plannedMinitsForCurrentDate = _plannedMinitsforDay;
                    if (HolidayRepository.IsHoliday(holidayList, model.FromDate.Value) != null)
                        _plannedMinitsForCurrentDate = 0;

                    //if (_plannedMinitsForCurrentDate > 0)
                    //    _sb.Append(string.Format("<span class=\"plan_span\">Plan : {0} Hrs</span></td>",
                    //         (_plannedMinitsForCurrentDate / 60).ToString("#.##")));
                    //else
                    //    _sb.Append("<span class=\"plan_span\">Plan : 0 Hrs</span></td>");

                    _tempDate = _tempDate.AddDays(1);
                } while (_tempDate <= model.ToDate);
                _sb.Append("<td class=\"hedder_clm\">Total </td>");
                _sb.Append("</tr>");

                #endregion

                #region ---- Generating body for the list of employees ----
                IDictionary<DateTime, int> _totalRowData = new Dictionary<DateTime, int>();
                var empIdList = attendanceCoverageReportModel.EmployeeCoverageList.Select(e => e.EmployeeID).ToList();

                IList<WorkingFromHomeTask> allTasks = dbContext.WorkingFromHomeTasks
                            .Join(dbContext.PendingAttendances, t => t.PendingAttendanceId, p => p.Id, (t, p) => new
                            {
                                WorkingFromHomeTask = t,
                                PendingAttendances = p
                            }).
                            Where(t => empIdList.Contains(t.WorkingFromHomeTask.EmployeeId) &&
                            DbFunctions.TruncateTime(t.WorkingFromHomeTask.Date) >= DbFunctions.TruncateTime(model.FromDate) &&
                            DbFunctions.TruncateTime(t.WorkingFromHomeTask.Date) <= DbFunctions.TruncateTime(model.ToDate) &&
                            t.PendingAttendances.ApproveType == 1).
                            Select(t => t.WorkingFromHomeTask).
                            ToList();

                // Generation of body section for all employees
                // Will generate a single row for each and every
                foreach (var employeeAttendance in attendanceCoverageReportModel.EmployeeCoverageList)
                {
                    _sb.Append(string.Format("<tr><td class=\"nametd\">{0}</td>", employeeAttendance.EmployeeName));
                    _tempDate = model.FromDate.Value.Date;
                    int _employeeTotal = 0;
                    do
                    {

                        int _totalTaskMinits = GetEmployeeTaskMinits(employeeAttendance.EmployeeID, _tempDate, allTasks);
                        //add to the total for the employee
                        _employeeTotal += _totalTaskMinits;

                        string _statusClass = "work_done";

                        if (_totalTaskMinits > 0)
                            _sb.Append(string.Format("<td class=\"hourstd {0}\">{1} </td>", _statusClass,
                                string.Format("{0}:{1}", _totalTaskMinits / 60, _totalTaskMinits % 60)));
                        else
                            _sb.Append(string.Format("<td class=\"hourstd {0}\">0 </td>", _statusClass));

                        //add to summary row
                        var keyExisit = _totalRowData.ContainsKey(_tempDate);
                        if (keyExisit)
                            _totalRowData[_tempDate] += _totalTaskMinits;
                        else
                            _totalRowData[_tempDate] = _totalTaskMinits;

                        _tempDate = _tempDate.AddDays(1);
                    } while (_tempDate <= model.ToDate);
                    _sb.Append(string.Format("<td class=\"hourstd\">{0} </td>",
                        _employeeTotal > 0 ? string.Format("{0}:{1}", _employeeTotal / 60, _employeeTotal % 60) : "0"));
                    _sb.Append("</tr>");
                }

                #region ----Generating total row
                _sb.Append("<tr><td class=\"nametd\">Total </td>");
                _totalRowData.OrderBy(d => d.Key);
                int _daySumTotal = 0;
                foreach (var daySum in _totalRowData)
                {
                    _sb.Append(string.Format("<td class=\"hourstd\">{0} </td>",
                        daySum.Value > 0 ? string.Format("{0}:{1}", daySum.Value / 60, daySum.Value % 60) : "0"));
                    _daySumTotal += daySum.Value;
                }
                _sb.Append(string.Format("<td class=\"hourstd\">{0} </td>",
                    _daySumTotal > 0 ? string.Format("{0}:{1}", _daySumTotal / 60, _daySumTotal % 60) : "0"));
                _sb.Append("</tr>");

                #endregion



                _sb.Append("</table>");

                #endregion

                #region ---- Generating the report summery ----

                //Adding the report summery sections HTML to the reoprt.
                TimeSpan _timeDuration = model.ToDate.Value - model.FromDate.Value;

                _sb.Append(string.Format("<tr><td colspan=\"{0}\" class=\"summerytd\">", (_timeDuration.Days + 2)));
                _sb.Append("<strong>Report summary</strong><br>");
                _sb.Append(string.Format("Report From : {0} To : {1}<br>", model.FromDate.Value.ToShortDateString(),
                    model.ToDate.Value.ToShortDateString()));

                var _daySumTotalHrs = "0";
                var _daySumTotalMnts = "0";

                if (_daySumTotal > 0)
                {
                    _daySumTotalHrs = (_daySumTotal / 60).ToString();
                    _daySumTotalMnts = (_daySumTotal % 60).ToString();
                }

                _sb.Append(string.Format("Total task hours : {0} Hrs {1} Mnts<br>", _daySumTotalHrs, _daySumTotalMnts));
                _sb.Append(string.Format("WFH % from the total attendance : {0}%", ((_daySumTotal / actualOnpremMin) * 100).ToString("#.##")));
                _sb.Append("</td></tr></table>");
                highlightTotalWFHPercentage = ((_daySumTotal / actualOnpremMin) * 100);

                #endregion
            }
            StringBuilder _sb_top = new StringBuilder();
            _sb_top.Append("<h3>Attendance Highlights</h3>");
            _sb_top.Append("<label>" + string.Format("Total Coverage : {0}%<br>", highlightTotalCoverage.ToString("#.##")) + "</label>");
            _sb_top.Append("<label>" + string.Format("WFH % from the total attendance : {0}%", highlightTotalWFHPercentage.ToString("#.##")) + "</label>");
            _sb_top.Append("<seperator>");
            _sb_top.Append(_sb.ToString());
            return _sb_top.ToString();
        }

        internal static void GenerateCoverageReportMonthly(ViewModels.AttendanceReportViewModel model, string reportHTML, string bodyHTML, string rowHTML)
        {

            IList<AttendaceReportStructure> coverageList = new List<AttendaceReportStructure>();

            StringBuilder _attendanceReportSb = new StringBuilder();
            _attendanceReportSb.Append(reportHTML);

            StringBuilder _weekTableBody = new StringBuilder();
            IList<Leave> leaveList = new List<Leave>();
            StringBuilder _employeeRowSb = new StringBuilder();





            DateTime fromDate = new DateTime(model.Year, model.Month, 1);
            DateTime toDate = fromDate.AddMonths(1).AddDays(-1);

            if (toDate >= DateTime.Today)
            {
                toDate = DateTime.Today.AddDays(-1); ;
            }




            var onSiteEmployeeList = OnSiteRepository.GetOnsiteEmployeesByDateRange(fromDate, toDate).Select(x => x.EmployeeId).ToList();

            var Atemployees = EmployeeRepository.GetAllEnableEmployees().Where(x => !onSiteEmployeeList.Contains(x.Id)).OrderBy(a => a.Name).ToList();


            int empcount = Atemployees.Count();
            int CoveredCount = 0;

            foreach (var item in Atemployees)
            {

                #region --- Local Variables----
                EmployeeEnrollment emp = dbContext.EmployeeEnrollment.Single(a => a.EmployeeId == item.Id);

                DateTime firstDayOfMonth = new DateTime(model.Year, model.Month, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                if (lastDayOfMonth >= DateTime.Today)
                {
                    lastDayOfMonth = DateTime.Today.AddDays(-1); ;
                }

                DateTime _tempStartDate = firstDayOfMonth;



                StringBuilder _sb = new StringBuilder();
                StringBuilder _employeeStringBuilder = new StringBuilder();
                attendanceCoverageReportModel = new ViewModels.AttendaceReportStructure();
                _sb.Append(string.Format("{0},{1},{2}\n", "Date", "In", "Out"));
                decimal _plannedMinitsforDay = decimal.Parse(ConfigurationManager.AppSettings["PlannedMinitsPerDay"].ToString());

                int _totalValidDaysForAverageCalculation = 0;

                ViewModels.TimeTrendAnalysisViewModel OutTimemodel = new ViewModels.TimeTrendAnalysisViewModel();

                #endregion
                if (item.DateJoined < lastDayOfMonth)///Check for the Employee out side the rang
                {
                    if (item.DateJoined > firstDayOfMonth)
                    {
                        firstDayOfMonth = item.DateJoined;
                    }


                    if (firstDayOfMonth < lastDayOfMonth)
                    {


                        leaveList = GetLeaveList(firstDayOfMonth, lastDayOfMonth, item);

                        var holidayList = HolidayRepository.GetHolidays(firstDayOfMonth, lastDayOfMonth);

                        do
                        {
                            decimal _plannedMinitsForThisDate = _plannedMinitsforDay;
                            bool _validForInOutAverageCalculation = true;

                            #region --- Check for holidays and leave ---

                            var _holiday = HolidayRepository.IsHoliday(holidayList, firstDayOfMonth);
                            var _leave = LeaveRepository.IsLeave(leaveList, firstDayOfMonth);

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
                                    attendanceCoverageReportModel.TotalPlanned += _plannedMinitsForThisDate;
                                    attendanceCoverageReportModel.WorkingDays = attendanceCoverageReportModel.WorkingDays + 1;
                                }
                            }

                            // Not considering the current date for the average in out calculation
                            if (firstDayOfMonth == Utility.GetDateTimeNow().Date)
                                _validForInOutAverageCalculation = false;

                            if (_validForInOutAverageCalculation)
                                _totalValidDaysForAverageCalculation++;

                            OutTimemodel.FromDate = firstDayOfMonth;
                            OutTimemodel.ToDate = lastDayOfMonth;
                            OutTimemodel.SelectedEmployeeID = item.Id;
                            _sb.Append(GetEmployeesInOutGraphData(OutTimemodel, firstDayOfMonth, _validForInOutAverageCalculation));

                            #endregion

                            firstDayOfMonth = firstDayOfMonth.AddDays(1);
                        } while (firstDayOfMonth <= lastDayOfMonth);


                        #region --- Update average in out time ---

                        int _averageInTimeDevation = 0;
                        int _averageOutTimeDevation = 0;

                        if (_totalValidDaysForAverageCalculation > 0)
                        {
                            _averageInTimeDevation = attendanceCoverageReportModel.TotalDeviationForInTimeAverage / _totalValidDaysForAverageCalculation;
                            _averageOutTimeDevation = attendanceCoverageReportModel.TotalDeviationForOutTimeAverage / _totalValidDaysForAverageCalculation;
                        }


                        DateTime _standardInTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 9, 0, 0);
                        DateTime _standardOutTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 18, 0, 0);
                        DateTime _averageIn = _standardInTime.AddSeconds((_averageInTimeDevation * -1));
                        DateTime _averageOut = _standardOutTime.AddSeconds((_averageOutTimeDevation * -1));


                        attendanceCoverageReportModel.AverageInTime = _averageIn.ToLongTimeString();
                        attendanceCoverageReportModel.AverageOutTime = _averageOut.ToLongTimeString();

                        #endregion


                    }

                    #region --- Calculating the planned leaves within the date range ---

                    foreach (var leave in leaveList)
                    {
                        if (leave.Date.Date >= _tempStartDate.Date && leave.Date.Date <= lastDayOfMonth)
                        {
                            if (leave.LeaveType == LeaveType.FullDay)
                                attendanceCoverageReportModel.TotalPlannedLeave = attendanceCoverageReportModel.TotalPlannedLeave + 1;
                            else
                                attendanceCoverageReportModel.TotalPlannedLeave = attendanceCoverageReportModel.TotalPlannedLeave + (decimal)0.5;
                        }
                    }

                    #endregion


                    decimal TotalOutOfOffice = attendanceCoverageReportModel.EmployeeOutOfOfficeList.Sum(a => a.OutMinits);
                    int hors = (int)(TotalOutOfOffice / 60);
                    int Min = (int)(TotalOutOfOffice % 60);

                    if (attendanceCoverageReportModel.TotalPlanned > 0)
                    {

                        attendanceCoverageReportModel.WorkCoverage = Math.Round((((attendanceCoverageReportModel.TotalActual - TotalOutOfOffice) / attendanceCoverageReportModel.TotalPlanned) * 100), 2);

                    }

                    #region --- Row Generation ---

                    _employeeRowSb.Append(
                         rowHTML.Replace("#@BADGNO", emp.CardNo.ToString())
                        .Replace("#@EMPLOYEENAME", item.Name)
                        .Replace("#@COVERAGE", attendanceCoverageReportModel.WorkCoverage.ToString())
                        .Replace("#@AVERAGEINTIME", attendanceCoverageReportModel.AverageInTime)
                        .Replace("#@AVERAGEOUTTIME", attendanceCoverageReportModel.AverageOutTime)
                        .Replace("#@OUTOFOFFICEHOURS", hors.ToString() + ":" + Min.ToString())
                        );

                    #endregion

                    if (90 < attendanceCoverageReportModel.WorkCoverage)
                    {
                        CoveredCount++;
                    }
                }
            }




            var coveredPercentage = Math.Round((decimal)CoveredCount / empcount, 2) * 100;

            _weekTableBody.Append(bodyHTML.Replace("#@ROWBODY", _employeeRowSb.ToString()));
            _attendanceReportSb = _attendanceReportSb.Replace(string.Format("#@WEEK{0}REPORTBODY", 1), _weekTableBody.ToString());
            _attendanceReportSb = _attendanceReportSb.Replace("#@COVEREDPERCENTAGE", "PERCENTAGE OF 90 % COVERAGE ACHIVED: " + coveredPercentage + "%");
            _attendanceReportSb = _attendanceReportSb.Replace("#@MONTH", model.Month.ToString());
            _attendanceReportSb = _attendanceReportSb.Replace("#@YEAR", model.Year.ToString());



            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Charset = "";
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            HttpContext.Current.Response.AddHeader("Content-Disposition", "inline;filename=CoverageReport.xls");
            HttpContext.Current.Response.Write(_attendanceReportSb.ToString());
            HttpContext.Current.Response.End();
            HttpContext.Current.Response.Flush();
        }

        internal static string GetMonth(int month)
        {
            switch (month)
            {
                case 1:
                    return "January";

                case 2:
                    return "February";
                case 3:
                    return "March";
                case 4:
                    return "April";
                case 5:
                    return "May";
                case 6:
                    return "June";
                case 7:
                    return "July";
                case 8:
                    return "Augest";
                case 9:
                    return "September";
                case 10:
                    return "October";
                case 11:
                    return "November";
                case 12:
                    return "December";

                default:
                    break;
            }

            return null;
        }

        internal static void GenerateTeamCoverageReportMonthly(ViewModels.AttendanceReportViewModel model, string reportHTML, string bodyHTML, string rowHTML)
        {
            string strMonth = GetMonth(model.Month);

            IList<AttendaceReportStructure> coverageList = new List<AttendaceReportStructure>();

            StringBuilder _attendanceReportSb = new StringBuilder();

            reportHTML.Replace("#@Month", strMonth);
            _attendanceReportSb.Append(reportHTML);

            StringBuilder _weekTableBody = new StringBuilder();
            IList<Leave> leaveList = new List<Leave>();
            StringBuilder _employeeRowSb = new StringBuilder();





            DateTime fromDate = new DateTime(model.Year, model.Month, 1);
            DateTime toDate = fromDate.AddMonths(1).AddDays(-1);

            if (toDate >= DateTime.Today)
            {
                toDate = DateTime.Today.AddDays(-1); ;
            }



            IList<int> EmpIdList = TeamManagementRepository.GetTeamMembersDetails(model.TeamId).TeamMembers.Select(a => a.Id).ToList<int>();
            var strTeamName = TeamManagementRepository.getTeamName(model.TeamId);
            var Atemployees = EmployeeRepository.GetAllEnableEmployees().Where(x => EmpIdList.Contains(x.Id)).ToList();
            var onSiteEmployeeList = OnSiteRepository.GetOnsiteEmployeesByDateRange(fromDate, toDate).Select(a => a.EmployeeId).ToList();
            Atemployees = Atemployees.Where(x => !onSiteEmployeeList.Contains(x.Id)).OrderBy(a => a.Name).ToList();

            int empcount = Atemployees.Count();
            int CoveredCount = 0;

            foreach (var item in Atemployees)
            {

                #region --- Local Variables----
                EmployeeEnrollment emp = dbContext.EmployeeEnrollment.Single(a => a.EmployeeId == item.Id);

                DateTime firstDayOfMonth = new DateTime(model.Year, model.Month, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                if (lastDayOfMonth >= DateTime.Today)
                {
                    lastDayOfMonth = DateTime.Today.AddDays(-1); ;
                }

                DateTime _tempStartDate = firstDayOfMonth;



                StringBuilder _sb = new StringBuilder();
                StringBuilder _employeeStringBuilder = new StringBuilder();
                attendanceCoverageReportModel = new ViewModels.AttendaceReportStructure();
                _sb.Append(string.Format("{0},{1},{2}\n", "Date", "In", "Out"));
                decimal _plannedMinitsforDay = decimal.Parse(ConfigurationManager.AppSettings["PlannedMinitsPerDay"].ToString());

                int _totalValidDaysForAverageCalculation = 0;

                ViewModels.TimeTrendAnalysisViewModel OutTimemodel = new ViewModels.TimeTrendAnalysisViewModel();

                #endregion
                if (item.DateJoined < lastDayOfMonth)///Check for the Employee out side the rang
                {
                    if (item.DateJoined > firstDayOfMonth)
                    {
                        firstDayOfMonth = item.DateJoined;
                    }


                    if (firstDayOfMonth < lastDayOfMonth)
                    {


                        leaveList = GetLeaveList(firstDayOfMonth, lastDayOfMonth, item);

                        var holidayList = HolidayRepository.GetHolidays(firstDayOfMonth, lastDayOfMonth);

                        do
                        {
                            decimal _plannedMinitsForThisDate = _plannedMinitsforDay;
                            bool _validForInOutAverageCalculation = true;

                            #region --- Check for holidays and leave ---

                            var _holiday = HolidayRepository.IsHoliday(holidayList, firstDayOfMonth);
                            var _leave = LeaveRepository.IsLeave(leaveList, firstDayOfMonth);

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
                                    attendanceCoverageReportModel.TotalPlanned += _plannedMinitsForThisDate;
                                    attendanceCoverageReportModel.WorkingDays = attendanceCoverageReportModel.WorkingDays + 1;
                                }
                            }

                            // Not considering the current date for the average in out calculation
                            if (firstDayOfMonth == Utility.GetDateTimeNow().Date)
                                _validForInOutAverageCalculation = false;

                            if (_validForInOutAverageCalculation)
                                _totalValidDaysForAverageCalculation++;

                            OutTimemodel.FromDate = firstDayOfMonth;
                            OutTimemodel.ToDate = lastDayOfMonth;
                            OutTimemodel.SelectedEmployeeID = item.Id;
                            _sb.Append(GetEmployeesInOutGraphData(OutTimemodel, firstDayOfMonth, _validForInOutAverageCalculation));

                            #endregion

                            firstDayOfMonth = firstDayOfMonth.AddDays(1);
                        } while (firstDayOfMonth <= lastDayOfMonth);


                        #region --- Update average in out time ---

                        int _averageInTimeDevation = 0;
                        int _averageOutTimeDevation = 0;

                        if (_totalValidDaysForAverageCalculation > 0)
                        {
                            _averageInTimeDevation = attendanceCoverageReportModel.TotalDeviationForInTimeAverage / _totalValidDaysForAverageCalculation;
                            _averageOutTimeDevation = attendanceCoverageReportModel.TotalDeviationForOutTimeAverage / _totalValidDaysForAverageCalculation;
                        }


                        DateTime _standardInTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 9, 0, 0);
                        DateTime _standardOutTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 18, 0, 0);
                        DateTime _averageIn = _standardInTime.AddSeconds((_averageInTimeDevation * -1));
                        DateTime _averageOut = _standardOutTime.AddSeconds((_averageOutTimeDevation * -1));


                        attendanceCoverageReportModel.AverageInTime = _averageIn.ToLongTimeString();
                        attendanceCoverageReportModel.AverageOutTime = _averageOut.ToLongTimeString();

                        #endregion


                    }

                    #region --- Calculating the planned leaves within the date range ---

                    foreach (var leave in leaveList)
                    {
                        if (leave.Date.Date >= _tempStartDate.Date && leave.Date.Date <= lastDayOfMonth)
                        {
                            if (leave.LeaveType == LeaveType.FullDay)
                                attendanceCoverageReportModel.TotalPlannedLeave = attendanceCoverageReportModel.TotalPlannedLeave + 1;
                            else
                                attendanceCoverageReportModel.TotalPlannedLeave = attendanceCoverageReportModel.TotalPlannedLeave + (decimal)0.5;
                        }
                    }

                    #endregion


                    decimal TotalOutOfOffice = attendanceCoverageReportModel.EmployeeOutOfOfficeList.Sum(a => a.OutMinits);
                    int hors = (int)(TotalOutOfOffice / 60);
                    int Min = (int)(TotalOutOfOffice % 60);

                    int horsActual = (int)((attendanceCoverageReportModel.TotalActual - TotalOutOfOffice) / 60);
                    int MinActual = (int)((attendanceCoverageReportModel.TotalActual - TotalOutOfOffice) % 60);

                    int horsRequired = (int)(attendanceCoverageReportModel.TotalPlanned) / 60;
                    int MinRequired = (int)(attendanceCoverageReportModel.TotalPlanned) % 60;

                    if (attendanceCoverageReportModel.TotalPlanned > 0)
                    {

                        attendanceCoverageReportModel.WorkCoverage = Math.Round((((attendanceCoverageReportModel.TotalActual - TotalOutOfOffice) / attendanceCoverageReportModel.TotalPlanned) * 100), 2);

                    }

                    #region --- Row Generation ---

                    _employeeRowSb.Append(
                         rowHTML.Replace("#@BADGNO", emp.CardNo.ToString())
                        .Replace("#@EMPLOYEENAME", item.Name)
                        .Replace("#@COVERAGE", attendanceCoverageReportModel.WorkCoverage.ToString())
                        .Replace("#@AVERAGEINTIME", attendanceCoverageReportModel.AverageInTime)
                        .Replace("#@AVERAGEOUTTIME", attendanceCoverageReportModel.AverageOutTime)
                        .Replace("#@HOURSREQUIRED", horsRequired.ToString() + ":" + MinRequired.ToString())
                        .Replace("#@ACTUALHOURS", horsActual.ToString() + ":" + MinActual.ToString())
                        .Replace("#@OUTOFOFFICEHOURS", hors.ToString() + ":" + Min.ToString())
                        );

                    #endregion

                    if (90 < attendanceCoverageReportModel.WorkCoverage)
                    {
                        CoveredCount++;
                    }
                }
            }




            var coveredPercentage = Math.Round((decimal)CoveredCount / empcount, 2) * 100;

            _weekTableBody.Append(bodyHTML.Replace("#@ROWBODY", _employeeRowSb.ToString()));
            _attendanceReportSb = _attendanceReportSb.Replace(string.Format("#@WEEK{0}REPORTBODY", 1), _weekTableBody.ToString());
            _attendanceReportSb = _attendanceReportSb.Replace("#@COVEREDPERCENTAGE", "PERCENTAGE OF 90 % COVERAGE ACHIVED : " + coveredPercentage + "%");
            _attendanceReportSb = _attendanceReportSb.Replace("#@TEAM", strTeamName);
            _attendanceReportSb = _attendanceReportSb.Replace("#@MONTH", strMonth);
            _attendanceReportSb = _attendanceReportSb.Replace("#@YEAR", DateTime.Today.Year.ToString());



            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Charset = "";
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            HttpContext.Current.Response.AddHeader("Content-Disposition", "inline;filename=CoverageReport.xls");
            HttpContext.Current.Response.Write(_attendanceReportSb.ToString());
            HttpContext.Current.Response.End();
            HttpContext.Current.Response.Flush();
        }

        internal static void GenerateTeamCoverageReportWeekly(ViewModels.AttendanceReportViewModel model, string reportHTML, string bodyHTML, string rowHTML)
        {
            #region --- Local variables ---

            StringBuilder _attendanceReportSb = new StringBuilder();
            _attendanceReportSb.Append(reportHTML);

            IList<ViewModels.WeekStructure> _weekStructureModelList = new List<ViewModels.WeekStructure>();
            DateTime _fromDate = new DateTime(model.Year, model.Month, 1);
            DateTime _toDate = _fromDate.AddMonths(1).AddDays(-1);

            DateTime _tempStartDate = _fromDate;

            if (_toDate >= DateTime.Today)
            {
                _toDate = DateTime.Today.AddDays(-1); ;
            }



            var onSiteEmployeeList = OnSiteRepository.GetOnsiteEmployeesByDateRange(_fromDate, _toDate).Select(a => a.EmployeeId).ToList();

            var _employeeList = EmployeeRepository.GetAllEnableEmployees().Where(x => !onSiteEmployeeList.Contains(x.Id)).ToList();

            IList<int> EmpIdList = TeamManagementRepository.GetTeamMembersDetails(model.TeamId).TeamMembers.Select(a => a.Id).ToList<int>();

            var strTeamName = TeamManagementRepository.getTeamName(model.TeamId);

            _employeeList = _employeeList.Where(x => EmpIdList.Contains(x.Id)).ToList();



            var employeeEnrollmentList = EmployeeEnrollmentRepository.GetEmployeeEnrollments();
            var _attendanceList = dbContext.Attendances.Where(a => a.Year == model.Year && a.Month == model.Month);
            DateTime _tempDate = _fromDate;
            int _plannedMinitsforDay = int.Parse(ConfigurationManager.AppSettings["PlannedMinitsPerDay"].ToString());

            int empcount = _employeeList.Count();

            #endregion


            List<ViewModels.EmployeeProjectStructure> _employeeProjectList = new List<ViewModels.EmployeeProjectStructure>();

            holidayList = HolidayRepository.GetHolidays(_fromDate, _toDate);
            var leaveList = LeaveRepository.GetApprovedLeaveForRpt(_fromDate, _toDate);

            #region --- Creating the week structure ---

            do
            {
                if (_tempDate.Day == 1 || (_tempDate.Day > 4 && _tempDate.DayOfWeek == DayOfWeek.Monday))
                {
                    ViewModels.WeekStructure _weekStr = new ViewModels.WeekStructure()
                    {
                        FromDate = _tempDate.Day,
                        Month = model.Month,
                        Year = model.Year,
                        Weeknumber = _weekStructureModelList.Count + 1
                    };
                    _weekStructureModelList.Add(_weekStr);
                }
                else
                    _weekStructureModelList[_weekStructureModelList.Count - 1].ToDate = _tempDate.Day;

                _tempDate = _tempDate.AddDays(1);
            } while (_tempDate < _toDate);

            _weekStructureModelList[_weekStructureModelList.Count - 1].ToDate = _toDate.Day;

            #endregion

            // For the generated week structure neeed to generate 
            // Seperate tabs in the excel sheet.



            foreach (var week in _weekStructureModelList)
            {

                DateTime _reportDateFrom = new DateTime(model.Year, model.Month, week.FromDate);
                DateTime _reportDateTo = new DateTime(model.Year, model.Month, week.ToDate);

                string _weekHedder = string.Format("Coverage Report - Day {0}-{1}", _reportDateFrom.ToString("dd-MM-yyyy"), _reportDateTo.ToString("dd-MM-yyyy"));
                StringBuilder _weekTableBody = new StringBuilder();
                StringBuilder _employeeRowSb = new StringBuilder();
                int CoveredCount = 0;
                foreach (var employee in _employeeList)
                {

                    attendanceCoverageReportModel = new ViewModels.AttendaceReportStructure();
                    DateTime DateFrom = new DateTime(model.Year, model.Month, week.FromDate);
                    DateTime DateTo = new DateTime(model.Year, model.Month, week.ToDate);
                    EmployeeEnrollment emp = dbContext.EmployeeEnrollment.Single(a => a.EmployeeId == employee.Id);

                    if (employee.DateJoined < DateTo)///Check for the Employee out side the rang
                    {

                        if (employee.DateJoined > DateFrom)
                        {
                            DateFrom = employee.DateJoined;
                        }

                        if (employee.DateJoined <= DateFrom)
                        {
                            // Add seperate rows for each employee from the selection.
                            int _totalValidDaysForAverageCalculation = 0;
                            //    //  var holidayList = HolidayRepository.GetHolidays(firstDayOfMonth, lastDayOfMonth);
                            ViewModels.TimeTrendAnalysisViewModel OutTimemodel = new ViewModels.TimeTrendAnalysisViewModel();
                            do
                            {
                                decimal _plannedMinitsForThisDate = _plannedMinitsforDay;
                                bool _validForInOutAverageCalculation = true;

                                #region --- Check for holidays and leave ---

                                var _holiday = HolidayRepository.IsHoliday(holidayList, DateFrom);
                                var _leave = LeaveRepository.IsLeave(leaveList, DateFrom, employee.Id);

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
                                        attendanceCoverageReportModel.TotalPlanned += _plannedMinitsForThisDate;
                                        attendanceCoverageReportModel.WorkingDays = attendanceCoverageReportModel.WorkingDays + 1;
                                    }
                                }

                                // Not considering the current date for the average in out calculation
                                if (DateFrom == Utility.GetDateTimeNow().Date)
                                    _validForInOutAverageCalculation = false;

                                if (_validForInOutAverageCalculation)
                                    _totalValidDaysForAverageCalculation++;

                                OutTimemodel.FromDate = DateFrom;
                                OutTimemodel.ToDate = DateTo;
                                OutTimemodel.SelectedEmployeeID = employee.Id;
                                GetEmployeesInOutGraphData(OutTimemodel, DateFrom, _validForInOutAverageCalculation);

                                #endregion

                                DateFrom = DateFrom.AddDays(1);
                            } while (DateFrom <= DateTo);


                            #region --- Update average in out time ---

                            int _averageInTimeDevation = 0;
                            int _averageOutTimeDevation = 0;

                            if (_totalValidDaysForAverageCalculation > 0)
                            {
                                _averageInTimeDevation = attendanceCoverageReportModel.TotalDeviationForInTimeAverage / _totalValidDaysForAverageCalculation;
                                _averageOutTimeDevation = attendanceCoverageReportModel.TotalDeviationForOutTimeAverage / _totalValidDaysForAverageCalculation;
                            }


                            DateTime _standardInTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 9, 0, 0);
                            DateTime _standardOutTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 18, 0, 0);
                            DateTime _averageIn = _standardInTime.AddSeconds((_averageInTimeDevation * -1));
                            DateTime _averageOut = _standardOutTime.AddSeconds((_averageOutTimeDevation * -1));


                            attendanceCoverageReportModel.AverageInTime = _averageIn.ToLongTimeString();
                            attendanceCoverageReportModel.AverageOutTime = _averageOut.ToLongTimeString();

                            #endregion




                            #region --- Calculating the planned leaves within the date range ---

                            foreach (var leave in leaveList)
                            {
                                if (leave.Date.Date >= _tempStartDate.Date && leave.Date.Date <= DateTo)
                                {
                                    if (leave.LeaveType == LeaveType.FullDay)
                                        attendanceCoverageReportModel.TotalPlannedLeave = attendanceCoverageReportModel.TotalPlannedLeave + 1;
                                    else
                                        attendanceCoverageReportModel.TotalPlannedLeave = attendanceCoverageReportModel.TotalPlannedLeave + (decimal)0.5;
                                }
                            }

                            #endregion


                            decimal TotalOutOfOffice = attendanceCoverageReportModel.EmployeeOutOfOfficeList.Sum(a => a.OutMinits);
                            int hors = (int)(TotalOutOfOffice / 60);
                            int Min = (int)(TotalOutOfOffice % 60);

                            int horsActual = (int)((attendanceCoverageReportModel.TotalActual - TotalOutOfOffice) / 60);
                            int MinActual = (int)((attendanceCoverageReportModel.TotalActual - TotalOutOfOffice) % 60);

                            int horsRequired = (int)(attendanceCoverageReportModel.TotalPlanned) / 60;
                            int MinRequired = (int)(attendanceCoverageReportModel.TotalPlanned) % 60;

                            if (attendanceCoverageReportModel.TotalPlanned > 0)
                            {

                                attendanceCoverageReportModel.WorkCoverage = Math.Round((((attendanceCoverageReportModel.TotalActual - TotalOutOfOffice) / attendanceCoverageReportModel.TotalPlanned) * 100), 2);

                            }

                            #region --- Row Generation ---




                            _employeeRowSb.Append(
                        rowHTML.Replace("#@BADGNO", emp.CardNo.ToString())
                       .Replace("#@EMPLOYEENAME", employee.Name)
                       .Replace("#@COVERAGE", attendanceCoverageReportModel.WorkCoverage.ToString())
                       .Replace("#@AVERAGEINTIME", attendanceCoverageReportModel.AverageInTime)
                       .Replace("#@AVERAGEOUTTIME", attendanceCoverageReportModel.AverageOutTime)
                       .Replace("#@HOURSREQUIRED", horsRequired.ToString() + ":" + MinRequired.ToString())
                       .Replace("#@ACTUALHOURS", horsActual.ToString() + ":" + MinActual.ToString())
                       .Replace("#@OUTOFOFFICEHOURS", hors.ToString() + ":" + Min.ToString())
                       );

                            #endregion
                            if (90 < attendanceCoverageReportModel.WorkCoverage)
                            {
                                CoveredCount++;
                            }
                        }
                    }/////End of Checking for the Employee out side the rang


                }

                var coveragePercentage = (Math.Round((decimal)CoveredCount / empcount, 2) * 100);


                _weekTableBody.Append(bodyHTML.Replace("#@ROWBODY", _employeeRowSb.ToString())
                  .Replace("#@ReportHedderWeek", string.Format("Coverage Report 0f " + strTeamName + "- From {0}  To  {1}", _reportDateFrom.ToString("dd-MM-yyyy"), _reportDateTo.ToString("dd-MM-yyyy"))));

                _attendanceReportSb = _attendanceReportSb.Replace(string.Format("#@WEEK{0}REPORTBODY", week.Weeknumber.ToString()), _weekTableBody.ToString());

                _attendanceReportSb = _attendanceReportSb.Replace("#@COVEREDPERCENTAGE", "PERCENTAGE OF 90 % COVERAGE ACHIVED : " + coveragePercentage + "%");


            }

            #region --- Clear the empty tabs ---

            for (int i = 1; i <= 6; i++)
                if (i > _weekStructureModelList.Count)
                {
                    _attendanceReportSb = _attendanceReportSb.Replace(string.Format("#@WEEK{0}REPORTBODY", i.ToString()), string.Empty);

                }

            #endregion

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Charset = "";
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            HttpContext.Current.Response.AddHeader("Content-Disposition", "inline;filename=AttendanceReport.xls");
            HttpContext.Current.Response.Write(_attendanceReportSb.ToString());
            HttpContext.Current.Response.End();


        }

        public static void GenerateCoverageReportYearly(ViewModels.AttendanceReportViewModel model, string reportHTML, string bodyHTML, string rowHTML)
        {

            IList<AttendaceReportStructure> coverageList = new List<AttendaceReportStructure>();

            StringBuilder _attendanceReportSb = new StringBuilder();
            _attendanceReportSb.Append(reportHTML);

            StringBuilder _weekTableBody = new StringBuilder();
            IList<Leave> leaveList = new List<Leave>();
            StringBuilder _employeeRowSb = new StringBuilder();
            _employeeRowSb.Append("<table  style='border-style: outset;'><tr><td style='font-size: 30px;font-weight: bold:text-align: center' colspan='6'>Annual coverage Report -  " + model.Year + " </td ><tr>"
             + "<tr><td style='font-size: 15px;font-weight: bold;text-align: left;'>Card No.</td><td style='font-size: 15px;font-weight: bold;text-align: left;'>Employee Name</td><td style='font-size: 15px;font-weight: bold;text-align: center;'>Coverage(%)</td><td style='font-size: 15px;font-weight: bold;text-align: center;'>Average In Time</td>"
             + "<td style='font-size: 15px;font-weight: bold;text-align: center;'>Average Out Time</td><td style='font-size: 15px;font-weight: bold;text-align: center;'>Out Of Office hrs</td> </tr>");
            var Atemployees = EmployeeRepository.GetAllEmployees().Where(a => a.IsEnable).OrderBy(a => a.Name);
            foreach (var item in Atemployees)
            {

                #region --- Local Variables----
                EmployeeEnrollment emp = dbContext.EmployeeEnrollment.Single(a => a.EmployeeId == item.Id);
                var firstDayOfMonth = new DateTime(model.Year, 1, 1);
                if ((item.DateJoined != null) && (item.DateJoined > firstDayOfMonth))
                {
                    firstDayOfMonth = item.DateJoined;
                }
                DateTime _tempStartDate = firstDayOfMonth;

                var lastDayOfMonth = new DateTime(model.Year, 12, 31);


                //if ((item.DateResigned != null) && (item.DateResigned < lastDayOfMonth))
                //{
                //    lastDayOfMonth = item.DateResigned;
                //}

                StringBuilder _sb = new StringBuilder();
                StringBuilder _employeeStringBuilder = new StringBuilder();
                attendanceCoverageReportModel = new ViewModels.AttendaceReportStructure();
                _sb.Append(string.Format("{0},{1},{2}\n", "Date", "In", "Out"));
                decimal _plannedMinitsforDay = decimal.Parse(ConfigurationManager.AppSettings["PlannedMinitsPerDay"].ToString());

                int _totalValidDaysForAverageCalculation = 0;

                ViewModels.TimeTrendAnalysisViewModel OutTimemodel = new ViewModels.TimeTrendAnalysisViewModel();

                #endregion


                if (firstDayOfMonth < lastDayOfMonth)
                {


                    leaveList = GetLeaveList(firstDayOfMonth, lastDayOfMonth, item);

                    var holidayList = HolidayRepository.GetHolidays(firstDayOfMonth, lastDayOfMonth);

                    do
                    {
                        decimal _plannedMinitsForThisDate = _plannedMinitsforDay;
                        bool _validForInOutAverageCalculation = true;

                        #region --- Check for holidays and leave ---

                        var _holiday = HolidayRepository.IsHoliday(holidayList, firstDayOfMonth);
                        var _leave = LeaveRepository.IsLeave(leaveList, firstDayOfMonth);

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
                                attendanceCoverageReportModel.TotalPlanned += _plannedMinitsForThisDate;
                                attendanceCoverageReportModel.WorkingDays = attendanceCoverageReportModel.WorkingDays + 1;
                            }
                        }

                        // Not considering the current date for the average in out calculation
                        if (firstDayOfMonth == Utility.GetDateTimeNow().Date)
                            _validForInOutAverageCalculation = false;

                        if (_validForInOutAverageCalculation)
                            _totalValidDaysForAverageCalculation++;

                        OutTimemodel.FromDate = firstDayOfMonth;
                        OutTimemodel.ToDate = lastDayOfMonth;
                        OutTimemodel.SelectedEmployeeID = item.Id;
                        _sb.Append(GetEmployeesInOutGraphData(OutTimemodel, firstDayOfMonth, _validForInOutAverageCalculation));

                        #endregion

                        firstDayOfMonth = firstDayOfMonth.AddDays(1);
                    } while (firstDayOfMonth <= lastDayOfMonth);


                    #region --- Update average in out time ---

                    int _averageInTimeDevation = 0;
                    int _averageOutTimeDevation = 0;

                    if (_totalValidDaysForAverageCalculation > 0)
                    {
                        _averageInTimeDevation = attendanceCoverageReportModel.TotalDeviationForInTimeAverage / _totalValidDaysForAverageCalculation;
                        _averageOutTimeDevation = attendanceCoverageReportModel.TotalDeviationForOutTimeAverage / _totalValidDaysForAverageCalculation;
                    }


                    DateTime _standardInTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 9, 0, 0);
                    DateTime _standardOutTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 18, 0, 0);
                    DateTime _averageIn = _standardInTime.AddSeconds((_averageInTimeDevation * -1));
                    DateTime _averageOut = _standardOutTime.AddSeconds((_averageOutTimeDevation * -1));


                    attendanceCoverageReportModel.AverageInTime = _averageIn.ToLongTimeString();
                    attendanceCoverageReportModel.AverageOutTime = _averageOut.ToLongTimeString();

                    #endregion


                }

                #region --- Calculating the planned leaves within the date range ---

                foreach (var leave in leaveList)
                {
                    if (leave.Date.Date >= _tempStartDate.Date && leave.Date.Date <= lastDayOfMonth)
                    {
                        if (leave.LeaveType == LeaveType.FullDay)
                            attendanceCoverageReportModel.TotalPlannedLeave = attendanceCoverageReportModel.TotalPlannedLeave + 1;
                        else
                            attendanceCoverageReportModel.TotalPlannedLeave = attendanceCoverageReportModel.TotalPlannedLeave + (decimal)0.5;
                    }
                }

                #endregion


                decimal TotalOutOfOffice = attendanceCoverageReportModel.EmployeeOutOfOfficeList.Sum(a => a.OutMinits);
                int hors = (int)(TotalOutOfOffice / 60);
                int Min = (int)(TotalOutOfOffice % 60);

                if (attendanceCoverageReportModel.TotalPlanned > 0)
                {

                    attendanceCoverageReportModel.WorkCoverage = Math.Round((((attendanceCoverageReportModel.TotalActual - TotalOutOfOffice) / attendanceCoverageReportModel.TotalPlanned) * 100), 2);

                }

                #region --- Row Generation ---

                if (attendanceCoverageReportModel.WorkCoverage < 80)
                {
                    _employeeRowSb.Append("<tr><td style='color:red;text-align: left;'>" + emp.CardNo.ToString() + "</td><td style='color:red;text-align: left;'>" + item.Name + "</td><td style='color:red;text-align: center;'>" + attendanceCoverageReportModel.WorkCoverage.ToString()
                    + "</td><td style='color:red;text-align: center;'>" + attendanceCoverageReportModel.AverageInTime + "</td>" + "<td style='color:red;text-align: center;'>" + attendanceCoverageReportModel.AverageOutTime
                    + "</td><td style='color:red;text-align: center;'>" + hors.ToString() + ":" + Min.ToString() + "</td> </tr>");

                }
                else if (attendanceCoverageReportModel.WorkCoverage < 90)
                {

                    _employeeRowSb.Append("<tr><td style='color:orange;text-align: left;'>" + emp.CardNo.ToString() + "</td><td style='color:orange;text-align: left;'>" + item.Name + "</td><td style='color:orange;text-align: center;'>" + attendanceCoverageReportModel.WorkCoverage.ToString()
                    + "</td><td style='color:orange;text-align: center;'>" + attendanceCoverageReportModel.AverageInTime + "</td>" + "<td style='color:::;text-align: center;'>" + attendanceCoverageReportModel.AverageOutTime
                    + "</td><td style='color:orange;text-align: center;'>" + hors.ToString() + ":" + Min.ToString() + "</td> </tr>");
                }
                else
                {
                    _employeeRowSb.Append("<tr><td style='color:green;text-align: left;'>" + emp.CardNo.ToString() + "</td><td style='color:green;text-align: left;'>" + item.Name + "</td><td style='color:green;text-align: center;'>" + attendanceCoverageReportModel.WorkCoverage.ToString()
                    + "</td><td style='color:green;text-align: center;'>" + attendanceCoverageReportModel.AverageInTime + "</td>" + "<td style='color:green;text-align: center;'>" + attendanceCoverageReportModel.AverageOutTime
                    + "</td><td style='color:green;text-align: center;'>" + hors.ToString() + ":" + Min.ToString() + "</td> </tr>");

                }


                #endregion
            }
            _employeeRowSb.Append("</table>");

            sendReport(_employeeRowSb.ToString());
        }


        public static void GenerateCoverageSummaryMonthly(ViewModels.AttendanceReportViewModel model, string reportHTML, string bodyHTML, string rowHTML)
        {
            StringBuilder _attendanceReportSb = new StringBuilder();
            _attendanceReportSb.Append(reportHTML);

            DateTime fromDate = new DateTime(model.Year, model.Month, Utility.GetDateTimeNow().Day).AddMonths(-1).AddDays(-1);
            DateTime toDate = new DateTime(model.Year, model.Month, Utility.GetDateTimeNow().Day).AddDays(-1);

            if (model.Year <= Utility.GetDateTimeNow().Year)
            {
                if (model.Month < Utility.GetDateTimeNow().Month)
                {
                    fromDate = new DateTime(model.Year, model.Month, 1);
                    var lastDayOfMonth = DateTime.DaysInMonth(model.Year, model.Month);
                    toDate = new DateTime(model.Year, model.Month, lastDayOfMonth);
                }

            }


            IList<BackgroundSlaves> BackgroundSlavesList =
            Repositories.BacgroungRepository.getBackgroundSlaveProcesData(
            toDate.Year, toDate.Month, toDate.Day).OrderByDescending(x => x.Coverage).ToList();


            string achiedCoverage =
             Repositories.BacgroungRepository.getBackgroundMasterProcesData(
             toDate.Year, toDate.Month, toDate.Day).AchievedCoverage.ToString();

            string companyCoverage =
            Repositories.BacgroungRepository.getBackgroundMasterProcesData(
            toDate.Year, toDate.Month, toDate.Day).CompanyCoverage.ToString();

            string _weekHedder = string.Format("Coverage Report company summary");


            StringBuilder _weekTableBody = new StringBuilder();
            StringBuilder _employeeRowSb = new StringBuilder();
            var BackgroundSlavesList100 = BackgroundSlavesList.Where(a => a.Coverage >= 100).ToList();
            foreach (var item in BackgroundSlavesList100)
            {

                var emp = Repositories.EmployeeRepository.GetEmployee(item.EmployeeId);
                var EmployeeName = emp.Name;
                var Coverage = item.Coverage;

                _employeeRowSb.Append(
                           rowHTML.Replace("#@EMPLOYEENAME", emp.Name)
                          .Replace("#@COVERAGE", item.Coverage.ToString()));

            }

            _weekTableBody.Append(bodyHTML.Replace("#@ROWBODY", _employeeRowSb.ToString())
                  .Replace("#@ReportHedderWeek", _weekHedder));


            _attendanceReportSb = _attendanceReportSb.Replace("#@WEEK1REPORTBODY", _weekTableBody.ToString());
            _attendanceReportSb = _attendanceReportSb.Replace("#@COMPANYCOVERAGE", companyCoverage + "%");
            _attendanceReportSb = _attendanceReportSb.Replace("#90PERCENTACHIEVED", achiedCoverage + "%");
            _attendanceReportSb = _attendanceReportSb.Replace("#@EMPLOYEECOUNT", BackgroundSlavesList100.Count.ToString());
            _attendanceReportSb = _attendanceReportSb.Replace("#@EMPLOYEEPERCENTAGE", Math.Round((decimal)BackgroundSlavesList100.Count / BackgroundSlavesList.Count, 2) * 100 + "%");
            _weekTableBody = new StringBuilder();
            _employeeRowSb = new StringBuilder();
            var BackgroundSlavesList90100 = BackgroundSlavesList.Where(a => a.Coverage >= 90 && a.Coverage < 100).ToList();
            foreach (var item in BackgroundSlavesList90100)
            {

                var emp = Repositories.EmployeeRepository.GetEmployee(item.EmployeeId);
                _employeeRowSb.Append(
                           rowHTML.Replace("#@EMPLOYEENAME", emp.Name)
                          .Replace("#@COVERAGE", item.Coverage.ToString()));

            }

            _weekTableBody.Append(bodyHTML.Replace("#@ROWBODY", _employeeRowSb.ToString())
                  .Replace("#@ReportHedderWeek", _weekHedder));

            _attendanceReportSb = _attendanceReportSb.Replace("#@WEEK2REPORTBODY", _weekTableBody.ToString());
            _attendanceReportSb = _attendanceReportSb.Replace("#@COMPANYCOVERAGE", companyCoverage + "%");
            _attendanceReportSb = _attendanceReportSb.Replace("#90PERCENTACHIEVED", achiedCoverage + "%");
            _attendanceReportSb = _attendanceReportSb.Replace("#@EMPLOYEECOUNT", BackgroundSlavesList90100.Count.ToString());
            _attendanceReportSb = _attendanceReportSb.Replace("#@EMPLOYEEPERCENTAGE", Math.Round((decimal)BackgroundSlavesList90100.Count / BackgroundSlavesList.Count, 2) * 100 + "%");
            _weekTableBody = new StringBuilder();
            _employeeRowSb = new StringBuilder();
            var BackgroundSlavesList90 = BackgroundSlavesList.Where(a => a.Coverage < 90).ToList();
            foreach (var item in BackgroundSlavesList90)
            {

                var emp = Repositories.EmployeeRepository.GetEmployee(item.EmployeeId);
                var EmployeeName = emp.Name;
                var Coverage = item.Coverage;

                _employeeRowSb.Append(
                           rowHTML.Replace("#@EMPLOYEENAME", emp.Name)
                          .Replace("#@COVERAGE", item.Coverage.ToString()));

            }

            _weekTableBody.Append(bodyHTML.Replace("#@ROWBODY", _employeeRowSb.ToString())
                  .Replace("#@ReportHedderWeek", _weekHedder));

            _attendanceReportSb = _attendanceReportSb.Replace("#@WEEK3REPORTBODY", _weekTableBody.ToString());
            _attendanceReportSb = _attendanceReportSb.Replace("#@COMPANYCOVERAGE", companyCoverage + "%");
            _attendanceReportSb = _attendanceReportSb.Replace("#90PERCENTACHIEVED", achiedCoverage + "%");
            _attendanceReportSb = _attendanceReportSb.Replace("#@EMPLOYEECOUNT", BackgroundSlavesList90.Count.ToString());
            _attendanceReportSb = _attendanceReportSb.Replace("#@EMPLOYEEPERCENTAGE", Math.Round((decimal)BackgroundSlavesList90.Count / BackgroundSlavesList.Count, 2) * 100 + "%");

            #region --- Clear the empty tabs ---    
            for (int i = 1; i <= 4; i++)
                if (i > 3)
                {
                    _attendanceReportSb = _attendanceReportSb.Replace(string.Format("#@WEEK{0}REPORTBODY", i.ToString()), string.Empty);

                }

            #endregion

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Charset = "";
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            HttpContext.Current.Response.AddHeader("Content-Disposition", "inline;filename=AttendanceReport.xls");
            HttpContext.Current.Response.Write(_attendanceReportSb.ToString());
            HttpContext.Current.Response.End();

        }

        public static void GenerateWorkingFromHome(ViewModels.AttendanceReportViewModel model, string reportHTML, string bodyHTML, string rowHTML)
        {
            StringBuilder _attendanceReportSb = new StringBuilder();
            _attendanceReportSb.Append(reportHTML);


            string _weekHedder = string.Format("Working from home" + " on  " + model.Day + " - " + model.Month + " - " + model.Year);


            StringBuilder _weekTableBody = new StringBuilder();
            StringBuilder _employeeRowSb = new StringBuilder();


            var Employees = EmployeeRepository.GetAllEmployees();
            var myLeaveEmployeList = LeaveRepository.GetMyLeaveService(new DateTime(model.Year, model.Month, model.Day), new DateTime(model.Year, model.Month, model.Day));
            var attendenceList = dbContext.PendingAttendances.Where(a => a.Year == model.Year && a.Month == model.Month && a.Day == model.Day).ToList();
            var attendencePendingList = attendenceList.Where(a => a.ApproveType == 0).ToList();
            foreach (var item in attendencePendingList)
            {

                var emp = Employees.Where(e => e.Id == item.EmployeeId).FirstOrDefault();
                var EmployeeName = emp.Name;
                string hours = "0";
                if (item.OutHour > item.InHour)
                {
                    hours = (item.OutHour - item.InHour).ToString();
                }
                string Min = "00";
                if (item.OutMinute > item.InMinute)
                {
                    Min = (item.OutMinute - item.InMinute).ToString();
                }

                _employeeRowSb.Append(
                           rowHTML.Replace("#@EMPLOYEENAME", emp.Name)
                           .Replace("#@HOURSWORKED", hours + ":" + Min));

            }

            _weekTableBody.Append(bodyHTML.Replace("#@ROWBODY", _employeeRowSb.ToString())
                  .Replace("#@ReportHedderWeek", _weekHedder));


            _attendanceReportSb = _attendanceReportSb.Replace("#@WEEK1REPORTBODY", _weekTableBody.ToString());
            _attendanceReportSb = _attendanceReportSb.Replace("#@EMPLOYEECOUNT", attendencePendingList.Count.ToString());
            _weekTableBody = new StringBuilder();
            _employeeRowSb = new StringBuilder();

            var attendenceApprovedList = attendenceList.Where(a => a.ApproveType == 1).ToList();
            foreach (var item in attendenceApprovedList)
            {

                var emp = Employees.Where(e => e.Id == item.EmployeeId).FirstOrDefault();
                var EmployeeName = emp.Name;
                string hours = "0";
                if (item.OutHour > item.InHour)
                {
                    hours = (item.OutHour - item.InHour).ToString();
                }
                string Min = "00";
                if (item.OutMinute > item.InMinute)
                {
                    Min = (item.OutMinute - item.InMinute).ToString();
                }

                _employeeRowSb.Append(
                           rowHTML.Replace("#@EMPLOYEENAME", emp.Name)
                           .Replace("#@HOURSWORKED", hours + ":" + Min));

            }

            _weekTableBody.Append(bodyHTML.Replace("#@ROWBODY", _employeeRowSb.ToString())
                  .Replace("#@ReportHedderWeek", _weekHedder));

            _attendanceReportSb = _attendanceReportSb.Replace("#@WEEK2REPORTBODY", _weekTableBody.ToString());
            _attendanceReportSb = _attendanceReportSb.Replace("#@EMPLOYEECOUNT", attendenceApprovedList.Count.ToString());

            _weekTableBody = new StringBuilder();
            _employeeRowSb = new StringBuilder();
            var attendenceRejectedList = attendenceList.Where(a => a.ApproveType == 2).ToList();
            foreach (var item in attendenceRejectedList)
            {

                var emp = Employees.Where(e => e.Id == item.EmployeeId).FirstOrDefault();
                var EmployeeName = emp.Name;
                string hours = "0";
                if (item.OutHour > item.InHour)
                {
                    hours = (item.OutHour - item.InHour).ToString();
                }
                string Min = "00";
                if (item.OutMinute > item.InMinute)
                {
                    Min = (item.OutMinute - item.InMinute).ToString();
                }

                _employeeRowSb.Append(
                           rowHTML.Replace("#@EMPLOYEENAME", emp.Name)
                           .Replace("#@HOURSWORKED", hours + ":" + Min));

            }

            _weekTableBody.Append(bodyHTML.Replace("#@ROWBODY", _employeeRowSb.ToString())
                  .Replace("#@ReportHedderWeek", _weekHedder));

            _attendanceReportSb = _attendanceReportSb.Replace("#@WEEK3REPORTBODY", _weekTableBody.ToString());
            _attendanceReportSb = _attendanceReportSb.Replace("#@EMPLOYEECOUNT", attendenceRejectedList.Count.ToString());

            _weekTableBody = new StringBuilder();
            _employeeRowSb = new StringBuilder();

            DateTime _fromDate = new DateTime(model.Year, model.Month, model.Day);
            var onSiteEmployeeList = OnSiteRepository.GetOnsiteEmployeesByDateRange(new DateTime(model.Year, model.Month, model.Day), new DateTime(model.Year, model.Month, model.Day)).Select(a => a.EmployeeId).ToList();

            var employeeList = EmployeeRepository.GetAllEnableEmployees().Where(x => !onSiteEmployeeList.Contains(x.Id)).ToList();

            var notReportedEmployeeList = employeeList.Where(y => !attendenceList.Select(a => a.EmployeeId).Contains(y.Id)).ToList();

            foreach (var item in notReportedEmployeeList)
            {

                var emp = Employees.Where(e => e.Id == item.Id).FirstOrDefault();
                var EmployeeName = emp.Name;


                var Isleave = myLeaveEmployeList.Where(a => a.user == emp.PrimaryEmailAddress).ToList();
                if (Isleave.Any())
                {
                    _employeeRowSb.Append(
                             rowHTML.Replace("#@EMPLOYEENAME", emp.Name)
                            .Replace("#@HOURSWORKED", "Leave"));
                }
                else
                {
                    _employeeRowSb.Append(
                            rowHTML.Replace("#@EMPLOYEENAME", emp.Name)
                           .Replace("#@HOURSWORKED", string.Empty));
                }

            }

            _weekTableBody.Append(bodyHTML.Replace("#@ROWBODY", _employeeRowSb.ToString())
                  .Replace("#@ReportHedderWeek", _weekHedder));

            _attendanceReportSb = _attendanceReportSb.Replace("#@WEEK4REPORTBODY", _weekTableBody.ToString());
            _attendanceReportSb = _attendanceReportSb.Replace("#@EMPLOYEECOUNT", notReportedEmployeeList.Count.ToString());

            #region --- Clear the empty tabs ---
            for (int i = 1; i <= 5; i++)
                if (i > 4)
                {
                    _attendanceReportSb = _attendanceReportSb.Replace(string.Format("#@WEEK{0}REPORTBODY", i.ToString()), string.Empty);

                }

            #endregion

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Charset = "";
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            HttpContext.Current.Response.AddHeader("Content-Disposition", "inline;filename=AttendanceReport.xls");
            HttpContext.Current.Response.Write(_attendanceReportSb.ToString());
            HttpContext.Current.Response.End();

        }


        //[DelphiAuthentication]
        public static string sendReport(string message)
        {


            var employeeId = int.Parse(System.Web.HttpContext.Current.Session["EmployeeId"].ToString());
            EmployeeData _loggedUser = EmployeeRepository.GetEmployee(employeeId);

            var host = ConfigurationManager.AppSettings["MailHost"];
            var port = ConfigurationManager.AppSettings["MailPort"];
            var fromAddress = ConfigurationManager.AppSettings["FromAddress"];
            var Username = ConfigurationManager.AppSettings["MailUserName"];
            var password = ConfigurationManager.AppSettings["MailPassword"];
            var toAddress = ConfigurationManager.AppSettings["CommentsSendTo"];
            var CCAddress = ConfigurationManager.AppSettings["CommentsCCTo"];

            var smtpClient = new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,
                Host = host,
                Port = Convert.ToInt32(port)
            };

            var credentials = new System.Net.NetworkCredential(Username, password);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = credentials;



            var mail = new MailMessage
            {
                IsBodyHtml = true,
                Body = message + " <br><br><br> / " + _loggedUser.Name,
                Subject = "Annual Coverage Report",
                From = new MailAddress(fromAddress, "ITS")

            };

            mail.To.Add(new MailAddress(_loggedUser.PrimaryEmailAddress));

            string _message = "";
            try
            {
                smtpClient.Send(mail);
                _message = "Successful";
            }
            catch (Exception Ex)
            {
                _message = "Error occurred while sending your comment.";
                throw;
            }

            return _message;
        }

        internal static void GenerateCoverageReportWeekly(ViewModels.AttendanceReportViewModel model, string reportHTML, string bodyHTML, string rowHTML)
        {
            #region --- Local variables ---

            StringBuilder _attendanceReportSb = new StringBuilder();
            _attendanceReportSb.Append(reportHTML);

            IList<ViewModels.WeekStructure> _weekStructureModelList = new List<ViewModels.WeekStructure>();
            DateTime _fromDate = new DateTime(model.Year, model.Month, 1);
            DateTime _toDate = _fromDate.AddMonths(1).AddDays(-1);

            DateTime _tempStartDate = _fromDate;

            if (_toDate >= DateTime.Today)
            {
                _toDate = DateTime.Today.AddDays(-1); ;
            }




            var onSiteEmployeeList = OnSiteRepository.GetOnsiteEmployeesByDateRange(_fromDate, _toDate).Select(a => a.EmployeeId).ToList();

            var _employeeList = EmployeeRepository.GetAllEnableEmployees().Where(x => !onSiteEmployeeList.Contains(x.Id)).ToList();


            var employeeEnrollmentList = EmployeeEnrollmentRepository.GetEmployeeEnrollments();
            var _attendanceList = dbContext.Attendances.Where(a => a.Year == model.Year && a.Month == model.Month);
            DateTime _tempDate = _fromDate;
            int _plannedMinitsforDay = int.Parse(ConfigurationManager.AppSettings["PlannedMinitsPerDay"].ToString());


            #endregion


            List<ViewModels.EmployeeProjectStructure> _employeeProjectList = new List<ViewModels.EmployeeProjectStructure>();

            holidayList = HolidayRepository.GetHolidays(_fromDate, _toDate);
            var leaveList = LeaveRepository.GetApprovedLeaveForRpt(_fromDate, _toDate);

            #region --- Creating the week structure ---

            do
            {
                if (_tempDate.Day == 1 || (_tempDate.Day > 4 && _tempDate.DayOfWeek == DayOfWeek.Monday))
                {
                    ViewModels.WeekStructure _weekStr = new ViewModels.WeekStructure()
                    {
                        FromDate = _tempDate.Day,
                        Month = model.Month,
                        Year = model.Year,
                        Weeknumber = _weekStructureModelList.Count + 1
                    };
                    _weekStructureModelList.Add(_weekStr);
                }
                else
                    _weekStructureModelList[_weekStructureModelList.Count - 1].ToDate = _tempDate.Day;

                _tempDate = _tempDate.AddDays(1);
            } while (_tempDate < _toDate);

            _weekStructureModelList[_weekStructureModelList.Count - 1].ToDate = _toDate.Day;

            #endregion

            // For the generated week structure neeed to generate 
            // Seperate tabs in the excel sheet.

            foreach (var week in _weekStructureModelList)
            {

                DateTime _reportDateFrom = new DateTime(model.Year, model.Month, week.FromDate);
                DateTime _reportDateTo = new DateTime(model.Year, model.Month, week.ToDate);

                string _weekHedder = string.Format("Coverage Report - Day {0}-{1}", _reportDateFrom.ToString("dd-MM-yyyy"), _reportDateTo.ToString("dd-MM-yyyy"));
                StringBuilder _weekTableBody = new StringBuilder();
                StringBuilder _employeeRowSb = new StringBuilder();
                foreach (var employee in _employeeList)
                {
                    attendanceCoverageReportModel = new ViewModels.AttendaceReportStructure();
                    DateTime DateFrom = new DateTime(model.Year, model.Month, week.FromDate);
                    DateTime DateTo = new DateTime(model.Year, model.Month, week.ToDate);
                    EmployeeEnrollment emp = dbContext.EmployeeEnrollment.Single(a => a.EmployeeId == employee.Id);

                    if (employee.DateJoined < DateTo)///Check for the Employee out side the rang
                    {

                        if (employee.DateJoined > DateFrom)
                        {
                            DateFrom = employee.DateJoined;
                        }

                        if (employee.DateJoined <= DateFrom)
                        {
                            // Add seperate rows for each employee from the selection.
                            int _totalValidDaysForAverageCalculation = 0;
                            //    //  var holidayList = HolidayRepository.GetHolidays(firstDayOfMonth, lastDayOfMonth);
                            ViewModels.TimeTrendAnalysisViewModel OutTimemodel = new ViewModels.TimeTrendAnalysisViewModel();
                            do
                            {
                                decimal _plannedMinitsForThisDate = _plannedMinitsforDay;
                                bool _validForInOutAverageCalculation = true;

                                #region --- Check for holidays and leave ---

                                var _holiday = HolidayRepository.IsHoliday(holidayList, DateFrom);
                                var _leave = LeaveRepository.IsLeave(leaveList, DateFrom, employee.Id);

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
                                        attendanceCoverageReportModel.TotalPlanned += _plannedMinitsForThisDate;
                                        attendanceCoverageReportModel.WorkingDays = attendanceCoverageReportModel.WorkingDays + 1;
                                    }
                                }

                                // Not considering the current date for the average in out calculation
                                if (DateFrom == Utility.GetDateTimeNow().Date)
                                    _validForInOutAverageCalculation = false;

                                if (_validForInOutAverageCalculation)
                                    _totalValidDaysForAverageCalculation++;

                                OutTimemodel.FromDate = DateFrom;
                                OutTimemodel.ToDate = DateTo;
                                OutTimemodel.SelectedEmployeeID = employee.Id;
                                GetEmployeesInOutGraphData(OutTimemodel, DateFrom, _validForInOutAverageCalculation);

                                #endregion

                                DateFrom = DateFrom.AddDays(1);
                            } while (DateFrom <= DateTo);


                            #region --- Update average in out time ---

                            int _averageInTimeDevation = 0;
                            int _averageOutTimeDevation = 0;

                            if (_totalValidDaysForAverageCalculation > 0)
                            {
                                _averageInTimeDevation = attendanceCoverageReportModel.TotalDeviationForInTimeAverage / _totalValidDaysForAverageCalculation;
                                _averageOutTimeDevation = attendanceCoverageReportModel.TotalDeviationForOutTimeAverage / _totalValidDaysForAverageCalculation;
                            }


                            DateTime _standardInTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 9, 0, 0);
                            DateTime _standardOutTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 18, 0, 0);
                            DateTime _averageIn = _standardInTime.AddSeconds((_averageInTimeDevation * -1));
                            DateTime _averageOut = _standardOutTime.AddSeconds((_averageOutTimeDevation * -1));


                            attendanceCoverageReportModel.AverageInTime = _averageIn.ToLongTimeString();
                            attendanceCoverageReportModel.AverageOutTime = _averageOut.ToLongTimeString();

                            #endregion




                            #region --- Calculating the planned leaves within the date range ---

                            foreach (var leave in leaveList)
                            {
                                if (leave.Date.Date >= _tempStartDate.Date && leave.Date.Date <= DateTo)
                                {
                                    if (leave.LeaveType == LeaveType.FullDay)
                                        attendanceCoverageReportModel.TotalPlannedLeave = attendanceCoverageReportModel.TotalPlannedLeave + 1;
                                    else
                                        attendanceCoverageReportModel.TotalPlannedLeave = attendanceCoverageReportModel.TotalPlannedLeave + (decimal)0.5;
                                }
                            }

                            #endregion


                            decimal TotalOutOfOffice = attendanceCoverageReportModel.EmployeeOutOfOfficeList.Sum(a => a.OutMinits);
                            int hors = (int)(TotalOutOfOffice / 60);
                            int Min = (int)(TotalOutOfOffice % 60);

                            if (attendanceCoverageReportModel.TotalPlanned > 0)
                            {

                                attendanceCoverageReportModel.WorkCoverage = Math.Round((((attendanceCoverageReportModel.TotalActual - TotalOutOfOffice) / attendanceCoverageReportModel.TotalPlanned) * 100), 2);

                            }

                            #region --- Row Generation ---

                            _employeeRowSb.Append(

                                rowHTML.Replace("#@BADGNO", emp.CardNo.ToString())
                               .Replace("#@EMPLOYEENAME", employee.Name)
                               .Replace("#@COVERAGE", attendanceCoverageReportModel.WorkCoverage.ToString())
                               .Replace("#@AVERAGEINTIME", attendanceCoverageReportModel.AverageInTime)
                               .Replace("#@AVERAGEOUTTIME", attendanceCoverageReportModel.AverageOutTime)
                               .Replace("#@OUTOFOFFICEHOURS", hors.ToString() + ":" + Min.ToString())
                               );

                            #endregion
                        }
                    }/////End of Checking for the Employee out side the rang
                }

                _weekTableBody.Append(bodyHTML.Replace("#@ROWBODY", _employeeRowSb.ToString())
                  .Replace("#@ReportHedderWeek", string.Format("Coverage Report - Day From {0}  To  {1}", _reportDateFrom.ToString("dd-MM-yyyy"), _reportDateTo.ToString("dd-MM-yyyy"))));


                _attendanceReportSb = _attendanceReportSb.Replace(string.Format("#@WEEK{0}REPORTBODY", week.Weeknumber.ToString()), _weekTableBody.ToString());

            }

            #region --- Clear the empty tabs ---

            for (int i = 1; i <= 6; i++)
                if (i > _weekStructureModelList.Count)
                    _attendanceReportSb = _attendanceReportSb.Replace(string.Format("#@WEEK{0}REPORTBODY", i.ToString()), string.Empty);

            #endregion

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Charset = "";
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            HttpContext.Current.Response.AddHeader("Content-Disposition", "inline;filename=AttendanceReport.xls");
            HttpContext.Current.Response.Write(_attendanceReportSb.ToString());
            HttpContext.Current.Response.End();


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

            var _firstInAttendance = attendanceList.FirstOrDefault(a => a.EmployeeId == model.SelectedEmployeeID);//Get the both in/Out Card pucn records for Time calculation.....

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

        internal static IList<Leave> GetLeaveList(DateTime dateFrom, DateTime dateTo, EmployeeData user)
        {
            IList<Leave> leaveList = new List<Leave>();
            var leaveListSer = LeaveRepository.GetMyLeaveService(dateFrom, dateTo);
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
        /// Generates the attandance report for the given month and date.
        /// Opens the Excel from the responce...
        /// </summary>
        /// <param name="model">View model from the client with parametes</param>
        /// <param name="reportHTML">Main Excel report HTML</param>
        /// <param name="bodyHTML">Tab body HTML</param>
        /// <param name="rowHTML">Table Row HTML</param>
        internal static void GenerateAttandanceReport(ViewModels.AttendanceReportViewModel model, string reportHTML, string bodyHTML, string rowHTML)
        {
            #region --- Local variables ---

            StringBuilder _attendanceReportSb = new StringBuilder();
            _attendanceReportSb.Append(reportHTML);

            IList<ViewModels.WeekStructure> _weekStructureModelList = new List<ViewModels.WeekStructure>();
            DateTime _fromDate = new DateTime(model.Year, model.Month, 1);
            DateTime _toDate = _fromDate.AddMonths(1).AddDays(-1);
            var _employeeList = EmployeeRepository.GetAllEmployeeOrderByName().Where(a => a.IsEnable == true);
            var employeeEnrollmentList = EmployeeEnrollmentRepository.GetEmployeeEnrollments();
            var _attendanceList = dbContext.Attendances.Where(a => a.Year == model.Year && a.Month == model.Month);
            DateTime _tempDate = _fromDate;
            int _totalActualMinits = 0;

            int _totalPlannedMinits = 0;
            int _plannedMinitsforDay = int.Parse(ConfigurationManager.AppSettings["PlannedMinitsPerDay"].ToString());
            List<ViewModels.EmployeeProjectStructure> _employeeProjectList = new List<ViewModels.EmployeeProjectStructure>();
            holidayList = HolidayRepository.GetHolidays(_fromDate, _toDate);


            #endregion

            #region --- Creating the week structure ---

            do
            {
                if (_tempDate.Day == 1 || (_tempDate.Day > 4 && _tempDate.DayOfWeek == DayOfWeek.Monday))
                {
                    ViewModels.WeekStructure _weekStr = new ViewModels.WeekStructure()
                    {
                        FromDate = _tempDate.Day,
                        Month = model.Month,
                        Year = model.Year,
                        Weeknumber = _weekStructureModelList.Count + 1
                    };
                    _weekStructureModelList.Add(_weekStr);
                }
                else
                    _weekStructureModelList[_weekStructureModelList.Count - 1].ToDate = _tempDate.Day;

                _tempDate = _tempDate.AddDays(1);
            } while (_tempDate < _toDate);

            _weekStructureModelList[_weekStructureModelList.Count - 1].ToDate = _toDate.Day;

            #endregion

            var leaveList = LeaveRepository.GetApprovedLeaveForRpt(_fromDate, _toDate);

            // For the generated week structure neeed to generate 
            // Seperate tabs in the excel sheet.
            foreach (var week in _weekStructureModelList)
            {
                string _weekHedder = string.Format("Attendance Report - Day {0}-{1}", week.FromDate.ToString(), week.ToDate.ToString());
                StringBuilder _weekTableBody = new StringBuilder();
                _totalPlannedMinits = 0;
                _totalActualMinits = 0;

                StringBuilder _employeeRowSb = new StringBuilder();
                // Add seperate rows for each employee from the selection.
                foreach (var employee in _employeeList)
                {
                    EmployeeEnrollment emp = dbContext.EmployeeEnrollment.Single(a => a.EmployeeId == employee.Id);


                    for (int i = week.FromDate; i <= week.ToDate; i++)
                    {
                        DateTime _reportDate = new DateTime(model.Year, model.Month, i);
                        if (employee.DateJoined != null && employee.DateJoined <= _reportDate)
                        {
                            var _employeeAttendanceListNotOrdered = _attendanceList.Where(a => a.EmployeeId == employee.Id
                                                                                     && a.Day == i).ToList();

                            var _employeeAttendanceList = _employeeAttendanceListNotOrdered.OrderBy(a => a.EmployeeId).ThenBy(a => a.Year).ThenBy(a => a.Month).
                                ThenBy(a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second).ToList();

                            #region ---- Parameters Init ----

                            string _badgeNumberStr = string.Empty;
                            string _nameStr = string.Empty;


                            string _dateStr = string.Empty;
                            string _dayOwWeekStrStr = string.Empty;

                            string _firstInStr = string.Empty;
                            string _lastOutStr = string.Empty;
                            string _actualWorkStr = string.Empty;
                            string _actualOutofOfficeStr = string.Empty;
                            string _leaveTypeStr = string.Empty;
                            string _leaveSatusStr = string.Empty;
                            string _remarkStr = string.Empty;
                            int _hours = 0;
                            int _mins = 0;

                            #endregion

                            #region --- Updating Parameter Values ---


                            _badgeNumberStr = emp.CardNo.ToString();
                            _nameStr = employee.Name;



                            _dateStr = _reportDate.ToString("dd/MM/yyyy");
                            _dayOwWeekStrStr = _reportDate.DayOfWeek.ToString();


                            DateTime? _firstInTime = null;
                            DateTime? _lastOutTime = null;
                            int _actualWorkMinits = GetActualWorkedMinitsForDay(_employeeAttendanceList, employee.Id, _reportDate.Day,
                                 ref _firstInTime, ref _lastOutTime);

                            int _outOfOfficeMinits = GetOutOfOfficeHourForDay(_employeeAttendanceList, employee.Id, _reportDate.Day,
                                 ref _firstInTime, ref _lastOutTime);

                            int _estimatedMinits = GetEstimateMinitsForDay(_plannedMinitsforDay, _reportDate, employee.Id, ref _leaveTypeStr, ref _leaveSatusStr, ref _remarkStr, leaveList);

                            if (_firstInTime != null)
                                _firstInStr = string.Format("{0}:{1}", _firstInTime.Value.Hour.ToString("#"), _firstInTime.Value.Minute.ToString("##"));
                            if (_lastOutTime != null)
                                _lastOutStr = string.Format("{0}:{1}", _lastOutTime.Value.Hour.ToString("#"), _lastOutTime.Value.Minute.ToString("##"));

                            if (_actualWorkMinits > 0)
                            {
                                _hours = _actualWorkMinits / 60;
                                _mins = _actualWorkMinits % 60;
                                _actualWorkStr = string.Format("{0}:{1}", _hours.ToString(), _mins.ToString("##"));
                                // _actualWorkStr = Math.Round(_actualWorkMinits / 60.00, 2).ToString();
                            }

                            if (_outOfOfficeMinits > 0)
                            {
                                _hours = _outOfOfficeMinits / 60;
                                _mins = _outOfOfficeMinits % 60;
                                _actualOutofOfficeStr = string.Format("{0}:{1}", _hours.ToString(), _mins.ToString("##"));

                            }

                            _totalPlannedMinits += _estimatedMinits;
                            _totalActualMinits += _actualWorkMinits;

                            #endregion

                            #region --- Row Generation ---

                            _employeeRowSb.Append(rowHTML.Replace("#@BADGNO", _badgeNumberStr)
                                .Replace("#@BADGNO", _badgeNumberStr)
                                .Replace("#@EMPLOYEENAME", _nameStr)
                                .Replace("#@ROWDATE", _dateStr)
                                .Replace("#@ROWDAY", _dayOwWeekStrStr)
                                .Replace("#@INTIME", _firstInStr)
                                .Replace("#@OUTTIME", _lastOutStr)
                                .Replace("#@ACTUALWORKHOURS", _actualWorkStr)
                                .Replace("#@OUTOFOFFICEHOURS", _actualOutofOfficeStr)
                                .Replace("#@LEAVETYPE", _leaveTypeStr)
                                .Replace("#@REMARKS", _remarkStr));

                            #endregion
                        }
                    }
                }

                #region --- Summery Variable Init ---

                int _estimatedHours = _totalPlannedMinits / 60;
                int _estimatedMins = _totalPlannedMinits % 60;
                int _actualHours = _totalActualMinits / 60;
                int _actualMins = _totalActualMinits % 60;

                int _totalDifferentInMinits = Math.Abs(_totalActualMinits - _totalPlannedMinits);
                string _differneceSign = _totalActualMinits < _totalPlannedMinits ? "-" : string.Empty;
                int _differentHours = _totalDifferentInMinits / 60;
                int _differentMins = _totalDifferentInMinits % 60;

                #endregion

                // Append all the generated variables and generate the main report HTML
                _weekTableBody.Append(bodyHTML.Replace("#@ROWBODY", _employeeRowSb.ToString())
                    .Replace("#@ReportHedderWeek", string.Format("Attendance Report - Day {0}-{1}", week.FromDate.ToString(), week.ToDate.ToString()))

                    //.Replace("#@ACTUALWORKHOURS", string.Format("{0}.{1}", _actualHours.ToString(), _actualMins.ToString("##")))
                    //.Replace("#@STANDARDWORKHOURS", string.Format("{0}.{1}", _estimatedHours.ToString(), _estimatedMins.ToString("##")))
                    //*****Due to chathurika Gunawardena's request we change the number format on 2-18-2015.
                    .Replace("#@ACTUALWORKHOURS", Math.Round(_totalActualMinits / 60.00, 2).ToString())
                    .Replace("#@STANDARDWORKHOURS", Math.Round(_totalPlannedMinits / 60.00, 2).ToString())


                    .Replace("#@DIFFERENCEPRECENTAGE", string.Format("{0} {1}.{2}", _differneceSign, _differentHours.ToString(), _differentMins.ToString("##"))));

                _attendanceReportSb = _attendanceReportSb.Replace(string.Format("#@WEEK{0}REPORTBODY", week.Weeknumber.ToString()), _weekTableBody.ToString());
            }

            #region --- Clear the empty tabs ---

            for (int i = 1; i <= 6; i++)
                if (i > _weekStructureModelList.Count)
                    _attendanceReportSb = _attendanceReportSb.Replace(string.Format("#@WEEK{0}REPORTBODY", i.ToString()), string.Empty);

            #endregion

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Charset = "";
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            HttpContext.Current.Response.AddHeader("Content-Disposition", "inline;filename=AttendanceReport.xls");
            HttpContext.Current.Response.Write(_attendanceReportSb.ToString());
            HttpContext.Current.Response.End();
            HttpContext.Current.Response.Flush();
        }
        public static IList<AttendanceDataViewModel> GenerateDailyAttandanceData(DateTime date)
        {
            #region --- Local variables ---           
            IList<AttendanceDataViewModel> attendanceDataViewModelList = new List<AttendanceDataViewModel>();
            var _employeeList = EmployeeRepository.GetAllEmployeeOrderByName().Where(a => a.IsEnable == true);
            var employeeEnrollmentList = EmployeeEnrollmentRepository.GetEmployeeEnrollments();

            int _totalActualMinits = 0;

            int _totalPlannedMinits = 0;
            int _plannedMinitsforDay = int.Parse(ConfigurationManager.AppSettings["PlannedMinitsPerDay"].ToString());
            List<ViewModels.EmployeeProjectStructure> _employeeProjectList = new List<ViewModels.EmployeeProjectStructure>();
            holidayList = HolidayRepository.GetHolidays(date, date);

            IList<Attendance> _attendanceList = dbContext.Attendances.Where(a => a.Year == date.Year && a.Month == date.Month).ToList();
            var attendenceListNotInOffice = dbContext.PendingAttendances.Where(a => a.Year == date.Year && a.Month == date.Month && a.Day == date.Day && a.ApproveType == 0).ToList();

            foreach (var item in attendenceListNotInOffice)
            {

                _attendanceList.Add(new Attendance
                {
                    EmployeeId = item.EmployeeId,
                    CardNo = item.CardNo,
                    Year = item.Year,
                    Month = item.Month,
                    Day = item.Day,
                    Hour = item.InHour,
                    Minute = item.InMinute,
                    Second = item.InSecond,
                    VerifyMode = 0,
                    InOutMode = "in",
                    WorkCode = 1,
                    LocationId = 27
                });



                _attendanceList.Add(new Attendance
                {
                    EmployeeId = item.EmployeeId,
                    CardNo = item.CardNo,
                    Year = item.Year,
                    Month = item.Month,
                    Day = item.Day,
                    Hour = item.OutHour,
                    Minute = item.OutMinute,
                    Second = item.OutSecond,
                    VerifyMode = 0,
                    InOutMode = "out",
                    WorkCode = 1,
                    LocationId = 27
                });

            }
            var leaveList = LeaveRepository.GetApprovedLeaveForRpt(date, date);



            _totalPlannedMinits = 0;
            _totalActualMinits = 0;



            #endregion --- Local variables ---

            foreach (var employee in _employeeList)
            {
                EmployeeEnrollment emp = dbContext.EmployeeEnrollment.Single(a => a.EmployeeId == employee.Id);


                DateTime _reportDate = new DateTime(date.Year, date.Month, date.Day);
                if (employee.DateJoined != null && employee.DateJoined <= _reportDate)
                {
                    var _employeeAttendanceListNotOrdered = _attendanceList.Where(a => a.EmployeeId == employee.Id
                                                                             && a.Day == date.Day).ToList();

                    var _employeeAttendanceList = _employeeAttendanceListNotOrdered.OrderBy(a => a.EmployeeId).ThenBy(a => a.Year).ThenBy(a => a.Month).
                        ThenBy(a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second).ToList();

                    #region ---- Parameters Init ----

                    string _badgeNumberStr = string.Empty;
                    string _nameStr = string.Empty;


                    string _dateStr = string.Empty;
                    string _dayOwWeekStrStr = string.Empty;

                    string _firstInStr = string.Empty;
                    string _lastOutStr = string.Empty;
                    string _actualWorkStr = string.Empty;
                    string _actualOutofOfficeStr = string.Empty;
                    string _leaveTypeStr = string.Empty;
                    string _leaveStatusStr = string.Empty;
                    string _leaveCategoryStr = string.Empty;
                    string _remarkStr = string.Empty;
                    int _hours = 0;
                    int _mins = 0;

                    #endregion

                    #region --- Updating Parameter Values ---

                    #region ---- Work status Calculation ----
                    string _workAtOfficeStr = "No";
                    string _workAtHomeStr = "No";
                    string _workCodeRemarksStr = string.Empty;

                    if (_employeeAttendanceList.Where(a => a.LocationId != 27).Any())
                    {
                        _workAtOfficeStr = "Yes";
                    }

                    if (_employeeAttendanceList.Where(a => a.LocationId == 27).Any())
                    {
                        _workAtHomeStr = "Yes";
                    }
                    #endregion



                    _badgeNumberStr = emp.CardNo.ToString();
                    _nameStr = employee.Name;



                    _dateStr = _reportDate.ToString("dd/MM/yyyy");
                    _dayOwWeekStrStr = _reportDate.DayOfWeek.ToString();

                    _leaveCategoryStr = "not available";

                    DateTime? _firstInTime = null;
                    DateTime? _lastOutTime = null;
                    int _actualWorkMinits = GetActualWorkedMinitsForDay(_employeeAttendanceList, employee.Id, _reportDate.Day,
                         ref _firstInTime, ref _lastOutTime);

                    int _outOfOfficeMinits = GetOutOfOfficeHourForDay(_employeeAttendanceList, employee.Id, _reportDate.Day,
                         ref _firstInTime, ref _lastOutTime);

                    int _estimatedMinits = GetEstimateMinitsForDay(_plannedMinitsforDay, _reportDate, employee.Id, ref _leaveTypeStr, ref _leaveStatusStr, ref _remarkStr, leaveList);

                    #region ---- Remarks Calculation ----

                    if (string.IsNullOrEmpty(_leaveTypeStr))
                    {
                        if (_employeeAttendanceList.Any())
                        {
                            if (_employeeAttendanceList.Where(a => a.LocationId == 27 && a.WorkCode == 0).Any())
                            {
                                _workCodeRemarksStr = "Working from home is Approved.";
                            }
                            else
                            {
                                _workCodeRemarksStr = " Working from home is Pending.";
                            }
                        }
                        else
                        {

                            _workCodeRemarksStr = "Attendence are not reported.";
                        }
                    }
                    else
                    {
                        if (_employeeAttendanceList.Where(a => a.LocationId == 27 && a.WorkCode == 0).Any())
                        {
                            _workCodeRemarksStr = "Leave is " + _leaveStatusStr + "and Working from home is Approved";
                        }

                        else if (_employeeAttendanceList.Where(a => a.LocationId == 27 && a.WorkCode == 1).Any())
                        {
                            _workCodeRemarksStr = "Leave is " + _leaveStatusStr + " and Working from home is Pending";

                        }
                        else
                        {
                            _workCodeRemarksStr = "Leave is " + _leaveStatusStr;
                        }
                    }
                    #endregion


                    if (_firstInTime != null)
                        _firstInStr = string.Format("{0}:{1}", _firstInTime.Value.Hour.ToString("#"), _firstInTime.Value.Minute.ToString("##"));
                    if (_lastOutTime != null)
                        _lastOutStr = string.Format("{0}:{1}", _lastOutTime.Value.Hour.ToString("#"), _lastOutTime.Value.Minute.ToString("##"));

                    if (_actualWorkMinits > 0)
                    {
                        _hours = _actualWorkMinits / 60;
                        _mins = _actualWorkMinits % 60;
                        _actualWorkStr = string.Format("{0}:{1}", _hours.ToString(), _mins.ToString("##"));
                        // _actualWorkStr = Math.Round(_actualWorkMinits / 60.00, 2).ToString();
                    }

                    if (_outOfOfficeMinits > 0)
                    {
                        _hours = _outOfOfficeMinits / 60;
                        _mins = _outOfOfficeMinits % 60;
                        _actualOutofOfficeStr = string.Format("{0}:{1}", _hours.ToString(), _mins.ToString("##"));

                    }

                    _totalPlannedMinits += _estimatedMinits;
                    _totalActualMinits += _actualWorkMinits;

                    #endregion

                    #region --- Row Generation ---

                    //_employeeRowSb.Append(rowHTML.Replace("#@BADGNO", _badgeNumberStr)
                    //    .Replace("#@BADGNO", _badgeNumberStr)
                    //    .Replace("#@EMPLOYEENAME", _nameStr)
                    //    .Replace("#@INTIME", _firstInStr)
                    //    .Replace("#@OUTTIME", _lastOutStr)
                    //    .Replace("#@ACTUALWORKHOURS", _actualWorkStr)
                    //    .Replace("#@OUTOFOFFICEHOURS", _actualOutofOfficeStr)
                    //     .Replace("#@LEAVECATEGORY", _leaveCategoryStr)
                    //    .Replace("#@LEAVETYPE", _leaveTypeStr)
                    //    .Replace("#@WORKATOFFICE", _workAtOfficeStr)
                    //    .Replace("#@WORKATHOME", _workAtHomeStr)
                    //    .Replace("#@REMARKS", _workCodeRemarksStr));

                    attendanceDataViewModelList.Add(new AttendanceDataViewModel()
                    {
                        CardNumber = _badgeNumberStr,
                        EmployeeId = employee.Id.ToString(),
                        EmployeeName = _nameStr,
                        InTime = _firstInStr,
                        OutTime = _lastOutStr,
                        ActualHours = _actualWorkStr,
                        OutOfOfficeHours = _actualOutofOfficeStr,
                        LeaveType = _leaveTypeStr,
                        WorkeAtOffice = _workAtOfficeStr,
                        WorkeAtHome = _workAtHomeStr,
                        Remarks = _workCodeRemarksStr
                    });

                    #endregion
                }

            }


            return attendanceDataViewModelList;


        }
        /// <summary>
        /// Generates the attandance report for the given month and date.
        /// Opens the Excel from the responce...
        /// </summary>
        /// <param name="model">View model from the client with parametes</param>
        /// <param name="reportHTML">Main Excel report HTML</param>
        /// <param name="bodyHTML">Tab body HTML</param>
        /// <param name="rowHTML">Table Row HTML</param>
        internal static void GenerateDailyAttandanceReport(ViewModels.AttendanceReportViewModel model, string reportHTML, string bodyHTML, string rowHTML)
        {
            #region --- Local variables ---

            StringBuilder _attendanceReportSb = new StringBuilder();
            _attendanceReportSb.Append(reportHTML);


            DateTime _date = new DateTime(model.Year, model.Month, model.Day);

            var _employeeList = EmployeeRepository.GetAllEmployeeOrderByName().Where(a => a.IsEnable == true);
            var employeeEnrollmentList = EmployeeEnrollmentRepository.GetEmployeeEnrollments();

            int _totalActualMinits = 0;

            int _totalPlannedMinits = 0;
            int _plannedMinitsforDay = int.Parse(ConfigurationManager.AppSettings["PlannedMinitsPerDay"].ToString());
            List<ViewModels.EmployeeProjectStructure> _employeeProjectList = new List<ViewModels.EmployeeProjectStructure>();
            holidayList = HolidayRepository.GetHolidays(_date, _date);

            IList<Attendance> _attendanceList = dbContext.Attendances.Where(a => a.Year == model.Year && a.Month == model.Month).ToList();
            var attendenceListNotInOffice = dbContext.PendingAttendances.Where(a => a.Year == model.Year && a.Month == model.Month && a.Day == model.Day && a.ApproveType == 0).ToList();

            foreach (var item in attendenceListNotInOffice)
            {

                _attendanceList.Add(new Attendance
                {
                    EmployeeId = item.EmployeeId,
                    CardNo = item.CardNo,
                    Year = item.Year,
                    Month = item.Month,
                    Day = item.Day,
                    Hour = item.InHour,
                    Minute = item.InMinute,
                    Second = item.InSecond,
                    VerifyMode = 0,
                    InOutMode = "in",
                    WorkCode = 1,
                    LocationId = 27
                });



                _attendanceList.Add(new Attendance
                {
                    EmployeeId = item.EmployeeId,
                    CardNo = item.CardNo,
                    Year = item.Year,
                    Month = item.Month,
                    Day = item.Day,
                    Hour = item.OutHour,
                    Minute = item.OutMinute,
                    Second = item.OutSecond,
                    VerifyMode = 0,
                    InOutMode = "out",
                    WorkCode = 1,
                    LocationId = 27
                });

            }
            var leaveList = LeaveRepository.GetApprovedLeaveForRpt(_date, _date);


            string _weekHedder = string.Format("Attendance Report for the {0} {1} {2} {3}", _date.DayOfWeek, _date.Day, _date.Month, _date.Year);
            StringBuilder _weekTableBody = new StringBuilder();
            _totalPlannedMinits = 0;
            _totalActualMinits = 0;

            StringBuilder _employeeRowSb = new StringBuilder();
            // Add seperate rows for each employee from the selection.


            #endregion --- Local variables ---

            foreach (var employee in _employeeList)
            {
                EmployeeEnrollment emp = dbContext.EmployeeEnrollment.Single(a => a.EmployeeId == employee.Id);


                DateTime _reportDate = new DateTime(model.Year, model.Month, model.Day);
                if (employee.DateJoined != null && employee.DateJoined <= _reportDate)
                {
                    var _employeeAttendanceListNotOrdered = _attendanceList.Where(a => a.EmployeeId == employee.Id
                                                                             && a.Day == model.Day).ToList();

                    var _employeeAttendanceList = _employeeAttendanceListNotOrdered.OrderBy(a => a.EmployeeId).ThenBy(a => a.Year).ThenBy(a => a.Month).
                        ThenBy(a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second).ToList();

                    #region ---- Parameters Init ----

                    string _badgeNumberStr = string.Empty;
                    string _nameStr = string.Empty;


                    string _dateStr = string.Empty;
                    string _dayOwWeekStrStr = string.Empty;

                    string _firstInStr = string.Empty;
                    string _lastOutStr = string.Empty;
                    string _actualWorkStr = string.Empty;
                    string _actualOutofOfficeStr = string.Empty;
                    string _leaveTypeStr = string.Empty;
                    string _leaveStatusStr = string.Empty;
                    string _leaveCategoryStr = string.Empty;
                    string _remarkStr = string.Empty;
                    int _hours = 0;
                    int _mins = 0;

                    #endregion

                    #region --- Updating Parameter Values ---

                    #region ---- Work status Calculation ----
                    string _workAtOfficeStr = "No";
                    string _workAtHomeStr = "No";
                    string _workCodeRemarksStr = string.Empty;

                    if (_employeeAttendanceList.Where(a => a.LocationId != 27).Any())
                    {
                        _workAtOfficeStr = "Yes";
                    }

                    if (_employeeAttendanceList.Where(a => a.LocationId == 27).Any())
                    {
                        _workAtHomeStr = "Yes";
                    }
                    #endregion



                    _badgeNumberStr = emp.CardNo.ToString();
                    _nameStr = employee.Name;



                    _dateStr = _reportDate.ToString("dd/MM/yyyy");
                    _dayOwWeekStrStr = _reportDate.DayOfWeek.ToString();

                    _leaveCategoryStr = "not available";

                    DateTime? _firstInTime = null;
                    DateTime? _lastOutTime = null;
                    int _actualWorkMinits = GetActualWorkedMinitsForDay(_employeeAttendanceList, employee.Id, _reportDate.Day,
                         ref _firstInTime, ref _lastOutTime);

                    int _outOfOfficeMinits = GetOutOfOfficeHourForDay(_employeeAttendanceList, employee.Id, _reportDate.Day,
                         ref _firstInTime, ref _lastOutTime);

                    int _estimatedMinits = GetEstimateMinitsForDay(_plannedMinitsforDay, _reportDate, employee.Id, ref _leaveTypeStr, ref _leaveStatusStr, ref _remarkStr, leaveList);

                    #region ---- Remarks Calculation ----

                    if (string.IsNullOrEmpty(_leaveTypeStr))
                    {
                        if (_employeeAttendanceList.Any())
                        {
                            if (_employeeAttendanceList.Where(a => a.LocationId == 27 && a.WorkCode == 0).Any())
                            {
                                _workCodeRemarksStr = "Working from home is Approved.";
                            }
                            else
                            {
                                _workCodeRemarksStr = " Working from home is Pending.";
                            }
                        }
                        else
                        {

                            _workCodeRemarksStr = "Attendence are not reported.";
                        }
                    }
                    else
                    {
                        if (_employeeAttendanceList.Where(a => a.LocationId == 27 && a.WorkCode == 0).Any())
                        {
                            _workCodeRemarksStr = "Leave is " + _leaveStatusStr + "and Working from home is Approved";
                        }

                        else if (_employeeAttendanceList.Where(a => a.LocationId == 27 && a.WorkCode == 1).Any())
                        {
                            _workCodeRemarksStr = "Leave is " + _leaveStatusStr + " and Working from home is Pending";

                        }
                        else
                        {
                            _workCodeRemarksStr = "Leave is " + _leaveStatusStr;
                        }
                    }
                    #endregion


                    if (_firstInTime != null)
                        _firstInStr = string.Format("{0}:{1}", _firstInTime.Value.Hour.ToString("#"), _firstInTime.Value.Minute.ToString("##"));
                    if (_lastOutTime != null)
                        _lastOutStr = string.Format("{0}:{1}", _lastOutTime.Value.Hour.ToString("#"), _lastOutTime.Value.Minute.ToString("##"));

                    if (_actualWorkMinits > 0)
                    {
                        _hours = _actualWorkMinits / 60;
                        _mins = _actualWorkMinits % 60;
                        _actualWorkStr = string.Format("{0}:{1}", _hours.ToString(), _mins.ToString("##"));
                        // _actualWorkStr = Math.Round(_actualWorkMinits / 60.00, 2).ToString();
                    }

                    if (_outOfOfficeMinits > 0)
                    {
                        _hours = _outOfOfficeMinits / 60;
                        _mins = _outOfOfficeMinits % 60;
                        _actualOutofOfficeStr = string.Format("{0}:{1}", _hours.ToString(), _mins.ToString("##"));

                    }

                    _totalPlannedMinits += _estimatedMinits;
                    _totalActualMinits += _actualWorkMinits;

                    #endregion

                    #region --- Row Generation ---

                    _employeeRowSb.Append(rowHTML.Replace("#@BADGNO", _badgeNumberStr)
                        .Replace("#@BADGNO", _badgeNumberStr)
                        .Replace("#@EMPLOYEENAME", _nameStr)
                        .Replace("#@INTIME", _firstInStr)
                        .Replace("#@OUTTIME", _lastOutStr)
                        .Replace("#@ACTUALWORKHOURS", _actualWorkStr)
                        .Replace("#@OUTOFOFFICEHOURS", _actualOutofOfficeStr)
                         .Replace("#@LEAVECATEGORY", _leaveCategoryStr)
                        .Replace("#@LEAVETYPE", _leaveTypeStr)
                        .Replace("#@WORKATOFFICE", _workAtOfficeStr)
                        .Replace("#@WORKATHOME", _workAtHomeStr)
                        .Replace("#@REMARKS", _workCodeRemarksStr));

                    #endregion
                }

            }

            #region --- Summery Variable Init ---

            int _estimatedHours = _totalPlannedMinits / 60;
            int _estimatedMins = _totalPlannedMinits % 60;
            int _actualHours = _totalActualMinits / 60;
            int _actualMins = _totalActualMinits % 60;

            int _totalDifferentInMinits = Math.Abs(_totalActualMinits - _totalPlannedMinits);
            string _differneceSign = _totalActualMinits < _totalPlannedMinits ? "-" : string.Empty;
            int _differentHours = _totalDifferentInMinits / 60;
            int _differentMins = _totalDifferentInMinits % 60;

            #endregion

            // Append all the generated variables and generate the main report HTML
            _weekTableBody.Append(bodyHTML.Replace("#@ROWBODY", _employeeRowSb.ToString())
                .Replace("#@ReportHedderWeek", string.Format("Attendance Report for the Day {0}", _date)));




            _attendanceReportSb = _attendanceReportSb.Replace("#@WEEK1REPORTBODY", _weekTableBody.ToString());



            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Charset = "";
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            HttpContext.Current.Response.AddHeader("Content-Disposition", "inline;filename=AttendanceReport.xls");
            HttpContext.Current.Response.Write(_attendanceReportSb.ToString());
            HttpContext.Current.Response.End();
            HttpContext.Current.Response.Flush();
        }

        internal static void GenerateTeamAttandanceReport(ViewModels.AttendanceReportViewModel model, string reportHTML, string bodyHTML, string rowHTML)
        {
            #region --- Local variables ---

            StringBuilder _attendanceReportSb = new StringBuilder();
            _attendanceReportSb.Append(reportHTML);

            IList<ViewModels.WeekStructure> _weekStructureModelList = new List<ViewModels.WeekStructure>();
            DateTime _fromDate = new DateTime(model.Year, model.Month, 1);
            DateTime _toDate = _fromDate.AddMonths(1).AddDays(-1);
            IList<int> EmpIdList = TeamManagementRepository.GetTeamMembersDetails(model.TeamId).TeamMembers.Select(a => a.Id).ToList<int>();
            var _employeeList = EmployeeRepository.GetAllEmployeeOrderByName().Where(a => a.IsEnable == true);

            _employeeList = _employeeList.Where(x => EmpIdList.Contains(x.Id)).ToList();

            var employeeEnrollmentList = EmployeeEnrollmentRepository.GetEmployeeEnrollments();
            var _attendanceList = dbContext.Attendances.Where(a => a.Year == model.Year && a.Month == model.Month);
            DateTime _tempDate = _fromDate;
            int _totalActualMinits = 0;

            int _totalPlannedMinits = 0;
            int _plannedMinitsforDay = int.Parse(ConfigurationManager.AppSettings["PlannedMinitsPerDay"].ToString());
            List<ViewModels.EmployeeProjectStructure> _employeeProjectList = new List<ViewModels.EmployeeProjectStructure>();
            holidayList = HolidayRepository.GetHolidays(_fromDate, _toDate);


            #endregion

            #region --- Creating the week structure ---

            do
            {
                if (_tempDate.Day == 1 || (_tempDate.Day > 4 && _tempDate.DayOfWeek == DayOfWeek.Monday))
                {
                    ViewModels.WeekStructure _weekStr = new ViewModels.WeekStructure()
                    {
                        FromDate = _tempDate.Day,
                        Month = model.Month,
                        Year = model.Year,
                        Weeknumber = _weekStructureModelList.Count + 1
                    };
                    _weekStructureModelList.Add(_weekStr);
                }
                else
                    _weekStructureModelList[_weekStructureModelList.Count - 1].ToDate = _tempDate.Day;

                _tempDate = _tempDate.AddDays(1);
            } while (_tempDate < _toDate);

            _weekStructureModelList[_weekStructureModelList.Count - 1].ToDate = _toDate.Day;

            #endregion

            var leaveList = LeaveRepository.GetApprovedLeaveForRpt(_fromDate, _toDate);

            // For the generated week structure neeed to generate 
            // Seperate tabs in the excel sheet.
            foreach (var week in _weekStructureModelList)
            {
                string _weekHedder = string.Format("Attendance Report - Day {0}-{1}", week.FromDate.ToString(), week.ToDate.ToString());
                StringBuilder _weekTableBody = new StringBuilder();
                _totalPlannedMinits = 0;
                _totalActualMinits = 0;

                StringBuilder _employeeRowSb = new StringBuilder();
                // Add seperate rows for each employee from the selection.
                foreach (var employee in _employeeList)
                {
                    EmployeeEnrollment emp = dbContext.EmployeeEnrollment.Single(a => a.EmployeeId == employee.Id);


                    for (int i = week.FromDate; i <= week.ToDate; i++)
                    {
                        DateTime _reportDate = new DateTime(model.Year, model.Month, i);
                        if (employee.DateJoined != null && employee.DateJoined <= _reportDate)
                        {
                            var _employeeAttendanceListNotOrdered = _attendanceList.Where(a => a.EmployeeId == employee.Id
                                                                                     && a.Day == i).ToList();

                            var _employeeAttendanceList = _employeeAttendanceListNotOrdered.OrderBy(a => a.EmployeeId).ThenBy(a => a.Year).ThenBy(a => a.Month).
                                ThenBy(a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second).ToList();

                            #region ---- Parameters Init ----

                            string _badgeNumberStr = string.Empty;
                            string _nameStr = string.Empty;


                            string _dateStr = string.Empty;
                            string _dayOwWeekStrStr = string.Empty;

                            string _firstInStr = string.Empty;
                            string _lastOutStr = string.Empty;
                            string _actualWorkStr = string.Empty;
                            string _actualOutofOfficeStr = string.Empty;
                            string _leaveTypeStr = string.Empty;
                            string _leaveSatusStr = string.Empty;
                            string _remarkStr = string.Empty;
                            int _hours = 0;
                            int _mins = 0;

                            #endregion

                            #region --- Updating Parameter Values ---


                            _badgeNumberStr = emp.CardNo.ToString();
                            _nameStr = employee.Name;



                            _dateStr = _reportDate.ToString("dd/MM/yyyy");
                            _dayOwWeekStrStr = _reportDate.DayOfWeek.ToString();


                            DateTime? _firstInTime = null;
                            DateTime? _lastOutTime = null;
                            int _actualWorkMinits = GetActualWorkedMinitsForDay(_employeeAttendanceList, employee.Id, _reportDate.Day,
                                 ref _firstInTime, ref _lastOutTime);

                            int _outOfOfficeMinits = GetOutOfOfficeHourForDay(_employeeAttendanceList, employee.Id, _reportDate.Day,
                                 ref _firstInTime, ref _lastOutTime);

                            int _estimatedMinits = GetEstimateMinitsForDay(_plannedMinitsforDay, _reportDate, employee.Id, ref _leaveTypeStr, ref _leaveSatusStr, ref _remarkStr, leaveList);

                            if (_firstInTime != null)
                                _firstInStr = string.Format("{0}:{1}", _firstInTime.Value.Hour.ToString("#"), _firstInTime.Value.Minute.ToString("##"));
                            if (_lastOutTime != null)
                                _lastOutStr = string.Format("{0}:{1}", _lastOutTime.Value.Hour.ToString("#"), _lastOutTime.Value.Minute.ToString("##"));

                            if (_actualWorkMinits > 0)
                            {
                                _hours = _actualWorkMinits / 60;
                                _mins = _actualWorkMinits % 60;
                                _actualWorkStr = string.Format("{0}:{1}", _hours.ToString(), _mins.ToString("##"));
                                // _actualWorkStr = Math.Round(_actualWorkMinits / 60.00, 2).ToString();
                            }

                            if (_outOfOfficeMinits > 0)
                            {
                                _hours = _outOfOfficeMinits / 60;
                                _mins = _outOfOfficeMinits % 60;
                                _actualOutofOfficeStr = string.Format("{0}:{1}", _hours.ToString(), _mins.ToString("##"));

                            }

                            _totalPlannedMinits += _estimatedMinits;
                            _totalActualMinits += _actualWorkMinits;

                            #endregion

                            #region --- Row Generation ---

                            _employeeRowSb.Append(rowHTML.Replace("#@BADGNO", _badgeNumberStr)
                                .Replace("#@BADGNO", _badgeNumberStr)
                                .Replace("#@EMPLOYEENAME", _nameStr)
                                .Replace("#@ROWDATE", _dateStr)
                                .Replace("#@ROWDAY", _dayOwWeekStrStr)
                                .Replace("#@INTIME", _firstInStr)
                                .Replace("#@OUTTIME", _lastOutStr)
                                .Replace("#@ACTUALWORKHOURS", _actualWorkStr)
                                .Replace("#@OUTOFOFFICEHOURS", _actualOutofOfficeStr)
                                .Replace("#@LEAVETYPE", _leaveTypeStr)
                                .Replace("#@REMARKS", _remarkStr));

                            #endregion
                        }
                    }
                }

                #region --- Summery Variable Init ---

                int _estimatedHours = _totalPlannedMinits / 60;
                int _estimatedMins = _totalPlannedMinits % 60;
                int _actualHours = _totalActualMinits / 60;
                int _actualMins = _totalActualMinits % 60;

                int _totalDifferentInMinits = Math.Abs(_totalActualMinits - _totalPlannedMinits);
                string _differneceSign = _totalActualMinits < _totalPlannedMinits ? "-" : string.Empty;
                int _differentHours = _totalDifferentInMinits / 60;
                int _differentMins = _totalDifferentInMinits % 60;

                #endregion

                // Append all the generated variables and generate the main report HTML
                _weekTableBody.Append(bodyHTML.Replace("#@ROWBODY", _employeeRowSb.ToString())
                    .Replace("#@ReportHedderWeek", string.Format("Attendance Report - Day {0}-{1}", week.FromDate.ToString(), week.ToDate.ToString()))

                    //.Replace("#@ACTUALWORKHOURS", string.Format("{0}.{1}", _actualHours.ToString(), _actualMins.ToString("##")))
                    //.Replace("#@STANDARDWORKHOURS", string.Format("{0}.{1}", _estimatedHours.ToString(), _estimatedMins.ToString("##")))
                    //*****Due to chathurika Gunawardena's request we change the number format on 2-18-2015.
                    .Replace("#@ACTUALWORKHOURS", Math.Round(_totalActualMinits / 60.00, 2).ToString())
                    .Replace("#@STANDARDWORKHOURS", Math.Round(_totalPlannedMinits / 60.00, 2).ToString())


                    .Replace("#@DIFFERENCEPRECENTAGE", string.Format("{0} {1}.{2}", _differneceSign, _differentHours.ToString(), _differentMins.ToString("##"))));

                _attendanceReportSb = _attendanceReportSb.Replace(string.Format("#@WEEK{0}REPORTBODY", week.Weeknumber.ToString()), _weekTableBody.ToString());
            }

            #region --- Clear the empty tabs ---

            for (int i = 1; i <= 6; i++)
                if (i > _weekStructureModelList.Count)
                    _attendanceReportSb = _attendanceReportSb.Replace(string.Format("#@WEEK{0}REPORTBODY", i.ToString()), string.Empty);

            #endregion

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Charset = "";
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            HttpContext.Current.Response.AddHeader("Content-Disposition", "inline;filename=AttendanceReport.xls");
            HttpContext.Current.Response.Write(_attendanceReportSb.ToString());
            HttpContext.Current.Response.End();
            HttpContext.Current.Response.Flush();
        }

        /// <summary>
        /// Calculate the actual working minutes for the an employee 
        /// On a selected date for the report calculation.
        /// </summary>
        /// <param name="attendanceList">List of attendance for the calculation</param>
        /// <param name="employee">Selected employee to count</param>
        /// <param name="day">Selected date for the calculation</param>
        /// <param name="_firstInTime">First in as reference</param>
        /// <param name="_lastOutTime">Last out as reference</param>
        /// <returns></returns>
        private static int GetActualWorkedMinitsForDay(List<Attendance> attendanceList, int employeeId, int day,
            ref DateTime? _firstInTime, ref DateTime? _lastOutTime)
        {
            int _actulatimeWorked = 0;
            List<int> _exitLocationList = GetExitLocationMachins();
            var _employeeAttendanceList = attendanceList.Where(a => a.EmployeeId == employeeId
                            && a.Day == day).OrderBy(a => a.EmployeeId).ThenBy(a => a.Year).ThenBy(a => a.Month).
                            ThenBy(a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second).ToList();

            var _firstInAttendance = _employeeAttendanceList.FirstOrDefault(a => a.InOutMode == "in");
            if (_firstInAttendance != null)
            {
                // Gets the first attendance record with in
                // consider this record as the employee IN time.
                _firstInTime = new DateTime(_firstInAttendance.Year, _firstInAttendance.Month, _firstInAttendance.Day,
                                              _firstInAttendance.Hour, _firstInAttendance.Minute, _firstInAttendance.Second);

                Attendance _lastAttendance = null;
                _lastOutTime = null;

                // Gets the last out attendance as the employee OUT time
                var outattendanceList = _employeeAttendanceList.Where(a => a.InOutMode == "out");
                if (outattendanceList.Count() > 0)
                    _lastAttendance = outattendanceList.Last(a => a.InOutMode == "out");

                if (_lastAttendance != null)
                {
                    _lastOutTime = new DateTime(_lastAttendance.Year, _lastAttendance.Month, _lastAttendance.Day,
                                               _lastAttendance.Hour, _lastAttendance.Minute, _lastAttendance.Second);
                }
                else
                {
                    if (_firstInTime.Value.Date == Utility.GetDateTimeNow().Date)
                        _lastOutTime = Utility.GetDateTimeNow();
                }

                if (_lastOutTime != null)
                {

                    TimeSpan _tsOutOfOffice = new TimeSpan();
                    if (_lastAttendance != null)
                    {
                        for (int i = 0; i < _employeeAttendanceList.Count; i++)
                        {
                            var _outOfficeEntry = _employeeAttendanceList[i];
                            // Check for the attendance entry is not the first and the last entry for the day.
                            if (_outOfficeEntry.Id != _firstInAttendance.Id && _outOfficeEntry.Id != _lastAttendance.Id
                                && _outOfficeEntry.InOutMode == "out")
                            {

                                var firstInAttendanceForTheDay = _employeeAttendanceList.FirstOrDefault(a => a.Year == _outOfficeEntry.Year &&
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
                                    if ((i + 1) < (_employeeAttendanceList.Count - 1))
                                    {
                                        if (_employeeAttendanceList[i + 1].InOutMode == "in")
                                        {
                                            var _backtoOfficeEntry = _employeeAttendanceList[i + 1];
                                            DateTime _outOfOfficeTime = new DateTime(_outOfficeEntry.Year, _outOfficeEntry.Month, _outOfficeEntry.Day,
                                                   _outOfficeEntry.Hour, _outOfficeEntry.Minute, _outOfficeEntry.Second);
                                            DateTime _backtoOfficeTime = new DateTime(_backtoOfficeEntry.Year, _backtoOfficeEntry.Month, _backtoOfficeEntry.Day,
                                                   _backtoOfficeEntry.Hour, _backtoOfficeEntry.Minute, _backtoOfficeEntry.Second);
                                            _tsOutOfOffice += _backtoOfficeTime - _outOfOfficeTime;

                                        }
                                    }
                                }
                            }
                        }
                    }
                    // Calculates the time on work if the last out recourd can be found
                    // if last out was not found and the date is equal to system date? then consider 
                    // current time as the last out.
                    TimeSpan _ts = _lastOutTime.Value - _firstInTime.Value - _tsOutOfOffice;
                    _actulatimeWorked += (int)_ts.TotalMinutes;
                }
            }

            return _actulatimeWorked;
        }

        private static int GetOutOfOfficeHourForDay(List<Attendance> attendanceList, int employeeId, int day,
           ref DateTime? _firstInTime, ref DateTime? _lastOutTime)
        {
            int _OutofofficeHours = 0;
            List<int> _exitLocationList = GetExitLocationMachins();
            var _employeeAttendanceList = attendanceList.Where(a => a.EmployeeId == employeeId
                            && a.Day == day).OrderBy(a => a.EmployeeId).ThenBy(a => a.Year).ThenBy(a => a.Month).
                            ThenBy(a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second).ToList();

            var _firstInAttendance = _employeeAttendanceList.FirstOrDefault(a => a.InOutMode == "in");
            if (_firstInAttendance != null)
            {
                // Gets the first attendance record with in
                // consider this record as the employee IN time.
                _firstInTime = new DateTime(_firstInAttendance.Year, _firstInAttendance.Month, _firstInAttendance.Day,
                                              _firstInAttendance.Hour, _firstInAttendance.Minute, _firstInAttendance.Second);

                Attendance _lastAttendance = null;
                _lastOutTime = null;

                // Gets the last out attendance as the employee OUT time
                var outattendanceList = _employeeAttendanceList.Where(a => a.InOutMode == "out");
                if (outattendanceList.Count() > 0)
                    _lastAttendance = outattendanceList.Last(a => a.InOutMode == "out");

                if (_lastAttendance != null)
                {
                    _lastOutTime = new DateTime(_lastAttendance.Year, _lastAttendance.Month, _lastAttendance.Day,
                                               _lastAttendance.Hour, _lastAttendance.Minute, _lastAttendance.Second);
                }
                else
                {
                    if (_firstInTime.Value.Date == Utility.GetDateTimeNow().Date)
                        _lastOutTime = Utility.GetDateTimeNow();
                }

                if (_lastOutTime != null)
                {

                    TimeSpan _tsOutOfOffice = new TimeSpan();
                    if (_lastAttendance != null)
                    {
                        for (int i = 0; i < _employeeAttendanceList.Count; i++)
                        {
                            var _outOfficeEntry = _employeeAttendanceList[i];
                            // Check for the attendance entry is not the first and the last entry for the day.
                            if (_outOfficeEntry.Id != _firstInAttendance.Id && _outOfficeEntry.Id != _lastAttendance.Id
                                && _outOfficeEntry.InOutMode == "out")
                            {

                                var firstInAttendanceForTheDay = _employeeAttendanceList.FirstOrDefault(a => a.Year == _outOfficeEntry.Year &&
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
                                    if ((i + 1) < (_employeeAttendanceList.Count - 1))
                                    {
                                        if (_employeeAttendanceList[i + 1].InOutMode == "in")
                                        {
                                            var _backtoOfficeEntry = _employeeAttendanceList[i + 1];
                                            DateTime _outOfOfficeTime = new DateTime(_outOfficeEntry.Year, _outOfficeEntry.Month, _outOfficeEntry.Day,
                                                   _outOfficeEntry.Hour, _outOfficeEntry.Minute, _outOfficeEntry.Second);
                                            DateTime _backtoOfficeTime = new DateTime(_backtoOfficeEntry.Year, _backtoOfficeEntry.Month, _backtoOfficeEntry.Day,
                                                   _backtoOfficeEntry.Hour, _backtoOfficeEntry.Minute, _backtoOfficeEntry.Second);
                                            _tsOutOfOffice += _backtoOfficeTime - _outOfOfficeTime;

                                        }
                                    }
                                }
                            }
                        }
                    }
                    _OutofofficeHours += (int)_tsOutOfOffice.TotalMinutes;
                }
            }

            return _OutofofficeHours;
        }

        /// <summary>
        /// Gets the estimated minutes for the selected date
        /// for the selected employee
        /// </summary>
        /// <param name="defaultPlannedMinits">Planned minutes as default</param>
        /// <param name="queryDate">Date for the calculation</param>
        /// <param name="employee">Selected employee for the calculation</param>
        /// <param name="leaveType">Reave type as reference</param>
        /// <param name="reason">Reason as reference</param>
        /// <returns></returns>
        private static int GetEstimateMinitsForDay(int defaultPlannedMinits, DateTime queryDate, int employeeId, ref string leaveType, ref string leaveSattus, ref string reason, IList<Leave> leaveList)
        {
            // Consider saturday and sunday as holidays
            int _estimatedMinits = defaultPlannedMinits;
            var _holiday = HolidayRepository.IsHoliday(holidayList, queryDate);

            if (_holiday != null)
            {
                if (_holiday.Type == HolidayType.FullDay)
                    _estimatedMinits = 0;
                else
                    _estimatedMinits = defaultPlannedMinits / 2;
                reason = _holiday.Reason;
                leaveType = "OFFICIAL LEAVE";
            }

            if (defaultPlannedMinits > 0)
            {
                var _leave = LeaveRepository.IsLeave(leaveList, queryDate, employeeId);
                if (_leave != null)
                {
                    if (_leave.LeaveType == LeaveType.FullDay)
                    {
                        _estimatedMinits = 0;
                        leaveType = "FULL DAY LEAVE";
                    }
                    else
                    {
                        _estimatedMinits -= defaultPlannedMinits / 2;
                        leaveType = "HALF DAY LEAVE";
                    }

                    leaveSattus = _leave.Status;

                }
            }

            return _estimatedMinits;
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
        /// <summary>
        /// Calculate the actual working hours for the selected date
        /// Updates the individiual working hours for each enployee
        /// </summary>
        /// <param name="model">View model from the client with parameter data</param>
        /// <param name="queryDate">Selected date for the actual hours calculation</param>
        /// <returns>Total number of hours</returns>
        private static int GetActualWorkedMinits(ViewModels.DailyAttendanceViewModel model, DateTime queryDate)
        {

            IQueryable<Attendance> _attendanceSearchQuery = dbContext.Attendances;
            _attendanceSearchQuery = GetAttendanceSearchQuery(model, queryDate);
            List<Attendance> attendanceList = _attendanceSearchQuery.ToList()
                                              .OrderBy(a => a.EmployeeId)
                                              .ThenBy(a => a.Year)
                                              .ThenBy(a => a.Month)
                                              .ThenBy(a => a.Day)
                                              .ThenBy(a => a.Hour)
                                              .ThenBy(a => a.Minute)
                                              .ThenBy(a => a.Second).ToList();

            List<int> _exitLocationList = GetExitLocationMachins();


            double _totalInTimeForAllEmployee = 0;
            double _totalInTimeWFHForAllEmployee = 0;

            // calculation for each employee
            foreach (var _employee in model.SelectedEmployeeList)
            {
                double _totalInTimeForEmployee = 0;

                List<Attendance> _employeeAttendanceList = attendanceList.Where(a => a.EmployeeId == _employee.Id).OrderBy(a => a.Year).ThenBy(a => a.Month).ThenBy(a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).
                    ThenBy(a => a.Second).ToList();

                var _firstInAttendance = _employeeAttendanceList.FirstOrDefault(a => a.EmployeeId == _employee.Id && a.InOutMode == "in");
                if (_firstInAttendance != null)
                {
                    // Gets the first attendance record with in
                    // consider this record as the employee IN time.
                    DateTime _firstInTime = new DateTime(_firstInAttendance.Year, _firstInAttendance.Month, _firstInAttendance.Day,
                                                  _firstInAttendance.Hour, _firstInAttendance.Minute, _firstInAttendance.Second);

                    Attendance _lastAttendance = null;
                    DateTime? _lastOutTime = null;

                    // Gets the last out attendance as the employee OUT time
                    var outattendanceList = _employeeAttendanceList.Where(a => a.EmployeeId == _employee.Id && a.InOutMode == "out");
                    if (outattendanceList.Count() > 0)
                        _lastAttendance = outattendanceList.Last(a => a.EmployeeId == _employee.Id && a.InOutMode == "out");

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



                    if (_lastAttendance != null)
                    {
                        TimeSpan _tsOutOfOffice = new TimeSpan();
                        for (int i = 0; i < _employeeAttendanceList.Count; i++)
                        {
                            var _outOfficeEntry = _employeeAttendanceList[i];
                            // Check for the attendance entry is not the first and the last entry for the day.
                            if (_outOfficeEntry.Id != _firstInAttendance.Id && _outOfficeEntry.Id != _lastAttendance.Id
                                && _outOfficeEntry.InOutMode == "out")
                            {

                                var firstInAttendanceForTheDay = _employeeAttendanceList.FirstOrDefault(a => a.Year == _outOfficeEntry.Year &&
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
                                    if ((i + 1) < (_employeeAttendanceList.Count - 1))
                                    {
                                        if (_employeeAttendanceList[i + 1].InOutMode == "in")
                                        {
                                            var _backtoOfficeEntry = _employeeAttendanceList[i + 1];
                                            DateTime _outOfOfficeTime = new DateTime(_outOfficeEntry.Year, _outOfficeEntry.Month, _outOfficeEntry.Day,
                                                   _outOfficeEntry.Hour, _outOfficeEntry.Minute, _outOfficeEntry.Second);
                                            DateTime _backtoOfficeTime = new DateTime(_backtoOfficeEntry.Year, _backtoOfficeEntry.Month, _backtoOfficeEntry.Day,
                                                   _backtoOfficeEntry.Hour, _backtoOfficeEntry.Minute, _backtoOfficeEntry.Second);
                                            _tsOutOfOffice += _backtoOfficeTime - _outOfOfficeTime;

                                            //_totalInTime = _totalInTime - (int)_tsOutOfOffice.TotalMinutes;

                                            //_totalInTimeForEmployee = _totalInTimeForEmployee - (int)_tsOutOfOffice.TotalMinutes;
                                        }
                                    }
                                }
                            }
                        }//---

                        TimeSpan tp = (_lastOutTime.Value - _firstInTime);

                        _totalInTimeForEmployee = (tp.Subtract(_tsOutOfOffice).TotalMinutes);
                        if (_lastOutTime != null)
                        {
                            foreach (var _employeeCoverage in attendanceCoverageReportModel.EmployeeCoverageList)
                            {
                                if (_employeeCoverage.EmployeeID == _employee.Id)
                                {
                                    _employeeCoverage.ActualMinits += Convert.ToDecimal(_totalInTimeForEmployee);
                                    _totalInTimeForAllEmployee += _totalInTimeForEmployee;
                                    break;
                                }
                            }
                        }

                    }

                    //if (_lastOutTime != null)
                    //{
                    //    // Calculates the time on work if the last out recourd can be found
                    //    // if last out was not found and the date is equal to system date? then consider 
                    //    // current time as the last out.
                    //    TimeSpan _ts = _lastOutTime.Value - _firstInTime;
                    //    _totalInTime += (int)_ts.TotalMinutes;

                    //    foreach (var _employeeCoverage in attendanceCoverageReportModel.EmployeeCoverageList)
                    //    {
                    //        if (_employeeCoverage.EmployeeID == _employee.Id)
                    //        {
                    //            _employeeCoverage.ActualMinits += (int)_ts.TotalMinutes;
                    //            break;
                    //        }
                    //    }
                    //}
                }
            }

            return (int)_totalInTimeForAllEmployee;
        }

        /// <summary>
        /// Calculate the actual working hours for the selected date
        /// and employee within the list of attendance records
        /// </summary>
        /// <param name="employeeID">Employee to be filter</param>
        /// <param name="queryDate">selected date to be query</param>
        /// <param name="attendanceList">list of attendance records to be search in</param>
        /// <returns></returns>
        private static int GetEmployeeWorkedMinits(int employeeID, DateTime queryDate, List<Attendance> attendanceList)
        {
            int _totalInTime = 0;
            // First in record for the selected date.
            var _employeeAttendanceList = attendanceList.Where(a => a.EmployeeId == employeeID).OrderBy(a => a.Year).ThenBy(a => a.Month).ThenBy(a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).
                    ThenBy(a => a.Second);

            var _firstInAttendance = _employeeAttendanceList.FirstOrDefault(a => a.Year == queryDate.Year
                && a.Month == queryDate.Month && a.Day == queryDate.Day && a.InOutMode == "in" && a.LocationId != 27);
            if (_firstInAttendance != null)
            {
                // Gets the first attendance record with in
                // consider this record as the employee IN time.
                DateTime _firstInTime = new DateTime(_firstInAttendance.Year, _firstInAttendance.Month, _firstInAttendance.Day,
                                              _firstInAttendance.Hour, _firstInAttendance.Minute, _firstInAttendance.Second);

                Attendance _lastAttendance = null;
                DateTime? _lastOutTime = null;

                // Gets the last out attendance as the employee OUT time
                var outattendanceList = _employeeAttendanceList.Where(a => a.Year == queryDate.Year
                    && a.Month == queryDate.Month && a.Day == queryDate.Day && a.InOutMode == "out" && a.LocationId != 27);

                if (outattendanceList.Count() > 0)
                    _lastAttendance = outattendanceList.Last();

                if (_lastAttendance != null)
                    _lastOutTime = new DateTime(_lastAttendance.Year, _lastAttendance.Month, _lastAttendance.Day,
                                               _lastAttendance.Hour, _lastAttendance.Minute, _lastAttendance.Second);
                else
                    if (_firstInTime.Date == Utility.GetDateTimeNow().Date)
                    _lastOutTime = Utility.GetDateTimeNow();

                if (_lastOutTime != null)
                {
                    // Calculates the time on work if the last out recourd can be found
                    // if last out was not found and the date is equal to system date? then consider 
                    // current time as the last out.
                    //TimeSpan _ts = _lastOutTime.Value - _firstInTime;
                    // _totalInTime += (int)_ts.TotalMinutes;
                    TimeSpan _ts = CalculateTotalTime(_firstInTime, _lastOutTime, _employeeAttendanceList, queryDate);
                    _totalInTime += (int)_ts.TotalMinutes;

                    foreach (var _employeeCoverage in attendanceCoverageReportModel.EmployeeCoverageList)
                    {
                        if (_employeeCoverage.EmployeeID == employeeID)
                        {
                            _employeeCoverage.ActualMinits += (int)_ts.TotalMinutes;
                            break;
                        }
                    }
                }
            }

            return _totalInTime;
        }

        private static TimeSpan CalculateTotalTime(DateTime firstInTime, DateTime? lastOutTime, IOrderedEnumerable<Attendance> employeeAttendanceList, DateTime queryDate)
        {
            var wfhInAttendance = employeeAttendanceList.FirstOrDefault(a => a.Year == queryDate.Year
                && a.Month == queryDate.Month && a.Day == queryDate.Day && a.InOutMode == "in" && a.LocationId == 27);

            var wfhOutAttendance = employeeAttendanceList.LastOrDefault(a => a.Year == queryDate.Year
                && a.Month == queryDate.Month && a.Day == queryDate.Day && a.InOutMode == "out" && a.LocationId == 27);
            DateTime? wfhInTime = null;
            DateTime? wfhIOutTime = null;
            TimeSpan? ts = null;

            if (wfhInAttendance != null)
            {
                wfhInTime = new DateTime(wfhInAttendance.Year, wfhInAttendance.Month, wfhInAttendance.Day, wfhInAttendance.Hour, wfhInAttendance.Minute, wfhInAttendance.Second);
            }

            if (wfhOutAttendance != null)
            {
                wfhIOutTime = new DateTime(wfhOutAttendance.Year, wfhOutAttendance.Month, wfhOutAttendance.Day, wfhOutAttendance.Hour, wfhOutAttendance.Minute, wfhOutAttendance.Second);
            }


            if (firstInTime < wfhInTime && lastOutTime > wfhIOutTime)
            {
                ts = lastOutTime - firstInTime;
            }
            else if (firstInTime > wfhInTime && lastOutTime < wfhIOutTime)
            {
                ts = wfhIOutTime - wfhInTime;
            }
            else if (firstInTime > wfhIOutTime)
            {
                ts = (lastOutTime.Value - firstInTime) + (wfhIOutTime - wfhInTime);
            }
            else if (firstInTime == wfhIOutTime)
            {
                ts = lastOutTime.Value - wfhInTime;
            }
            else if (firstInTime > wfhInTime && lastOutTime > wfhIOutTime)
            {
                ts = lastOutTime.Value - wfhInTime;
            }
            else if (firstInTime > wfhInTime && lastOutTime == wfhIOutTime)
            {
                //xx
                ts = wfhIOutTime - wfhInTime;
            }
            else if (firstInTime == wfhInTime && lastOutTime > wfhIOutTime)
            {
                ts = lastOutTime - firstInTime;
            }
            else if (firstInTime == wfhInTime && lastOutTime == wfhIOutTime)
            {
                ts = lastOutTime - firstInTime;
            }
            else if (firstInTime == wfhInTime && lastOutTime < wfhIOutTime)
            {
                ts = wfhIOutTime - wfhInTime;
            }
            else if (firstInTime < wfhInTime && lastOutTime == wfhIOutTime)
            {
                ts = lastOutTime - firstInTime;
            }
            else if (lastOutTime == wfhInTime)
            {
                ts = wfhIOutTime - firstInTime;
                //ts = wfhIOutTime - firstInTime;
            }
            else if (lastOutTime < wfhInTime)//firstInTime < wfhInTime && lastOutTime < wfhIOutTime
            {
                ts = (lastOutTime.Value - firstInTime) + (wfhIOutTime - wfhInTime);
                //ts = wfhIOutTime - firstInTime;
                //ts = wfhIOutTime - firstInTime;
            }
            else if (firstInTime < wfhInTime && lastOutTime < wfhIOutTime)
            {
                ts = wfhIOutTime - firstInTime;
                //ts = (lastOutTime.Value - firstInTime) + (wfhIOutTime - wfhInTime);
            }
            else
            {
                ts = lastOutTime - firstInTime;
            }

            return ts.Value;
        }

        private static int GetEmployeeTaskMinits(int employeeID, DateTime queryDate, IList<WorkingFromHomeTask> allTasks)
        {
            int _totalTaskTimeMinutes = 0;

            var taskList = allTasks.Where(t => t.EmployeeId == employeeID && t.Date.Date == queryDate).ToList();

            foreach (var item in taskList)
            {
                _totalTaskTimeMinutes += (item.Hours * 60) + item.Minutes;
            }

            return _totalTaskTimeMinutes;
        }

        /// <summary>
        /// Creates the dynamic search Query for the attendace.
        /// </summary>
        /// <param name="model">view model from the client with parameter data</param>
        /// <param name="queryDate">slected date for the query</param>
        /// <returns>Returns the search query</returns>
        private static IQueryable<Attendance> GetAttendanceSearchQuery(ViewModels.DailyAttendanceViewModel model, DateTime queryDate)
        {
            int[] _employeeIDList = new int[model.SelectedEmployeeList.Count];
            for (int i = 0; i < model.SelectedEmployeeList.Count; i++)
                _employeeIDList[i] = model.SelectedEmployeeList[i].Id;

            IQueryable<Attendance> _attendanceSearchQuery = dbContext.Attendances;
            _attendanceSearchQuery = dbContext.Attendances;
            _attendanceSearchQuery = _attendanceSearchQuery.Where(entity => _employeeIDList.Contains(entity.EmployeeId));
            _attendanceSearchQuery = _attendanceSearchQuery.Where(a => a.Day == queryDate.Day).
                Where(a => a.Month == queryDate.Month).Where(a => a.Year == queryDate.Year);
            //_attendanceSearchQuery = _attendanceSearchQuery.OrderBy(a => a.EmployeeId).ThenBy(a => a.Year).ThenBy(a => a.Month).ThenBy(a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).
            //        ThenBy(a => a.Second);
            return _attendanceSearchQuery;
        }

        /// <summary>
        /// Creates the dynamic search Query for the employee attendace.
        /// </summary>
        /// <param name="employee">selected employee for the attandance search</param>
        /// <param name="queryDate">slected date for the query</param>
        /// <returns>Returns the search query</returns>
        private static IQueryable<Attendance> GetAttendanceSearchQuery(int employeeId, DateTime queryDate)
        {
            IQueryable<Attendance> _attendanceSearchQuery = dbContext.Attendances;
            _attendanceSearchQuery = dbContext.Attendances;
            _attendanceSearchQuery = _attendanceSearchQuery.Where(entity => entity.EmployeeId == employeeId);
            _attendanceSearchQuery = _attendanceSearchQuery.Where(a => a.Day == queryDate.Day).
                Where(a => a.Month == queryDate.Month).Where(a => a.Year == queryDate.Year);
            _attendanceSearchQuery = _attendanceSearchQuery.OrderBy(a => a.EmployeeId).ThenBy(a => a.Year).ThenBy(a => a.Month).ThenBy(a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).
                    ThenBy(a => a.Second);
            return _attendanceSearchQuery;
        }

        /// <summary>
        /// Creates the employee list HTML for the view
        /// Will be directly displayed on the view
        /// </summary>
        /// <param name="employeeList">List of employees to be displayed</param>
        /// <returns>Employee list HTML</returns>
        private static string GetEmployeeListView(List<EmployeeData> employeeList)
        {
            StringBuilder _sb = new StringBuilder();
            foreach (var employee in employeeList)
            {
                _sb.Append(string.Format("<div class=\"employeelistdiv\" id=\"DIV_{0}_FilterEmployeeItem\">{1}</div>",
                   employee.Id.ToString(), employee.Name));
            }

            if (employeeList.Count == 0)
            {
                _sb.Append("<div style=\"padding-top: 15px; padding-bottom: 15px; padding-left: 10px; font-size: 13px; color: #8a8a8a; border: 0px;\">");
                _sb.Append("No employees found.</div>");
            }

            return _sb.ToString();
        }

        /// <summary>
        /// Creates the employee list HTML for the view
        /// Will be directly displayed on the view
        /// </summary>
        /// <param name="employeeList">List of employees to be displayed</param>
        /// <returns>Employee list HTML</returns>
        private static string GetSharedEmployeeListView(List<EmployeeData> employeeList)
        {
            StringBuilder _sb = new StringBuilder();
            foreach (var employee in employeeList)
            {
                _sb.Append(string.Format("<div class=\"employeeSharedlistdiv\" id=\"DIV_{0}_FilterSharedEmployeeItem\">{1}</div>",
                   employee.Id.ToString(), employee.Name));
            }

            if (employeeList.Count == 0)
            {
                _sb.Append("<div style=\"padding-top: 15px; padding-bottom: 15px; padding-left: 10px; font-size: 13px; color: #8a8a8a; border: 0px;\">");
                _sb.Append("No employees found.</div>");
            }

            return _sb.ToString();
        }

        /// <summary>
        /// Gets the Sup value for the date
        /// </summary>
        /// <param name="date">Date to generate</param>
        /// <returns>Sup value</returns>
        private static string GetDateSup(int date)
        {
            string _lastChar = date.ToString().Substring(date.ToString().Length - 1);
            if (_lastChar == "1")
                return "st";
            if (_lastChar == "2")
                return "nd";
            if (_lastChar == "3")
                return "rd";

            return "th";
        }

        /// <summary>
        /// Gets the abbr for the month
        /// </summary>
        /// <param name="month">month</param>
        /// <returns>Abbr</returns>
        private static string GetMonthAbbr(int month)
        {
            if (month == 1)
                return "Jan";
            if (month == 2)
                return "Feb";
            if (month == 3)
                return "Mar";
            if (month == 4)
                return "Apr";
            if (month == 5)
                return "May";
            if (month == 6)
                return "Jun";
            if (month == 7)
                return "Jul";
            if (month == 8)
                return "Aug";
            if (month == 9)
                return "Sep";
            if (month == 10)
                return "Oct";
            if (month == 11)
                return "Nov";

            return "Dec";
        }

        /// <summary>
        /// TODO:Need to add the Comment
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        internal static string GetLateEmployeesQuery(ViewModels.LateEmployeesModel model)
        {
            #region Filter attendance details
            //getting attendance details for the selected date
            var employeeAttendances = dbContext.Attendances.Where(a => a.Year == model.Date.Year && a.Month == model.Date.Month && a.Day == model.Date.Day).Where(a => a.InOutMode == "in");

            //filter attendance details for main entrance and basement
            //employeeAttendances = employeeAttendances.Where(a => a.LocationId == 8 || a.LocationId == 9); 

            //grouping by employee for avoid several hittings
            IQueryable<IGrouping<int, Attendance>> groups = employeeAttendances.Include(a => a.Employee).GroupBy(a => a.Employee.EmployeeId);

            #endregion

            #region Html table header
            StringBuilder sb = new StringBuilder();
            sb.Append("<tr>");
            sb.Append("<td class=\" employee_clm hedder_clm\">Employee Name</td>");
            sb.Append("<td class=\"hedder_clm\">First In</td>");
            sb.Append("</tr>");
            #endregion

            List<Attendance> lateEmployees = new List<Attendance>();

            #region Html table body
            foreach (var item in groups)
            {
                //order attendance details according to time
                var orderedItem = item.OrderBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second);

                var firstEntry = orderedItem.First();
                lateEmployees.Add(firstEntry);
            }

            var lateEmpQuery = lateEmployees.OrderBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second);

            foreach (var item in lateEmpQuery)
            {
                DateTime firstInTime = DateTime.Parse(string.Format("{0}:{1}:{2}", item.Hour, item.Minute, item.Second));
                DateTime timeMargin = DateTime.Parse(string.Format("{0}:{1}:{2}", model.Date.Hour, model.Date.Minute, model.Date.Second));
                if (firstInTime > timeMargin)
                {
                    EmployeeData emp = EmployeeRepository.GetEmployee(item.EmployeeId);
                    sb.Append("<tr>");
                    sb.Append(string.Format("<td class=\"nametd\"><a href=\"javascript:new HomeLogin().QuickFindWithEmployee({1},'{2}');\">{0}</a></td>", emp.Name, emp.Id, model.Date.ToString("dd/MM/yyyy")));
                    sb.Append(string.Format("<td class=\"hourstd work_incomplete\">{0}</td>", firstInTime.ToString("HH:mm")));
                    sb.Append("</tr>");
                }
            }
            #endregion

            return sb.ToString();
        }

        public static IList<WorkingFromHomeTask> GetTaskListForEmployeeByDate(int employeeId, DateTime date)
        {
            return dbContext.WorkingFromHomeTasks.Where(a => a.EmployeeId == employeeId
            && a.Date.Year == date.Year
            && a.Date.Month == date.Month
            && a.Date.Day == date.Day).ToList();
        }
    }
    public enum MonthsOfyear
    {
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12
    }
}