using System;
using Exilesoft.Models;
using Exilesoft.MyTime.Common;

namespace Exilesoft.MyTime.Mappings
{
    public class AttendanceViewModelMapper : MapperBase<Attendance, AttendanceView>
    {
        public override Attendance Map(AttendanceView element)
        {
            throw new NotImplementedException();
        }

        public override AttendanceView Map(Attendance element)
        {
            return new AttendanceView()
                   {
                       Id=element.Id,
                       EmployeeId = element.EmployeeId,
                       Time=new DateTime(element.Year,element.Month,element.Day,element.Hour,element.Minute,element.Second),
                       Date=new DateTime(element.Year,element.Month,element.Day),
                       InOutMode=element.InOutMode,
                       LocationId = element.LocationId,
                       CardNo = element.CardNo
                   };
        }
    }
}