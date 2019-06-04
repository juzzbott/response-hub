using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Messages;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{
    public class JobCodeRepository : IJobCodeRepository
    {

        /// <summary>
		/// The MongoDB collection for the current repository.
		/// </summary>
		public IMongoCollection<JobCode> Collection;

        /// <summary>
        /// Contains the reference to the mongo client object.
        /// </summary>
        private MongoClient _mongoClient;

        /// <summary>
        /// The database to get from the mongo server. The name of the database is contained within the mongo connection string.
        /// </summary>
        protected IMongoDatabase _mongoDb;

        public JobCodeRepository() : this(ConfigurationManager.ConnectionStrings["MongoServer"].ConnectionString)
        {
        }

        /// <summary>
        /// Creates a new instance of the MongoRepository object.
        /// </summary>
        public JobCodeRepository(string connectionString)
        {

            // Change the mongo defaults to use standard binary guid instead of legacy guid.
            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;

            // Load the configuration to read the connection string
            MongoUrl mongoUrl = new MongoUrl(connectionString);

            // Specify the mongo client, db and collection objects.
            _mongoClient = new MongoClient(mongoUrl);
            _mongoDb = _mongoClient.GetDatabase(mongoUrl.DatabaseName);

            string collectionName = "job_codes";
            Collection = _mongoDb.GetCollection<JobCode>(collectionName);
        }

        public async Task<IList<JobCode>> GetAll()
        {
            return await Collection.Find<JobCode>(new BsonDocument()).ToListAsync();
        }
    }
}
