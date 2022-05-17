using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exilesoft.Models
{
    public class VisitInformation
    {
		public VisitInformation()
        {
			//this.VisitorPassAllocations = new HashSet<VisitorPassAllocation>();
        }
        public int Id { get; set; }
        public string VisitPurpose { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime? FomDate { get; set; }
        public DateTime? AppointmentTime { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CardAccessLevelId { get; set; }
        public int? EmployeeId { get; set; }
        public int? EnteredBy { get; set; }
        public int EntityId { get; set; }
        public int EntityType { get; set; }

        [ForeignKey("CardAccessLevelId")]
        public virtual CardAccessLevel CardAccessLevel { get; set; }

        [ForeignKey("EntityId")]
        public virtual Visitor Visitor { get; set; }


		//public virtual ICollection<VisitorPassAllocation> VisitorPassAllocations { get; set; }

    }
    public enum VisitInfoEntityTypeEnum
    {
        Visitor = 1, Customer = 2
    }

    public enum VisitStatusEnum
    { Pending , Allocated , Closed  }
}
