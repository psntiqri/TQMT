using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace Exilesoft.MyTime.Areas.Reception.Controllers
{
    public class DashboardController : ApiController
    {
        // GET api/visit
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/visit/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/visit
        public void Post([FromBody]string value)
        {
        }

        // PUT api/visit/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/visit/5
        public void Delete(int id)
        {
        }
    
    }
}
