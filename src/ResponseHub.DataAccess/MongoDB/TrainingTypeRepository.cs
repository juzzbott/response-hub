using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Training;

using MongoDB.Bson;
using MongoDB.Driver;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{

	[MongoCollectionName("training_types")]
	public class TrainingTypeRepository : MongoRepository<TrainingType>, ITrainingTypeRepository
	{

		/// <summary>
		/// Gets all the training types in the database.
		/// </summary>
		/// <returns></returns>
		public override async Task<IList<TrainingType>> GetAll()
		{
			// create the sort definition
			SortDefinition<TrainingType> sort = Builders<TrainingType>.Sort.Ascending(i => i.SortOrder).Ascending(i => i.Name);

			// return the list of training types
			return await Collection.Find(new BsonDocument()).Sort(sort).ToListAsync();

		}

	}
}
