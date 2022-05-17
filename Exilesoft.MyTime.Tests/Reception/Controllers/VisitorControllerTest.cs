using Exilesoft.Models;
using Exilesoft.MyTime.Areas.Reception.Controllers;
using Exilesoft.MyTime.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Exilesoft.MyTime.Tests.Reception.Controllers
{
    [TestClass]
    public class VisitorControllerTest
    {
        [TestMethod]
        public void Visitor_Controller_Get_By_MobileNo_Returns_Visitor()
        {
            var visitorMock = new Mock<IVisitorRepository>();
            var visitMock = new Mock<IVisitRepository>();
            var cardAllocationMock = new Mock<IVisitorPassAllocationRepository>();
            visitorMock.Setup(service => service.GetVisitorByMobileNo("888777664"))
                            .Returns(new Visitor { Company = "ibm", Name = "saman silva", Email = "saman@ibm.lk", Id = 11, IdentificationNo = "123456", MobileNo = "888777664", EnteredBy = 234 });
            var visitorcontroller = new VisitorController(visitorMock.Object, visitMock.Object, cardAllocationMock.Object);

            var visitor = visitorcontroller.GetByMobileNo("888777664");
            Assert.IsNotNull(visitor);
            Assert.AreEqual("888777664", visitor.MobileNo);
            Assert.AreEqual("saman silva", visitor.Name);

            //try
            //{
            //    var visitornull = visitorcontroller.GetByMobileNo(null);
            //    Assert.Fail("no exception thrown");
            //}
            //catch (Exception ex)
            //{
            //    Assert.IsTrue(ex is HttpResponseException);
            //}
        }

        //[TestMethod]
        //[Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedException(typeof(HttpResponseException))]
        //public async Task Visitor_Controller_Get_By_MobileNo_Should_Throw_HttpResponseException()
        //{
        //    var visitorMock = new Mock<IVisitorRepository>();
        //    var visitMock = new Mock<IVisitRepository>();
        //    var cardAllocationMock = new Mock<IVisitorPassAllocationRepository>();
        //    visitorMock.Setup(service => service.GetVisitorByMobileNo("888777664"))
        //                    .Returns(new Visitor { Company = "ibm", Name = "saman silva", Email = "saman@ibm.lk", Id = 11, IdentificationNo = "123456", MobileNo = "888777664", EnteredBy = 234 });
        //    var visitorcontroller = new VisitorController(visitorMock.Object, visitMock.Object, cardAllocationMock.Object);

        //    var visitorNull = visitorcontroller.GetByMobileNo(null);
        //    Assert.IsNull(visitorNull);
        //}

        [TestMethod]
        public void Visitor_Controller_Get_By_IdentificationNo_Returns_Visitor()
        {
            var visitorMock = new Mock<IVisitorRepository>();
            var visitMock = new Mock<IVisitRepository>();
            var cardAllocationMock = new Mock<IVisitorPassAllocationRepository>();
            visitorMock.Setup(service => service.GetVisitorByIdentityNo("123456789v"))
                            .Returns(new Visitor { Company = "abans", Name = "Rajitha silva", Email = "Rajitha@agans.lk", Id = 245, IdentificationNo = "123456789v", MobileNo = "9876543210", EnteredBy = 235 });
            var visitorcontroller = new VisitorController(visitorMock.Object, visitMock.Object, cardAllocationMock.Object);

            var visitor = visitorcontroller.GetByIdNo("123456789v");
            Assert.IsNotNull(visitor);

            //try
            //{
            //    var visitornull = visitorcontroller.GetByIdNo(null);
            //    Assert.Fail("no exception thrown");
            //}
            //catch (Exception ex)
            //{
            //    Assert.IsTrue(ex is HttpResponseException);
            //}
        }

        //[TestMethod]
        //[Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedException(typeof(HttpResponseException))]
        //public async Task Visitor_Controller_Get_By_IdentificationNo_Should_Throw_HttpResponseException()
        //{
        //    var visitorMock = new Mock<IVisitorRepository>();
        //    var visitMock = new Mock<IVisitRepository>();
        //    var cardAllocationMock = new Mock<IVisitorPassAllocationRepository>();
        //    visitorMock.Setup(service => service.GetVisitorByIdentityNo("123456789v"))
        //                    .Returns(new Visitor { Company = "abans", Name = "Rajitha silva", Email = "Rajitha@agans.lk", Id = 245, IdentificationNo = "123456789v", MobileNo = "9876543210", EnteredBy = 235 });
        //    var visitorcontroller = new VisitorController(visitorMock.Object, visitMock.Object, cardAllocationMock.Object);

        //    var visitor = visitorcontroller.GetByIdNo(null);
        //    Assert.IsNull(visitor);
        //}

        [TestMethod]
        public void Visitor_Controller_Post_Visitor_Returns_HttpCreatedStatus()
        {
            var visitorMock = new Mock<IVisitorRepository>();
            var visitMock = new Mock<IVisitRepository>();
            var cardAllocationMock = new Mock<IVisitorPassAllocationRepository>();
            visitorMock.Setup(service => service.InsertVisitorDetails(It.IsAny<Visitor>()));
            visitMock.Setup(service => service.InsertVisitDetails(It.IsAny<VisitInformation>()));
            cardAllocationMock.Setup(service => service.AssignVisitorCard(It.IsAny<VisitorPassAllocation>()));
            //   .Returns(new Visitor { Company = "abans", Name = "Rajitha silva", Email = "Rajitha@agans.lk", Id = 245, IdentificationNo = "123456789v", MobileNo = "9876543210", EnteredBy = 235 });
            //var visitorcontroller = new VisitorController(visitorMock.Object, visitMock.Object, cardAllocationMock.Object);

           /* var response = visitorcontroller.Post(new VisitorVisitModel
            {
                IdentificationNo = "123456789v",
                CardAccessLevelId = 2,
                CardId = 2345,
                Company = "Management company",
                Description = "laptop",
                Email = "rwa@mgt.com",
                EmployeeId = 208,
                MobileNo = "9876543210",
                Name = "Ravi Wanasinghe",
                VisitPurpose = "personal"
            });
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);*/
        }
        //private HttpClient client;

        //[SetUp]
        //public void HttpClientSetup()
        //{
        //    var config = new HttpConfiguration();

        //   // config.Routes.AddHttpRoutes();
        //    config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

        //    var server = new HttpServer(config);
        //    this.client = new HttpClient(server);

        //}

        //[TestMethod]
        //public void PostRecordTest()
        //{
            
        //    var visitorVisitModel = new VisitorVisitModel
        //    {
        //        Company = "ibm",
        //        CardAccessLevelId = 1,
        //    };
        //    var request = new HttpRequestMessage { RequestUri = new Uri("http://localhost:55820/Reception#/api/Visitor") };
        //    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    request.Method = HttpMethod.Post;
        //    request.Content = new ObjectContent<VisitorVisitModel>(visitorVisitModel, new JsonMediaTypeFormatter());

        //    HttpResponseMessage response = this.client.SendAsync(request, new CancellationTokenSource().Token).Result;

        //    Assert.IsTrue(response.IsSuccessStatusCode);
        //    Assert.IsNotNull(response.Content);
        //}
    }
}
