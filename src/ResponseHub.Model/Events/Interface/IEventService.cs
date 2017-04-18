using Enivate.ResponseHub.Model.Crews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Events.Interface
{
	public interface IEventService
	{

		Task<Event> CreateEvent(string name, Guid unitId, Guid userId, DateTime startDate);

		Task<Event> GetById(Guid id);

		Task<IList<Event>> GetEventsByUnit(IEnumerable<Guid> unitIds);

		Task<IList<Event>> FindByKeywords(string keywords, IEnumerable<Guid> unitIds);

		Task<EventResource> AddResourceToEvent(Guid eventId, string name, Guid agency, Guid? userId, ResourceType resourceType);

		Task<Crew> CreateCrew(Guid eventId, string name);

		Task<IList<Crew>> GetCrewsForEvent(Guid eventId);

	}
}
