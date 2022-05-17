using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{
    public class UserTeam
    {
        public int Id { get; set; }
        public string TeamName { get; set; }
        public EmployeeEnrollment CreatedBy { get; set; }
        public string TeamMembersIdString { get; set; }
        public string TeamSharedEmpIdString { get; set; }
        public DateTime DateCreated { get; set; }
	   
    }
}