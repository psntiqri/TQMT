
using System;
using System.ComponentModel.DataAnnotations;

namespace Exilesoft.Models
{
    public class SpecialEvent
    {
        //public long Id { get; set; }

        [Display(Name = "Event Name")]
        public string EventName { get; set; }
        [Display(Name = "Description")]
        public string Description { get; set; }
        [Display(Name = "From Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public DateTime? EventFromDate { get; set; }
        [Display(Name = "To Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public DateTime? EventToDate { get; set; }
        public int Id { get; set; }
    }
}