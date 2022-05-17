using Exilesoft.Models;
using Exilesoft.MyTime.Filters;
using Exilesoft.MyTime.Repositories;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Mvc;

namespace Exilesoft.MyTime.Controllers
{
  
    public class EmployeAttendanceController : BaseController
    {
        [ApplicationAuthentication]
        [System.Web.Mvc.HttpGet]
        [System.Web.Mvc.ActionName("GetEmployeeAttendance")]
        public IEnumerable<AttendanceDataViewModel> GetEmployeeAttendance(DateTime date)
        {
            return DailyAttendanceRepository.GenerateDailyAttandanceData(date);
        }
        [ApplicationAuthentication]
        [System.Web.Mvc.HttpGet]
        [System.Web.Mvc.ActionName("GetTaskListForEmployeeByDate")]
        public IEnumerable<WorkingFromHomeTask> GetTaskListForEmployeeByDate(int id, DateTime date)
        {
            return DailyAttendanceRepository.GetTaskListForEmployeeByDate(id,date);
        }
        
    }
}
