using Exilesoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.Repositories
{
    public class BacgroungRepository
    {
        

        internal static BackgroundMasters getBackgroundMasterProcesData(int year, int month, int day)
        {
           Context dbContext = new Context();
            return dbContext.BackgroundMasters.Where(a => a.Year == year && a.Month == month && a.Day == day).FirstOrDefault();
        }


        internal static BackgroundSlaves getBackgroundSlaveProcesDataByEmployeeId(int year, int month, int day, int EmpId)
        {
            Context dbContext = new Context();
            return dbContext.BackgroundSlaves.Where(a => a.Year == year && a.Month == month && a.Day == day && a.EmployeeId == EmpId).FirstOrDefault();

        }

        internal static IList<BackgroundSlaves> getBackgroundSlaveProcesData(int year, int month, int day)
        {
            Context dbContext = new Context();
            return dbContext.BackgroundSlaves.Where(a => a.Year == year && a.Month == month && a.Day == day).ToList();
        }

    }
}