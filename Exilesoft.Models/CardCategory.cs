using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Exilesoft.Models
{
    public class CardCategory
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }

    public enum CardCatetoryEnum
    {
        Employee = 1, Visitor = 2
    }
}
