using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels
{
	public class MyLeaveViewModel
	{
		public string user { get; set; }
		public List<MyLeaveListEntry> Leaves { get; set; }
	}
	public class MyLeaveListEntry
	{
		public string date { get; set; }
		public string status { get; set; }
		public string LeaveType { get; set; }
	}
}