using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Caching;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Model.Spatial;
using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class GroupService : IGroupService
	{

		private const string RecentlyAddedGroupsCacheKey = "RecentlyAddedGroups";
		private const string AllRegionsCacheKey = "AllRegions";

		private IGroupRepository _repository;

		private IUserRepository _userRepository;

		private IRegionRepository _regionRepository;

		/// <summary>
		/// Creates a new instance of the Group application service.
		/// </summary>
		/// <param name="repository">The repository used to persist group data.</param>
		public GroupService(IGroupRepository repository, IUserRepository userRepository, IRegionRepository regionRepository)
		{
			_repository = repository;
			_userRepository = userRepository;
			_regionRepository = regionRepository;
		}

		/// <summary>
		/// Creates a new group object.
		/// </summary>
		/// <param name="name">The name of the group.</param>
		/// <param name="service">The service the group belongs to.</param>
		/// <returns>The created group object.</returns>
		public async Task<Group> CreateGroup(string name, ServiceType service, string capcode, IList<Guid> additionalCapcodes, Guid groupAdministratorId, string description, Region region, Coordinates headquartersCoords)
		{
			Group group = new Group()
			{
				Name = name,
				Created = DateTime.UtcNow,
				Updated = DateTime.UtcNow,
				Service = service,
				Capcode = capcode,
				AdditionalCapcodes = additionalCapcodes,
				Description = description,
				Region = region,
				HeadquartersCoordinates = headquartersCoords
			};

			// Add the user mapping for the group administrator
			group.Users.Add(new UserMapping()
			{
				Role = RoleTypes.GroupAdministrator,
				UserId = groupAdministratorId
			});

			// Create the new group
			Group newGroup = await _repository.CreateGroup(group, await GetRegions());

			// If the new group exists, we need to clear the recently added group cache
			if (newGroup != null)
			{
				CacheManager.RemoveItem(RecentlyAddedGroupsCacheKey);
			}

			return newGroup;

		}

		/// <summary>
		/// Gets all the groups in the repository.
		/// </summary>
		/// <returns></returns>
		public async Task<IList<Group>> GetAll()
		{
			return await _repository.GetAll(await _regionRepository.GetAll());
		}

		/// <summary>
		/// Gets the most recently added groups in the system.
		/// </summary>
		/// <param name="count">The limit of results to return from the database query.</param>
		/// <returns>The most recent groups found.</returns>
		public async Task<IList<Group>> GetRecentlyAdded(int count)
		{

			if (count < 1)
			{
				throw new ArgumentOutOfRangeException("count", "The count parameter must be a positive integer.");
			}

			// If the cache has the recently added items, get from cache
			IList<Group> groups = CacheManager.GetItem<IList<Group>>(RecentlyAddedGroupsCacheKey);

			// If the groups are null, then load from db and add to the cache
			if (groups == null)
			{
				// Get the groups from the db
				groups = await _repository.GetRecentlyAdded(count, await GetRegions());

				// If the groups arent null and contains items, add to the cache
				if (groups != null && groups.Count > 0)
				{
					CacheManager.AddItem(RecentlyAddedGroupsCacheKey, groups);
				}
			}

			return groups;
		}

		/// <summary>
		/// Gets the specific group from the ID.
		/// </summary>
		/// <param name="id">The ID of the group to return.</param>
		/// <returns>The group is found by ID, otherwise null.</returns>
		public async Task<Group> GetById(Guid id)
		{

			// Get the group from cache if it exists
			Group group = CacheManager.GetEntity<Group>(id);

			// If the group is null, get from the database, and add to the cache for next load
			if (group == null)
			{
				// Get the group from the db
				group = await _repository.GetById(id, await GetRegions());

				// If the group is not null, store it in cache for next use.
				if (group != null)
				{
					CacheManager.AddItem(group);
				}

			}

			// return the group
			return group;
		}

		/// <summary>
		/// Gets the groups by the specified collection of Ids.
		/// </summary>
		/// <param name="ids">The collection of ids to get the groups by.</param>
		/// <returns>The collection of groups that have the ids.</returns>
		public async Task<IList<Group>> GetByIds(IEnumerable<Guid> ids)
		{
			return await _repository.GetByIds(ids, await GetRegions());
		}

		/// <summary>
		/// Determines if the group name exists for the specified service already within the system.
		/// </summary>
		/// <param name="name">The name of the group.</param>
		/// <param name="service">The service type to check.</param>
		/// <returns>True if the group name exists, otherwise false.</returns>
		public async Task<bool> CheckIfGroupExists(string name, ServiceType service)
		{
			return await _repository.CheckIfGroupExists(name, service);
		}

		/// <summary>
		/// Adds the specified user to the group.
		/// </summary>
		/// <param name="userId">The ID of the user to add to the group.</param>
		/// <param name="role">The role of the user within the group.</param>
		/// <param name="groupId">The Id of the group to add the user to.</param>
		/// <returns></returns>
		public async Task AddUserToGroup(Guid userId, string role, Guid groupId)
		{
			UserMapping mapping = new UserMapping()
			{
				Role = role,
				UserId = userId
			};

			await _repository.AddUserToGroup(mapping, groupId);

			// Clear the group from the cache
			CacheManager.RemoveItem(RecentlyAddedGroupsCacheKey);
			CacheManager.RemoveItem(CacheUtility.GetEntityCacheKey(typeof(Group), groupId.ToString()));
			string groupAdminListCacheKey = String.Format("GroupAdminGroupIds_{0}", userId);
			CacheManager.RemoveItem(groupAdminListCacheKey);
		}

		public async Task RemoveUserFromGroup(Guid userId, Guid groupId)
		{
			await _repository.RemoveUserFromGroup(userId, groupId);

			// Clear the group from the cache
			CacheManager.RemoveItem(RecentlyAddedGroupsCacheKey);
			CacheManager.RemoveItem(CacheUtility.GetEntityCacheKey(typeof(Group), groupId.ToString()));
			string groupAdminListCacheKey = String.Format("GroupAdminGroupIds_{0}", userId);
			CacheManager.RemoveItem(groupAdminListCacheKey);
		}

		/// <summary>
		/// Gets the groups a user is a member of.
		/// </summary>
		/// <param name="userId">The id of the user to get the groups for.</param>
		/// <returns>The collection of groups the user is a member of.</returns>
		public async Task<IList<Group>> GetGroupsForUser(Guid userId)
		{
			return await _repository.GetGroupsForUser(userId, await GetRegions());
		}

		/// <summary>
		/// Gets the collection of users for the specified group.
		/// </summary>
		/// <param name="groupId">The id of the group to get the users for.</param>
		/// <returns>The list of identity users for the specified group.</returns>
		public async Task<IList<IdentityUser>> GetUsersForGroup(Guid groupId)
		{
			// Get the group
			Group group = await _repository.GetById(groupId, await GetRegions());

			// If the group is null, just return an empty list
			if (group == null)
			{
				return new List<IdentityUser>();
			}

			// Now that we have the group, get all users for that group
			IList<IdentityUser> users = await _userRepository.GetUsersByIds(group.Users.Select(i => i.UserId));

			// return the users
			return users;
		}

		/// <summary>
		/// Gets all the regions that a group can be a member of.
		/// </summary>
		/// <returns></returns>
		public async Task<IList<Region>> GetRegions()
		{

			// Try and get the regions from the cache
			IList<Region> regions = CacheManager.GetItem<IList<Region>>(AllRegionsCacheKey);

			// If the regions are null, cache doesn't exist, so load from DB.
			if (regions == null)
			{
				// Get from db
				regions = await _regionRepository.GetAll();

				// Add to the cache
				if (regions != null)
				{
					CacheManager.AddItem(AllRegionsCacheKey, regions);
				}
			}

			return regions;
		}

		/// <summary>
		/// Finds groups by name. This is a text based search and will match any of the words in the group name.
		/// </summary>
		/// <param name="name">The name of the group to search for.</param>
		/// <returns>The list of groups that match against the group name.</returns>
		public async Task<IList<Group>> FindByName(string name)
		{
			return await _repository.FindByName(name, await GetRegions());
		}

		/// <summary>
		/// Updates the group in the database.
		/// </summary>
		/// <param name="group">The group to update in the database.</param>
		/// <returns></returns>
		public async Task UpdateGroup(Group group)
		{

			// If the group is null or the group id is empty guid, throw exception as the group should be saved first.
			if (group == null || group.Id == Guid.Empty)
			{
				throw new Exception("The group must exist before it can be updated.");
			}

			// Save the group to the database.
			await _repository.UpdateGroup(group);

			// Remove the group from cache so that a fresh reload occurs
			CacheManager.RemoveItem(RecentlyAddedGroupsCacheKey);
			CacheManager.RemoveEntity(group);

		}

		/// <summary>
		/// Change the users role in the specified group.
		/// </summary>
		/// <param name="groupId">The id of the group to change the users role for.</param>
		/// <param name="userId">The id of the user to change the role for.</param>
		/// <param name="newRole">The new role to change the user to.</param>
		/// <returns></returns>
		public async Task ChangeUserRoleInGroup(Guid groupId, Guid userId, string newRole)
		{
			// If the group is null or the group id is empty guid, throw exception as the group should be saved first.
			if (groupId == Guid.Empty)
			{
				throw new Exception("The group id cannot be null or empty.");
			}
			if (userId == Guid.Empty)
			{
				throw new Exception("The user id cannot be null or empty.");
			}
			if (String.IsNullOrEmpty(newRole))
			{
				throw new Exception("The role cannot be null or empty.");
			}

			// Save the group to the database.
			await _repository.ChangeUserRoleInGroup(groupId, userId, newRole);

			// Remove the group from cache so that a fresh reload occurs
			CacheManager.RemoveItem(RecentlyAddedGroupsCacheKey);
			CacheManager.RemoveItem(CacheUtility.GetEntityCacheKey(typeof(Group), groupId.ToString()));
			string groupAdminListCacheKey = String.Format("GroupAdminGroupIds_{0}", userId);
			CacheManager.RemoveItem(groupAdminListCacheKey);

		}

		/// <summary>
		/// Gets the group id and user mappings for each of the groups a given user id is a member of.
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public async Task<IDictionary<Guid, UserMapping>> GetUserMappingsForUser(Guid userId)
		{
			return await _repository.GetUserMappingsForUser(userId);
		}
		
		/// <summary>
		/// Gets the list of GroupIds the current user is a group admin of.
		/// </summary>
		/// <returns></returns>
		public async Task<IList<Guid>> GetGroupIdsUserIsGroupAdminOf(Guid userId)
		{

			// Specify the cache key
			string cacheKey = String.Format("GroupAdminGroupIds_{0}", userId);

			// Get the item from cache if it exists
			IList<Guid> groupAdminGroupIds = CacheManager.GetItem<IList<Guid>>(cacheKey);

			// If the cache item is not null, return it
			if (groupAdminGroupIds != null)
			{
				return groupAdminGroupIds;
			}

			// Get the group ids and user mappings for those groups
			IDictionary<Guid, UserMapping> userGroupMappings = await GetUserMappingsForUser(userId);

			groupAdminGroupIds = userGroupMappings.Where(i => i.Value.Role == RoleTypes.GroupAdministrator).Select(i => i.Key).ToList();

			// Add to cache for the next time
			CacheManager.AddItem(cacheKey, groupAdminGroupIds, 1440);

			// Get the user mappings for groups for the user
			return groupAdminGroupIds;

		}
	}
}
