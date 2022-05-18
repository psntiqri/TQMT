using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Exilesoft.Models;

namespace Exilesoft.MyTime.ViewModels
{
    public class DailyAttendanceViewModel
    {
        public DailyAttendanceViewModel()
        {

        }

        public DailyAttendanceViewModel(int? employeeID, DateTime? fromDate, DateTime? toDate)
        {
            this.FromDate = fromDate != null ? fromDate : System.DateTime.Today.AddDays(-7);
            this.ToDate = toDate != null ? toDate : System.DateTime.Today;
            this.SelectedEmployeeList = new List<EmployeeData>();
        }

        public string ReportType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public System.Collections.Generic.IList<EmployeeData> SelectedEmployeeList { get; set; }
        public IList<UserTeam> TeamSelectionList { get; set; }
    }

    public class AttendaceReportStructure
    {
        public AttendaceReportStructure()
        {
            this.EmployeeCoverageList = new List<EmployeeAttandaceCoverage>();
            this.EmployeeOutOfOfficeList = new List<EmployeeOutOfOffice>();
            this.EmployeeOutOfWFHList = new List<EmployeeOutOfWFH>();
            this.HolidayDateList = new List<HolidayOnView>();
        }

        public string ResultGraphData { get; set; }
        public IList<EmployeeAttandaceCoverage> EmployeeCoverageList { get; set; }
        public IList<EmployeeOutOfOffice> EmployeeOutOfOfficeList { get; set; }
        public IList<EmployeeOutOfWFH> EmployeeOutOfWFHList { get; set; }
        public IList<HolidayOnView> HolidayDateList { get; set; }

        public string Duration { get; set; }
        public decimal WorkingDays { get; set; }
        public int LoggedDays { get; set; }
        public decimal TotalPlanned { get; set; }
        public decimal TotalActual { get; set; }
        public decimal TotalActualWFH { get; set; }
        public decimal TotalOutOfOffice { get; set; }
        public decimal TotalPlannedLeave { get; set; }
        public decimal WorkCoverage { get; set; }
        public decimal WFHPercentage { get; set; }
        public string TotalTeamWorkCoverage { get; set; }
        public string TotalLeaveCount { get; set; }

        public int TotalDeviationForInTimeAverage { get; set; }
        public int TotalDeviationForOutTimeAverage { get; set; }
        public string AverageInTime { get; set; }
        public string AverageOutTime { get; set; }
    }

    public class EmployeeAttandaceCoverage
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public decimal ActualMinits { get; set; }
        public decimal PlannedMinits { get; set; }
        public string ActualHours { get; set; }
        public string PlannedHors { get; set; }
        public string Precentage { get; set; }
        public string WFHPrecentage { get; set; }
    }

    public class EmployeeOutOfOffice
    {
        public int EmployeeID { get; set; }
        public decimal OutMinits { get; set; }
        public string OutDate { get; set; }
        public decimal FromTime { get; set; }
        public decimal ToTime { get; set; }
    }
    public class EmployeeOutOfWFH
    {
        public int EmployeeID { get; set; }
        public decimal OutMinits { get; set; }
        public string OutDate { get; set; }
        public decimal FromTime { get; set; }
        public decimal ToTime { get; set; }
    }

    public class HolidayOnView
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Reason { get; set; }
    }

    public class WeekStructure
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int FromDate { get; set; }
        public int ToDate { get; set; }
        public int Weeknumber { get; set; }
    }

    public class EmployeeProjectStructure
    {
        public EmployeeData Employee { get; set; }
        public string Project { get; set; }
    }

    public class LateEmployeesModel
    {
        public LateEmployeesModel()
        {
        }
        public LateEmployeesModel(DateTime? date)
        {
            if (date == null)
            {
                this.Date = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 9, 0, 0);
            }
            else
                this.Date = date.Value;
        }
        public DateTime Date { get; set; }
    }

    public class PhysicallyNotAvailableEmployeModel
    {
        public PhysicallyNotAvailableEmployeModel()
        {

        }

        public PhysicallyNotAvailableEmployeModel(DateTime? fromdate, DateTime? toDate)
        {
            if (fromdate == null && toDate == null)
            {
                this.DateFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
                this.DateTo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
            }
            else
            {
                this.DateFrom = fromdate.Value;
                this.DateTo = toDate.Value;
            }
        }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string AbsentDate { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeID { get; set; }
    }
}