using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Exilesoft.Models;
using Exilesoft.MyTime.Repositories;

namespace Exilesoft.MyTime.Areas.Reception.Controllers
{
    public class CardAccessLevelController : ApiController
    {
        private ICardAccessLevelRepository cardAccessLevelRepository;
        public CardAccessLevelController()
        {
            cardAccessLevelRepository = new CardAccessLevelRepository();
        }

        public CardAccessLevelController(ICardAccessLevelRepository cardAccessLevelRepository)
        {
            this.cardAccessLevelRepository = cardAccessLevelRepository;
        }

        public List<CardAccessLevel> Get()
        {
            return cardAccessLevelRepository.GetCardAccessLevels();
        }
    }
}
