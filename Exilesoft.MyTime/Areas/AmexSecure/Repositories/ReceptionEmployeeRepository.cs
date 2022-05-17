using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Exilesoft.Models;
using Exilesoft.MyTime.Repositories;

namespace Exilesoft.MyTime.Areas.AmexSecure.Repositories
{
    public class AmexEmployeeRepository : IAmexEmployeeRepository
    {
        public EmployeeData GetEmployee(int? employeeId)
        {

            return EmployeeRepository.GetEmployee(employeeId);
                
        }
    }
}