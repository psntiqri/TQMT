using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Script.Serialization;
using Exilesoft.MyTime.Areas.Reception.Models;
using Exilesoft.MyTime.Areas.Reception.ViewModels;
using Microsoft.AspNet.SignalR;


namespace Exilesoft.MyTime.Areas.Reception.Services
{
    public class MyTimeReceptionHub : Hub
    {
        public void PushVisitDetails(VisitorVisitModel visitorVisitModel)
        {
            //Clients.All.hello();
            Clients.All.broadcastbroadVisitDetails(new JavaScriptSerializer().Serialize(visitorVisitModel));
        }
    }
}