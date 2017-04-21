using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Driver;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Events;
using Enivate.ResponseHub.Model.Events;
using Enivate.ResponseHub.Model.Crews;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{

	[MongoCollectionName("events")]
	public class EventRepository : MongoRepository<Event>, IEventRepository
	{

		/// <summary>
		/// Creates the new event object in the database.
		/// </summary>
		/// <param name="newEvent">The new event to save to th db.</param>
		/// <returns></returns>
		public async Task<Event> CreateEvent(Event newEvent)
		{

			// Save the event to the database
			Event savedEvent = await Save(newEvent);

			// return the mapped model
			return savedEvent;

		}

		/// <summary>
		/// Gets the event by the id specified. If no event is found, then null is returned.
		/// </summary>
		/// <param name="id">The Id of the event to return.</param>
		/// <returns>The event if found, otherwise null.</returns>
		public new async Task<Event> GetById(Guid id)
		{
			// Get the db event from the database
			Event dbEvent = await Collection.Find(i => i.Id == id).FirstOrDefaultAsync();

			// return the mapped item
			return dbEvent;
		}

		/// <summary>
		/// Gets the collection of events based on the unit id.
		/// </summary>
		/// <param name="unitId">The id of the unit to get the events for.</param>
		/// <returns>The colection of events for the unit.</returns>
		public async Task<IList<Event>> GetEventsByUnit(Guid unitId)
		{

			// Define the 'in' filter
			FilterDefinition<Event> filter = Builders<Event>.Filter.Eq(i => i.UnitId, unitId);

			// Find the event data objects
			IList<Event> events = await Collection.Find(filter).ToListAsync();

			// return the events return events
			return events;
		}

		/// <summary>
		/// Finds the events that match the keywords entered.
		/// </summary>
		/// <param name="keywords">The keywords to find the event by.</param>
		/// <param name="unitId">The unit id to limit the results by.</param>
		/// <returns>The list of units matching the result.</returns>
		public async Task<IList<Event>> FindByKeywords(string keywords, Guid unitId)
		{

			// Build the query
			FilterDefinition<Event> filter = Builders<Event>.Filter.Text(keywords) & Builders<Event>.Filter.Eq(i => i.UnitId, unitId);

			// Get the results of the text search.
			IList<Event> results = await Collection.Find(filter).ToListAsync();

			// Create the list of events
			List<Event> mappedEvents = new List<Event>();

			// For each result, map to a Event model object.
			//foreach (EventDto result in results)
			//{
			//	mappedEvents.Add(MapDbObjectToModel(result));
			//}

			// return the mapped units.
			return results;
		}
		
		/// <summary>
		/// Creates a new crew for the event.
		/// </summary>
		/// <param name="eventId">The id of the event to create the crew for.</param>
		/// <param name="name">The name of the crew.</param>
		/// <returns></returns>
		public async Task<Crew> CreateCrew(Guid eventId, Crew crew)
		{

			// Create the filter
			FilterDefinition<Event> filter = Builders<Event>.Filter.Eq(i => i.Id, eventId);

			// Create the update
			UpdateDefinition<Event> update = Builders<Event>.Update.Push(i => i.Crews, crew);

			// Perform the update
			await Collection.UpdateOneAsync(filter, update);

			// return the crew
			return crew;

		}

		/// <summary>
		/// Gets the crew from the event.
		/// </summary>
		/// <param name="eventId">The Id of the event to get the crew from.</param>
		/// <param name="crewId">The Id of the crew to return</param>
		/// <returns>The crew if found, otherwise null</returns>
		public async Task<Crew> GetCrewFromEvent(Guid eventId, Guid crewId)
		{

			// Create the filter
			FilterDefinition<Event> filter = Builders<Event>.Filter.Eq(i => i.Id, eventId);

			// Get the event
			Event eventObj = await Collection.Find(filter).FirstOrDefaultAsync();

			// return the crew with matching id
			return eventObj.Crews.FirstOrDefault(i => i.Id == crewId);

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
			// Create the filter
			FilterDefinition<Event> filter = Builders<Event>.Filter.Eq(i => i.Id, eventId) & Builders<Event>.Filter.ElemMatch(i => i.Crews, f => f.Id == crewId);

			// Create the update
			UpdateDefinition<Event> update = Builders<Event>.Update.Set("Crews.$.JobMessageIds", assignedJobIds);

			// Perform the update
			await Collection.UpdateOneAsync(filter, update);
		}

		/// <summary>
		/// Finishes the event by setting the finish date. 
		/// </summary>
		/// <param name="eventId">The id of the event to set the finish date for.</param>
		/// <param name="finishDateTime">The date and time the event finished.</param>
		/// <returns></returns>
		public async Task FinishEvent(Guid eventId, DateTime finishDateTime)
		{
			// Create the filter
			FilterDefinition<Event> filter = Builders<Event>.Filter.Eq(i => i.Id, eventId);

			// Create the update
			UpdateDefinition<Event> update = Builders<Event>.Update.Set(i => i.EventFinished, finishDateTime);

			// Perform the update
			await Collection.UpdateOneAsync(filter, update);

		}

		#region Mappers

		/// <summary>
		/// Maps the event dto object to a model object.
		/// </summary>
		/// <param name="dbObject">The dto object to map to the model object</param>
		/// <returns>The mapped model object</returns>
		public Event MapDbObjectToModel(EventDto dbObject)
		{
			// If the db object is null, retun null
			if (dbObject == null)
			{
				return null;
			}

			// Map the model object
			Event modelObject = new Event()
			{
				Created = dbObject.Created,
				EventFinished = dbObject.EventFinished,
				EventStarted = dbObject.EventStarted,
				UnitId = dbObject.UnitId,
				Id = dbObject.Id,
				Name = dbObject.Name,
				Crews = dbObject.Crews
			};

			// return the model event
			return modelObject;
		}

		/// <summary>
		/// Maps the event model object to the event dto object.
		/// </summary>
		/// <param name="modelObject">The model object to map to the dto object.</param>
		/// <returns>The mapped dto object.</returns>
		public EventDto MapModelObjectToDb(Event modelObject)
		{
			// If the model object is null, retun null
			if (modelObject == null)
			{
				return null;
			}

			// Map the db object
			EventDto dbObject = new EventDto()
			{
				Created = modelObject.Created,
				EventFinished = modelObject.EventFinished,
				EventStarted = modelObject.EventStarted,
				UnitId = modelObject.UnitId,
				Id = modelObject.Id,
				Name = modelObject.Name,
				Crews = modelObject.Crews
			};

			// return the model event
			return dbObject;
		}

		#endregion

	}
}
