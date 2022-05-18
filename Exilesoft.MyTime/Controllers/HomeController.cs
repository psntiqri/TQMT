using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Web;
using System.Net.Mail;
using System.Web.Script.Serialization;
using Exilesoft.MyTime.Common;
using Exilesoft.MyTime.Filters;
using Exilesoft.MyTime.Helpers;
using Exilesoft.MyTime.Mappings;
using Exilesoft.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using Exilesoft.MyTime.Repositories;
using Exilesoft.MyTime.ViewModels;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Cookies;
using System.Security.Claims;
using Exilesoft.MyTime.ViewModels.Home;

namespace Exilesoft.MyTime.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private const string EventTextFormat = "<div style='max-width:1000px;'><b> Event: </b>{0}</br><b>From: </b>{1}</br><b>To: </b>{2}</br><b>Detail: </b>{3}</div>";

        private Context dbContext = new Context();
        private static string selectedDateKey = "DASHBOARD_SELECTEDDATE";
        private static string attendanceListKey = "DASHBOARD_ATTENDANCELIST";
        public int EmployeeId = 0;

        ClaimsIdentity userClaims;

        public HomeController()
        {

        }

        public static string CookieName
        {
            get { return ConfigurationManager.AppSettings["LoginCoockieName"]; }

        }

        public void GetEmployeeId()
        {
            try
            {
                userClaims = User.Identity as ClaimsIdentity;
                var claimEmail = userClaims?.FindFirst("preferred_username")?.Value;
                var claimUsername = claimEmail.Split('@')[0];

                var loggedEmployeeId = dbContext.EmployeeEnrollment.Where(x => x.UserName == claimUsername).Select(x => x.EmployeeId).FirstOrDefault();

                EmployeeId = loggedEmployeeId;
                //HttpRuntime.Cache.Insert("EmployeeID", loggedEmployeeId);
                System.Web.HttpContext.Current.Session.Add("EmployeeId", loggedEmployeeId);
                var a = Session["EmployeeId"];
            }
            catch (Exception ex)
            {
                var error = ex.Message;
            }

        }

        [HttpGet]
        [AllowAnonymous]
        public void EndSession()
        {
            // If AAD sends a single sign-out message to the app, end the user's session, but don't redirect to AAD for sign out.
            Request.GetOwinContext().Authentication.SignOut();
            Request.GetOwinContext().Authentication.SignOut(Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ApplicationCookie);
            this.HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            Session.RemoveAll();
            Response.Redirect("/");
        }
        public void SignIn()
        {
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }


        /// <summary>
        /// Send an OpenID Connect sign-out request.
        /// </summary>
        public void SignOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(
            new AuthenticationProperties { RedirectUri = ConfigurationManager.AppSettings["redirectUri"] },
            OpenIdConnectAuthenticationDefaults.AuthenticationType,
            CookieAuthenticationDefaults.AuthenticationType);
        }


        public ActionResult Index()
        {
            SignIn();

            userClaims = User.Identity as ClaimsIdentity;

            if (userClaims.IsAuthenticated)
            {

                ////You get the user's first and last name below:
                //ViewBag.Name = userClaims?.FindFirst("name")?.Value;

                //// The 'preferred_username' claim can be used for showing the username
                //ViewBag.Username = userClaims?.FindFirst("preferred_username")?.Value;

                //// The subject/ NameIdentifier claim can be used to uniquely identify the user across the web
                //ViewBag.Subject = userClaims?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                //// TenantId is the unique Tenant Id - which represents an organization in Azure AD
                //ViewBag.TenantId = userClaims?.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

                //ViewBag.Role = userClaims?.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
                RoleData roleData = new RoleData();

                var roleclaims = userClaims?.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

                for (int i = 0; i < roleclaims.Count(); i++)
                {
                    //var a = roleclaims.ElementAt(i)?.Value;

                    String role = roleclaims.ElementAt(i).Value;
                    roleData.Roles.Add(role);

                }

                ViewBag.Roles = roleData.Roles;

                var testRole = User.IsInRole("Employee");
                GetEmployeeId();

                var t = EmployeeId;
            }
            else
            {
                SignIn();
            }

            //Common Methods


            //return View();


            //if( Request.Url.Equals("https://mytime.tiqri.com/Home/Index?ReturnUrl=%2fhangfire")){
            //    Response.Redirect("/hangfire");
            //}



            //var authCookie = System.Web.HttpContext.Current.Request.Cookies.Get(CookieName);

            //if (authCookie == null || string.IsNullOrEmpty(authCookie.Value))
            //{
            //    return Redirect(Authenticate());
            //}

            //FormsAuthenticationTicket authenticationTicket = FormsAuthentication.Decrypt(authCookie.Value);

            //if (authenticationTicket == null)
            //{
            //    return Redirect(Authenticate());
            //}

            //if (authenticationTicket.Expired)
            //{
            //    return Redirect(Authenticate());
            //}

            //var userData = authenticationTicket.UserData.Split(',');
            //string[] roles = userData[1].Split('|');
            //string[] rule = { "Admin", "Manager", "Employee" };

            bool HavingRights = false;
            if (User.IsInRole("Administrator") || User.IsInRole("Manager") || User.IsInRole("Employee"))
            {
                HavingRights = true;
            }
            if (HavingRights)
            {
                var viewModel = new ViewModels.Home.HomeViewModel();
                new ViewModelDirector().Construct(viewModel);
                return View(viewModel);
            }
            return Content("<html><p><i>Hello! You don't have permission to view this page.</u></i></p></html>", "text/html");
        }

        public ActionResult LogOut()
        {
            CookieHelper.DeleteCookie(CookieHelper.CookieName);
            return Redirect(ConfigurationManager.AppSettings["PortalUrl"]);
        }

        public ActionResult CallBack(string code)
        {
            var data = new StringBuilder();

            data.Append("code=" + code);
            data.Append("&clientId=" + Uri.EscapeDataString(Base64Encode(ConfigurationManager.AppSettings["ClientId"])));
            data.Append("&clientSecret=" + Uri.EscapeDataString(Base64Encode(ConfigurationManager.AppSettings["ClientSecret"])));
            data.Append("&redirectUri=" + "");

            HttpPost(data, ConfigurationManager.AppSettings["TokenkUrl"]);

            return RedirectToAction("Index", "Home");
        }

        //[DelphiAuthentication]
        public JsonResult LoginUser()
        {
            try
            {
                //var user = System.Web.HttpContext.Current.User;

                //var s = Thread.CurrentPrincipal.IsInRole("Admin");

                //var employeeId = int.Parse(HttpRuntime.Cache.Get("EmployeeID").ToString());

                //if(true)//Membership.ValidateUser(userName, password))
                //{

                //HttpContext.GetOwinContext().Authentication.Challenge(
                //    new AuthenticationProperties { RedirectUri = "/" },
                //    OpenIdConnectAuthenticationDefaults.AuthenticationType);


                return Json(new { Success = "True" });


                // }

                //return Json(new { url = "http://localhost:54585/Account/Login" });
                //return Json(new { Success = "False", Message = "Employee cannot found. Please try again." });


            }
            catch (Exception ex)
            {
                //return Json(new { url = "http://localhost:54585/Account/Login", redirect = "true" });
                return Json(new { Success = "False", Message = ex.InnerException.InnerException.Message });
            }

        }

        //[DelphiAuthentication]
        public ActionResult Landing()
        {
            var employeeId = int.Parse(Session["EmployeeId"].ToString());
            var IsSuperVisors = EmployeeRepository.GetAllSupervisorsFromService().Where(a => a.Id == employeeId).FirstOrDefault();
            if (IsSuperVisors != null)
            {
                ViewBag.HavingAttendence = true;
            }

            userClaims = User.Identity as ClaimsIdentity;
            RoleData roleData = new RoleData();

            var roleclaims = userClaims?.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

            for (int i = 0; i < roleclaims.Count(); i++)
            {
                //var a = roleclaims.ElementAt(i)?.Value;

                String role = roleclaims.ElementAt(i).Value;
                roleData.Roles.Add(role);

            }
            ViewBag.LoggedUserName = User.Identity.Name;
            ViewBag.Message = "Right Now Picture";
            return View(roleData);
        }

        //[DelphiAuthentication]
        public ActionResult Dashboard(string pickeddateStr)
        {
            DateTime? pickeddate = Helpers.Utility.ParseDate(pickeddateStr);
            ViewBag.Message = "Right Now Picture";
            if (pickeddate != null)
            {
                ViewBag.date = new DateTime(pickeddate.Value.Year, pickeddate.Value.Month, pickeddate.Value.Day,
                                               pickeddate.Value.Hour, pickeddate.Value.Minute, pickeddate.Value.Second).ToString("dd/MM/yyyy HH:mm");
                ViewBag.date1 = pickeddate.Value.ToString("dd/MM/yyyy HH:mm:ss");
            }
            else
            {
                pickeddate = Utility.GetDateTimeNow();
                ViewBag.date1 = pickeddate.Value.ToString("dd/MM/yyyy HH:mm:ss");
                ViewBag.date = pickeddate.Value.ToString("dd/MM/yyyy HH:mm");
            }

            Session[selectedDateKey] = pickeddate;

            return View();
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

        public ActionResult About()
        {
            ViewBag.Message = "About";
            return View();
        }

        public ActionResult Employees()
        {
            //var query = from e in dbContext.Employees
            //            select e;
            return View(EmployeeRepository.GetAllEmployees());
        }

        /// <summary>
        /// Iniating the hourly attendance data requesting from the server
        /// Sends the formated data for the hourly graph and pie charts.
        /// </summary>
        /// <returns>Formated data for the graphs</returns>
        // [HttpPost]
        //[DelphiAuthentication]
        public JsonResult GetHourlyAttendanceData()
        {
            List<HourlyTrend> hourlyTrends = Session["HOURLY_ATTENDANCE"] as List<HourlyTrend>;
            return Json(GetHourlyTrendGraphData(hourlyTrends));
        }

        /// <summary>
        /// Gets the exit locations from the configuraton file
        /// </summary>
        /// <returns>List of location IDs</returns>
        public List<int> GetExitLocationMachins()
        {
            string _exitLocationMachines = ConfigurationManager.AppSettings["ExitLocationMachines"].ToString();
            List<int> _locationIDList = new List<int>();
            foreach (string locationID in _exitLocationMachines.Split(','))
                _locationIDList.Add(int.Parse(locationID));

            return _locationIDList;
        }

        //public class NoCache : ActionFilterAttribute
        //{
        //    public override void OnResultExecuting(ResultExecutingContext filterContext)
        //    {


        //        System.Diagnostics.Trace.WriteLine("Cach clearing starte");

        //        System.Diagnostics.Trace.WriteLine("Company Coverage :" + DailyAttendanceRepository.getBackgroundMasterProcesData(2019, 9, 8).CompanyCoverage + " Time :" + Utility.GetDateTimeNow().ToString());

        //        filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
        //        filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
        //        filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
        //        filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        //        filterContext.HttpContext.Response.Cache.SetNoStore();

        //        base.OnResultExecuting(filterContext);

        //        System.Diagnostics.Trace.WriteLine("Cach clearing Finished");
        //        System.Diagnostics.Trace.WriteLine("Company Coverage :" + DailyAttendanceRepository.getBackgroundMasterProcesData(2019, 9, 8).CompanyCoverage + " Time :" + Utility.GetDateTimeNow().ToString());
        //    }
        //}


        //[DelphiAuthentication]
        public JsonResult GetEmployeeAvailability()
        {
            string specialEventFullText = string.Empty;
            DateTime selectedDate = (DateTime)Session[selectedDateKey];
            if (selectedDate == null)
                selectedDate = Utility.GetDateTimeNow();



            specialEventFullText = this.DisplaySpecialEvent(specialEventFullText);


            List<Attendance> allAttendanceForDay = dbContext.Attendances.Where(a => a.Year == selectedDate.Year &&
                a.Month == selectedDate.Month && a.Day == selectedDate.Day).ToList<Attendance>();

            var onSiteEmpList = OnSiteRepository.GetOnsiteEmployees(selectedDate).Select(a => a.EmployeeId).ToList();
            allAttendanceForDay = allAttendanceForDay.Where(x => !onSiteEmpList.Contains(x.EmployeeId)).ToList();


            var result = allAttendanceForDay.ToList().Where(a => new DateTime(a.Year, a.Month, a.Day, a.Hour, a.Minute, 0).TimeOfDay <= selectedDate.TimeOfDay).ToList();
            Session[attendanceListKey] = result.ToList();

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
                var currentHour = selectedDate.Hour;
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
                    var hourlyGroupResult = hourlyResult.GroupBy(x => x.EmployeeId);

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

                    string _dateTimeStr = string.Format("{0}-{1}-{2} {3}:{4}:{5}", selectedDate.Year, selectedDate.Month,
                    selectedDate.Day, i1.ToString(), "00", "00");
                    hourlyTrends.Add(new HourlyTrend { InCount = numberOfPeopleIn, Time = _dateTimeStr });
                }

                // Generate the hour summery record for the last minute
                // Need to generate the graph up to the selected time.
                #region --- For the last min ---

                if (selectedDate.Minute != 0)
                {
                    var finalResult =
                            result.ToList().Where(
                                a =>
                                new DateTime(a.Year, a.Month, a.Day, a.Hour, a.Minute, 0).TimeOfDay <=
                                new DateTime(a.Year, a.Month, a.Day, selectedDate.Hour, selectedDate.Minute, 0).TimeOfDay).ToList();


                    var finalGroupResult = finalResult.GroupBy(x => x.EmployeeId);

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

                    string _finalDateTimeStr = string.Format("{0}-{1}-{2} {3}:{4}:{5}", selectedDate.Year, selectedDate.Month,
                    selectedDate.Day, selectedDate.Hour.ToString(), selectedDate.Minute.ToString(), "00");

                    hourlyTrends.Add(new HourlyTrend { InCount = numberOfPeopleIn, Time = _finalDateTimeStr });
                }

                #endregion
            }

            absentCount = totalEmployeeCount - onsiteEmployeeCount - numberOfPeopleIn - numberOfPeopleOut;

            //var loggedEmployeeId = int.Parse(HttpRuntime.Cache.Get("EmployeeID").ToString());


            ViewModels.DailyAttendanceViewModel model = new ViewModels.DailyAttendanceViewModel();



            DateTime fromDate = selectedDate.AddMonths(-1).AddDays(-1);
            model.FromDate = fromDate;

            DateTime toDate = selectedDate.AddDays(-1);
            model.ToDate = toDate;

            var allActiveEmpList = EmployeeRepository.GetAllEnableEmployees();
            var onSiteEmployeeList = OnSiteRepository.GetOnsiteEmployeesByDateRange(fromDate, toDate).Select(a => a.EmployeeId).ToList();

            allActiveEmpList = allActiveEmpList.Where(x => !onSiteEmployeeList.Contains(x.Id)).ToList();

            model.SelectedEmployeeList = allActiveEmpList;



            var backGroundSlave = BacgroungRepository.getBackgroundSlaveProcesDataByEmployeeId(toDate.Year, toDate.Month, toDate.Day, EmployeeId);
            decimal MyCoverage = 0.0M;
            if (backGroundSlave == null)
            {
                EmployeeEnrollment loggedUser = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == EmployeeId);
                ViewModels.TimeTrendAnalysisViewModel _model = new ViewModels.TimeTrendAnalysisViewModel(loggedUser, null, null);
                var Ep = Repositories.TimeTrendRepository.GetEmployeesInOutGraphData(_model);
                MyCoverage = Ep.WorkCoverage;

            }
            else
            {
                MyCoverage = backGroundSlave.Coverage;
            }
            var backGroundMaster = BacgroungRepository.getBackgroundMasterProcesData(toDate.Year, toDate.Month, toDate.Day);

            decimal CompanyCoverage = 0.0M;
            decimal AchievedCoverage = 0.0M;

            if (backGroundMaster == null)
            {
                var cx = Repositories.DailyAttendanceRepository.GetEmployeesHorsGraphData(model);
                if (cx.TotalTeamWorkCoverage != string.Empty)
                {
                    CompanyCoverage = Decimal.Parse(cx.TotalTeamWorkCoverage);
                }
                AchievedCoverage = 67.0M;
            }
            else
            {
                CompanyCoverage = backGroundMaster.CompanyCoverage;
                AchievedCoverage = backGroundMaster.AchievedCoverage;

                // System.Diagnostics.Trace.WriteLine("Company Coverage :" + DailyAttendanceRepository.getBackgroundMasterProcesData(toDate.Year, toDate.Month, toDate.Day).CompanyCoverage + " Time :" + Utility.GetDateTimeNow().ToString());
            }

            return Json(new
            {
                GraphData = GetHourlyTrendGraphData(hourlyTrends),
                PeopleIn = numberOfPeopleIn,
                PeopleOut = numberOfPeopleOut,
                absentEmployeeCount = absentCount,
                totalEmployeeCount = totalEmployeeCount,
                onSiteCount = onsiteEmployeeCount,
                specilEvent = specialEventFullText,
                workCoverage = MyCoverage,
                CompanyCoverage = CompanyCoverage,
                AchievedCoverage = AchievedCoverage
            });
        }

        /// <summary>
        /// Gets the list of employees in/Out/OnSite/Absent/AtWork at a given time
        /// Retuns the list of employees to be displayed on the home screen.
        /// </summary>
        /// <returns>The Json list of arrays for the employees</returns>
        [HttpPost]
        public JsonResult GetEmployeesInandOutOffice()
        {
            IList<Holiday> holidayList = new List<Holiday>();
            IList<Leave> leaveList = new List<Leave>();
            var attendancelist = Session[attendanceListKey] as List<Attendance>;
            DateTime? selectedDate = (DateTime)Session[selectedDateKey];
            if (selectedDate == null)
                selectedDate = Utility.GetDateTimeNow();

            List<int> deviceList = GetExitLocationMachins();
            List<Location> onSiteLocationList = dbContext.Locations.Where(a => a.OnSiteLocation).ToList();



            var onSiteAttList = OnSiteRepository.GetOnsiteEmployees(selectedDate.Value);

            foreach (var item in onSiteAttList)
            {
                attendancelist.RemoveAll(t => t.EmployeeId == item.EmployeeId);
            }



            var employeeAttendanceList = attendancelist.GroupBy(x => x.EmployeeId);
            ViewModels.DashBoardViewModel model = new ViewModels.DashBoardViewModel();
            var employeeList = EmployeeRepository.GetAllEnableEmployees();
            string displayText = string.Empty;
            EmployeeEnrollment loggedUser = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.UserName == User.Identity.Name);

            //AtWorkEmployeeList Creation
            var CrrentloggedUser = System.Web.HttpContext.Current.User;
            var CrrentLoggedEmployeeId = int.Parse(Session["EmployeeId"].ToString());


            foreach (var employeeAttendace in employeeAttendanceList)
            {
                var orderedItem = employeeAttendace.OrderBy(
                            a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second);
                var orderedLastItem = orderedItem.Last();
                var employee = employeeList.FirstOrDefault(a => a.Id == orderedLastItem.EmployeeId);

                displayText = employee.Name;
                // if (loggedUser.Privillage > 0 || employee.Id == loggedUser.EmployeeId)
                if (!(CrrentloggedUser.IsInRole("Employee")))
                    displayText = string.Format("<a href=\"javascript:new HomeLogin().QuickFindWithEmployee({0},'{1}');\" style=\"text-decoration:none;\">{2}</a>",
                    employee.Id, selectedDate.Value.ToString("dd/MM/yyyy"), employee.Name);

                model.AtWorkEmployeeList.Add(new ViewModels.DashBoardEmployeeViewModel()
                {
                    EmployeeID = orderedLastItem.EmployeeId.ToString(),
                    DisplayText = displayText,
                    EmployeeName = employee.Name
                });

                if (orderedLastItem.InOutMode == "in")
                {
                    if (!onSiteLocationList.Any(a => a.Id == orderedLastItem.LocationId))
                    {
                        model.InsideEmployeeList.Add(new ViewModels.DashBoardEmployeeViewModel()
                        {
                            EmployeeID = orderedLastItem.EmployeeId.ToString(),
                            DisplayText = displayText,
                            EmployeeName = employee.Name
                        });
                    }
                }
                else if (orderedLastItem.InOutMode == "out")
                {
                    if (!deviceList.Contains(orderedLastItem.LocationId))
                    {
                        model.InsideEmployeeList.Add(new ViewModels.DashBoardEmployeeViewModel()
                        {
                            EmployeeID = orderedLastItem.EmployeeId.ToString(),
                            DisplayText = displayText,
                            EmployeeName = employee.Name
                        });
                    }
                    else
                        model.OutOfOfficeEmployeeList.Add(new ViewModels.DashBoardEmployeeViewModel()
                        {
                            EmployeeID = orderedLastItem.EmployeeId.ToString(CultureInfo.InvariantCulture),
                            DisplayText = displayText,
                            EmployeeName = employee.Name
                        });
                }
            }

            // var onsiteEmployeeList = dbContext.EmployeesOnSite.Where(a => a.FromDate <= selectedDate && a.ToDate >= selectedDate);
            var onsiteEmployeeList1 = dbContext.EmployeesOnSite.Join(dbContext.EmployeeEnrollment, onsiteEmp => onsiteEmp.EmployeeId, employeeEnrollment => employeeEnrollment.EmployeeId,
               (onsiteEmp, employeeEnrollment) => new { onsiteEmp, employeeEnrollment }).Where(a => a.onsiteEmp.FromDate <= selectedDate && a.onsiteEmp.ToDate >= selectedDate && a.employeeEnrollment.IsEnable == true);

            foreach (var onsiteEmployee in onsiteEmployeeList1)
            {
                var employee = EmployeeRepository.GetEmployee(onsiteEmployee.onsiteEmp.EmployeeId);

                displayText = employee.Name;


                //  if (loggedUser.Privillage > 0 || onsiteEmployee.onsiteEmp.EmployeeEnrollment.EmployeeId == loggedUser.EmployeeId)
                if ((!(CrrentloggedUser.IsInRole("Employee"))) || onsiteEmployee.onsiteEmp.EmployeeEnrollment.EmployeeId == CrrentLoggedEmployeeId)
                    displayText = string.Format("<a href=\"javascript:new HomeLogin().QuickFindWithEmployee({0},'{1}');\" style=\"text-decoration:none;\">{2}</a>",
                    onsiteEmployee.onsiteEmp.EmployeeEnrollment.EmployeeId, selectedDate.Value.ToString("dd/MM/yyyy"), employee.Name);

                model.OnSiteEmployeeList.Add(new ViewModels.DashBoardEmployeeViewModel()
                {
                    EmployeeID = onsiteEmployee.onsiteEmp.EmployeeId.ToString(CultureInfo.InvariantCulture),
                    DisplayText = displayText,
                    EmployeeName = employee.Name
                });
            }

            holidayList = HolidayRepository.GetHolidays(Utility.GetDateTimeNow(), Utility.GetDateTimeNow());
            List<MyLeaveViewModel> employeeLeaves = LeaveRepository.GetMyLeaveService(selectedDate.Value, selectedDate.Value);
            foreach (var employee in employeeList)
            {
                if (!attendancelist.Any(a => a.EmployeeId == employee.Id))
                {
                    if (!onsiteEmployeeList1.Any(a => a.onsiteEmp.EmployeeId == employee.Id))
                    {
                        displayText = employee.Name;
                        //if (loggedUser.Privillage > 0 || employee.Id == loggedUser.EmployeeId)
                        if ((!(CrrentloggedUser.IsInRole("Employee"))) || employee.Id == CrrentLoggedEmployeeId)
                            displayText = string.Format("<a href=\"javascript:new HomeLogin().QuickFindWithEmployee({0},'{1}');\" style=\"text-decoration:none;\">{2}</a>",
                            employee.Id, selectedDate.Value.ToString("dd/MM/yyyy"), employee.Name);


                        leaveList = LeaveRepository.GetLeave(selectedDate.Value, selectedDate.Value, EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(employee.Id), holidayList, employeeLeaves);

                        if (leaveList.Count != 0 && (leaveList[0].Status == "Approved" || leaveList[0].Status == "Pending"))
                        {
                            displayText = "<img src=\"../../Content/images/tick-button.png\" title=\"Approved Leave\"/>&nbsp&nbsp&nbsp" + displayText;
                        }
                        else
                        {
                            displayText = "<img src=\"../../Content/images/exclamation-button.png\" title=\"Unapproved Leave\"/>&nbsp&nbsp&nbsp" + displayText;
                        }
                        model.AbsentEmployeeList.Add(new ViewModels.DashBoardEmployeeViewModel()
                        {
                            EmployeeID = employee.Id.ToString(),
                            DisplayText = displayText,
                            EmployeeName = employee.Name
                        });
                    }
                }
            }

            return Json(model);

        }

        /// <summary>
        /// Updates the selected time for the Home user interface.
        /// Need to populate the home screen as it was at the selected time.
        /// </summary>
        /// <param name="selectedDate">selected date from home screen</param>
        /// <returns>Status of the update.</returns>
        [HttpPost]
        public JsonResult UpdateSelectedTime(string selectedDate)
        {
            ViewBag.Message = "Picture as at";
            DateTime? pickeddate = Helpers.Utility.ParseDate(selectedDate);
            if (pickeddate != null)
            {
                ViewBag.date = new DateTime(pickeddate.Value.Year, pickeddate.Value.Month, pickeddate.Value.Day,
                                               pickeddate.Value.Hour, pickeddate.Value.Minute, pickeddate.Value.Second).ToString("dd/MM/yyyy HH:mm");
                ViewBag.date1 = pickeddate.Value.ToString("dd/MM/yyyy HH:mm:ss");
            }
            else
            {
                pickeddate = Utility.GetDateTimeNow();
                ViewBag.date1 = pickeddate.Value.ToString("dd/MM/yyyy HH:mm:ss");
                ViewBag.date = pickeddate.Value.ToString("dd/MM/yyyy HH:mm");
            }

            Session[selectedDateKey] = pickeddate;

            return Json(new { Status = "Successful" });
        }

        //[DelphiAuthentication("Admin")]
        public ActionResult PastAttendance()
        {
            DateRangeViewModel dateRangeViewModel = new DateRangeViewModel();
            return PartialView(dateRangeViewModel);
        }

        /// <summary>
        /// Gather data to generate past attendence chart for given time period
        /// </summary>
        /// <param name="fromDate">Date Range - from date</param>
        /// <param name="toDate">Date Range - to date</param>
        /// <returns>chart data in json format</returns>
        public JsonResult GetPastAttendance(string fromDate, string toDate)
        {
            DateTime selectedFromDate = Utility.ParseDate(fromDate).GetValueOrDefault();
            DateTime selectedToDate = Utility.ParseDate(toDate).GetValueOrDefault();
            List<string> dates = new List<string>();
            List<int> inOffice = new List<int>();
            List<int> onSite = new List<int>();
            List<int> absent = new List<int>();
            List<int> ActiveEmployees = new List<int>();


            // Active Employees in given time frame
            List<EmployeeData> employees = EmployeeRepository.GetEmployeesByDateRange(selectedFromDate, selectedToDate).ToList();

            // Holiday List 
            List<Holiday> holidayList = HolidayRepository.GetHolidays(selectedFromDate, selectedToDate).ToList();

            // Fill lists
            DateTime recursiveDate = selectedFromDate;

            for (int index = 0; recursiveDate <= selectedToDate; index++)
            {

                if (!HolidayRepository.CheckIsHoliday(holidayList, recursiveDate))
                {
                    //Dates List
                    dates.Add(recursiveDate.Day + "/" + recursiveDate.Month);

                    //In office list   
                    List<Attendance> allAttendanceForDay = dbContext.Attendances.Where(a => a.Year == recursiveDate.Year &&
                    a.Month == recursiveDate.Month && a.Day == recursiveDate.Day).ToList<Attendance>();

                    var onSiteAttList = OnSiteRepository.GetOnsiteEmployees(recursiveDate);

                    foreach (var item in onSiteAttList)
                    {
                        allAttendanceForDay.RemoveAll(t => t.EmployeeId == item.EmployeeId);
                    }

                    int inOfficeEmployeesForTheDay = allAttendanceForDay.GroupBy(e => e.EmployeeId).Count();

                    inOffice.Add(inOfficeEmployeesForTheDay);

                    //On site List                   
                    int onsiteCountForTheDay = onSiteAttList.Count;
                    onSite.Add(onsiteCountForTheDay);

                    //Absent List
                    int activeEmployeesForTheDay = employees.Count(e => e.DateJoined <= recursiveDate && (e.DateResigned >= recursiveDate || e.DateResigned == null));
                    ActiveEmployees.Add(activeEmployeesForTheDay);
                    absent.Add(activeEmployeesForTheDay - inOfficeEmployeesForTheDay - onsiteCountForTheDay);

                }

                recursiveDate = recursiveDate.AddDays(1);
                index++;

            }

            return Json(new
            {
                ActEmp = ActiveEmployees.ToArray(),
                InOffice = inOffice.ToArray(),
                OnSite = onSite.ToArray(),
                Absent = absent.ToArray(),
                Dates = dates.ToArray(),
                InOfficeCount = inOffice.Sum(),
                OnsiteCount = onSite.Sum(),
                AbsentCount = absent.Sum(),
                ActEmpCount = ActiveEmployees.Sum()
            });

        }
        public JsonResult GetEmployeesWorkingFromHomeList(DateTime DateFrom, DateTime DateTo)
        {

            ViewModels.WorkingFromHomeEmloyeeViewModel model = new ViewModels.WorkingFromHomeEmloyeeViewModel();
            try
            {

                DateTime recursiveDate = DateFrom;
                for (int index = 0; recursiveDate <= DateTo; index++)
                {

                    var Atemployees = EmployeeRepository.GetAllEmployees().Where(a => a.IsEnable).OrderBy(a => a.Name);
                    foreach (var item in Atemployees)
                    {
                        PendingAttendance Att = dbContext.PendingAttendances.Where(a => a.EmployeeId == item.Id && a.Year == recursiveDate.Year && a.Month == recursiveDate.Month && a.Day == recursiveDate.Day).FirstOrDefault();
                        if (Att != null)
                        {
                            string stus = string.Empty;
                            switch (Att.ApproveType)
                            {
                                case 0:
                                    stus = "Pending";

                                    break;

                                case 1:
                                    stus = "Approved";

                                    break;
                                case 2:
                                    stus = "Rejected";

                                    break;
                            }

                            string hours = "0";
                            if (Att.OutHour > Att.InHour)
                            {
                                hours = (Att.OutHour - Att.InHour).ToString();
                            }
                            string Min = "00";
                            if (Att.OutMinute > Att.InMinute)
                            {
                                Min = (Att.OutMinute - Att.InMinute).ToString();
                            }

                            model.WokingFromHomeEmployeeList.Add(new ViewModels.WorkingFromHomeEmloyeeViewModel()
                            {
                                EmployeeID = Att.EmployeeId.ToString(),
                                AbsentDate = recursiveDate.ToShortDateString(),
                                EmployeeName = string.Format("<a href=\"javascript:new HomeLogin().QuickFindWithEmployee({0},'{1}');\" style=\"text-decoration:none;\">{2}</a>",
                                Att.EmployeeId, recursiveDate.ToString("dd/MM/yyyy"), item.Name + " - " + hours + ":" + Min + "Hrs" + "  Status : " + stus),
                                Status = stus

                            });
                        }
                    }
                    recursiveDate = recursiveDate.AddDays(1);
                    index++;
                }
                return Json(model);
            }
            catch (Exception ex)
            {
                return Json(new { Success = "false", Message = ex.StackTrace });
            }
        }
        public JsonResult GetEmployeesHalfDayList(DateTime DateFrom, DateTime DateTo)
        {
            try
            {
                IList<Holiday> holidayList = HolidayRepository.GetHolidays(DateFrom, DateTo);
                ViewModels.AbsentEmloyeeReportViewModel model = new ViewModels.AbsentEmloyeeReportViewModel();
                List<MyLeaveViewModel> employeeLeaves = LeaveRepository.GetMyLeaveService(DateFrom, DateTo);
                List<int> _exitLocationList = Repositories.TimeTrendRepository.GetExitLocationMachins();
                DateTime recursiveDate = DateFrom;
                for (int index = 0; recursiveDate <= DateTo; index++)
                {

                    if (!HolidayRepository.CheckIsHoliday(holidayList, recursiveDate))
                    {
                        var Atemployees = EmployeeRepository.GetAllEmployees().Where(a => a.IsEnable).OrderBy(a => a.Name);
                        foreach (var item in Atemployees)
                        {
                            IList<Leave> leaveList = LeaveRepository.GetLeave2(recursiveDate, recursiveDate, item.Id, holidayList, employeeLeaves);
                            if (leaveList.Count == 0)
                            {
                                List<Attendance> AttList = dbContext.Attendances.Where(a => a.EmployeeId == item.Id && a.Year == recursiveDate.Year && a.Month == recursiveDate.Month && a.Day == recursiveDate.Day).ToList();

                                var Groupatt = AttList.GroupBy(b => new DateTime(b.Year, b.Month, b.Day)).ToList();
                                foreach (var Gatt in Groupatt)
                                {

                                    var OrderedGatt = Gatt.OrderBy(a => a.Year).ThenBy(a => a.Month).ThenBy(a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second).ToList();

                                    var FirstIn = OrderedGatt.FirstOrDefault();


                                    var LasstOut = OrderedGatt.LastOrDefault(a => a.InOutMode == "out");
                                    if (LasstOut == null)
                                    {
                                        LasstOut = OrderedGatt.LastOrDefault(a => a.InOutMode == "in");

                                    }
                                    TimeSpan TimeInside = (new DateTime(LasstOut.Year, LasstOut.Month, LasstOut.Day, LasstOut.Hour, LasstOut.Minute, LasstOut.Second) - new DateTime(FirstIn.Year, FirstIn.Month, FirstIn.Day, FirstIn.Hour, FirstIn.Minute, FirstIn.Second));


                                    if (FirstIn != null)
                                    {
                                        var _employeeOutAttenedanceList = OrderedGatt;

                                        for (int i = 0; i < _employeeOutAttenedanceList.ToList().Count; i++)
                                        {
                                            var _outOfficeEntry = _employeeOutAttenedanceList[i];
                                            // Check for the attendance entry is not the first and the last entry for the day.
                                            if (_outOfficeEntry.Id != FirstIn.Id && _outOfficeEntry.Id != LasstOut.Id
                                                && _outOfficeEntry.InOutMode == "out")
                                            {
                                                // Check if the attendece entry is from any exit location.(This exit location  list is mention in the web con fig file i.e 
                                                // <add key="ExitLocationMachines" value="8,9,25,26" /> you can declare the exit point as shown here.)

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
                                                            TimeInside = TimeInside.Subtract(_tsOutOfOffice);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        int hours = TimeInside.Hours;
                                        int Min = TimeInside.Minutes;

                                        if (hours < 7)
                                        {

                                            if (hours < 0) hours = 0;
                                            model.AbsentEmployeeList.Add(new ViewModels.AbsentEmloyeeViewModel()
                                            {
                                                EmployeeID = LasstOut.EmployeeId.ToString(),
                                                AbsentDate = recursiveDate.ToShortDateString(),
                                                EmployeeName = string.Format("<a href=\"javascript:new HomeLogin().QuickFindWithEmployee({0},'{1}');\" style=\"text-decoration:none;\">{2}</a>",
                                                                   LasstOut.EmployeeId, recursiveDate.ToString("dd/MM/yyyy"), item.Name + " - " + hours + ":" + Min + "Hrs")

                                            });

                                        }

                                    }
                                }
                            }

                        }
                    }
                    recursiveDate = recursiveDate.AddDays(1);
                    index++;
                }




                return Json(model);
            }
            catch (Exception ex)
            {
                return Json(new { Success = "false", Message = ex.StackTrace });
            }
        }
        public JsonResult GetEmployeesAbsentList(DateTime DateFrom, DateTime DateTo)
        {
            try
            {
                DateTime selectedFromDate = DateFrom;
                DateTime selectedToDate = DateTo;


                IList<Holiday> holidayList = new List<Holiday>();
                IList<Leave> leaveList = new List<Leave>();
                //holidayList = HolidayRepository.GetHolidays(selectedFromDate, selectedToDate);
                var attendancelist = Session[attendanceListKey] as List<Attendance>;
                //DateTime? selectedDate = (DateTime)Session[selectedDateKey];
                //if (selectedDate == null)
                //    selectedDate = Utility.GetDateTimeNow();

                //List<int> deviceList = GetExitLocationMachins();
                //List<Location> onSiteLocationList = dbContext.Locations.Where(a => a.OnSiteLocation).ToList();
                //var employeeAttendanceList = attendancelist.GroupBy(x => x.CardNo);
                ViewModels.AbsentEmloyeeReportViewModel model = new ViewModels.AbsentEmloyeeReportViewModel();

                //var employeeList = dbContext.Employees.Where(a => a.Enable == true && a.Username == "jdc").ToList();
                var employeeList = EmployeeRepository.GetAllEnableEmployees();
                string displayText = string.Empty;
                //  EmployeeEnrollment loggedUser = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.UserName == User.Identity.Name);

                holidayList = HolidayRepository.GetHolidays(selectedFromDate, selectedToDate);

                // Fill lists
                DateTime recursiveDate = selectedFromDate;
                var onsiteEmployeeList = dbContext.EmployeesOnSite.Where(a => a.FromDate <= recursiveDate && a.ToDate >= recursiveDate && a.IsPermanant);
                //var onsiteEmployeeList1 = dbContext.EmployeesOnSite.Join(dbContext.Employees, e => e.EmployeeId, r => r.Id,
                //       (e, r) => new { e, r }).Where(a => a.e.FromDate <= selectedDate && a.e.ToDate >= selectedDate && a.r.Enable == true);
                List<MyLeaveViewModel> employeeLeaves = LeaveRepository.GetMyLeaveService(selectedFromDate, selectedToDate);
                for (int index = 0; recursiveDate <= selectedToDate; index++)
                {
                    var allAttendanceForDayNotGrouped = dbContext.Attendances.Where(a => a.Employee.IsEnable == true && a.Year == recursiveDate.Year &&
                                               a.Month == recursiveDate.Month && a.Day == recursiveDate.Day).ToList();

                    var allAttendanceForDay = allAttendanceForDayNotGrouped.GroupBy(m => new { m.Year, m.Month, m.Day, m.EmployeeId }).
                                                                            Select(n => new { employeeIds = n.Select(x => new { employeeId = x.EmployeeId }) }).ToList();


                    //attendancelist = result.ToList();

                    if (!HolidayRepository.CheckIsHoliday(holidayList, recursiveDate))
                    {
                        foreach (var employee in employeeList)
                        {
                            if (employee.DateJoined <= recursiveDate)
                            {

                                if (!allAttendanceForDay.Any(x => x.employeeIds.Any(a => a.employeeId == employee.Id)))
                                //if (!attendancelist.Any(a => a.EmployeeId == employee.Id))
                                {
                                    leaveList = LeaveRepository.GetLeave2(recursiveDate, recursiveDate, employee.Id, holidayList, employeeLeaves);
                                    if (leaveList.Count == 0)
                                    {
                                        if (!onsiteEmployeeList.Any(a => a.EmployeeId == employee.Id))
                                        {
                                            displayText = employee.Name;
                                            //if (loggedUser.Privillage > 0 || employee.Id == loggedUser.EmployeeId)
                                            //	displayText =
                                            //		string.Format(
                                            //			"<a href=\"javascript:new HomeLogin().QuickFindWithEmployee({0},'{1}');\" style=\"text-decoration:none;\">{2}</a>",
                                            //			employee.Id, recursiveDate.ToString("dd/MM/yyyy"), employee.Name);

                                            model.AbsentEmployeeList.Add(new ViewModels.AbsentEmloyeeViewModel()
                                            {
                                                EmployeeID = employee.Id.ToString(),
                                                AbsentDate = recursiveDate.ToShortDateString(),
                                                EmployeeName = employee.Name
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                    recursiveDate = recursiveDate.AddDays(1);
                    index++;
                }
                return Json(model);
            }
            catch (Exception ex)
            {
                return Json(new { Success = "false", Message = ex.StackTrace });
            }
        }

        private List<DateGroup> GetAttendenceCountByDate(DateTime fromDate, DateTime toDate)
        {
            //// In Office List
            //List<AttendanceView> attendenceList = (new AttendanceViewModelMapper()).Map(dbContext.Attendances.Where(a => a.Employee.IsEnable && (a.Year >= fromDate.Year && a.Year <= toDate.Year)
            //    && a.InOutMode == "in" ).ToList());

            // In Office List
            List<AttendanceView> attendenceList = (new AttendanceViewModelMapper()).Map(dbContext.Attendances.Where(a => a.Year >= fromDate.Year && a.Year <= toDate.Year).ToList()); //Kishan Corrected 27-11-2017

            //List<AttendanceView> allAttendanceBetweenDates =
            // dbContext.AttendanceView.Where(a => a.Employee.Enable == true && a.Date <= toDate && a.Date >= fromDate && a.InOutMode == "in" && (a.LocationId == 8 || a.LocationId == 9)).ToList();
            List<AttendanceView> allAttendanceBetweenDates = attendenceList.Where(a => a.Date <= toDate && a.Date >= fromDate).ToList();


            List<EmployeeGroup> attendenceByEmployeeAndDate = allAttendanceBetweenDates.GroupBy(g => new { g.Date, g.EmployeeId })
                           .Select(g => new EmployeeGroup
                           {
                               Date = g.Key.Date,
                               EmployeeId = g.Key.EmployeeId
                           }
                        ).ToList();

            //Group by date and employee count by date
            List<DateGroup> attendenceByDate = attendenceByEmployeeAndDate.GroupBy(a => new { a.Date })
                .Select(grouping => new DateGroup
                {
                    Date = grouping.Key.Date,
                    EmployeeCount = grouping.Count()
                }
             ).ToList();

            return attendenceByDate;
        }

        private string DisplaySpecialEvent(string specialEventFullText)
        {


            List<SpecialEvent> specilEventList = dbContext.SpecialEvents.Where(c => c.EventFromDate <= DateTime.Today && DateTime.Today <= c.EventToDate).ToList();

            StringBuilder specialEventBuilder = new StringBuilder();

            foreach (var specialEvent in specilEventList)
            {
                specialEventBuilder.Append(string.Format(EventTextFormat, specialEvent.EventName,
                    specialEvent.EventFromDate.Value.ToString("dd/MM/yyyy"),
                    specialEvent.EventToDate.Value.ToString("dd/MM/yyyy"), specialEvent.Description));
                specialEventBuilder.Append("</br>");
                specialEventBuilder.Append("<div style='border-top: 1px solid black;'></div>");
                specialEventBuilder.Append("</br>");
            }

            return specialEventBuilder.ToString();
        }

        /// <summary>
        /// Send comment to the MyTime Group by the logged in employee
        /// </summary>
        /// <param name="comment">Comment Message</param>
        /// <returns>Message sent status</returns>
        public ActionResult ShowEmployeeAttendence(int EmployeeId)
        {



            var employees = EmployeeRepository.GetAllEmployees().Where(a => a.IsEnable).OrderBy(a => a.Name);

            ViewBag.EmployeeList = new SelectList(employees, "Id", "Name");

            var employeeId = int.Parse(Session["EmployeeId"].ToString());
            var superVisors = EmployeeRepository.GetAllSupervisorsFromService().Where(a => a.Id != employeeId).OrderBy(a => a.Name);
            //var superVisors = EmployeeRepository.GetAllSupervisorsFromService().OrderBy(a => a.Name);

            ViewBag.SupervisorList = new SelectList(superVisors, "Id", "Name", "Id");

            var list = dbContext.PendingAttendances.Where(a => a.EmployeeId == EmployeeId).OrderByDescending(a => a.Id).ToList();

            ViewBag.AtendenceList = list;

            //return View("ShowEmployeeAttendence");

            return View();

        }

        public ActionResult ShowCoverage()
        {
            //DateTime fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            //DateTime toDate = fromDate.AddMonths(1).AddDays(-1);

            //if (toDate >= DateTime.Today)
            //{
            //    toDate = DateTime.Today.AddDays(-1); ;
            //}

            DateTime fromDate = Utility.GetDateTimeNow().AddMonths(-1).AddDays(-1);
            DateTime toDate = Utility.GetDateTimeNow().AddDays(-1);

            IList<BackgroundSlaves> BackgroundSlavesList =
            Repositories.BacgroungRepository.getBackgroundSlaveProcesData(
            toDate.Year, toDate.Month, toDate.Day).OrderByDescending(x => x.Coverage).ToList();

            IList<EmployeeCoverageRowViewModel> empCoverageList = new List<EmployeeCoverageRowViewModel>();
            foreach (var item in BackgroundSlavesList)
            {

                var emp = Repositories.EmployeeRepository.GetEmployee(item.EmployeeId);
                var EmployeeName = emp.Name;
                var ImageUrl = ConfigurationManager.AppSettings["EmployeeWebApi"].Replace("/api/Employee/{0}", "/Content/ProfilePic/") + emp.ImagePath;
                EmployeeCoverageRowViewModel employeeCoverageViewModel = new EmployeeCoverageRowViewModel
                {
                    Year = item.Year,
                    Month = item.Month,
                    Day = item.Day,
                    EmployeeId = item.EmployeeId,
                    EmployeeName = EmployeeName,
                    ImageUrl = ImageUrl,
                    Coverage = item.Coverage,
                    MonthName = ((MonthsOfyear)item.Month).ToString()
                };
                empCoverageList.Add(employeeCoverageViewModel);

            }
            return View("CoverageList", empCoverageList);
        }


        public ActionResult ShowCoverageListMore90Less100()
        {
            //DateTime fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            //DateTime toDate = fromDate.AddMonths(1).AddDays(-1);

            //if (toDate >= DateTime.Today)
            //{
            //    toDate = DateTime.Today.AddDays(-1); ;
            //}

            DateTime fromDate = Utility.GetDateTimeNow().AddMonths(-1).AddDays(-1);
            DateTime toDate = Utility.GetDateTimeNow().AddDays(-1);

            IList<BackgroundSlaves> BackgroundSlavesList =
            Repositories.BacgroungRepository.getBackgroundSlaveProcesData(
            toDate.Year, toDate.Month, toDate.Day).OrderByDescending(x => x.Coverage).ToList();

            EmployeeCoverageViewModel employeeCoverageViewModel = new EmployeeCoverageViewModel();
            IList<EmployeeCoverageRowViewModel> empCoverageList = new List<EmployeeCoverageRowViewModel>();
            foreach (var item in BackgroundSlavesList)
            {
                if (item.Coverage >= 90 && item.Coverage <= 100)
                {
                    var emp = Repositories.EmployeeRepository.GetEmployee(item.EmployeeId);
                    var EmployeeName = emp.Name;
                    var ImageUrl = ConfigurationManager.AppSettings["EmployeeWebApi"].Replace("/api/Employee/{0}", "/Content/ProfilePic/") + emp.ImagePath;
                    EmployeeCoverageRowViewModel employeeCoverageRowViewModel = new EmployeeCoverageRowViewModel
                    {
                        Year = item.Year,
                        Month = item.Month,
                        Day = item.Day,
                        EmployeeId = item.EmployeeId,
                        EmployeeName = EmployeeName,
                        ImageUrl = ImageUrl,
                        Coverage = item.Coverage,
                        MonthName = ((MonthsOfyear)item.Month).ToString()
                    };
                    empCoverageList.Add(employeeCoverageRowViewModel);
                }
            }

            employeeCoverageViewModel.employeeCoverageRowList = empCoverageList;
            employeeCoverageViewModel.TabCount = 2;
            return View("CoverageList", employeeCoverageViewModel);
        }
        public ActionResult ShowCoverageListMoreThan100()
        {
            //DateTime fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            //DateTime toDate = fromDate.AddMonths(1).AddDays(-1);

            //if (toDate >= DateTime.Today)
            //{
            //    toDate = DateTime.Today.AddDays(-1); ;
            //}

            DateTime fromDate = Utility.GetDateTimeNow().AddMonths(-1).AddDays(-1);
            DateTime toDate = Utility.GetDateTimeNow().AddDays(-1);

            IList<BackgroundSlaves> BackgroundSlavesList =
            Repositories.BacgroungRepository.getBackgroundSlaveProcesData(
            toDate.Year, toDate.Month, toDate.Day).OrderByDescending(x => x.Coverage).ToList();

            EmployeeCoverageViewModel employeeCoverageViewModel = new EmployeeCoverageViewModel();
            IList<EmployeeCoverageRowViewModel> empCoverageList = new List<EmployeeCoverageRowViewModel>();
            foreach (var item in BackgroundSlavesList)
            {
                if (item.Coverage > 100)
                {
                    var emp = Repositories.EmployeeRepository.GetEmployee(item.EmployeeId);
                    var EmployeeName = emp.Name;
                    var ImageUrl = ConfigurationManager.AppSettings["EmployeeWebApi"].Replace("/api/Employee/{0}", "/Content/ProfilePic/") + emp.ImagePath;
                    EmployeeCoverageRowViewModel employeeCoveragerowViewModel = new EmployeeCoverageRowViewModel
                    {
                        Year = item.Year,
                        Month = item.Month,
                        Day = item.Day,
                        EmployeeId = item.EmployeeId,
                        EmployeeName = EmployeeName,
                        ImageUrl = ImageUrl,
                        Coverage = item.Coverage,
                        MonthName = ((MonthsOfyear)item.Month).ToString()
                    };
                    empCoverageList.Add(employeeCoveragerowViewModel);
                }
            }

            employeeCoverageViewModel.employeeCoverageRowList = empCoverageList;
            employeeCoverageViewModel.TabCount = 1;
            return View("CoverageList", employeeCoverageViewModel);
        }
        public ActionResult ShowCoverageList90Less()
        {
            //DateTime fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            //DateTime toDate = fromDate.AddMonths(1).AddDays(-1);

            //if (toDate >= DateTime.Today)
            //{
            //    toDate = DateTime.Today.AddDays(-1); ;
            //}

            DateTime fromDate = Utility.GetDateTimeNow().AddMonths(-1).AddDays(-1);
            DateTime toDate = Utility.GetDateTimeNow().AddDays(-1);

            IList<BackgroundSlaves> BackgroundSlavesList =
            Repositories.BacgroungRepository.getBackgroundSlaveProcesData(
            toDate.Year, toDate.Month, toDate.Day).OrderByDescending(x => x.Coverage).ToList();

            EmployeeCoverageViewModel employeeCoverageViewModel = new EmployeeCoverageViewModel();
            IList<EmployeeCoverageRowViewModel> empCoverageList = new List<EmployeeCoverageRowViewModel>();
            foreach (var item in BackgroundSlavesList)
            {
                if (item.Coverage < 90)
                {
                    var emp = Repositories.EmployeeRepository.GetEmployee(item.EmployeeId);
                    var EmployeeName = emp.Name;
                    var ImageUrl = ConfigurationManager.AppSettings["EmployeeWebApi"].Replace("/api/Employee/{0}", "/Content/ProfilePic/") + emp.ImagePath;
                    EmployeeCoverageRowViewModel employeeCoverageRowViewModel = new EmployeeCoverageRowViewModel
                    {
                        Year = item.Year,
                        Month = item.Month,
                        Day = item.Day,
                        EmployeeId = item.EmployeeId,
                        EmployeeName = EmployeeName,
                        ImageUrl = ImageUrl,
                        Coverage = item.Coverage,
                        MonthName = ((MonthsOfyear)item.Month).ToString()
                    };
                    empCoverageList.Add(employeeCoverageRowViewModel);
                }
            }
            employeeCoverageViewModel.employeeCoverageRowList = empCoverageList;
            employeeCoverageViewModel.TabCount = 3;
            return View("CoverageList", employeeCoverageViewModel);
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
        //[DelphiAuthentication]
        public ActionResult WorkingHome(int? pEmployeeId, double? updateRecordId)
        {
            WorkingFromHomeViewModel workingFromHomeViewModel = new WorkingFromHomeViewModel();

            IList<EmployeeData> allemployees = EmployeeRepository.GetAllEmployees();
            IList<EmployeeData> employees = allemployees.Where(a => a.IsEnable).OrderBy(a => a.Name).ToList();




            var employeeId = 0;

            if (pEmployeeId == null)
            {
                employeeId = int.Parse(Session["EmployeeId"].ToString());
            }
            else
            {
                employeeId = int.Parse(pEmployeeId.ToString());
            }

            if (employeeId == null || employeeId == 0)
            {
                return Content("<html><p><i>Hello! You don't have permission to view this page.</u></i></p></html>", "text/html");
            }

            workingFromHomeViewModel.EmployeeList = new SelectList(employees, "Id", "Name", employeeId);

            ViewBag.dateTime = Utility.GetDateTimeNow().ToString("dd/MM/yyyy HH:mm");

            IList<EmployeeData> supervisors = new List<EmployeeData>();

            var supervisorsInclusive = ConfigurationManager.AppSettings["SuperVisorInclusion"];

            if (supervisorsInclusive == "1")
            {
                supervisors = EmployeeRepository.GetAllSupervisorsFromService().OrderBy(a => a.Name).ToList();
            }
            else
            {
                supervisors = EmployeeRepository.GetAllSupervisorsFromService().Where(a => a.Id != employeeId).OrderBy(a => a.Name).ToList();

            }

            workingFromHomeViewModel.SupervisorList = new SelectList(supervisors, "Id", "Name", "Id");

            IList<PendingAttendance> AtendenceList = dbContext.PendingAttendances.Where(a => a.EmployeeId == employeeId).OrderByDescending(a => a.Id).ToList();


            IList<WorkingFromHomeRowViewModel> rowList = new List<WorkingFromHomeRowViewModel>();
            foreach (var item in AtendenceList)
            {
                WorkingFromHomeRowViewModel row = new WorkingFromHomeRowViewModel();
                EmployeeData supervisor = allemployees.Where(a => a.Id == item.ApproverId).FirstOrDefault<EmployeeData>();



                row.ApproverName = supervisor.Name;
                row.ApproverImgUrl = ConfigurationManager.AppSettings["EmployeeWebApi"].Replace("/api/Employee/{0}", "/Content/ProfilePic/") + supervisor.ImagePath;
                row.ApproverId = item.ApproverId;
                row.ApproveType = item.ApproveType;
                row.CardNo = item.CardNo;
                row.InHour = item.InHour;
                row.Day = item.Day;
                row.Month = item.Month;
                row.Year = item.Year;
                row.EmployeeId = item.EmployeeId;
                row.EmployeeName = item.EmployeeName;
                row.Id = item.Id;
                row.InMinute = item.InMinute;
                row.InSecond = item.InSecond;
                row.OutHour = item.OutHour;
                row.OutMinute = item.OutMinute;
                row.OutSecond = item.OutSecond;
                row.Description = item.Description;
                row.TaskList = item.TaskList;

                rowList.Add(row);
            }
            workingFromHomeViewModel.WorkingFromHomeRowList = rowList;

            if (updateRecordId != null && updateRecordId != 0)
            {
                workingFromHomeViewModel.IsUpdate = true;
                workingFromHomeViewModel.UpdateRecordId = updateRecordId.Value;
                //workingFromHomeViewModel.AttendanceRecord = new UpdateAttendanceViewModel();
                var record = dbContext.PendingAttendances.Where(a => a.Id == updateRecordId).FirstOrDefault();

                workingFromHomeViewModel.TaskList = dbContext.WorkingFromHomeTasks.Where(a => a.PendingAttendanceId == updateRecordId).ToList();
                workingFromHomeViewModel.Description = record.Description;
                workingFromHomeViewModel.SupervisorId = record.ApproverId;


                foreach (var item in workingFromHomeViewModel.SupervisorList)
                {
                    if (workingFromHomeViewModel.SupervisorId.ToString() == item.Value)
                    {
                        item.Selected = true;
                        break;
                    }
                }

                DateTime inTime = new DateTime(record.Year,
                    record.Month,
                    record.Day,
                    record.InHour,
                    record.InMinute,
                    record.InSecond);
                workingFromHomeViewModel.AttendanceDateIn = inTime.ToString("yyyy-MM-dd HH:mm");

                DateTime outTime = new DateTime(record.Year,
                    record.Month,
                    record.Day,
                    record.OutHour,
                    record.OutMinute,
                    record.OutSecond);
                workingFromHomeViewModel.AttendanceDateOut = outTime.ToString("yyyy-MM-dd HH:mm");
            }

            return View(workingFromHomeViewModel);
        }

        //[DelphiAuthentication]
        public void ForwardEntry(string[] idList, string supervisorId)
        {
            foreach (var key in idList)
            {
                var approveKey = Guid.Parse(key);
                PendingAttendance pendingAttendence = dbContext.PendingAttendances.Where(a => a.ApproveKey == approveKey).FirstOrDefault();
                pendingAttendence.ApproverId = int.Parse(supervisorId);
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        dbContext.SaveChanges();
                        transaction.Commit();
                        var attendanceDateIn = new DateTime(pendingAttendence.Year, pendingAttendence.Month, pendingAttendence.InHour).AddMinutes(pendingAttendence.InMinute).AddSeconds(pendingAttendence.InSecond);
                        var attendanceDateOut = new DateTime(pendingAttendence.Year, pendingAttendence.Month, pendingAttendence.OutHour).AddMinutes(pendingAttendence.OutMinute).AddSeconds(pendingAttendence.OutSecond);


                        var Val = SendWorkingFromHomemail(attendanceDateIn, attendanceDateOut, pendingAttendence.EmployeeId, pendingAttendence.ApproverId, pendingAttendence.ApproveKey, pendingAttendence.Description, null);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();


                    }
                }
            }

        }
        //[DelphiAuthentication]
        public ActionResult PendingAttendence(string pEmployeeId)
        {

            var employeeId = 0;

            if (pEmployeeId == null)
            {
                employeeId = int.Parse(Session["EmployeeId"].ToString());
            }
            else
            {
                employeeId = int.Parse(pEmployeeId.ToString());
            }

            WorkingFromHomeViewModel workingFromHomeViewModel = new WorkingFromHomeViewModel();

            IList<EmployeeData> allemployees = EmployeeRepository.GetAllEmployees();
            IList<EmployeeData> employees = allemployees.Where(a => a.IsEnable).OrderBy(a => a.Name).ToList();
            workingFromHomeViewModel.EmployeeList = new SelectList(employees, "Id", "Name", employeeId);



            IList<EmployeeData> supervisors = new List<EmployeeData>();

            var supervisorsInclusive = ConfigurationManager.AppSettings["SuperVisorInclusion"];

            if (supervisorsInclusive == "1")
            {
                supervisors = EmployeeRepository.GetAllSupervisorsFromService().OrderBy(a => a.Name).ToList();
            }
            else
            {
                supervisors = EmployeeRepository.GetAllSupervisorsFromService().Where(a => a.Id != employeeId).OrderBy(a => a.Name).ToList();

            }

            workingFromHomeViewModel.SupervisorList = new SelectList(supervisors, "Id", "Name", "Id");

            workingFromHomeViewModel.WorkingAttendenceList = dbContext.PendingAttendances.Where(a => a.ApproverId == employeeId && a.ApproveType == 0)
            .OrderByDescending(x => x.Year * 12 * 30 + x.Month * 30 + x.Day).ToList();



            return View("PendingAttendence", workingFromHomeViewModel);

        }

        //[DelphiAuthentication]
        public ActionResult ConfirmAttendence(int? pEmployeeId)
        {

            var employeeId = 0;

            if (pEmployeeId == null)
            {
                employeeId = int.Parse(Session["EmployeeId"].ToString());
            }
            else
            {
                employeeId = int.Parse(pEmployeeId.ToString());
            }

            WorkingFromHomeViewModel workingFromHomeViewModel = new WorkingFromHomeViewModel();

            IList<EmployeeData> allemployees = EmployeeRepository.GetAllEmployees();
            IList<EmployeeData> employees = allemployees.Where(a => a.IsEnable).OrderBy(a => a.Name).ToList();
            workingFromHomeViewModel.EmployeeList = new SelectList(employees, "Id", "Name", employeeId);


            workingFromHomeViewModel.WorkingAttendenceList = dbContext.PendingAttendances.Where(a => a.ApproverId == employeeId && a.ApproveType == 1)
            .OrderByDescending(x => x.Year * 12 * 30 + x.Month * 30 + x.Day).ToList();

            return View("ConfirmAttendence", workingFromHomeViewModel);

        }
        //[DelphiAuthentication]
        public ActionResult RejectedAttendence(int? pEmployeeId)
        {
            var employeeId = 0;

            if (pEmployeeId == null)
            {
                employeeId = int.Parse(Session["EmployeeId"].ToString());
            }
            else
            {
                employeeId = int.Parse(pEmployeeId.ToString());
            }

            WorkingFromHomeViewModel workingFromHomeViewModel = new WorkingFromHomeViewModel();

            IList<EmployeeData> allemployees = EmployeeRepository.GetAllEmployees();
            IList<EmployeeData> employees = allemployees.Where(a => a.IsEnable).OrderBy(a => a.Name).ToList();
            workingFromHomeViewModel.EmployeeList = new SelectList(employees, "Id", "Name", employeeId);


            workingFromHomeViewModel.WorkingAttendenceList = dbContext.PendingAttendances.Where(a => a.ApproverId == employeeId && a.ApproveType == 2)
           .OrderByDescending(x => x.Year * 12 * 30 + x.Month * 30 + x.Day).ToList();

            return View("RejectedAttendence", workingFromHomeViewModel);
        }
        //[DelphiAuthentication]


        //[DelphiAuthentication]
        public JsonResult SaveWorkingHome(int SupervisorId, DateTime attendanceDateIn, DateTime attendanceDateOut, int EmployeeId, string Description, IList<WorkingFromHomeTask> WorkingFromHomeTaskList, double UpdateRecordId)
        {
            string Val = string.Empty;

            if (attendanceDateIn != null && attendanceDateOut != null)
            {

                if ((attendanceDateIn == null) || (attendanceDateOut == null) || (attendanceDateOut.Year != attendanceDateIn.Year) || (attendanceDateOut.Month != attendanceDateIn.Month)
                    || (attendanceDateOut.Day != attendanceDateIn.Day) || (attendanceDateOut.Hour < attendanceDateIn.Hour))
                {
                    return Json(new { status = "Incorrect reported time.....\n" + "From " + attendanceDateIn + " to " + attendanceDateOut });
                }
                var isValid = ValidateTaskTimeAgainstInOutTime(attendanceDateIn, attendanceDateOut, WorkingFromHomeTaskList);
                if (!isValid)
                {
                    return Json(new { status = "Total number of hours reported under tasks should be less than or equal to time clocked with start and end time\n" });
                }

                //isValid = ValidateExistentry(attendanceDateIn, attendanceDateOut, EmployeeId);
                //if (isValid)
                //{
                //    return Json(new { status = "Existing Time entry can not be overridden\n" });
                //}


                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    if (UpdateRecordId > 0)
                    {
                        var currentTimeRecord = dbContext.PendingAttendances.FirstOrDefault(a => a.Id == UpdateRecordId);
                        var currentTasks = dbContext.WorkingFromHomeTasks.Where(t => t.PendingAttendanceId == UpdateRecordId);
                        dbContext.WorkingFromHomeTasks.RemoveRange(currentTasks);
                        dbContext.PendingAttendances.Remove(currentTimeRecord);
                        dbContext.SaveChanges();
                    }

                    EmployeeEnrollment Employee = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == EmployeeId);
                    var attendace = new PendingAttendance();
                    Guid GUIDobj = Guid.NewGuid();
                    attendace.EmployeeId = EmployeeId;
                    attendace.EmployeeName = EmployeeRepository.GetEmployee(EmployeeId).Name;
                    attendace.CardNo = Employee.CardNo;
                    attendace.Year = attendanceDateIn.Year;
                    attendace.Month = attendanceDateIn.Month;
                    attendace.Day = attendanceDateIn.Day;
                    attendace.InHour = attendanceDateIn.Hour;
                    attendace.InMinute = attendanceDateIn.Minute;
                    attendace.InSecond = attendanceDateIn.Second;
                    attendace.OutHour = attendanceDateOut.Hour;
                    attendace.OutMinute = attendanceDateOut.Minute;
                    attendace.OutSecond = attendanceDateOut.Second;
                    attendace.ApproverId = SupervisorId;
                    attendace.ApproveType = 0;
                    attendace.ApproveKey = GUIDobj;
                    attendace.Description = Description;
                    attendace.TaskList = CaptureAllTasks(WorkingFromHomeTaskList);
                    dbContext.PendingAttendances.Add(attendace);

                    try
                    {
                        dbContext.SaveChanges();
                        foreach (var task in WorkingFromHomeTaskList)
                        {
                            if (!string.IsNullOrEmpty(task.Description))
                            {
                                var workingFromHomeTasks = new WorkingFromHomeTask();
                                workingFromHomeTasks.EmployeeId = EmployeeId;
                                workingFromHomeTasks.EmployeeName = EmployeeRepository.GetEmployee(EmployeeId).Name;
                                workingFromHomeTasks.Date = attendanceDateIn;
                                workingFromHomeTasks.Description = task.Description;
                                workingFromHomeTasks.Hours = task.Hours;
                                workingFromHomeTasks.Minutes = task.Minutes;
                                workingFromHomeTasks.PendingAttendanceId = attendace.Id;

                                dbContext.WorkingFromHomeTasks.Add(workingFromHomeTasks);
                                dbContext.SaveChanges();
                            }
                        }
                        transaction.Commit();
                        //Val = SendWorkingFromHomemail(attendanceDateIn, attendanceDateOut, EmployeeId, SupervisorId, GUIDobj, Description, CaptureAllTasks(WorkingFromHomeTaskList));
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return Json(new { status = "Error occured....." });
                    }
                }
            }
            else
            {
                return Json(new { status = "Start time and End time is Required... :)" });
            }
            return Json(new { status = Val });
        }

        //public JsonResult SaveTemplate(int SupervisorId, DateTime attendanceDateIn, DateTime attendanceDateOut, int EmployeeId, string name)
        //{

        //    try
        //    {
        //        var existingRecord = dbContext.PendingAttendances.Where(x => x.ApproverId == SupervisorId && x.EmployeeId == EmployeeId && (x.Year == attendanceDateIn.Year && x.Month == attendanceDateIn.Month && x.Day == attendanceDateIn.Day));
        //        if (existingRecord.Any())
        //        {
        //            WorkingFromHomeTaskTemplate workingFromHomeTaskTemplate = new WorkingFromHomeTaskTemplate();
        //            workingFromHomeTaskTemplate.EmployeeId = EmployeeId;
        //            workingFromHomeTaskTemplate.Name = name;
        //            workingFromHomeTaskTemplate.PendingAttendanceId = Convert.ToInt32(existingRecord.SingleOrDefault());
        //            dbContext.WorkingFromHomeTaskTemplates.Add(workingFromHomeTaskTemplate);
        //            dbContext.SaveChanges();
        //        }
        //        else
        //        {
        //            return Json(new { status = "No such record exist....." });
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(new { Success = "False", Message = ex.InnerException.InnerException.Message });
        //    }
        //    return Json(new { status = "Operation Success....." });
        //}

        public JsonResult SaveTemplate(int SupervisorId, DateTime attendanceDateIn, DateTime attendanceDateOut, int EmployeeId, string Description, IList<WorkingFromHomeTask> WorkingFromHomeTaskList, string TemplateName, double UpdateRecordId)
        {
            //if (attendanceDateIn != null && attendanceDateOut != null)
            //{


                //if ((attendanceDateIn == null) || (attendanceDateOut == null) || (attendanceDateOut.Year != attendanceDateIn.Year) || (attendanceDateOut.Month != attendanceDateIn.Month)
                //    || (attendanceDateOut.Day != attendanceDateIn.Day) || (attendanceDateOut.Hour < attendanceDateIn.Hour))
                //{
                //    return Json(new { status = "Incorrect reported time.....\n" + "From " + attendanceDateIn + " to " + attendanceDateOut });
                //}
                //var isValid = ValidateTaskTimeAgainstInOutTime(attendanceDateIn, attendanceDateOut, WorkingFromHomeTaskList);
                //if (!isValid)
                //{
                //    return Json(new { status = "Total number of hours reported under tasks should be less than or equal to time clocked with start and end time\n" });
                //}

                if (UpdateRecordId > 0)
                {
                    var updateTemplate = dbContext.WorkingFromHomeTaskTemplates.FirstOrDefault(x => x.Id == UpdateRecordId);
                    updateTemplate.EmployeeId = EmployeeId;
                    updateTemplate.SupervisorId = SupervisorId;
                    //updateTemplate.StartTime = attendanceDateIn.ToString();
                    //updateTemplate.EndTime = attendanceDateOut.ToString();
                    updateTemplate.Description = Description;
                    updateTemplate.Name = TemplateName;
                    updateTemplate.TaskList = string.Join("%@@@@%", WorkingFromHomeTaskList.Select(x => x.Description));
                    updateTemplate.IsEnable = true;
                    dbContext.SaveChanges();
                    return Json(new { status = "Operation Success....." });
                }
                else
                {
                    try
                    {
                        var template = new WorkingFromHomeTaskTemplate();
                        template.EmployeeId = EmployeeId;
                        template.SupervisorId = SupervisorId;
                        //template.StartTime = attendanceDateIn.ToString();
                        //template.EndTime = attendanceDateOut.ToString();
                        template.Description = Description;
                        template.Name = TemplateName;
                        template.TaskList = string.Join("%@@@@%", WorkingFromHomeTaskList.Select(x => x.Description));
                        template.IsEnable = true;
                        dbContext.WorkingFromHomeTaskTemplates.Add(template);
                        dbContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        return Json(new { Success = "False", Message = ex.InnerException.InnerException.Message });
                    }
                }

            //}
            //else
            //{
            //    return Json(new { status = "Start time and End time is Required... :)" });
            //}
            return Json(new { status = "Operation Success....." });
        }

        public JsonResult RemoveTemplate(int TemplateId)
        {
            var template = dbContext.WorkingFromHomeTaskTemplates.FirstOrDefault(x => x.Id == TemplateId && x.IsEnable == true);
            template.IsEnable = false;
            dbContext.SaveChanges();
            return Json(new { status = "Operation Success....." });
        }

        public JsonResult LoadTemplateDetails(int EmployeeId)
        {
            var ListOfTemplates = new List<TemplateDetailsViewModel>();
            var templates = dbContext.WorkingFromHomeTaskTemplates.Where(x => x.EmployeeId == EmployeeId && x.IsEnable == true);
            if (templates.Any())
            {
                foreach (var template in templates)
                {
                    ListOfTemplates.Add(new TemplateDetailsViewModel()
                    {
                        TemplateName = template.Name,
                        SupervisorName = EmployeeRepository.GetEmployee(template.SupervisorId).Name,
                        //StartTime = template.StartTime,
                        //EndTime = template.EndTime,
                        Description = template.Description,
                        Id = template.Id,
                        TaskList = template.TaskList.Split(new string[] { "%@@@@%" }, StringSplitOptions.None).ToList()
                    });

                }
            }
            else
            {
                return Json(new { status = "You do not have any saved templates. please add them !" });
            }

            return Json(ListOfTemplates, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadTemplate(int TemplateId)
        {
            TemplateDetailsViewModel temp  = new TemplateDetailsViewModel();
            var template = dbContext.WorkingFromHomeTaskTemplates.FirstOrDefault(x => x.Id == TemplateId && x.IsEnable == true);
            //temp.TemplateName = template.Name;
            //temp.SupervisorName = EmployeeRepository.GetEmployee(template.SupervisorId).Name;
            temp.EmployeeId = template.EmployeeId;
            temp.SupervisorId = template.SupervisorId;
            //temp.StartTime = template.StartTime;
            //temp.EndTime = template.EndTime;
            temp.Description = template.Description;
            temp.Id = template.Id;
            temp.TemplateName = template.Name;
            temp.TaskList = template.TaskList.Split(new string[] { "%@@@@%" }, StringSplitOptions.None).ToList();
            return Json(temp, JsonRequestBehavior.AllowGet);
        }

        private bool ValidateExistentry(DateTime attendanceDateIn, DateTime attendanceDateOut, int EmployeeId)
        {


            var existingTimeRecord = dbContext.PendingAttendances.Where(a => a.EmployeeId == EmployeeId
            && a.ApproveType != 2
            && a.Year == attendanceDateIn.Year
            && a.Month == attendanceDateIn.Month
            && a.Day == attendanceDateIn.Day
            && (
                 (a.InHour * 60 + a.InMinute > attendanceDateIn.Hour * 60 + attendanceDateIn.Minute && a.InHour * 60 + a.InMinute < attendanceDateOut.Hour * 60 + attendanceDateOut.Minute)
            ||
                 (a.InHour * 60 + a.InMinute <= attendanceDateIn.Hour * 60 + attendanceDateIn.Minute && a.OutHour * 60 + a.OutMinute > attendanceDateIn.Hour * 60 + attendanceDateIn.Minute)
            ||
                 (a.OutHour * 60 + a.OutMinute > attendanceDateIn.Hour * 60 + attendanceDateIn.Minute && a.InHour * 60 + a.InMinute <= attendanceDateIn.Hour * 60 + attendanceDateIn.Minute)
                )
            ).ToList();

            return existingTimeRecord.Any();

        }

        //[DelphiAuthentication]
        public JsonResult DeleteWorkingHome(int EmployeeId, double RecordId)
        {
            string Val = string.Empty;
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var timeRecord = dbContext.PendingAttendances.FirstOrDefault(a => a.Id == RecordId);
                    var tasks = dbContext.WorkingFromHomeTasks.Where(t => t.PendingAttendanceId == RecordId);
                    dbContext.WorkingFromHomeTasks.RemoveRange(tasks);
                    dbContext.PendingAttendances.Remove(timeRecord);
                    dbContext.SaveChanges();
                    Val = "Success";

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { status = "Error occured....." });
                }
            }
            return Json(new { status = Val });
        }

        public JsonResult SaveConfirmAttendenceBulk(string[] idList)
        {
            string Val = string.Empty;
            foreach (var key in idList)
            {

                var approveKey = Guid.Parse(key);
                PendingAttendance pendingAttendence = dbContext.PendingAttendances.Where(a => a.ApproveKey == approveKey).FirstOrDefault();
                EmployeeEnrollment loogedEmployee = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == pendingAttendence.EmployeeId);
                Location location = dbContext.Locations.Find(27);

                if (pendingAttendence != null)
                {
                    using (var transaction = dbContext.Database.BeginTransaction())
                    {

                        var attendace = new Attendance();
                        attendace.Year = pendingAttendence.Year;
                        attendace.Month = pendingAttendence.Month;
                        attendace.Day = pendingAttendence.Day;
                        attendace.Hour = pendingAttendence.InHour;
                        attendace.Minute = pendingAttendence.InMinute;
                        attendace.Second = pendingAttendence.InSecond;

                        attendace.Employee = loogedEmployee;
                        attendace.EmployeeId = loogedEmployee.EmployeeId;
                        attendace.CardNo = loogedEmployee.CardNo;

                        attendace.Location = location;
                        attendace.LocationId = location.Id;
                        attendace.InOutMode = "in";
                        dbContext.Attendances.Add(attendace);

                        attendace = new Attendance();

                        attendace.Year = pendingAttendence.Year;
                        attendace.Month = pendingAttendence.Month;
                        attendace.Day = pendingAttendence.Day;
                        attendace.Hour = pendingAttendence.OutHour;
                        attendace.Minute = pendingAttendence.OutMinute;
                        attendace.Second = pendingAttendence.OutSecond;
                        attendace.Employee = loogedEmployee;
                        attendace.EmployeeId = loogedEmployee.EmployeeId;
                        attendace.CardNo = loogedEmployee.CardNo;
                        attendace.Location = location;
                        attendace.LocationId = location.Id;
                        attendace.InOutMode = "out";
                        dbContext.Attendances.Add(attendace);

                        pendingAttendence.ApproveType = 1;


                        try
                        {
                            dbContext.SaveChanges();
                            transaction.Commit();
                            Val = SendConfirmWorkingFromHomemail(new DateTime(pendingAttendence.Year, pendingAttendence.Month, pendingAttendence.Day), pendingAttendence.EmployeeId, pendingAttendence.Description);

                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return Json(new { status = "Error occured....." });

                        }
                    }
                }
            }
            return Json(new { status = Val });

        }

        public JsonResult SaveConfirmAttendence(int? id)
        {
            string Val = string.Empty;
            PendingAttendance pendingAttendence = dbContext.PendingAttendances.Where(a => a.Id == id).FirstOrDefault();
            EmployeeEnrollment loogedEmployee = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == pendingAttendence.EmployeeId);
            Location location = dbContext.Locations.Find(27);


            if (pendingAttendence != null)
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    var attendace = new Attendance();
                    attendace.Year = pendingAttendence.Year;
                    attendace.Month = pendingAttendence.Month;
                    attendace.Day = pendingAttendence.Day;
                    attendace.Hour = pendingAttendence.InHour;
                    attendace.Minute = pendingAttendence.InMinute;
                    attendace.Second = pendingAttendence.InSecond;

                    attendace.Employee = loogedEmployee;
                    attendace.EmployeeId = loogedEmployee.EmployeeId;
                    attendace.CardNo = loogedEmployee.CardNo;

                    attendace.Location = location;
                    attendace.LocationId = location.Id;
                    attendace.InOutMode = "in";
                    dbContext.Attendances.Add(attendace);

                    attendace = new Attendance();

                    attendace.Year = pendingAttendence.Year;
                    attendace.Month = pendingAttendence.Month;
                    attendace.Day = pendingAttendence.Day;
                    attendace.Hour = pendingAttendence.OutHour;
                    attendace.Minute = pendingAttendence.OutMinute;
                    attendace.Second = pendingAttendence.OutSecond;
                    attendace.Employee = loogedEmployee;
                    attendace.EmployeeId = loogedEmployee.EmployeeId;
                    attendace.CardNo = loogedEmployee.CardNo;
                    attendace.Location = location;
                    attendace.LocationId = location.Id;
                    attendace.InOutMode = "out";
                    dbContext.Attendances.Add(attendace);

                    pendingAttendence.ApproveType = 1;


                    try
                    {

                        dbContext.SaveChanges();

                        transaction.Commit();

                        Val = SendConfirmWorkingFromHomemail(new DateTime(pendingAttendence.Year, pendingAttendence.Month, pendingAttendence.Day), pendingAttendence.EmployeeId, pendingAttendence.Description);

                    }
                    catch (Exception ex)
                    {

                        transaction.Rollback();
                        return Json(new { status = "Error occured....." });

                    }
                }

            }
            return Json(new { status = Val });

        }


        public JsonResult doRejectAttendenceBulk(string[] idList)
        {
            string Val = string.Empty;

            foreach (var key in idList)
            {


                var approveKey = Guid.Parse(key);
                PendingAttendance pendingAttendence = dbContext.PendingAttendances.Where(a => a.ApproveKey == approveKey).FirstOrDefault();

                if (pendingAttendence != null)
                {
                    IList<Attendance> ListexistAtendence = dbContext.Attendances.Where(a => a.Year == pendingAttendence.Year
                        && a.Month == pendingAttendence.Month && a.Day == pendingAttendence.Day && a.LocationId == 27
                        && a.EmployeeId == pendingAttendence.EmployeeId).ToList();



                    using (var transaction = dbContext.Database.BeginTransaction())
                    {

                        pendingAttendence.ApproveType = 2;
                        try
                        {
                            foreach (var item in ListexistAtendence)
                            {
                                dbContext.Attendances.Remove(item);
                            }

                            dbContext.SaveChanges();
                            transaction.Commit();
                            Val = SendRejectmWorkingFromHomemail(new DateTime(pendingAttendence.Year, pendingAttendence.Month, pendingAttendence.Day), pendingAttendence.EmployeeId, pendingAttendence.Description);

                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return Json(new { status = "Error occured....." });

                        }
                    }

                }


            }

            return Json(new { status = Val });
        }


        public JsonResult doRejectAttendence(int? id)
        {

            string Val = string.Empty;
            PendingAttendance pendingAttendence = dbContext.PendingAttendances.Where(a => a.Id == id).FirstOrDefault();



            if (pendingAttendence != null)
            {
                IList<Attendance> ListexistAtendence = dbContext.Attendances.Where(a => a.Year == pendingAttendence.Year
                    && a.Month == pendingAttendence.Month && a.Day == pendingAttendence.Day && a.LocationId == 27
                    && a.EmployeeId == pendingAttendence.EmployeeId).ToList();
                using (var transaction = dbContext.Database.BeginTransaction())
                {

                    pendingAttendence.ApproveType = 2;
                    try
                    {
                        foreach (var item in ListexistAtendence)
                        {
                            dbContext.Attendances.Remove(item);
                        }

                        dbContext.SaveChanges();
                        transaction.Commit();
                        Val = SendRejectmWorkingFromHomemail(new DateTime(pendingAttendence.Year, pendingAttendence.Month, pendingAttendence.Day), pendingAttendence.EmployeeId, pendingAttendence.Description);

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return Json(new { status = "Error occured....." });

                    }
                }
                return Json(new { status = Val });


            }

            return null;
        }



        public ActionResult SearchIncompleteAttendences(DateTime DateFrom, DateTime DateTo)
        {
            ViewModels.AbsentEmloyeeReportViewModel model = new ViewModels.AbsentEmloyeeReportViewModel();
            DateTime recursiveDate = DateFrom;
            try
            {
                for (int index = 0; recursiveDate <= DateTo; index++)
                {


                    List<int> _exitLocationList = Repositories.TimeTrendRepository.GetExitLocationMachins();

                    _exitLocationList.AddRange(dbContext.Locations.Where(a => a.OnSiteLocation == true).Select(a => a.Id).ToList());

                    var Atemployees = EmployeeRepository.GetAllEmployees().Where(a => a.IsEnable).OrderBy(a => a.Name);
                    foreach (var item in Atemployees)
                    {

                        List<Attendance> AttList = dbContext.Attendances.Where(a => a.EmployeeId == item.Id && a.Year == recursiveDate.Year && a.Month == recursiveDate.Month && a.Day == recursiveDate.Day).ToList();

                        var Groupatt = AttList.GroupBy(b => new DateTime(b.Year, b.Month, b.Day)).ToList();
                        foreach (var Gatt in Groupatt)
                        {
                            var GattIn = Gatt.Where(a => a.InOutMode == "in");
                            var FirstIn = GattIn.OrderBy(a => a.Year).ThenBy(a => a.Month).ThenBy(a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second).FirstOrDefault();
                            var GattOut = Gatt.Where(a => a.InOutMode == "out");
                            var LasstOut = GattOut.OrderBy(a => a.Year).ThenBy(a => a.Month).ThenBy(a => a.Day).ThenBy(a => a.Hour).ThenBy(a => a.Minute).ThenBy(a => a.Second).LastOrDefault();

                            if (FirstIn != null && !_exitLocationList.Contains(FirstIn.LocationId))
                            {
                                model.AbsentEmployeeList.Add(new ViewModels.AbsentEmloyeeViewModel()
                                {
                                    EmployeeID = FirstIn.EmployeeId.ToString(),
                                    AbsentDate = recursiveDate.ToShortDateString(),
                                    EmployeeName = string.Format("<a href=\"javascript:new HomeLogin().QuickFindWithEmployee({0},'{1}');\" style=\"text-decoration:none;\">{2}</a>",
                                    LasstOut.EmployeeId, recursiveDate.ToString("dd/MM/yyyy"), item.Name)
                                });

                            }
                            else if (LasstOut != null && !_exitLocationList.Contains(LasstOut.LocationId))
                            {
                                model.AbsentEmployeeList.Add(new ViewModels.AbsentEmloyeeViewModel()
                                {
                                    EmployeeID = LasstOut.EmployeeId.ToString(),
                                    AbsentDate = recursiveDate.ToShortDateString(),
                                    EmployeeName = string.Format("<a href=\"javascript:new HomeLogin().QuickFindWithEmployee({0},'{1}');\" style=\"text-decoration:none;\">{2}</a>",
                                    LasstOut.EmployeeId, recursiveDate.ToString("dd/MM/yyyy"), item.Name)
                                });

                            }


                        }

                    }
                    recursiveDate = recursiveDate.AddDays(1);
                    index++;
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                return Json(new { Success = "false", Message = ex.StackTrace });
            }
        }

        [HttpPost]
        //[DelphiAuthentication]
        public JsonResult SendComment(string comment)
        {
            return Json(EmailComment(comment));
        }

        public string SendWorkingFromHomemail(DateTime FromeDate, DateTime ToDate, int employeeId, int SupervisorId, Guid Key, string description, string taskList)
        {

            var host = ConfigurationManager.AppSettings["MailHost"];
            var port = ConfigurationManager.AppSettings["MailPort"];
            var fromAddress = ConfigurationManager.AppSettings["FromAddress"];
            var Username = ConfigurationManager.AppSettings["MailUserName"];
            var password = ConfigurationManager.AppSettings["MailPassword"];
            var DefaultUrl = ConfigurationManager.AppSettings["DefaultUrl"];
            var CCAddress = ConfigurationManager.AppSettings["CommentsCCTo"];


            EmployeeData _loggedUser = EmployeeRepository.GetEmployee(employeeId);

            EmployeeData Supervisor = EmployeeRepository.GetEmployee(SupervisorId);

            var message = "Hi, <br><br>This is to inform that<i><b> " + _loggedUser.Name + " </b></i>worked from home on " + FromeDate.ToString("yyyy-MM-dd") + " From " + FromeDate.ToString("HH:mm") + " to " + ToDate.ToString("HH:mm") + "(" + (ToDate - FromeDate).ToString() + " hrs)<br>"
                + "<br><b>Description: </b>" + description + "<br><br>" +
                "Please <b>confirm</b> it through following Link : " + DefaultUrl + "/api/login/GetApprovedAttendece?key=" + Key.ToString() + "<br><br>" +
                "Please <b>reject</b> it through following Link : " + DefaultUrl + "/api/login/GetRejectedAttendece?key=" + Key.ToString() + "<br><br>";

            if (!string.IsNullOrEmpty(taskList))
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("<b>Tasks worked on: </b><br>");
                stringBuilder.AppendLine("<ul>");
                string[] tasks = taskList.Split(new string[] { "#;" }, StringSplitOptions.None);
                foreach (var task in tasks)
                {
                    stringBuilder.AppendLine($"<li><span>{task}</span></li>");
                }
                stringBuilder.AppendLine("</ul><br>");
                message = message + stringBuilder.ToString();
            }

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
                Body = message,
                Subject = "Working From Home - " + _loggedUser.Name + " on " + FromeDate.ToString("yyyy-MM-dd"),
                From = new MailAddress(fromAddress, "ITS")

            };


            mail.To.Add(new MailAddress(Supervisor.PrimaryEmailAddress));
            string _message = "";
            try
            {
                smtpClient.Send(mail);
                _message = "Success";
            }
            catch (Exception Ex)
            {
                _message = "Error occurred while sending your mail.";
                throw;
            }

            return _message;
        }

        public string SendConfirmWorkingFromHomemail(DateTime FromeDate, int employeeId, string description)
        {

            var host = ConfigurationManager.AppSettings["MailHost"];
            var port = ConfigurationManager.AppSettings["MailPort"];
            var fromAddress = ConfigurationManager.AppSettings["FromAddress"];
            var Username = ConfigurationManager.AppSettings["MailUserName"];
            var password = ConfigurationManager.AppSettings["MailPassword"];
            var CCAddress = ConfigurationManager.AppSettings["CommentsCCTo"];

            EmployeeData _loggedUser = EmployeeRepository.GetEmployee(employeeId);



            var message = "This is to inform that " + _loggedUser.Name + " worked at home on " + FromeDate.ToString("yyyy-MM-dd") + " is Confirmed" +

                "<br><br> Description :" + description +
                "<br><br><br><br>";



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
                Body = message,
                Subject = "Working From Home - " + _loggedUser.Name + " on " + FromeDate.ToString("yyyy-MM-dd") + " is Confirmed",
                From = new MailAddress(fromAddress, "ITS")

            };


            mail.To.Add(new MailAddress(_loggedUser.PrimaryEmailAddress));
            string _message = "";
            try
            {
                smtpClient.Send(mail);
                _message = "Success";
            }
            catch (Exception Ex)
            {
                _message = "Error occurred while sending your mail.";
                throw;
            }

            return _message;

        }

        public string SendRejectmWorkingFromHomemail(DateTime FromeDate, int employeeId, string description)
        {

            var host = ConfigurationManager.AppSettings["MailHost"];
            var port = ConfigurationManager.AppSettings["MailPort"];
            var fromAddress = ConfigurationManager.AppSettings["FromAddress"];
            var Username = ConfigurationManager.AppSettings["MailUserName"];
            var password = ConfigurationManager.AppSettings["MailPassword"];
            var CCAddress = ConfigurationManager.AppSettings["CommentsCCTo"];

            EmployeeData _loggedUser = EmployeeRepository.GetEmployee(employeeId);



            var message = "This is to inform that " + _loggedUser.Name + " worked at home on " + FromeDate.ToString("yyyy-MM-dd") + " is rejected."
               + "<br><br> Description : " + description
                + "<br><br>" + "Please contact your Project supervisor." + "<br><br><br><br>";


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
                Body = message,
                Subject = "Working From Home - " + _loggedUser.Name + " on " + FromeDate.ToString("yyyy-MM-dd") + " is Rejected",
                From = new MailAddress(fromAddress, "ITS")

            };


            mail.To.Add(new MailAddress(_loggedUser.PrimaryEmailAddress));
            string _message = "";
            try
            {
                smtpClient.Send(mail);
                _message = "Success";
            }
            catch (Exception Ex)
            {
                _message = "Error occurred while sending your mail.";
                throw;
            }

            return _message;

        }

        //[DelphiAuthentication]
        public string EmailComment(string message)
        {


            var employeeId = int.Parse(Session["EmployeeId"].ToString());
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
                Subject = "MyTime Comment",
                From = new MailAddress(fromAddress, "ITS")

            };

            mail.To.Add(new MailAddress(toAddress));

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

        private string CaptureAllTasks(IList<WorkingFromHomeTask> WorkingFromHomeTaskList)
        {
            string taskList = string.Empty;
            for (int index = 0; index < WorkingFromHomeTaskList.Count; index++)
            {
                if (!string.IsNullOrEmpty(WorkingFromHomeTaskList[index].Description))
                {
                    if (index == 0)
                    {
                        taskList += $"{WorkingFromHomeTaskList[index].Description} ({WorkingFromHomeTaskList[index].Hours}:{WorkingFromHomeTaskList[index].Minutes})";
                    }
                    else if (index == WorkingFromHomeTaskList.Count - 1)
                    {
                        taskList += $"#;{WorkingFromHomeTaskList[index].Description} ({WorkingFromHomeTaskList[index].Hours}:{WorkingFromHomeTaskList[index].Minutes})";
                    }
                    else
                    {
                        taskList += $"#;{WorkingFromHomeTaskList[index].Description} ({WorkingFromHomeTaskList[index].Hours}:{WorkingFromHomeTaskList[index].Minutes})";
                    }
                }
            }
            return taskList;
        }

        private bool ValidateTaskTimeAgainstInOutTime(DateTime attendanceDateIn, DateTime attendanceDateOut, IList<WorkingFromHomeTask> workingFromHomeTaskList)
        {
            var timeSpent = attendanceDateOut.Subtract(attendanceDateIn);
            var timeSpentInMinutes = timeSpent.TotalMinutes;

            var taskMinutes = 0;
            var taskHours = 0;
            for (int index = 0; index < workingFromHomeTaskList.Count; index++)
            {
                taskHours += workingFromHomeTaskList[index].Hours;
                taskMinutes += workingFromHomeTaskList[index].Minutes;
            }
            var taskTotalMinutes = taskMinutes + (taskHours * 60);

            if (taskTotalMinutes > timeSpentInMinutes)
                return false;
            else
                return true;
        }

        //public string EmailComment(string message)
        //{
        //    //string _companyDomain = ConfigurationManager.AppSettings["CompanyDomain"].ToString();
        //    //string _defaultMailSendTo = ConfigurationManager.AppSettings["CommentsSendTo"].ToString();
        //    //string _defaultMailCCTo = ConfigurationManager.AppSettings["CommentsCCTo"].ToString();


        //    #region --- Creating Mail Object ---


        //    EmployeeData _loggedUser = EmployeeRepository.GetEmployee(int.Parse(HttpRuntime.Cache.Get("EmployeeID").ToString()));


        //    var host = ConfigurationManager.AppSettings["MailHost"];
        //    var port = ConfigurationManager.AppSettings["MailPort"];
        //    var fromAddress = _loggedUser.PrimaryEmailAddress;
        //    var displayName = _loggedUser.Name;
        //    var mailSubject = "MyTime Comment";
        //    var Username = ConfigurationManager.AppSettings["MailUserName"];
        //    var password = ConfigurationManager.AppSettings["MailPassword"];

        //    var smtpClient = new SmtpClient
        //    {
        //        DeliveryMethod = SmtpDeliveryMethod.Network,
        //        EnableSsl = true,
        //        Host = host,
        //        Port = Convert.ToInt32(port)
        //    };

        //    var credentials = new System.Net.NetworkCredential(Username, password);
        //    smtpClient.UseDefaultCredentials = false;
        //    smtpClient.Credentials = credentials;

        //    string emailAddress = "smtp@exilesoft.com";




        //    var mail = new MailMessage
        //    {
        //        IsBodyHtml = true,
        //        Body = message,
        //        Subject = mailSubject,
        //        From = new MailAddress(fromAddress, displayName)
        //    };

        //    mail.To.Add(new MailAddress(emailAddress));


        //    #endregion

        //    string _message = string.Empty;
        //    try
        //    {
        //        smtpClient.Send(mail);
        //        _message = "Successful";
        //    }
        //    catch (Exception Ex)
        //    {
        //        _message = "Error occurred while sending your comment.";
        //        throw;
        //    }

        //    return _message;
        //}

        #region Helpers

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private static void HttpPost(StringBuilder data, string url)
        {
            HttpWebResponse response = null;

            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(data.ToString());

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;
                request.ClientCertificates.Add(new X509Certificate());

                Stream postStream = request.GetRequestStream();
                postStream.Write(byteArray, 0, byteArray.Length);
                postStream.Close();

                response = (HttpWebResponse)request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string json = reader.ReadToEnd();
                    Console.WriteLine(json);

                    var ser = new JavaScriptSerializer();
                    var x = (Dictionary<string, object>)ser.DeserializeObject(json);

                    if (x != null)
                    {
                        string issuedOn = Base64Decode(x["IssuedOn"].ToString());
                        string expiresIn = Base64Decode(x["ExpiresIn"].ToString());
                        var idToken = x["IdToken"] as string;
                        string token = Base64Decode(x["AuthorizationToken"].ToString());

                        string[] id_token_payload = idToken.Split('.');

                        var payload = System.Web.Helpers.Json.Decode(Base64Decode(id_token_payload[1]));
                        var xx = (Dictionary<string, object>)ser.DeserializeObject(Base64Decode(id_token_payload[1]));
                        CookieHelper.SetCookie(xx["email"].ToString(), Convert.ToInt32(xx["employeeId"]), xx["roles"] as object[]);
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    using (var err = (HttpWebResponse)e.Response)
                    {
                        Console.WriteLine("The server returned '{0}' with the status code '{1} ({2:d})'.",
                          err.StatusDescription, err.StatusCode, err.StatusCode);
                    }
                }
            }
            finally
            {
                if (response != null) { response.Close(); }
            }
        }

        private static string Authenticate()
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["ClientId"]);
            var clientId = Convert.ToBase64String(plainTextBytes);

            var authUrl = ConfigurationManager.AppSettings["AuthUrl"];
            var callbackUrl = ConfigurationManager.AppSettings["CallBackUrl"];

            var url = string.Format("{0}clientId={1}&responseType=code&redirectUri={2}", authUrl,
                Uri.EscapeDataString(clientId), callbackUrl);

            return url;
        }

        #endregion
    }
}
