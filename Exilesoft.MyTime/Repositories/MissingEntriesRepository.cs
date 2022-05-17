using Exilesoft.Models;
using Exilesoft.MyTime.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.Repositories
{
    public class MissingEntriesRepository
    {
        private static Context dbContext = new Context();

        internal static IList<EmployeeWiseMissingEntries> getEmployeeMissingEntries()
        {
            IList<EmployeeWiseMissingEntries> empMissingEntries = new List<EmployeeWiseMissingEntries>();
            IList <EmployeeMissingEntry> employeeMissingEntries = new List<EmployeeMissingEntry>();
            employeeMissingEntries = dbContext.EmployeeMissingEntries.ToList();
            IList<int> ids = employeeMissingEntries.Select(e => e.EmployeeId).Distinct().ToList();
            foreach (int id in ids)
            {
                IList<DateTime> dateTimes = employeeMissingEntries.Where(s => s.EmployeeId == id).Select(e => e.MissingDate).ToList();

                if (dateTimes.Count>0)
                {
                    empMissingEntries.Add(new EmployeeWiseMissingEntries
                    {
                        employeeId = id.ToString(),
                        employeeName = employeeMissingEntries.Where(s => s.EmployeeId == id).Select(e => e.EmployeeName).FirstOrDefault(),
                        missingDates = employeeMissingEntries.Where(s => s.EmployeeId == id).Select(e => e.MissingDate).ToList()
                    });
                }
                
            }

            return empMissingEntries;

        }
    }
}