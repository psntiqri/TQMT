using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Exilesoft.MyTime.Helpers;

namespace Exilesoft.MyTime.Areas.Reception.Filters
{
    public class ReceptionAuthentication : AuthorizeAttribute
    {
        private readonly string[] _roleArray;

        public static string CookieName
        {
            get
            {
                return System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/Areas/Reception/Web.config")
                        .AppSettings.Settings["ReceptionDesktopCoockieName"].Value;
            }
        }

        public ReceptionAuthentication(params string[] roles)
		{
			_roleArray = roles;
		}

		public override void OnAuthorization(AuthorizationContext filterContext)
		{
            var authCookie = HttpContext.Current.Request.Cookies.Get(CookieName);

            if (authCookie == null || string.IsNullOrEmpty(authCookie.Value))
		    {
                filterContext.Result = new RedirectResult("ReceptionHome/Index");
		    }
		    else
		    {
		        FormsAuthenticationTicket authenticationTicket = FormsAuthentication.Decrypt(authCookie.Value);

		        if (authenticationTicket == null)
		        {
		            HandleUnotherizeResponce(filterContext, HttpStatusCode.Unauthorized, "Invalid Token");
                    filterContext.Result = new RedirectResult("ReceptionHome/Index");
		            return;
		        }

		        if (authenticationTicket.Expired)
		        {
		            HandleUnotherizeResponce(filterContext, HttpStatusCode.Forbidden, "Invalid Expired");
                    filterContext.Result = new RedirectResult("ReceptionHome/Index");
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