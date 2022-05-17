using System.Web.Http;
using Exilesoft.MyTime.Repositories;

namespace Exilesoft.MyTime.Areas.Reception.Controllers
{
    public class CardController : ApiController
    {
		private IVisitorPassAllocationRepository _visitorPassAllocationRepository;

		public CardController()
		{
			_visitorPassAllocationRepository = new VisitorPassAllocationRepository();
		}

		public CardController(IVisitorPassAllocationRepository visitorPassAllocation)
        {
			_visitorPassAllocationRepository = visitorPassAllocation;
        }

		public int? Get()
		{
			return _visitorPassAllocationRepository.GetAvailableCards();
		}

    }
}
