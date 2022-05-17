using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exilesoft.Models
{
    public class MessageLog
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public int? VisitInformationId { get; set; }
    }
}
