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

		Task<Event> CreateEvent(string name, string description, Guid unitId, Guid userId, DateTime startDate);

		Task<Event> GetById(Guid id);

		Task<IList<Event>> GetEventsByUnit(Guid unitId);

		Task<IList<Event>> FindByKeywords(string keywords, Guid unitId);

		Task<Crew> CreateCrew(Guid eventId, string name, IList<Guid> crewMembers, Guid crewLeaderId);

		Task SaveCrew(Guid eventId, Crew crew);

		Task<IList<Crew>> GetCrewsForEvent(Guid eventId);

		Task<Crew> GetCrewFromEvent(Guid eventId, Guid crewId);

		Task AssignJobsToCrew(Guid eventId, Guid crewId, IList<Guid> assignedJobIds);

		Task FinishEvent(Guid eventId);
		
		Task SaveEvent(Guid eventId, string name, string description, DateTime startDate);

		Task<IList<Event>> GetActiveEvents();

		Task SetJobsToEvent(Guid eventId, IList<Guid> jobMessageIds);

		Task<int> CountActiveEventsForUser(Guid userId);

		Task<IList<Event>> GetActiveEventsForUser(Guid userId);

	}
}
