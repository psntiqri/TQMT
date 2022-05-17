using Exilesoft.MyTime.Areas.Amex.Common;
using Exilesoft.MyTime.Areas.AmexSecure.Filters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Exilesoft.MyTime.Areas.AmexSecure.Controllers
{
    public class AmexHomeController : Controller
    {

        public static string CookieName
        {
            get
            {
                return System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/Areas/AmexSecure/Web.config")
                 .AppSettings.Settings["AmexDesktopCoockieName"].Value;                  
              
            }
        }
        // GET: /AmexSecure/Amex/

        public ActionResult Index()
        {
            var authCookie = System.Web.HttpContext.Current.Request.Cookies.Get(CookieName);

            if (authCookie == null || string.IsNullOrEmpty(authCookie.Value))
            {
                return Redirect(AmexHelper.Authenticate(DeviceType.Desktop));
            }

            FormsAuthenticationTicket authenticationTicket = FormsAuthentication.Decrypt(authCookie.Value);

            if (authenticationTicket == null)
            {
                return Redirect(AmexHelper.Authenticate(DeviceType.Desktop));
            }

            if (authenticationTicket.Expired)
            {
                return Redirect(AmexHelper.Authenticate(DeviceType.Desktop));
            }

            return RedirectToAction("Index", "Amex");
        }
        
        public ActionResult CallBack(string code)
        {
            var data = new StringBuilder();

            data.Append("code=" + code);
            data.Append("&clientId=" + Uri.EscapeDataString(AmexHelper.Base64Encode(ConfigurationManager.AppSettings["ClientId"])));
            data.Append("&clientSecret=" + Uri.EscapeDataString(AmexHelper.Base64Encode(ConfigurationManager.AppSettings["ClientSecret"])));
            data.Append("&redirectUri=" + "");

            AmexHelper.HttpPost(data, ConfigurationManager.AppSettings["TokenkUrl"], DeviceType.Desktop);

            return Redirect(ConfigurationManager.AppSettings["DefaultUrl"] + "/amexsecure");
            //return RedirectToAction("Index", "Amex");
        }

        public ActionResult LogOut()
        {
            AmexHelper.DeleteCookie(CookieName);

            var redirectUrl = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/Areas/amexsecure/Web.config")
                                .AppSettings.Settings["DefaultUrl"].Value;

            return Redirect(redirectUrl);
        }
    }
}
