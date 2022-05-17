using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exilesoft.Models;

namespace Exilesoft.MyTime.Areas.AmexSecure.Repositories
{
    public interface IAmexEmployeeRepository
    {
        EmployeeData GetEmployee(int? employeeId);
    }
}
