using System;
using System.Configuration;
using System.Web.Http;
using System.Web.Mvc;

namespace Exilesoft.MyTime.Areas.AmexSecure
{
    public class AmexSecureAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "AmexSecure";
            }
        }


        public static string CookieName
        {
            get
            {
                return System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/Areas/AmexSecure/Web.config")
                        .AppSettings.Settings["AmexDesktopCoockieName"].Value;
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.Routes.MapHttpRoute(
            name: "AmexSecure_defaultApi",
            routeTemplate: "AmexSecure/api/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional });

            var actionPage = "Index";
            var enableMyTimeReception = ConfigurationManager.AppSettings["EnableMyTimeAmexModule"];

            if (enableMyTimeReception == null) throw new ArgumentNullException("EnableMyTimeAmexModule");
            if (enableMyTimeReception == "0")
            {
                actionPage = "UnderConstruction";
            }

            context.MapRoute(
                name: "AmexSecure_default",
                url: "AmexSecure/{controller}/{action}/{id}",
                defaults: new { controller = "AmexHome", action = actionPage, id = UrlParameter.Optional }
            );
        }
    }
}
