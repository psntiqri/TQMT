using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Security;
using Exilesoft.Models;
using Exilesoft.MyTime.Repositories;
using Exilesoft.MyTime.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using System.Net.Mail;
using System.IO;
using System.Text;
using System.Net;


namespace Exilesoft.MyTime.Controllers
{
    public class LogInController : ApiController
    {
        //DbContext
        private Context db = new Context();
        private Guid guid;

        public class CurrentUser
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        public class OtherUser
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        // GET api/login?username=username&password=password

        //public object Get(string username, string password)
        //{
        //    string sessionId = string.Empty;
        //    string state = "0";
        //    int userId = 0;
        //    string userName = string.Empty;
        //    int userPriv = 0;

        //    var userAuth = MobileService.AuthenticateMobileUser(username, password);
        //    if (userAuth.IsAuthenticated)
        //    {

        //        state = "1";
        //        EmployeeEnrollment emp = db.EmployeeEnrollment.First(a => a.UserName == username);
        //        userId = emp.EmployeeId;
        //        userName = emp.UserName;//TODO: Take the data from HR web API once it done
        //        userPriv = userAuth.Privilage;
        //        guid = Guid.NewGuid();
        //        sessionId = guid.ToString();
        //        emp.MobileId = guid;
        //        emp.Privillage=userAuth.Privilage;
        //        db.SaveChanges();
        //    }

        //    return new { state = state, sessionId = sessionId, id = userId, name = userName, priv = userPriv };

        //}

        public HttpResponseMessage Get(string username, string password)
        {

            string sessionId = string.Empty;
            string state = "0";
            int userId = 0;
            string userName = string.Empty;
            int userPriv = 0;

            var userAuth = MobileService.AuthenticateMobileUser(username, password);

            if (userAuth.IsAuthenticated)
            {
                state = "1";
                EmployeeEnrollment emp = db.EmployeeEnrollment.First(a => a.EmployeeId == userAuth.EmployeeId);
                userId = emp.EmployeeId;
                userName = emp.UserName;
                userPriv = userAuth.Privilage;
                guid = Guid.NewGuid();
                sessionId = guid.ToString();
                emp.MobileId = guid;
                emp.Privillage = userAuth.Privilage;
                db.SaveChanges();
            }

            var resp = new HttpResponseMessage()
            {

                Content = new StringContent("{\"state\":\"" + state + "\",\"sessionId\":\"" + sessionId + "\",\"id\":\"" + userId + "\",\"name\":\"" + userName + "\",\"priv\":\"" + userPriv + "\"}")

            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;
        }

        public void GetExpired(string eXpSSID)
        {
            Guid guid = Guid.Parse(eXpSSID);
            EmployeeEnrollment emp = db.EmployeeEnrollment.FirstOrDefault(a => a.MobileId == guid);
            if (emp != null)
            {
                emp.MobileId = new Guid();
                db.SaveChanges();
            }
        }
        public IEnumerable<OtherUser> Get(string sessionId)
        {
            List<OtherUser> userList = new List<OtherUser>();
            try
            {
                guid = Guid.Parse(sessionId);
                var emp = db.EmployeeEnrollment.FirstOrDefault(a => a.MobileId == guid);

                if (emp != null)
                {
                    var employees = EmployeeRepository.GetAllEmployees().Select(a => new { a.Id, a.Name }); //db.Employees.Select(a => new { a.Id, a.Name });
                    foreach (var item in employees)
                    {
                        userList.Add(new OtherUser() { Id = item.Id, Name = item.Name });
                    }
                }
            }
            catch (Exception)
            {
            }
            return userList;
        }

        //GET api/login?sessionId1=sessionId
        public IEnumerable<OtherUser> UserList(string sessionId)
        {
            List<OtherUser> userList = new List<OtherUser>();
            //if (sessionId == HttpContext.Current.Session.SessionID)
            //{
            //    var employees = EmployeeRepository.GetAllEmployees().Select(a => new { a.Id, a.Name });
            //    foreach (var item in employees)
            //    {
            //        userList.Add(new OtherUser() { Id = item.Id, Name = item.Name });
            //    }
            //}
            return userList;
        }
        //GET api/login/GetApprovedAttendec?Key=Key
        public HttpResponseMessage GetApprovedAttendece(Guid Key)
        {

            HttpResponseMessage msg = new HttpResponseMessage();
            string Val = string.Empty;
            PendingAttendance pendingAttendence = db.PendingAttendances.Where(a => a.ApproveKey == Key).FirstOrDefault();
            EmployeeEnrollment loogedEmployee = db.EmployeeEnrollment.FirstOrDefault(a => a.EmployeeId == pendingAttendence.EmployeeId);
            Location location = db.Locations.Find(27);
            var attendace = new Attendance();


            string url = string.Empty;
            if (pendingAttendence.ApproveType == 1)
            {
                url = ConfigurationManager.AppSettings["DefaultUrl"] + "/AttendenceAllReadyConfirmed.html";

            }
            else
            {
                using (var transaction = db.Database.BeginTransaction())
                {

                    attendace.Year = pendingAttendence.Year;
                    attendace.Month = pendingAttendence.Month;
                    attendace.Day = pendingAttendence.Day;
                    attendace.Hour = pendingAttendence.InHour;
                    attendace.Minute = pendingAttendence.InMinute;
                    attendace.Second = pendingAttendence.InSecond;

                    attendace.Employee = loogedEmployee;
                    attendace.EmployeeId = loogedEmployee.EmployeeId;
                    attendace.CardNo = loogedEmployee.CardNo;

                    attendace.Location = location;
                    attendace.LocationId = location.Id;
                    attendace.InOutMode = "in";
                    db.Attendances.Add(attendace);

                    attendace = new Attendance();

                    attendace.Year = pendingAttendence.Year;
                    attendace.Month = pendingAttendence.Month;
                    attendace.Day = pendingAttendence.Day;
                    attendace.Hour = pendingAttendence.OutHour;
                    attendace.Minute = pendingAttendence.OutMinute;
                    attendace.Second = pendingAttendence.OutSecond;
                    attendace.Employee = loogedEmployee;
                    attendace.EmployeeId = loogedEmployee.EmployeeId;
                    attendace.CardNo = loogedEmployee.CardNo;
                    attendace.Location = location;
                    attendace.LocationId = location.Id;
                    attendace.InOutMode = "out";
                    db.Attendances.Add(attendace);

                    pendingAttendence.ApproveType = 1;


                    try
                    {
                        db.SaveChanges();
                        transaction.Commit();
                        msg = SendConfirmWorkingFromHomemail(new DateTime(pendingAttendence.Year, pendingAttendence.Month, pendingAttendence.Day), pendingAttendence.EmployeeId);
                        url = ConfigurationManager.AppSettings["DefaultUrl"] + "/AttendenceConfirmed.html";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                    }
                }
            }

            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(url);
            return response;

        }
        //GET api/login/GetRejectedAttendece?Key=Key
        public HttpResponseMessage GetRejectedAttendece(Guid Key)
        {
            HttpResponseMessage msg = new HttpResponseMessage();
            PendingAttendance pendingAttendence = db.PendingAttendances.Where(a => a.ApproveKey == Key).FirstOrDefault();
            string url = string.Empty;
            if (pendingAttendence != null)
            {

                IList<Attendance> ListexistAtendence = db.Attendances.Where(a => a.Year == pendingAttendence.Year
                    && a.Month == pendingAttendence.Month && a.Day == pendingAttendence.Day
                    && a.EmployeeId == pendingAttendence.EmployeeId).ToList();
                if (pendingAttendence.ApproveType == 2)
                {
                    url = ConfigurationManager.AppSettings["DefaultUrl"] + "/AttendenceAllReadyRejected.html";

                }
                else
                {

                    using (var transaction = db.Database.BeginTransaction())
                    {
                        pendingAttendence.ApproveType = 2;


                        try
                        {

                            foreach (var item in ListexistAtendence)
                            {
                                db.Attendances.Remove(item);
                            }
                            db.SaveChanges();
                            transaction.Commit();
                            msg = SendRejectmWorkingFromHomemail(new DateTime(pendingAttendence.Year, pendingAttendence.Month, pendingAttendence.Day), pendingAttendence.EmployeeId);
                            url = ConfigurationManager.AppSettings["DefaultUrl"] + "/AttendenceRejected.html";
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                        }
                    }
                }


                var response = Request.CreateResponse(HttpStatusCode.Moved);
                response.Headers.Location = new Uri(url);
                return response;

            }
            return null;
        }
        public HttpResponseMessage SendConfirmWorkingFromHomemail(DateTime FromeDate, int employeeId)
        {

            var host = ConfigurationManager.AppSettings["MailHost"];
            var port = ConfigurationManager.AppSettings["MailPort"];
            var fromAddress = ConfigurationManager.AppSettings["FromAddress"];
            var Username = ConfigurationManager.AppSettings["MailUserName"];
            var password = ConfigurationManager.AppSettings["MailPassword"];
            var CCAddress = ConfigurationManager.AppSettings["CommentsCCTo"];

            EmployeeData _loggedUser = EmployeeRepository.GetEmployee(employeeId);



            var message = "This is to inform that " + _loggedUser.Name + " worked at home on " + FromeDate.ToString("yyyy-MM-dd") + " is Confirmed" + "<br><br><br><br>";



            var smtpClient = new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,
                Host = host,
                Port = Convert.ToInt32(port)
            };

            var credentials = new System.Net.NetworkCredential(Username, password);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = credentials;


            var mail = new MailMessage
            {
                IsBodyHtml = true,
                Body = message,
                Subject = "Working From Home - " + _loggedUser.Name + " on " + FromeDate.ToString("yyyy-MM-dd") + " is Confirmed",
                From = new MailAddress(fromAddress, "ITS")

            };


            mail.To.Add(new MailAddress(_loggedUser.PrimaryEmailAddress));
            string _message = "";
            try
            {
                smtpClient.Send(mail);
                _message = "Success";
            }
            catch (Exception Ex)
            {
                _message = "Error occurred while sending your mail.";
                throw;
            }



            var resp = new HttpResponseMessage()
            {

                Content = new StringContent(_message)

            };

            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return resp;

        }
        public HttpResponseMessage SendRejectmWorkingFromHomemail(DateTime FromeDate, int employeeId)
        {

            var host = ConfigurationManager.AppSettings["MailHost"];
            var port = ConfigurationManager.AppSettings["MailPort"];
            var fromAddress = ConfigurationManager.AppSettings["FromAddress"];
            var Username = ConfigurationManager.AppSettings["MailUserName"];
            var password = ConfigurationManager.AppSettings["MailPassword"];
            var CCAddress = ConfigurationManager.AppSettings["CommentsCCTo"];

            EmployeeData _loggedUser = EmployeeRepository.GetEmployee(employeeId);



            var message = "This is to inform that " + _loggedUser.Name + " worked at home on " + FromeDate.ToString("yyyy-MM-dd") + " is rejected."
                + "<br><br>" + "Please contact your Project supervisor." + "<br><br><br><br>";


            var smtpClient = new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,
                Host = host,
                Port = Convert.ToInt32(port)
            };

            var credentials = new System.Net.NetworkCredential(Username, password);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = credentials;


            var mail = new MailMessage
            {
                IsBodyHtml = true,
                Body = message,
                Subject = "Working From Home - " + _loggedUser.Name + " on " + FromeDate.ToString("yyyy-MM-dd") + " is Rejected",
                From = new MailAddress(fromAddress, "ITS")

            };


            mail.To.Add(new MailAddress(_loggedUser.PrimaryEmailAddress));
            string _message = "";
            try
            {
                smtpClient.Send(mail);
                _message = "Rejected";
            }
            catch (Exception Ex)
            {
                _message = "Error occurred while sending your mail.";
                throw;
            }

            var resp = new HttpResponseMessage()
            {

                Content = new StringContent(_message)

            };

            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return resp; ;

        }


        public HttpResponseMessage Post([FromBody]CurrentUser user)
        {
            string sessionId = string.Empty;
            string state = "0";
            int userId = 0;
            string userName = string.Empty;
            int userPriv = 0;
            var userAuth = MobileService.AuthenticateMobileUser(user.username, user.password);

            if (userAuth.IsAuthenticated)
            {
                state = "1";
                EmployeeEnrollment emp = db.EmployeeEnrollment.First(a => a.EmployeeId == userAuth.EmployeeId);
                userId = emp.EmployeeId;
                userName = emp.UserName;
                userPriv = userAuth.Privilage; ;
                guid = Guid.NewGuid();
                sessionId = guid.ToString();

                emp.Privillage = userAuth.Privilage;
                emp.MobileId = guid;
                db.SaveChanges();
            }

            var resp = new HttpResponseMessage()
            {
                Content = new StringContent("{\"state\":\"" + state + "\",\"sessionId\":\"" + sessionId + "\",\"id\":\"" + userId + "\",\"name\":\"" + userName + "\",\"priv\":\"" + userPriv + "\"}")
            };

            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
