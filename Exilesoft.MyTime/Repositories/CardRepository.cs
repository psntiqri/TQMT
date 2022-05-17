using Exilesoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.Repositories
{
    public class CardRepository
    {

        internal static string GetCardAccessLevel(int cardNo)
        {
            string cardAccessLevel="";
            using (Context context = new Context())
            {
                Card card = context.Cards.SingleOrDefault(a => a.Id == cardNo);
                if (card != null)
                    cardAccessLevel = card.CardAccessLevel.Description;
            }
            return cardAccessLevel;
        }
    }
}