using Exilesoft.MyTime.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Exilesoft.MyTime.Controllers
{
    public class BaseController : Controller
    {

        ClaimsIdentity userClaims;
        // GET: Base
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            RoleData roleData = new RoleData();
            userClaims = User.Identity as ClaimsIdentity;

            var roleclaims = userClaims?.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

            if (userClaims != null)
            {
                for (int i = 0; i < roleclaims.Count(); i++)
                {
                    //var a = roleclaims.ElementAt(i)?.Value;

                    String role = roleclaims.ElementAt(i).Value;
                    roleData.Roles.Add(role);

                }


                ViewBag.Roles = roleData.Roles;
            }
           
            base.OnActionExecuting(filterContext);
        }
    }
}