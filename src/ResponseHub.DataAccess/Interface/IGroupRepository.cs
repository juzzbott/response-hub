using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Groups;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface IGroupRepository
	{

		Task<Group> CreateGroup(Group group);

		Task<IList<Group>> GetRecentlyAdded(int count);

		Task<bool> CheckIfGroupExists(string name, ServiceType service);

		Task AddUserToGroup(UserMapping userMapping, Guid groupId);

		Task<IList<Group>> GetAll();

		Task<Group> GetById(Guid id);

	}

}
