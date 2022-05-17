using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using Exilesoft.MyTime.Helpers;

namespace Exilesoft.MyTime.Areas.Reception.Common
{
    public class ReceptionHelper
    {
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static void HttpPost(StringBuilder data, string url, DeviceType type)
        {
            HttpWebResponse response = null;

            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(data.ToString());

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;
                request.ClientCertificates.Add(new X509Certificate());

                Stream postStream = request.GetRequestStream();
                postStream.Write(byteArray, 0, byteArray.Length);
                postStream.Close();

                response = (HttpWebResponse)request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string json = reader.ReadToEnd();
                    Console.WriteLine(json);

                    var ser = new JavaScriptSerializer();
                    var x = (Dictionary<string, object>)ser.DeserializeObject(json);

                    if (x != null)
                    {
                        var idToken = x["IdToken"] as string;
                        string[] id_token_payload = idToken.Split('.');
                        var xx = (Dictionary<string, object>)ser.DeserializeObject(Base64Decode(id_token_payload[1]));

                        var coockieName = GetCoockieName(type);

	                    SetCookie(xx["email"].ToString(), Convert.ToInt32(xx["employeeId"]), xx["roles"] as object[],
	                              coockieName, type);
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    using (var err = (HttpWebResponse)e.Response)
                    {
                        Console.WriteLine("The server returned '{0}' with the status code '{1} ({2:d})'.",
                          err.StatusDescription, err.StatusCode, err.StatusCode);
                    }
                }
            }
            finally
            {
                if (response != null) { response.Close(); }
            }
        }

        private static string GetCoockieName(DeviceType type)
        {
            string coockieName;

            if (type == DeviceType.Desktop)
            {
                coockieName =
                    System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(
                        "/Areas/Reception/Web.config")
                        .AppSettings.Settings["ReceptionDesktopCoockieName"].Value;
            }
            else
            {
                coockieName =
                    System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(
                        "/Areas/Reception/Web.config")
                        .AppSettings.Settings["ReceptionMobileCoockieName"].Value;
            }
            return coockieName;
        }

        public static string Authenticate(DeviceType type)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["ClientId"]);
            var clientId = Convert.ToBase64String(plainTextBytes);

            var authUrl = ConfigurationManager.AppSettings["AuthUrl"];

            var callbackUrl = GetCallbackUrl(type);

            var url = string.Format("{0}clientId={1}&responseType=code&redirectUri={2}", authUrl,
                Uri.EscapeDataString(clientId), callbackUrl);

            return url;
        }

        private static string GetCallbackUrl(DeviceType type)
        {
            string callbackUrl;

            if (type == DeviceType.Desktop)
                callbackUrl =
                    System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/Areas/Reception/Web.config")
                        .AppSettings.Settings["ReceptionCallBackUrl"].Value;
            else
            {
                callbackUrl =
                    System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/Areas/Reception/Web.config")
                        .AppSettings.Settings["MobileCallBackUrl"].Value;
            }
            return callbackUrl;
        }
		
        public static void SetCookie(string userName, int? employeeId, object[] roles, string coockieName, DeviceType type)
        {
			string rolesForUser = string.Join("|", roles);
			string userData = string.Format("{0},{1}", employeeId, rolesForUser);
	        DateTime expiration;
	        if (type == DeviceType.Desktop)
		        expiration = DateTime.Now.AddMinutes(60).ToLocalTime();
	        else
	        {
				expiration = DateTime.Now.AddYears(10).ToLocalTime();
	        }

			var authentiacationTicket = new FormsAuthenticationTicket(1, userName,  DateTime.Now.ToLocalTime(), expiration, false, userData);
            string encriptedTicket = FormsAuthentication.Encrypt(authentiacationTicket);
            var loginCookie = new HttpCookie(coockieName, encriptedTicket);
            HttpContext.Current.Response.SetCookie(loginCookie);
            HttpContext.Current.Response.Cookies.Add(loginCookie);
        }

        public static void DeleteCookie(string cookieName)
        {
            var httpCookie = HttpContext.Current.Request.Cookies[cookieName];

            if (httpCookie != null && !string.IsNullOrEmpty(httpCookie.Value))
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(httpCookie.Value);

                if (ticket != null)
                {
					var newAuthentiacationTicket = new FormsAuthenticationTicket(1, ticket.Name,
						Utility.GetDateTimeNow().AddDays(-1), Utility.GetDateTimeNow().AddDays(-1).AddMinutes(60), false, ticket.UserData);
					string encriptedTicket = FormsAuthentication.Encrypt(newAuthentiacationTicket);
					var loginCookie = new HttpCookie(cookieName, encriptedTicket);
					HttpContext.Current.Response.SetCookie(loginCookie);
					HttpContext.Current.Response.Cookies.Add(loginCookie);
				}
            }
        }
    }

    public enum DeviceType
    {
        Desktop = 1,
        Mobile = 2
    }
}