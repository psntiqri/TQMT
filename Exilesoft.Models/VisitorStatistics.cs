using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{
    public class VisitorStatistics
    {


        public int VisitorId { get; set; }
        public string VisitorRepresentation { get; set; }
        public string Date { get; set; }
        public string FirstIn { get; set; }
        public string LastOut { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        public TimeSpan TimeInOffice { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        public TimeSpan TimeInside { get; set; }
        public Location Location { get; set; }
        public bool ErrorFlag { get; set; }
    }
}