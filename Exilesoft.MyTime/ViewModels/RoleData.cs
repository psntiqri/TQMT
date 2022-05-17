using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels
{
    public class RoleData
    {

        public RoleData(){
            Roles = new List<string> { };
        }
        public List<String> Roles { get; set; }
    }
}