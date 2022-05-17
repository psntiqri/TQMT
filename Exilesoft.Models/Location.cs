using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{
    public class Location
    {
        public int Id { get; set; }
        public int DeviceNo { get; set; }
        public string Floor { get; set; }
        public string DeviceIP { get; set; }
        public string DeviceName { get; set; }
        public int DeviceNumber { get; set; }
        public int Port { get; set; }
        public int CommKey { get; set; }
        public bool ClearLogsAutomatically { get; set; }
        public bool OnSiteLocation { get; set; }
    }
}