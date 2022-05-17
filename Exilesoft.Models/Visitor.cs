using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exilesoft.Models
{
    public class Visitor
    {
        //public Visitor()
        //{
        //    this.VisitInformations = new HashSet<VisitInformation>();
        //}
        public int Id { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public string IdentificationType { get; set; }
        public string IdentificationNo { get; set; }
        public int? EnteredBy { get; set; }        
        public string Company { get; set; }
       
        //public virtual ICollection<VisitInformation> VisitInformations { get; set; }
    }
}
