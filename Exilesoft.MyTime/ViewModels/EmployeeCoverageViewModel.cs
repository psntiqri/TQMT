using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels
{
    public class EmployeeCoverageViewModel
    {

        public int TabCount { get; set; }

        public IList<EmployeeCoverageRowViewModel> employeeCoverageRowList { get; set; }

        public EmployeeCoverageViewModel()
        {
        }

    }
}