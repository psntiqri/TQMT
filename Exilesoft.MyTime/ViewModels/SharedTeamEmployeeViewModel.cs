using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels
{
    public class SharedTeamEmployeeViewModel
    {
        public int TeamId { get; set; }

        public string TeamName { get; set; }

        public SharedEmployee[] SharedEmployeeList { get; set; }
    }

    public class SharedEmployee
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}