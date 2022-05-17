using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Security;
using System.Web.UI.WebControls;
//using ProjectDelphi.ApiFilters.Helper;
using Exilesoft.MyTime.Controllers;
using Exilesoft.MyTime.Helpers;

namespace Exilesoft.MyTime.Filters
{
	public class DelphiAuthentication: AuthorizeAttribute
	{
		private readonly string[] _roleArray;

        public static string CookieName
        {
            get { return CookieHelper.CookieName; }
        }

		public DelphiAuthentication(params string[] roles)
		{
			_roleArray = roles;
		}

		public override void OnAuthorization(AuthorizationContext filterContext)
		{
            var authCookie = HttpContext.Current.Request.Cookies.Get(CookieName);

            if (authCookie == null || string.IsNullOrEmpty(authCookie.Value))
		    {
                filterContext.Result = new RedirectResult("Home/Index");
		    }
		    else
		    {
		        FormsAuthenticationTicket authenticationTicket = FormsAuthentication.Decrypt(authCookie.Value);

		        if (authenticationTicket == null)
		        {
		            HandleUnotherizeResponce(filterContext, HttpStatusCode.Unauthorized, "Invalid Token");
                    filterContext.Result = new RedirectResult("Home/Index");
		            return;
		        }

		        if (authenticationTicket.Expired)
		        {
		            HandleUnotherizeResponce(filterContext, HttpStatusCode.Forbidden, "Invalid Expired");
                    filterContext.Result = new RedirectResult("Home/Index");
		            return;
		        }

		        //user data format EmployeeId, Roles
		        string[] userData = authenticationTicket.UserData.Split(',');
		        string employeeId = userData[0];
		        string[] roles = userData[1].Split('|');
		        string userName = authenticationTicket.Name;
                var userIdentity = new GenericIdentity(userName);
		        var userPrincipal = new GenericPrincipal(userIdentity, roles);
		        HttpContext.Current.User = userPrincipal;

		        HttpRuntime.Cache.Insert("EmployeeId", employeeId);

		        if (!IsUserInRoles())
		        {
                    filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    filterContext.HttpContext.Response.StatusDescription = HttpStatusCode.Unauthorized.ToString();
                    return;
		        }

                CookieHelper.SetCookie(userName, authenticationTicket.UserData);
		    }
		}

		private void HandleUnotherizeResponce(AuthorizationContext actionContext, HttpStatusCode status, string message)
		{
			actionContext.HttpContext.Response.StatusCode = (int) status;
			actionContext.HttpContext.Response.StatusDescription = status.ToString(); 
			HttpContext.Current.User = null;
		}

		private bool IsUserInRoles()
		{
			if (_roleArray.Any())
			{
				return _roleArray.Any(role => HttpContext.Current.User.IsInRole(role));
			}
			return true;
		}
	}
}