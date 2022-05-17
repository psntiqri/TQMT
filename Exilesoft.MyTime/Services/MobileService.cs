using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using Exilesoft.Models;

namespace Exilesoft.MyTime.Services
{
    public class MobileService
    {
        internal static UserAuthModel AuthenticateMobileUser(string userName, string password)
        {
            var webClient = new WebClient();
            webClient.Headers.Add(ConfigurationManager.AppSettings["ClientIdToBeVerified"], ConfigurationManager.AppSettings["ClientId"]);
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            
            byte[] response = webClient.UploadValues(string.Format(ConfigurationManager.AppSettings["SSOService"]), "POST", new NameValueCollection()
            {
                { "UserName", userName },
                { "Password", password }
            });

            var jsonSerializer = new JavaScriptSerializer();
            var userAuthModel = (UserAuthModel)jsonSerializer.Deserialize(Encoding.ASCII.GetString(response), typeof(UserAuthModel));
            
            return userAuthModel;
        }

        internal static string getUserImage(int id)
        {
            id = 219;
            
            var webClient = new System.Net.WebClient();
            webClient.Headers.Add(ConfigurationManager.AppSettings["ClientIdToBeVerified"], ConfigurationManager.AppSettings["ClientId"]);

            var url = "https://people.tiqri.com/api/api/Employee/GetMyTimeEmployeeImageById?id={0}";

            string address = string.Format(url, id);

            string data = webClient.DownloadString(address);
            var jsonSerializer = new JavaScriptSerializer();

            return data.Replace('"', ' '); 
            
        }


    }
}