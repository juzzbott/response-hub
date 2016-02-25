using Enivate.ResponseHub.Model.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Groups.Interface
{
	public interface IGroupService
	{

		Task<Group> CreateGroup(string name, ServiceType service, string capcode, Guid groupAdministratorId, string description, Region region, Coordinates headquartersCoords);

		Task<IList<Group>> GetAll();

		Task<IList<Group>> GetRecentlyAdded(int count);

		Task<Group> GetById(Guid id);

		Task<bool> CheckIfGroupExists(string name, ServiceType service);

		Task AddUserToGroup(Guid userId, string role, Guid groupId);

		Task<IList<Region>> GetRegions();

		Task<IList<Group>> FindByName(string name);

	}

}
