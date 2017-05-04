using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Caching;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Model.Spatial;
using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class UnitService : IUnitService
	{

		private const string RecentlyAddedUnitsCacheKey = "RecentlyAddedUnits";
		private const string AllRegionsCacheKey = "AllRegions";

		private IUnitRepository _repository;

		private IUserRepository _userRepository;

		private IRegionRepository _regionRepository;

		/// <summary>
		/// Creates a new instance of the Unit application service.
		/// </summary>
		/// <param name="repository">The repository used to persist unit data.</param>
		public UnitService(IUnitRepository repository, IUserRepository userRepository, IRegionRepository regionRepository)
		{
			_repository = repository;
			_userRepository = userRepository;
			_regionRepository = regionRepository;
		}

		/// <summary>
		/// Creates a new unit object.
		/// </summary>
		/// <param name="name">The name of the unit.</param>
		/// <param name="service">The service the unit belongs to.</param>
		/// <returns>The created unit object.</returns>
		public async Task<Unit> CreateUnit(string name, ServiceType service, string capcode, IList<Guid> additionalCapcodes, Guid unitAdministratorId, string description, Region region, Coordinates headquartersCoords, TrainingNightInfo trainingNight)
		{
			Unit unit = new Unit()
			{
				Name = name,
				Created = DateTime.UtcNow,
				Updated = DateTime.UtcNow,
				Service = service,
				Capcode = capcode,
				AdditionalCapcodes = additionalCapcodes,
				Description = description,
				Region = region,
				HeadquartersCoordinates = headquartersCoords,
				TrainingNight = trainingNight
			};

			// Add the user mapping for the unit administrator
			unit.Users.Add(new UserMapping()
			{
				Role = RoleTypes.UnitAdministrator,
				UserId = unitAdministratorId
			});

			// Create the new unit
			Unit newUnit = await _repository.CreateUnit(unit, await GetRegions());

			// If the new unit exists, we need to clear the recently added unit cache
			if (newUnit != null)
			{
				CacheManager.RemoveItem(RecentlyAddedUnitsCacheKey);
			}

			return newUnit;

		}

		/// <summary>
		/// Gets all the units in the repository.
		/// </summary>
		/// <returns></returns>
		public async Task<IList<Unit>> GetAll()
		{
			return await _repository.GetAll(await _regionRepository.GetAll());
		}

		/// <summary>
		/// Gets the most recently added units in the system.
		/// </summary>
		/// <param name="count">The limit of results to return from the database query.</param>
		/// <returns>The most recent units found.</returns>
		public async Task<IList<Unit>> GetRecentlyAdded(int count)
		{

			if (count < 1)
			{
				throw new ArgumentOutOfRangeException("count", "The count parameter must be a positive integer.");
			}

			// If the cache has the recently added items, get from cache
			IList<Unit> units = CacheManager.GetItem<IList<Unit>>(RecentlyAddedUnitsCacheKey);

			// If the units are null, then load from db and add to the cache
			if (units == null)
			{
				// Get the units from the db
				units = await _repository.GetRecentlyAdded(count, await GetRegions());

				// If the units arent null and contains items, add to the cache
				if (units != null && units.Count > 0)
				{
					CacheManager.AddItem(RecentlyAddedUnitsCacheKey, units);
				}
			}

			return units;
		}

		/// <summary>
		/// Gets the specific unit from the ID.
		/// </summary>
		/// <param name="id">The ID of the unit to return.</param>
		/// <returns>The unit is found by ID, otherwise null.</returns>
		public async Task<Unit> GetById(Guid id)
		{

			// Get the unit from cache if it exists
			Unit unit = CacheManager.GetEntity<Unit>(id);

			// If the unit is null, get from the database, and add to the cache for next load
			if (unit == null)
			{
				// Get the unit from the db
				unit = await _repository.GetById(id, await GetRegions());

				// If the unit is not null, store it in cache for next use.
				if (unit != null)
				{
					CacheManager.AddItem(unit);
				}

			}

			// return the unit
			return unit;
		}

		/// <summary>
		/// Gets the units by the specified collection of Ids.
		/// </summary>
		/// <param name="ids">The collection of ids to get the units by.</param>
		/// <returns>The collection of units that have the ids.</returns>
		public async Task<IList<Unit>> GetByIds(IEnumerable<Guid> ids)
		{
			return await _repository.GetByIds(ids, await GetRegions());
		}

		/// <summary>
		/// Determines if the unit name exists for the specified service already within the system.
		/// </summary>
		/// <param name="name">The name of the unit.</param>
		/// <param name="service">The service type to check.</param>
		/// <returns>True if the unit name exists, otherwise false.</returns>
		public async Task<bool> CheckIfUnitExists(string name, ServiceType service)
		{
			return await _repository.CheckIfUnitExists(name, service);
		}

		/// <summary>
		/// Adds the specified user to the unit.
		/// </summary>
		/// <param name="userId">The ID of the user to add to the unit.</param>
		/// <param name="role">The role of the user within the unit.</param>
		/// <param name="unitId">The Id of the unit to add the user to.</param>
		/// <returns></returns>
		public async Task AddUserToUnit(Guid userId, string role, Guid unitId)
		{
			UserMapping mapping = new UserMapping()
			{
				Role = role,
				UserId = userId
			};

			await _repository.AddUserToUnit(mapping, unitId);

			// Clear the unit from the cache
			CacheManager.RemoveItem(RecentlyAddedUnitsCacheKey);
			CacheManager.RemoveItem(CacheUtility.GetEntityCacheKey(typeof(Unit), unitId.ToString()));
			string unitAdminListCacheKey = String.Format("UnitAdminUnitIds_{0}", userId);
			CacheManager.RemoveItem(unitAdminListCacheKey);
		}

		public async Task RemoveUserFromUnit(Guid userId, Guid unitId)
		{
			await _repository.RemoveUserFromUnit(userId, unitId);

			// Clear the unit from the cache
			CacheManager.RemoveItem(RecentlyAddedUnitsCacheKey);
			CacheManager.RemoveItem(CacheUtility.GetEntityCacheKey(typeof(Unit), unitId.ToString()));
			string unitAdminListCacheKey = String.Format("UnitAdminUnitIds_{0}", userId);
			CacheManager.RemoveItem(unitAdminListCacheKey);
		}

		/// <summary>
		/// Gets the units a user is a member of.
		/// </summary>
		/// <param name="userId">The id of the user to get the units for.</param>
		/// <returns>The collection of units the user is a member of.</returns>
		public async Task<IList<Unit>> GetUnitsForUser(Guid userId)
		{
			return await _repository.GetUnitsForUser(userId, await GetRegions());
		}

		/// <summary>
		/// Gets the collection of users for the specified unit.
		/// </summary>
		/// <param name="unitId">The id of the unit to get the users for.</param>
		/// <returns>The list of identity users for the specified unit.</returns>
		public async Task<IList<IdentityUser>> GetUsersForUnit(Guid unitId)
		{
			// Get the unit
			Unit unit = await _repository.GetById(unitId, await GetRegions());

			// If the unit is null, just return an empty list
			if (unit == null)
			{
				return new List<IdentityUser>();
			}

			// Now that we have the unit, get all users for that unit
			IList<IdentityUser> users = await _userRepository.GetUsersByIds(unit.Users.Select(i => i.UserId), true);

			// return the users
			return users;
		}

		/// <summary>
		/// Gets all the regions that a unit can be a member of.
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
		/// Finds units by name. This is a text based search and will match any of the words in the unit name.
		/// </summary>
		/// <param name="name">The name of the unit to search for.</param>
		/// <returns>The list of units that match against the unit name.</returns>
		public async Task<IList<Unit>> FindByName(string name)
		{
			return await _repository.FindByName(name, await GetRegions());
		}

		/// <summary>
		/// Updates the unit in the database.
		/// </summary>
		/// <param name="unit">The unit to update in the database.</param>
		/// <returns></returns>
		public async Task UpdateUnit(Unit unit)
		{

			// If the unit is null or the unit id is empty guid, throw exception as the unit should be saved first.
			if (unit == null || unit.Id == Guid.Empty)
			{
				throw new Exception("The unit must exist before it can be updated.");
			}

			// Save the unit to the database.
			await _repository.UpdateUnit(unit);

			// Remove the unit from cache so that a fresh reload occurs
			CacheManager.RemoveItem(RecentlyAddedUnitsCacheKey);
			CacheManager.RemoveEntity(unit);

		}

		/// <summary>
		/// Change the users role in the specified unit.
		/// </summary>
		/// <param name="unitId">The id of the unit to change the users role for.</param>
		/// <param name="userId">The id of the user to change the role for.</param>
		/// <param name="newRole">The new role to change the user to.</param>
		/// <returns></returns>
		public async Task ChangeUserRoleInUnit(Guid unitId, Guid userId, string newRole)
		{
			// If the unit is null or the unit id is empty guid, throw exception as the unit should be saved first.
			if (unitId == Guid.Empty)
			{
				throw new Exception("The unit id cannot be null or empty.");
			}
			if (userId == Guid.Empty)
			{
				throw new Exception("The user id cannot be null or empty.");
			}
			if (String.IsNullOrEmpty(newRole))
			{
				throw new Exception("The role cannot be null or empty.");
			}

			// Save the unit to the database.
			await _repository.ChangeUserRoleInUnit(unitId, userId, newRole);

			// Remove the unit from cache so that a fresh reload occurs
			CacheManager.RemoveItem(RecentlyAddedUnitsCacheKey);
			CacheManager.RemoveItem(CacheUtility.GetEntityCacheKey(typeof(Unit), unitId.ToString()));
			string unitAdminListCacheKey = String.Format("UnitAdminUnitIds_{0}", userId);
			CacheManager.RemoveItem(unitAdminListCacheKey);

		}

		/// <summary>
		/// Gets the unit id and user mappings for each of the units a given user id is a member of.
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public async Task<IDictionary<Guid, UserMapping>> GetUserMappingsForUser(Guid userId)
		{
			return await _repository.GetUserMappingsForUser(userId);
		}
		
		/// <summary>
		/// Gets the list of UnitIds the current user is a unit admin of.
		/// </summary>
		/// <returns></returns>
		public async Task<IList<Guid>> GetUnitIdsUserIsUnitAdminOf(Guid userId)
		{

			// Specify the cache key
			string cacheKey = String.Format("UnitAdminUnitIds_{0}", userId);

			// Get the item from cache if it exists
			IList<Guid> unitAdminUnitIds = CacheManager.GetItem<IList<Guid>>(cacheKey);

			// If the cache item is not null, return it
			if (unitAdminUnitIds != null)
			{
				return unitAdminUnitIds;
			}

			// Get the unit ids and user mappings for those units
			IDictionary<Guid, UserMapping> userUnitMappings = await GetUserMappingsForUser(userId);

			unitAdminUnitIds = userUnitMappings.Where(i => i.Value.Role == RoleTypes.UnitAdministrator).Select(i => i.Key).ToList();

			// Add to cache for the next time
			CacheManager.AddItem(cacheKey, unitAdminUnitIds, 1440);

			// Get the user mappings for units for the user
			return unitAdminUnitIds;

		}

		/// <summary>
		/// Gets all the units by the specified capcode
		/// </summary>
		/// <param name="capcodeAddress">The capcode address to get the units by.</param>
		/// <returns>All the units for the capcode.</returns>
		public async Task<IList<Unit>> GetUnitsByCapcode(Capcode capcode)
		{
			return await _repository.GetUnitsByCapcode(capcode, await _regionRepository.GetAll());
		}

		/// <summary>
		/// Gets the unit by the specified capcode
		/// </summary>
		/// <param name="capcodeAddress">The capcode address to get the units by.</param>
		/// <returns>The unit for the capcode.</returns>
		public async Task<Unit> GetUnitByCapcode(Capcode capcode)
		{
			IList<Unit> units = await _repository.GetUnitsByCapcode(capcode, await _regionRepository.GetAll());
			return units.FirstOrDefault();
		}
	}
}
