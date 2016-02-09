using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Groups.Interface
{
	public interface IGroupService
	{

		Task<Group> CreateGroup(string name, ServiceType service, string capCode, Guid groupAdministratorId, string description);

		Task<IList<Group>> GetAll();

		Task<IList<Group>> GetRecentlyAdded(int count);

		Task<Group> GetById(Guid id);

		Task<bool> CheckIfGroupExists(string name, ServiceType service);

	}

}
