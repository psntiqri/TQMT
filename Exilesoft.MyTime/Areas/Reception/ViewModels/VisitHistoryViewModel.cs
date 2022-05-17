using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.Areas.Reception.ViewModels
{
    public class VisitHistoryViewModel
    {
        public int Id { get; set; }
        public string VisitorName { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Company { get; set; }
        public string IdentificationNumber { get; set; }
        public string VisitorOfEmployee { get; set; }
        public int CardId { get; set; }
        public DateTime DateAssigned { get; set; }
        public DateTime DateReturned { get; set; }
        public string VisitorType { get; set; }
        public int? EmployeeId { get; set; }
		public string MobileNo { get; set; }
		public string VisitPurpose { get; set; }
		public DateTime? DeallocateDate { get; set; }
		public bool IsActive { get; set; }
    }

    public enum VisitorType
    {
        Employee = 1,
        Visitor = 2
    }
}