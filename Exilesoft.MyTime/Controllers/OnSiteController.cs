using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Exilesoft.Models;
using Exilesoft.MyTime.Filters;
using Exilesoft.MyTime.Helpers;
using Exilesoft.MyTime.Repositories;
using Exilesoft.MyTime.ViewModels;


namespace Exilesoft.MyTime.Controllers
{
    public class OnSiteController : BaseController
    {
        private Context db = new Context();

        //
        // GET: /OnSite/
        //[DelphiAuthentication]
        public ViewResult Index()
        {
            ViewBag.Message = "OnSite";
            int logedInUserId = int.Parse(Session["EmployeeId"].ToString());
            EmployeeEnrollment loggedUser = db.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == logedInUserId);

            if (Thread.CurrentPrincipal.IsInRole("Employee"))
            {
                ViewBag.Role = 0;
            }
            else
            {
                ViewBag.Role = 1;
            }
            
            var onsites = db.EmployeesOnSite.Include(o => o.EmployeeEnrollment);

			List<OnSiteEmployeeViewModel> OnSiteEmployeeList = new List<OnSiteEmployeeViewModel>();

	        foreach (var employeeOnSite in onsites)
	        {
		        var EmpName = EmployeeRepository.GetEmployee(employeeOnSite.EmployeeId);
				OnSiteEmployeeViewModel objOnSiteEmployeeViewModel = new OnSiteEmployeeViewModel();
		        objOnSiteEmployeeViewModel.Employee = EmpName.Name;
		        objOnSiteEmployeeViewModel.FromDate = employeeOnSite.FromDate.ToString();
		        objOnSiteEmployeeViewModel.ToDate = employeeOnSite.ToDate.ToString();
				objOnSiteEmployeeViewModel.MobileNumber = employeeOnSite.MobileNumber;
		        objOnSiteEmployeeViewModel.EditLink = employeeOnSite.Id.ToString();
		        objOnSiteEmployeeViewModel.DeleteLink = employeeOnSite.Id.ToString();
                objOnSiteEmployeeViewModel.IsPermanat = employeeOnSite.IsPermanant.ToString();
				OnSiteEmployeeList.Add(objOnSiteEmployeeViewModel);
	        }
           // return View(onsites.OrderBy(a => a.EmployeeId).ToList());
			return View(OnSiteEmployeeList);
        }

        //[DelphiAuthentication]
        public ActionResult OnSiteAttendanceList()
        {
            ViewBag.Message = "OnSite";

           // EmployeeEnrollment loggedUser = db.EmployeeEnrollment.FirstOrDefault(a => a.UserName == User.Identity.Name);
		   // ViewBag.Role = loggedUser.Privillage;
            //DateTime entriesBeforeDate = System.DateTime.Today.AddMonths(-1);
			//var onsites = db.Attendances.Where(a => a.EmployeeId == loggedUser.EmployeeId && a.Location.OnSiteLocation).Include(o => o.Location);

			var user = System.Web.HttpContext.Current.User;
            ViewBag.Role = Utility.GetUserRoleInt();
			var CurrentLoggedemployeeId = int.Parse(Session["EmployeeId"].ToString());
			DateTime entriesBeforeDate = System.DateTime.Today.AddMonths(-1);
			var onsites = db.Attendances.Where(a => a.EmployeeId == CurrentLoggedemployeeId && a.Location.OnSiteLocation).Include(o => o.Location);
           
   
            return View(onsites.ToList().Where(a => new DateTime(a.Year, a.Month, a.Day) > entriesBeforeDate));
        }

        //
        // GET: /OnSite/Details/5

        public ViewResult Details(int id)
        {
            EmployeeOnSite onsite = db.EmployeesOnSite.Find(id);
            return View(onsite);
        }

        //
        // GET: /OnSite/Create
        //[DelphiAuthentication]
        public ActionResult Create()
        {
            ViewBag.Message = "OnSite";
            ViewBag.EmployeeId = new SelectList(EmployeeRepository.GetAllEmployeeOrderByName(), "Id", "Name");
            ViewBag.LocationId = new SelectList(db.Locations.Where(a => a.DeviceNo == 0).OrderBy(a => a.Floor).ToList(), "Id", "Floor");
            return View();
        }

        //
        // POST: /OnSite/Create

        [HttpPost]
        public ActionResult Create(EmployeeOnSite onsite)
        {
            if (ModelState.IsValid)
            {
                db.EmployeesOnSite.Add(onsite);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.EmployeeId = new SelectList(EmployeeRepository.GetAllEmployeeOrderByName(), "Id", "Name", onsite.EmployeeId);
            return View(onsite);
        }

        //[DelphiAuthentication]
        public ActionResult OnSiteAttendance(int? id)
        {
            int logedInUserId = int.Parse(Session["EmployeeId"].ToString());
            EmployeeEnrollment loggedUser = db.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == logedInUserId);

            if (Thread.CurrentPrincipal.IsInRole("Employee"))
            {
                ViewBag.Role = 0;
            }
            else
            {
                ViewBag.Role = 1;
            }

           
            string employeeSelectedID = "Id";
            string locationSelectedID = "Id";

            Attendance attendance = null;
            if (id != null)
            {
                attendance = db.Attendances.FirstOrDefault(a => a.Id == id);
                if (attendance != null)
                {
                    ViewBag.OnSiteAttendanceID = id;
                    employeeSelectedID = attendance.EmployeeId.ToString();
                    locationSelectedID = attendance.LocationId.ToString();

                }
            }

            ViewBag.Message = "OnSite";
            var employees = EmployeeRepository.GetEmployee(loggedUser.EmployeeId);
            ViewBag.EmployeeList = new SelectList(new[] {employees}, "Id", "Name", employeeSelectedID);
            var locations = db.Locations.Where(a => a.DeviceNo == 0).OrderBy(a => a.Floor);
            ViewBag.LocationList = new SelectList(locations, "Id", "Floor", locationSelectedID);

            ViewBag.dateTime = Utility.GetDateTimeNow().ToString("dd/MM/yyyy HH:mm");
            ViewBag.inOutMode = "in";
            if (attendance != null)
            {
                ViewBag.dateTime = string.Format("{0}/{1}/{2} {3}:{4}", attendance.Day, attendance.Month, attendance.Year,
                    attendance.Hour, attendance.Minute);
                ViewBag.inOutMode = attendance.InOutMode;
            }
            return View();
        }

        //
        // POST: /OnSite/Create

        [HttpPost]
        public ActionResult OnSiteAttendance(int? OnSiteAttendanceID, int EmployeeId, int LocationId, string InOutMode, DateTime AttendanceDate)
        {
            var onsiteEntry = db.EmployeesOnSite.FirstOrDefault(a => a.FromDate <= AttendanceDate
                && a.ToDate >= AttendanceDate && a.EmployeeId == EmployeeId);
            if (onsiteEntry == null)
            {
                ViewBag.ErrorMessage = string.Format("You are not assigned as On Site for today : {0}", AttendanceDate);
                var employees = EmployeeRepository.GetEmployee(EmployeeId);
                ViewBag.EmployeeList = new SelectList(new[] {employees}, "Id", "Name", "Id");
                var locations = db.Locations.Where(a => a.DeviceNo == 0).OrderBy(a => a.Floor);
                ViewBag.LocationList = new SelectList(locations, "Id", "Floor", "Id");
                return View();
            }

            db = new Context();

            EmployeeEnrollment emp = EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(EmployeeId);
            Location location = db.Locations.Find(LocationId);
            int logedInUserId = int.Parse(Session["EmployeeId"].ToString());
            EmployeeEnrollment loogedEmployee = db.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == logedInUserId);

            var attendace = new Attendance();
            if (OnSiteAttendanceID != null)
                attendace = db.Attendances.FirstOrDefault(a => a.Id == OnSiteAttendanceID.Value);
            attendace.Day = AttendanceDate.Day;
			//attendace.Employee = emp;
            attendace.EmployeeId = emp.EmployeeId;
            attendace.CardNo = loogedEmployee.CardNo;
            attendace.Hour = AttendanceDate.Hour;
            attendace.InOutMode = InOutMode;
            attendace.Location = location;
            attendace.LocationId = location.Id;
            attendace.Minute = AttendanceDate.Minute;
            attendace.Month = AttendanceDate.Month;
            attendace.Second = AttendanceDate.Second;
            attendace.Year = AttendanceDate.Year;
            if (OnSiteAttendanceID == null)
                db.Attendances.Add(attendace);

            db.SaveChanges();

            ViewBag.OnSiteSuccessMessage = "Attendance record updated successfully.";

            #region --- Regenerate TODO Need to remove ---

            var employeesToView = EmployeeRepository.GetEmployee(loogedEmployee.EmployeeId);
            ViewBag.EmployeeList = new SelectList(new[] {employeesToView}, "Id", "Name", null);
            var locationsToView = db.Locations.Where(a => a.DeviceNo == 0).OrderBy(a => a.Floor);
            ViewBag.LocationList = new SelectList(locationsToView, "Id", "Floor", null);
            ViewBag.dateTime = Utility.GetDateTimeNow().ToString("dd/MM/yyyy HH:mm");
            ViewBag.inOutMode = "in";
            
            #endregion

            return View();
        }

        //
        // GET: /OnSite/Edit/5

        public ActionResult Edit(int id)
        {
            ViewBag.Message = "OnSite";
            EmployeeOnSite onsite = db.EmployeesOnSite.Find(id);
            ViewBag.EmployeeId = new SelectList(EmployeeRepository.GetAllEmployeeOrderByName(), "Id", "Name", onsite.EmployeeId);
            ViewBag.LocationId = new SelectList(db.Locations.Where(a => a.DeviceNo == 0).OrderBy(a => a.Floor).ToList(), "Id", "Floor", onsite.LocationId);
            return View(onsite);
        }


        //
        // POST: /OnSite/Edit/5

        [HttpPost]
        public ActionResult Edit(EmployeeOnSite onsite)
        {
            if (ModelState.IsValid)
            {
                db.Entry(onsite).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.EmployeeId = new SelectList(EmployeeRepository.GetAllEmployeeOrderByName(), "Id", "Name", onsite.EmployeeId);
            return View(onsite);
        }

        //
        // GET: /OnSite/Delete/5

        public ActionResult Delete(int id)
        {
            ViewBag.Message = "OnSite";
            EmployeeOnSite onsite = db.EmployeesOnSite.Find(id);
            ViewBag.employeName = EmployeeRepository.GetEmployee(onsite.EmployeeId).Name;
            return View(onsite);
        }

        public ActionResult DeleteOnSiteAttendance(int id)
        {
            ViewBag.Message = "OnSite";
            var attendance = db.Attendances.FirstOrDefault(a => a.Id == id);
            db.Attendances.Remove(attendance);
            db.SaveChanges();
            return RedirectToAction("OnSiteAttendanceList");
        }

        //
        // POST: /OnSite/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            EmployeeOnSite onsite = db.EmployeesOnSite.Find(id);
            db.EmployeesOnSite.Remove(onsite);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing) 
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        
        [HttpPost]
        public JsonResult GetEmployeeOnSiteAttendanceEntries()
        {
            return Json(Repositories.OnSiteRepository.GetAllEmployeeAttendanceList());
        }

        [HttpPost]
        public JsonResult DeleteOnSiteAttendanceEntry(int Id)
        {
            var attendance = db.Attendances.FirstOrDefault(a => a.Id == Id);
            db.Attendances.Remove(attendance);
            db.SaveChanges();

            return Json(new { status = "Success" });
        }
    }
}
