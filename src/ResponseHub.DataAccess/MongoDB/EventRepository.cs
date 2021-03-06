using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Driver;

using Enivate.ResponseHub.DataAccess.Interface;
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
		/// Saves the crew in the event.
		/// </summary>
		/// <param name="eventId">The id of the event to save the crew for.</param>
		/// <param name="crew">The crew to save.</param>
		public async Task SaveCrew(Guid eventId, Crew crew)
		{

			// Set the updated date
			crew.Updated = DateTime.UtcNow;

			// Create the filter
			FilterDefinition<Event> filter = Builders<Event>.Filter.Eq(i => i.Id, eventId) & Builders<Event>.Filter.ElemMatch(i => i.Crews, c => c.Id == crew.Id);

			// Create the update
			UpdateDefinition<Event> update = Builders<Event>.Update.Set(i => i.Crews[-1], crew);

			// Perform the update
			await Collection.UpdateOneAsync(filter, update);
			
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
			UpdateDefinition<Event> update = Builders<Event>.Update.Set(i => i.FinishedDate, finishDateTime);

			// Perform the update
			await Collection.UpdateOneAsync(filter, update);

		}

		/// <summary>
		/// Saves the name, description and datetime the event started to the specified event.
		/// </summary>
		/// <param name="eventId">The id of the event to update.</param>
		/// <param name="name">The name of the event</param>
		/// <param name="description">The description for the event.</param>
		/// <param name="startDate">The date and time the event was started.</param>
		/// <returns></returns>
		public async Task SaveEvent(Guid eventId, string name, string description, DateTime startDate)
		{

			// Create the filter
			FilterDefinition<Event> filter = Builders<Event>.Filter.Eq(i => i.Id, eventId);

			// Create the update
			UpdateDefinition<Event> update = Builders<Event>.Update
				.Set(i => i.Description, description)
				.Set(i => i.StartDate, startDate)
				.Set(i => i.Name, name);

			// Do the update
			await Collection.UpdateOneAsync(filter, update);

		}

		/// <summary>
		/// Returns a list of the currently actived events.
		/// </summary>
		/// <returns>The list of currently active events (where there is no finish date specified).</returns>
		public async Task<IList<Event>> GetActiveEvents()
		{
			// Create the filter
			FilterDefinition<Event> filter = Builders<Event>.Filter.Eq(i => i.FinishedDate, null);

			// return the results for active events.
			IList<Event> activeEvents = await Collection.Find(filter).ToListAsync();

			// return the active events
			return activeEvents;
		}

		/// <summary>
		/// Sets the specified list of job ids to the event. 
		/// </summary>
		/// <param name="eventId">The Id of the event to set the job ids for.</param>
		/// <param name="jobMessageIds">The list of job message ids to set to the event.</param>
		public async Task SetJobsToEvent(Guid eventId, IList<Guid> jobMessageIds)
		{
			// Create the filter
			FilterDefinition<Event> filter = Builders<Event>.Filter.Eq(i => i.Id, eventId);

			// Create the update
			UpdateDefinition<Event> update = Builders<Event>.Update.Set(i => i.JobMessageIds, jobMessageIds);

			await Collection.UpdateOneAsync(filter, update);
		}

		/// <summary>
		/// Counts the number of active events for the specified user.
		/// </summary>
		/// <param name="userId">The user id to check if there are active events for.</param>
		/// <returns>The number of active events for the user.</returns>
		public async Task<int> CountActiveEventsForUser(Guid userId)
		{
			// Define the filter
			FilterDefinition<Event> filter = Builders<Event>.Filter.ElemMatch(i => i.Crews, Builders<Crew>.Filter.AnyEq(i => i.CrewMembers, userId)) & Builders<Event>.Filter.Eq(i => i.FinishedDate, null);

			// return the number of events that match the filter
			long results = await Collection.Find(filter).CountDocumentsAsync();

			// return the results
			return (int)results;

		}

		/// <summary>
		/// Returns the active events for the specified user.
		/// </summary>
		/// <param name="userId">The user id to check if there are active events for.</param>
		/// <returns>The active events for the user.</returns>
		public async Task<IList<Event>> GetActiveEventsForUser(Guid userId)
		{
			// Define the filter
			FilterDefinition<Event> filter = Builders<Event>.Filter.ElemMatch(i => i.Crews, Builders<Crew>.Filter.AnyEq(i => i.CrewMembers, userId)) & Builders<Event>.Filter.Eq(i => i.FinishedDate, null);

			// return the number of events that match the filter
			return await Collection.Find(filter).ToListAsync();
		}

	}
}
