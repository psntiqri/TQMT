using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exilesoft.Models
{
    public class CardAccessLevel
    {
        //public CardAccessLevel()
        //{
        //    this.VisitInformations = new HashSet<VisitInformation>();
        //}

        public int Id { get; set; }
        public string Description { get; set; }

        //public virtual ICollection<VisitInformation> VisitInformations { get; set; }
    }
}
