using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Enivate.ResponseHub.DataAccess
{
	public class MongoRepository<T> : IRepository<T> where T : IEntity
	{

		/// <summary>
		/// The MongoDB collection for the current repository.
		/// </summary>
		public IMongoCollection<T> Collection;

		/// <summary>
		/// Contains the reference to the mongo client object.
		/// </summary>
		private MongoClient _mongoClient;

		/// <summary>
		/// The database to get from the mongo server. The name of the database is contained within the mongo connection string.
		/// </summary>
		protected IMongoDatabase _mongoDb;

		public MongoRepository() : this(ConfigurationManager.ConnectionStrings["MongoServer"].ConnectionString)
		{
		}

		/// <summary>
		/// Creates a new instance of the MongoRepository object.
		/// </summary>
		public MongoRepository(string connectionString)
		{

			// Change the mongo defaults to use standard binary guid instead of legacy guid.
			BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;

			// Load the configuration to read the connection string
			MongoUrl mongoUrl = new MongoUrl(connectionString);

			// Specify the mongo client, db and collection objects.
			_mongoClient = new MongoClient(mongoUrl);
			_mongoDb = _mongoClient.GetDatabase(mongoUrl.DatabaseName);

			string collectionName = GetCollectionName(typeof(T));
			Collection = _mongoDb.GetCollection<T>(collectionName);
		}

		/// <summary>
		/// Adds the user to collection.
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public virtual async Task<T> Add(T entity)
		{
			await Collection.InsertOneAsync(entity);
			return entity;
		}

		/// <summary>
		/// Returns an enumeration of all the objects in the collection.
		/// </summary>
		/// <returns></returns>
		public virtual async Task<IList<T>> GetAll()
		{
			return await Collection.Find<T>(new BsonDocument()).ToListAsync();
		}

		/// <summary>
		/// Returns an enumerable of the objects in the collection that match the query.
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public virtual async Task<IList<T>> Find(Expression<Func<T, bool>> query)
		{
			// Return the enumerable of the collection
			return await Collection.Find<T>(query).ToListAsync();
		}

		/// <summary>
		/// Returns an enumerable of the objects in the collection that match the query.
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public virtual async Task<T> FindOne(Expression<Func<T, bool>> query)
		{
			// Return the enumerable of the collection
			T result = await Collection.Find<T>(query).FirstOrDefaultAsync();

			return result;
		}

		/// <summary>
		/// Returns an IApplicationUser by ID.
		/// </summary>
		/// <param name="id">The Id of the item to return.</param>
		/// <returns>The IApplicationUser that matches the id.</returns>
		public virtual async Task<T> GetById(Guid id)
		{

			// Ensure an id is passed
			if (id == Guid.Empty)
			{
				throw new ArgumentException("The 'id' parameter cannot be an empty guid.", "id");
			}

			// Query by ID (ensure it's an object id)
			T result = await Collection.Find(Builders<T>.Filter.Eq(i => i.Id, id)).FirstOrDefaultAsync();

			return result;

		}

		/// <summary>
		/// Removes the specified user from the collection.
		/// </summary>
		/// <param name="user">The user object to remove from the collection.</param>
		public virtual async Task Remove(T entity)
		{
			// Remove the object from the collection based on the user id.
			await Collection.DeleteOneAsync<T>(i => i.Id == entity.Id);
		}

		/// <summary>
		/// Saves the object to the collection.
		/// </summary>
		/// <param name="entity">The user object to save to the collection.</param>
		/// <returns>The saved user object.</returns>
		public virtual async Task<T> Save(T entity)
		{
			// Save the object to the collection.
			ReplaceOneResult result = await Collection.ReplaceOneAsync(Builders<T>.Filter.Eq(i => i.Id, entity.Id), entity, new UpdateOptions() { IsUpsert = true });

			// return the saved user object.
			return entity;
		}


		public async Task<PagedResultSet<T>> TextSearch(string keywords, int limit, int skip, bool countTotal)
		{

			// Create the search filter
			FilterDefinition<T> filter = Builders<T>.Filter.Text(keywords);

			long totalCount = 0;
			if (countTotal)
			{
				totalCount = await Collection.Find<T>(filter).CountAsync();
			}

			// Return the find results.
			IList<T> results = await Collection.Find<T>(filter).Skip(skip).Limit(limit).ToListAsync();

			// Create the result object and return it
			PagedResultSet<T> resultSet = new PagedResultSet<T>()
			{
				Items = results,
				Limit = limit,
				Skip = skip,
				TotalResults = (int)totalCount
			};

			return resultSet;

		}

		#region Helper Functions

		/// <summary>
		/// Gets the collection name for the specific type. By default, the type name is used with the first character lowercase.
		/// Alternatively, if a CollectionName attribute exists for the class or struct, then the CollectionName value will be used in stead.
		/// </summary>
		/// <param name="type">The type of object to get the collection name for.</param>
		/// <returns>The collection name to use.</returns>
		public string GetCollectionName(Type type)
		{

			// Get the default collection name as the object with the first letter lowercase
			string collectionName = String.Format("{0}{1}", type.Name.Substring(0, 1).ToLower(), type.Name.Substring(1));

			// Get the list of attributes for the type.
			object[] attributes = this.GetType().GetCustomAttributes(typeof(MongoCollectionNameAttribute), true);

			// Ensure there is attributes
			if (attributes != null && attributes.Length > 0)
			{

				try
				{
					// Get the first
					MongoCollectionNameAttribute nameAttr = (MongoCollectionNameAttribute)attributes[0];

					// If the collection name attribute is not null or empty, then set the name from the attribute
					if (!String.IsNullOrEmpty(nameAttr.CollectionName))
					{
						collectionName = nameAttr.CollectionName;
					}

				}
				catch (Exception) { }

			}

			// return the collection name for the type.
			return collectionName;

		}

		#endregion

	}
}
