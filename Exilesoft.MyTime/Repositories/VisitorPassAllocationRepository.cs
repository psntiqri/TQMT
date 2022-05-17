using System.Data.Entity;
using System.Security.Cryptography;
using System.Web.Http.ModelBinding;
using Exilesoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Exilesoft.MyTime.Areas.Reception.DTO;
using Exilesoft.MyTime.Areas.Reception.ViewModels;
using Exilesoft.MyTime.Helpers;
using Microsoft.Ajax.Utilities;
using Context = Exilesoft.Models.Context;

namespace Exilesoft.MyTime.Repositories
{
    public class VisitorPassAllocationRepository : IVisitorPassAllocationRepository
    {

        internal static List<VisitorPassAllocation> GetActiveVisitorPassAllocationForEmployee(int employeeId)
        {
            List<VisitorPassAllocation> visitorPassAllocations;
            using (Context context = new Context())
            {
                visitorPassAllocations = context.VisitorPassAllocations.Where(
                    v =>
                        v.EmployeeId == employeeId &&
                        v.IsActive == true).ToList();
            }
            return visitorPassAllocations;
        }

        //bool IVisitorPassAllocationRepository.AssignEmployeeCard(VisitorPassAllocation visitorPassAllocation, ref string message)
        //{
        //    return AssignEmployeeCard(visitorPassAllocation, ref message);
        //}

        void IVisitorPassAllocationRepository.AssignVisitorCard(VisitorPassAllocation visitorPassAllocation)
        {
            AssignVisitorCard(visitorPassAllocation);
        }

       
        bool IVisitorPassAllocationRepository.ValidateVisitorPass(VisitorPassAllocation visitorPassAllocation, int visitorId, bool isVisitorView, ref string message)
        {
            return ValidateVisitorPass(visitorPassAllocation,visitorId, isVisitorView, ref message);
        }

        List<VisitorPassAllocation> IVisitorPassAllocationRepository.GetActiveVisitorPassAllocationForEmployee(int employeeId)
        {
            return GetActiveVisitorPassAllocationForEmployee(employeeId);
        }

        public bool IsValidCard(int cardId, int employeeId, ref string message)
        {
            using (Context context = new Context())
            {
                VisitorPassAllocation alreadyHasCard = context.VisitorPassAllocations.FirstOrDefault(
                   c => c.IsActive == true && c.IsCardReturned == false && c.EmployeeId == employeeId);

                if (alreadyHasCard != null)
                {
                    message = "Already has a active card";
                    return false;
                }
            }
            return IsValidCard(cardId, ref message);
        }

        private bool IsValidCard( int cardId, ref string message)
        {
            try
            {
                using (Context context = new Context())
                {

                    Card validCard=context.Cards.FirstOrDefault(a => a.Id == cardId);
                    if (validCard == null)
                    {
						message = "Please enter correct card number";
                        return false;
                    }
                    VisitorPassAllocation cardInUse = context.VisitorPassAllocations.FirstOrDefault(
                        c => c.CardNo == cardId && c.IsActive == true && c.IsCardReturned == false);

                    if (cardInUse != null)
                    {
						message = "This card is already issued";
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                message = "Error";
                return false;
            }

        }


        public  void  AssignEmployeeCard(VisitorPassAllocation visitorPassAllocation)
        {
            using (Context context = new Context())
            {
                context.VisitorPassAllocations.Add(visitorPassAllocation);
                context.SaveChanges();
            }

        }

        internal static void AssignVisitorCard(VisitorPassAllocation visitorPassAllocation)
        {
            using (var context = new Context())
            {
                context.VisitorPassAllocations.Add(visitorPassAllocation);
                context.SaveChanges();
            }
        }

        public  bool ValidateVisitorPass(VisitorPassAllocation visitorPassAllocation,int visitorId, bool isVisitorView, ref string message)
        {
            if (!isVisitorView && visitorPassAllocation.CardNo == 0)
            {
				message = "Please enter correct card number";
                return false;
            }
            if (visitorPassAllocation.VisitInformationId != 0)
            {
                var visitorPass = GetByVisitInfromationId(visitorPassAllocation.VisitInformationId.Value);

                if (visitorPass!=null && visitorPass.CardNo == visitorPassAllocation.CardNo)
                    return true;
            }

            var activeVisitorPass = GetActiveCardByVisitorId(visitorId);
            if (activeVisitorPass != null)
            {
                message = "Already has an active card";
                return false;
            }

            if (visitorPassAllocation.CardNo == 0) return true;

            return IsValidCard(visitorPassAllocation.CardNo, ref message);
        }

        public VisitorPassAllocation GetActiveVisitorPassAllocationForVisitor(int visitorId)
        {
            VisitorPassAllocation visitorPassAllocation;
            using (var context = new Context())
            {
                visitorPassAllocation =
                    context.VisitorPassAllocations.SingleOrDefault(
                        v => v.VisitInformationId == visitorId && v.IsActive && v.IsCardReturned == false);
            }
            return visitorPassAllocation;
        }

        public VisitorPassAllocation GetById(int id)
        {
            VisitorPassAllocation visitorPassAllocation;
            using (var context = new Context())
            {
                visitorPassAllocation = context.VisitorPassAllocations.Single(v => v.Id == id);
            }
            return visitorPassAllocation;
        }
        
        public VisitorPassAllocation GetByVisitInfromationId(int visitInfromationId)
        {
            VisitorPassAllocation visitorPassAllocation;
            using (var context = new Context())
            {
                visitorPassAllocation =
                    context.VisitorPassAllocations.FirstOrDefault(v => v.VisitInformationId == visitInfromationId);
            }
            return visitorPassAllocation;
        }



        public VisitorPassAllocation GetActiveCardByVisitorId(int visitorId)
        {
            VisitorPassAllocation visitorPassAllocation;
            using (var context = new Context())
            {
                visitorPassAllocation = (from allocations in context.VisitorPassAllocations
                                         join visitInformation in context.VisitInformation
                                             on allocations.VisitInformationId equals visitInformation.Id
                                         where visitInformation.EntityId == visitorId
                                         && allocations.IsCardReturned == false
                                         select allocations).FirstOrDefault();

            }
            return visitorPassAllocation;
        }

        public void UpdateVisitorPassAllocation(VisitorPassAllocation visitorPass)
        {
            using (var context = new Context())
            {
                context.VisitorPassAllocations.Attach(visitorPass);
                context.Entry(visitorPass).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public IList<VisitHistoryViewModel> GetAllocationHistory(VisitHistoryFilterDto filter)
        {
            var allocationHistory = new List<VisitHistoryViewModel>();

            using (var context = new Context())
            {
                var visitInfromationList = GetVisitorList(filter, context);
                allocationHistory.AddRange(visitInfromationList);

                var employeeInformationList = GetEmployeeList(filter, context);
                allocationHistory.AddRange(employeeInformationList);
            }

            return allocationHistory;
        }

        private IEnumerable<VisitHistoryViewModel> GetEmployeeList(VisitHistoryFilterDto filter, Context context)
        {
            var employeeInformationList =   (from allocations in context.VisitorPassAllocations
                                            join employeeEnrollment in context.EmployeeEnrollment
                                            on allocations.EmployeeId equals employeeEnrollment.EmployeeId
                                            where (allocations.AssignDate >= filter.FromFilterDate && allocations.AssignDate <= filter.ToFilterDate)
                                            select new VisitHistoryViewModel()
                                            {
                                                Id = allocations.Id,
                                                CardId = allocations.CardNo,
                                                DateAssigned = allocations.AssignDate,
                                                EmployeeId = employeeEnrollment.EmployeeId,
												DeallocateDate = allocations.DeallocateDate,
												IsActive = allocations.IsActive
                                            }).ToList();

            foreach (var employeeInformationListItem in employeeInformationList)
            {
                if (employeeInformationListItem.EmployeeId.HasValue)
                {
	                EmployeeData employeeData = EmployeeRepository.GetEmployee(employeeInformationListItem.EmployeeId);

                    employeeInformationListItem.VisitorName = employeeData.Name;
	                employeeInformationListItem.MobileNo = employeeData.MobileNumber;
                }
                employeeInformationListItem.VisitorType = Enum.GetName(typeof (VisitorType), 1);
            }

            return employeeInformationList;
        }

        private IEnumerable<VisitHistoryViewModel> GetVisitorList(VisitHistoryFilterDto filter, Context context)
        {
            var visitInfromationList = (from allocations in context.VisitorPassAllocations
                                        join visitInformation in context.VisitInformation
                                        on allocations.VisitInformationId equals visitInformation.Id
                                        join visitor in context.Visitors
                                        on visitInformation.EntityId equals visitor.Id
                                        join accessLevel in context.CardAccessLevels
                                        on visitInformation.CardAccessLevelId equals accessLevel.Id
                                        where (allocations.AssignDate > filter.FromFilterDate && allocations.AssignDate < filter.ToFilterDate)

                                        select new VisitHistoryViewModel()
                                        {
                                            Id = allocations.Id,
                                            VisitorName = visitor.Name,
                                            Description = visitInformation.Description,
                                            Company = visitor.Company,
                                            IdentificationNumber = visitor.IdentificationNo,
                                            EmployeeId = visitInformation.EmployeeId,
                                            CardId = allocations.CardNo,
                                            Location = accessLevel.Description,
                                            DateAssigned = allocations.AssignDate,
											MobileNo = visitor.MobileNo,
											VisitPurpose = visitInformation.VisitPurpose,
											DeallocateDate = allocations.DeallocateDate,
											IsActive = allocations.IsActive
                                        }).ToList();

            foreach (var visitInfromationListItem in visitInfromationList)
            {
                if (visitInfromationListItem.EmployeeId.HasValue)
                {
                    visitInfromationListItem.VisitorOfEmployee =
                        EmployeeRepository.GetEmployee(visitInfromationListItem.EmployeeId).Name;
                }
                visitInfromationListItem.VisitorType = Enum.GetName(typeof (VisitorType), 2);
            }

            return visitInfromationList;
        }

		public static List<ActiveCardHistoryViewModel> GetAllActiveVisitorPasses()
	    {
			List<ActiveCardHistoryViewModel> activeCardHistory = new List<ActiveCardHistoryViewModel>();
			using (Context context = new Context())
			{
				var allCards = context.Cards.GroupJoin(context.VisitorPassAllocations.Where(a => a.IsCardReturned == false),
					u => u.Id,
					p => p.CardNo,
					(u, p) =>
						new { Cards = u, VisitorPassAllocations = p.DefaultIfEmpty() })
					.SelectMany(a => a.VisitorPassAllocations.Select(b => new { CardId = a.Cards.Id, VisitorInformation = b })).ToList();

			
				foreach (var allCard in allCards)
				{
					var objActiveCardHistoryViewModel = new ActiveCardHistoryViewModel();
					objActiveCardHistoryViewModel.CardId = allCard.CardId;
					if (allCard.VisitorInformation != null)
					{
						if (allCard.VisitorInformation.EmployeeId != null)
						{
							EmployeeEnrollment emp = allCard.VisitorInformation.EmployeeEnrollment;
							objActiveCardHistoryViewModel.Name = EmployeeRepository.GetEmployee(allCard.VisitorInformation.EmployeeId).Name;
							objActiveCardHistoryViewModel.Type = Enum.GetName(typeof(VisitorType), 1);
						}
						if (allCard.VisitorInformation.VisitInformationId != null)
						{

							objActiveCardHistoryViewModel.Name = allCard.VisitorInformation.VisitInformation.Visitor.Name;
							objActiveCardHistoryViewModel.Type = Enum.GetName(typeof(VisitorType), 2);
						}
					}

					activeCardHistory.Add(objActiveCardHistoryViewModel);

				}
				
			}



			return activeCardHistory;
	    }


        public void ModifyVisitorPassAllocation(int allocationId,int cardId)
        {
            using (var context = new Context())
            {
                VisitorPassAllocation currentAllocation = context.VisitorPassAllocations.First(a => a.Id == allocationId);
                currentAllocation.CardNo = cardId;
                context.SaveChanges();
            }
        }

	    public int? GetAvailableCards()
	    {
		    int? card = null;
		    using (var context = new Context())
		    {
				 var cardNo =   context.Cards.Where(a => !context.VisitorPassAllocations.Any(b => b.CardNo == a.Id && b.IsCardReturned == false))
			           .Where(c => c.CardAccessLevelId == 1)
			           .ToList().OrderBy(a=>a.Id).FirstOrDefault();
				 if (cardNo != null) card = cardNo.Id;
		    }
		    return card;
	    }
    }

    public interface IVisitorPassAllocationRepository
    {
        List<VisitorPassAllocation> GetActiveVisitorPassAllocationForEmployee(int employeeId);
        void AssignEmployeeCard(VisitorPassAllocation visitorPassAllocation);
        void AssignVisitorCard(VisitorPassAllocation visitorPassAllocation);
        void ModifyVisitorPassAllocation(int allocationId, int cardId);
        bool ValidateVisitorPass(VisitorPassAllocation visitorPassAllocation,int visitorId,bool isVisitorView, ref string message);
        VisitorPassAllocation GetActiveVisitorPassAllocationForVisitor(int entityId);
        void UpdateVisitorPassAllocation(VisitorPassAllocation visitorPass);
        VisitorPassAllocation GetById(int id);
        IList<VisitHistoryViewModel> GetAllocationHistory(VisitHistoryFilterDto filter);
        VisitorPassAllocation GetByVisitInfromationId(int visitInfromationId);
        bool IsValidCard(int cardId, int employeeId, ref string message);
	    int? GetAvailableCards();
    }
}