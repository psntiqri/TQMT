

using System;

namespace Exilesoft.Models
{
    public class BackgroundSlaves
    {

        

        public long Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int EmployeeId { get; set; }
        public decimal Coverage { get; set; }
        public decimal? WFHPercentage { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
