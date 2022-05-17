using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{

    public class EmployeeData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public DateTime DateJoined { get; set; }
        public byte[] Image { get; set; }
        public string MobileNumber { get; set; }
        public string PrimaryEmailAddress { get; set; }
        public DateTime? DateResigned { get; set; }
        public bool IsEnable { get; set; }
        public string Region { get; set; }
        
    }
}