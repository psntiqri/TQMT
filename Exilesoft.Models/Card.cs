using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exilesoft.Models
{
    public class Card
    {
        //public Card()
        //{
        //    this.VisitorPassAllocations = new HashSet<VisitorPassAllocation>();
        //}
         [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [ForeignKey("CardCategoryId")]
        public CardCategory CardCategory { get; set; }
        public int? CardCategoryId { get; set; }
        [ForeignKey("CardAccessLevelId")]
        public virtual CardAccessLevel CardAccessLevel { get; set; }
        public int? CardAccessLevelId { get; set; }

        //public virtual ICollection<VisitorPassAllocation> VisitorPassAllocations { get; set; }
    }
    
}
