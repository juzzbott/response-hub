using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Events;
using Enivate.ResponseHub.Model.Events.Interface;
using Enivate.ResponseHub.Model.Crews;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class EventService : IEventService
	{

		private ILogger _log;

		private IEventRepository _repository;

		public EventService(IEventRepository repository, ILogger log)
		{
			_repository = repository;
			_log = log;
		}

		/// <summary>
		/// Creates a new event.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="unitId"></param>
		/// <param name="userId"></param>
		/// <param name="startDate"></param>
		/// <returns></returns>
		public async Task<Event> CreateEvent(string name, string description, Guid unitId, Guid userId, DateTime startDate)
		{
			// Create the new event
			Event newEvent = new Event()
			{
				Created = DateTime.UtcNow,
				EventStarted = startDate.ToUniversalTime(),
				UnitId = unitId,
				Name = name,
				Description = description
			};

			// Create the event
			newEvent = await _repository.CreateEvent(newEvent);

			// return the new event
			return newEvent;
		}

		/// <summary>
		/// Gets the event by the id specified. If no event is found, then null is returned.
		/// </summary>
		/// <param name="id">The Id of the event to return.</param>
		/// <returns>The event if found, otherwise null.</returns>
		public async Task<Event> GetById(Guid id)
		{
			return await _repository.GetById(id);
		}

		/// <summary>
		/// Gets the collection of events based on the unit id.
		/// </summary>
		/// <param name="unitId">The id of the unit to get the events for.</param>
		/// <returns>The colection of events for the unit.</returns>
		public async Task<IList<Event>> GetEventsByUnit(Guid unitId)
		{
			return await _repository.GetEventsByUnit(unitId);
		}

		/// <summary>
		/// Find the event by the keywords. 
		/// </summary>
		/// <param name="keywords">Keywords to find the event for.</param>
		/// <param name="unitId">The id of the unit to limit the results to.</param>
		/// <returns>The list of events that match the search terms and unit ids.</returns>
		public async Task<IList<Event>> FindByKeywords(string keywords, Guid unitId)
		{
			return await _repository.FindByKeywords(keywords, unitId);
		}
		
		/// <summary>
		/// Creates a new Crew to be assigned to an event.
		/// </summary>
		/// <param name="eventId">The ID of the event to create the crew for.</param>
		/// <param name="name">The name of the crew</param>
		/// <param name="crewMembers">The members for the crew</param>
		/// <returns>The newly created crew object</returns>
		public async Task<Crew> CreateCrew(Guid eventId, string name, IList<Guid> crewMembers, Guid crewLeaderId)
		{

			// Ensure there is a crew name
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentException("The crew must have a name.");
			}

			// Create the new crew object
			Crew crew = new Crew()
			{
				Name = name,
				CrewMembers = crewMembers,
				CrewLeaderId = crewLeaderId,
				Created = DateTime.UtcNow,
				Updated = DateTime.UtcNow,
			};

			return await _repository.CreateCrew(eventId, crew);
		}

		/// <summary>
		/// Saves the crew in the event.
		/// </summary>
		/// <param name="eventId">The id of the event to save the crew for.</param>
		/// <param name="crew">The crew to save.</param>
		public async Task SaveCrew(Guid eventId, Crew crew)
		{
			// Save the crew
			await _repository.SaveCrew(eventId, crew);
		}

		/// <summary>
		/// Gets the crews for the event.
		/// </summary>
		/// <param name="eventId">The Id of the event to get the crews for.</param>
		/// <returns></returns>
		public Task<IList<Crew>> GetCrewsForEvent(Guid eventId)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Finishes the event by setting the finish date. 
		/// </summary>
		/// <param name="eventId">The id of the event to set the finish date for.</param>
		/// <returns></returns>
		public async Task FinishEvent(Guid eventId)
		{
			await _repository.FinishEvent(eventId, DateTime.UtcNow);
		}
		
		/// <summary>
		/// Gets the crew from the event.
		/// </summary>
		/// <param name="eventId">The Id of the event to get the crew from.</param>
		/// <param name="crewId">The Id of the crew to return</param>
		/// <returns>The crew if found, otherwise null</returns>
		public async Task<Crew> GetCrewFromEvent(Guid eventId, Guid crewId)
		{
			// return the crew
			return await _repository.GetCrewFromEvent(eventId, crewId);

		}

		/// <summary>
		/// Updates the crew within the event for the specified assigned job ids.
		/// </summary>
		/// <param name="eventId">The id of the event that contains the crew.</param>
		/// <param name="crewId">The id of the crew to update.</param>
		/// <param name="assignedJobIds">The list of job ids to set as the assigned jobs for the crew</param>
		/// <returns></returns>
		public async Task AssignJobsToCrew(Guid eventId, Guid crewId, IList<Guid> assignedJobIds)
		{
			await _repository.AssignJobsToCrew(eventId, crewId, assignedJobIds);
		}

		/// <summary>
		/// Saves the name, description and datetime the event started to the specified event.
		/// </summary>
		/// <param name="eventId">The id of the event to update.</param>
		/// <param name="name">The name of the event</param>
		/// <param name="description">The description for the event.</param>
		/// <param name="eventStarted">The date and time the event was started.</param>
		/// <returns></returns>
		public async Task SaveEvent(Guid eventId, string name, string description, DateTime eventStarted)
		{
			await _repository.SaveEvent(eventId, name, description, eventStarted);
		}
	}
}
