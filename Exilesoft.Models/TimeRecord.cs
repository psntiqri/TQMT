using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exilesoft.Models
{
    public class TimeRecord
    {
        public int Id { get; set; }
        public int CardNo { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public int VerifyMode { get; set; }
        public int InOutMode { get; set; }
        public int WorkCode { get; set; }
        public int LocationId { get; set; }
        //0 - not synced ; 1 - synced
        public int IsSynced { get; set; }
    }
}
