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
		public new async Task<IList<Group>> GetAll()
		{
			IList<GroupDto> allResults = await base.GetAll();

			// Create the list of groups
			IList<Group> groups = new List<Group>();
			foreach(GroupDto groupDto in allResults)
			{
				groups.Add(await MapDtoToModel(groupDto));
			}

			return groups;
		}

		/// <summary>
		/// Gets a group by the specific id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
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

			// return the mapped result set
			return await MapDbObjectListToModelObjectList(groups);

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
		/// Gets the groups a user is a member of.
		/// </summary>
		/// <param name="userId">The id of the user to get the groups for.</param>
		/// <returns>The collection of groups the user is a member of.</returns>
		public async Task<IList<Group>> GetGroupsForUser(Guid userId)
		{

			// Create the filter to search the Users sub collection for the users id
			FilterDefinition<GroupDto> filter = Builders<GroupDto>.Filter.ElemMatch(i => i.Users, u => u.UserId == userId);

			// Get the users in the group
			IList<GroupDto> usersGroups = await Collection.Find(filter).ToListAsync();

			// return the mapped result set
			return await MapDbObjectListToModelObjectList(usersGroups);

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
		private async Task<Group> MapDtoToModel(GroupDto dbObj)
		{

			// If the database object is null, nothing we can do, so just return null.
			if (dbObj == null)
			{
				return null;
			}

			// Get the region from the dto region id
			IList<Region> regions = await _mongoDb.GetCollection<Region>("regions").Find(new BsonDocument()).ToListAsync();

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
		private async Task<IList<Group>> MapDbObjectListToModelObjectList(IList<GroupDto> groups)
		{
			// return the groups found in the database.
			IList<Group> mappedGroups = new List<Group>();
			foreach (GroupDto group in groups)
			{
				mappedGroups.Add(await MapDtoToModel(group));
			}

			return mappedGroups;
		}

		#endregion

	}
}
