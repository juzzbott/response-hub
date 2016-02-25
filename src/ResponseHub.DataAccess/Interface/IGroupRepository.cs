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

		Task<IList<Group>> FindByName(string name);

		/// <summary>
		/// Updates a group in the database with the specified group values.
		/// </summary>
		/// <param name="group">The group to save to the database.</param>
		/// <returns></returns>
		Task UpdateGroup(Group group);

	}

}
