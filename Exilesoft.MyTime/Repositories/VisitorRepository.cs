using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Exilesoft.Models;

namespace Exilesoft.MyTime.Repositories
{
    public class VisitorRepository : IVisitorRepository
    {
        // private static Context _dbContext = new Context();
        public static IEnumerable<Visitor> GetVisitors()
        {
            using (var dbContext = new Context())
            {
                var visitors = from v in dbContext.Visitors
                               select v;
                return visitors;
            }
        }

        Visitor IVisitorRepository.GetVisitorByIdentityNo(string identityNo)
        {
            return GetVisitorByIdentityNo(identityNo);
        }

        int IVisitorRepository.InsertVisitorDetails(Visitor visitor)
        {
            return InsertVisitorDetails(visitor);
        }

        void IVisitorRepository.InsertVisitDetails(VisitInformation visitInformation)
        {
            InsertVisitDetails(visitInformation);
        }

        int IVisitorRepository.UpdateVisitorDetails(Visitor visitor)
        {
            return UpdateVisitorDetails(visitor);
        }

        public Visitor GetVisitorById(int id)
        {
            using (var dbContext = new Context())
            {
                var visitor = from v in dbContext.Visitors where v.Id == id
                               select v;
                return visitor.SingleOrDefault();
            }
        }

        IEnumerable<Visitor> IVisitorRepository.GetVisitors()
        {
            return GetVisitors();
        }

        public Visitor GetVisitorByMobileNo(string mobileNo)
        {
            using (var dbContext = new Context())
            {
                //var visitors = from v in dbContext.Visitors
                //               where mobileNo != null && v.MobileNo.ToLower().Equals(mobileNo.ToLower())
                //               select v;
                return dbContext.Visitors.FirstOrDefault(a => a.MobileNo.ToLower() == mobileNo.ToLower());

            }
        }

        public static Visitor GetVisitorByIdentityNo(string identityNo)
        {
            using (var dbContext = new Context())
            {
                return dbContext.Visitors.FirstOrDefault(a => a.IdentificationNo.ToLower() == identityNo.ToLower());
            }
        }

        public static int InsertVisitorDetails(Visitor visitor)
        {
            //_dbContext.Visitors.Add(new Visitor()
            //{
            //    Email = "nsu@exilesoft.com",
            //    Id = 1,
            //    IdentificationNo = "12345678",
            //    MobileNo = "777009642",
            //    Name = "Niluka Subasinhe",
            //    Company = "IBM pvt ltd."
            //});
            //_dbContext.SaveChanges();
            using (var dbContext = new Context())
            {
                dbContext.Visitors.Add(visitor);
                dbContext.SaveChanges();
                return visitor.Id;
            }
        }
        public static void InsertVisitDetails(VisitInformation visitInformation)
        {
            //_dbContext.VisitInformation.Add(new VisitInformation
            //{
            //   CardAccessLevelId = 1,
            //   Description = "Laptop, Bag, Mobile phone",
            //   EntityId = 1,
            //   EntityType = (int)VisitInfoEntityTypeEnum.Visitor

            //});
            //_dbContext.SaveChanges();
            using (var dbContext = new Context())
            {
                dbContext.VisitInformation.Add(visitInformation);
                dbContext.SaveChanges();
            }
        }
        public static int UpdateVisitorDetails(Visitor visitor)
        {
            //_dbContext.Visitors.Add(new Visitor()
            //{
            //    Email = "nsu@exilesoft.com",
            //    Id = 1,
            //    IdentificationNo = "12345678",
            //    MobileNo = "777009642",
            //    Name = "Niluka Subasinhe",
            //    Company = "IBM pvt ltd."
            //});
            //_dbContext.SaveChanges();
            using (var dbContext = new Context())
            {
                dbContext.Visitors.Attach(visitor);
                dbContext.Entry(visitor).State = EntityState.Modified;
                dbContext.SaveChanges();
                return visitor.Id;
            }
        }
    }

    public interface IVisitorRepository
    {
        IEnumerable<Visitor> GetVisitors();
        Visitor GetVisitorByMobileNo(string mobileNo);
        Visitor GetVisitorByIdentityNo(string identityNo);
        int InsertVisitorDetails(Visitor visitor);
        void InsertVisitDetails(VisitInformation visitInformation);
        int UpdateVisitorDetails(Visitor visitor);
        Visitor GetVisitorById(int id);
    }
}