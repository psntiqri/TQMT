using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{
    public class VisitorPassAllocation
    {
        //public VisitorPassAllocation()
        //{
        //    this.VisitorAttendances = new HashSet<VisitorAttendance>();
        //}

        public int Id { get; set; }
        [ForeignKey("EmployeeId")]
        public virtual EmployeeEnrollment EmployeeEnrollment { get; set; }
		public virtual int? EmployeeId { get; set; }
        public int CardNo { get; set; }
        public DateTime AssignDate { get; set; }
	    public DateTime? DeallocateDate { get; set; }
	    public bool IsActive { get; set; }
        public string Notes { get; set; }
        //public int? EntityId { get; set; }
        //public int? EntityType { get; set; }
        public bool IsCardReturned { get; set; }
        public int? CardIssuedBy { get; set; }

		public virtual int? VisitInformationId { get; set; }
        [ForeignKey("VisitInformationId")]
        public virtual VisitInformation VisitInformation { get; set; }

        //[ForeignKey("CardNo")]
        //public virtual Card Card { get; set; }
        
        //public virtual ICollection<VisitorAttendance> VisitorAttendances { get; set; }
    }

    public enum VisitorPassAllocationTypes
    {
        Employee = 1,
        VisitInformation = 2
    }
}