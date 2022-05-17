using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Exilesoft.Models;

namespace Exilesoft.MyTime.Repositories
{
    public class CardAccessLevelRepository : ICardAccessLevelRepository
    {
        public List<CardAccessLevel> GetCardAccessLevels()
        {
            List<CardAccessLevel> cardAccessLevels=new List<CardAccessLevel>();
            using (Context dbContext = new Context())
            {
                var cardAcessLevelList = from v in dbContext.CardAccessLevels
                    select v;
                cardAccessLevels = cardAcessLevelList.ToList();
            }
            return cardAccessLevels;
        }
    }

    public interface ICardAccessLevelRepository
    {
        List<CardAccessLevel> GetCardAccessLevels();
    }
}