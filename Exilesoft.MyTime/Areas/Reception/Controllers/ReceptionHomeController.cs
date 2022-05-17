using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using Exilesoft.MyTime.Areas.Reception.Common;
using Exilesoft.MyTime.Areas.Reception.Filters;
using Exilesoft.MyTime.Areas.Reception.Repositories;
using Exilesoft.MyTime.Helpers;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;

namespace Exilesoft.MyTime.Areas.Reception.Controllers
{
	public class ReceptionHomeController : Controller
	{
        public static string CookieName
        {
            get
            {
                return System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/Areas/Reception/Web.config")
                        .AppSettings.Settings["ReceptionDesktopCoockieName"].Value;
            }
        }

		public ActionResult Index()
		{


			//var authCookie = System.Web.HttpContext.Current.Request.Cookies.Get(CookieName);

			//if (authCookie == null || string.IsNullOrEmpty(authCookie.Value))
			//{
			//    return Redirect(ReceptionHelper.Authenticate(DeviceType.Desktop));
			//}

			//FormsAuthenticationTicket authenticationTicket = FormsAuthentication.Decrypt(authCookie.Value);

			//if (authenticationTicket == null)
			//{
			//    return Redirect(ReceptionHelper.Authenticate(DeviceType.Desktop));
			//}

			//if (authenticationTicket.Expired)
			//{
			//    return Redirect(ReceptionHelper.Authenticate(DeviceType.Desktop));
			//}
			if (!Request.IsAuthenticated)
			{
				HttpContext.GetOwinContext().Authentication.Challenge(
					new AuthenticationProperties { RedirectUri = "/" },
					OpenIdConnectAuthenticationDefaults.AuthenticationType);
			}
			if (User.IsInRole("Receptionist"))
			{
				return View();
			}
			else
			{
				return Content("<html><p><i>Hello! You don't have permission to view this page.</u></i></p></html>", "text/html");
			}

			
		}

        //[ReceptionAuthentication("Admin", "PMO", "Receptionist")]
		public ActionResult Dashboard()
		{
			return View();
		}

        //[ReceptionAuthentication("Admin", "PMO", "Receptionist")]
		public ActionResult History()
		{
			return View();
		}

        //[ReceptionAuthentication("Admin", "PMO", "Receptionist")]
		public ActionResult EmployeeSearch()
		{
			return View();
		}

        //[ReceptionAuthentication("Admin", "PMO", "Receptionist")]
		public ActionResult EmployeeDetails()
		{
			return View();
		}

        //[ReceptionAuthentication("Admin", "PMO", "Receptionist")]
		public ActionResult VisitorDetails(string deviceType)
		{
			ViewBag.deviceType = deviceType;
			return View();
		}

        //[ReceptionAuthentication("Admin", "PMO", "Receptionist")]
		public ActionResult VisitorBasicDetails(string deviceType)
		{
			ViewBag.deviceType = deviceType;
			return View();
		}

        //[ReceptionAuthentication("Admin", "PMO", "Receptionist")]
		public ActionResult VisitorAdvancedDetails(string deviceType)
		{
			return View();
		}

        //[ReceptionAuthentication("Admin", "PMO", "Receptionist")]
		public ActionResult VisitInformation()
		{
			return View();
		}

        //[ReceptionAuthentication("Admin", "PMO", "Receptionist")]
		public ActionResult UnderConstruction()
		{
			return View();
		}

        //[ReceptionAuthentication("Admin", "PMO", "Receptionist")]
		public ActionResult ViewAllCardHistory()
		{
			return View();
		}

        public ActionResult CallBack(string code)
        {
            var data = new StringBuilder();

            data.Append("code=" + code);
            data.Append("&clientId=" + Uri.EscapeDataString(ReceptionHelper.Base64Encode(ConfigurationManager.AppSettings["ClientId"])));
            data.Append("&clientSecret=" + Uri.EscapeDataString(ReceptionHelper.Base64Encode(ConfigurationManager.AppSettings["ClientSecret"])));
            data.Append("&redirectUri=" + "");

            ReceptionHelper.HttpPost(data, ConfigurationManager.AppSettings["TokenkUrl"], DeviceType.Desktop);

            return Redirect(ConfigurationManager.AppSettings["DefaultUrl"] + "/reception");
        }

		public ActionResult LogOut()
		{
			ReceptionHelper.DeleteCookie(CookieName);

			var redirectUrl = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/Areas/Reception/Web.config")
								.AppSettings.Settings["DefaultUrl"].Value;

			return Redirect(redirectUrl);
		}
	}
}
