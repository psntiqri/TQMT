using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.Areas.Reception.ViewModels
{
    public class  EmployeeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EmployeeCardAccessLevel { get; set; }
        public string VisitorCardsNotReturned { get; set; }
        public int NewVisitorCard { get; set; }
        public string ImagePath { get; set; }
        public bool IsUpdate { get; set; }
        public int VisitorPassAllocationId { get; set; }
    }
}