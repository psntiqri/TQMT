// -------------DEVELOPER COMMENT---------------------------//
//
// Filename     : EmployeeRepository.cs
// Created By   : Harinda Dias
// Date         : 2013-Jul-04, Thu
// Description  : Repository for the On Site view 

//
// Modified By  : 
// Date         : 
// Purpose      : 
//
// ---------------------------------------------------------//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Exilesoft.Models;

namespace Exilesoft.MyTime.Repositories
{
    /// <summary>
    /// Repository for the onsite view
    /// </summary>
    public class OnSiteRepository
    {
        private static Context dbContext = new Context();
        /// <summary>
        /// Gets the employee onsite entry list to the client
        /// Converted to the user view
        /// </summary>
        /// <returns>OnSite attendance List for the gridview</returns>
        internal static List<ViewModels.OnSiteAttendanceViewModel> GetAllEmployeeAttendanceList()
        {
            dbContext = new Context();
            //var employeeList = EmployeeRepository.GetAllEmployees(); //dbContext.Employees.ToList();
            int logedInUserId = int.Parse(System.Web.HttpContext.Current.Session["EmployeeId"].ToString());
            EmployeeEnrollment loggedUser = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == logedInUserId);
            //Employee loggedUser = dbContext.Employees.FirstOrDefault(a => a.Username == HttpContext.Current.User.Identity.Name);
            DateTime entriesBeforeDate = System.DateTime.Today.AddMonths(-1);
            var locationList = dbContext.Locations.ToList();
            var onsites = dbContext.Attendances.Where(a => a.EmployeeId == loggedUser.EmployeeId && a.Location.OnSiteLocation);
            var attendanceList = onsites.ToList().Where(a => new DateTime(a.Year, a.Month, a.Day) > entriesBeforeDate);


            List<ViewModels.OnSiteAttendanceViewModel> attendanceViewLogList = new List<ViewModels.OnSiteAttendanceViewModel>();
            // Query for the latest 1000 records for the selection to the client
            foreach (var attendance in attendanceList)
            {
                var attendancelocation = locationList.FirstOrDefault(a => a.Id == attendance.LocationId);
                string fullName = EmployeeRepository.GetEmployee(attendance.EmployeeId).Name;

                // Creating the employee viewmodels for the client
                attendanceViewLogList.Add(new ViewModels.OnSiteAttendanceViewModel()
                {
                    EditLink = string.Format("<a href=\"/OnSite/OnSiteAttendance?id={0}\" class=\"CNewOnSiteAtte\">Edit</a>", attendance.Id.ToString()),
                    DeleteLink = string.Format("<a href=\"javascript:new OnSiteForm().DeleteAttendance({0});\">Delete</a>", attendance.Id.ToString()),
                    Employee = fullName,
                    Date = string.Format("{0}/{1}/{2}", attendance.Day.ToString("00"), attendance.Month.ToString("00"), attendance.Year),
                    Time = string.Format("{0}:{1}", attendance.Hour.ToString("00"), attendance.Minute.ToString("00")),
                    Location = attendancelocation != null ? attendancelocation.Floor : string.Empty,
                    Status = attendance.InOutMode
                });
            }

            return attendanceViewLogList;
        }

        internal static List<EmployeeOnSite> GetOnsiteEmployeesByDateRange(DateTime fromDate, DateTime toDate)
        {
            List<EmployeeOnSite> onsiteEmployees = dbContext.EmployeesOnSite.Where(a => (a.ToDate >= fromDate) && (a.FromDate <= toDate)).ToList();

            return onsiteEmployees;
        }

        internal static List<EmployeeOnSite> GetOnsiteEmployees(DateTime Date)        {
            List<EmployeeOnSite> onsiteEmployees = dbContext.EmployeesOnSite.Where(a =>(a.EmployeeEnrollment.IsEnable==true) && (a.FromDate <= Date)&&(a.ToDate >= Date) ).ToList();
            return onsiteEmployees;
        }
    }
}