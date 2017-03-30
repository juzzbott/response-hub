using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Training;

using MongoDB.Driver;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{

	[MongoCollectionName("training_sessions")]
	public class TrainingSessionRepository : MongoRepository<TrainingSession>, ITrainingSessionRepository
	{

		/// <summary>
		/// Gets the list of training sessions for the specific group.
		/// </summary>
		/// <param name="groupId">The id of the group to get the training sessions for.</param>
		/// <returns>The list of training sessions.</returns>
		public async Task<IList<TrainingSession>> GetTrainingSessionsForGroup(Guid groupId)
		{
			FilterDefinition<TrainingSession> filter = Builders<TrainingSession>.Filter.Eq(i => i.GroupId, groupId);

			return await Collection.Find(filter).ToListAsync();
		}

		/// <summary>
		/// Gets the list of training sessions for the specific group.
		/// </summary>
		/// <param name="groupId">The id of the group to get the training sessions for.</param>
		/// <returns>The list of training sessions.</returns>
		public async Task<IList<TrainingSession>> GetTrainingSessionsForGroup(Guid groupId, int limit)
		{
			FilterDefinition<TrainingSession> filter = Builders<TrainingSession>.Filter.Eq(i => i.GroupId, groupId);
			SortDefinition<TrainingSession> sort = Builders<TrainingSession>.Sort.Descending(i => i.SessionDate);

			return await Collection.Find(filter).Sort(sort).Limit(limit).ToListAsync();
		}

	}
}
