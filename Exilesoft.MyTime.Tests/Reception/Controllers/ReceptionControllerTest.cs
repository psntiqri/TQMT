using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exilesoft.Models;
using Exilesoft.MyTime.Areas.Reception.Controllers;
using Exilesoft.MyTime.Helpers;
using Exilesoft.MyTime.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Exilesoft.MyTime.Tests.Reception.Controllers
{
    [TestClass]
    public class ReceptionControllerTest
    {
        [TestMethod]
        public void Reception_Controller_DeAllocateVisitorPass()
        {
            var visitorPassRepoMock = new Mock<IVisitorPassAllocationRepository>();
            var visitRepoMock = new Mock<IVisitRepository>();

            visitorPassRepoMock.Setup(service => service.GetActiveVisitorPassAllocationForVisitor(1000001))
                .Returns(new VisitorPassAllocation
                {
                    Id = 1000001,
                    AssignDate = Utility.GetDateTimeNow().Date,
                    CardIssuedBy = 1,
                    CardNo = 952,
                    EmployeeId = 2,
                    IsActive = true,
                    IsCardReturned = false,
                    VisitInformationId = 5,
                    EmployeeEnrollment =
                        new EmployeeEnrollment
                        {
                            CardNo = 952,
                            EmployeeId = 2,
                            UserName = "test",
                            IsEnable = true,
                            MobileId = Guid.NewGuid(),
                            Privillage = 1
                        }
                });

            visitRepoMock.Setup(
                service => service.GetActiveCardsList())
                .Returns(new List<VisitInformation>
            {
                new VisitInformation
                {
                    AppointmentTime = Utility.GetDateTimeNow(),
                    Visitor =
                        new Visitor
                        {
                            Company = "test",
                            Email = "test@test.com",
                            Id = 1000001,
                            EnteredBy = 42,
                            IdentificationNo = "123456789V",
                            IdentificationType = "Nic",
                            Name = "Test Visitor"
                        },
                    EmployeeId = 2
                }
            }
        );


            var receptioncontroller = new ReceptionController(visitRepoMock.Object, visitorPassRepoMock.Object);
            var deAllocationStatus = receptioncontroller.DeAllocateVisitorPass(1000001);
            Assert.AreNotEqual(0, deAllocationStatus);
            Assert.AreEqual(deAllocationStatus,1);
        }
    }
}
