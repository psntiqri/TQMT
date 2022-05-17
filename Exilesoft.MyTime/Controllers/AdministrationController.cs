using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Exilesoft.Models;
using Exilesoft.MyTime.Filters;
using Exilesoft.MyTime.Helpers;
using Exilesoft.MyTime.Mappings;
using Exilesoft.MyTime.Repositories;
using Exilesoft.MyTime.ViewModels;
using System.Data.Entity.SqlServer;
using System.Configuration;
using Hangfire;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace Exilesoft.MyTime.Controllers
{
    public class AdministrationController : BaseController
    {

        private Context _dbContext = new Context();
        //
        // GET: /Administration/
        private Context dbContext = new Context();

        //[DelphiAuthentication("Admin", "Manager")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SpecialEventList()
        {
           
            return View();
        }
        public ActionResult loadHangFire()
        {
            var DefaultUrl = ConfigurationManager.AppSettings["DefaultUrl"] + "/hangfire";
            return Redirect(DefaultUrl);
        }
        [HttpPost]
        public JsonResult GetEmployeeTableEntries()
        {
            List<SpecialEventListViewModel> specialEventListViewModel = new List<SpecialEventListViewModel>();
            var specilaEventList = dbContext.SpecialEvents.ToList();
            foreach (var specialEvent in specilaEventList)
            {
                specialEventListViewModel.Add(new SpecialEventListViewModel
                {
                    EditLink = string.Format("<a href=\"javascript:new AdministrationForm().AddEditSpecialEvent({0});\">Edit</a>", specialEvent.Id),
                    Description = specialEvent.Description,
                    EventName = specialEvent.EventName,
                    EventFromDate = specialEvent.EventFromDate != null ? specialEvent.EventFromDate.Value.ToShortDateString() : string.Empty,
                    EventToDate = specialEvent.EventToDate != null ? specialEvent.EventToDate.Value.ToShortDateString() : string.Empty,
                });
            }
            return Json(specialEventListViewModel);
        }

        [HttpPost]
        public ActionResult AddEditSpecialEvent(int? id)
        {
            SpecialEvent specialEvent = new SpecialEvent();
            if (id != null)
                specialEvent = dbContext.SpecialEvents.Single(e => e.Id == id);
            return View(specialEvent);
        }

        [HttpPost]
        public JsonResult SaveSpecialEvent(SpecialEvent model)
        {
            SpecialEvent specialEvent = new SpecialEvent();

            if (model.Id != 0)
                specialEvent = dbContext.SpecialEvents.Single(e => e.Id == model.Id);
            specialEvent.EventName = model.EventName;
            specialEvent.Description = model.Description;
            specialEvent.EventFromDate = model.EventFromDate;
            specialEvent.EventToDate = model.EventToDate;

            try
            {
                if (specialEvent.Id == 0)
                    dbContext.SpecialEvents.Add(specialEvent);
                dbContext.SaveChanges();
                return Json(new { status = "Success" });
            }
            catch (Exception Ex)
            {
                return Json(new { status = "Failed", message = Ex.Message });
            }

        }

        //[Authorize]
        //[DelphiAuthentication("Admin", "Manager")]
        public ActionResult UnplannedLeaves(string fromDate, string toDate)
        {
            ViewModels.PhysicallyNotAvailableEmployeModel phyciallyNotAvailableModel = new ViewModels.PhysicallyNotAvailableEmployeModel(null, null);
            return View(phyciallyNotAvailableModel);

        }

        // [Authorize]
        //[DelphiAuthentication("Admin")]
        public ActionResult UserManagement()
        {
            return View();
        }

        public JsonResult EmployeeAutoCompleteResult(string text)
        {
            List<EmployeeViewModel> employees =
                 (new EmployeeViewModelMapper()).Map(EmployeeRepository.EmployeeSearchByName(text));

            return Json(employees);
        }

        //[Authorize]
        //[DelphiAuthentication("Admin")]
        public ActionResult VisitorPass()
        {
            //ViewBag.Message = "Employee";
            //var employees = db.Employees.OrderBy(a => a.Name);
            var employees = EmployeeRepository.GetAllEmployees();
            ViewBag.EmployeeList = new SelectList(employees, "Id", "Name", "Id");
            ViewBag.date = DateTime.Today.ToShortDateString();
            return View();
        }

        [HttpPost]
        public JsonResult SaveVisitorPass(int employeeId, int cardNo, DateTime pickeddate)
        {
            var loggedEmployeeId = int.Parse(Session["EmployeeId"].ToString());

            EmployeeEnrollment employee = dbContext.EmployeeEnrollment.Find(employeeId);

            EmployeeEnrollment loogedEmployee = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == loggedEmployeeId);

            var visitorAttendanceswithCard = dbContext.VisitorAttendances.Where(a => a.CardNo == cardNo).ToList();
            var visitorAttendances = visitorAttendanceswithCard.Where(a => a.DateTime.Date == pickeddate.Date && a.isTransferred == false).ToList();

            foreach (var item in visitorAttendances)
            {
                Attendance attendace = new Attendance();
                //creating new Attendance
                attendace.Year = item.DateTime.Year;
                attendace.Month = item.DateTime.Month;
                attendace.Day = item.DateTime.Day;
                attendace.Hour = item.DateTime.Hour;
                attendace.Minute = item.DateTime.Minute;
                attendace.Second = item.DateTime.Second;
                attendace.Employee = employee;
                attendace.EmployeeId = employee.EmployeeId;
                attendace.CardNo = employee.CardNo;
                attendace.InOutMode = item.InOutMode;
                attendace.VerifyMode = item.VerifyMode;
                attendace.WorkCode = item.WorkCode;
                attendace.Location = item.Location;
                attendace.LocationId = item.LocationId;

                //modifying VisitorAttendance details
                item.isTransferred = true;
                item.TransferredBy = loogedEmployee;
                item.TransferredDate = DateTime.Today;

                if (loogedEmployee != null)
                {
                    var loggedEmployyeeName = EmployeeRepository.GetEmployee(loogedEmployee.EmployeeId);
                    item.Note = string.Format("Transferred to employee : {0} by {1} at {2}", employee.CardNo, loggedEmployyeeName, Utility.GetDateTimeNow().ToString());
                }

                dbContext.Attendances.Add(attendace);
            }

            VisitorPassAllocation visitorPassAllocation = new VisitorPassAllocation();
            visitorPassAllocation.AssignDate = pickeddate.Date;
            visitorPassAllocation.EmployeeEnrollment = employee;
            visitorPassAllocation.CardNo = cardNo;
            visitorPassAllocation.IsActive = true;
            if (pickeddate.Date < DateTime.Today.Date)
                visitorPassAllocation.IsActive = false;
            dbContext.VisitorPassAllocations.Add(visitorPassAllocation);

            dbContext.SaveChanges();
            //ViewBag.TransferMessage = "Visitor pass allocated successfully.";
            var employees = EmployeeRepository.GetAllEmployees();
            //var employees = dbContext.Employees.OrderBy(a => a.Name);
            ViewBag.EmployeeList = new SelectList(employees, "Id", "Name", "Id");
            ViewBag.date = DateTime.Today.ToShortDateString();
            return Json(new { status = "Success" });
        }
        public ActionResult FloorInformation()
        {


            var locations = dbContext.Locations.OrderBy(a => a.Floor);
            ViewBag.LocationList = new SelectList(locations, "Id", "Floor", "Id");

            ViewBag.dateTime = Utility.GetDateTimeNow().ToString("dd/MM/yyyy HH:mm");


            return View();

        }

        [HttpPost]
        public JsonResult FindFloorInformation(int locationId, DateTime selectedDate, string userType)
        {

            return Json(new { SearchResult = AttendanceRepository.getEmployeeByFloor(locationId, selectedDate, userType) });


        }

        [HttpPost]
        public JsonResult ShowDetailInOut(int locationId, DateTime SelectedDate, int UserId, string UserName, string UserType)
        {

            return Json(new { SearchResult = AttendanceRepository.ShowDetailInOut(locationId, SelectedDate, UserId, UserName, UserType) });

        }

        internal static List<string> GetExitLocationMachins()
        {
            string _exitLocationMachines = ConfigurationManager.AppSettings["ExitLocationMachines"].ToString();
            List<string> _locationIDList = new List<string>();
            foreach (string locationID in _exitLocationMachines.Split(','))
                _locationIDList.Add(locationID);

            return _locationIDList;
        }
        //[DelphiAuthentication("Admin", "Manager")]
        public ActionResult GetIncompleteAttendences(string fromDate, string toDate)
        {

            ViewModels.PhysicallyNotAvailableEmployeModel phyciallyNotAvailableModel = new ViewModels.PhysicallyNotAvailableEmployeModel(null, null);
            return View(phyciallyNotAvailableModel);

        }
        
        //[Authorize]
        //[DelphiAuthentication("Admin")]
        public ActionResult ManualAttendance()
        {

            var employeeId = int.Parse(Session["EmployeeId"].ToString());
            //EmployeeEnrollment loggedUser = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.UserName == User.Identity.Name);
            EmployeeEnrollment loggedUser = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == employeeId);
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

            var employees = EmployeeRepository.GetAllEmployees().Where(a => a.IsEnable).OrderBy(a => a.Name);
            //var employees = dbContext.Employees.OrderBy(a => a.Name);
            ViewBag.EmployeeList = new SelectList(employees, "Id", "Name", "Id");
            var locations = dbContext.Locations.OrderBy(a => a.Floor);
            ViewBag.LocationList = new SelectList(locations, "Id", "Floor", "Id");

            ViewBag.dateTime = Utility.GetDateTimeNow().ToString("dd/MM/yyyy HH:mm");
            return View();
        }

        public ActionResult MissingTimeEntries()
        {
            IList<EmployeeWiseMissingEntries> empMissingEntries = MissingEntriesRepository.getEmployeeMissingEntries();
            MissingEntriesViewModel missingEntriesViewModel = new MissingEntriesViewModel();
            missingEntriesViewModel.entries = empMissingEntries;
            return View(missingEntriesViewModel);
        }

        public async Task<ActionResult> SendEmail(string entry)
        {
            EmployeeWiseMissingEntries employeeWiseMissingEntries = JsonConvert.DeserializeObject<EmployeeWiseMissingEntries>(entry, new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local });
            EmployeeData emp = EmployeeRepository.GetEmployee(Int32.Parse(employeeWiseMissingEntries.employeeId));

            string msg = "";
            msg = " <div> Hi   <label>" + emp.Name+ ",</label>" + " <p> This is to inform you that you have not entered time for following days.  </p>";

            foreach(DateTime date in employeeWiseMissingEntries.missingDates)
            {
                msg = msg + "<div> <label>" + date.Date.ToString().Split(' ')[0]+ " </label> </div>";
            }



            var sendEmail = new {
                email = emp.PrimaryEmailAddress,
                body = msg,
            };

            var json = JsonConvert.SerializeObject(sendEmail);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var url = ConfigurationManager.AppSettings["MissingTimeEmail"]+"/api/SendEmailMonth";
            var client = new HttpClient();

            var response =  await client.PostAsync(url, data);

            //if (response.IsSuccessStatusCode)
            //{
            //    int id = int.Parse(employeeWiseMissingEntries.employeeId);
            //    List<EmployeeMissingEntry> entities = dbContext.EmployeeMissingEntries.Where(item => item.EmployeeId == id).ToList();


            //    foreach (EmployeeMissingEntry entity in entities)
            //    {
            //        if (entity != null)
            //        {

            //            entity.Mailed = true;


            //        }
            //    }
            //    dbContext.SaveChanges();
            //}
            


            return new HttpStatusCodeResult(response.StatusCode); ;
            //string result = response.Content.ReadAsStringAsync().Result;
        }

        //[Authorize]
        //[DelphiAuthentication("Admin")]
        public ActionResult ManualAttendanceBulk()
        {
            var employeeId = int.Parse(Session["EmployeeId"].ToString());

            EmployeeEnrollment loggedUser = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == employeeId);
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
            var employees = EmployeeRepository.GetAllEmployees().Where(a => a.IsEnable).OrderBy(a => a.Name);

            ViewBag.EmployeeList = employees;

            var locations = dbContext.Locations.OrderBy(a => a.Floor);
            ViewBag.LocationList = new SelectList(locations, "Id", "Floor", "Id");

            ViewBag.dateTime = Utility.GetDateTimeNow().ToString("dd/MM/yyyy HH:mm");
            return View();
        }
        [HttpPost]
        //[DelphiAuthentication("Admin")]
        public JsonResult SaveManualAttendance(int employeeId, int locationId, string inOutMode, DateTime attendanceDate)
        {
            if (attendanceDate == null)
            {
                return Json(new { status = "Please enter attendence Date and Time." });
            }

            Location location = dbContext.Locations.Find(locationId);
            EmployeeEnrollment loogedEmployee = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == employeeId);

            var attendace = new Attendance();
            attendace.Day = attendanceDate.Day;
            attendace.Employee = loogedEmployee;
            attendace.EmployeeId = loogedEmployee.EmployeeId;
            attendace.CardNo = loogedEmployee.CardNo;
            attendace.Hour = attendanceDate.Hour;
            attendace.InOutMode = inOutMode;
            attendace.Location = location;
            attendace.LocationId = location.Id;
            attendace.Minute = attendanceDate.Minute;
            attendace.Month = attendanceDate.Month;
            attendace.Second = attendanceDate.Second;
            attendace.Year = attendanceDate.Year;
            dbContext.Attendances.Add(attendace);

            dbContext.SaveChanges();

            var employees = EmployeeRepository.GetAllEmployees().Where(a => a.IsEnable).OrderBy(a => a.Name);
            ViewBag.EmployeeList = new SelectList(employees, "Id", "Name", "Id");
            var locations = dbContext.Locations.OrderBy(a => a.Floor);
            ViewBag.LocationList = new SelectList(locations, "Id", "Floor", "Id");
            ViewBag.dateTime = Utility.GetDateTimeNow().ToString("dd/MM/yyyy HH:mm");
            return Json(new { status = "Success" });
        }
        [HttpPost]
        //[DelphiAuthentication("Admin")]
        public JsonResult SaveManualAttendanceBulk(string[] Ids, int locationId, string inOutMode, DateTime attendanceDate)
        {
            if (attendanceDate == null)
            {
                return Json(new { status = "Please enter attendence Date and Time." });
            }

            if (Ids != null & attendanceDate != null)
            {
                foreach (var EmpId in Ids)
                {
                    int EmployeeId = int.Parse(EmpId);
                    Location location = dbContext.Locations.Find(locationId);
                    EmployeeEnrollment loogedEmployee = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == EmployeeId);

                    var attendace = new Attendance();
                    attendace.Day = attendanceDate.Day;
                    attendace.Employee = loogedEmployee;
                    attendace.EmployeeId = loogedEmployee.EmployeeId;
                    attendace.CardNo = loogedEmployee.CardNo;
                    attendace.Hour = attendanceDate.Hour;
                    attendace.InOutMode = inOutMode;
                    attendace.Location = location;
                    attendace.LocationId = location.Id;
                    attendace.Minute = attendanceDate.Minute;
                    attendace.Month = attendanceDate.Month;
                    attendace.Second = attendanceDate.Second;
                    attendace.Year = attendanceDate.Year;
                    dbContext.Attendances.Add(attendace);
                }

                dbContext.SaveChanges();
            }
            var employees = EmployeeRepository.GetAllEmployees().Where(a => a.IsEnable).OrderBy(a => a.Name);
            ViewBag.EmployeeList = employees;
            var locations = dbContext.Locations.OrderBy(a => a.Floor);
            ViewBag.LocationList = new SelectList(locations, "Id", "Floor", "Id");
            ViewBag.dateTime = Utility.GetDateTimeNow().ToString("dd/MM/yyyy HH:mm");

            return Json(new { status = "Success" });
        }

        //[HttpPost]
        //[DelphiAuthentication("Admin")]
        //public ActionResult SaveManualAttendanceBulk(FormCollection formCollection)
        //{
        //    int locationId = int.Parse(formCollection["locationId"]);
        //    DateTime attendanceDate = DateTime.Parse(formCollection["AttendanceDate"]);
        //    string inOutMode = formCollection["InOutMode"];

        //    String[] Ids = formCollection["Prints"].Split(',');

        //    foreach (var EmpId in Ids)
        //    {
        //        int EmployeeId = int.Parse(EmpId);
        //        Location location = dbContext.Locations.Find(locationId);
        //        EmployeeEnrollment loogedEmployee = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == EmployeeId);

        //        var attendace = new Attendance();
        //        attendace.Day = attendanceDate.Day;
        //        attendace.Employee = loogedEmployee;
        //        attendace.EmployeeId = loogedEmployee.EmployeeId;
        //        attendace.CardNo = loogedEmployee.CardNo;
        //        attendace.Hour = attendanceDate.Hour;
        //        attendace.InOutMode = inOutMode;
        //        attendace.Location = location;
        //        attendace.LocationId = location.Id;
        //        attendace.Minute = attendanceDate.Minute;
        //        attendace.Month = attendanceDate.Month;
        //        attendace.Second = attendanceDate.Second;
        //        attendace.Year = attendanceDate.Year;
        //        dbContext.Attendances.Add(attendace);
        //    }

        //    dbContext.SaveChanges();
        //    var employees = EmployeeRepository.GetAllEmployees().Where(a => a.IsEnable).OrderBy(a => a.Name);
        //    ViewBag.EmployeeList = employees;
        //    var locations = dbContext.Locations.OrderBy(a => a.Floor);
        //    ViewBag.LocationList = new SelectList(locations, "Id", "Floor", "Id");
        //    ViewBag.dateTime = Utility.GetDateTimeNow().ToString("dd/MM/yyyy HH:mm");

        //    //return View("ManualAttendanceBulk");
        //    //return Json(new { status = "Success" });
        //    return PartialView("ManualAttendanceBulk", "Landing");
        //    //return RedirectToAction("ManualAttendanceBulk", "Administration");
        //    //return Json(null);
        //}
        //[Authorize]
        //[DelphiAuthentication("Admin", "Manager")]
        public ActionResult AttendanceReport()
        {
            //ViewBag.Message = "Employee";
            ViewBag.Privileges = new SelectList(PrivilegesRepository.GetAllPrivilegeList(), "Id", "Name", 0);
            ViewBag.TitleList = new SelectList(EmployeeRepository.titles, "Id", "Name", 0);
            ViewBag.CivilStatuses = new SelectList(EmployeeRepository.civilStatuses, "Id", "Name", 0);
            ViewBag.GenderList = new SelectList(EmployeeRepository.genderList, "Id", "Name", 0);
            return View(new ViewModels.AttendanceReportViewModel());
        }

        //[DelphiAuthentication("Admin", "Manager")]
        [HttpPost]
        public ActionResult AttendanceReport(ViewModels.AttendanceReportViewModel model)
        {
            if (model.AttendenceRptTypeId == "0")
            {

                var _reportContents = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/attnd_rpt.mht"));
                var _reportPageBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/attnd_body.txt"));
                var _reportRowBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/attnd_row_body.txt"));

                Repositories.DailyAttendanceRepository.GenerateAttandanceReport(model, _reportContents, _reportPageBody, _reportRowBody);
            }
            else if (model.AttendenceRptTypeId == "1")
            {
                var _reportContents = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/attnd_rpt.mht"));
                var _reportPageBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/attnd_body.txt"));
                var _reportRowBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/attnd_row_body.txt"));

                Repositories.DailyAttendanceRepository.GenerateTeamAttandanceReport(model, _reportContents, _reportPageBody, _reportRowBody);
            }
            else if (model.AttendenceRptTypeId == "2")
            {
                var _reportContents = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/coverage_Monthly_rpt.mht"));
                var _reportPageBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/coverage_Monthly_body.txt"));
                var _reportRowBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/coverage_Monthly_row_body.txt"));
                Repositories.DailyAttendanceRepository.GenerateCoverageReportMonthly(model, _reportContents, _reportPageBody, _reportRowBody);
            }
            else if (model.AttendenceRptTypeId == "3")
            {
                var _reportContents = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/coverage_Weekly_rpt.mht"));
                var _reportPageBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/coverage_Weekly_body.txt"));
                var _reportRowBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/coverage_Weekly_row_body.txt"));
                Repositories.DailyAttendanceRepository.GenerateCoverageReportWeekly(model, _reportContents, _reportPageBody, _reportRowBody);
                          

            }
           
            else if (model.AttendenceRptTypeId == "4")
            {
                var _reportContents = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/Team_Coverage_Monthly_rpt.mht"));
                var _reportPageBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/Team_Coverage_Monthly_body.txt"));
                var _reportRowBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/Team_Coverage_Monthly_row_body.txt"));

                Repositories.DailyAttendanceRepository.GenerateTeamCoverageReportMonthly(model, _reportContents, _reportPageBody, _reportRowBody);
            }
            else if (model.AttendenceRptTypeId == "5")
            {
                var _reportContents = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/Team_Coverage_Weekly_rpt.mht"));
                var _reportPageBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/Team_Coverage_Weekly_body.txt"));
                var _reportRowBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/Team_Coverage_Weekly_row_body.txt"));

                Repositories.DailyAttendanceRepository.GenerateTeamCoverageReportWeekly(model, _reportContents, _reportPageBody, _reportRowBody);
            }
            else if (model.AttendenceRptTypeId == "6")
            {
                var _reportContents = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/Coverage_Summary_rpt.mht"));
                var _reportPageBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/Coverage_Summary_body.txt"));
                var _reportRowBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/Coverage_Summary_row_body.txt"));

                Repositories.DailyAttendanceRepository.GenerateCoverageSummaryMonthly(model, _reportContents, _reportPageBody, _reportRowBody);
            }
            else if (model.AttendenceRptTypeId == "7")
            {
                var _reportContents = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/coverage_rpt.mht"));
                var _reportPageBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/coverage_body.txt"));
                var _reportRowBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/coverage_row_body.txt"));

                var client = new BackgroundJobClient();

                client.Enqueue(() => Repositories.DailyAttendanceRepository.GenerateCoverageReportYearly(model, _reportContents, _reportPageBody, _reportRowBody));

            }
            else if (model.AttendenceRptTypeId == "8")
            {
                var _reportContents = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/working_From_Home_rpt.mht"));
                var _reportPageBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/working_From_Home_body.txt"));
                var _reportRowBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/working_From_Home_row_body.txt"));

                 Repositories.DailyAttendanceRepository.GenerateWorkingFromHome(model, _reportContents, _reportPageBody, _reportRowBody);

            }
            else if (model.AttendenceRptTypeId == "9")
            {
                var _reportContents = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/attnd_daily_rpt.mht"));
                var _reportPageBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/attnd_daily_body.txt"));
                var _reportRowBody = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/excl/attnd_daily_row_body.txt"));

                Repositories.DailyAttendanceRepository.GenerateDailyAttandanceReport(model, _reportContents, _reportPageBody, _reportRowBody);

            }
            return RedirectToAction("AttendanceReport");
        }

    }
}
