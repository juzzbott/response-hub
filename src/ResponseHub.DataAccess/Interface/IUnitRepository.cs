using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Units;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface IUnitRepository
	{

		Task<Unit> CreateUnit(Unit unit, IList<Region> regions);

		Task<IList<Unit>> GetRecentlyAdded(int count, IList<Region> regions);

		Task<bool> CheckIfUnitExists(string name, ServiceType service);

		Task AddUserToUnit(UserMapping userMapping, Guid unitId);

		Task RemoveUserFromUnit(Guid userId, Guid unitId);

		Task<IList<Unit>> GetUnitsForUser(Guid userId, IList<Region> regions);

		Task<IList<Unit>> GetAll(IList<Region> regions);

		Task<Unit> GetById(Guid id, IList<Region> regions);

		Task<IList<Unit>> GetByIds(IEnumerable<Guid> ids, IList<Region> regions);

		Task<IList<Unit>> FindByName(string name, IList<Region> regions);

		/// <summary>
		/// Updates a unit in the database with the specified unit values.
		/// </summary>
		/// <param name="unit">The unit to save to the database.</param>
		/// <returns></returns>
		Task UpdateUnit(Unit unit);

		Task ChangeUserRoleInUnit(Guid unitId, Guid userId, string newRole);

		Task<IDictionary<Guid, UserMapping>> GetUserMappingsForUser(Guid userId);

		Task<IList<Unit>> GetUnitsByCapcode(Capcode capcode, IList<Region> regions);

	}

}