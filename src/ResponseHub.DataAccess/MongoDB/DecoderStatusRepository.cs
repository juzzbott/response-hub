using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Decoding;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{
	public class DecoderStatusRepository : IDecoderStatusRepository
	{
		
		/// <summary>
		/// The MongoDB collection for the current repository.
		/// </summary>
		private IMongoCollection<DecoderStatus> _collection;

		/// <summary>
		/// Contains the reference to the mongo client object.
		/// </summary>
		private MongoClient _mongoClient;

		/// <summary>
		/// The database to get from the mongo server. The name of the database is contained within the mongo connection string.
		/// </summary>
		private IMongoDatabase _mongoDb;

		private const string _collectionName = "decoder_status";

		public DecoderStatusRepository() : this(ConfigurationManager.ConnectionStrings["MongoServer"].ConnectionString)
		{
		}

		public DecoderStatusRepository(string connectionString)
		{
			// Change the mongo defaults to use standard binary guid instead of legacy guid.
			BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;

			// Load the configuration to read the connection string
			MongoUrl mongoUrl = new MongoUrl(connectionString);

			// Specify the mongo client, db and collection objects.
			_mongoClient = new MongoClient(mongoUrl);
			_mongoDb = _mongoClient.GetDatabase(mongoUrl.DatabaseName);

			_collection = _mongoDb.GetCollection<DecoderStatus>(_collectionName);
		}

		/// <summary>
		/// Gets the decoder status from the database.
		/// </summary>
		/// <returns></returns>
		public async Task<DecoderStatus> GetDecoderStatus()
		{
			// return only the first decoder status, as it's a single item collection
			return await _collection.Find(new BsonDocument()).FirstOrDefaultAsync();
		}

		/// <summary>
		/// Sets the last cleanup operation timestamp.
		/// </summary>
		/// <param name="timestamp">The timestamp of the last cleanup operation</param>
		public async Task SetLastCleanupOperationTimestamp(DateTime timestamp)
		{

			// Create the update definition
			UpdateDefinition<DecoderStatus> update = Builders<DecoderStatus>.Update.Set(i => i.LastCleanOperation, timestamp);

			// Perform the update
			await _collection.UpdateOneAsync(new BsonDocument(), update);

		}

		/// <summary>
		/// Sets the last email warning timestamp.
		/// </summary>
		/// <param name="timestamp">The timestamp of the last email warning</param>
		public async Task SetLastEmailWarningTimestamp(DateTime timestamp)
		{
			
			// Create the update definition
			UpdateDefinition<DecoderStatus> update = Builders<DecoderStatus>.Update.Set(i => i.LastEmailWarning, timestamp);

			// Perform the update
			await _collection.UpdateOneAsync(new BsonDocument(), update);
		}

		/// <summary>
		/// Adds an invalid message to the database.
		/// </summary>
		/// <param name="timestamp">The timestamp of the invalid message</param>
		/// <param name="invalidMessage">The content of the invalid message</param>
		public async Task AddInvalidMessage(DateTime timestamp, string invalidMessage)
		{

			// Create the key value pair
			KeyValuePair<DateTime, string> kvpInvalidMessage = new KeyValuePair<DateTime, string>(timestamp, invalidMessage);

			// Create the update definition
			UpdateDefinition<DecoderStatus> update = Builders<DecoderStatus>.Update.Push(i => i.InvalidMessages, kvpInvalidMessage);

			// Perform the update
			await _collection.UpdateOneAsync(new BsonDocument(), update);
		}

		/// <summary>
		/// Clears the list of invalid messages from the database.
		/// </summary>
		/// <returns></returns>
		public async Task ClearInvalidMessages()
		{

			// Create the update definition to clear the invalid messages
			UpdateDefinition<DecoderStatus> update = Builders<DecoderStatus>.Update.Set(i => i.InvalidMessages, new List<KeyValuePair<DateTime, string>>());

			// Perform the update
			await _collection.UpdateOneAsync(new BsonDocument(), update);

		}

		/// <summary>
		/// Clears the existing decoder status values and resets it to default values.
		/// </summary>
		/// <returns></returns>
		public async Task ResetDecoderStatus()
		{

			// Clear any existing records
			await _collection.DeleteManyAsync(new BsonDocument());

			// Insert a blank new document
			await _collection.InsertOneAsync(new DecoderStatus());

		}
	}
}
