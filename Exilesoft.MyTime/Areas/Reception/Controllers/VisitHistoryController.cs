using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Exilesoft.MyTime.Areas.Reception.DTO;
using Exilesoft.MyTime.Areas.Reception.ViewModels;
using Exilesoft.MyTime.Repositories;

namespace Exilesoft.MyTime.Areas.Reception.Controllers
{
    public class VisitHistoryController : ApiController
    {
        private readonly IVisitorPassAllocationRepository _visitorPassAllocationRepository;

        public VisitHistoryController()
        {
            _visitorPassAllocationRepository = new VisitorPassAllocationRepository();
        }
        
        [System.Web.Http.HttpPost]
        public IList<VisitHistoryViewModel> GetHistory(VisitHistoryFilterDto historyFilterDto)
        {
            return _visitorPassAllocationRepository.GetAllocationHistory(historyFilterDto);
        }

    }
}
