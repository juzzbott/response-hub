using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Events;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface IEventRepository
	{

		Task<Event> CreateEvent(Event newEvent);

		Task<Event> GetById(Guid id);

		Task<IList<Event>> GetEventsByUnit(IEnumerable<Guid> unitIds);

		Task<IList<Event>> FindByKeywords(string keywords, IEnumerable<Guid> unitIds);

		Task<bool> AddResourceToEvent(Guid eventId, EventResource resource);

	}
}
