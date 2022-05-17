using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Exilesoft.Models;
using Exilesoft.MyTime.Areas.Reception.ViewModels;
using Exilesoft.MyTime.Repositories;

namespace Exilesoft.MyTime.Areas.Reception.Controllers
{
    public class ActiveCardHistoryController : ApiController
    {
        // GET api/activecardhistory
		public List<ActiveCardHistoryViewModel> Get()
		{
			return VisitorPassAllocationRepository.GetAllActiveVisitorPasses();

			//var activeCardHistory = new List<ActiveCardHistoryViewModel>();

			//activeCardHistory.Add(new ActiveCardHistoryViewModel() { CardId = 1, Name = "Exilesoft", EntityType = VisitorPassAllocationTypes.Employee });
			//return activeCardHistory;
        }

        // GET api/activecardhistory/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/activecardhistory
        public void Post([FromBody]string value)
        {
        }

        // PUT api/activecardhistory/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/activecardhistory/5
        public void Delete(int id)
        {
        }
    }
}
