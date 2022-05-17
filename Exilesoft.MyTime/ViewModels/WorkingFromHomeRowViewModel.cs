using Exilesoft.Models;
using Exilesoft.MyTime.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels
{
    public class WorkingFromHomeRowViewModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int CardNo { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int InHour { get; set; }
        public int InMinute { get; set; }
        public int InSecond { get; set; }
        public int OutHour { get; set; }
        public int OutMinute { get; set; }
        public int OutSecond { get; set; }
        public int ApproverId { get; set; }
        public int ApproveType { get; set; }
        public string ApproverName { get; set; }
        public string ApproverImgUrl { get; set; }
        public string Description { get; set; }
        public string TaskList { get; set; }

        public WorkingFromHomeRowViewModel()
        {
        }
        
    }
 


}