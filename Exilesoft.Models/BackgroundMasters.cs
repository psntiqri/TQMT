

using System;

namespace Exilesoft.Models
{
    public class BackgroundMasters
    {
        public long Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }

        public decimal CompanyCoverage { get; set; }

        public decimal AchievedCoverage { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
