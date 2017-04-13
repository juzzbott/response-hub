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
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Spatial;
using MongoDB.Bson.Serialization;

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
		public async Task<IList<JobMessage>> GetMostRecent(IEnumerable<string> capcodes, MessageType messageTypes, int count, int skip)
		{
			// return the messages without date filters
			return await GetMessagesBetweenDates(capcodes, messageTypes, count, skip, null, null);
		}

		/// <summary>
		///  Gets the job messages for the list of capcodes specified between the specific dates. Results are limited to count number of items.
		/// </summary>
		/// <param name="capcodes"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public async Task<IList<JobMessage>> GetMessagesBetweenDates(IEnumerable<string> capcodes, MessageType messageTypes, int count, int skip, DateTime? dateFrom, DateTime? dateTo)
		{
			
			// Create the filter and sort
			FilterDefinitionBuilder<JobMessageDto> builder = Builders<JobMessageDto>.Filter;
			FilterDefinition<JobMessageDto> filter = builder.In(i => i.Capcode, capcodes);

			// If there is dateFrom and dateTo filters, add them
			if (dateFrom.HasValue)
			{
				filter &= builder.Gte(i => i.Timestamp, dateFrom.Value.ToUniversalTime());
			}
			if (dateTo.HasValue)
			{
				filter &= builder.Lte(i => i.Timestamp, dateTo.Value.ToUniversalTime());
			}


			// Add the message type to the filter.
			FilterDefinition<JobMessageDto> priorityFilter = builder.Or();
			bool prioritySet = false;
			if (messageTypes.HasFlag(MessageType.Job))
			{
				priorityFilter = priorityFilter | (builder.Eq(i => i.Priority, MessagePriority.Emergency) | builder.Eq(i => i.Priority, MessagePriority.NonEmergency));
				prioritySet = true;
			}
			if (messageTypes.HasFlag(MessageType.Message))
			{
				priorityFilter = priorityFilter | builder.Eq(i => i.Priority, MessagePriority.Administration);
				prioritySet = true;
			}
			if (prioritySet)
			{
				filter &= priorityFilter;
			}

			// Create the sort filter
			SortDefinition<JobMessageDto> sort = Builders<JobMessageDto>.Sort.Descending(i => i.Timestamp);

			// Find the job messages by capcode
			IList<JobMessageDto> results = await Collection.Find(filter).Sort(sort).Limit(count).Skip(skip).ToListAsync();

			// Map the dto objects to model objects and return
			List<JobMessage> messages = new List<JobMessage>();
			messages.AddRange(results.Select(i => MapDbObjectToModel(i)));

			// return the messages
			return messages;
		}

		/// <summary>
		/// Gets the most recent job messages, limited by count and skip.
		/// </summary>
		/// <param name="count"></param>
		/// <param name="skip"></param>
		/// <returns></returns>
		public async Task<IList<JobMessage>> GetMostRecent(int count, int skip)
		{
			// Create the sort filter
			SortDefinition<JobMessageDto> sort = Builders<JobMessageDto>.Sort.Descending(i => i.Timestamp);

			// Find the job messages by capcode
			IList<JobMessageDto> results = await Collection.Find(new BsonDocument()).Sort(sort).Limit(count).Skip(skip).ToListAsync();

			// Map the dto objects to model objects and return
			List<JobMessage> messages = new List<JobMessage>();
			messages.AddRange(results.Select(i => MapDbObjectToModel(i)));

			// return the messages
			return messages;

		}

		/// <summary>
		/// Gets the list of latest messages that are new since the last message.
		/// </summary>
		/// <param name="lastId"></param>
		/// <param name="capcodes"></param>
		/// <param name="messageTypes"></param>
		/// <returns></returns>
		public async Task<IList<JobMessage>> GetMostRecent(Guid lastId)
		{
			// Get the 'Created' date from the last message id.
			JobMessageDto lastMessage = await Collection.Find(i => i.Id == lastId).SingleOrDefaultAsync();

			// If the last message cannot be found, return empty list
			if (lastMessage == null)
			{
				return new List<JobMessage>();
			}

			// Get the last message date time
			DateTime lastMessageDate = lastMessage.Timestamp;

			// Create the filter and sort
			FilterDefinitionBuilder<JobMessageDto> builder = Builders<JobMessageDto>.Filter;
			FilterDefinition<JobMessageDto> filter = builder.Gt(i => i.Timestamp, lastMessageDate);

			// Create the sort filter
			SortDefinition<JobMessageDto> sort = Builders<JobMessageDto>.Sort.Descending(i => i.Timestamp);

			// Find the job messages by capcode
			IList<JobMessageDto> results = await Collection.Find(filter).Sort(sort).Limit(200).ToListAsync();

			// Map the dto objects to model objects and return
			List<JobMessage> messages = new List<JobMessage>();
			messages.AddRange(results.Select(i => MapDbObjectToModel(i)));

			// return the messages
			return messages;

		}

		/// <summary>
		/// Gets the list of latest messages that are new since the last message. The results are limited to the selected message types and capcodes.
		/// </summary>
		/// <param name="lastId"></param>
		/// <param name="capcodes"></param>
		/// <param name="messageTypes"></param>
		/// <returns></returns>
		public async Task<IList<JobMessage>> GetLatestFromLastMessage(Guid lastId, IEnumerable<string> capcodes, MessageType messageTypes)
		{
			// Get the 'Created' date from the last message id.
			JobMessageDto lastMessage = await Collection.Find(i => i.Id == lastId).SingleOrDefaultAsync();

			// If the last message cannot be found, return empty list
			if (lastMessage == null)
			{
				return new List<JobMessage>();
			}

			// Get the last message date time
			DateTime lastMessageDate = lastMessage.Timestamp;

			// Create the filter and sort
			FilterDefinitionBuilder<JobMessageDto> builder = Builders<JobMessageDto>.Filter;
			FilterDefinition<JobMessageDto> filter = builder.In(i => i.Capcode, capcodes);

			// Add the message type to the filter.
			FilterDefinition<JobMessageDto> priorityFilter = builder.Or();
			bool prioritySet = false;
			if (messageTypes.HasFlag(MessageType.Job))
			{
				priorityFilter = priorityFilter | (builder.Eq(i => i.Priority, MessagePriority.Emergency) | builder.Eq(i => i.Priority, MessagePriority.NonEmergency));
				prioritySet = true;
			}
			if (messageTypes.HasFlag(MessageType.Message))
			{
				priorityFilter = priorityFilter | builder.Eq(i => i.Priority, MessagePriority.Administration);
				prioritySet = true;
			}
			if (prioritySet)
			{
				filter &= priorityFilter;
			}

			// Add the date time filter
			filter = filter & builder.Gt(i => i.Timestamp, lastMessageDate);

			// Create the sort filter
			SortDefinition<JobMessageDto> sort = Builders<JobMessageDto>.Sort.Descending(i => i.Timestamp);

			// Find the job messages by capcode
			IList<JobMessageDto> results = await Collection.Find(filter).Sort(sort).Limit(200).ToListAsync();

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
		/// Gets a JobMessage based on the job number.
		/// </summary>
		/// <param name="jobNumber">The number of the job message to get.</param>
		/// <returns>The job message if found, otherwise null.</returns>
		public async Task<JobMessage> GetByJobNumber(string jobNumber)
		{
			// Get the data object from the db
			JobMessageDto message = await Collection.Find(Builders<JobMessageDto>.Filter.Eq(i => i.JobNumber, jobNumber.ToUpper())).FirstOrDefaultAsync();

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
		/// Gets the notes for specific job.
		/// </summary>
		/// <param name="jobMessageId">The ID of the job to get the notes for.</param>
		/// <returns>The job notes collection.</returns>
		public async Task<IList<JobNote>> GetNotesForJob(Guid jobMessageId)
		{

			// Create the filter
			FilterDefinition<JobMessageDto> filter = Builders<JobMessageDto>.Filter.Eq(i => i.Id, jobMessageId);

			// Create the projection
			ProjectionDefinition<JobMessageDto> projection = Builders<JobMessageDto>.Projection.Include(i => i.Notes);

			// Get the list of job notes from the results
			BsonDocument results = await Collection.Find(filter).Project(projection).FirstOrDefaultAsync();
			BsonArray notes = results["Notes"].AsBsonArray;

			// Create the list of notes
			IList<JobNote> notesList = new List<JobNote>();

			// Deserialise the note
			foreach(BsonValue note in notes)
			{
				notesList.Add(BsonSerializer.Deserialize<JobNote>(note.AsBsonDocument));
			}

			return notesList;


		}

		/// <summary>
		/// Adds the progress to the job message.
		/// </summary>
		/// <param name="jobMessageId">The id of the job message to add the progress to.</param>
		/// <param name="progress">The progress to add to the job message.</param>
		/// <returns></returns>
		public async Task SaveProgress(Guid jobMessageId, MessageProgress progress)
		{

			// If the progress already exists, then update the timestamp of the existing progress update.
			FilterDefinition<JobMessageDto> countFilter = Builders<JobMessageDto>.Filter.Eq(i => i.Id, jobMessageId) &
														  Builders<JobMessageDto>.Filter.ElemMatch(i => i.ProgressUpdates, p => p.ProgressType == progress.ProgressType);
			long count = await Collection.CountAsync(countFilter);

			// Create the filter and update
			FilterDefinition<JobMessageDto> filter;
			UpdateDefinition<JobMessageDto> update;

			// If there is already progress update for the type, then throw exception
			if (count > 0)
			{
				// Create the filter
				filter = Builders<JobMessageDto>.Filter.Eq(i => i.Id, jobMessageId) &
						 Builders<JobMessageDto>.Filter.ElemMatch(i => i.ProgressUpdates, p => p.ProgressType == progress.ProgressType);

				// Create the update
				update = Builders<JobMessageDto>.Update.Set("ProgressUpdates.$.Timestamp", progress.Timestamp).Set("ProgressUpdates.$.UserId", progress.UserId);
			}
			else
			{

				// Create the filter
				filter = Builders<JobMessageDto>.Filter.Eq(i => i.Id, jobMessageId);

				// Create the update
				update = Builders<JobMessageDto>.Update.Push(i => i.ProgressUpdates, progress);

			}

			// Do the update
			await Collection.UpdateOneAsync(filter, update);
		}

		/// <summary>
		/// Removes the specified progress update type from the job.
		/// </summary>
		/// <param name="jobMessageId">The id of the job to remove the progres from.</param>
		/// <param name="progressType">The progress type to remove.</param>
		/// <returns></returns>
		public async Task RemoveProgress(Guid jobMessageId, MessageProgressType progressType)
		{
			// If the progress already exists, then throw exception as we can't re-add progress.
			FilterDefinition<JobMessageDto> filter = Builders<JobMessageDto>.Filter.Eq(i => i.Id, jobMessageId);

			// Create the update
			UpdateDefinition<JobMessageDto> update = Builders<JobMessageDto>.Update.PullFilter(i => i.ProgressUpdates, x => x.ProgressType == progressType);

			// perform the update
			await Collection.UpdateOneAsync(filter, update);
		}

		/// <summary>
		/// Adds the specified attachment id to the job attachment list.
		/// </summary>
		/// <param name="jobMessageId">The ID of the job to store the attachment against.</param>
		/// <param name="attachmentId">The ID of the attachment to store.</param>
		/// <returns></returns>
		public async Task AddAttachmentToJob(Guid jobMessageId, Guid attachmentId)
		{
			FilterDefinition<JobMessageDto> filter = Builders<JobMessageDto>.Filter.Eq(i => i.Id, jobMessageId);

			// Create the update
			UpdateDefinition<JobMessageDto> update = Builders<JobMessageDto>.Update.Push(i => i.AttachmentIds, attachmentId);

			// Do the update
			await Collection.UpdateOneAsync(filter, update);
		}

		/// <summary>
		/// Removes teh attachment from the job message.
		/// </summary>
		/// <param name="jobMessageId">The job message id to remove the attachment from.</param>
		/// <param name="attachmentId">The id of the attachment to remove from the job.</param>
		/// <returns></returns>
		public async Task RemoveAttachmentFromJob(Guid jobMessageId, Guid attachmentId)
		{
			FilterDefinition<JobMessageDto> filter = Builders<JobMessageDto>.Filter.Eq(i => i.Id, jobMessageId);

			// Create the update
			UpdateDefinition<JobMessageDto> update = Builders<JobMessageDto>.Update.Pull(i => i.AttachmentIds, attachmentId);

			// Do the update
			await Collection.UpdateOneAsync(filter, update);
		}

		#region Text search

		public async Task<PagedResultSet<JobMessage>> FindByKeyword(string keywords, IEnumerable<string> capcodes, MessageType messageTypes, DateTime dateFrom, DateTime dateTo, int limit, int skip, bool countTotal)
		{

			// Create the search filter
			FilterDefinitionBuilder<JobMessageDto> builder = Builders<JobMessageDto>.Filter;
			FilterDefinition<JobMessageDto> filter = builder.Text(keywords);

			if (capcodes != null)
			{
				filter = filter & builder.In(i => i.Capcode, capcodes);
			}

			// Add the message type to the filter.
			FilterDefinition<JobMessageDto> priorityFilter = builder.Or();
			bool prioritySet = false;
			if (messageTypes.HasFlag(MessageType.Job))
			{
				priorityFilter = priorityFilter | (builder.Eq(i => i.Priority, MessagePriority.Emergency) | builder.Eq(i => i.Priority, MessagePriority.NonEmergency));
				prioritySet = true;
			}
			if (messageTypes.HasFlag(MessageType.Message))
			{
				priorityFilter = priorityFilter | builder.Eq(i => i.Priority, MessagePriority.Administration);
				prioritySet = true;
			}
			if (prioritySet)
			{
				filter &= priorityFilter;
			}

			// Reset DateTo to 23:59:59 of that date and dateFrom to 00:00:00 so that the entire day is captured
			dateFrom = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day, 0, 0, 0);
			dateTo = new DateTime(dateTo.Year, dateTo.Month, dateTo.Day, 23, 59, 59);

			// Add the date filters
			filter = filter & builder.Gte(i => i.Timestamp, dateFrom);
			filter = filter & builder.Lte(i => i.Timestamp, dateTo);

			long totalCount = 0;
			if (countTotal)
			{
				totalCount = await Collection.Find<JobMessageDto>(filter).CountAsync();
			}

			// Create the sort definition
			SortDefinition<JobMessageDto> sort = Builders<JobMessageDto>.Sort.Descending(i => i.Timestamp);

			// Return the find results.
			IList<JobMessageDto> results = await Collection.Find<JobMessageDto>(filter).Sort(sort).Skip(skip).Limit(limit).ToListAsync();

			// Create the result object and return it
			PagedResultSet<JobMessage> resultSet = new PagedResultSet<JobMessage>()
			{
				Items = results.Select(i => MapDbObjectToModel(i)).ToList(),
				Limit = limit,
				Skip = skip,
				TotalResults = (int)totalCount
			};

			return resultSet;

		}

		#endregion
		
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
				ProgressUpdates = dbObject.ProgressUpdates,
				AttachmentIds = dbObject.AttachmentIds,
				Type = dbObject.Type
				
			};

			// Map the location property.
			if (dbObject.Location != null)
			{
				model.Location = MapLocationInfoDbObjectToModel(dbObject.Location);
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
				ProgressUpdates = modelObject.ProgressUpdates,
				AttachmentIds = modelObject.AttachmentIds,
				Type = modelObject.Type
			};

			// Map the location property
			if (modelObject.Location != null)
			{
				dbObject.Location = MapLocationInfoModelToDbObject(modelObject.Location);
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
				Address = dbObject.Address,
				GridSquare = dbObject.GridSqaure,
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
				Address = modelObject.Address,
				GridSqaure = modelObject.GridSquare,
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
