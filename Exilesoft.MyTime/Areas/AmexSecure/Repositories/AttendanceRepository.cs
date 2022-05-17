using System.Data.Entity;
using Exilesoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exilesoft.MyTime.Helpers;
using WebGrease.Css.Extensions;
using Exilesoft.MyTime.Services;
using System.Configuration;
using Exilesoft.MyTime.Repositories;
namespace Exilesoft.MyTime.Areas.AmexSecure.Repositories
{
    public class AttendanceRepository
    {

        internal static ViewModels.AttendanceListViewModel QuickFindLocation(int? employeeId, string employeeName, string selectedDateStr, string userType)
        {
            System.DateTime? selectedDate = Helpers.Utility.ParseDate(selectedDateStr);

            if (userType.Equals("Visitor"))
            {
                int visitoID = 0;
                if (Helpers.Utility.ParseInt(employeeName) != null)
                    visitoID = Helpers.Utility.ParseInt(employeeName).Value;
                return QuickFindVisitorLocation(visitoID, selectedDate);
            }
            bool _foundUser = false;
            Context dbContext = new Context();

            List<EmployeeStatistics> _employeeStatisticList = new List<EmployeeStatistics>();

           
            StringBuilder employeeLocationSB = new StringBuilder();
            StringBuilder employeeAttendanceDetailSB = new StringBuilder();
            ViewModels.AttendanceListViewModel model = new ViewModels.AttendanceListViewModel();

            List<Location> _locationList = dbContext.Locations.OrderBy(a => a.Floor).ToList();
          
            #region --- Checking for the date selected ---

            int _day = 0;
            int _month = 0;
            int _year = 0;

            if (selectedDate == null)
            {
                selectedDate = Utility.GetDateTimeNow();
                _day = selectedDate.Value.Day;
                _month = selectedDate.Value.Month;
                _year = selectedDate.Value.Year;
            }
            else
            {
                _day = selectedDate.Value.Day;
                _month = selectedDate.Value.Month;
                _year = selectedDate.Value.Year;
            }

            #endregion



            var _attendanceList = dbContext.Attendances.Where(a => a.Day == _day && a.Month == _month && a.Year == _year && a.LocationId == 32);
            var _employeeList = EmployeeRepository.GetAllEmployeeOrderByName();

            //if (employeeId != null && employeeId != 0 && )
            //    _employeeList = _employeeList.Where(a => a.Id == employeeId).ToList();

            _employeeList = _employeeList.Where(a => a.Name.ToLower().Contains(employeeName.ToLower())).ToList();

            var EmpIdList = _attendanceList.Select(a => a.EmployeeId).Distinct();


            var selected_employeeList = from u in _employeeList
                                        where EmpIdList.Contains(u.Id)
                                        select u;


            employeeAttendanceDetailSB.Append("<table id=\"TBL_EmployeeAttendanceList\">");

            foreach (var _employee in selected_employeeList)
            {
                var _employeeStatistic = new EmployeeStatistics();
                _employeeStatistic.EmployeeId = _employee.Id;
                _employeeStatistic.Name = _employee.Name;
                _employeeStatistic.EmployeeImagePath = _employee.ImagePath;
                _employeeStatistic.Date = selectedDate.Value.ToShortDateString();

                List<Attendance> listAttenndance = _attendanceList.Where(a => a.EmployeeId == _employee.Id).ToList();
                List<Attendance> orderedListAttenndance = listAttenndance.OrderBy(a => a.Hour)
                                                                     .ThenBy(b => b.Minute)
                                                                     .ThenBy(c => c.Second)
                                                                     .ToList();


                string EmplployeeImageurl = ConfigurationManager.AppSettings["EmployeeWebApi"].Replace("/api/Employee/{0}", "/Content/ProfilePic/") + _employeeStatistic.EmployeeImagePath;
                employeeAttendanceDetailSB.Append("<tr><td style=\"padding-top:7px;padding-bottom:7px;\"><table class=\"cleartable\"><tr>");
                employeeAttendanceDetailSB.Append("<td rowspan=\"2\" class=\"TD_QuickListEmpImg\">");
                if (string.IsNullOrEmpty(_employeeStatistic.EmployeeImagePath))
                    employeeAttendanceDetailSB.Append("<img class=\"QuickListEmpImg\" src=\"../../Content/images/default_user.png\" />");
                else

                    employeeAttendanceDetailSB.Append(string.Format("<img class=\"QuickListEmpImg\" src=\"{0}\" />", EmplployeeImageurl));

                employeeAttendanceDetailSB.Append(string.Format("</td><td class=\"QuickListEmpName\">{0}</td>", _employeeStatistic.Name));
                employeeAttendanceDetailSB.Append("</tr><tr><td class=\"QuickListEmpContent\"><table class=\"cleartable\">");


                var allEmployeeAttendances = listAttenndance.OrderBy(a => a.Year).ThenBy(a => a.Month).ThenBy(a => a.Day).ThenBy(a => a.Hour)
                .ThenBy(a => a.Minute).ThenBy(a => a.Second);

                var compressedAllAttendances = new List<Attendance>();
                Attendance previousItem1 = null;

                foreach (var employeeAttendance in allEmployeeAttendances)
                {
                    if (previousItem1 != null && employeeAttendance.LocationId == previousItem1.LocationId)
                    {
                        if (employeeAttendance.Hour == previousItem1.Hour && employeeAttendance.Minute == previousItem1.Minute && Math.Abs(employeeAttendance.Second - previousItem1.Second) <= 5)
                        {
                            continue;
                        }
                    }

                    employeeAttendance.Location = _locationList.FirstOrDefault(l => l.Id == employeeAttendance.LocationId);

                    employeeAttendanceDetailSB.Append("<tr><td class=\"TD_Date_Text\">Date:</td>");
                    employeeAttendanceDetailSB.Append(string.Format("<td class=\"TD_Date_Value\">{0}/{1}/{2}</td>", employeeAttendance.Day.ToString("00"), employeeAttendance.Month.ToString("00"), employeeAttendance.Year));
                    employeeAttendanceDetailSB.Append("<td class=\"TD_Time_Text\">Time:</td>");
                    employeeAttendanceDetailSB.Append(string.Format("<td class=\"TD_Time_Value\">{0}:{1}:{2}</td>", employeeAttendance.Hour.ToString("00"), employeeAttendance.Minute.ToString("00"), employeeAttendance.Second.ToString("00")));
                    employeeAttendanceDetailSB.Append("<td class=\"TD_Mode_Text\">Mode:</td>");
                    employeeAttendanceDetailSB.Append(string.Format("<td class=\"TD_Mode_Value\">{0}</td>", employeeAttendance.InOutMode));
                    employeeAttendanceDetailSB.Append("<td class=\"TD_Location_Text\">Location:</td>");
                    employeeAttendanceDetailSB.Append(string.Format("<td class=\"TD_Location_Value\">{0}</td></tr>", employeeAttendance.Location != null ? employeeAttendance.Location.Floor : string.Empty));
                    previousItem1 = employeeAttendance;
                }


                employeeAttendanceDetailSB.Append("</table></td></tr></table></td></tr>");
            }

            employeeLocationSB.Append("<table id=\"TBL_QuickFindEmployeeList\">");
            foreach (var employee in selected_employeeList)
            {
                _foundUser = true;
                var _employeeStatistic = new EmployeeStatistics();
                _employeeStatistic.EmployeeId = employee.Id;
                _employeeStatistic.Name = employee.Name;
                _employeeStatistic.EmployeeImagePath = employee.ImagePath;
                _employeeStatistic.Date = selectedDate.Value.ToShortDateString();
                string EmplployeeImageurl = ConfigurationManager.AppSettings["EmployeeWebApi"].Replace("/api/Employee/{0}", "/Content/ProfilePic/") + _employeeStatistic.EmployeeImagePath;

                employeeLocationSB.Append("<tr><td onclick=\"new QuickFind().ShowEmployeeAttendnace(" + _employeeStatistic.EmployeeId + ");\"><table class=\"cleartable\"><tr><td rowspan=\"2\" class=\"TD_QuickListEmpImg\">");
                if (string.IsNullOrEmpty(_employeeStatistic.EmployeeImagePath))
                    employeeLocationSB.Append("<img class=\"QuickListEmpImg\" src=\"../../Content/images/default_user.png\" />");
                else
                    employeeLocationSB.Append(string.Format("<img class=\"QuickListEmpImg\" src=\"{0}\" />", EmplployeeImageurl));
                employeeLocationSB.Append(string.Format("<td class=\"QuickListEmpName\" colspan=\"8\">{0} ~ {1}</td></tr><tr>", _employeeStatistic.Name, "Amex Secure Room"));


                employeeLocationSB.Append("</tr></table></td></tr>");
            }

            employeeLocationSB.Append("</table>");

            if (!_foundUser)
            {
                employeeLocationSB.Clear();
                employeeLocationSB.Append("0");
            }

            model.AttendanceListEntryContent = employeeAttendanceDetailSB.ToString();
            model.EmployeeLocationContent = employeeLocationSB.ToString();
            return model;
        }


        internal static ViewModels.AttendanceListViewModel QuickFindLocation(int? employeeId, string employeeName, string selectedDateStr, string userType, int logedInUserId)
        {
            System.DateTime? selectedDate = Helpers.Utility.ParseDate(selectedDateStr);

            if (userType.Equals("Visitor"))
            {
                int visitoID = 0;
                if (Helpers.Utility.ParseInt(employeeName) != null)
                    visitoID = Helpers.Utility.ParseInt(employeeName).Value;
                return QuickFindVisitorLocation(visitoID, selectedDate);
            }

            Context dbContext = new Context();

            List<EmployeeStatistics> _employeeStatisticList = new List<EmployeeStatistics>();
            var _employeeList = EmployeeRepository.GetAllEmployeeOrderByName();

            EmployeeEnrollment loggedUser = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == logedInUserId);
            // For the inner HTML Generation for multiple views
            StringBuilder employeeLocationSB = new StringBuilder();
            StringBuilder employeeAttendanceDetailSB = new StringBuilder();
            ViewModels.AttendanceListViewModel model = new ViewModels.AttendanceListViewModel();

            List<Location> _locationList = dbContext.Locations.OrderBy(a => a.Floor).ToList();
            List<int> _exitLocationList = Exilesoft.MyTime.Repositories.TimeTrendRepository.GetExitLocationMachins();

            #region --- Checking for the date selected ---

            int _day = 0;
            int _month = 0;
            int _year = 0;

            if (selectedDate == null)
            {
                selectedDate = Utility.GetDateTimeNow();
                _day = selectedDate.Value.Day;
                _month = selectedDate.Value.Month;
                _year = selectedDate.Value.Year;
            }
            else
            {
                _day = selectedDate.Value.Day;
                _month = selectedDate.Value.Month;
                _year = selectedDate.Value.Year;
            }

            #endregion

            #region --- Generate Employee Statistics ---

            var _attendanceList = dbContext.Attendances.Where(a => a.Day == _day && a.Month == _month && a.Year == _year);
            if (employeeId != null && employeeId != 0)
                _employeeList = _employeeList.Where(a => a.Id == employeeId).ToList();
            _employeeList = _employeeList.Where(a => a.Name.ToLower().Contains(employeeName.ToLower())).ToList();
            employeeAttendanceDetailSB.Append("<table id=\"TBL_EmployeeAttendanceList\">");

            foreach (var _employee in _employeeList)
            {
                var _employeeStatistic = new EmployeeStatistics();
                _employeeStatistic.EmployeeId = _employee.Id;
                _employeeStatistic.Name = _employee.Name;
                _employeeStatistic.EmployeeImagePath = _employee.ImagePath;
                _employeeStatistic.Date = selectedDate.Value.ToShortDateString();

                // var _employeeAttendanceList = _attendanceList.Where(a => a.EmployeeId == _employee.Id).
                //    OrderBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second);
                List<Attendance> listAttenndance = _attendanceList.Where(a => a.EmployeeId == _employee.Id).ToList();
                List<Attendance> orderedListAttenndance = listAttenndance.OrderBy(a => a.Hour)
                                                                     .ThenBy(b => b.Minute)
                                                                     .ThenBy(c => c.Second)
                                                                     .ToList();

                //var _firstInAttendance = orderedListAttenndance.FirstOrDefault(a => a.InOutMode == "in");--Kishan Changed it.
                // (Removed the Limitation for Viewing records).
                var _firstInAttendance = orderedListAttenndance.FirstOrDefault();
                if (_firstInAttendance != null)
                {
                    // Gets the first attendance record with in
                    // consider this record as the employee IN time.
                    DateTime _firstInTime = new DateTime(_firstInAttendance.Year, _firstInAttendance.Month, _firstInAttendance.Day,
                                                  _firstInAttendance.Hour, _firstInAttendance.Minute, _firstInAttendance.Second);

                    _employeeStatistic.FirstIn = _firstInTime.ToLongTimeString();

                    Attendance _lastAttendance = null;
                    DateTime? _lastAttendanceTime = null;


                    if (orderedListAttenndance != null)
                    {

                        _lastAttendance = orderedListAttenndance.Last();

                        if (_lastAttendance != null)
                        {
                            _lastAttendanceTime = new DateTime(_lastAttendance.Year, _lastAttendance.Month, _lastAttendance.Day,
                                                  _lastAttendance.Hour, _lastAttendance.Minute, _lastAttendance.Second);

                            if (_lastAttendance.InOutMode == "in")
                            {
                                _employeeStatistic.LastOut = "In Office";
                                _employeeStatistic.Location = _locationList.FirstOrDefault(l => l.Id == _lastAttendance.LocationId);
                                if (selectedDate.Value.Date == Utility.GetDateTimeNow().Date)
                                {
                                    _lastAttendanceTime = Utility.GetDateTimeNow();
                                }
                            }
                            else
                            {
                                if (_exitLocationList.IndexOf(_lastAttendance.LocationId) != -1)
                                {
                                    _employeeStatistic.LastOut = _lastAttendanceTime.Value.ToLongTimeString();
                                    _employeeStatistic.Location = new Location { DeviceNo = 100, Floor = "Out of Office" };
                                }
                                else
                                {
                                    var _outFloor = _locationList.FirstOrDefault(l => l.Id == _lastAttendance.LocationId);
                                    _employeeStatistic.LastOut = "In Office";
                                    _employeeStatistic.Location = new Location { DeviceNo = 0, Floor = "Out (From " + _outFloor.Floor + ")" };

                                    if (selectedDate.Value.Date == Utility.GetDateTimeNow().Date)
                                    {
                                        _lastAttendanceTime = Utility.GetDateTimeNow();
                                    }
                                }
                            }
                        }
                    }

                    if (_firstInTime != null && _lastAttendanceTime != null)
                    {
                        _employeeStatistic.TimeInOffice = _lastAttendanceTime.Value - _firstInTime;
                        _employeeStatistic.TimeInside = _employeeStatistic.TimeInOffice;

                        var _employeeOutAttenedanceList = orderedListAttenndance;

                        for (int i = 0; i < _employeeOutAttenedanceList.ToList().Count; i++)
                        {
                            var _outOfficeEntry = _employeeOutAttenedanceList[i];
                            // Check for the attendance entry is not the first and the last entry for the day.
                            if (_outOfficeEntry.Id != _firstInAttendance.Id && _outOfficeEntry.Id != _lastAttendance.Id
                                && _outOfficeEntry.InOutMode == "out")
                            {
                                // Check if the attendece entry is from any exit location.
                                if (_exitLocationList.IndexOf(_outOfficeEntry.LocationId) != -1)
                                {
                                    // Check the next attendance entry is not the last entry
                                    if ((i + 1) < (_employeeOutAttenedanceList.Count - 1))
                                    {
                                        if (_employeeOutAttenedanceList[i + 1].InOutMode == "in")
                                        {
                                            var _backtoOfficeEntry = _employeeOutAttenedanceList[i + 1];
                                            DateTime _outOfOfficeTime = new DateTime(_outOfficeEntry.Year, _outOfficeEntry.Month, _outOfficeEntry.Day,
                                                   _outOfficeEntry.Hour, _outOfficeEntry.Minute, _outOfficeEntry.Second);
                                            DateTime _backtoOfficeTime = new DateTime(_backtoOfficeEntry.Year, _backtoOfficeEntry.Month, _backtoOfficeEntry.Day,
                                                   _backtoOfficeEntry.Hour, _backtoOfficeEntry.Minute, _backtoOfficeEntry.Second);
                                            TimeSpan _tsOutOfOffice = _backtoOfficeTime - _outOfOfficeTime;
                                            _employeeStatistic.TimeInside = _employeeStatistic.TimeInside.Subtract(_tsOutOfOffice);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    _employeeStatisticList.Add(_employeeStatistic);

                    string EmplployeeImageurl = ConfigurationManager.AppSettings["EmployeeWebApi"].Replace("/api/Employee/{0}", "/Content/ProfilePic/") + _employeeStatistic.EmployeeImagePath;
                    employeeAttendanceDetailSB.Append("<tr><td style=\"padding-top:7px;padding-bottom:7px;\"><table class=\"cleartable\"><tr>");
                    employeeAttendanceDetailSB.Append("<td rowspan=\"2\" class=\"TD_QuickListEmpImg\">");
                    if (string.IsNullOrEmpty(_employeeStatistic.EmployeeImagePath))
                        employeeAttendanceDetailSB.Append("<img class=\"QuickListEmpImg\" src=\"../../Content/images/default_user.png\" />");
                    else
                        // employeeAttendanceDetailSB.Append(string.Format("<img class=\"QuickListEmpImg\" src=\"../../Content/images/employee/{0}\" />", _employeeStatistic.EmployeeImagePath));

                        employeeAttendanceDetailSB.Append(string.Format("<img class=\"QuickListEmpImg\" src=\"{0}\" />", EmplployeeImageurl));

                    employeeAttendanceDetailSB.Append(string.Format("</td><td class=\"QuickListEmpName\">{0}</td>", _employeeStatistic.Name));
                    employeeAttendanceDetailSB.Append("</tr><tr><td class=\"QuickListEmpContent\"><table class=\"cleartable\">");


                    var allEmployeeAttendances = listAttenndance.OrderBy(a => a.Year).ThenBy(a => a.Month).ThenBy(a => a.Day).ThenBy(a => a.Hour)
                    .ThenBy(a => a.Minute).ThenBy(a => a.Second);

                    var compressedAllAttendances = new List<Attendance>();
                    Attendance previousItem1 = null;

                    if ((_employeeStatistic.EmployeeId == loggedUser.EmployeeId) || (Utility.IsUserInRole("Administrator")) || (Utility.IsUserInRole("Manager")) || (Utility.IsUserInRole("Top Manager")))
                    {
                        foreach (var employeeAttendance in allEmployeeAttendances)
                        {
                            if (previousItem1 != null && employeeAttendance.LocationId == previousItem1.LocationId)
                            {
                                if (employeeAttendance.Hour == previousItem1.Hour && employeeAttendance.Minute == previousItem1.Minute && Math.Abs(employeeAttendance.Second - previousItem1.Second) <= 5)
                                {
                                    continue;
                                }
                            }

                            employeeAttendance.Location = _locationList.FirstOrDefault(l => l.Id == employeeAttendance.LocationId);

                            employeeAttendanceDetailSB.Append("<tr><td class=\"TD_Date_Text\">Date:</td>");
                            employeeAttendanceDetailSB.Append(string.Format("<td class=\"TD_Date_Value\">{0}/{1}/{2}</td>", employeeAttendance.Day.ToString("00"), employeeAttendance.Month.ToString("00"), employeeAttendance.Year));
                            employeeAttendanceDetailSB.Append("<td class=\"TD_Time_Text\">Time:</td>");
                            employeeAttendanceDetailSB.Append(string.Format("<td class=\"TD_Time_Value\">{0}:{1}:{2}</td>", employeeAttendance.Hour.ToString("00"), employeeAttendance.Minute.ToString("00"), employeeAttendance.Second.ToString("00")));
                            employeeAttendanceDetailSB.Append("<td class=\"TD_Mode_Text\">Mode:</td>");
                            employeeAttendanceDetailSB.Append(string.Format("<td class=\"TD_Mode_Value\">{0}</td>", employeeAttendance.InOutMode));
                            employeeAttendanceDetailSB.Append("<td class=\"TD_Location_Text\">Location:</td>");
                            employeeAttendanceDetailSB.Append(string.Format("<td class=\"TD_Location_Value\">{0}</td></tr>", employeeAttendance.Location != null ? employeeAttendance.Location.Floor : string.Empty));
                            previousItem1 = employeeAttendance;
                        }
                    }
                    else
                    {
                        var employeeAttendance = allEmployeeAttendances.ToList().LastOrDefault();
                        if (employeeAttendance != null)
                        {
                            employeeAttendanceDetailSB.Append("<tr><td class=\"TD_Date_Text\">Date:</td>");
                            employeeAttendanceDetailSB.Append(string.Format("<td class=\"TD_Date_Value\">{0}/{1}/{2}</td>", employeeAttendance.Day.ToString("00"), employeeAttendance.Month.ToString("00"), employeeAttendance.Year));
                            employeeAttendanceDetailSB.Append("<td class=\"TD_Time_Text\">Time:</td>");
                            employeeAttendanceDetailSB.Append(string.Format("<td class=\"TD_Time_Value\">{0}:{1}:{2}</td>", employeeAttendance.Hour.ToString("00"), employeeAttendance.Minute.ToString("00"), employeeAttendance.Second.ToString("00")));
                            employeeAttendanceDetailSB.Append("<td class=\"TD_Mode_Text\">Mode:</td>");
                            employeeAttendanceDetailSB.Append(string.Format("<td class=\"TD_Mode_Value\">{0}</td>", employeeAttendance.InOutMode));
                            employeeAttendanceDetailSB.Append("<td class=\"TD_Location_Text\">Location:</td>");
                            employeeAttendanceDetailSB.Append(string.Format("<td class=\"TD_Location_Value\">{0}</td></tr>", employeeAttendance.Location != null ? employeeAttendance.Location.Floor : string.Empty));
                        }
                    }

                    employeeAttendanceDetailSB.Append("</table></td></tr></table></td></tr>");
                }
            }

            employeeAttendanceDetailSB.Append("</table>");

            for (int i = 0; i < _employeeStatisticList.Count; i++)
            {
                _employeeStatisticList[i].LocationString = _employeeStatisticList[i].Location.Floor;
                _employeeStatisticList[i].TimeInOfficeString = GetTimeStringForTimeSpan(_employeeStatisticList[i].TimeInOffice);
                _employeeStatisticList[i].TimeInsideString = GetTimeStringForTimeSpan(_employeeStatisticList[i].TimeInside);
            }

            #endregion

            bool _foundUser = false;

            EmployeeStatistics myStatistic = _employeeStatisticList.ToList().FirstOrDefault(a => a.EmployeeId == loggedUser.EmployeeId);
            if (myStatistic != null)
            {
                _employeeStatisticList.Insert(0, myStatistic);
                _employeeStatisticList.RemoveAt(_employeeStatisticList.Select(T => T.EmployeeId).ToList().LastIndexOf(loggedUser.EmployeeId));
            }


            employeeLocationSB.Append("<table id=\"TBL_QuickFindEmployeeList\">");
            foreach (var employeeStastic in _employeeStatisticList)
            {
                string EmplployeeImageurl = ConfigurationManager.AppSettings["EmployeeWebApi"].Replace("/api/Employee/{0}", "/Content/ProfilePic/") + employeeStastic.EmployeeImagePath;

                _foundUser = true;
                employeeLocationSB.Append("<tr><td onclick=\"new QuickFind().ShowEmployeeAttendnace(" + employeeStastic.EmployeeId + ");\"><table class=\"cleartable\"><tr><td rowspan=\"2\" class=\"TD_QuickListEmpImg\">");
                if (string.IsNullOrEmpty(employeeStastic.EmployeeImagePath))
                    employeeLocationSB.Append("<img class=\"QuickListEmpImg\" src=\"../../Content/images/default_user.png\" />");
                else
                    employeeLocationSB.Append(string.Format("<img class=\"QuickListEmpImg\" src=\"{0}\" />", EmplployeeImageurl));
                employeeLocationSB.Append(string.Format("<td class=\"QuickListEmpName\" colspan=\"8\">{0} ~ {1}</td></tr><tr>", employeeStastic.Name, employeeStastic.Location.Floor));
                if ((employeeStastic.EmployeeId == loggedUser.EmployeeId) || (Utility.IsUserInRole("Administrator")) || (Utility.IsUserInRole("Manager")) || (Utility.IsUserInRole("Top Manager")))
                {
                    employeeLocationSB.Append("<td class=\"QuickListEmpContent TD_FirstIn_Text\">First:</td>");
                    employeeLocationSB.Append(string.Format("<td class=\"QuickListEmpContent TD_FirstIn_Value\">{0}</td>", employeeStastic.FirstIn));

                    employeeLocationSB.Append("<td class=\"QuickListEmpContent TD_LastOut_Text\">Last:</td>");
                    employeeLocationSB.Append(string.Format("<td class=\"QuickListEmpContent TD_LastOut_Value\">{0}</td>", employeeStastic.LastOut));

                    //employeeLocationSB.Append("<td class=\"QuickListEmpContent TD_TotalTimeIn_Text\">Total in Office:</td>");
                    //employeeLocationSB.Append(string.Format("<td class=\"QuickListEmpContent TD_TotalTimeIn_Value\">{0}</td>", employeeStastic.TimeInOfficeString));

                    employeeLocationSB.Append("<td class=\"QuickListEmpContent TD_TotalTimeInSide_Text\">Total Time Inside Office:</td>");
                    employeeLocationSB.Append(string.Format("<td class=\"QuickListEmpContent TD_TotalTimeInSide_Value\">{0}</td>", employeeStastic.TimeInsideString));
                }
                else
                {
                    employeeLocationSB.Append("<td colspan=\"8\"></td>");
                }

                employeeLocationSB.Append("</tr></table></td></tr>");
            }

            employeeLocationSB.Append("</table>");

            if (!_foundUser)
            {
                employeeLocationSB.Clear();
                employeeLocationSB.Append("0");
            }

            model.AttendanceListEntryContent = employeeAttendanceDetailSB.ToString();
            model.EmployeeLocationContent = employeeLocationSB.ToString();
            return model;
        }

        internal static ViewModels.AttendanceListViewModel QuickFindVisitorLocation(int? VisitorId, System.DateTime? pickeddate)
        {
            Context dbContext = new Context();

            #region Checking for the date selected
            int day = 0;
            int month = 0;
            int year = 0;

            if (pickeddate == null)
            {
                pickeddate = Utility.GetDateTimeNow();
                day = pickeddate.Value.Day;
                month = pickeddate.Value.Month;
                year = pickeddate.Value.Year;
            }
            else
            {
                day = pickeddate.Value.Day;
                month = pickeddate.Value.Month;
                year = pickeddate.Value.Year;
            }
            #endregion

            IQueryable<VisitorAttendance> result;
            IQueryable<VisitorAttendance> subResult;
            var visitorList = new List<VisitorStatistics>();
            IQueryable<IGrouping<int, VisitorAttendance>> group;
            // For the inner HTML Generation for multiple views
            StringBuilder employeeLocationSB = new StringBuilder();
            StringBuilder employeeAttendanceDetailSB = new StringBuilder();
            ViewModels.AttendanceListViewModel model = new ViewModels.AttendanceListViewModel();
            List<Location> _locationList = dbContext.Locations.OrderBy(a => a.Floor).ToList();
           
            //List<int> _exitLocationList = Exilesoft.MyTime.Repositories.TimeTrendRepository.GetExitLocationMachins();

            employeeAttendanceDetailSB.Append("<table id=\"TBL_EmployeeAttendanceList\">");

            #region --- Generating Visitor Statistics ---

            #region Getting the Attendance list

            if (VisitorId != null && VisitorId > 0)
                result = from m in dbContext.VisitorAttendances.Where(a => a.CardNo == VisitorId && a.LocationId==32) select m;
            else
                result = from m in dbContext.VisitorAttendances.Where(a => a.LocationId == 32) select m;
            subResult = result.Where(a => a.DateTime.Day == day).Where(a => a.DateTime.Month == month).Where(a => a.DateTime.Year == year);
            group = subResult.GroupBy(a => a.CardNo);
            var firstIndictionary = new Dictionary<int, VisitorAttendance>();

            #endregion

            #region Going throug person by person

            foreach (var groupitem in group)
            {
                var inTime = new DateTime();
                var outTime = new DateTime();
                var visStat = new VisitorStatistics();
                bool firstInFound = false;

                var groupItems = groupitem.OrderBy(a => a.DateTime);

                #region Getting the location if still in the office

                var lastInItem = groupItems.Last();
                //getting the last known location if not out from basement or main entrance
                Location loc = _locationList.FirstOrDefault(l => l.Id == lastInItem.LocationId);
                Location outLoc = new Location { DeviceNo = 100, Floor = "Out of Office" };
                Location unkonwnLoc = new Location { DeviceNo = 0, Floor = "Out (From " + loc.Floor + ")" };
                if (lastInItem.InOutMode == "in")
                    visStat.Location = loc;
                if (lastInItem.InOutMode == "out")
                {
                    if (loc.DeviceNo == 2 || loc.DeviceNo == 3)
                        visStat.Location = outLoc;
                    else
                        visStat.Location = unkonwnLoc;
                }

                #endregion

                #region Going through each entry for visitor

                foreach (var item in groupItems)
                {
                    visStat.VisitorId = item.CardNo;
                    visStat.VisitorRepresentation = item.CardNo.ToString();
                    item.Location = _locationList.FirstOrDefault(a => a.Id == item.LocationId);

                    if (item.InOutMode == "in")
                    {
                        inTime = new DateTime(item.DateTime.Year, item.DateTime.Month, item.DateTime.Day,
                                              item.DateTime.Hour, item.DateTime.Minute, item.DateTime.Second);
                        visStat.FirstIn = inTime.ToString("HH:mm");
                        firstIndictionary.Add(item.CardNo, item);
                        firstInFound = true;
                    }

                    if (firstInFound)
                    {
                        var lastItem = groupItems.Last();
                        lastItem.Location = _locationList.FirstOrDefault(a => a.Id == lastItem.LocationId);
                        if (lastItem.InOutMode == "out" && (lastItem.Location.DeviceNo == 2 || lastItem.Location.DeviceNo == 3)) //made a change to check the two out points
                        {
                            outTime = new DateTime(lastItem.DateTime.Year, lastItem.DateTime.Month, lastItem.DateTime.Day,
                                                   lastItem.DateTime.Hour, lastItem.DateTime.Minute, lastItem.DateTime.Second);
                            visStat.LastOut = outTime.ToString("HH:mm");
                            visStat.TimeInOffice = outTime.Subtract(inTime);

                        }
                        else
                        {
                            //visStat.LastOut = "In Office";
                            visStat.LastOut = "-";
                            if (pickeddate.Value.Date == Utility.GetDateTimeNow().Date)
                            {

                                visStat.TimeInOffice = Utility.GetDateTimeNow().Subtract(inTime);
                            }
                            else
                            {
                                DateTime dt = new DateTime(year, month, day, 23, 0, 0);
                                visStat.TimeInOffice = dt.Subtract(inTime);
                            }
                        }

                        break;
                    }
                    //visStat.FirstIn = "Out Of Office";
                    visStat.FirstIn = "-";
                    outTime = new DateTime(item.DateTime.Year, item.DateTime.Month, item.DateTime.Day,
                                              item.DateTime.Hour, item.DateTime.Minute, item.DateTime.Second);
                    visStat.LastOut = outTime.ToString("HH:mm");
                }

                #region --- Generate Inner HTML Content ---

                employeeAttendanceDetailSB.Append("<tr><td style=\"padding-top:7px;padding-bottom:7px;\"><table class=\"cleartable\"><tr>");
                employeeAttendanceDetailSB.Append("<td rowspan=\"2\" class=\"TD_QuickListEmpImg\">");
                employeeAttendanceDetailSB.Append("<img class=\"QuickListEmpImg\" src=\"../../Content/images/default_user.png\" />");
                employeeAttendanceDetailSB.Append(string.Format("</td><td class=\"QuickListEmpName\">{0}</td>", lastInItem.CardNo));
                employeeAttendanceDetailSB.Append("</tr><tr><td class=\"QuickListEmpContent\"><table class=\"cleartable\">");

                foreach (var item in groupItems)
                {
                    visStat.VisitorId = item.CardNo;
                    visStat.VisitorRepresentation = item.CardNo.ToString();
                    item.Location = _locationList.FirstOrDefault(a => a.Id == item.LocationId);

                    employeeAttendanceDetailSB.Append("<tr><td class=\"TD_Date_Text\">Date:</td>");
                    employeeAttendanceDetailSB.Append(string.Format("<td class=\"TD_Date_Value\">{0}/{1}/{2}</td>", item.DateTime.Day.ToString("00"), item.DateTime.Month.ToString("00"), item.DateTime.Year));
                    employeeAttendanceDetailSB.Append("<td class=\"TD_Time_Text\">Time:</td>");
                    employeeAttendanceDetailSB.Append(string.Format("<td class=\"TD_Time_Value\">{0}:{1}:{2}</td>", item.DateTime.Hour.ToString("00"), item.DateTime.Minute.ToString("00"), item.DateTime.Second.ToString("00")));
                    employeeAttendanceDetailSB.Append("<td class=\"TD_Mode_Text\">Mode:</td>");
                    employeeAttendanceDetailSB.Append(string.Format("<td class=\"TD_Mode_Value\">{0}</td>", item.InOutMode));
                    employeeAttendanceDetailSB.Append("<td class=\"TD_Location_Text\">Location:</td>");
                    employeeAttendanceDetailSB.Append(string.Format("<td class=\"TD_Location_Value\">{0}</td></tr>", item.Location != null ? item.Location.Floor : string.Empty));

                }

                employeeAttendanceDetailSB.Append("</table></td></tr></table></td></tr>");

                #endregion

                #endregion

                visitorList.Add(visStat);
            }

            employeeAttendanceDetailSB.Append("</table>");

            #endregion

            //performanceTest.Add("3- " + Utility.GetDateTimeNow().ToLongTimeString());
            //get first in record and all other attendance records from the basement and main entrance + all other in's
            var deviceList = new List<int> { 2, 3 };
            var validResult = from c in result
                              where
                                  (c.DateTime.Day == day && c.DateTime.Month == month && c.DateTime.Year == year) &&
                                  (deviceList.Contains(c.Location.DeviceNo) || c.InOutMode == "in")
                              select c;

            var validGroup = validResult.GroupBy(a => a.CardNo);
            //get first in record and all other attendance records from the basement and main entrance
            //performanceTest.Add("4- " + Utility.GetDateTimeNow().ToLongTimeString());
            //actual time in office

            var attendancePairs = new List<VisitorAttendance>();
            foreach (var groupItem1 in validGroup)
            {
                var attendanceItems = groupItem1.OrderBy(a => a.DateTime);
                int visId = 0;
                var attendanceInsBeforeOutItem = new List<VisitorAttendance>();
                foreach (var item in attendanceItems)
                {
                    visId = item.CardNo;
                    // todo: find locationId for device 2 and 3  and compare it
                    //item.Location = db.Locations.Find(item.LocationId);

                    //in out pairing
                    //if (item.InOutMode == "out" && (item.Location.DeviceNo == 2 || item.Location.DeviceNo == 3)) //made a change to check the two out points
                    if (item.InOutMode == "out" && (item.LocationId == 8 || item.LocationId == 9))
                    {
                        if (attendanceInsBeforeOutItem.Count != 0 && attendanceInsBeforeOutItem.First().InOutMode == "in")
                        {
                            attendancePairs.Add(attendanceInsBeforeOutItem.First());
                            attendancePairs.Add(item);
                            attendanceInsBeforeOutItem.Clear();
                        }
                    }

                    if (item.InOutMode == "in")
                        attendanceInsBeforeOutItem.Add(item);
                }

                //Here there will be 2, 3 out's which are covered by the grouping as well as only the in's from other devices
                if (attendanceItems.Last().InOutMode == "in")
                {
                    attendancePairs.Add(attendanceInsBeforeOutItem.First());
                    if (pickeddate.Value.Date == Utility.GetDateTimeNow().Date)
                    {
                        attendancePairs.Add(new VisitorAttendance
                        {
                            CardNo = visId,
                            DateTime = Utility.GetDateTimeNow()
                        });
                    }
                    else
                    {
                        attendancePairs.Add(new VisitorAttendance
                        {
                            CardNo = visId,
                            DateTime = Utility.GetDateTimeNow()
                        });
                    }
                }
            }

            //performanceTest.Add("5- " + Utility.GetDateTimeNow().ToLongTimeString());
            var dictionary = new Dictionary<int, TimeSpan>();
            if (attendancePairs.Count > 0)
            {
                int id = 0;
                var timePairGroups = attendancePairs.GroupBy(a => a.CardNo);
                foreach (var timePairGroup in timePairGroups)
                {
                    IOrderedEnumerable<VisitorAttendance> orderedSample = timePairGroup.OrderBy(a => a.DateTime);
                    int count = 0;

                    DateTime attendanceInTime = new DateTime();
                    DateTime attendanceTime = new DateTime();
                    TimeSpan time = new TimeSpan();
                    foreach (var attendance in orderedSample)
                    {
                        count++;
                        attendanceTime = new DateTime(attendance.DateTime.Year, attendance.DateTime.Month,
                                                      attendance.DateTime.Day, attendance.DateTime.Hour,
                                                      attendance.DateTime.Minute, attendance.DateTime.Second);
                        if (count == 2)
                        {
                            time = time + attendanceTime.Subtract(attendanceInTime);
                            id = attendance.CardNo; // move statement before dictinary add
                            count = 0;

                        }
                        else
                        {
                            attendanceInTime = attendanceTime;
                        }
                    }

                    dictionary.Add(id, time);
                }


                foreach (var item in visitorList)
                {
                    foreach (var item1 in dictionary)
                    {
                        if (item1.Key == item.VisitorId)
                            item.TimeInside = item1.Value;
                    }
                }

                //actual time in office
                //performanceTest.Add("6- " + Utility.GetDateTimeNow().ToLongTimeString());
                //Error flag
                //here we need to add converge logic as well as get the revised list

                var dictionary1 = new Dictionary<int, bool>();
                //VisitorAttendance attendancePreviousItem = null;

                var validResult1 = from c in result
                                   where
                                       (c.DateTime.Day == day && c.DateTime.Month == month && c.DateTime.Year == year) &&
                                       (deviceList.Contains(c.Location.DeviceNo))
                                   select c;
                var validResult2 = validResult1.ToList();

                var validGroup1 = validResult2.GroupBy(a => a.CardNo);
                //performanceTest.Add("7- " + Utility.GetDateTimeNow().ToLongTimeString());
                foreach (var groupItem in validGroup1)
                {
                    int inCount = 0;
                    int outCount = 0;

                    var attItems1 = groupItem.ToList();
                    foreach (var fIn in firstIndictionary)
                    {

                        if (fIn.Key == attItems1.First().CardNo && (fIn.Value.Location.DeviceNo != 2 || fIn.Value.Location.DeviceNo != 3))
                        {
                            attItems1.Add(fIn.Value);
                        }
                    }

                    var attItems = attItems1.OrderBy(a => a.DateTime);


                    //compress
                    var compressedAttendances = new List<VisitorAttendance>();
                    VisitorAttendance previousItem = null;
                    foreach (var visitorAttendance in attItems)
                    {
                        visitorAttendance.Location = dbContext.Locations.Find(visitorAttendance.LocationId);
                        if (previousItem != null)
                        {
                            if (visitorAttendance.Location.DeviceNo == previousItem.Location.DeviceNo && visitorAttendance.DateTime.Hour == previousItem.DateTime.Hour && visitorAttendance.DateTime.Minute == previousItem.DateTime.Minute && Math.Abs(visitorAttendance.DateTime.Second - previousItem.DateTime.Second) <= 5)
                            {
                                break;
                            }
                        }
                        compressedAttendances.Add(visitorAttendance);
                        previousItem = visitorAttendance;
                    }

                    //compress
                    if (compressedAttendances.First().InOutMode != "out")
                    {

                        foreach (var attendance in compressedAttendances)
                        {
                            if (attendance.InOutMode == "in")
                            {
                                if (attendance.Equals(attItems.Last()))
                                {
                                    break;
                                }

                                inCount++;
                                outCount = 0;
                                if (inCount > 1)
                                {

                                    dictionary1.Add(attendance.CardNo, true);
                                    break;
                                }
                            }

                            else if (attendance.InOutMode == "out")
                            {
                                outCount++;
                                inCount = 0;
                                if (outCount > 1)
                                {

                                    dictionary1.Add(attendance.CardNo, true);
                                    break;
                                }

                            }
                        }
                    }
                    else
                    {

                        dictionary1.Add(compressedAttendances.First().CardNo, true);
                    }
                }

                foreach (var item in visitorList)
                {
                    foreach (var item1 in dictionary1)
                    {
                        if (item1.Key == item.VisitorId)
                        {
                            item.ErrorFlag = item1.Value;
                        }
                    }
                }
            }

            #endregion

            #region --- Generating InnerHTML Content ---

            bool _foundUser = false;
            employeeLocationSB.Append("<table id=\"TBL_QuickFindEmployeeList\">");

            foreach (var visitorStatistic in visitorList)
            {
                _foundUser = true;
                employeeLocationSB.Append("<tr><td onclick=\"new QuickFind().ShowVisitorAttendnace(" + visitorStatistic.VisitorId + ");\"><table class=\"cleartable\"><tr><td rowspan=\"2\" class=\"TD_QuickListEmpImg\">");
                employeeLocationSB.Append("<img class=\"QuickListEmpImg\" src=\"../../Content/images/default_user.png\" />");
                employeeLocationSB.Append(string.Format("<td class=\"QuickListEmpName\" colspan=\"8\">{0} ~ {1}</td></tr><tr>", visitorStatistic.VisitorId, visitorStatistic.Location.Floor));

                //employeeLocationSB.Append("<td class=\"QuickListEmpContent TD_FirstIn_Text\">First:</td>");
                //employeeLocationSB.Append(string.Format("<td class=\"QuickListEmpContent TD_FirstIn_Value\">{0}</td>", visitorStatistic.FirstIn));

                //employeeLocationSB.Append("<td class=\"QuickListEmpContent TD_LastOut_Text\">Last:</td>");
                //employeeLocationSB.Append(string.Format("<td class=\"QuickListEmpContent TD_LastOut_Value\">{0}</td>", visitorStatistic.LastOut));

                //employeeLocationSB.Append("<td class=\"QuickListEmpContent TD_TotalTimeIn_Text\">Total in Office:</td>");
                //employeeLocationSB.Append(string.Format("<td class=\"QuickListEmpContent TD_TotalTimeIn_Value\">{0}</td>", GetTimeStringForTimeSpan(visitorStatistic.TimeInOffice)));

                //employeeLocationSB.Append("<td class=\"QuickListEmpContent TD_TotalTimeInSide_Text\">Time In Side Office:</td>");
                //employeeLocationSB.Append(string.Format("<td class=\"QuickListEmpContent TD_TotalTimeInSide_Value\">{0}</td>", GetTimeStringForTimeSpan(visitorStatistic.TimeInside)));

                employeeLocationSB.Append("</tr></table></td></tr>");
            }

            employeeLocationSB.Append("</table>");

            if (!_foundUser)
            {
                employeeLocationSB.Clear();
                employeeLocationSB.Append("0");
            }

            #endregion

            model.AttendanceListEntryContent = employeeAttendanceDetailSB.ToString();
            model.EmployeeLocationContent = employeeLocationSB.ToString();
            return model;
        }

        internal static string GetTimeStringForTimeSpan(TimeSpan ts)
        {
            int totalMinutes = (int)ts.TotalMinutes;
            int hours = (int)totalMinutes / 60;
            int mins = (int)totalMinutes % 60;

            return hours.ToString("00") + ":" + mins.ToString("00");
        }



        public void ModifyVisitorAttendances(int cardId, int allocationId, int informationId, DateTime date)
        {
            var dbContext = new Context();
            var fromDate = date;
            var toDate = date.AddDays(1);

            var visitorAttendancesList =
                dbContext.VisitorAttendances.Where(
                    v => v.CardNo == cardId && v.VisitorPassAllocationId == null &&
                         (v.DateTime >= fromDate && v.DateTime < toDate)
                    );

            visitorAttendancesList.ForEach(v => v.VisitorPassAllocationId = allocationId);
            visitorAttendancesList.ForEach(v => v.VisitInformationId = informationId);

            dbContext.SaveChanges();
        }

        public void ModifyCardNumber(int newCardId, int allocationId, int employeeId, DateTime date)
        {
            var dbContext = new Context();

            var visitorPassAllocation = dbContext.VisitorPassAllocations.First(a => a.Id == allocationId);
            visitorPassAllocation.CardNo = newCardId;
            dbContext.SaveChanges();
            AllocatedHistoryAttendance(newCardId, allocationId, employeeId, date);
        }

        public void AllocatedHistoryAttendance(int newCardId, int allocationId, int employeeId, DateTime date)
        {
            var dbContext = new Context();
            var fromDate = date;
            var toDate = date.AddDays(1);

            //var visitorPassAllocation = dbContext.VisitorPassAllocations.First(a => a.Id == allocationId);
            //visitorPassAllocation.CardNo = newCardId;

            var visitorAttendancesList =
                dbContext.VisitorAttendances.Where(
                    v => v.CardNo == newCardId && v.VisitorPassAllocationId == null && (v.DateTime >= fromDate && v.DateTime < toDate));
            var visitorAttendancesListOriginal = visitorAttendancesList.ToList();
            visitorAttendancesList.ForEach(v => v.VisitorPassAllocationId = allocationId);
            visitorAttendancesList.ForEach(v => v.EmployeeId = employeeId);
            visitorAttendancesList.ForEach(v => v.isTransferred = true);
            dbContext.SaveChanges();


            foreach (var visitorAttendance in visitorAttendancesListOriginal)
            {
                var newAttendance = new Attendance
                                        {
                                            CardNo = newCardId,
                                            EmployeeId = employeeId,
                                            Year = visitorAttendance.DateTime.Year,
                                            Month = visitorAttendance.DateTime.Month,
                                            Day = visitorAttendance.DateTime.Day,
                                            Hour = visitorAttendance.DateTime.Hour,
                                            Minute = visitorAttendance.DateTime.Minute,
                                            Second = visitorAttendance.DateTime.Second,
                                            VerifyMode = visitorAttendance.VerifyMode,
                                            InOutMode = visitorAttendance.InOutMode,
                                            WorkCode = visitorAttendance.WorkCode,
                                            LocationId = visitorAttendance.LocationId,
                                        };

                dbContext.Attendances.Add(newAttendance);

            }

            dbContext.SaveChanges();


        }

        // ShowDetailInOut
        internal static ViewModels.AttendanceListViewModel getEmployeeByFloor(int locationId, DateTime selectedDate, string userType)
        {



            var dbContext = new Context();
            ViewModels.AttendanceListViewModel model = new ViewModels.AttendanceListViewModel();

            if (userType == "Visitor")
            {
                //var EmployeeIDList = dbContext.VisitorAttendances.Where(i => i.DateTime.Year == selectedDate.Year
                //        && i.DateTime.Month == selectedDate.Month
                //        && i.DateTime.Day == selectedDate.Day
                //        && i.LocationId == locationId).GroupBy(p => p.CardNo).Select(grp => grp.FirstOrDefault()).Select(p => p.CardNo).ToList();                

                var EmployeeIDListAtendence = dbContext.VisitorAttendances.Where(i => i.DateTime.Year == selectedDate.Year
                        && i.DateTime.Month == selectedDate.Month
                        && i.DateTime.Day == selectedDate.Day
                        && i.LocationId == locationId).ToList();

                var EmployeeIDList = EmployeeIDListAtendence.GroupBy(p => p.CardNo).Select(grp => grp.FirstOrDefault()).Select(p => p.CardNo).ToList();

                StringBuilder SB = new StringBuilder();
                SB.Append("<Div class=\"EmployeeList\" ><table id=\"TBL_EmployeeList\" class=\"TBL_EmployeeList\">");
                foreach (var emp in EmployeeIDList)
                {
                    SB.Append("<tr colspan=2><td  onclick=\"new AdministrationForm().ShowDetailInOut(" + emp.ToString() + ",'" + emp.ToString() + "');\"><table class=\"cleartable\"><tr><td  class=\"TD_QuickListEmpImg\">");
                    SB.Append("<img class=\"QuickListEmpImg\" src=\"../../Content/images/default_user.png\" />");
                    SB.Append("</tr></table></td>");


                    SB.Append("<td onclick=\"new AdministrationForm().ShowDetailInOut(" + emp.ToString() + ",'" + emp.ToString() + "');\">" + emp.ToString() + "</td>");


                    SB.Append("</tr>");

                }
                SB.Append("</table></Div>");

                model.EmployeeLocationContent = SB.ToString();
                return model;


            }
            else
            {
                IList<EmployeeData> empList = new List<EmployeeData>();
                var EmployeeIDList = dbContext.Attendances.Where(i => i.Year == selectedDate.Year
                    && i.Month == selectedDate.Month
                    && i.Day == selectedDate.Day
                    && i.LocationId == locationId).GroupBy(p => p.EmployeeId).Select(grp => grp.FirstOrDefault()).Select(p => p.EmployeeId).ToList();

                empList = EmployeeRepository.GetAllEmployees().Where(p => EmployeeIDList.Contains(p.Id)).ToList<EmployeeData>();
                empList = empList.OrderBy(p => p.Name).ToList();

                StringBuilder SB = new StringBuilder();
                SB.Append("<Div class=\"EmployeeList\" ><table id=\"TBL_EmployeeList\" class=\"TBL_EmployeeList\">");


                foreach (var emp in empList)
                {
                    string EmplployeeImageurl = ConfigurationManager.AppSettings["EmployeeWebApi"].Replace("/api/Employee/{0}", "/Content/ProfilePic/") + emp.ImagePath;
                    SB.Append("<tr colspan=2><td  onclick=\"new AdministrationForm().ShowDetailInOut(" + emp.Id + ",'" + emp.Name + "');\"><table class=\"cleartable\"><tr><td  class=\"TD_QuickListEmpImg\">");
                    if (string.IsNullOrEmpty(emp.ImagePath))
                    {
                        SB.Append("<img class=\"QuickListEmpImg\" src=\"../../Content/images/default_user.png\" />");
                    }
                    else
                    {

                        SB.Append(string.Format("<img class=\"QuickListEmpImg\" src=\"{0}\" />", EmplployeeImageurl));
                        // SB.Append("<img class=\"QuickListEmpImg\" src=\"data:image/gif;base64," + MobileService.getUserImage(emp.Id) + "\" >");
                        //Should be implemented later.
                    }
                    SB.Append("</tr></table></td>");


                    SB.Append("<td onclick=\"new AdministrationForm().ShowDetailInOut(" + emp.Id + ",'" + emp.Name + "');\">" + emp.Name + "</td>");


                    SB.Append("</tr>");

                }
                SB.Append("</table></Div>");

                model.EmployeeLocationContent = SB.ToString();
                return model;

            }


        }





        internal static ViewModels.AttendanceListViewModel ShowDetailInOut(int locationId, DateTime selectedDate, int UserId, string userName, string UserType)
        {
            var dbContext = new Context();
            ViewModels.AttendanceListViewModel model = new ViewModels.AttendanceListViewModel();

            if (UserType == "Visitor")
            {
                IList<VisitorAttendance> atendanceList = dbContext.VisitorAttendances.Where(i => i.DateTime.Year == selectedDate.Year
                    && i.DateTime.Month == selectedDate.Month
                    && i.DateTime.Day == selectedDate.Day
                    && i.LocationId == locationId && i.CardNo == UserId).OrderBy(i => i.Id).ToList<VisitorAttendance>();


                StringBuilder SB = new StringBuilder();
                SB.Append("<Div class=\"AttendenceList\"><table id=\"TBL_AttendenceList\" class=\"TBL_AttendenceList\">");
                SB.Append("<tr ><td>" + userName + "<td></tr>");
                foreach (var att in atendanceList)
                {
                    SB.Append("<tr ><td>Mode :" + att.InOutMode + " Time ~" + att.DateTime.Hour + ":" + att.DateTime.Minute + " :" + att.DateTime.Second + "<td></tr>");
                }
                SB.Append("</table></Div>");

                model.EmployeeLocationContent = SB.ToString();

            }
            else
            {
                IList<Attendance> atendanceList = dbContext.Attendances.Where(i => i.Year == selectedDate.Year
                    && i.Month == selectedDate.Month
                    && i.Day == selectedDate.Day
                    && i.LocationId == locationId && i.EmployeeId == UserId).OrderBy(i => i.Id).ToList<Attendance>();


                StringBuilder SB = new StringBuilder();
                SB.Append("<Div class=\"AttendenceList\"><table id=\"TBL_AttendenceList\" class=\"TBL_AttendenceList\">");
                SB.Append("<tr ><td>" + userName + "<td></tr>");
                foreach (var att in atendanceList)
                {
                    SB.Append("<tr ><td>Mode :" + att.InOutMode + " Time ~" + att.Hour + ":" + att.Minute + " :" + att.Second + "<td></tr>");
                }
                SB.Append("</table></Div>");

                model.EmployeeLocationContent = SB.ToString();

            }

            return model;

        }



        public string GetCurrentLocationName(int visitorPassAllocationId)
        {
            using (var dbContext = new Context())
            {
                var attendanceList =
                    dbContext.VisitorAttendances.Where(v => v.VisitorPassAllocationId == visitorPassAllocationId).ToList();
                VisitorAttendance visitorAttendance = attendanceList.OrderBy(o => o.DateTime).LastOrDefault();
                if (visitorAttendance != null && visitorAttendance.Location != null)
                {
                    return visitorAttendance.Location.Floor;
                }
                return "";
            }
        }


    }
}