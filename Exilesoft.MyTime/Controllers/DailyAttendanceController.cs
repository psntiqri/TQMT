using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Exilesoft.Models;
using Exilesoft.MyTime.Filters;

namespace Exilesoft.MyTime.Controllers
{
    public class DailyAttendanceController : BaseController
    {
        private Context dbContext = new Context();
        /// <summary>
        /// Daily attendance graphical view UI
        /// </summary>
        /// <returns></returns>
        //[DelphiAuthentication("Admin", "Manager", "Top Manager")]
        public ActionResult Index(int? employeeId, string fromDate, string toDate)
        {
            ViewBag.Message = "Analysis";
            ViewModels.DailyAttendanceViewModel _model = new ViewModels.DailyAttendanceViewModel
                (employeeId, ParseDate(fromDate), ParseDate(toDate));
            return View(_model);
        }

        public DateTime? ParseDate(string dateStr)
        {
            DateTime dateTime;
            if (DateTime.TryParse(dateStr, CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out dateTime))
                return dateTime;

            return null;
        }

        /// <summary>
        /// Gets the search list of employees for the 
        /// employee auto complete
        /// </summary>
        /// <param name="searchText">Text to search with employee name</param>
        /// <returns>List of employees to display</returns>
        [HttpPost]
        public JsonResult SearchEmployees(string searchText)
        {
            return Json(new { SearchResult = Repositories.DailyAttendanceRepository.SearchEmployees(searchText) });
        }


        [HttpPost]
        public JsonResult SearchSharedEmployees(string searchText)
        {
            return Json(new { SearchResult = Repositories.DailyAttendanceRepository.SearchShareEmployees(searchText) });
        }

        /// <summary>
        /// Gets the graph data with in and out times for the selected list of employees.
        /// </summary>
        /// <param name="model">Model with the parameters from the UI</param>
        /// <returns>Json result with the formated data to the graph</returns>
        [HttpPost]
        public JsonResult GetEmployeesHorsGraphData(ViewModels.DailyAttendanceViewModel model)
        {
            return Json(new { AttendanceStructure = Repositories.DailyAttendanceRepository.GetEmployeesHorsGraphData(model) });
        }

        /// <summary>
        /// Gets the graph data with in and out times for the selected employee.
        /// </summary>
        /// <param name="model">Model with the parameters from the UI</param>
        /// <returns>Json result with the formated data to the graph</returns>
        [HttpPost]
        public JsonResult GetSelectedEmployeesHorsGraphData(ViewModels.DailyAttendanceViewModel model)
        {
            return Json(new { AttendanceStructure = Repositories.DailyAttendanceRepository.GetSelectedEmployeeHorsGraphData(model) });
        }

        /// <summary>
        /// Gets the summery report HTML to be displayed in the report
        /// </summary>
        /// <param name="model">Model with the parameters from the UI</param>
        /// <returns>Json result with the formated HTML report</returns>
        [HttpPost]
        public JsonResult GetEmployeesHorsSummeryData(ViewModels.DailyAttendanceViewModel model)
        {
            return Json(new { AttendanceReport = Repositories.DailyAttendanceRepository.GetEmployeesHorsSummeryData(model) });
        }

        [HttpPost]
        public JsonResult GetEmployeesTaskHoursSummeryData(ViewModels.DailyAttendanceViewModel model)
        {
            return Json(new { AttendanceReport = Repositories.DailyAttendanceRepository.GetEmployeesTaskHoursSummeryData(model) });
        }


        [HttpPost]
        public JsonResult GetLateEmployeesTableData(ViewModels.LateEmployeesModel model)
        {
            return Json(new { LateEmployeesTable = Repositories.DailyAttendanceRepository.GetLateEmployeesQuery(model) });
        }

        /// <summary>
        /// Gets the graph data with in and out times for the selected employee.
        /// </summary>
        /// <param name="model">Model with the parameters from the UI</param>
        /// <returns>Json result with the formated data to the graph</returns>
        [HttpPost]
        public JsonResult GetSelectedEmployeesHorsGraphData2(ViewModels.LateEmployeesModel model)
        {
            return Json(new { LateEmployeesTable = Repositories.DailyAttendanceRepository.GetLateEmployeesQuery(model) });
        }

        /// <summary>
        /// Create team 
        /// </summary>
        /// <param name="teamModel">team members id list, team name</param>
        /// <returns>message containing success or failure of creation the team</returns>
        [HttpPost]
        //[DelphiAuthentication]
        public JsonResult AddTeamToDb(ViewModels.TeamManagementViewModel teamModel)
        {
            var employeeId = int.Parse(Session["EmployeeId"].ToString());
            EmployeeEnrollment loggedUser = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == employeeId);
            string loggedUsername = loggedUser.UserName;
            return Json(new { Msg = Repositories.TeamManagementRepository.CreateTeam(teamModel, loggedUsername) });
        }

        /// <summary>
        /// Create team 
        /// </summary>
        /// <param name="teamId">team members id list, team name</param>
        /// <returns>message containing success or failure of creation the team</returns>
        [HttpPost]
        //[DelphiAuthentication]
        public JsonResult TeamUpdate(int teamId, string memberString)
        {
            //var employeeId = int.Parse(HttpRuntime.Cache.Get("EmployeeID").ToString());
            //EmployeeEnrollment loggedUser = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == employeeId);
            //string loggedUsername = loggedUser.UserName;
            return Json(new { Msg = Repositories.TeamManagementRepository.UpdateTeam(teamId, memberString) });
        }



        /// <summary>
        /// Create team 
        /// </summary>
        /// <param name="teamId">team members id list, team name</param>
        /// <returns>message containing success or failure of creation the team</returns>
        [HttpPost]
        public JsonResult TeamDelete(int teamId)
        {
            return Json(new { Msg = Repositories.TeamManagementRepository.TeamDelete(teamId) });
        }

        [HttpPost]
        public JsonResult ShareTeam(ViewModels.SharedTeamEmployeeViewModel sharedTeamEmployeeViewModel)
        {
            string loggedUsername = User.Identity.Name;
            return Json(new { Msg = Repositories.TeamManagementRepository.UpdateTeamSharedMembers(sharedTeamEmployeeViewModel.TeamId, sharedTeamEmployeeViewModel) });
        }

        [HttpPost]
        //[DelphiAuthentication]
        public JsonResult GetTeamDropdownHtml()
        {
            var employeeId = int.Parse(Session["EmployeeId"].ToString());
            EmployeeEnrollment loggedUser = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == employeeId);
            string loggedUsername = loggedUser.UserName;
            //string loggedUsername = User.Identity.Name;
            return Json(new { Html = Repositories.TeamManagementRepository.TeamDropDownHtml(loggedUsername) });
        }

        //[HttpPost]
        //[DelphiAuthentication]
        public JsonResult GetTeamMembersDetails(int teamId)
        {
            return Json(new { TeamMembers = Repositories.TeamManagementRepository.GetTeamMembersDetails(teamId) });
        }

        [HttpPost]
        public JsonResult GetSharedMemberDetails(int teamId)
        {
            return Json(new { TeamMembers = Repositories.TeamManagementRepository.GetSharedMemberDetails(teamId) });
        }
    }
}
