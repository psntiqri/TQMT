using Exilesoft.MyTime.Filters;
using Hangfire;
using Microsoft.Owin;
using Owin;
using System.Configuration;
using System.Web;

using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Notifications;
using System;
using System.Threading.Tasks;
using Microsoft.Owin.Host.SystemWeb;

[assembly: OwinStartup(typeof(Exilesoft.MyTime.Startup))]
namespace Exilesoft.MyTime
{

    public class Startup
    {
        // The Client ID is used by the application to uniquely identify itself to Microsoft identity platform.
        string clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];

        // RedirectUri is the URL where the user will be redirected to after they sign in.
        string redirectUri = System.Configuration.ConfigurationManager.AppSettings["redirectUri"];

        // Tenant is the tenant ID (e.g. contoso.onmicrosoft.com, or 'common' for multi-tenant)
        static string tenant = System.Configuration.ConfigurationManager.AppSettings["Tenant"];

        // Authority is the URL for authority, composed of the Microsoft identity platform and the tenant name (e.g. https://login.microsoftonline.com/contoso.onmicrosoft.com/v2.0)
        //string authority = String.Format(System.Globalization.CultureInfo.InvariantCulture, System.Configuration.ConfigurationManager.AppSettings["Authority"], tenant);
        string authority = "https://login.microsoftonline.com/"+tenant+"/v2.0";

        /// <summary>
        /// Configure OWIN to use OpenIdConnect
        /// </summary>
        /// <param name="app"></param>
        /// 
        public void Configuration(IAppBuilder app)
        {            
            
            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                CookieManager = new SystemWebCookieManager(),
                CookieHttpOnly = true,
                // We are expecting a different site to POST back to us,
                // so the ASP.Net Core default of Lax is not appropriate in this case
                CookieSameSite = SameSiteMode.None,
            });
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    // Sets the ClientId, authority, RedirectUri as obtained from web.config
                    ClientId = ConfigurationManager.AppSettings["ClientId"],
                    Authority = "https://login.microsoftonline.com/" + ConfigurationManager.AppSettings["Tenant"] + "/v2.0",
                    RedirectUri = ConfigurationManager.AppSettings["redirectUri"],
                    // PostLogoutRedirectUri is the page that users will be redirected to after sign-out. In this case, it's using the home page
                    PostLogoutRedirectUri = ConfigurationManager.AppSettings["redirectUri"],
                    Scope = OpenIdConnectScope.OpenIdProfile,
                    // ResponseType is set to request the code id_token, which contains basic information about the signed-in user
                    ResponseType = OpenIdConnectResponseType.CodeIdToken,
                    // ValidateIssuer set to false to allow personal and work accounts from any organization to sign in to your application
                    // To only allow users from a single organization, set ValidateIssuer to true and the 'tenant' setting in Web.config to the tenant name
                    // To allow users from only a list of specific organizations, set ValidateIssuer to true and use the ValidIssuers parameter
                    TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true, // Simplification (see note below)
                        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                        NameClaimType = "name"
                    },
                    // OpenIdConnectAuthenticationNotifications configures OWIN to send notification of failed authentications to the OnAuthenticationFailed method
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthenticationFailed = OnAuthenticationFailed
                    }
                }
            );
            app.MapSignalR();
        }

        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        {
            context.HandleResponse();
            context.Response.Redirect("/Home/Index");
            return Task.FromResult(0);
        }
    }
}