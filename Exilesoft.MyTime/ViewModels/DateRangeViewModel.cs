using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels
{
    public class DateRangeViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public DateRangeViewModel()
            : this(null, null)
        {

        }

        public DateRangeViewModel(DateTime? fromDate, DateTime? toDate)
        {
            this.FromDate = fromDate != null ? fromDate.GetValueOrDefault() : System.DateTime.Today.AddMonths(-1);
            this.ToDate = toDate != null ? toDate.GetValueOrDefault() : System.DateTime.Today;
        }
    }
}