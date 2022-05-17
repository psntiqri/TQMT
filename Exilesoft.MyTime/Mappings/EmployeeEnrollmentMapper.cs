using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Exilesoft.Models;
using Exilesoft.MyTime.ViewModels.Administration;

namespace Exilesoft.MyTime.Mappings
{
    public class EmployeeEnrollmentMapper : MapperBase<EmployeeEnrollment, EmployeeEnrollmentViewModel>
    {

        public override EmployeeEnrollment Map(EmployeeEnrollmentViewModel element)
        {
            throw new NotImplementedException();
        }

        public override EmployeeEnrollmentViewModel Map(EmployeeEnrollment element)
        {
            if (element == null)
                return null;

            return new EmployeeEnrollmentViewModel()
            {
                EmployeeId = element.EmployeeId,
                CardNo = element.CardNo,
                UserName = element.UserName,
                Privillage = element.Privillage,
                IsEnable = element.IsEnable
            };
        }
    }
}