using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels
{
    public class OnSiteEmployeeViewModel
    {
        public string EditLink { get; set; }
        public string DeleteLink { get; set; }
        public string Employee { get; set; }
		public string FromDate { get; set; }
		public string ToDate { get; set; }
        public string Time { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public string MobileNumber { get; set; }
        public string IsPermanat { get; set; }

    }
}