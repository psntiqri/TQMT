using System;
using System.Configuration;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using Exilesoft.MyTime.Areas.Reception.Common;
using Exilesoft.MyTime.Areas.Reception.Filters;

namespace Exilesoft.MyTime.Areas.Reception.Controllers
{
    public class MobileController : Controller
    {
        public static string CookieName
        {
            get
            {
                return System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/Areas/Reception/Web.config")
                        .AppSettings.Settings["ReceptionMobileCoockieName"].Value;
            }
        }

        //[ReceptionMobileAuthentication("Admin", "PMO", "Receptionist")]
        public ActionResult Index()
        {
            var authCookie = System.Web.HttpContext.Current.Request.Cookies.Get(CookieName);

            if (authCookie == null || string.IsNullOrEmpty(authCookie.Value))
            {
                return Redirect(ReceptionHelper.Authenticate(DeviceType.Mobile));
            }

            FormsAuthenticationTicket authenticationTicket = FormsAuthentication.Decrypt(authCookie.Value);

            if (authenticationTicket == null)
            {
                return Redirect(ReceptionHelper.Authenticate(DeviceType.Mobile));
            }

            if (authenticationTicket.Expired)
            {
                return Redirect(ReceptionHelper.Authenticate(DeviceType.Mobile));
            }

            return View();
        }

        public ActionResult CallBack(string code)
        {
            var data = new StringBuilder();

            data.Append("code=" + code);
            data.Append("&clientId=" + Uri.EscapeDataString(ReceptionHelper.Base64Encode(ConfigurationManager.AppSettings["ClientId"])));
            data.Append("&clientSecret=" + Uri.EscapeDataString(ReceptionHelper.Base64Encode(ConfigurationManager.AppSettings["ClientSecret"])));
            data.Append("&redirectUri=" + "");

            ReceptionHelper.HttpPost(data, ConfigurationManager.AppSettings["TokenkUrl"], DeviceType.Mobile);

            return Redirect(ConfigurationManager.AppSettings["DefaultUrl"] + "reception/mobile");
        }

        public ActionResult Visitor()
        {
            return View();
        }

        public ActionResult Employee()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            return View();
        }
    }
}
