using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Groups.Interface
{
	public interface IGroupRepository : IRepository<Group>
	{

		Task<Group> CreateGroup(Group group);

		Task<IList<Group>> GetRecentlyAdded(int count);

		Task<bool> CheckIfGroupExists(string name, ServiceType service);

		Task AddUserToGroup(UserMapping userMapping, Guid groupId);

	}

}
