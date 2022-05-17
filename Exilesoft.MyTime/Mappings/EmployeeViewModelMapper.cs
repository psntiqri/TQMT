using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Exilesoft.Models;
using Exilesoft.MyTime.ViewModels;

namespace Exilesoft.MyTime.Mappings
{
    public class EmployeeViewModelMapper : MapperBase<EmployeeData, EmployeeViewModel>
    {
        public override EmployeeData Map(EmployeeViewModel element)
        {
            throw new NotImplementedException();
        }

        public override EmployeeViewModel Map(EmployeeData element)
        {
            return new EmployeeViewModel()
                   {
                       Id = element.Id,
                       Name = element.Name
                      
                   };

        }
    }
}