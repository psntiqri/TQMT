using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels
{
    public class EmployeeCoverageRowViewModel
    {


        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal Coverage { get; set; }
        public decimal? WFHPercentage { get; set; }
        public DateTime TimeStamp { get; set; }
        public string ImageUrl { get; set; }
        public string MonthName { get; set; }

        public EmployeeCoverageRowViewModel()
        {


        }
    }
}