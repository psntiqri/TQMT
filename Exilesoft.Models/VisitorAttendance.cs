using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{
    public class VisitorAttendance
    {
        public VisitorAttendance()
        {
            isTransferred = false;
        }
        public int Id { get; set; }
        public int CardNo { get; set; }
        public DateTime DateTime { get; set; }
        public int VerifyMode { get; set; }
        public string InOutMode { get; set; }
        public int WorkCode { get; set; }
        [ForeignKey("LocationId")]
        public virtual Location Location { get; set; }
        public int LocationId { get; set; }
        public bool isTransferred { get; set; }
        
        public EmployeeEnrollment TransferredBy { get; set; }
        public DateTime? TransferredDate { get; set; }
        public string Note { get; set; }

        public int? VisitorPassAllocationId { get; set; }

        public int? VisitInformationId { get; set; }
        [ForeignKey("VisitInformationId")]
        public VisitInformation VisitInformation { get; set; }

        public int? EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public EmployeeEnrollment EmployeeEnrollment { get; set; }

        // [ForeignKey("VisitorPassAllocationId")]
        //public virtual VisitorPassAllocation VisitorPassAllocation { get; set; }
    }
}