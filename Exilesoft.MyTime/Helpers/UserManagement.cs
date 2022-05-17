using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Exilesoft.Models;
using Exilesoft.MyTime.Helpers;

namespace Exilesoft.MyTime.Util
{
    public static class UserManagement
    {
        const string UglCustomerName = "UGL";

        public static bool IsLoggedUserAdmin(this HtmlHelper instance)
        {
            Context db = new Context();
            string name = instance.ViewContext.HttpContext.User.Identity.Name;
            if (!String.IsNullOrEmpty(name))
            {
                EmployeeEnrollment loggedUser = db.EmployeeEnrollment.FirstOrDefault(a => a.UserName == name);
                if (null == loggedUser) return false;
                if (false == loggedUser.IsEnable) return false;
				if (!(Utility.IsUserInRole("Employee")) && !(Utility.IsUserInRole("System Admin")))//(loggedUser.Privillage > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsDeviceSyncEnable(this HtmlHelper instance)
        {
            Context db = new Context();
            string name = instance.ViewContext.HttpContext.User.Identity.Name;
            if (!String.IsNullOrEmpty(name))
            {
                EmployeeEnrollment loggedUser = db.EmployeeEnrollment.FirstOrDefault(a => a.UserName == name);
                if (null == loggedUser) return false;
                if (false == loggedUser.IsEnable) return false;
				if (!Utility.IsUserInRole("Employee"))//(loggedUser.Privillage > 0 || loggedUser.Privillage == -1)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsLoggedUserCustomer(this HtmlHelper instance)
        {
            Context db = new Context();
            string name = instance.ViewContext.HttpContext.User.Identity.Name;
            if (!String.IsNullOrEmpty(name))
            {
                if (name.ToUpper() == UglCustomerName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}