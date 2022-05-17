using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exilesoft.Models;

namespace Exilesoft.MyTime.Areas.Reception.Repositories
{
    public interface IReceptionEmployeeRepository
    {
        EmployeeData GetEmployee(int? employeeId);
    }
}
