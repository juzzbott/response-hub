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
		
		/// <summary>
		/// Creates a new instance of the GroupRepository
		/// </summary>
		/// <param name="logger"></param>
		public GroupRepository(ILogger logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Gets all of the groups in the system.
		/// </summary>
		/// <returns></returns>
		public async Task<IList<Group>> GetAll(IList<Region> regions)
		{
			IList<GroupDto> allResults = await base.GetAll();

			// Create the list of groups
			IList<Group> groups = new List<Group>();
			foreach(GroupDto groupDto in allResults)
			{
				groups.Add(MapDtoToModel(groupDto, regions));
			}

			return groups;
		}

		/// <summary>
		/// Gets a group by the specific id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<Group> GetById(Guid id, IList<Region> regions)
		{
			GroupDto group = await base.GetById(id);
			return MapDtoToModel(group, regions);
		}

		/// <summary>
		/// Gets the groups based on the collection of ids submitted. 
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public async Task<IList<Group>> GetByIds(IEnumerable<Guid> ids, IList<Region> regions)
		{

			// Create the filter definition
			FilterDefinition<GroupDto> filter = Builders<GroupDto>.Filter.In(i => i.Id, ids);

			// Get the group dtos from the database
			IList<GroupDto> groups = await Collection.Find(filter).ToListAsync();

			// return the mapped list of objects.
			return MapDbObjectListToModelObjectList(groups, regions);
		}

		/// <summary>
		/// Creates the group in the database.
		/// </summary>
		/// <param name="group">The group to create.</param>
		/// <returns>The saved group.</returns>
		public async Task<Group> CreateGroup(Group group, IList<Region> regions)
		{

			// Debug logging
			await _logger.Debug(String.Format("New Group created. Id: {0} - Name {1}", group.Id, group.Name));

			// Save the group to the database.
			GroupDto groupDto = await Save(MapModelToDto(group));

			// return the group
			return MapDtoToModel(groupDto, regions);
		}

		/// <summary>
		/// Finds the most recently created groups and limits them by 'count'.
		/// </summary>
		/// <param name="count">The limit of results to return from the database query.</param>
		/// <returns>The most recent groups found.</returns>
		public async Task<IList<Group>> GetRecentlyAdded(int count, IList<Region> regions)
		{

			// Find most recent groups and limit by count
			IList<GroupDto> groups = await Collection.Find(new BsonDocument()).Sort(Builders<GroupDto>.Sort.Descending(i => i.Created)).Limit(count).ToListAsync();

			// return the mapped result set
			return MapDbObjectListToModelObjectList(groups, regions);

		}

		/// <summary>
		/// Determines if a group already exists with the name in the service.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="service"></param>
		/// <returns></returns>
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
		/// Removes the specified user mapping from the group for the specific user.
		/// </summary>
		/// <param name="userId">The ID of the user to remove from the group.</param>
		/// <param name="groupId">The ID of the group to remove the user from.</param>
		/// <returns></returns>
		public async Task RemoveUserFromGroup(Guid userId, Guid groupId)
		{

			// Create the filter to match the group
			FilterDefinition<GroupDto> filter = Builders<GroupDto>.Filter.Eq(i => i.Id, groupId);

			// Create the update to pull the user mapping where the user id exists.
			UpdateDefinition<GroupDto> update = Builders<GroupDto>.Update.PullFilter(i => i.Users, f => f.UserId == userId);

			// Perform fthe update
			await Collection.FindOneAndUpdateAsync(filter, update);
		}

		/// <summary>
		/// Gets the groups a user is a member of.
		/// </summary>
		/// <param name="userId">The id of the user to get the groups for.</param>
		/// <returns>The collection of groups the user is a member of.</returns>
		public async Task<IList<Group>> GetGroupsForUser(Guid userId, IList<Region> regions)
		{

			// Create the filter to search the Users sub collection for the users id
			FilterDefinition<GroupDto> filter = Builders<GroupDto>.Filter.ElemMatch(i => i.Users, u => u.UserId == userId);

			// Get the users in the group
			IList<GroupDto> usersGroups = await Collection.Find(filter).ToListAsync();

			// return the mapped result set
			return MapDbObjectListToModelObjectList(usersGroups, regions);

		}

		/// <summary>
		/// Finds the groups that match the name entered.
		/// </summary>
		/// <param name="name">The name to find the group by.</param>
		/// <returns>The list of groups matching the result.</returns>
		public async Task<IList<Group>> FindByName(string name, IList<Region> regions)
		{

			// Get the results of the text search.
			PagedResultSet<GroupDto> results = await TextSearch(name, Int32.MaxValue, 0, false);

			// Create the list of groups
			List<Group> mappedGroups = new List<Group>();

			// For each result, map to a Group model object.
			foreach(GroupDto result in results.Items)
			{
				mappedGroups.Add(MapDtoToModel(result, regions));
			}

			// return the mapped groups.
			return mappedGroups;
		}

		/// <summary>
		/// Updates a group in the database with the specified group values.
		/// </summary>
		/// <param name="group">The group to save to the database.</param>
		/// <returns></returns>
		public async Task UpdateGroup(Group group)
		{
			await Save(MapModelToDto(group));
		}

		public async Task ChangeUserRoleInGroup(Guid groupId, Guid userId, string newRole)
		{

			// Create tehe filter and update objects
			FilterDefinition<GroupDto> filter = Builders<GroupDto>.Filter.Eq(i => i.Id, groupId) & 
												Builders<GroupDto>.Filter.ElemMatch(i => i.Users, u => u.UserId == userId);

			// Create the update definition.
			// -1 is used for $ in mongodb
			UpdateDefinition<GroupDto> update = Builders<GroupDto>.Update.Set(i => i.Users[-1].Role, newRole);

			// Update the document. 
			UpdateResult result = await Collection.UpdateOneAsync(filter, update);
			
		}

		/// <summary>
		/// Gets the group id and user mappings for each of the groups a given user id is a member of.
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public async Task<IDictionary<Guid, UserMapping>> GetUserMappingsForUser(Guid userId)
		{

			// Create the filter to get the user in the groups.
			FilterDefinition<GroupDto> filter = Builders<GroupDto>.Filter.ElemMatch(i => i.Users, u => u.UserId == userId);

			// Create the projection
			ProjectionDefinition<GroupDto> projection = Builders<GroupDto>.Projection.Include(i => i.Id).Include(i => i.Users);

			// Get the results.
			IList<GroupDto> results = await Collection.Find(filter).Project<GroupDto>(projection).ToListAsync();

			// Create the dictionary of user mappings
			IDictionary<Guid, UserMapping> userMappings = new Dictionary<Guid, UserMapping>();

			foreach(GroupDto result in results)
			{
				userMappings.Add(new KeyValuePair<Guid, UserMapping>(result.Id, result.Users.FirstOrDefault(i => i.UserId == userId)));
			}

			return userMappings;

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
				AdditionalCapcodes = modelObj.AdditionalCapcodes,
				Created = modelObj.Created,
				Updated = modelObj.Updated,
				Description = modelObj.Description,
				Id = modelObj.Id,
				Name = modelObj.Name,
				RegionId = modelObj.Region.Id,
				Service = modelObj.Service,
				Users = modelObj.Users,
				HeadquartersCoordinates = new GeoJson2DGeographicCoordinates(modelObj.HeadquartersCoordinates.Longitude, modelObj.HeadquartersCoordinates.Latitude)
			};
		}

		/// <summary>
		/// Maps the Dto object to the Model object.
		/// </summary>
		/// <param name="dbObj"></param>
		/// <returns></returns>
		private Group MapDtoToModel(GroupDto dbObj, IList<Region> regions)
		{

			// If the database object is null, nothing we can do, so just return null.
			if (dbObj == null)
			{
				return null;
			}

			return new Group()
			{
				Capcode = dbObj.Capcode,
				AdditionalCapcodes = dbObj.AdditionalCapcodes,
				Created = dbObj.Created,
				Updated = dbObj.Updated,
				Description = dbObj.Description,
				Id = dbObj.Id,
				Name = dbObj.Name,
				Region = regions.FirstOrDefault(i => i.Id == dbObj.RegionId),
				Service = dbObj.Service,
				Users = dbObj.Users,
				HeadquartersCoordinates = new Coordinates(dbObj.HeadquartersCoordinates.Latitude, dbObj.HeadquartersCoordinates.Longitude)
			};
		}

		/// <summary>
		/// Maps a collection of GroupDto objects to the Group model objects.
		/// </summary>
		/// <param name="groups"></param>
		/// <returns></returns>
		private IList<Group> MapDbObjectListToModelObjectList(IList<GroupDto> groups, IList<Region> regions)
		{
			// return the groups found in the database.
			IList<Group> mappedGroups = new List<Group>();
			foreach (GroupDto group in groups)
			{
				mappedGroups.Add(MapDtoToModel(group, regions));
			}

			return mappedGroups;
		}

		#endregion

	}
}
