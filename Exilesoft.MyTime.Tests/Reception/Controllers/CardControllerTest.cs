using Exilesoft.MyTime.Areas.Reception.Controllers;
using Exilesoft.MyTime.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Exilesoft.MyTime.Tests.Reception.Controllers
{
	[TestClass]
	public class CardControllerTest
	{
		[TestMethod]
		public void Card_Controller_Get_NextCard_Returns_AvailableCard()
		{
			var cardAllocationMock = new Mock<IVisitorPassAllocationRepository>();

			cardAllocationMock.Setup(service => service.GetAvailableCards())
							.Returns(1069);

			var cardController = new CardController(cardAllocationMock.Object);

			int? cardNo = cardController.Get();

			Assert.AreEqual(1069, cardNo);
		}
	}
}
