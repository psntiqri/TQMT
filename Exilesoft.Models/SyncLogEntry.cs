using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{
    public class SyncLogEntry
    {
        public int Id { get; set; }

        [ForeignKey("SyncSessionLogId")]
        public SyncSessionLog SyncSessionLog { get; set; }
        public int SyncSessionLogId { get; set; }

        public int LocationId { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public DateTime SyncTime { get; set; }
        public string CardNo { get; set; }
        public int EmployeeId { get; set; }
    }
}