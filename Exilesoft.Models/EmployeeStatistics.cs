using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{
    public class EmployeeStatistics
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string FirstIn { get; set; }
        public string LastOut { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        public TimeSpan TimeInOffice { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        public TimeSpan TimeInside { get; set; }
        public Location Location { get; set; }
        public bool ErrorFlag { get; set; }

        public string TimeInOfficeString { get; set; }
        public string TimeInsideString { get; set; }
        public string LocationString { get; set; }
        public string EmployeeImagePath { get; set; }
        [NotMapped]
        public string Email { get; set; }
        [NotMapped]
        public string MobileNo { get; set; }
    }
}