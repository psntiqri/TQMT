using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Exilesoft.Models;
using Exilesoft.MyTime.Filters;
using Exilesoft.MyTime.Helpers;


namespace Exilesoft.MyTime.Controllers
{
    public class AnalysisController : BaseController
    {
        private Context _dbContext = new Context();

        // GET: /Chart/
       // [Authorize]
		//[DelphiAuthentication]
        public ActionResult Index()
        {
            ViewBag.Message = "Analysis";
            return View();
        }

       // [Authorize]
		//[DelphiAuthentication]
        public ActionResult TimeTrendAnalysis(int? employeeId, string fromDate, string toDate)
        {
			//EmployeeEnrollment loggedUser = _dbContext.EmployeeEnrollment.FirstOrDefault(a => a.UserName == User.Identity.Name);

			var loggedEmployeeId = int.Parse(Session["EmployeeId"].ToString());
			EmployeeEnrollment loggedUser = _dbContext.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == loggedEmployeeId);
            ViewModels.TimeTrendAnalysisViewModel _model = new ViewModels.TimeTrendAnalysisViewModel(loggedUser, ParseDate(fromDate), ParseDate(toDate));
            return View(_model);
        }

        //[Authorize]
		//[DelphiAuthentication]
        public ActionResult PlannedVsActualAnalysis(string date, string timeFrom, string timeTo)
        {
            EmployeeEnrollment loggedUser = _dbContext.EmployeeEnrollment.FirstOrDefault(a => a.UserName == User.Identity.Name);
            ViewModels.TimeTrendAnalysisViewModel _model = new ViewModels.TimeTrendAnalysisViewModel(loggedUser, null, null);
            return View(_model);
        }

       // [Authorize]
		//[DelphiAuthentication]
        public ActionResult EmployeesAtTime()
        {
           // ViewBag.Priv = _dbContext.EmployeeEnrollment.FirstOrDefault(a => a.UserName == User.Identity.Name).Privillage;
            ViewBag.Date = Utility.GetDateTimeNow().Date;
            ViewBag.Time = Utility.GetDateTimeNow();

            ViewModels.LateEmployeesModel lateModel = new ViewModels.LateEmployeesModel(null);

            return View(lateModel);
        }

        public DateTime? ParseDate(string dateStr)
        {
            DateTime dateTime;
            if (DateTime.TryParse(dateStr, CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out dateTime))
                return dateTime;

            return null;
        }

        //[HttpPost]
		//[DelphiAuthentication]
        public JsonResult GetEmployeeTimeTrendGraphData(ViewModels.TimeTrendAnalysisViewModel model)
        {
            return Json(new { AttendanceStructure = Repositories.TimeTrendRepository.GetEmployeesInOutGraphData(model) });
        }

		
    }
}
