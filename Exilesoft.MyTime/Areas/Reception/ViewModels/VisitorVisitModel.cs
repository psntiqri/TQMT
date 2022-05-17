using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.Areas.Reception.ViewModels
{
    public class VisitorVisitModel
    {
	    public int Id { get; set; }
	    public int VisitorId { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public string IdentificationNo { get; set; }
        public string Company { get; set; }
        public int? EmployeeId { get; set; }
        
        public int VisitId { get; set; }
        public int CardId { get; set; }
        public string VisitPurpose { get; set; }
        
        public string Description { get; set; }
        public string Status { get; set; }
        public int? CardAccessLevelId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? AppointmentTime { get; set; }
        public DateTime AssignDate { get; set; }
        public string Error { get; set; }
        public bool IsVisitorView { get; set; }
        public bool IsUpdate { get; set; }
        public string Location { get; set; }

    }
}