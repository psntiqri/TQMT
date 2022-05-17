using System;
using System.Configuration;
using System.Web.Http;
using System.Web.Mvc;

namespace Exilesoft.MyTime.Areas.Reception
{
    public class ReceptionAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Reception";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.Routes.MapHttpRoute(
            name: "Reception_defaultApi",
            routeTemplate: "Reception/api/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional });

            var actionPage = "Index";
            var enableMyTimeReception = ConfigurationManager.AppSettings["EnableMyTimeReceptionModule"];

            if (enableMyTimeReception == null) throw new ArgumentNullException("EnableMyTimeReceptionModule");
            if (enableMyTimeReception == "0")
            {
                actionPage = "UnderConstruction";
            }

            context.MapRoute(
                name: "Reception_default",
                url: "Reception/{controller}/{action}/{id}",
                defaults: new { controller = "ReceptionHome", action = actionPage, id = UrlParameter.Optional }
            );
            //new { action = "Index", id = UrlParameter.Optional }
        }
    }
}
