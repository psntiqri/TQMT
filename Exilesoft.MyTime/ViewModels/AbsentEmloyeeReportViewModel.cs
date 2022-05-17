using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels
{
	public class AbsentEmloyeeReportViewModel
	{

		public AbsentEmloyeeReportViewModel()
		{
			this.AbsentEmployeeList = new List<AbsentEmloyeeViewModel>();
            
		}

		public List<AbsentEmloyeeViewModel> AbsentEmployeeList { get; set; }
	}

	public class AbsentEmloyeeViewModel
		{
			public string EmployeeName { get; set; }
			public string EmployeeID { get; set; }
			public string AbsentDate { get; set; }
		}
    public class WorkingFromHomeEmloyeeViewModel
    {
        public string EmployeeName { get; set; }
        public string EmployeeID { get; set; }
        public string Status { get; set; }
        public string AbsentDate { get; set; }
        public List<WorkingFromHomeEmloyeeViewModel> WokingFromHomeEmployeeList { get; set; }
        public WorkingFromHomeEmloyeeViewModel()
		{
            this.WokingFromHomeEmployeeList = new List<WorkingFromHomeEmloyeeViewModel>();
            
		}
        
    }    
}
