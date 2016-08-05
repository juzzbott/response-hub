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

		Task<Group> CreateGroup(Group group, IList<Region> regions);

		Task<IList<Group>> GetRecentlyAdded(int count, IList<Region> regions);

		Task<bool> CheckIfGroupExists(string name, ServiceType service);

		Task AddUserToGroup(UserMapping userMapping, Guid groupId);

		Task<IList<Group>> GetGroupsForUser(Guid userId, IList<Region> regions);

		Task<IList<Group>> GetAll(IList<Region> regions);

		Task<Group> GetById(Guid id, IList<Region> regions);

		Task<IList<Group>> GetByIds(IEnumerable<Guid> ids, IList<Region> regions);

		Task<IList<Group>> FindByName(string name, IList<Region> regions);

		/// <summary>
		/// Updates a group in the database with the specified group values.
		/// </summary>
		/// <param name="group">The group to save to the database.</param>
		/// <returns></returns>
		Task UpdateGroup(Group group);

		Task ChangeUserRoleInGroup(Guid groupId, Guid userId, string newRole);

		Task<IDictionary<Guid, UserMapping>> GetUserMappingsForUser(Guid userId);

	}

}