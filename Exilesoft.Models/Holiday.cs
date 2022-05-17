using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{
    /// <summary>
    /// Holidays for the planned hours calculation
    /// </summary>
    public class Holiday
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public HolidayType Type { get; set; }
        public string Reason { get; set; }
    }

    public enum HolidayType
    {
        FullDay, HalfDay
    }
}