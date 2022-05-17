using Exilesoft.Models;
using Exilesoft.MyTime.ViewModels;

namespace Exilesoft.MyTime.Mappings
{
    public class PrivilegeViewModelMapper : MapperBase<Privilege, PrivilegeViewModel>
    {
        public override Privilege Map(PrivilegeViewModel element)
        {
            return new Privilege()
                   {
                       Id = element.Id,
                       Name = element.Name
                   };
        }

        public override PrivilegeViewModel Map(Privilege element)
        {
            return new PrivilegeViewModel()
            {
                Id = element.Id,
                Name = element.Name
            };
        }
    }
}