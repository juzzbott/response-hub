using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Units.Interface
{
	public interface IUnitService
	{

		Task<Unit> CreateUnit(string name, ServiceType service, string capcode, IList<Guid> additionalCapcodes, Guid unitAdministratorId, string description, Region region, Coordinates headquartersCoords, TrainingNightInfo trainingNight);

		Task<IList<Unit>> GetAll();

		Task<IList<Unit>> GetRecentlyAdded(int count);

		Task<Unit> GetById(Guid id);

		Task<IList<Unit>> GetByIds(IEnumerable<Guid> ids);

		Task<bool> CheckIfUnitExists(string name, ServiceType service);

		Task AddUserToUnit(Guid userId, string role, Guid unitId);

		Task RemoveUserFromUnit(Guid userId, Guid unitId);

		Task<IList<Unit>> GetUnitsForUser(Guid userId);

		Task<IList<IdentityUser>> GetUsersForUnit(Guid unitId);

		Task<IList<Region>> GetRegions();

		Task<IList<Unit>> FindByName(string name);

		Task UpdateUnit(Unit unit);

		Task ChangeUserRoleInUnit(Guid unitId, Guid userId, string newRole);

		Task<IDictionary<Guid, UserMapping>> GetUserMappingsForUser(Guid userId);

		Task<IList<Guid>> GetUnitIdsUserIsUnitAdminOf(Guid userId);

		Task<IList<Unit>> GetUnitsByCapcode(Capcode capcode);

		Task<Unit> GetUnitByCapcode(Capcode capcode);

	}

}
