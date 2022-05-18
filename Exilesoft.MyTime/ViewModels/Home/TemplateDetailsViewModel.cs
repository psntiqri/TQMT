using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels.Home
{
    public class TemplateDetailsViewModel
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public string SupervisorName { get; set; }
        public int EmployeeId { get; set; }
        public int SupervisorId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public List<string> TaskList { get; set; }
        public string Description { get; set; }
    }
}