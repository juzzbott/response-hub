using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Events;
using Enivate.ResponseHub.Model.Events.Interface;

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
		public async Task<Event> CreateEvent(string name, Guid unitId, Guid userId, DateTime startDate)
		{
			// Create the new event
			Event newEvent = new Event()
			{
				Created = DateTime.UtcNow,
				EventStarted = startDate.ToUniversalTime(),
				UnitId = unitId,
				Name = name
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
		public async Task<IList<Event>> GetEventsByUnit(IEnumerable<Guid> unitIds)
		{
			return await _repository.GetEventsByUnit(unitIds);
		}

		/// <summary>
		/// Find the event by the keywords. 
		/// </summary>
		/// <param name="keywords">Keywords to find the event for.</param>
		/// <param name="unitIds">The collection of unit ids to limit the results to.</param>
		/// <returns>The list of events that match the search terms and unit ids.</returns>
		public async Task<IList<Event>> FindByKeywords(string keywords, IEnumerable<Guid> unitIds)
		{
			return await _repository.FindByKeywords(keywords, unitIds);
		}

		/// <summary>
		/// Adds the specified resource to the event. 
		/// </summary>
		/// <param name="eventId"></param>
		/// <param name="name"></param>
		/// <param name="agency"></param>
		/// <param name="userId"></param>
		/// <param name="resourceType"></param>
		/// <returns></returns>
		public async Task<EventResource> AddResourceToEvent(Guid eventId, string name, Guid agency, Guid? userId, ResourceType resourceType)
		{

			// Create the event resource
			EventResource resource = new EventResource()
			{
				Active = true,
				AgencyId = agency,
				Name = name,
				Type = resourceType,
				UserId = userId,
				Created = DateTime.UtcNow
			};

			// Add the resource to the event
			bool result = await _repository.AddResourceToEvent(eventId, resource);

			// If the result is null, throw exception
			if (!result)
			{
				throw new ApplicationException("Count not add resource to event.");
			}

			return resource;
		}


	}
}
