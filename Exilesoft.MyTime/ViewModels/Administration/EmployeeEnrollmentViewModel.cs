using System.Collections.Generic;

namespace Exilesoft.MyTime.ViewModels.Administration
{
    public class EmployeeEnrollmentViewModel
    {
        public int EmployeeId { get; set; }
        public int CardNo { get; set; }
        public string UserName { get; set; }
        public int Privillage { get; set; }
        public bool IsEnable { get; set; }

        //public List<EmployeeViewModel> Employees { get; set; }
        public List<PrivilegeViewModel> Privileges { get; set; }
    }
}