using Exilesoft.Models;
using Exilesoft.MyTime.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels
{
    public class AttendanceReportViewModel
    {
        public AttendanceReportViewModel()
        {
            this.Year = System.DateTime.Today.Year;
            this.Month = System.DateTime.Today.Month;
            this.Day = System.DateTime.Today.Day;
            this.TeamList = TeamManagementRepository.getTeamList();
        }

        [Display(Name = "Year")]
        public int Year { get; set; }
        [Display(Name = "Month")]
        public int Month { get; set; }
        [Display(Name = "Day")]
        public int Day { get; set; }
        [Display(Name = "Team")]        
        public int TeamId { get; set; }
        [Display(Name = "Employee Attendance Report")]
        public string AttendenceRptTypeId { get; set; }

        public IList<TeamModel> TeamList{ get; set; }
        
    }   
}