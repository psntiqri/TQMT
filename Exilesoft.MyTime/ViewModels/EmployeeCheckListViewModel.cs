using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Exilesoft.Models;
using Exilesoft.MyTime.Repositories;

namespace Exilesoft.MyTime.ViewModels
{
    public class EmployeeCheckListViewModel
    {
        public EmployeeCheckListViewModel()
        {

        }

        public EmployeeCheckListViewModel(int employeeID)
        {
            this.Employee = EmployeeRepository.GetEmployee(employeeID);
        }

        public EmployeeData Employee { get; set; }
        public string OnBoardCheckListRawTable { get; set; }
        public string ExitCheckListRawTable { get; set; }
    }
}