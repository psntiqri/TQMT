using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exilesoft.Models
{
    public class LeaveApprover
    {
        public int userId { get; set; }
        public string email { get; set; }
        public int roleId { get; set; }
        public string RoleName { get; set; }
    }
}
