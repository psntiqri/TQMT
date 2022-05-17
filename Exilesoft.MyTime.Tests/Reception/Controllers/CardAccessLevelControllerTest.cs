using System.Collections.Generic;
using Exilesoft.Models;
using Exilesoft.MyTime.Areas.Reception.Controllers;
using Exilesoft.MyTime.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Exilesoft.MyTime.Tests.Reception.Controllers
{
    [TestClass]
    public class CardAccessLevelControllerTest
    {
        [TestMethod]
        public void Get_Test()
        {
            var cardAccessLevelRepositoryMock = new Mock<ICardAccessLevelRepository>();
            cardAccessLevelRepositoryMock.Setup(service => service.GetCardAccessLevels()).Returns(new List<CardAccessLevel> { new CardAccessLevel { Id = 1, Description = "All Floors" }, new CardAccessLevel { Id = 2, Description = "4th Floor" } });
            var cardAccessLevelController = new CardAccessLevelController(cardAccessLevelRepositoryMock.Object);
            List<CardAccessLevel> cardAccessLevels = cardAccessLevelController.Get();
            Assert.IsNotNull(cardAccessLevels);
            Assert.AreEqual(2, cardAccessLevels.Count);
        }
    }
}
