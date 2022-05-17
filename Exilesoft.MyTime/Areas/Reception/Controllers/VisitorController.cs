using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Permissions;
using System.Threading;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Routing;
using System.Web.Script.Serialization;
using Exilesoft.Models;
using Exilesoft.MyTime.Areas.Reception.Models;
using Exilesoft.MyTime.Areas.Reception.Services;
using Exilesoft.MyTime.Areas.Reception.ViewModels;
using Exilesoft.MyTime.Helpers;
using Exilesoft.MyTime.Repositories;
using Microsoft.AspNet.SignalR;

namespace Exilesoft.MyTime.Areas.Reception.Controllers
{
    public class VisitorController : ApiController
    {
        // GET api/visitor
        //public IEnumerable<Visitor> Get(string term)
        //{
        //    IEnumerable<Visitor> visitors = from v in VisitorRepository.GetVisitors()
        //        where term != null && v.Name.ToLower().Contains(term.ToLower())
        //        select v;

        //    return visitors;

        //}
        private readonly IVisitorRepository _visitorRepository;
        private readonly IVisitRepository _visitRepository;
        private readonly IVisitorPassAllocationRepository _visitorPassAllocation;
        private IVisitorPassAllocationRepository visitorPassAllocationRepository;
        private AttendanceRepository attendanceRepository;

        public VisitorController()
        {
            _visitorRepository = new VisitorRepository();
            _visitRepository = new VisitRepository();
            _visitorPassAllocation = new VisitorPassAllocationRepository();
            visitorPassAllocationRepository= new VisitorPassAllocationRepository();
            attendanceRepository= new AttendanceRepository();
        }

        public VisitorController(IVisitorRepository visitorRepository, IVisitRepository visitRepository, IVisitorPassAllocationRepository visitorPassAllocation)
        {
            _visitorRepository = visitorRepository;
            _visitRepository = visitRepository;
            _visitorPassAllocation = visitorPassAllocation;
        }

        //public IEnumerable<Visitor> Get()
        //{
        //    var visitors = from v in _visitorRepository.GetVisitors()
        //                   select v;
        //    return visitors;
        //}

        public Visitor GetByMobileNo(string mobileNo)
        {
            if (string.IsNullOrWhiteSpace(mobileNo))
                return null;
            var visitor = _visitorRepository.GetVisitorByMobileNo(mobileNo);
            return visitor;
        }

        public Visitor GetByIdNo(string identityNo)
        {
            if (string.IsNullOrWhiteSpace(identityNo))
                return null;
            var visitor = _visitorRepository.GetVisitorByIdentityNo(identityNo);
            return visitor;
        }

        // GET api/visitor/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST api/visitor
        public HttpResponseMessage Post([FromBody] VisitorVisitModel visitorVisitModel)
        {
            if (!visitorVisitModel.IsUpdate)// Checked by Kishan
            {
                visitorVisitModel.VisitId = 0;
            }
            var visitorPass = new VisitorPassAllocation
            {
                CardNo = visitorVisitModel.CardId,
                VisitInformationId = visitorVisitModel.VisitId
            };
            var message = string.Empty;
            var result = new ReceptionActionResult();
            var isValid = _visitorPassAllocation.ValidateVisitorPass(visitorPass, visitorVisitModel.VisitorId, visitorVisitModel.IsVisitorView, ref message);
            result.Message = message;
            result.Status = isValid;
            if (isValid)
            {
                InsertVisitorVisitAndPassInformation(visitorVisitModel);
                if (visitorVisitModel.IsVisitorView)
                {
                    visitorVisitModel.AssignDate = Utility.GetDateTimeNow();
                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<MyTimeReceptionHub>();
                    hubContext.Clients.All.broadcastbroadVisitDetails(new JavaScriptSerializer().Serialize(visitorVisitModel));
                    //MyTimeReceptionHub myTimeReceptionHub = new MyTimeReceptionHub();
                    //myTimeReceptionHub.PushVisitDetails(visitorVisitModel);
                }
                return Request.CreateResponse(HttpStatusCode.Created, result);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, result);
        }

        private void InsertVisitorVisitAndPassInformation(VisitorVisitModel visitorVisitModel)
        {
            int visitorId;
            var visitor = new Visitor
            {
                Email = visitorVisitModel.Email,
                EnteredBy = 111,
                IdentificationNo = visitorVisitModel.IdentificationNo,
                IdentificationType = VisitorIdentificationTypeEnum.Nic.ToString(),
                Company = visitorVisitModel.Company,
                MobileNo = visitorVisitModel.MobileNo,
                Name = visitorVisitModel.Name,
                Id = visitorVisitModel.VisitorId
            };
            if (GetByMobileNo(visitorVisitModel.MobileNo) == null &&
                GetByIdNo(visitorVisitModel.IdentificationNo) == null &&
                (!visitorVisitModel.IsUpdate))
            {
                visitorId = _visitorRepository.InsertVisitorDetails(visitor);
            }
            else
            {
                if (visitor.Id == 0)
                {

                    var visitorIdForNotSearch = GetByIdNo(visitorVisitModel.IdentificationNo);
                    if (visitorIdForNotSearch == null)
                    {
                        visitorIdForNotSearch = GetByMobileNo(visitorVisitModel.MobileNo);
                        if (visitorIdForNotSearch == null)
                            return;
                    }
                    visitor.Id = visitorIdForNotSearch.Id;
                }

                visitorId = _visitorRepository.UpdateVisitorDetails(visitor);
            }
            int visitId = 0;
            if (visitorVisitModel.VisitId == 0)
            {
                visitId = InsertVisitDetails(visitorVisitModel, visitorId);
                visitorVisitModel.VisitId = visitId;
            }
            else
            {
                UpdateVisitDetails(visitorVisitModel, visitorId);
                visitId = visitorVisitModel.VisitId;
            }

            if (visitorVisitModel.CardId != 0)
            {
                AssignVisitorPass(visitorVisitModel, visitId);
            }
            if (visitorVisitModel.VisitorId == 0)
                visitorVisitModel.VisitorId = visitorId;
        }

        private void UpdateVisitDetails(VisitorVisitModel visitorVisitModel, int visitorId)
        {
            var visitInformation = new VisitInformation
            {
                Id = visitorVisitModel.VisitId,
                CardAccessLevelId = visitorVisitModel.CardAccessLevelId,
                Description = visitorVisitModel.Description,
                EmployeeId = visitorVisitModel.EmployeeId,
                EntityId = visitorId,
                EntityType = (int)VisitInfoEntityTypeEnum.Visitor,
                FomDate = Utility.GetDateTimeNow(),
                ToDate = Utility.GetDateTimeNow(),
                VisitPurpose = visitorVisitModel.VisitPurpose,
                Status = VisitStatusEnum.Allocated.ToString(),
                EnteredBy = 1212//TODO
            };
            if (visitorVisitModel.CardId == 0)
            {
                visitInformation.Status = VisitStatusEnum.Pending.ToString();
            }
            _visitRepository.UpdateVisitDetails(visitInformation);

        }

        private int InsertVisitDetails(VisitorVisitModel visitorVisitModel, int visitorId)
        {
            var visitInformation = new VisitInformation
            {
                CardAccessLevelId = visitorVisitModel.CardAccessLevelId,
                Description = visitorVisitModel.Description,
                EmployeeId = visitorVisitModel.EmployeeId,
                EntityId = visitorId,
                EntityType = (int)VisitInfoEntityTypeEnum.Visitor,
                FomDate = Utility.GetDateTimeNow(),
                ToDate = Utility.GetDateTimeNow(),
                VisitPurpose = visitorVisitModel.VisitPurpose,
                Status = VisitStatusEnum.Allocated.ToString(),
                EnteredBy = 1212//TODO
            };
            if (visitorVisitModel.CardId == 0)
            {
                visitInformation.Status = VisitStatusEnum.Pending.ToString();
            }
            var visitId = _visitRepository.InsertVisitDetails(visitInformation);
            return visitId;
        }

        private void AssignVisitorPass(VisitorVisitModel visitorVisitModel, int visitId)
        {
            if (visitorVisitModel.IsUpdate)
            {
                UpdateVisitorPass(visitId, visitorVisitModel.CardId);
            }
            else
            {


                var visitorPassAllocation = new VisitorPassAllocation
                                                {
                                                    AssignDate = Utility.GetDateTimeNow().Date,
                                                    CardIssuedBy = 123, //TODO
                                                    CardNo = visitorVisitModel.CardId,
                                                    IsActive = true,
                                                    IsCardReturned = false,
                                                    VisitInformationId = visitId
                                                };

                _visitorPassAllocation.AssignVisitorCard(visitorPassAllocation);
            }
        }

        private void UpdateVisitorPass(int visitId, int cardId)
        {
            var visitorPass = visitorPassAllocationRepository.GetByVisitInfromationId(visitId);
            if(visitorPass.CardNo==cardId)
                return;
           
            _visitorPassAllocation.ModifyVisitorPassAllocation(visitorPass.Id, cardId);

            attendanceRepository.ModifyVisitorAttendances(cardId, visitorPass.Id, visitId, Utility.GetDateTimeNow().Date);
        }

        // PUT api/visitor/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/visitor/5
        public void Delete(int id)
        {
        }
    }

    //public static class VisitorRepository1
    //{

    //    public static IList<Visitor> GetVisitors()
    //    {
    //        IList<Visitor> visitors = new List<Visitor>();
    //        Visitor visitor1 = new Visitor
    //        {
    //            Email = "nsu@exilesoft.com",
    //            Id = 1,
    //            IdentificationNo = "1111111111",
    //            MobileNo = "111009642",
    //            Name = "Niluka Subasinghe",

    //        };
    //        visitors.Add(visitor1);

    //        Visitor visitor2 = new Visitor
    //        {
    //            Email = "nfe@exilesoft.com",
    //            Id = 1,
    //            IdentificationNo = "2222222222",
    //            MobileNo = "777009642",
    //            Name = "Nilan Fernando"
    //        };
    //        visitors.Add(visitor2);

    //        Visitor visitor3 = new Visitor
    //        {
    //            Email = "jcr@exilesoft.com",
    //            Id = 1,
    //            IdentificationNo = "3333333333",
    //            MobileNo = "777009642",
    //            Name = "Johann De Cruz"
    //        };
    //        visitors.Add(visitor3);

    //        Visitor visitor4 = new Visitor
    //        {
    //            Email = "jbr@exilesoft.com",
    //            Id = 1,
    //            IdentificationNo = "4444444444",
    //            MobileNo = "777009642",
    //            Name = "Johannes BroadWall"
    //        };
    //        visitors.Add(visitor4);

    //        return visitors;

    //    }
    //}

    public enum VisitorIdentificationTypeEnum
    {
        Nic, Passport
    }
}