using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exilesoft.Models
{
    public class EmployeeMissingEntry
    {
        [Key, Column(Order = 0)]
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        [Key, Column(Order = 1)]
        public DateTime MissingDate { get; set; }
    }
}
