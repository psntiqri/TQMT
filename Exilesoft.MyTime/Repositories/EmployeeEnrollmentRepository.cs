using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Exilesoft.Models;

namespace Exilesoft.MyTime.Repositories
{
    public class EmployeeEnrollmentRepository
    {
        private static Context dbContext = new Context();

        internal static EmployeeEnrollment GetEmployeeEnrollmentById(int employeeId)
        {
            EmployeeEnrollment employeeEnrollment;
            using (Context context= new Context())
            {
                employeeEnrollment = context.EmployeeEnrollment.SingleOrDefault(a => a.EmployeeId == employeeId);
            }
            return employeeEnrollment;
        }

        internal static EmployeeEnrollment GetEmployeeEnrollmentByMobileId(Guid mobileId)
        {
            return dbContext.EmployeeEnrollment.SingleOrDefault(a => a.MobileId == mobileId);
        }

        internal static List<EmployeeEnrollment> GetEmployeeEnrollments()
        {
           return dbContext.EmployeeEnrollment.ToList();
        }

        internal static EmployeeEnrollment GetEmployeeEnrollmentByUsername(string searchText)
        {
            return dbContext.EmployeeEnrollment.SingleOrDefault(a => a.UserName.ToUpper() == searchText.Trim().ToUpper());
        }

        internal static void SaveUser(EmployeeEnrollment user)
        {
            var enrollmwnt = dbContext.EmployeeEnrollment.SingleOrDefault(u => u.EmployeeId == user.EmployeeId);

            if (enrollmwnt == null)

            {              
                dbContext.EmployeeEnrollment.Add(user);                
            }
            else
            {
                enrollmwnt.CardNo = user.CardNo;
                enrollmwnt.EmployeeId = user.EmployeeId;
                enrollmwnt.IsEnable = user.IsEnable;
                enrollmwnt.Privillage = user.Privillage;
                enrollmwnt.UserName = user.UserName;    
            }
            dbContext.SaveChanges();         
            			
        }
    }
}