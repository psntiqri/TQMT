using System.Configuration;
using System.Web;
using System.Web.Security;

namespace Exilesoft.MyTime.Helpers
{
    public class CookieHelper
    {
        public static string CookieName
        {
            get { return ConfigurationManager.AppSettings["LoginCoockieName"]; }
        }

        public static void SetCookie(string userName, int? employeeId, object[] roles)
        {
            string rolesForUser = string.Join("|", roles);
            string userData = string.Format("{0},{1}", employeeId, rolesForUser);
            SetCookie(userName, userData);
        }

        public static void SetCookie(string userName, string userData)
        {
            var authentiacationTicket = new FormsAuthenticationTicket(1, userName, Utility.GetDateTimeNow(), Utility.GetDateTimeNow().AddMinutes(60), false, userData);
            string encriptedTicket = FormsAuthentication.Encrypt(authentiacationTicket);
            var loginCookie = new HttpCookie(CookieName, encriptedTicket);
            HttpContext.Current.Response.SetCookie(loginCookie);
            HttpContext.Current.Response.Cookies.Add(loginCookie);
        }

        public static void DeleteCookie(string cookieName)
        {
            var httpCookie = HttpContext.Current.Request.Cookies[CookieName];

            if (httpCookie != null && !string.IsNullOrEmpty(httpCookie.Value))
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(httpCookie.Value);
                
                if (ticket != null)
                {
                    var newAuthentiacationTicket = new FormsAuthenticationTicket(1, ticket.Name,
                        Utility.GetDateTimeNow().AddDays(-1), Utility.GetDateTimeNow().AddDays(-1).AddMinutes(60), false, ticket.UserData);
                    string encriptedTicket = FormsAuthentication.Encrypt(newAuthentiacationTicket);
                    var loginCookie = new HttpCookie(CookieName, encriptedTicket);
                    HttpContext.Current.Response.SetCookie(loginCookie);
                    HttpContext.Current.Response.Cookies.Add(loginCookie);
                }
            }
        }
    }
}