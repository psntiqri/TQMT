using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Script.Serialization;

namespace Exilesoft.MyTime.Filters
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ApplicationAuthenticationAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
           
            var clientIdToBeVerified = HttpContext.Current.Request.Headers["ClientIdToBeVerified"];

            var data = new StringBuilder();

            data.Append("requesterClientId=" + Uri.EscapeDataString(Base64Encode(ConfigurationManager.AppSettings["ClientId"])));
            data.Append("&clientIdToBeVerified=" + Uri.EscapeDataString(Base64Encode(clientIdToBeVerified)));

            var result = HttpsPost(data, ConfigurationManager.AppSettings["VerifyRequest"]);

            if (!result)
            {
                HandleUnotherizeResponce(actionContext, HttpStatusCode.Unauthorized, "Invalid Token");
            }
        }

        private void HandleUnotherizeResponce(HttpActionContext actionContext, HttpStatusCode status, string message)
        {
            actionContext.Response = new HttpResponseMessage(status)
            {
                Content = new StringContent(message)
            };
            HttpContext.Current.User = null;
        }

        private bool HttpsPost(StringBuilder data, string url)
        {
            HttpWebResponse response = null;

            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(data.ToString());

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;

                Stream postStream = request.GetRequestStream();
                postStream.Write(byteArray, 0, byteArray.Length);
                postStream.Close();

                response = (HttpWebResponse)request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string json = reader.ReadToEnd();
                    Console.WriteLine(json);

                    var ser = new JavaScriptSerializer();
                    var x = (bool)ser.DeserializeObject(json);

                    if (x != null)
                    {
                        return x;
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

            return false;
        }

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
    }
}