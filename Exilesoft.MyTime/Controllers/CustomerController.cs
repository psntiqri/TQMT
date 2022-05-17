using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Exilesoft.MyTime.Filters;

namespace Exilesoft.MyTime.Controllers
{
    public class CustomerController : BaseController
    {
        //
        // GET: /Customer/

        public ActionResult Index()
        {
            return View();
        }

        [DelphiAuthentication("Customer")]
        public ActionResult TeamAnalysisReport(int? employeeId, string fromDate, string toDate)
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
    }
}
