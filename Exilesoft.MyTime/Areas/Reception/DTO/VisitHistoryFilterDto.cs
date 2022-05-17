using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.Areas.Reception.DTO
{
    public class VisitHistoryFilterDto
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }

        public DateTime? FromFilterDate
        {
            get
            {
                return string.IsNullOrEmpty(FromDate)
                           ? DateTime.MinValue
                           : DateTime.ParseExact(FromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
        }

        public DateTime? ToFilterDate
        {
            get
            {
                return string.IsNullOrEmpty(ToDate)
                           ? DateTime.MaxValue
                           : DateTime.ParseExact(ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).AddDays(1);
            }
        }
    }
}