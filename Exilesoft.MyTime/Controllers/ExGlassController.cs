using Exilesoft.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Exilesoft.MyTime.Helpers;
using Exilesoft.MyTime.Repositories;

namespace Exilesoft.MyTime.Controllers
{
    public class ExGlassController : BaseController
    {
        private Context dbContext = new Context();

        public ActionResult CurrentStatus()
        {
            DateTime pickeddate = Utility.GetDateTimeNow();
            pickeddate = new DateTime(2013, 7, 10, 12, 45, 30);

            GlassCurrentStatusModel model = new GlassCurrentStatusModel();
            model.PictureAsAt = pickeddate.ToString("dd/MM/yyyy HH:mm:ss");
            model.SelectedDate = pickeddate;
            UpdateAvailability(model);
            return View(model);
        }

        public ActionResult InOutTimeAnalysis()
        {
            return View();
        }

        public ActionResult CurrentAvailability()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetEmployeeTimeTrendGraphData()
        {
            EmployeeEnrollment currentUser = dbContext.EmployeeEnrollment.First(s => s.UserName == "hdi");
            var employee = EmployeeRepository.GetEmployee(currentUser.EmployeeId);
            //var employee = dbContext.Employees.FirstOrDefault(a => a.Username == "hdi");
            DateTime pickeddate = Utility.GetDateTimeNow();
            pickeddate = new DateTime(2013, 7, 10, 12, 45, 30);
            ViewModels.TimeTrendAnalysisViewModel model = new ViewModels.TimeTrendAnalysisViewModel();
            model.FromDate = pickeddate.AddMonths(-1);
            model.ToDate = pickeddate;
            model.ViewInTime = true;
            model.ViewOutTime = true;
            model.SelectedEmployeeID = employee.Id;
            return Json(new { AttendanceStructure = Repositories.TimeTrendRepository.GetEmployeesInOutGraphData(model) });
        }

        [HttpPost]
        public JsonResult GetEmpCurrnetAvailability()
        {
            DateTime pickeddate = Utility.GetDateTimeNow();
            pickeddate = new DateTime(2013, 7, 10, 12, 45, 30);
            DateTime? selectedDate = pickeddate;
            if (selectedDate == null)
                selectedDate = Utility.GetDateTimeNow();

            var allAttendanceForDay = dbContext.Attendances.Where(a => a.Employee.IsEnable == true && a.Year == selectedDate.Value.Year &&
                a.Month == selectedDate.Value.Month && a.Day == selectedDate.Value.Day);

            var result = allAttendanceForDay.ToList().Where(a => new DateTime(a.Year, a.Month, a.Day, a.Hour, a.Minute, 0).TimeOfDay <= selectedDate.Value.TimeOfDay).ToList();

            List<int> deviceList = GetExitLocationMachins();
            List<HourlyTrend> hourlyTrends = null;
            hourlyTrends = new List<HourlyTrend>();
            const int startTime = 6;

            var orderedSubResult = result.OrderBy(
                           a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second);

            int count = orderedSubResult.ToList().Count;
            int numberOfPeopleIn = 0;
            int numberOfPeopleOut = 0;
            int totalEmployeeCount = EmployeeRepository.EmployeeCount();
            int onsiteEmployeeCount = EmployeeRepository.EmployeesOnSite(selectedDate); //dbContext.EmployeesOnSite.Count(a => a.FromDate <= selectedDate && a.ToDate >= selectedDate);
            int absentCount = 0;
            if (count > 0)
            {
                var lastHour = orderedSubResult.ToList().Last().Hour;
                var currentHour = selectedDate.Value.Hour;
                if (currentHour > lastHour)
                    lastHour = currentHour;

                for (int i = startTime; i <= lastHour; i++)
                {
                    int i1 = i;
                    var hourlyResult =
                        result.ToList().Where(
                            a =>
                            new DateTime(a.Year, a.Month, a.Day, a.Hour, a.Minute, 0).TimeOfDay <=
                            new DateTime(a.Year, a.Month, a.Day, i1, 0, 0).TimeOfDay).ToList();
                    var hourlyGroupResult = hourlyResult.GroupBy(x => x.CardNo);

                    numberOfPeopleIn = 0;
                    numberOfPeopleOut = 0;

                    foreach (var hourlyItem in hourlyGroupResult)
                    {
                        var orderedItem = hourlyItem.OrderBy(
                            a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second);
                        var orderedLastItem = orderedItem.Last();
                        if (orderedLastItem.InOutMode == "in")
                            numberOfPeopleIn++;
                        else if (orderedLastItem.InOutMode == "out")
                        {
                            if (!deviceList.Contains(orderedLastItem.LocationId))
                                numberOfPeopleIn++;
                            else
                                numberOfPeopleOut++;
                        }
                    }

                    string _dateTimeStr = string.Format("{0}-{1}-{2} {3}:{4}:{5}", selectedDate.Value.Year, selectedDate.Value.Month,
                    selectedDate.Value.Day, i1.ToString(), "00", "00");
                    hourlyTrends.Add(new HourlyTrend { InCount = numberOfPeopleIn, Time = _dateTimeStr });
                }

                // Generate the hour summery record for the last minute
                // Need to generate the graph up to the selected time.
                #region --- For the last min ---

                if (selectedDate.Value.Minute != 0)
                {
                    var finalResult =
                            result.ToList().Where(
                                a =>
                                new DateTime(a.Year, a.Month, a.Day, a.Hour, a.Minute, 0).TimeOfDay <=
                                new DateTime(a.Year, a.Month, a.Day, selectedDate.Value.Hour, selectedDate.Value.Minute, 0).TimeOfDay).ToList();
                    var finalGroupResult = finalResult.GroupBy(x => x.CardNo);

                    numberOfPeopleIn = 0;
                    numberOfPeopleOut = 0;

                    foreach (var hourlyItem in finalGroupResult)
                    {
                        var orderedItem = hourlyItem.OrderBy(
                            a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second);
                        var orderedLastItem = orderedItem.Last();
                        if (orderedLastItem.InOutMode == "in")
                            numberOfPeopleIn++;
                        else if (orderedLastItem.InOutMode == "out")
                        {
                            if (!deviceList.Contains(orderedLastItem.LocationId))
                                numberOfPeopleIn++;
                            else
                                numberOfPeopleOut++;
                        }
                    }

                    string _finalDateTimeStr = string.Format("{0}-{1}-{2} {3}:{4}:{5}", selectedDate.Value.Year, selectedDate.Value.Month,
                    selectedDate.Value.Day, selectedDate.Value.Hour.ToString(), selectedDate.Value.Minute.ToString(), "00");

                    hourlyTrends.Add(new HourlyTrend { InCount = numberOfPeopleIn, Time = _finalDateTimeStr });
                }

                #endregion
            }

            absentCount = totalEmployeeCount - onsiteEmployeeCount - numberOfPeopleIn - numberOfPeopleOut;

            return Json(new
            {
                GraphData = GetHourlyTrendGraphData(hourlyTrends),
                PeopleIn = numberOfPeopleIn,
                PeopleOut = numberOfPeopleOut,
                absentEmployeeCount = absentCount,
                totalEmployeeCount = totalEmployeeCount,
                onSiteCount = onsiteEmployeeCount
            });
        }


        public void UpdateAvailability(GlassCurrentStatusModel model)
        {
            DateTime? selectedDate = model.SelectedDate;

            var allAttendanceForDay = dbContext.Attendances.Where(a => a.Employee.IsEnable == true && a.Year == selectedDate.Value.Year &&
                a.Month == selectedDate.Value.Month && a.Day == selectedDate.Value.Day);

            var result = allAttendanceForDay.ToList().Where(a => new DateTime(a.Year, a.Month, a.Day, a.Hour, a.Minute, 0).TimeOfDay <= selectedDate.Value.TimeOfDay).ToList();
            List<int> deviceList = GetExitLocationMachins();
            List<HourlyTrend> hourlyTrends = null;
            hourlyTrends = new List<HourlyTrend>();
            const int startTime = 6;

            var orderedSubResult = result.OrderBy(
                           a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second);

            int count = orderedSubResult.ToList().Count;
            int numberOfPeopleIn = 0;
            int numberOfPeopleOut = 0;
            int totalEmployeeCount = EmployeeRepository.EmployeeCount();
            int onsiteEmployeeCount = EmployeeRepository.EmployeesOnSite(selectedDate);
            int absentCount = 0;
            if (count > 0)
            {
                var lastHour = orderedSubResult.ToList().Last().Hour;
                var currentHour = selectedDate.Value.Hour;
                if (currentHour > lastHour)
                    lastHour = currentHour;

                for (int i = startTime; i <= lastHour; i++)
                {
                    int i1 = i;
                    var hourlyResult =
                        result.ToList().Where(
                            a =>
                            new DateTime(a.Year, a.Month, a.Day, a.Hour, a.Minute, 0).TimeOfDay <=
                            new DateTime(a.Year, a.Month, a.Day, i1, 0, 0).TimeOfDay).ToList();
                    var hourlyGroupResult = hourlyResult.GroupBy(x => x.CardNo);

                    numberOfPeopleIn = 0;
                    numberOfPeopleOut = 0;

                    foreach (var hourlyItem in hourlyGroupResult)
                    {
                        var orderedItem = hourlyItem.OrderBy(
                            a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second);
                        var orderedLastItem = orderedItem.Last();
                        if (orderedLastItem.InOutMode == "in")
                            numberOfPeopleIn++;
                        else if (orderedLastItem.InOutMode == "out")
                        {
                            if (!deviceList.Contains(orderedLastItem.LocationId))
                                numberOfPeopleIn++;
                            else
                                numberOfPeopleOut++;
                        }
                    }

                    string _dateTimeStr = string.Format("{0}-{1}-{2} {3}:{4}:{5}", selectedDate.Value.Year, selectedDate.Value.Month,
                    selectedDate.Value.Day, i1.ToString(), "00", "00");
                    hourlyTrends.Add(new HourlyTrend { InCount = numberOfPeopleIn, Time = _dateTimeStr });
                }

                // Generate the hour summery record for the last minute
                // Need to generate the graph up to the selected time.
                #region --- For the last min ---

                if (selectedDate.Value.Minute != 0)
                {
                    var finalResult =
                            result.ToList().Where(
                                a =>
                                new DateTime(a.Year, a.Month, a.Day, a.Hour, a.Minute, 0).TimeOfDay <=
                                new DateTime(a.Year, a.Month, a.Day, selectedDate.Value.Hour, selectedDate.Value.Minute, 0).TimeOfDay).ToList();
                    var finalGroupResult = finalResult.GroupBy(x => x.CardNo);

                    numberOfPeopleIn = 0;
                    numberOfPeopleOut = 0;

                    foreach (var hourlyItem in finalGroupResult)
                    {
                        var orderedItem = hourlyItem.OrderBy(
                            a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second);
                        var orderedLastItem = orderedItem.Last();
                        if (orderedLastItem.InOutMode == "in")
                            numberOfPeopleIn++;
                        else if (orderedLastItem.InOutMode == "out")
                        {
                            if (!deviceList.Contains(orderedLastItem.LocationId))
                                numberOfPeopleIn++;
                            else
                                numberOfPeopleOut++;
                        }
                    }

                    string _finalDateTimeStr = string.Format("{0}-{1}-{2} {3}:{4}:{5}", selectedDate.Value.Year, selectedDate.Value.Month,
                    selectedDate.Value.Day, selectedDate.Value.Hour.ToString(), selectedDate.Value.Minute.ToString(), "00");

                    hourlyTrends.Add(new HourlyTrend { InCount = numberOfPeopleIn, Time = _finalDateTimeStr });
                }

                #endregion
            }

            absentCount = totalEmployeeCount - onsiteEmployeeCount - numberOfPeopleIn - numberOfPeopleOut;
            model.Absent = absentCount;
            model.InSideOffice = numberOfPeopleIn;
            model.OnSite = onsiteEmployeeCount;
            model.OutOfOffice = numberOfPeopleOut;
            model.AtWork = numberOfPeopleIn + numberOfPeopleOut;
        }

        public List<int> GetExitLocationMachins()
        {
            string _exitLocationMachines = ConfigurationManager.AppSettings["ExitLocationMachines"].ToString();
            List<int> _locationIDList = new List<int>();
            foreach (string locationID in _exitLocationMachines.Split(','))
                _locationIDList.Add(int.Parse(locationID));

            return _locationIDList;
        }

        public string GetHourlyTrendGraphData(List<HourlyTrend> hourlyTrend)
        {
            StringBuilder _sb = new StringBuilder();
            _sb.Append(string.Format("{0},{1}\n", "Time", "Count"));
            if (hourlyTrend != null)
            {
                foreach (var trend in hourlyTrend)
                {
                    // Creating the data for the graph to process on the 
                    // required format for the javascript.
                    _sb.Append(string.Format("{0},{1}\n", trend.Time, trend.InCount.ToString()));
                }
            }

            return _sb.ToString();
        }
    }
}
