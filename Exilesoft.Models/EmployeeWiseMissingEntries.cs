using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exilesoft.Models
{
    public class EmployeeWiseMissingEntries
    {
        public string employeeId;
        public string employeeName;
        public IList<DateTime> missingDates;
    }
}
