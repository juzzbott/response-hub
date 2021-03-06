using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Events;
using Enivate.ResponseHub.Model.Crews;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface IEventRepository
	{

		Task<Event> CreateEvent(Event newEvent);

		Task<Event> GetById(Guid id);

		Task<IList<Event>> GetEventsByUnit(Guid unitIds);

		Task<IList<Event>> FindByKeywords(string keywords, Guid unitId);
		
		Task<Crew> CreateCrew(Guid eventId, Crew crew);

		Task SaveCrew(Guid eventId, Crew crew);

		Task<Crew> GetCrewFromEvent(Guid eventId, Guid crewId);

		Task AssignJobsToCrew(Guid eventId, Guid crewId, IList<Guid> assignedJobIds);

		Task FinishEvent(Guid eventId, DateTime finishDateTime);

		Task SaveEvent(Guid eventId, string name, string description, DateTime startDate);

		Task<IList<Event>> GetActiveEvents();

		Task SetJobsToEvent(Guid eventId, IList<Guid> jobMessageIds);

		Task<int> CountActiveEventsForUser(Guid userId);

		Task<IList<Event>> GetActiveEventsForUser(Guid userId);

	}
}
