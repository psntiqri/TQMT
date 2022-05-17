using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{
    public class GlassCurrentStatusModel
    {
        public int InSideOffice { get; set; }
        public int OutOfOffice { get; set; }
        public int AtWork { get; set; }
        public int Absent { get; set; }
        public int OnSite { get; set; }
        public DateTime SelectedDate { get; set; }
        public string PictureAsAt { get; set; }
    }
}