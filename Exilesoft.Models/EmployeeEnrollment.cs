using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{
    public class EmployeeEnrollment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EmployeeId { get; set; }
        public int CardNo { get; set; }
        public string UserName { get; set; }
        public Guid MobileId { get; set; }
        public int Privillage { get; set; }
        public bool IsEnable { get; set; }
    }
}