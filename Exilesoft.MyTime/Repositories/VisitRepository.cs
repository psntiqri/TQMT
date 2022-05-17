using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Exilesoft.Models;

namespace Exilesoft.MyTime.Repositories
{
    public class VisitRepository : IVisitRepository
    {
        public static int InsertVisitDetails(VisitInformation visitInformation)
        {
            using (var dbContext = new Context())
            {
                dbContext.VisitInformation.Add(visitInformation);
                dbContext.SaveChanges();
                return visitInformation.Id;
            }
        }

        public List<VisitInformation> GetPendingVisitList()
        {
            //using (Context context = new Context())
            //{
            //    foreach (var s in context.Visitors)
            //    {
            //        Console.WriteLine(s.Name);


            //    }
            //    foreach (var s1 in context.VisitInformation.Where(a => a.EntityId == 1))
            //    {
            //        Console.WriteLine(s1.Description);
            //    }
            //}

            List<VisitInformation> visitInformations;
            using (Context context = new Context())
            {
                context.Visitors.ToList();
                visitInformations = (from v in context.VisitInformation.ToList()
                                     where v.Status == VisitStatusEnum.Pending.ToString()
                                     select v).ToList();
            }
            return visitInformations;
        }

        int IVisitRepository.InsertVisitDetails(VisitInformation visitInformation)
        {
            return InsertVisitDetails(visitInformation);
        }

        public List<VisitInformation> GetActiveCardsList()
        {
            List<VisitInformation> visitInformations;
            using (Context context = new Context())
            {
                context.Visitors.ToList();
                visitInformations = (from v in context.VisitInformation.ToList()
                                     where v.Status == VisitStatusEnum.Allocated.ToString()
                                     select v).ToList();
            }
            return visitInformations;
        }

		public List<VisitorPassAllocation> GetActiveCardsListForEmployees()
		{
			List<VisitorPassAllocation> visitorPassAllocation;
			using (var context = new Context())
			{
				visitorPassAllocation =
					context.VisitorPassAllocations.Where(v => v.IsCardReturned==false && v.EmployeeId != null)
						.ToList();
			}
			return visitorPassAllocation;
		}

        public void UpdateVisitDetails(VisitInformation visitInformation)
        {
            using (var dbContext = new Context())
            {
                dbContext.VisitInformation.Attach(visitInformation);
                dbContext.Entry(visitInformation).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
        }

	    public void UpdateVisitStatus(int id)
	    {
			using (var dbContext = new Context())
			{
				var visitInformation = dbContext.VisitInformation.Single(v => v.Id == id);

				visitInformation.Status = "Closed";
				dbContext.VisitInformation.Attach(visitInformation);
				dbContext.Entry(visitInformation).State = EntityState.Modified;
				dbContext.SaveChanges();
			}
	    }

        public VisitInformation GetById(int visitId)
        {
            using (var dbContext = new Context())
            {
                return dbContext.VisitInformation.Single(v => v.Id == visitId);
            }
            
        }
    }
	
    public interface IVisitRepository
    {
        int InsertVisitDetails(VisitInformation visitInformation);
        List<VisitInformation> GetPendingVisitList();
        List<VisitInformation> GetActiveCardsList();
        VisitInformation GetById(int visitId);
        void UpdateVisitDetails(VisitInformation visitInformation);
	    List<VisitorPassAllocation> GetActiveCardsListForEmployees();
	    void UpdateVisitStatus(int id);
    }
}