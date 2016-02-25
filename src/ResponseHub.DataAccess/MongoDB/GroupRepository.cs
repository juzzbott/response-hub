using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Groups;
using Enivate.ResponseHub.DataAccess.Interface;
using MongoDB.Driver.GeoJsonObjectModel;
using Enivate.ResponseHub.Model.Spatial;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{

	[MongoCollectionName("groups")]
	public class GroupRepository : MongoRepository<GroupDto>, IGroupRepository
	{

		/// <summary>
		/// The ILogger that is responsible for logging data.
		/// </summary>
		private ILogger _logger;
		
		public GroupRepository(ILogger logger)
		{
			_logger = logger;
		}

		public new async Task<IList<Group>> GetAll()
		{
			IList<GroupDto> allResults = await base.GetAll();
			return (IList<Group>)allResults.Select(async i => await MapDtoToModel(i)).ToList();
		}

		public new async Task<Group> GetById(Guid id)
		{
			GroupDto group = await base.GetById(id);
			return await MapDtoToModel(group);
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
			GroupDto groupDto = await Save(MapModelToDto(group));

			// return the group
			return await MapDtoToModel(groupDto);
		}

		/// <summary>
		/// Finds the most recently created groups and limits them by 'count'.
		/// </summary>
		/// <param name="count">The limit of results to return from the database query.</param>
		/// <returns>The most recent groups found.</returns>
		public async Task<IList<Group>> GetRecentlyAdded(int count)
		{

			// Find most recent groups and limit by count
			IList<GroupDto> groups = await Collection.Find(new BsonDocument()).Sort(Builders<GroupDto>.Sort.Descending(i => i.Created)).Limit(count).ToListAsync();

			// return the groups found in the database.
			IList<Group> mappedGroups = new List<Group>();
			foreach(GroupDto group in groups)
			{
				mappedGroups.Add(await MapDtoToModel(group));
			}

			// return the mapped groups
			return mappedGroups;

		}

		public async Task<bool> CheckIfGroupExists(string name, ServiceType service)
		{
			GroupDto group = await FindOne(i => i.Name.ToUpper() == name.ToUpper() && i.Service == service);

			return (group != null);
		}

		/// <summary>
		/// Adds the user mapping to the group.
		/// </summary>
		/// <param name="userMapping">The user mapping to add to the group.</param>
		/// <param name="groupId">The id of the group to remove the mapping from.</param>
		/// <returns></returns>
		public async Task AddUserToGroup(UserMapping userMapping, Guid groupId)
		{
			// Update the group to include the new user mapping.
			await Collection.UpdateOneAsync(
				Builders<GroupDto>.Filter.Eq(i => i.Id, groupId),
				Builders<GroupDto>.Update.Push(i => i.Users, userMapping));
		}

		/// <summary>
		/// Finds the groups that match the name entered.
		/// </summary>
		/// <param name="name">The name to find the group by.</param>
		/// <returns>The list of groups matching the result.</returns>
		public async Task<IList<Group>> FindByName(string name)
		{

			// Get the results of the text search.
			PagedResultSet<GroupDto> results = await TextSearch(name, Int32.MaxValue, 0, false);

			// Create the list of groups
			List<Group> mappedGroups = new List<Group>();

			// For each result, map to a Group model object.
			foreach(GroupDto result in results.Items)
			{
				mappedGroups.Add(await MapDtoToModel(result));
			}

			// return the mapped groups.
			return mappedGroups;
		}

		#region Object mapping functions

		/// <summary>
		/// Maps the model object to the DTO object.
		/// </summary>
		/// <param name="modelObj"></param>
		/// <returns></returns>
		private GroupDto MapModelToDto(Group modelObj)
		{
			return new GroupDto()
			{
				Capcode = modelObj.Capcode,
				Created = modelObj.Created,
				Description = modelObj.Description,
				Id = modelObj.Id,
				Name = modelObj.Name,
				RegionId = modelObj.Region.Id,
				Service = modelObj.Service,
				Users = modelObj.Users,
				HeadquartersCoordinates = new GeoJson2DGeographicCoordinates(modelObj.HeadquartersCoordinates.Longitude, modelObj.HeadquartersCoordinates.Latitude)
			};
		}

		private async Task<Group> MapDtoToModel(GroupDto dbObj)
		{

			// Get the region from the dto region id
			IList<Region> regions = await _mongoDb.GetCollection<Region>("regions").Find(new BsonDocument()).ToListAsync();

			return new Group()
			{
				Capcode = dbObj.Capcode,
				Created = dbObj.Created,
				Description = dbObj.Description,
				Id = dbObj.Id,
				Name = dbObj.Name,
				Region = regions.FirstOrDefault(i => i.Id == dbObj.RegionId),
				Service = dbObj.Service,
				Users = dbObj.Users,
				HeadquartersCoordinates = new Coordinates(dbObj.HeadquartersCoordinates.Latitude, dbObj.HeadquartersCoordinates.Longitude)
			};
		}

		#endregion

	}
}
