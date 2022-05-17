using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using AuthorizeAttribute = System.Web.Mvc.AuthorizeAttribute;

namespace Exilesoft.MyTime
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<Context, Migrations.Configuration>());

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            System.Net.ServicePointManager.SecurityProtocol =
            SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            // GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            // GlobalConfiguration.Configuration.Formatters.Remove(GlobalConfiguration.Configuration.Formatters.XmlFormatter);
        }

        //protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        //{
        //    HttpCookie authCookie =
        //        Context.Request.Cookies[FormsAuthentication.FormsCookieName];
        //    if (authCookie != null)
        //    {
        //        FormsAuthenticationTicket authTicket =
        //            FormsAuthentication.Decrypt(authCookie.Value);
        //        string[] roles = authTicket.UserData.Split(new Char[] { ',' });
        //        GenericPrincipal userPrincipal =
        //            new GenericPrincipal(new GenericIdentity(authTicket.Name), roles);
        //        Context.User = userPrincipal;
        //    }
        //}

        //protected class RolesAttribute : AuthorizeAttribute
        //{
        //    public RolesAttribute(params string[] roles)
        //    {
        //        Roles = String.Join(",", roles);
        //    }
        //}
    }
}