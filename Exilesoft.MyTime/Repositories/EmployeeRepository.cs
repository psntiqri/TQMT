// -------------DEVELOPER COMMENT---------------------------//
//
// Filename     : EmployeeRepository.cs
// Created By   : Harinda Dias
// Date         : 2013-Jul-04, Thu
// Description  : Repository for the Employee view 

//
// Modified By  : 
// Date         : 
// Purpose      : 
//
// ---------------------------------------------------------//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using Exilesoft.Models;
using Newtonsoft.Json;


namespace Exilesoft.MyTime.Repositories
{
    /// <summary>
    /// Repository for the Employee view 
    /// </summary>
    public class EmployeeRepository
    {
        private static Context dbContext = new Context();

        internal static List<Title> titles = new List<Title> { new Title { Id = 0, Name = "Mr" }, new Title { Id = 1, Name = "Mrs" }, new Title { Id = 2, Name = "Miss" }, new Title { Id = 3, Name = "NotDefined" } };
        internal static List<Gender> genderList = new List<Gender> { new Gender { Id = 0, Name = "Male" }, new Gender { Id = 1, Name = "Female" }, new Gender { Id = 2, Name = "NotDefined" } };
        internal static List<CivilStatus> civilStatuses = new List<CivilStatus> { new CivilStatus { Id = 0, Name = "Single" }, new CivilStatus { Id = 1, Name = "Married" }, new CivilStatus { Id = 2, Name = "Divorced" }, new CivilStatus { Id = 3, Name = "NotDefined" } };

        private static List<EmployeeData> cacheEmployeeDataList = new List<EmployeeData>();


        internal static List<EmployeeData> GetAllEmployeeOrderByName()
        {
            return GetAllEmployees().OrderBy(e => e.Name).ToList();
            // return dbContext.Employees.OrderBy(a => a.Name).ToList();
        }

        internal static int EmployeeCount()
        {
            List<EmployeeData> employeeDataList = GetAllEnableEmployees();

            return employeeDataList.Count();

            //dbContext = new Context();
            //return dbContext.Employees.Count(a => a.Enable);
        }

        //internal static List<Employee> GetAllEnableEmployees()
        //{
        //    dbContext = new Context();
        //    return dbContext.Employees.Where(a => a.Enable).ToList();
        //}

        internal static int EmployeesOnSite(DateTime? selectedDate)
        {
            dbContext = new Context();
            return dbContext.EmployeesOnSite.Count(a => (a.EmployeeEnrollment.IsEnable == true) && a.FromDate <= selectedDate && a.ToDate >= selectedDate);
        }

        internal static List<EmployeeData> EmployeeSearchByNameInCacheAndEmployeeService(string searchText)
        {
            bool searchedFromService = false;
            if (cacheEmployeeDataList.Count < 1)
            {
                List<EmployeeData> cacheEmployeeDataListCopy = GetAllEmployeesFromService();
                List<int> employeeEnrollmentsIds = EmployeeEnrollmentRepository.GetEmployeeEnrollments().Select(s => s.EmployeeId).ToList();
                cacheEmployeeDataList = cacheEmployeeDataListCopy.Where(s => employeeEnrollmentsIds.Contains(s.Id)).ToList();
                searchedFromService = true;
            }
            List<EmployeeData> employeeList = cacheEmployeeDataList.Where(a => a.Name.ToLower().StartsWith(searchText.ToLower())).ToList();
            if (employeeList.Count == 0 && !searchedFromService)
            {
                List<EmployeeData> cacheEmployeeDataListCopy = GetAllEmployeesFromService();
                List<int> employeeEnrollmentsIds = EmployeeEnrollmentRepository.GetEmployeeEnrollments().Select(s => s.EmployeeId).ToList();
                cacheEmployeeDataList = cacheEmployeeDataListCopy.Where(s => employeeEnrollmentsIds.Contains(s.Id)).ToList();
                employeeList = cacheEmployeeDataList.Where(a => a.Name.ToLower().StartsWith(searchText.ToLower())).ToList();
            }
            return employeeList;
        }

        internal static List<EmployeeData> EmployeeSearchByName(string searchText)
        {
            var tempValue = GetAllEmployeesFromService();
            List<EmployeeData> employeeDataList = tempValue.Where(a => a.Name.ToLower().Trim().StartsWith(searchText.ToLower())).ToList<EmployeeData>(); 
            
            return employeeDataList;
            
        }

        internal static EmployeeData GetEmployee(int? employeeId)
        {
            if (employeeId == null)
                return null;

            var tempValue = CasheEmployeeData(); ;

            EmployeeData employeeData = tempValue.Find(a => a.Id == employeeId);

            //string url = ConfigurationManager.AppSettings["EmployeeWebApi"];
            //string data = new WebClient().DownloadString(string.Format(url, "GetMyTimeEmployeeById?Id=" + employeeId));

            // JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            // EmployeeData employeeData =  (EmployeeData)jsonSerializer.Deserialize(data, typeof(EmployeeData));


            return employeeData;
        }
        internal static EmployeeData GetEmployeeByEmail(string emaiId)
        {
            if (emaiId == null)
                return null;

            var tempValue = CasheEmployeeData(); ;

            EmployeeData employeeData = tempValue.Find(a => a.PrimaryEmailAddress == emaiId);


            return employeeData;
        }
        internal static List<EmployeeData> GetAllEmployeesFromService()
        {
            var webClient = new WebClient();
            //webClient.Headers.Add(ConfigurationManager.AppSettings["ClientIdToBeVerified"], ConfigurationManager.AppSettings["ClientId"]);

            var data = webClient.DownloadString(string.Format(ConfigurationManager.AppSettings["EmployeeWebApi"], "GetMyTimeEmployees"));
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();

            cacheEmployeeDataList = (List<EmployeeData>)jsonSerializer.Deserialize(data, typeof(List<EmployeeData>));
            return cacheEmployeeDataList;

        }

        internal static List<EmployeeData> GetAllEmployeesTeamWiseFromService(string teamId,DateTime from, DateTime to)
        {
            
            
            var webClient = new WebClient();           
           // var url = string.Format(ConfigurationManager.AppSettings["EmployeeListByTeam"] + "/api/AllocationByProjects/team?projectId=" + teamId + "&from=" + from + "&to=" + to);
            var url = "https://teams.tiqri.com/api/AllocationByProjects/team?projectId=00000000-0000-0000-0000-000000000007&from=08/01/2018&to=09/30/2018";
            var data = webClient.DownloadString(url);
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();


            //dynamic EmpList = (person)jsonSerializer.Deserialize(data, typeof(person));
            dynamic obj = JsonConvert.DeserializeObject(data);
            var EmpList = obj.team;
            IList<person> personList = (IList<person>)EmpList;
            //var idList =EmpList.Select(a=>a.id).ToList();


           //return cacheEmployeeDataList.Where(a => idList.Contains(a.Id)).Select(s => s).ToList();

    
           return null;
        }


        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
        internal static List<EmployeeData> GetAllSupervisorsFromService()
        {
            List<EmployeeData> employeList = new List<EmployeeData>();

            var webClient = new WebClient();


            var data = webClient.DownloadString(ConfigurationManager.AppSettings["approverList"] + Base64Encode("BC0B17DE-C731-4A81-9544-13F442A6E16D"));

            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();



            var approverList = (List<LeaveApprover>)jsonSerializer.Deserialize(data, typeof(List<LeaveApprover>));

            foreach (var item in approverList)
            {
                var emp = EmployeeRepository.GetEmployeeByEmail(item.email);
                if (emp!=null && emp.IsEnable)
                {
                    employeList.Add(emp);
                }
            }


            return employeList;

        }

        internal static List<EmployeeData> GetAllEmployees()
        {
            cacheEmployeeDataList = GetAllEmployeesFromService();
            List<int> employeeEnrollmentsIds = EmployeeEnrollmentRepository.GetEmployeeEnrollments().Select(s => s.EmployeeId).ToList();
            return cacheEmployeeDataList.Where(s => employeeEnrollmentsIds.Contains(s.Id)).ToList();
        }

        internal static List<EmployeeData> GetAllEnableEmployees()
        {

            {
                //string data = new WebClient().DownloadString(string.Format(ConfigurationManager.AppSettings["EmployeeWebApi"], "GetMyTimeEmployees"));
                //JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();

                //List<EmployeeData> employeeDataList = (List<EmployeeData>)jsonSerializer.Deserialize(data, typeof(List<EmployeeData>));
                //return employeeDataList.Where(e => e.IsEnable).ToList();
                var tempValue = CasheEmployeeData();
                List<int> employeeEnrollmentsIds = EmployeeEnrollmentRepository.GetEmployeeEnrollments().Select(s => s.EmployeeId).ToList();
                var employeeList = tempValue.Where(s => employeeEnrollmentsIds.Contains(s.Id)).ToList();
                return employeeList.Where(e => e.IsEnable).ToList();
            }

        }

        private static List<EmployeeData> CasheEmployeeData()
        {
            if (cacheEmployeeDataList.Count < 1)
            {
                cacheEmployeeDataList = GetAllEmployees();
            }
            return cacheEmployeeDataList;
        }


        /// <summary>
        /// Gets the employee view models list to the client
        /// Converted to the user view
        /// </summary>
        /// <returns>Employee List for the gridview</returns>
        //internal static List<ViewModels.EmployeeListviewModel> GetAllEmployeeList()
        //{
        //    dbContext = new Context();
        //    var employeeList = EmployeeRepository.GetAllEmployees();

        //    List<ViewModels.EmployeeListviewModel> employeeViewLogList = new List<ViewModels.EmployeeListviewModel>();
        //    // Query for the latest 1000 records for the selection to the client
        //    foreach (var employee in employeeList.OrderByDescending(a => a.I).ThenBy(a => a.Name))
        //    {
        //        //string priviladgeValue = employee.Priv != null ? PrivilegesRepository.GetAllPrivilegeList().FirstOrDefault(a => a.Id == employee.Priv).Name : string.Empty;
        //        string titleValue = employee.Title != null ? titles.FirstOrDefault(a => a.Id == employee.Title).Name : string.Empty;
        //        string civilStatusvalue = employee.CivilStatus != null ? civilStatuses.FirstOrDefault(a => a.Id == employee.CivilStatus).Name : string.Empty;
        //        string genderValue = employee.Gender != null ? genderList.FirstOrDefault(a => a.Id == employee.Gender).Name : string.Empty;
        //        string totalExperience = string.Empty;
        //        string exileExperience = string.Empty;

        //        if (employee.DateCareerStarted != null)
        //        {
        //            var totalExpSpan = DateTimeSpan.CompareDates(employee.DateCareerStarted.Value, DateTime.Today);
        //            totalExperience = string.Format("{0} Years {1} Months", totalExpSpan.Years, totalExpSpan.Months);
        //        }

        //        if (employee.DateJoined.Year != 1900)
        //        {
        //            var exileExpSpan = DateTimeSpan.CompareDates(employee.DateJoined, DateTime.Today);
        //            exileExperience = string.Format("{0} Years {1} Months", exileExpSpan.Years, exileExpSpan.Months);
        //        }

        //        string fullName = employee.FirstName + " " + employee.LastName;
        //        if (!titleValue.Equals("NotDefined"))
        //            fullName = titleValue + " " + fullName;

        //        // Creating the employee viewmodels for the client
        //        employeeViewLogList.Add(new ViewModels.EmployeeListviewModel()
        //        {
        //            EditLink = string.Format("<a href=\"javascript:new EmployeeForm().AddEditEmployee({0});\">Edit</a>", employee.Id.ToString()),
        //            AllocateLink = string.Format("<a class=\"iframe cboxElement\" href=\"/Employee/TeamAllocation/{0}\">Allocate</a>", employee.Id.ToString()),
        //            CheckListLink = string.Format("<a class=\"iframe cboxElement\" href=\"/Checklist/EmployeeCheckList/{0}\">CheckList</a>", employee.Id.ToString()),
        //            EmployeeID = employee.Id,
        //            CivilStatus = civilStatusvalue,
        //            CurrentAddress = employee.CurrentAddress,
        //            DateCareerStarted = employee.DateCareerStarted != null ? employee.DateCareerStarted.Value.ToShortDateString() : string.Empty,
        //            DateJoin = employee.DateJoined != null ? employee.DateJoined.ToShortDateString() : string.Empty,
        //            DateofBirth = employee.DateOfBirth != null ? employee.DateOfBirth.Value.ToShortDateString() : string.Empty,
        //            Designation = employee.Designation,
        //            EmerContactAddress = employee.EmergencyContactAddress,
        //            EmerContactNumber = employee.EmergencyContactNumber,
        //            EmerName = employee.EmergencyContactName,
        //            EmerRelationship = employee.EmergencyContactRelationship,
        //            Enable = employee.Enable ? "Yes" : "No",
        //            // EnrollmentNo = employee.CardNo.ToString(),
        //            FullName = fullName,
        //            Gender = genderValue,
        //            HomePhone = employee.HomePhone,
        //            MobileNumber = employee.MobileNumber,
        //            MSNID = employee.MSNID,
        //            Name = employee.Name,
        //            NIC = employee.NICNumber,
        //            Passport = employee.PassportNumber,
        //            PermanentAddress = employee.PermanentAddress,
        //            PreviousEmployer = employee.PreviousEmployer,
        //            PrimaryEmail = employee.PrimaryEmailAddress,
        //            //Privilege = priviladgeValue,
        //            SecondaryEmail = employee.SecondaryEmailAddress,
        //            SkypeID = employee.SkypeID,
        //            ExpExilesoft = exileExperience,
        //            ExpTotal = totalExperience
        //        });
        //    }

        //    return employeeViewLogList;
        //}

        /// <summary>
        /// Gets active employees between the given date range. If employee is active for only one day between the given 
        /// date range, he will be included in the return list.
        /// </summary>
        /// <param name="fromDate">Date Range - From F</param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        internal static List<EmployeeData> GetActiveEmployeesByDateRange(DateTime fromDate, DateTime toDate)
        {

            var tempValue = CasheEmployeeData();
            List<int> employeeEnrollmentsIds = EmployeeEnrollmentRepository.GetEmployeeEnrollments().Select(s => s.EmployeeId).ToList();
            var employeeList = tempValue.Where(s => employeeEnrollmentsIds.Contains(s.Id)).ToList();
            return employeeList.Where(e => e.DateJoined <= toDate && (e.DateResigned >= fromDate || e.DateResigned == null) && e.IsEnable).ToList();

        }
        internal static List<EmployeeData> GetEmployeesByDateRange(DateTime fromDate, DateTime toDate)
        {
            var tempValue = CasheEmployeeData();
            List<int> employeeEnrollmentsIds = EmployeeEnrollmentRepository.GetEmployeeEnrollments().Select(s => s.EmployeeId).ToList();
            var employeeList = tempValue.Where(s => employeeEnrollmentsIds.Contains(s.Id)).ToList();
            return employeeList.Where(e => e.DateJoined <= toDate && (e.DateResigned >= fromDate || e.DateResigned == null)).ToList();

        }
    }

    public struct DateTimeSpan
    {
        private readonly int years;
        private readonly int months;
        private readonly int days;
        private readonly int hours;
        private readonly int minutes;
        private readonly int seconds;
        private readonly int milliseconds;

        public DateTimeSpan(int years, int months, int days, int hours, int minutes, int seconds, int milliseconds)
        {
            this.years = years;
            this.months = months;
            this.days = days;
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
            this.milliseconds = milliseconds;
        }

        public int Years { get { return years; } }
        public int Months { get { return months; } }
        public int Days { get { return days; } }
        public int Hours { get { return hours; } }
        public int Minutes { get { return minutes; } }
        public int Seconds { get { return seconds; } }
        public int Milliseconds { get { return milliseconds; } }

        enum Phase { Years, Months, Days, Done }

        public static DateTimeSpan CompareDates(DateTime date1, DateTime date2)
        {
            if (date2 < date1)
            {
                var sub = date1;
                date1 = date2;
                date2 = sub;
            }

            DateTime current = date1;
            int years = 0;
            int months = 0;
            int days = 0;

            Phase phase = Phase.Years;
            DateTimeSpan span = new DateTimeSpan();

            while (phase != Phase.Done)
            {
                switch (phase)
                {
                    case Phase.Years:
                        if (current.AddYears(years + 1) > date2)
                        {
                            phase = Phase.Months;
                            current = current.AddYears(years);
                        }
                        else
                        {
                            years++;
                        }
                        break;
                    case Phase.Months:
                        if (current.AddMonths(months + 1) > date2)
                        {
                            phase = Phase.Days;
                            current = current.AddMonths(months);
                        }
                        else
                        {
                            months++;
                        }
                        break;
                    case Phase.Days:
                        if (current.AddDays(days + 1) > date2)
                        {
                            current = current.AddDays(days);
                            var timespan = date2 - current;
                            span = new DateTimeSpan(years, months, days, timespan.Hours, timespan.Minutes, timespan.Seconds, timespan.Milliseconds);
                            phase = Phase.Done;
                        }
                        else
                        {
                            days++;
                        }
                        break;
                }
            }

            return span;
        }

        //private string GetEmployeeName(int id)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri("http://localhost:9000/");
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //        // New code:
        //        Task<HttpResponseMessage> response = client.GetAsync("api/products/1");
        //        if (response.IsCompleted)
        //        {

        //        }
        //    }
        //}
    }

    public class person
    {

        int id { get; set; }
        string name { get; set; }
       
    }
   
}