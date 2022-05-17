using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Data;
using System.Net;
using System.Web.Script.Serialization;
using Exilesoft.Models;
using Exilesoft.MyTime.ViewModels;

namespace Exilesoft.MyTime.Repositories
{
    public class LeaveRepository
    {
        

		public static List<MyLeaveViewModel> GetMyLeaveService(DateTime fromDate, DateTime toDate)
		{
            string url = string.Format(ConfigurationManager.AppSettings["MyLeaveWebApi"], "leaveListGateWay") +
                            "?dateFrom={0}&dateTo={1}&appId=myTime";
            string finalUrl = string.Format(url, fromDate.ToString("MM/dd/yyyy"), toDate.ToString("MM/dd/yyyy"));

            string data = new WebClient().DownloadString(finalUrl);
            JavaScriptSerializer js = new JavaScriptSerializer();
            MyLeaveViewModel[] persons = js.Deserialize<MyLeaveViewModel[]>(data);
            return persons.ToList();
		}

        public static IList<Leave> GetLeave(DateTime fromDate, DateTime toDate, EmployeeEnrollment employee, IList<Holiday> holidayList,List<MyLeaveViewModel> employeeLeaves)
        {
            var myLeaveEmployeList = employeeLeaves;
			var employeeData = EmployeeRepository.GetEmployee(employee.EmployeeId);
			List<MyLeaveViewModel> leaveDetails = myLeaveEmployeList.Where(a => a.user == employeeData.PrimaryEmailAddress).ToList();

            IList<Leave> _leaveList = new List<Leave>();

			EmployeeEnrollment employeeEnrollmentData = EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(employee.EmployeeId);
            if (employee == null)
                return _leaveList;

			foreach (var leaves in leaveDetails)
			{
				
                foreach (var leave in leaves.Leaves)
				{
					Leave _leave = new Leave()
					{
						Date = Convert.ToDateTime(leave.date),
						Employee = employeeEnrollmentData,
						LeaveType = leave.LeaveType == "HALFDAY_NO" ? LeaveType.FullDay : LeaveType.HalfDay,
						Status = leave.status
					};
					_leaveList.Add(_leave);
				}
			}

            return _leaveList;
        }

        public static IList<Leave> GetApprovedLeave(DateTime fromDate, DateTime toDate, EmployeeEnrollment employee, IList<Holiday> holidayList, List<MyLeaveViewModel> employeeLeaves)
        {
            var myLeaveEmployeList = employeeLeaves;
            var employeeData = EmployeeRepository.GetEmployee(employee.EmployeeId);
            List<MyLeaveViewModel> leaveDetails = myLeaveEmployeList.Where(a => a.user == employeeData.PrimaryEmailAddress).ToList();

            IList<Leave> _leaveList = new List<Leave>();

            EmployeeEnrollment employeeEnrollmentData = EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(employee.EmployeeId);
            if (employee == null)
                return _leaveList;

            foreach (var leaves in leaveDetails)
            {
                var approvedLeaves = leaves.Leaves.Where(l => l.status == "Approved" || l.status == "Pending").ToList();
                foreach (var leave in approvedLeaves)
                {
                    Leave _leave = new Leave()
                    {
                        Date = Convert.ToDateTime(leave.date),
                        Employee = employeeEnrollmentData,
                        LeaveType = leave.LeaveType == "HALFDAY_NO" ? LeaveType.FullDay : LeaveType.HalfDay,
                        Status = leave.status
                    };
                    _leaveList.Add(_leave);
                }
            }

            return _leaveList;
        }
        public static IList<Leave> GetApprovedLeaveForRpt(DateTime fromDate, DateTime toDate)
        {
            var myLeaveEmployeList = GetMyLeaveService(fromDate, toDate);

            IList<Leave> _leaveList = new List<Leave>();

            if (myLeaveEmployeList == null)
                return _leaveList;

            foreach (var leaves in myLeaveEmployeList)
            {
                var approvedLeaves = leaves.Leaves.Where(l => l.status == "Approved" || l.status == "Pending").ToList();
                var employee = EmployeeEnrollmentRepository.GetEmployeeEnrollmentByUsername(leaves.user.Substring(0, 3));
                if (employee != null)
                {
                    foreach (var leave in approvedLeaves)
                    {
                        Leave _leave = new Leave()
                        {
                            Date = Convert.ToDateTime(leave.date),
                            Employee = employee,
                            LeaveType = leave.LeaveType == "HALFDAY_NO" ? LeaveType.FullDay : LeaveType.HalfDay,
                            Status = leave.status
                        };
                        _leaveList.Add(_leave);
                    }
                }
            }

            return _leaveList;
        }

        private static IList<Leave> FormatLeaveListFromMyLeaveService(IList<MyLeaveViewModel> leaveViewModelList, int employeeId, DateTime fromDate)
        {
            IList<Leave> leaveList = new List<Leave>();

            foreach (var myLeaveViewModel in leaveViewModelList)
            {
                
                foreach (var leave in myLeaveViewModel.Leaves)
                {
                    if (Convert.ToDateTime(leave.date).ToString("MM/dd/yyyy") != fromDate.ToString("MM/dd/yyyy"))
                        continue;

                    var leaveObj = new Leave()
                    {
                        Date = Convert.ToDateTime(leave.date),
                        LeaveType = leave.LeaveType == "HALFDAY_NO" ? LeaveType.FullDay : LeaveType.HalfDay,
                        Status = leave.status,
                        Employee = EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(employeeId)

                    };

                    if (leaveObj.Status != "Cancelled")
                        leaveList.Add(leaveObj);
                }
            }

            return leaveList;
        }

        public static IList<Leave> GetLeave(DateTime fromDate, DateTime toDate, IList<EmployeeEnrollment> employeeList, IList<Holiday> holidayList)
        {
            IList<Leave> _leaveList = new List<Leave>();

            var useLMSService = ConfigurationManager.AppSettings["UseLMSService"];
            if (useLMSService == "1")
                return _leaveList;

            string queryText = @"SELECT U.Username,[DateFrom]
                              ,[DateTo]
                              ,[AnnualDays]
                              ,[MedicalDays]
                              ,[CasualDays]
                              ,[NoPayDays]
                              ,[OtherDays]
                              ,[Reason]
                              ,[Status]
                              FROM [LeaveRegistry] L
                              Inner Join [UserTbl] U on U.UserId = L.UserId
                              where [Status] = 'Approved' AND [DateFrom] >= '{0}' AND [DateFrom] <='{1}'";

            string query = string.Format(queryText, fromDate.ToString("MM/dd/yyyy"), toDate.ToString("MM/dd/yyyy"));
            var _selectedEmployeeList = employeeList.Where(a => a.UserName != null);
           
            DataTable _leaveDT = Util.SqlHelper.ExecuteStatement(query);

            for (int i = 0; i < _leaveDT.Rows.Count; i++)
            {
                var _employee = _selectedEmployeeList.FirstOrDefault(e => e.UserName.ToUpper().Equals(_leaveDT.Rows[i][0].ToString().ToUpper()));
                if (_employee != null)
                {
                    decimal _totalLeaveCount = TryParseLeave(_leaveDT.Rows[i]["AnnualDays"].ToString()) +
                        TryParseLeave(_leaveDT.Rows[i]["MedicalDays"].ToString()) +
                        TryParseLeave(_leaveDT.Rows[i]["CasualDays"].ToString()) +
                        TryParseLeave(_leaveDT.Rows[i]["NoPayDays"].ToString()) +
                        TryParseLeave(_leaveDT.Rows[i]["OtherDays"].ToString());

                    DateTime? _leaveFromDate = TryParseDate(_leaveDT.Rows[i]["DateFrom"].ToString());
                    DateTime? _leaveToDate = TryParseDate(_leaveDT.Rows[i]["DateTo"].ToString());
                    string _deductedFrom = string.Empty;

                    #region --- Leave Deducted From  ---

                    if (TryParseLeave(_leaveDT.Rows[i]["MedicalDays"].ToString()) > 0)
                        _deductedFrom += "SICK LEAVE";
                    if (TryParseLeave(_leaveDT.Rows[i]["AnnualDays"].ToString()) > 0)
                    {
                        if (!_deductedFrom.Equals(string.Empty))
                            _deductedFrom += ",";
                        _deductedFrom += "ANNUAL LEAVE";
                    }
                    if (TryParseLeave(_leaveDT.Rows[i]["CasualDays"].ToString()) > 0)
                    {
                        if (!_deductedFrom.Equals(string.Empty))
                            _deductedFrom += ",";
                        _deductedFrom += "CASUAL LEAVE";
                    }
                    if (TryParseLeave(_leaveDT.Rows[i]["OtherDays"].ToString()) > 0)
                    {
                        if (!_deductedFrom.Equals(string.Empty))
                            _deductedFrom += ",";
                        _deductedFrom += "OTHER LEAVE";
                    }

                    #endregion                       

                    while (_leaveFromDate <= toDate)
                    {
                        if (_totalLeaveCount <= 0)
                            break;

                        if (HolidayRepository.IsHoliday(holidayList, _leaveFromDate.Value) == null)
                        {
                            #region --- Not Holidays consider as leave ---

                            Leave _leave = new Leave() { Date = _leaveFromDate.Value, Employee = _employee, LeaveType = LeaveType.FullDay };
                            if (_totalLeaveCount < 1)
                                _leave.LeaveType = LeaveType.HalfDay;

                            if (_leave.LeaveType == LeaveType.FullDay)
                                _totalLeaveCount -= 1;
                            else
                                _totalLeaveCount -= (decimal)0.5;
                            _leave.Reason = _leaveDT.Rows[i]["Reason"].ToString();
                            _leave.Status = _leaveDT.Rows[i]["Status"].ToString();
                            _leave.LeaveDeductedFrom = _deductedFrom;                           

                            _leaveList.Add(_leave);

                            #endregion
                        }
                        else
                        {
                            #region --- For HalfDay Leave Validation ---

                            var _holiday = HolidayRepository.IsHoliday(holidayList, _leaveFromDate.Value);
                            if (_holiday.Type == HolidayType.HalfDay)
                            {
                                Leave _leave = new Leave() { Date = _leaveFromDate.Value, Employee = _employee, LeaveType = LeaveType.HalfDay };
                                _leave.Reason = _leaveDT.Rows[i]["Reason"].ToString();
                                _leave.Status = _leaveDT.Rows[i]["Status"].ToString();
                                _leave.LeaveDeductedFrom = _deductedFrom;
                                _totalLeaveCount -= (decimal)0.5;
                                _leaveList.Add(_leave);
                            }

                            #endregion
                        }

                        _leaveFromDate = _leaveFromDate.Value.AddDays(1);
                    }
                }
            }

            return _leaveList;
        }

        internal static Leave IsLeave(IList<Leave> leaveList, DateTime checkDate)
        {
            Leave _leave = leaveList.FirstOrDefault(h => h.Date.Date == checkDate.Date);
            return _leave;
        }

        internal static Leave IsLeave(IList<Leave> leaveList, DateTime checkDate, int employeeId)
        {
            var _employeeLeaveList = leaveList.Where(e => e.Employee.EmployeeId == employeeId);
            Leave _leave = _employeeLeaveList.FirstOrDefault(h => h.Date.Date == checkDate.Date && h.Employee.EmployeeId == employeeId);
            return _leave;
        }

        private static decimal TryParseLeave(string leaveCountString)
        {
            decimal _leaveCount = 0;
            if (Decimal.TryParse(leaveCountString, out _leaveCount))
                return _leaveCount;
            else
                return 0; ;
        }

        private static DateTime? TryParseDate(string dateString)
        {
            DateTime _date;
            if (DateTime.TryParse(dateString, out _date))
                return _date;
            else
                return null;
        }
        
		public static int GetLeaveCount(DateTime fromDate, DateTime toDate, int employeeId, IList<Holiday> holidayList)
		{
			IList<Leave> _leaveList = new List<Leave>();

            var useLMSService = ConfigurationManager.AppSettings["UseLMSService"];
            if (useLMSService == "1")
                return 0;

           EmployeeEnrollment employee = EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(employeeId);

			#region --- Query Builder ---

			string queryText = @"SELECT [DateFrom]
                              ,[DateTo]
                              ,[AnnualDays]
                              ,[MedicalDays]
                              ,[CasualDays]
                              ,[NoPayDays]
                             ,[OtherDays]
                              ,[Status]
                              FROM [LeaveRegistry] L
                              Inner Join [UserTbl] U on U.UserId = L.UserId
                               where U.Username='{0}' AND [Status] <> 'Cancelled' AND [DateFrom] <= '{1}' AND [DateTo] >='{2}'";


			string query = string.Format(queryText, employee.UserName,
				fromDate.ToString("MM/dd/yyyy"), toDate.ToString("MM/dd/yyyy"));

			//string query = string.Format(queryText, employee.Username,
			//	fromDate, toDate);

			#endregion

			DataTable _leaveDT = Util.SqlHelper.ExecuteStatement(query);
			

			return _leaveList.Count;
		}

        public static IList<Leave> GetLeave2(DateTime fromDate, DateTime toDate, int employeeId, IList<Holiday> holidayList, List<MyLeaveViewModel> employeeLeaves)
		{
			IList<Leave> _leaveList = new List<Leave>();

            var useLMSService = ConfigurationManager.AppSettings["UseLMSService"];

            var _leaveDT = new DataTable();

		    if (useLMSService == "1")
		    {
                var leaveListFromMyLeaveService = employeeLeaves;
		        var employeeData = EmployeeRepository.GetEmployee(employeeId);
		        List<MyLeaveViewModel> leaveDetails =
		            leaveListFromMyLeaveService.Where(a => a.user == employeeData.PrimaryEmailAddress).ToList();
		       
                _leaveList = FormatLeaveListFromMyLeaveService(leaveDetails, employeeId, fromDate);
		    }
		    else
		    {
		        EmployeeEnrollment employee = EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(employeeId);

		        if (employee == null)
		            return _leaveList;

		        #region --- Query Builder ---

		        string queryText = @"SELECT [DateFrom]
                              ,[DateTo]
                              ,[AnnualDays]
                              ,[MedicalDays]
                              ,[CasualDays]
                              ,[NoPayDays]
                             ,[OtherDays]
                              ,[Status]
                              FROM [LeaveRegistry] L
                              Inner Join [UserTbl] U on U.UserId = L.UserId
                               where U.Username='{0}' AND [Status] <> 'Cancelled' AND [DateFrom] <= '{1}' AND [DateTo] >='{2}'";

		        string query = string.Format(queryText, employee.UserName,
		            fromDate.ToString("MM/dd/yyyy"), toDate.ToString("MM/dd/yyyy"));

		        #endregion
                _leaveDT = Util.SqlHelper.ExecuteStatement(query);

                for (int i = 0; i < _leaveDT.Rows.Count; i++)
                {
                    decimal _totalLeaveCount = TryParseLeave(_leaveDT.Rows[i]["AnnualDays"].ToString()) +
                        TryParseLeave(_leaveDT.Rows[i]["MedicalDays"].ToString()) +
                        TryParseLeave(_leaveDT.Rows[i]["CasualDays"].ToString()) +
                        TryParseLeave(_leaveDT.Rows[i]["NoPayDays"].ToString()) +
                        TryParseLeave(_leaveDT.Rows[i]["OtherDays"].ToString());

                    DateTime? _leaveFromDate = TryParseDate(_leaveDT.Rows[i]["DateFrom"].ToString());

                    while (_leaveFromDate <= toDate)
                    {
                        if (_totalLeaveCount <= 0)
                            break;

                        if (HolidayRepository.IsHoliday(holidayList, _leaveFromDate.Value) == null)
                        {
                            #region --- Not Holidays consider as leave ---

                            Leave _leave = new Leave() { Date = _leaveFromDate.Value, Employee = employee, LeaveType = LeaveType.FullDay };
                            if (_totalLeaveCount < 1)
                                _leave.LeaveType = LeaveType.HalfDay;

                            if (_leave.LeaveType == LeaveType.FullDay)
                                _totalLeaveCount -= 1;
                            else
                                _totalLeaveCount -= (decimal)0.5;
                            //_leave.Status = _leaveDT.Rows[i]["Status"].ToString();
                            _leaveList.Add(_leave);

                            #endregion
                        }
                        else
                        {
                            #region --- For HalfDay Leave Validation ---

                            var _holiday = HolidayRepository.IsHoliday(holidayList, _leaveFromDate.Value);
                            if (_holiday.Type == HolidayType.HalfDay)
                            {
                                Leave _leave = new Leave() { Date = _leaveFromDate.Value, Employee = employee, LeaveType = LeaveType.HalfDay };
                                _totalLeaveCount -= (decimal)0.5;
                                //_leave.Status = _leaveDT.Rows[i]["Status"].ToString();
                                _leaveList.Add(_leave);
                            }

                            #endregion
                        }

                        _leaveFromDate = _leaveFromDate.Value.AddDays(1);
                    }
                }
		    }
		   


			return _leaveList;
		}

        public static IList<Models.Leave> GetLeaveApproved(DateTime fromDate, DateTime toDate, EmployeeEnrollment employee, IList<Models.Holiday> holidayList)
        {
            IList<Models.Leave> _leaveList = new List<Models.Leave>();

            var useLMSService = ConfigurationManager.AppSettings["UseLMSService"];
            if (useLMSService == "1")
                return _leaveList;

            EmployeeEnrollment employeeData = EmployeeEnrollmentRepository.GetEmployeeEnrollmentById(employee.EmployeeId);
            if (employee == null)
                return _leaveList;

            #region --- Query Builder ---

            string queryText = @"SELECT [DateFrom]
                              ,[DateTo]
                              ,[AnnualDays]
                              ,[MedicalDays]
                              ,[CasualDays]
                              ,[NoPayDays]
                              ,[OtherDays]
                              ,[Status]
                              FROM [LeaveRegistry] L
                              Inner Join [UserTbl] U on U.UserId = L.UserId
                              where U.Username='{0}' AND [Status] = 'Approved' AND [DateFrom] <= '{1}' AND [DateTo] >='{2}'";

            string query = string.Format(queryText, employee.UserName,
                fromDate.ToString("MM/dd/yyyy"), toDate.ToString("MM/dd/yyyy"));

            #endregion

            DataTable _leaveDT = Util.SqlHelper.ExecuteStatement(query);
            for (int i = 0; i < _leaveDT.Rows.Count; i++)
            {
                decimal _totalLeaveCount = TryParseLeave(_leaveDT.Rows[i]["AnnualDays"].ToString()) +
                    TryParseLeave(_leaveDT.Rows[i]["MedicalDays"].ToString()) +
                    TryParseLeave(_leaveDT.Rows[i]["CasualDays"].ToString()) +
                    TryParseLeave(_leaveDT.Rows[i]["NoPayDays"].ToString()) +
                    TryParseLeave(_leaveDT.Rows[i]["OtherDays"].ToString());

                DateTime? _leaveFromDate = TryParseDate(_leaveDT.Rows[i]["DateFrom"].ToString());
                DateTime? _leaveToDate = TryParseDate(_leaveDT.Rows[i]["DateTo"].ToString());

                while (_leaveFromDate <= toDate)
                {
                    if (_totalLeaveCount <= 0)
                        break;

                    if (HolidayRepository.IsHoliday(holidayList, _leaveFromDate.Value) == null)
                    {
                        #region --- Not Holidays consider as leave ---

                        Models.Leave _leave = new Leave() { Date = _leaveFromDate.Value, Employee = employeeData, LeaveType = LeaveType.FullDay };
                        if (_totalLeaveCount < 1)
                            _leave.LeaveType = LeaveType.HalfDay;

                        if (_leave.LeaveType == LeaveType.FullDay)
                            _totalLeaveCount -= 1;
                        else
                            _totalLeaveCount -= (decimal)0.5;
                        _leave.Status = _leaveDT.Rows[i]["Status"].ToString();
                        _leaveList.Add(_leave);

                        #endregion
                    }
                    else
                    {
                        #region --- For HalfDay Leave Validation ---

                        var _holiday = HolidayRepository.IsHoliday(holidayList, _leaveFromDate.Value);
                        if (_holiday.Type == HolidayType.HalfDay)
                        {
                            Models.Leave _leave = new Leave() { Date = _leaveFromDate.Value, Employee = employeeData, LeaveType = LeaveType.HalfDay };
                            _totalLeaveCount -= (decimal)0.5;
                            _leave.Status = _leaveDT.Rows[i]["Status"].ToString();
                            _leaveList.Add(_leave);
                        }

                        #endregion
                    }

                    _leaveFromDate = _leaveFromDate.Value.AddDays(1);
                }
            }

            return _leaveList;
        }


    }
}