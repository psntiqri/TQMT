using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels
{
    public class DashBoardViewModel
    {
        public DashBoardViewModel()
        {
            this.InsideEmployeeList = new List<DashBoardEmployeeViewModel>();
            this.OutOfOfficeEmployeeList = new List<DashBoardEmployeeViewModel>();
            this.OnSiteEmployeeList = new List<DashBoardEmployeeViewModel>();
            this.AbsentEmployeeList = new List<DashBoardEmployeeViewModel>();
            this.AtWorkEmployeeList = new List<DashBoardEmployeeViewModel>();
        }

        public List<DashBoardEmployeeViewModel> InsideEmployeeList { get; set; }
        public List<DashBoardEmployeeViewModel> OutOfOfficeEmployeeList { get; set; }
        public List<DashBoardEmployeeViewModel> OnSiteEmployeeList { get; set; }
        public List<DashBoardEmployeeViewModel> AbsentEmployeeList { get; set; }
        public List<DashBoardEmployeeViewModel> AtWorkEmployeeList { get; set; }
    }

    public class DashBoardEmployeeViewModel
    {
        public string DisplayText { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeID { get; set; }
    }
}