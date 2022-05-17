using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Exilesoft.Models;
using Exilesoft.MyTime.Controllers;
using Exilesoft.MyTime.Repositories;

namespace Exilesoft.MyTime.ViewModels
{
    public class TimeTrendAnalysisViewModel
    {
        private Context _dbContext = new Context();
		HomeController _homeController = new HomeController();

		public TimeTrendAnalysisViewModel()
        {

        }

		public TimeTrendAnalysisViewModel(EmployeeEnrollment employeeEnrollment, DateTime? fromDate, DateTime? toDate)
        {
			this.FromDate = fromDate != null ? fromDate : System.DateTime.Today.AddMonths(-1).AddDays(-1);
            this.ToDate = toDate != null ? toDate : System.DateTime.Today.AddDays(-1);

			var CrrentloggedUser = System.Web.HttpContext.Current.User;

            var CrrentLoggedEmployeeId = int.Parse(System.Web.HttpContext.Current.Session["EmployeeId"].ToString());

            //var CrrentLoggedEmployeeId = _homeController.EmployeeId;

			
			this.EmployeeSelectionList = new SelectList(new[] { EmployeeRepository.GetEmployee(CrrentLoggedEmployeeId) }, "Id", "Name", "Id");

			//if (employeeEnrollment.Privillage == 0)

			//if ((CrrentloggedUser.IsInRole("Employee")))
              //  this.EmployeeSelectionList = new SelectList(new[] {EmployeeRepository.GetEmployee(employeeEnrollment.EmployeeId)}, "Id", "Name", "Id");
				//this.EmployeeSelectionList = new SelectList(new[] { EmployeeRepository.GetEmployee(CrrentLoggedEmployeeId) }, "Id", "Name", "Id");
    //        else
    //            this.EmployeeSelectionLicst = new SelectList(EmployeeRepository.GetAllEmployeeOrderByName().Where(e=>e.IsEnable), "Id", "Name", "Id");

			//this.Privladge = employeeEnrollment.Privillage;
			//this.SelectedEmployeeID = employeeEnrollment.EmployeeId;
	        if (CrrentloggedUser.IsInRole("Administrator"))
	        {
				this.Privladge = "Admin";
	        }
			if (CrrentloggedUser.IsInRole("Employee"))
			{
				this.Privladge = "Employee";
			}
			if (CrrentloggedUser.IsInRole("SystemAdmin"))
			{
				this.Privladge = "SystemAdmin";
			}
			if (CrrentloggedUser.IsInRole("Manager"))
			{
				this.Privladge = "Manager";
			} if (CrrentloggedUser.IsInRole("TopManager"))
			{
				this.Privladge = "TopManager";
			}
			this.SelectedEmployeeID = CrrentLoggedEmployeeId;
        }

        public string ReportType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool ViewInTime { get; set; }
        public bool ViewOutTime { get; set; }
        public int SelectedEmployeeID { get; set; }
        public IEnumerable<SelectListItem> EmployeeSelectionList { get; set; }
		//public int Privladge { get; set; }
		public string Privladge { get; set; }
    }
}