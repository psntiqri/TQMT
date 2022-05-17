// -------------DEVELOPER COMMENT---------------------------//
//
// Filename     : HolidayRepository.cs
// Created By   : Harinda Dias
// Date         : 2013-May-10, Fri
// Description  : Repository for the Holiday management

//
// Modified By  : 
// Date         : 
// Purpose      : 
//
// ---------------------------------------------------------//

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
    /// <summary>
    /// Repository for the holiday management
    /// </summary>
    public static class HolidayRepository
    {
        /// <summary>
        /// Get the list of holidays for the date range
        /// </summary>
        /// <param name="fromDate">Range starting from date</param>
        /// <param name="toDate">Range end date</param>
        /// <returns>List of holidays</returns>
        public static IList<Holiday> GetHolidays(DateTime fromDate, DateTime toDate)
        {
            IList<Holiday> _holidayList = new List<Holiday>();
        
            DataTable _holidayDT = new DataTable();

            _holidayDT.Columns.Add("date", typeof(DateTime));
            _holidayDT.Columns.Add("Summary", typeof(String));

            
                var holidays = GetMyLeaveServiceHolidays("LK");
                holidays = holidays.Where(o => o.Date >= fromDate && o.Date <= toDate).ToList();
                foreach (var holidayViewModel in holidays)
                {
                    var dataRow = _holidayDT.NewRow();


                    dataRow[0] = holidayViewModel.Date;
                    dataRow[1] = holidayViewModel.Summary;
                    _holidayDT.Rows.Add(dataRow);     

                }
          
            
            for (int i = 0; i < _holidayDT.Rows.Count; i++)
            {
                DateTime _holidayDate;
                if (DateTime.TryParse(_holidayDT.Rows[i][0].ToString(), out _holidayDate))
                {
                    var _holiday = new Holiday()
                    {
                        Date = _holidayDate,
                        Description = _holidayDT.Rows[i][1].ToString(),
                        Type = HolidayType.FullDay
                    };

                    if (_holidayDT.Rows[i][1].ToString().ToUpper().IndexOf("HALF DAY") != -1)
                        _holiday.Type = HolidayType.HalfDay;
                    _holiday.Reason = _holidayDT.Rows[i][1].ToString().ToUpper();
                    _holidayList.Add(_holiday);
                }
            }

            return _holidayList;
        }

        private static IList<HolidayViewModel> GetMyLeaveServiceHolidays(string region)
        {
            string queryString = string.Format("holidays/" +  region);
            string url = string.Format(ConfigurationManager.AppSettings["MyLeaveWebApi"], queryString);
            // string finalUrl = string.Format(url, fromDate.ToString("MM/dd/yyyy"), toDate.ToString("MM/dd/yyyy"));

            WebClient webClient = new WebClient();
            string data =  webClient.DownloadString(url);
            var js = new JavaScriptSerializer();
            var holidays = js.Deserialize<HolidayViewModel[]>(data);

            return holidays.ToList();

        }

        /// <summary>
        /// Validate if the selected date is a holiday
        /// </summary>
        /// <param name="holidaysList">List of holidays to be validate with</param>
        /// <param name="checkDate">Check with the date</param>
        /// <returns></returns>
        internal static Holiday IsHoliday(IList<Holiday> holidaysList, DateTime checkDate)
        {
            Holiday _holiday = holidaysList.FirstOrDefault(h => h.Date.Date == checkDate);
            if (_holiday != null)
                return _holiday;

            if (checkDate.DayOfWeek == DayOfWeek.Saturday || checkDate.DayOfWeek == DayOfWeek.Sunday)
                _holiday = new Holiday() { Date = checkDate, Description = checkDate.DayOfWeek.ToString() };

            return _holiday;
        }

        internal static bool CheckIsHoliday(IList<Holiday> holidaysList, DateTime checkDate)
        {
            Holiday _holiday = IsHoliday(holidaysList, checkDate);

            if (_holiday != null)
                return true;

            return false;
        }
    }
}