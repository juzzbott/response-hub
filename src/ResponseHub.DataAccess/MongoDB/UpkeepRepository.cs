using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Upkeep;
using MongoDB.Driver;
using System.Configuration;
using MongoDB.Bson;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{
	public class UpkeepRepository : IUpkeepRepository
	{

		/// <summary>
		/// The MongoDB collection for the current repository.
		/// </summary>
		private IMongoCollection<Asset> _assetCollection;

		/// <summary>
		/// The MongoDB collection for the tasks repository.
		/// </summary>
		private IMongoCollection<UpkeepTask> _tasksCollection;

		/// <summary>
		/// The MongoDB collection for the upkeep maintentace reports repository.
		/// </summary>
		private IMongoCollection<UpkeepReport> _reportsCollection;

		/// <summary>
		/// Contains the reference to the mongo client object.
		/// </summary>
		private MongoClient _mongoClient;

		/// <summary>
		/// The database to get from the mongo server. The name of the database is contained within the mongo connection string.
		/// </summary>
		private IMongoDatabase _mongoDb;

		private const string _assetCollectionName = "assets";

		private const string _tasksCollectionName = "tasks";

		private const string _reportsCollectionName = "maintenance_reports";

		public UpkeepRepository() : this(ConfigurationManager.ConnectionStrings["MongoServer"].ConnectionString)
		{
		}

		public UpkeepRepository(string connectionString)
		{
			// Change the mongo defaults to use standard binary guid instead of legacy guid.
			BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;

			// Load the configuration to read the connection string
			MongoUrl mongoUrl = new MongoUrl(connectionString);

			// Specify the mongo client, db and collection objects.
			_mongoClient = new MongoClient(mongoUrl);
			_mongoDb = _mongoClient.GetDatabase(mongoUrl.DatabaseName);

			_assetCollection = _mongoDb.GetCollection<Asset>(_assetCollectionName);
			_tasksCollection = _mongoDb.GetCollection<UpkeepTask>(_tasksCollectionName);
			_reportsCollection = _mongoDb.GetCollection<UpkeepReport>(_reportsCollectionName);
		}

		/// <summary>
		/// Saves an asset to the database. If the asset does not exist, it is created.
		/// </summary>
		/// <param name="asset">The asset to add to the database</param>
		public async Task<Asset> SaveAsset(Asset asset)
		{

			// Save the object to the collection.
			ReplaceOneResult result = await _assetCollection.ReplaceOneAsync(Builders<Asset>.Filter.Eq(i => i.Id, asset.Id), asset, new UpdateOptions() { IsUpsert = true });

			// return the saved user object.
			return asset;
		}

		/// <summary>
		/// Gets an asset from the database based on the id.
		/// </summary>
		/// <param name="id">The id of the asset to get from the database.</param>
		/// <returns>The asset based on the id.</returns>
		public async Task<Asset> GetAssetById(Guid id)
		{
			// Create the filter
			FilterDefinition<Asset> filter = Builders<Asset>.Filter.Eq(i => i.Id, id);

			// Get the asset based on the filter
			return await _assetCollection.Find(filter).FirstOrDefaultAsync();
		}

		/// <summary>
		/// Gets the list of assets for the specific unit id. 
		/// </summary>
		/// <param name="unitId">The Id of the unit to get the assets for.</param>
		/// <returns>The list of assets for the unit.</returns>
		public async Task<IList<Asset>> GetAssetsByUnitId(Guid unitId)
		{
			// Create the filter
			FilterDefinition<Asset> filter = Builders<Asset>.Filter.Eq(i => i.UnitId, unitId) & Builders<Asset>.Filter.Eq(i => i.Deleted, false);

			// Get the asset based on the filter
			return await _assetCollection.Find(filter).ToListAsync();
		}

		/// <summary>
		/// Marks the specified asset as deleted within the database. Cannot delete the asset as it be needed for reporting. 
		/// </summary>
		/// <param name="assetId">The ID of the asset to mark for deletion. </param>
		/// <returns></returns>
		public async Task DeleteAsset(Guid assetId)
		{
			// Create the filter
			FilterDefinition<Asset> filter = Builders<Asset>.Filter.Eq(i => i.Id, assetId);

			// Create the update
			UpdateDefinition<Asset> update = Builders<Asset>.Update.Set(i => i.Deleted, true);

			// Send to mongo
			await _assetCollection.UpdateOneAsync(filter, update);
		}

		/// <summary>
		/// Saves a task to the database. If the task does not exist, it is created.
		/// </summary>
		/// <param name="task">The task to add to the database</param>
		public async Task<UpkeepTask> SaveTask(UpkeepTask task)
		{

			// Save the object to the collection.
			ReplaceOneResult result = await _tasksCollection.ReplaceOneAsync(Builders<UpkeepTask>.Filter.Eq(i => i.Id, task.Id), task, new UpdateOptions() { IsUpsert = true });

			// return the saved user object.
			return task;
		}

		/// <summary>
		/// Gets an task from the database based on the id.
		/// </summary>
		/// <param name="id">The id of the task to get from the database.</param>
		/// <returns>The task based on the id.</returns>
		public async Task<UpkeepTask> GetTaskById(Guid id)
		{
			// Create the filter
			FilterDefinition<UpkeepTask> filter = Builders<UpkeepTask>.Filter.Eq(i => i.Id, id);

			// Get the task based on the filter
			return await _tasksCollection.Find(filter).FirstOrDefaultAsync();
		}

		/// <summary>
		/// Gets the list of tasks for the specific unit id. 
		/// </summary>
		/// <param name="unitId">The Id of the unit to get the tasks for.</param>
		/// <returns>The list of tasks for the unit.</returns>
		public async Task<IList<UpkeepTask>> GetTasksByUnitId(Guid unitId)
		{
			// Create the filter
			FilterDefinition<UpkeepTask> filter = Builders<UpkeepTask>.Filter.Eq(i => i.UnitId, unitId) & Builders<UpkeepTask>.Filter.Eq(i => i.Deleted, false);

			// Get the asset based on the filter
			return await _tasksCollection.Find(filter).ToListAsync();
		}

		/// <summary>
		/// Marks the specified task as deleted within the database. Cannot delete the task as it be needed for reporting. 
		/// </summary>
		/// <param name="taskId">The ID of the task to mark for deletion. </param>
		/// <returns></returns>
		public async Task DeleteTask(Guid taskId)
		{
			// Create the filter
			FilterDefinition<UpkeepTask> filter = Builders<UpkeepTask>.Filter.Eq(i => i.Id, taskId);

			// Create the update
			UpdateDefinition<UpkeepTask> update = Builders<UpkeepTask>.Update.Set(i => i.Deleted, true);

			// Send to mongo
			await _tasksCollection.UpdateOneAsync(filter, update);
		}

		public async Task<UpkeepReport> SaveReport(UpkeepReport report)
		{
			// Save the object to the collection.
			ReplaceOneResult result = await _reportsCollection.ReplaceOneAsync(Builders<UpkeepReport>.Filter.Eq(i => i.Id, report.Id), report, new UpdateOptions() { IsUpsert = true });

			// return the saved user object.
			return report;
		}
	}
}
