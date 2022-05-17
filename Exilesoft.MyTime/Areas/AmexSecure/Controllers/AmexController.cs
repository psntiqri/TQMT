using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Exilesoft.Models;
using Exilesoft.MyTime.Helpers;
using Exilesoft.MyTime.Repositories;
using Exilesoft.MyTime.Filters;
using Exilesoft.MyTime.Areas.AmexSecure.Filters;

namespace Exilesoft.MyTime.Areas.AmexSecure.Controllers
{
    public class AmexController : Controller
    {
        private Context db = new Context();
        //
        // GET: /AmexSecure/Amex/

        [AmexAuthentication]
        public ActionResult Index(int? EmployeeId, string pickeddate)
        {
            ViewBag.EmployeeID = EmployeeId != null ? EmployeeId.Value : 0;
            ViewBag.clickStatus = "Link";
            System.DateTime? selectedDate = ParseDate(pickeddate);

            ViewBag.PickDate = selectedDate != null ? selectedDate.Value.ToShortDateString() : System.DateTime.Today.ToShortDateString();
            int employeeId = int.Parse(Session["EmployeeId"].ToString());
            EmployeeEnrollment loggedUser = db.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == employeeId);

            ViewBag.EmployeeID = loggedUser.EmployeeId;

            ViewBag.SearchUserType = "Employee";
            return View();
        }

        [HttpPost]
        [AmexAuthentication]
        public JsonResult QuickFindLocation(int? EmployeeId, string employeeName, string selectedDate, string userType)
        {

            return Json(new { SearchResult = Exilesoft.MyTime.Areas.AmexSecure.Repositories.AttendanceRepository.QuickFindLocation(EmployeeId, employeeName,selectedDate, userType) });
        }

        //
        // GET: /Attendance/
        //[DelphiAuthentication]
        public ActionResult IndexBack(int? EmployeeId, string pickeddate)
        {
            System.DateTime? selectedDate = ParseDate(pickeddate);
            //user info
            ViewBag.Username = User.Identity.Name;
            EmployeeEnrollment loggedUser = db.EmployeeEnrollment.FirstOrDefault(a => a.UserName == User.Identity.Name);
            if (null == loggedUser)
            {
                TempData["ErrorMessage"] = "Active directory user not found.";
                return RedirectToAction("LogOn", "Account");
            }
            if (false == loggedUser.IsEnable)
            {
                TempData["ErrorMessage"] = "Active directory user inactive.";
                return RedirectToAction("LogOn", "Account");
            }
            ViewBag.Role = Utility.GetUserRoleInt();//loggedUser.Privillage;

            //for ordinary user show only his/her data
            if (Utility.IsUserInRole("Employee"))//(loggedUser.Privillage == 0)
            {
                EmployeeId = loggedUser.EmployeeId;
                ViewBag.EmployeeId = new SelectList(new[] { EmployeeRepository.GetEmployee(loggedUser.EmployeeId) }, "Id", "Name", "Id");
            }
            else
            {
                ViewBag.EmployeeId = new SelectList(EmployeeRepository.GetAllEmployeeOrderByName(), "Id", "Name", "Id");
            }

            ViewBag.Message = "AttendanceHistory";


            int day = 0;
            int month = 0;
            int year = 0;

            if (selectedDate == null)
            {
                DateTime date = Utility.GetDateTimeNow();
                day = date.Day;
                month = date.Month;
                year = date.Year;
                ViewBag.date = date.ToShortDateString();
            }
            else
            {
                day = selectedDate.Value.Day;
                month = selectedDate.Value.Month;
                year = selectedDate.Value.Year;
                ViewBag.date = selectedDate.Value.ToShortDateString();
            }

            if (EmployeeId > 0)
            {
                var employeeAttendances = db.Attendances.Include(a => a.Employee).Include(a => a.Location).Where(x => x.Employee.EmployeeId == EmployeeId).Where(x => x.Day == day).Where(x => x.Month == month).Where(x => x.Year == year)
                    .OrderBy(a => a.Year).ThenBy(a => a.Month).ThenBy(a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).
                    ThenBy(a => a.Second);

                var compressedAttendances = new List<Attendance>();
                Attendance previousItem = null;
                foreach (var employeeAttendance in employeeAttendances)
                {
                    if (previousItem != null && employeeAttendance.Location.DeviceNo == previousItem.Location.DeviceNo)
                    {
                        if (employeeAttendance.Hour == previousItem.Hour && employeeAttendance.Minute == previousItem.Minute && Math.Abs(employeeAttendance.Second - previousItem.Second) <= 5)
                        {
                            continue;
                        }
                    }

                    compressedAttendances.Add(employeeAttendance);
                    previousItem = employeeAttendance;
                }

                return View(compressedAttendances);
            }

            var allEmployeeAttendances = db.Attendances.Include(a => a.Employee).Include(a => a.Location).Where(a => a.Day == day).Where(x => x.Month == month).Where(x => x.Year == year).OrderBy(a => a.Employee.EmployeeId).ThenBy(a => a.Year).ThenBy(a => a.Month).ThenBy(a => a.Day).ThenBy(a => a.Hour)
               .ThenBy(a => a.Minute).ThenBy(a => a.Second);

            var compressedAllAttendances = new List<Attendance>();
            Attendance previousItem1 = null;
            foreach (var employeeAttendance in allEmployeeAttendances)
            {
                if (previousItem1 != null && employeeAttendance.Location.DeviceNo == previousItem1.Location.DeviceNo)
                {
                    if (employeeAttendance.Hour == previousItem1.Hour && employeeAttendance.Minute == previousItem1.Minute && Math.Abs(employeeAttendance.Second - previousItem1.Second) <= 5)
                    {
                        continue;
                    }
                }

                compressedAllAttendances.Add(employeeAttendance);
                previousItem1 = employeeAttendance;
            }

            return View(compressedAllAttendances);
            //return View(db.Attendances.Include(a => a.Employee).Include(a=>a.Location).Where(a => a.Day == day).Where(x => x.Month == month).Where(x => x.Year == year).OrderBy(a => a.Employee.Name).ThenBy(a => a.Year).ThenBy(a => a.Month).ThenBy(a => a.Day).ThenBy(a => a.Hour)
            //   .ThenBy(a => a.Minute).ThenBy(a => a.Second));  
        }

        // GET: /Attendance/Details/5

        public ViewResult Details(int id)
        {
            Attendance attendance = db.Attendances.Find(id);
            return View(attendance);
        }

        //[DelphiAuthentication]
        public ActionResult SearchIndex(int? EmployeeId, string pickeddate)
        {
            System.DateTime? selectedDate = ParseDate(pickeddate);
            ViewBag.TimeInsideOffice = "Employee Quick Find";
            List<EmployeeStatistics> _employeeStatisticList = new List<EmployeeStatistics>();
            var _employeeList = EmployeeRepository.GetAllEmployeeOrderByName();
            SelectList empList = null;

            var _locationList = db.Locations.OrderBy(a => a.Floor);
            List<int> _exitLocationList = Exilesoft.MyTime.Repositories.TimeTrendRepository.GetExitLocationMachins();

            #region --- Checking Logged In user details ---

            ViewBag.Username = User.Identity.Name;
            EmployeeEnrollment loggedUser = db.EmployeeEnrollment.FirstOrDefault(a => a.UserName == User.Identity.Name);
            if (null == loggedUser)
            {
                TempData["ErrorMessage"] = "Active directory user not found.";
                return RedirectToAction("LogOn", "Account");
            }
            if (false == loggedUser.IsEnable)
            {
                TempData["ErrorMessage"] = "Active directory user inactive.";
                return RedirectToAction("LogOn", "Account");
            }

            empList = new SelectList(_employeeList.ToList(), "Id", "Name");


            if (EmployeeId != null)
                _employeeList = _employeeList.Where(a => a.Id == EmployeeId).ToList();

            #region Checking the privilages

            ViewBag.Role = Utility.GetUserRoleInt();//loggedUser.Privillage;
            ViewBag.Name = loggedUser.UserName; //TODO: take the employee name from HR Web API 
            ViewBag.isCurrentUserView = false;
            if (EmployeeId == null && Utility.IsUserInRole("Employee"))//(EmployeeId == null && loggedUser.Privillage == 0)
            {
                EmployeeId = loggedUser.EmployeeId;
                ViewBag.isCurrentUserView = true;
            }

            ViewBag.SelectedEmployeeID = loggedUser.EmployeeId;
            ViewBag.EmployeeId = empList;

            ViewBag.Message = "QuickFind";

            #endregion

            #region Checking for the date selected

            int _day = 0;
            int _month = 0;
            int _year = 0;

            if (selectedDate == null)
            {

                selectedDate = Utility.GetDateTimeNow();
                _day = selectedDate.Value.Day;
                _month = selectedDate.Value.Month;
                _year = selectedDate.Value.Year;
                ViewBag.date = selectedDate.Value.ToShortDateString();
            }
            else
            {
                _day = selectedDate.Value.Day;
                _month = selectedDate.Value.Month;
                _year = selectedDate.Value.Year;
                ViewBag.date = selectedDate.Value.ToShortDateString();
            }

            #endregion

            #endregion

            var _attendanceList = db.Attendances.Where(a => a.Day == _day && a.Month == _month && a.Year == _year);
            if (EmployeeId != null)
                _employeeList = _employeeList.Where(a => a.Id == EmployeeId.Value).ToList();

            foreach (var _employee in _employeeList)
            {
                var _employeeStatistic = new EmployeeStatistics();
                _employeeStatistic.EmployeeId = _employee.Id;
                _employeeStatistic.Name = _employee.Name;
                _employeeStatistic.Date = selectedDate.Value.ToShortDateString();

                var _employeeAttendanceList = _attendanceList.Where(a => a.EmployeeId == _employee.Id).
                    OrderBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second);

                var _firstInAttendance = _employeeAttendanceList.FirstOrDefault(a => a.InOutMode == "in");
                if (_firstInAttendance != null)
                {
                    // Gets the first attendance record with in
                    // consider this record as the employee IN time.
                    DateTime _firstInTime = new DateTime(_firstInAttendance.Year, _firstInAttendance.Month, _firstInAttendance.Day,
                                                  _firstInAttendance.Hour, _firstInAttendance.Minute, _firstInAttendance.Second);

                    _employeeStatistic.FirstIn = _firstInTime.ToLongTimeString();

                    Attendance _lastAttendance = null;
                    DateTime? _lastAttendanceTime = null;

                    if (_employeeAttendanceList != null)
                    {
                        _lastAttendance = _employeeAttendanceList.ToList().Last();
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

                        var _employeeOutAttenedanceList = _employeeAttendanceList.ToList();
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
                }
            }


            return View(_employeeStatisticList);
        }


        //[DelphiAuthentication]
        public ActionResult SearchIndexBackup(int? EmployeeId, System.DateTime? pickeddate)
        {
            //List<string> performanceTest = new List<string>();//test performance
            //performanceTest.Add("0- " + Utility.GetDateTimeNow().ToLongTimeString());
            //user info
            #region Checking Logged In user details
            ViewBag.Username = User.Identity.Name;
            EmployeeEnrollment loggedUser = db.EmployeeEnrollment.FirstOrDefault(a => a.UserName == User.Identity.Name);
            if (null == loggedUser)
            {
                TempData["ErrorMessage"] = "Active directory user not found.";
                return RedirectToAction("LogOn", "Account");
            }
            if (false == loggedUser.IsEnable)
            {
                TempData["ErrorMessage"] = "Active directory user inactive.";
                return RedirectToAction("LogOn", "Account");
            }
            #endregion
            #region Checking the privilages

            ViewBag.Role = Utility.GetUserRoleInt();//loggedUser.Privillage;
            //TODO: Take the name from HR web API once it done
            ViewBag.Name = loggedUser.UserName; //loggedUser.Name; 
            ViewBag.isCurrentUserView = false;
            if (EmployeeId == null && Utility.IsUserInRole("Employee"))//(EmployeeId == null && loggedUser.Privillage == 0)
            {
                EmployeeId = loggedUser.EmployeeId;
                ViewBag.isCurrentUserView = true;
            }
            else if (EmployeeId != null && loggedUser.EmployeeId == EmployeeId)
            {
                EmployeeId = loggedUser.EmployeeId;
                ViewBag.isCurrentUserView = true;
            }


            //for ordinary user show only his/her data
            //if (loggedUser.Priv == 0)
            //{
            //EmployeeId = loggedUser.Id;
            //Sending all employee ids to view for location finder - AS-153
            //ViewBag.EmployeeId = new SelectList(db.Employees.Where(a => a.Username == User.Identity.Name).ToList(), "Id", "Name", "Id");
            //}
            //else
            //{
            //    var empList = new SelectList(db.Employees.OrderBy(a => a.Name).ToList(), "Id", "Name", "Id");

            //    ViewBag.EmployeeId = empList;
            //}

            var empList = new SelectList(EmployeeRepository.GetAllEmployeeOrderByName(), "Id", "Name");
            ViewBag.SelectedEmployeeID = loggedUser.EmployeeId;
            ViewBag.EmployeeId = empList;

            ViewBag.Message = "QuickFind";
            #endregion
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
                ViewBag.date = pickeddate.Value.ToShortDateString();
            }
            else
            {
                day = pickeddate.Value.Day;
                month = pickeddate.Value.Month;
                year = pickeddate.Value.Year;
                ViewBag.date = pickeddate.Value.ToShortDateString();
            }
            #endregion

            IQueryable<Attendance> result;
            IQueryable<Attendance> subResult;
            IQueryable<IGrouping<int, Attendance>> group;
            //performanceTest.Add("1- " +Utility.GetDateTimeNow().ToLongTimeString());

            #region Getting the Attendance list
            if (EmployeeId > 0)
            {
                result = from m in
                             db.Attendances.Where(a => a.EmployeeId == EmployeeId)
                         join item in db.EmployeeEnrollment on m.EmployeeId equals item.EmployeeId
                         //where m.Year==year && m.Month==month && m.Day==day
                         select m;

            }

            else
            {
                result = from m in
                             db.Attendances
                         join item in db.EmployeeEnrollment on m.EmployeeId equals item.EmployeeId
                         //where m.Year == year && m.Month == month && m.Day == day
                         select m;
            }
            #endregion
            #region Getting the Attendance result according to the date picked
            subResult = result.Where(a => a.Day == day).Where(a => a.Month == month).Where(a => a.Year == year);
            #endregion
            //performanceTest.Add("2- " + Utility.GetDateTimeNow().ToLongTimeString());
            #region Grouping the final results according to the enroll no
            group = subResult.GroupBy(a => a.CardNo);
            #endregion
            var employeeList = new List<EmployeeStatistics>();
            var firstIndictionary = new Dictionary<int, Attendance>();
            #region Going throug person by person
            foreach (var groupitem in group)
            {
                var inTime = new DateTime();
                var outTime = new DateTime();
                var empStat = new EmployeeStatistics();
                bool firstInFound = false;

                #region Getting results by date sorted
                var groupItems = groupitem.OrderBy(a => a.Year).ThenBy(a => a.Month).ThenBy(
                    a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second);
                #endregion
                #region Getting the location if still in the office
                var lastInItem = groupItems.Last();
                //getting the last known location if not out from basement or main entrance
                Location loc = db.Locations.Find(lastInItem.LocationId);
                Location outLoc = new Location { DeviceNo = 100, Floor = "Out of Office" };
                Location unkonwnLoc = new Location { DeviceNo = 0, Floor = "Out (From " + loc.Floor + ")" };
                if (lastInItem.InOutMode == "in")
                    empStat.Location = loc;
                if (lastInItem.InOutMode == "out")
                {
                    if (loc.DeviceNo == 2 || loc.DeviceNo == 3)
                        empStat.Location = outLoc;
                    else
                        empStat.Location = unkonwnLoc;
                }
                #endregion
                #region Going through each entry for employee
                foreach (var item in groupItems)
                {
                    empStat.EmployeeId = item.EmployeeId;
                    empStat.Name = EmployeeRepository.GetEmployee(item.EmployeeId).Name;
                    item.Location = db.Locations.Find(item.LocationId);

                    if (item.InOutMode == "in")
                    {
                        inTime = new DateTime(item.Year, item.Month, item.Day,
                                              item.Hour, item.Minute, item.Second);
                        empStat.FirstIn = inTime.ToString("HH:mm");
                        firstIndictionary.Add(item.EmployeeId, item);
                        firstInFound = true;
                    }

                    if (firstInFound)
                    {
                        var lastItem = groupItems.Last();
                        lastItem.Location = db.Locations.Find(lastItem.LocationId);
                        if (lastItem.InOutMode == "out" && (lastItem.Location.DeviceNo == 2 || lastItem.Location.DeviceNo == 3)) //made a change to check the two out points
                        {
                            outTime = new DateTime(lastItem.Year, lastItem.Month, lastItem.Day,
                                                   lastItem.Hour, lastItem.Minute, lastItem.Second);
                            empStat.LastOut = outTime.ToString("HH:mm");
                            empStat.TimeInOffice = outTime.Subtract(inTime);

                        }
                        else
                        {
                            empStat.LastOut = "In Office";
                            if (pickeddate.Value.Date == Utility.GetDateTimeNow().Date)
                            {
                                //empStat.LastOut = "In Office";
                                empStat.TimeInOffice = Utility.GetDateTimeNow().Subtract(inTime);
                            }
                            else
                            {
                                DateTime dt = new DateTime(year, month, day, 23, 0, 0);
                                empStat.TimeInOffice = dt.Subtract(inTime);
                            }
                        }

                        break;
                    }
                    empStat.FirstIn = "Out Of Office";
                    outTime = new DateTime(item.Year, item.Month, item.Day,
                                              item.Hour, item.Minute, item.Second);
                    empStat.LastOut = outTime.ToString("HH:mm");
                }
                #endregion
                employeeList.Add(empStat);
            }
            #endregion
            //performanceTest.Add("3- " + Utility.GetDateTimeNow().ToLongTimeString());
            //get first in record and all other attendance records from the basement and main entrance + all other in's
            var deviceList = new List<int> { 2, 3 };
            var validResult = from c in result
                              where
                                  (c.Day == day && c.Month == month && c.Year == year) &&
                                  (deviceList.Contains(c.Location.DeviceNo) || c.InOutMode == "in")
                              select c;

            var validGroup = validResult.GroupBy(a => a.CardNo);
            //get first in record and all other attendance records from the basement and main entrance
            //performanceTest.Add("4- " + Utility.GetDateTimeNow().ToLongTimeString());
            //actual time in office
            var attendancePairs = new List<Attendance>();
            foreach (var groupItem1 in validGroup)
            {
                var attendanceItems = groupItem1.OrderBy(a => a.Year).ThenBy(a => a.Month).ThenBy(
                    a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second);
                int empId = 0;
                var attendanceInsBeforeOutItem = new List<Attendance>();
                foreach (var item in attendanceItems)
                {
                    empId = item.EmployeeId;
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
                        attendancePairs.Add(new Attendance
                        {
                            EmployeeId = empId,
                            Year = Utility.GetDateTimeNow().Year,
                            Month = Utility.GetDateTimeNow().Month,
                            Day = Utility.GetDateTimeNow().Day,
                            Hour = Utility.GetDateTimeNow().Hour,
                            Minute = Utility.GetDateTimeNow().Minute,
                            Second = Utility.GetDateTimeNow().Second
                        });
                    }
                    else
                    {
                        attendancePairs.Add(new Attendance
                        {
                            EmployeeId = empId,
                            Year = pickeddate.Value.Year,
                            Month = pickeddate.Value.Month,
                            Day = pickeddate.Value.Day,
                            Hour = 23,
                            Minute = 0,
                            Second = 0
                        });
                    }
                }
            }
            //performanceTest.Add("5- " + Utility.GetDateTimeNow().ToLongTimeString());
            var dictionary = new Dictionary<int, TimeSpan>();

            if (attendancePairs.Count > 0)
            {
                int id = 0;
                var timePairGroups = attendancePairs.GroupBy(a => a.EmployeeId);
                foreach (var timePairGroup in timePairGroups)
                {
                    IOrderedEnumerable<Attendance> orderedSample = timePairGroup.OrderBy(a => a.Year).ThenBy(
                        a => a.Month).ThenBy(
                            a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second);
                    int count = 0;

                    DateTime attendanceInTime = new DateTime();
                    DateTime attendanceTime = new DateTime();
                    TimeSpan time = new TimeSpan();
                    foreach (var attendance in orderedSample)
                    {
                        count++;
                        attendanceTime = new DateTime(attendance.Year, attendance.Month,
                                                      attendance.Day, attendance.Hour,
                                                      attendance.Minute, attendance.Second);
                        if (count == 2)
                        {
                            time = time + attendanceTime.Subtract(attendanceInTime);
                            id = attendance.EmployeeId; // move statement before dictinary add
                            count = 0;

                        }
                        else
                        {
                            attendanceInTime = attendanceTime;
                        }
                    }

                    dictionary.Add(id, time);
                }


                foreach (var item in employeeList)
                {
                    foreach (var item1 in dictionary)
                    {
                        if (item1.Key == item.EmployeeId)
                            item.TimeInside = item1.Value;
                    }
                }
                //actual time in office
                //performanceTest.Add("6- " + Utility.GetDateTimeNow().ToLongTimeString());
                //Error flag
                //here we need to add converge logic as well as get the revised list

                var dictionary1 = new Dictionary<int, bool>();
                //Attendance attendancePreviousItem = null;

                var validResult1 = from c in result
                                   where
                                       (c.Day == day && c.Month == month && c.Year == year) &&
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
                        if (fIn.Key == attItems1.First().EmployeeId && (fIn.Value.Location.DeviceNo != 2 || fIn.Value.Location.DeviceNo != 3))
                        {
                            attItems1.Add(fIn.Value);
                        }
                    }

                    var attItems = attItems1.OrderBy(a => a.Year).ThenBy(a => a.Month).ThenBy(
                        a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second);

                    //compress
                    var compressedAttendances = new List<Attendance>();
                    Attendance previousItem = null;
                    foreach (var employeeAttendance in attItems)
                    {
                        employeeAttendance.Location = db.Locations.Find(employeeAttendance.LocationId);
                        if (previousItem != null)
                        {
                            if (employeeAttendance.Location.DeviceNo == previousItem.Location.DeviceNo && employeeAttendance.Hour == previousItem.Hour && employeeAttendance.Minute == previousItem.Minute && Math.Abs(employeeAttendance.Second - previousItem.Second) <= 5)
                            {
                                break;
                            }
                        }
                        compressedAttendances.Add(employeeAttendance);
                        previousItem = employeeAttendance;
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
                                    dictionary1.Add(attendance.EmployeeId, true);
                                    break;
                                }
                            }

                            else if (attendance.InOutMode == "out")
                            {
                                outCount++;
                                inCount = 0;
                                if (outCount > 1)
                                {
                                    dictionary1.Add(attendance.EmployeeId, true);
                                    break;
                                }

                            }
                        }
                    }
                    else
                    {
                        dictionary1.Add(compressedAttendances.First().EmployeeId, true);
                    }
                }

                foreach (var item in employeeList)
                {
                    foreach (var item1 in dictionary1)
                    {
                        if (item1.Key == item.EmployeeId)
                        {
                            item.ErrorFlag = item1.Value;
                        }
                    }
                }
            }
            //Error flag
            //performanceTest.Add("8- " +Utility.GetDateTimeNow().ToLongTimeString());
            //ViewBag.PerformanceTest = performanceTest.ToArray();
            return View(employeeList.OrderBy(a => a.Name));
        }

        public DateTime? ParseDate(string dateStr)
        {
            DateTime dateTime;
            if (DateTime.TryParse(dateStr, CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out dateTime))
                return dateTime;

            return null;
        }

        #region Visitor attendance

        //[DelphiAuthentication]
        public ActionResult VisitorSearchIndex(int? VisitorId, System.DateTime? pickeddate)
        {
            ViewBag.TimeInsideOffice = "Visitor Quick Find";
            //List<string> performanceTest = new List<string>();//test performance
            //performanceTest.Add("0- " + Utility.GetDateTimeNow().ToLongTimeString());
            //user info
            #region Checking Logged In user details
            ViewBag.Username = User.Identity.Name;
            EmployeeEnrollment loggedUser = db.EmployeeEnrollment.FirstOrDefault(a => a.UserName == User.Identity.Name);
            if (null == loggedUser)
            {
                TempData["ErrorMessage"] = "Active directory user not found.";
                return RedirectToAction("LogOn", "Account");
            }
            if (false == loggedUser.IsEnable)
            {
                TempData["ErrorMessage"] = "Active directory user inactive.";
                return RedirectToAction("LogOn", "Account");
            }
            #endregion
            #region Checking the privilages

            ViewBag.Role = Utility.GetUserRoleInt();//loggedUser.Privillage;
            //TODO: Take the data from HR web API once it done
            ViewBag.Name = loggedUser.UserName; //loggedUser.Name;
            ViewBag.isCurrentUserView = false;
            if (VisitorId == null && Utility.IsUserInRole("Employee"))//(VisitorId == null && loggedUser.Privillage == 0)
            {
                VisitorId = loggedUser.EmployeeId;
                ViewBag.isCurrentUserView = true;
            }

            //for ordinary user show only his/her data
            //if (loggedUser.Priv == 0)
            //{
            //EmployeeId = loggedUser.Id;
            //Sending all employee ids to view for location finder - AS-153
            //ViewBag.EmployeeId = new SelectList(db.Employees.Where(a => a.Username == User.Identity.Name).ToList(), "Id", "Name", "Id");
            //}
            //else
            //{
            //    var empList = new SelectList(db.Employees.OrderBy(a => a.Name).ToList(), "Id", "Name", "Id");

            //    ViewBag.EmployeeId = empList;
            //}

            //var empList = new SelectList(db.Employees.OrderBy(a => a.Name).ToList(), "Id", "Name");

            SelectList visList;

            if (pickeddate.HasValue)
            {

                visList = new SelectList(db.VisitorAttendances.Where(a => a.DateTime.Day == pickeddate.Value.Day).Where(a => a.DateTime.Month == pickeddate.Value.Month).Where(a => a.DateTime.Year == pickeddate.Value.Year).Select(g => new { CardNo = g.CardNo }).Distinct().OrderBy(a => a.CardNo).ToList(), "CardNo", "CardNo");

            }
            else
            {
                visList = new SelectList(db.VisitorAttendances.Where(a => a.DateTime.Day == Utility.GetDateTimeNow().Day).Where(a => a.DateTime.Month == Utility.GetDateTimeNow().Month).Where(a => a.DateTime.Year == Utility.GetDateTimeNow().Year).Select(g => new { CardNo = g.CardNo }).Distinct().OrderBy(a => a.CardNo).ToList(), "CardNo", "CardNo");


            }

            ViewBag.SelectedVisitorId = 0;
            ViewBag.VisitorId = visList;

            ViewBag.Message = "QuickFind";
            #endregion
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
                ViewBag.date = pickeddate.Value.ToShortDateString();
            }
            else
            {
                day = pickeddate.Value.Day;
                month = pickeddate.Value.Month;
                year = pickeddate.Value.Year;
                ViewBag.date = pickeddate.Value.ToShortDateString();
            }
            #endregion




            IQueryable<VisitorAttendance> result;
            IQueryable<VisitorAttendance> subResult;
            IQueryable<IGrouping<int, VisitorAttendance>> group;
            //performanceTest.Add("1- " + Utility.GetDateTimeNow().ToLongTimeString());

            #region Getting the Attendance list
            if (VisitorId > 0)
            {
                result = from m in
                             db.VisitorAttendances.Where(a => a.CardNo == VisitorId)
                         //where m.Year==year && m.Month==month && m.Day==day
                         select m;

            }

            else
            {
                result = from m in
                             db.VisitorAttendances
                         //where m.Year == year && m.Month == month && m.Day == day
                         select m;
            }
            #endregion
            #region Getting the Attendance result according to the date picked
            subResult = result.Where(a => a.DateTime.Day == day).Where(a => a.DateTime.Month == month).Where(a => a.DateTime.Year == year);
            #endregion
            //performanceTest.Add("2- " + Utility.GetDateTimeNow().ToLongTimeString());
            #region Grouping the final results according to the enroll no
            group = subResult.GroupBy(a => a.CardNo);
            #endregion
            var visitorList = new List<VisitorStatistics>();
            var firstIndictionary = new Dictionary<int, VisitorAttendance>();
            #region Going throug person by person
            foreach (var groupitem in group)
            {
                var inTime = new DateTime();
                var outTime = new DateTime();
                var visStat = new VisitorStatistics();
                bool firstInFound = false;

                #region Getting results by date sorted
                var groupItems = groupitem.OrderBy(a => a.DateTime);

                #endregion
                #region Getting the location if still in the office
                var lastInItem = groupItems.Last();
                //getting the last known location if not out from basement or main entrance
                Location loc = db.Locations.Find(lastInItem.LocationId);
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

                    item.Location = db.Locations.Find(item.LocationId);

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
                        lastItem.Location = db.Locations.Find(lastItem.LocationId);
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
                #endregion
                visitorList.Add(visStat);
            }
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
                //performanceTest.Add("6- " +Utility.GetDateTimeNow().ToLongTimeString());
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
                //performanceTest.Add("7- " +Utility.GetDateTimeNow().ToLongTimeString());
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
                        visitorAttendance.Location = db.Locations.Find(visitorAttendance.LocationId);
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
            //Error flag
            //performanceTest.Add("8- " + Utility.GetDateTimeNow().ToLongTimeString());
            //ViewBag.PerformanceTest = performanceTest.ToArray();
            return View(visitorList.OrderBy(a => a.VisitorRepresentation));
        }


        //[DelphiAuthentication]
        public ActionResult VisitorIndex(int? VisitorId, System.DateTime? pickeddate)
        {
            //user info

            ViewBag.Username = User.Identity.Name;
            EmployeeEnrollment loggedUser = db.EmployeeEnrollment.FirstOrDefault(a => a.UserName == User.Identity.Name);
            if (null == loggedUser)
            {
                TempData["ErrorMessage"] = "Active directory user not found.";
                return RedirectToAction("LogOn", "Account");
            }
            if (false == loggedUser.IsEnable)
            {
                TempData["ErrorMessage"] = "Active directory user inactive.";
                return RedirectToAction("LogOn", "Account");
            }
            ViewBag.Role = Utility.GetUserRoleInt();//loggedUser.Privillage;

            SelectList visList;


            visList = new SelectList(db.VisitorAttendances.Select(g => new { CardNo = g.CardNo }).Distinct().OrderBy(a => a.CardNo).ToList(), "CardNo", "CardNo");



            ViewBag.VisitorId = visList;


            ViewBag.Message = "AttendanceHistory";


            int day = 0;
            int month = 0;
            int year = 0;

            if (pickeddate == null)
            {
                DateTime date = Utility.GetDateTimeNow();
                day = date.Day;
                month = date.Month;
                year = date.Year;
                ViewBag.date = date.ToShortDateString();
            }
            else
            {
                day = pickeddate.Value.Day;
                month = pickeddate.Value.Month;
                year = pickeddate.Value.Year;
                ViewBag.date = pickeddate.Value.ToShortDateString();
            }

            if (VisitorId > 0)
            {

                var visitorAttendances = db.VisitorAttendances.Include(a => a.Location).Where(x => x.CardNo == VisitorId).Where(x => x.DateTime.Day == day).Where(x => x.DateTime.Month == month).Where(x => x.DateTime.Year == year)
                   .OrderBy(a => a.DateTime);

                var compressedAttendances = new List<VisitorAttendance>();
                VisitorAttendance previousItem = null;
                foreach (var visitorAttendance in visitorAttendances)
                {
                    if (previousItem != null && visitorAttendance.Location.DeviceNo == previousItem.Location.DeviceNo)
                    {
                        if (visitorAttendance.DateTime.Hour == previousItem.DateTime.Hour && visitorAttendance.DateTime.Minute == previousItem.DateTime.Minute && Math.Abs(visitorAttendance.DateTime.Second - previousItem.DateTime.Second) <= 5)
                        {
                            continue;
                        }
                    }

                    compressedAttendances.Add(visitorAttendance);
                    previousItem = visitorAttendance;
                }

                return View(compressedAttendances);
            }


            var allVisitorAttendances = db.VisitorAttendances.Include(a => a.Location).Where(a => a.DateTime.Day == day).Where(x => x.DateTime.Month == month).Where(x => x.DateTime.Year == year).OrderBy(a => a.CardNo).ThenBy(a => a.DateTime.Year).ThenBy(a => a.DateTime.Month).ThenBy(a => a.DateTime.Day).ThenBy(a => a.DateTime.Hour)
              .ThenBy(a => a.DateTime.Minute).ThenBy(a => a.DateTime.Second);

            var compressedAllAttendances = new List<VisitorAttendance>();
            VisitorAttendance previousItem1 = null;
            foreach (var visitorAttendance in allVisitorAttendances)
            {
                if (previousItem1 != null && visitorAttendance.Location.DeviceNo == previousItem1.Location.DeviceNo)
                {
                    if (visitorAttendance.DateTime.Hour == previousItem1.DateTime.Hour && visitorAttendance.DateTime.Minute == previousItem1.DateTime.Minute && Math.Abs(visitorAttendance.DateTime.Second - previousItem1.DateTime.Second) <= 5)
                    {
                        continue;
                    }
                }

                compressedAllAttendances.Add(visitorAttendance);
                previousItem1 = visitorAttendance;
            }

            return View(compressedAllAttendances);

        }

        /// <summary>
        /// Asynchronous method for getting visitor list for drop down
        /// </summary>
        /// <param name="pickeddate"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetSelectorFieldValues(DateTime pickeddate)
        {
            var result = db.VisitorAttendances.Where(a => a.DateTime.Day == pickeddate.Day).Where(a => a.DateTime.Month == pickeddate.Month).Where(a => a.DateTime.Year == pickeddate.Year).Select(g => new { CardNo = g.CardNo }).Distinct().OrderBy(a => a.CardNo).ToList();

            return Json(result);
        }


        #endregion


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}
