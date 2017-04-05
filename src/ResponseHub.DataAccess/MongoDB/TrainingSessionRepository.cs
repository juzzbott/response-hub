﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Training;

using MongoDB.Driver;
using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Training;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{

	[MongoCollectionName("training_sessions")]
	public class TrainingSessionRepository : MongoRepository<TrainingSessionDto>, ITrainingSessionRepository
	{

		/// <summary>
		/// Gets the list of training sessions for the specific group.
		/// </summary>
		/// <param name="groupId">The id of the group to get the training sessions for.</param>
		/// <returns>The list of training sessions.</returns>
		public async Task<IList<TrainingSession>> GetTrainingSessionsForGroup(Guid groupId, IEnumerable<TrainingType> trainingTypes)
		{
			FilterDefinition<TrainingSessionDto> filter = Builders<TrainingSessionDto>.Filter.Eq(i => i.GroupId, groupId);
			SortDefinition<TrainingSessionDto> sort = Builders<TrainingSessionDto>.Sort.Descending(i => i.SessionDate);

			// Get the results from the db
			IList<TrainingSessionDto> sessions = await Collection.Find(filter).Sort(sort).ToListAsync();

			// return the mapped results
			return sessions.Select(i => MapDbObjectToModel(i, trainingTypes)).ToList();
		}

		/// <summary>
		/// Gets the list of training sessions for the specific group.
		/// </summary>
		/// <param name="groupId">The id of the group to get the training sessions for.</param>
		/// <returns>The list of training sessions.</returns>
		public async Task<IList<TrainingSession>> GetTrainingSessionsForGroup(Guid groupId, int limit, IEnumerable<TrainingType> trainingTypes)
		{
			FilterDefinition<TrainingSessionDto> filter = Builders<TrainingSessionDto>.Filter.Eq(i => i.GroupId, groupId);
			SortDefinition<TrainingSessionDto> sort = Builders<TrainingSessionDto>.Sort.Descending(i => i.SessionDate);

			// Get the results from the db
			IList<TrainingSessionDto> sessions = await Collection.Find(filter).Sort(sort).Limit(limit).ToListAsync();

			// return the mapped results
			return sessions.Select(i => MapDbObjectToModel(i, trainingTypes)).ToList();
		}

		/// <summary>
		/// Gets the specific training session based on the id.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="trainingTypes"></param>
		/// <returns></returns>
		public async Task<TrainingSession> GetById(Guid id, IEnumerable<TrainingType> trainingTypes)
		{
			// Create the filter
			FilterDefinition<TrainingSessionDto> filter = Builders<TrainingSessionDto>.Filter.Eq(i => i.Id, id);

			// Get the result from the db
			TrainingSessionDto result = await Collection.Find(filter).FirstOrDefaultAsync();

			// return the result
			return MapDbObjectToModel(result, trainingTypes);
		}

		/// <summary>
		/// Adds the training session to the database.
		/// </summary>
		/// <param name="session">The session to save in the database.</param>
		/// <returns></returns>
		public async Task Add(TrainingSession session)
		{
			// Map to the data object
			TrainingSessionDto sessionDto = MapModelObjectToDb(session);

			// Insert the session
			await Collection.InsertOneAsync(sessionDto);

		}

		/// <summary>
		/// Saves the changes to the training session to the database.
		/// </summary>
		/// <param name="session"></param>
		/// <returns></returns>
		public async Task Save(TrainingSession session)
		{
			// Create the filter
			FilterDefinition<TrainingSessionDto> filter = Builders<TrainingSessionDto>.Filter.Eq(i => i.Id, session.Id);

			// Save the object to the collection.
			await Collection.ReplaceOneAsync(filter, MapModelObjectToDb(session), new UpdateOptions() { IsUpsert = true });
		}

		#region Mappers

		/// <summary>
		/// Maps the training session data object to the model object.
		/// </summary>
		/// <param name="dbObject"></param>
		/// <param name="trainingTypes"></param>
		/// <returns></returns>
		private TrainingSession MapDbObjectToModel(TrainingSessionDto dbObject, IEnumerable<TrainingType> trainingTypes)
		{

			// Ensure the model object exists
			if (dbObject == null)
			{
				return null;
			}

			return new TrainingSession()
			{
				Created = dbObject.Created,
				Description = dbObject.Description,
				Duration = dbObject.Duration,
				GroupId = dbObject.GroupId,
				Id = dbObject.Id,
				Members = dbObject.Members,
				Name = dbObject.Name,
				SessionDate = dbObject.SessionDate,
				SessionType = dbObject.SessionType,
				Trainers = dbObject.Trainers,
				TrainingTypes = trainingTypes.Where(i => dbObject.TrainingTypeIds.Contains(i.Id)).ToList()
			};
		}

		/// <summary>
		/// Maps the training session data object to the model object.
		/// </summary>
		/// <param name="modelObject"></param>
		/// <param name="trainingTypes"></param>
		/// <returns></returns>
		private TrainingSessionDto MapModelObjectToDb(TrainingSession modelObject)
		{

			// Ensure the model object exists
			if (modelObject == null)
			{
				return null;
			}

			return new TrainingSessionDto()
			{
				Created = modelObject.Created,
				Description = modelObject.Description,
				Duration = modelObject.Duration,
				GroupId = modelObject.GroupId,
				Id = modelObject.Id,
				Members = modelObject.Members,
				Name = modelObject.Name,
				SessionDate = modelObject.SessionDate,
				SessionType = modelObject.SessionType,
				Trainers = modelObject.Trainers,
				TrainingTypeIds = modelObject.TrainingTypes.Select(i => i.Id).ToList()
			};
		}

		#endregion

	}
}
