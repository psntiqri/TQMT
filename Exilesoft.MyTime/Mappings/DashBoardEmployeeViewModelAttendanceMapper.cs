using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Exilesoft.Models;
using Exilesoft.MyTime.Repositories;
using Exilesoft.MyTime.ViewModels;

namespace Exilesoft.MyTime.Mappings
{
    public class DashBoardEmployeeViewModelAttendanceMapper : MapperBase<Attendance, DashBoardEmployeeViewModel>
    {
        public override Attendance Map(DashBoardEmployeeViewModel element)
        {
            throw new NotImplementedException();
        }

        public override DashBoardEmployeeViewModel Map(Attendance element)
        {
            return new DashBoardEmployeeViewModel()
                   {
                       EmployeeID = element.EmployeeId.ToString(CultureInfo.InvariantCulture),
                       EmployeeName =EmployeeRepository.GetEmployee(element.EmployeeId).Name
                   };
        }
    }
}