using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels
{
    public class SpecialEventListViewModel
    {
        public string EditLink { get; set; }
        public string DeleteLink { get; set; }
        public string AllocateLink { get; set; }
        public string CheckListLink { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string EventFromDate { get; set; }
        public string EventToDate { get; set; }
        public int Id { get; set; }
       
    }
}