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
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Units;
using Enivate.ResponseHub.DataAccess.Interface;
using MongoDB.Driver.GeoJsonObjectModel;
using Enivate.ResponseHub.Model.Spatial;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{

	[MongoCollectionName("units")]
	public class UnitRepository : MongoRepository<UnitDto>, IUnitRepository
	{

		/// <summary>
		/// The ILogger that is responsible for logging data.
		/// </summary>
		private ILogger _logger;
		
		/// <summary>
		/// Creates a new instance of the UnitRepository
		/// </summary>
		/// <param name="logger"></param>
		public UnitRepository(ILogger logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Gets all of the units in the system.
		/// </summary>
		/// <returns></returns>
		public async Task<IList<Unit>> GetAll(IList<Region> regions)
		{
			IList<UnitDto> allResults = await base.GetAll();

			// Create the list of units
			IList<Unit> units = new List<Unit>();
			foreach(UnitDto unitDto in allResults)
			{
				units.Add(MapDtoToModel(unitDto, regions));
			}

			return units;
		}

		/// <summary>
		/// Gets a unit by the specific id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<Unit> GetById(Guid id, IList<Region> regions)
		{
			UnitDto unit = await base.GetById(id);
			return MapDtoToModel(unit, regions);
		}

		/// <summary>
		/// Gets the units based on the collection of ids submitted. 
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public async Task<IList<Unit>> GetByIds(IEnumerable<Guid> ids, IList<Region> regions)
		{

			// Create the filter definition
			FilterDefinition<UnitDto> filter = Builders<UnitDto>.Filter.In(i => i.Id, ids);

			// Get the unit dtos from the database
			IList<UnitDto> units = await Collection.Find(filter).ToListAsync();

			// return the mapped list of objects.
			return MapDbObjectListToModelObjectList(units, regions);
		}

		/// <summary>
		/// Creates the unit in the database.
		/// </summary>
		/// <param name="unit">The unit to create.</param>
		/// <returns>The saved unit.</returns>
		public async Task<Unit> CreateUnit(Unit unit, IList<Region> regions)
		{

			// Debug logging
			await _logger.Debug(String.Format("New Unit created. Id: {0} - Name {1}", unit.Id, unit.Name));

			// Save the unit to the database.
			UnitDto unitDto = await Save(MapModelToDto(unit));

			// return the unit
			return MapDtoToModel(unitDto, regions);
		}

		/// <summary>
		/// Finds the most recently created units and limits them by 'count'.
		/// </summary>
		/// <param name="count">The limit of results to return from the database query.</param>
		/// <returns>The most recent units found.</returns>
		public async Task<IList<Unit>> GetRecentlyAdded(int count, IList<Region> regions)
		{

			// Find most recent units and limit by count
			IList<UnitDto> units = await Collection.Find(new BsonDocument()).Sort(Builders<UnitDto>.Sort.Descending(i => i.Created)).Limit(count).ToListAsync();

			// return the mapped result set
			return MapDbObjectListToModelObjectList(units, regions);

		}

		/// <summary>
		/// Determines if a unit already exists with the name in the service.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="service"></param>
		/// <returns></returns>
		public async Task<bool> CheckIfUnitExists(string name, ServiceType service)
		{
			UnitDto unit = await FindOne(i => i.Name.ToUpper() == name.ToUpper() && i.Service == service);

			return (unit != null);
		}

		/// <summary>
		/// Adds the user mapping to the unit.
		/// </summary>
		/// <param name="userMapping">The user mapping to add to the unit.</param>
		/// <param name="unitId">The id of the unit to remove the mapping from.</param>
		/// <returns></returns>
		public async Task AddUserToUnit(UserMapping userMapping, Guid unitId)
		{
			// Update the unit to include the new user mapping.
			await Collection.UpdateOneAsync(
				Builders<UnitDto>.Filter.Eq(i => i.Id, unitId),
				Builders<UnitDto>.Update.Push(i => i.Users, userMapping));
		}

		/// <summary>
		/// Removes the specified user mapping from the unit for the specific user.
		/// </summary>
		/// <param name="userId">The ID of the user to remove from the unit.</param>
		/// <param name="unitId">The ID of the unit to remove the user from.</param>
		/// <returns></returns>
		public async Task RemoveUserFromUnit(Guid userId, Guid unitId)
		{

			// Create the filter to match the unit
			FilterDefinition<UnitDto> filter = Builders<UnitDto>.Filter.Eq(i => i.Id, unitId);

			// Create the update to pull the user mapping where the user id exists.
			UpdateDefinition<UnitDto> update = Builders<UnitDto>.Update.PullFilter(i => i.Users, f => f.UserId == userId);

			// Perform fthe update
			await Collection.FindOneAndUpdateAsync(filter, update);
		}

		/// <summary>
		/// Gets the units a user is a member of.
		/// </summary>
		/// <param name="userId">The id of the user to get the units for.</param>
		/// <returns>The collection of units the user is a member of.</returns>
		public async Task<IList<Unit>> GetUnitsForUser(Guid userId, IList<Region> regions)
		{

			// Create the filter to search the Users sub collection for the users id
			FilterDefinition<UnitDto> filter = Builders<UnitDto>.Filter.ElemMatch(i => i.Users, u => u.UserId == userId);

			// Get the users in the unit
			IList<UnitDto> usersUnits = await Collection.Find(filter).ToListAsync();

			// return the mapped result set
			return MapDbObjectListToModelObjectList(usersUnits, regions);

		}

		/// <summary>
		/// Finds the units that match the name entered.
		/// </summary>
		/// <param name="name">The name to find the unit by.</param>
		/// <returns>The list of units matching the result.</returns>
		public async Task<IList<Unit>> FindByName(string name, IList<Region> regions)
		{

			// Get the results of the text search.
			PagedResultSet<UnitDto> results = await TextSearch(name, Int32.MaxValue, 0, false);

			// Create the list of units
			List<Unit> mappedUnits = new List<Unit>();

			// For each result, map to a Unit model object.
			foreach(UnitDto result in results.Items)
			{
				mappedUnits.Add(MapDtoToModel(result, regions));
			}

			// return the mapped units.
			return mappedUnits;
		}

		/// <summary>
		/// Updates a unit in the database with the specified unit values.
		/// </summary>
		/// <param name="unit">The unit to save to the database.</param>
		/// <returns></returns>
		public async Task UpdateUnit(Unit unit)
		{
			await Save(MapModelToDto(unit));
		}

		public async Task ChangeUserRoleInUnit(Guid unitId, Guid userId, string newRole)
		{

			// Create tehe filter and update objects
			FilterDefinition<UnitDto> filter = Builders<UnitDto>.Filter.Eq(i => i.Id, unitId) & 
												Builders<UnitDto>.Filter.ElemMatch(i => i.Users, u => u.UserId == userId);

			// Create the update definition.
			// -1 is used for $ in mongodb
			UpdateDefinition<UnitDto> update = Builders<UnitDto>.Update.Set(i => i.Users[-1].Role, newRole);

			// Update the document. 
			UpdateResult result = await Collection.UpdateOneAsync(filter, update);
			
		}

		/// <summary>
		/// Gets the unit id and user mappings for each of the units a given user id is a member of.
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public async Task<IDictionary<Guid, UserMapping>> GetUserMappingsForUser(Guid userId)
		{

			// Create the filter to get the user in the units.
			FilterDefinition<UnitDto> filter = Builders<UnitDto>.Filter.ElemMatch(i => i.Users, u => u.UserId == userId);

			// Create the projection
			ProjectionDefinition<UnitDto> projection = Builders<UnitDto>.Projection.Include(i => i.Id).Include(i => i.Users);

			// Get the results.
			IList<UnitDto> results = await Collection.Find(filter).Project<UnitDto>(projection).ToListAsync();

			// Create the dictionary of user mappings
			IDictionary<Guid, UserMapping> userMappings = new Dictionary<Guid, UserMapping>();

			foreach(UnitDto result in results)
			{
				userMappings.Add(new KeyValuePair<Guid, UserMapping>(result.Id, result.Users.FirstOrDefault(i => i.UserId == userId)));
			}

			return userMappings;

		}

		/// <summary>
		/// Gets all the units by the specified capcode
		/// </summary>
		/// <param name="capcodeAddress">The capcode address to get the units by.</param>
		/// <returns>All the units for the capcode.</returns>
		public async Task<IList<Unit>> GetUnitsByCapcode(Capcode capcode, IList<Region> regions)
		{
			// Create the filter
			FilterDefinition<UnitDto> filter = Builders<UnitDto>.Filter.Or(
				Builders<UnitDto>.Filter.Eq(i => i.Capcode, capcode.CapcodeAddress),
				Builders<UnitDto>.Filter.AnyEq(i => i.AdditionalCapcodes, capcode.Id)
				);

			// Find all the units
			IList <UnitDto> results = await Collection.Find(filter).ToListAsync();

			// return the results
			return results.Select(i => MapDtoToModel(i, regions)).ToList();

		}

		#region Object mapping functions

		/// <summary>
		/// Maps the model object to the DTO object.
		/// </summary>
		/// <param name="modelObj"></param>
		/// <returns></returns>
		private UnitDto MapModelToDto(Unit modelObj)
		{
			return new UnitDto()
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
				HeadquartersCoordinates = new GeoJson2DGeographicCoordinates(modelObj.HeadquartersCoordinates.Longitude, modelObj.HeadquartersCoordinates.Latitude),
				TrainingNight = modelObj.TrainingNight
			};
		}

		/// <summary>
		/// Maps the Dto object to the Model object.
		/// </summary>
		/// <param name="dbObj"></param>
		/// <returns></returns>
		private Unit MapDtoToModel(UnitDto dbObj, IList<Region> regions)
		{

			// If the database object is null, nothing we can do, so just return null.
			if (dbObj == null)
			{
				return null;
			}

			return new Unit()
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
				HeadquartersCoordinates = new Coordinates(dbObj.HeadquartersCoordinates.Latitude, dbObj.HeadquartersCoordinates.Longitude),
				TrainingNight = dbObj.TrainingNight
			};
		}

		/// <summary>
		/// Maps a collection of UnitDto objects to the Unit model objects.
		/// </summary>
		/// <param name="units"></param>
		/// <returns></returns>
		private IList<Unit> MapDbObjectListToModelObjectList(IList<UnitDto> units, IList<Region> regions)
		{
			// return the units found in the database.
			IList<Unit> mappedUnits = new List<Unit>();
			foreach (UnitDto unit in units)
			{
				mappedUnits.Add(MapDtoToModel(unit, regions));
			}

			return mappedUnits;
		}

		#endregion

	}
}
