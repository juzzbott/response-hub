using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Messages;
using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Spatial;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Spatial;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{

	[MongoCollectionName("job_messages")]
	public class JobMessageRepository : MongoRepository<JobMessageDto>, IJobMessageRepository
	{

		/// <summary>
		/// Adds the messages to the database.
		/// </summary>
		/// <param name="messages"></param>
		/// <returns></returns>
		public async Task AddMessages(IList<JobMessage> messages)
		{

			// Write the job messages to the database.
			await Collection.InsertManyAsync(messages.Select(i => MapModelToDbObject(i)));
		}

		/// <summary>
		///  Gets the most recent job messages for the list of capcodes specified. Results are limited to count number of items.
		/// </summary>
		/// <param name="capcodes"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public async Task<IList<JobMessage>> GetMostRecent(IEnumerable<string> capcodes, int count, MessageType messageTypes)
		{

			// Create the filter and sort
			FilterDefinitionBuilder<JobMessageDto> builder = Builders<JobMessageDto>.Filter;
			FilterDefinition<JobMessageDto> filter = builder.In(i => i.Capcode, capcodes);

			if (messageTypes.HasFlag(MessageType.Job))
			{
				filter = filter & builder.Ne(i => i.Priority, MessagePriority.Administration);
			}
			else if (messageTypes.HasFlag(MessageType.Message))
			{
				filter = filter & builder.Eq(i => i.Priority, MessagePriority.Administration);
			}

			SortDefinition<JobMessageDto> sort = Builders<JobMessageDto>.Sort.Descending(i => i.Timestamp);

			// Find the job messages by capcode
			IList<JobMessageDto> results = await Collection.Find(filter).Sort(sort).Limit(count).ToListAsync();

			// Map the dto objects to model objects and return
			List<JobMessage> messages = new List<JobMessage>();
			messages.AddRange(results.Select(i => MapDbObjectToModel(i)));

			// return the messages
			return messages;


		}

		/// <summary>
		/// Gets a JobMessage based on the id.
		/// </summary>
		/// <param name="id">The Id of the job message to get.</param>
		/// <returns>The job message if found, otherwise null.</returns>
		public new async Task<JobMessage> GetById(Guid id)
		{
			// Get the data object from the db
			JobMessageDto message = await Collection.Find(Builders<JobMessageDto>.Filter.Eq(i => i.Id, id)).SingleOrDefaultAsync();

			// return the mapped job message
			return MapDbObjectToModel(message);
		}

		/// <summary>
		/// Adds a new note to an existing job message. 
		/// </summary>
		/// <param name="jobMessageId">The id of the job message to add the note to.</param>
		/// <param name="note">The note to add to the job.</param>
		public async Task AddNoteToJobMessage(Guid jobMessageId, JobNote note)
		{

			// Validate the parameters
			if (jobMessageId == Guid.Empty)
			{
				throw new ArgumentException("The jobMessageId parameter must be a valid, non-empty guid.");
			}

			if (note == null)
			{
				throw new ArgumentNullException("note");
			}

			// Create the filter
			FilterDefinition<JobMessageDto> filter = Builders<JobMessageDto>.Filter.Eq(i => i.Id, jobMessageId);

			// Create the update
			UpdateDefinition<JobMessageDto> update = Builders<JobMessageDto>.Update.Push(i => i.Notes, note);

			// Send to mongo
			await Collection.UpdateOneAsync(filter, update);

		}

		/// <summary>
		/// Adds the progress to the job message.
		/// </summary>
		/// <param name="jobMessageId">The id of the job message to add the progress to.</param>
		/// <param name="progress">The progress to add to the job message.</param>
		/// <returns></returns>
		public async Task AddProgress(Guid jobMessageId, MessageProgress progress)
		{

			// If the progress already exists, then throw exception as we can't re-add progress.
			FilterDefinition<JobMessageDto> countFilter = Builders<JobMessageDto>.Filter.Eq(i => i.Id, jobMessageId) &
														  Builders<JobMessageDto>.Filter.ElemMatch(i => i.ProgressUpdates, p => p.ProgressType == progress.ProgressType);
			long count = await Collection.CountAsync(countFilter);

			// If there is already progress update for the type, then throw exception
			if (count > 0)
			{
				throw new ApplicationException(String.Format("The job message already contains a progress update with type '{0}'.", progress.ProgressType.GetEnumDescription()));
			}

			// Create the filter
			FilterDefinition<JobMessageDto> filter = Builders<JobMessageDto>.Filter.Eq(i => i.Id, jobMessageId);

			// Create the update
			UpdateDefinition<JobMessageDto> update = Builders<JobMessageDto>.Update.Push(i => i.ProgressUpdates, progress);

			// Do the update
			await Collection.UpdateOneAsync(filter, update);
		}

		#region Mappers

		/// <summary>
		/// Maps the JobMessageDto data object to the JobMessage model object.
		/// </summary>
		/// <param name="dbObject"></param>
		/// <returns></returns>
		public JobMessage MapDbObjectToModel(JobMessageDto dbObject)
		{

			// If the db object is null, return null
			if (dbObject == null)
			{
				return null;
			}

			JobMessage model = new JobMessage()
			{
				Capcode = dbObject.Capcode,
				Id = dbObject.Id,
				JobNumber = dbObject.JobNumber,
				MessageContent = dbObject.MessageContent,
				Priority = dbObject.Priority,
				Timestamp = dbObject.Timestamp,
				Notes = dbObject.Notes,
				ProgressUpdates = dbObject.ProgressUpdates
			};

			// Map the location property.
			if (dbObject.Location != null)
			{
				model.Location = MapLocationInfoDbObjectToModel(dbObject.Location);
			}
			else
			{
				model.Location = new LocationInfo();
			}

			// return the model
			return model;
		}

		/// <summary>
		/// Maps the JobMessage model object to the JobMessageDto data object.
		/// </summary>
		/// <param name="modelObject"></param>
		/// <returns></returns>
		public JobMessageDto MapModelToDbObject(JobMessage modelObject)
		{

			// Ensure we have a valid model object
			if (modelObject == null)
			{
				throw new ArgumentNullException("modelObject");
			}

			JobMessageDto dbObject = new JobMessageDto()
			{
				Capcode = modelObject.Capcode,
				Id = modelObject.Id,
				JobNumber = modelObject.JobNumber,
				MessageContent = modelObject.MessageContent,
				Priority = modelObject.Priority,
				Timestamp = modelObject.Timestamp,
				Notes = modelObject.Notes,
				ProgressUpdates = modelObject.ProgressUpdates
			};

			// Map the location property
			if (modelObject.Location != null)
			{
				dbObject.Location = MapLocationInfoModelToDbObject(modelObject.Location);
			} 
			else
			{
				dbObject.Location = new LocationInfoDto();
			}

			// return the db object
			return dbObject;
		}

		/// <summary>
		/// Maps the LocationInfoDto data object to the LocationInfo model object.
		/// </summary>
		/// <param name="dbObject"></param>
		/// <returns></returns>
		private LocationInfo MapLocationInfoDbObjectToModel(LocationInfoDto dbObject)
		{

			if (dbObject == null)
			{
				return null;
			}

			LocationInfo model = new LocationInfo()
			{
				AddressInfo = dbObject.AddressInfo,
				GridReference = dbObject.GridReference,
				MapPage = dbObject.MapPage,
				MapReference = dbObject.MapReference,
				MapType = dbObject.MapType
			};

			if (dbObject.Coordinates != null)
			{
				model.Coordinates = new Coordinates(dbObject.Coordinates.Latitude, dbObject.Coordinates.Longitude);
			}

			return model;
		}

		/// <summary>
		/// Maps the LocationInfo model object to the LocationInfoDto object.
		/// </summary>
		/// <param name="location"></param>
		/// <returns></returns>
		private LocationInfoDto MapLocationInfoModelToDbObject(LocationInfo modelObject)
		{

			// Ensure we have a valid model object
			if (modelObject == null)
			{
				throw new ArgumentNullException("modelObject");
			}

			LocationInfoDto dbObject = new LocationInfoDto()
			{
				AddressInfo = modelObject.AddressInfo,
				GridReference = modelObject.GridReference,
				MapPage = modelObject.MapPage,
				MapReference = modelObject.MapReference,
				MapType = modelObject.MapType
			};

			// Set the coordinates field if not null
			if (modelObject.Coordinates != null)
			{
				dbObject.Coordinates = new GeoJson2DGeographicCoordinates(modelObject.Coordinates.Longitude, modelObject.Coordinates.Latitude);
			}

			// return the db object
			return dbObject;
		}

		#endregion

	}
}
