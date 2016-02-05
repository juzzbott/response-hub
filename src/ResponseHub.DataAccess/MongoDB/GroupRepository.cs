using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;

using Enivate.ResponseHub.Logging;

using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Groups.Interface;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{

	[MongoCollectionName("groups")]
	public class GroupRepository : MongoRepository<Group>, IGroupRepository
	{

		/// <summary>
		/// The ILogger that is responsible for logging data.
		/// </summary>
		private ILogger _logger;
		
		public GroupRepository(ILogger logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Creates the group in the database.
		/// </summary>
		/// <param name="group">The group to create.</param>
		/// <returns>The saved group.</returns>
		public async Task<Group> CreateGroup(Group group)
		{

			// Debug logging
			await _logger.Debug(String.Format("New Group created. Id: {0} - Name {1}", group.Id, group.Name));

			// Save the group to the database.
			group = await Save(group);

			// return the group
			return group;
		}

		/// <summary>
		/// Finds the most recently created groups and limits them by 'count'.
		/// </summary>
		/// <param name="count">The limit of results to return from the database query.</param>
		/// <returns>The most recent groups found.</returns>
		public async Task<IList<Group>> GetRecentlyAdded(int count)
		{

			// Find most recent groups and limit by count
			IList<Group> groups = await Collection.Find(new BsonDocument()).Sort(Builders<Group>.Sort.Descending(i => i.Created)).Limit(count).ToListAsync();

			// return the groups found in the database.
			return groups;

		}

		public async Task<bool> CheckIfGroupExists(string name, ServiceType service)
		{
			Group group = await FindOne(i => i.Name.ToUpper() == name.ToUpper() && i.Service == service);

			return (group != null);
		}

	}
}
