using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels
{
    public class TeamManagementViewModel
    {
        public string TeamName { get; set; }
        public TeamMember[] TeamMembers { get; set; }
        
          public SharedEmployee[] SharedEmployeeList { get; set; }
    }
    
    public class TeamMember
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}