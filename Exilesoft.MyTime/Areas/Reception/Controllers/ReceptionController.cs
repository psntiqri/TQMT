using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Helpers;
using System.Web.Http;
using Exilesoft.Models;
using Exilesoft.MyTime.Areas.Reception.ViewModels;
using Exilesoft.MyTime.Areas.Reception.Repositories;
using Exilesoft.MyTime.Helpers;
using Exilesoft.MyTime.Repositories;
using Newtonsoft.Json;

namespace Exilesoft.MyTime.Areas.Reception.Controllers
{
    public class ReceptionController : ApiController
    {
        private IVisitRepository visitRepository;
        private IVisitorRepository visitorRepository;
        private IReceptionEmployeeRepository receptioEmployeeRepository;
        private IVisitorPassAllocationRepository visitorPassAllocationRepository;
        private AttendanceRepository attendanceRepository;
        
        public ReceptionController()
        {
            this.receptioEmployeeRepository = new ReceptionEmployeeRepository();
            this.visitRepository = new VisitRepository();
            this.visitorRepository = new VisitorRepository();
            this.visitorPassAllocationRepository = new VisitorPassAllocationRepository();
            this.attendanceRepository= new AttendanceRepository();
        }

        public ReceptionController(IVisitRepository visitRepository, IVisitorPassAllocationRepository visitorPassAllocation)
        {
            this.visitRepository = visitRepository;
            this.visitorPassAllocationRepository = visitorPassAllocation;
        }

        public ReceptionController(IVisitRepository visitRepository, IVisitorRepository visitorRepository, IReceptionEmployeeRepository employeeRepository, IVisitorPassAllocationRepository visitorPassAllocation)
        {
            this.visitRepository = visitRepository;
            this.visitorRepository = visitorRepository;
            this.receptioEmployeeRepository = employeeRepository;
            this.visitorPassAllocationRepository = visitorPassAllocation;
        }
      
        // GET api/<controller>
        public List<VisitorVisitModel> GetPendingVisits(string type)
        {          
            try
            {
                if (type == "Pending")
                {
                    var visitorVisitListPending = new List<VisitorVisitModel>();
                    var visitInformations = visitRepository.GetPendingVisitList();
                    foreach (var visitInformation in visitInformations)
                    {
                        var visitorVisitModel = new VisitorVisitModel
                        {
							Id  = visitInformation.Id,
                            Company = visitInformation.Visitor.Company,
                            IdentificationNo = visitInformation.Visitor.IdentificationNo,
                            MobileNo = visitInformation.Visitor.MobileNo,
                            Name = visitInformation.Visitor.Name,
                            Email = visitInformation.Visitor.Email,
                            EmployeeId = visitInformation.EmployeeId,
                            AppointmentTime = visitInformation.AppointmentTime
                        };
                        if (receptioEmployeeRepository.GetEmployee(visitInformation.EmployeeId) != null)
                        {
                            visitorVisitModel.EmployeeName =
                                receptioEmployeeRepository.GetEmployee(visitInformation.EmployeeId).Name;
                        }
                        visitorVisitModel.VisitorId = visitInformation.EntityId;
                        visitorVisitModel.Description = visitInformation.Description;
                        visitorVisitModel.VisitId = visitInformation.Id;
                        visitorVisitModel.VisitPurpose = visitInformation.VisitPurpose;
                        visitorVisitListPending.Add(visitorVisitModel);
                    }
                    return visitorVisitListPending.OrderByDescending(o => o.Id).ToList();
                }
                else
                {
                    //var visitorVisitList = new List<VisitorVisitModel>();
                    var visitorVisitListActive = new List<VisitorVisitModel>();
                    var visitInformations = visitRepository.GetActiveCardsList();
                    foreach (var visitInformation in visitInformations)
                    {
                        var visitorVisitModel = new VisitorVisitModel
                        {
                            Company = visitInformation.Visitor.Company,
                            IdentificationNo = visitInformation.Visitor.IdentificationNo,
                            MobileNo = visitInformation.Visitor.MobileNo,
                            Name = visitInformation.Visitor.Name,
                            Email = visitInformation.Visitor.Email,
                            EmployeeId = visitInformation.EmployeeId,
                            AppointmentTime = visitInformation.AppointmentTime
                        };
                        if (receptioEmployeeRepository.GetEmployee(visitInformation.EmployeeId) != null)
                        {
                            visitorVisitModel.EmployeeName =
                                receptioEmployeeRepository.GetEmployee(visitInformation.EmployeeId).Name;
                        }
                        VisitorPassAllocation visitorPass =
                            visitorPassAllocationRepository.GetActiveVisitorPassAllocationForVisitor(visitInformation.Id);
                        if (visitorPass != null)
                        {
                            visitorVisitModel.CardId = visitorPass.CardNo;
                            visitorVisitModel.AssignDate = visitorPass.AssignDate;
                            visitorVisitModel.Location = attendanceRepository.GetCurrentLocationName(visitorPass.Id);
                        }
                        visitorVisitModel.VisitorId = visitInformation.EntityId;
                        visitorVisitModel.Description = visitInformation.Description;
                        visitorVisitModel.VisitId = visitInformation.Id;
                        visitorVisitModel.VisitPurpose = visitInformation.VisitPurpose;

                        visitorVisitListActive.Add(visitorVisitModel);
                    }
                    return visitorVisitListActive.OrderByDescending(o => o.AssignDate).ToList();
                }
            }
            catch (Exception ex)
            {

                var visitModel1 = new VisitorVisitModel();
                visitModel1.Error = ex.Message + " plus " + ex.StackTrace;
                var visitorVisitListError = new List<VisitorVisitModel>();
                visitorVisitListError.Add(visitModel1);
                return visitorVisitListError;

            }

        }

        [HttpPost]
        public void DeAllocateEmployeePass(int visitorPassAllocationId)
        {
            var visitorPass = visitorPassAllocationRepository.GetById(visitorPassAllocationId);
            visitorPass.IsActive = false;
            visitorPass.IsCardReturned = true;
			visitorPass.DeallocateDate = Utility.GetDateTimeNow(); 
            visitorPassAllocationRepository.UpdateVisitorPassAllocation(visitorPass);
        }

        [HttpPost]
        public int DeAllocateVisitorPass(int visitId)
        {
            var retVal = 0;
            try
            {
                if (visitId > 0)
                {
                    VisitorPassAllocation visitorPass =
                        visitorPassAllocationRepository.GetActiveVisitorPassAllocationForVisitor(visitId);

                    if (!Equals(visitorPass, null))
                    {
                        visitorPass.IsActive = false;
                        visitorPass.IsCardReturned = true;
						visitorPass.DeallocateDate = Utility.GetDateTimeNow(); 
                        visitorPassAllocationRepository.UpdateVisitorPassAllocation(visitorPass);

                        retVal = 1;
                    }

                    VisitInformation visitInfo = visitRepository.GetById(visitId);

                    if (!Equals(visitInfo, null))
                    {
                        visitInfo.Status = VisitStatusEnum.Closed.ToString();
                        visitRepository.UpdateVisitDetails(visitInfo);

                        retVal = retVal & 1;
                    }
                }               

            }
            catch (Exception ex)
            {
                retVal = 0;
                throw;
            }
            return retVal;
        }

        public List<EmployeeViewModel> GetActiveCards()
        {
            var employeeList = new List<EmployeeViewModel>();
            var visitorPasses = visitRepository.GetActiveCardsListForEmployees();
            foreach (var pass in visitorPasses)
            {
                var objEmployeeViewModel = new EmployeeViewModel();
                FillEmployeeInformation(pass, objEmployeeViewModel);
                objEmployeeViewModel.NewVisitorCard = pass.CardNo;
                objEmployeeViewModel.VisitorPassAllocationId = pass.Id;
                employeeList.Add(objEmployeeViewModel);
            }
            return employeeList.OrderByDescending(e => e.Id).ToList();
        }


        private void FillEmployeeInformation(VisitorPassAllocation pass, EmployeeViewModel objEmployeeViewModel)
        {
            if (receptioEmployeeRepository.GetEmployee(pass.EmployeeId) != null)
            {
                objEmployeeViewModel.Id = pass.Id;
                objEmployeeViewModel.Name = receptioEmployeeRepository.GetEmployee(pass.EmployeeId).Name;
            }
        }

		[HttpPut]
	    public void CloseAppoinment(int appoinmentId)
	    {
		    visitRepository.UpdateVisitStatus(appoinmentId);
	    }
        //public List<VisitorVisitModel> GetActiveCards()
        //{
        //    var visitorVisitList = new List<VisitorVisitModel>();
        //    var visitInformations = visitRepository.GetActiveCardsList();
        //    foreach (var visitInformation in visitInformations)
        //    {
        //        var visitorVisitModel = new VisitorVisitModel
        //        {
        //            Company = visitInformation.Visitor.Company,
        //            IdentificationNo = visitInformation.Visitor.IdentificationNo,
        //            MobileNo = visitInformation.Visitor.MobileNo,
        //            Name = visitInformation.Visitor.Name,
        //            Email = visitInformation.Visitor.Email,
        //            EmployeeName = receptioEmployeeRepository.GetEmployee(visitInformation.EmployeeId).Name,
        //            EmployeeId = visitInformation.EmployeeId,
        //            AppointmentTime = visitInformation.AppointmentTime
        //        };
        //        VisitorPassAllocation visitorPass =
        //            visitorPassAllocationRepository.GetActiveVisitorPassAllocationForVisitor(visitInformation.EntityId);
        //        visitorVisitModel.CardId = visitorPass.CardNo;

        //        visitorVisitModel.VisitorId = visitInformation.EntityId;
        //        visitorVisitModel.Description = visitInformation.Description;
        //        visitorVisitModel.VisitId = visitInformation.Id;
        //        visitorVisitModel.VisitPurpose = visitInformation.VisitPurpose;
        //        visitorVisitList.Add(visitorVisitModel);
        //    }
        //    return visitorVisitList.OrderBy(o => o.AppointmentTime).ToList();
        //}
    }
}