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

		Task<IList<Event>> GetEventsByGroup(IEnumerable<Guid> groupIds);

		Task<IList<Event>> FindByKeywords(string keywords, IEnumerable<Guid> groupIds);

		Task<bool> AddResourceToEvent(Guid eventId, EventResource resource);

	}
}
