using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Groups.Interface;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{
	public class GroupRepository : MongoRepository<Group>, IGroupRepository
	{
		
		public GroupRepository()
		{
		}

		/// <summary>
		/// Creates the group in the database.
		/// </summary>
		/// <param name="group">The group to create.</param>
		/// <returns>The saved group.</returns>
		public async Task<Group> CreateGroup(Group group)
		{
			// Save the group to the database.
			group = await Save(group);

			// return the group
			return group;
		}
	}
}
