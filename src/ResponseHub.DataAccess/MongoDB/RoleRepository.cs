using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.Identity;

using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Identity.Interface;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{
	public class RoleRepository : MongoRepository<Role>, IRoleRepository
	{

		/// <summary>
		/// Creates a new role.
		/// </summary>
		/// <param name="role">The role to add.</param>
		/// <returns></returns>
		public async Task CreateAsync(Role role)
		{
			await Add(role);
		}

		/// <summary>
		/// Removes a role.
		/// </summary>
		/// <param name="role">The role to remove.</param>
		/// <returns></returns>
		public Task DeleteAsync(Role role)
		{
			return Task.Run(() => Remove(role));
		}

		public void Dispose()
		{
			// no need to dispose of anything, mongodb handles connection pooling automatically
		}

		/// <summary>
		/// Finds the role based on the Id.
		/// </summary>
		/// <param name="roleId">The role id.</param>
		/// <returns>The role if it exists otherwise null.</returns>
		public Task<Role> FindByIdAsync(Guid roleId)
		{

			// Ensure a role name is specified
			if (roleId == Guid.Empty)
			{
				return Task.FromResult<Role>(null);
			}

			Task<Role> result = Task.Run(() => GetById(roleId));

			return result;
		}

		/// <summary>
		/// Finds the role based on the name of the role.
		/// </summary>
		/// <param name="roleName">The name of the role.</param>
		/// <returns>The role if it exists otherwise null.</returns>
		public Task<Role> FindByNameAsync(string roleName)
		{

			// Ensure a role name is specified
			if (String.IsNullOrEmpty(roleName))
			{
				return Task.FromResult<Role>(null);
			}

			Task<Role> result = Task.Run(() => FindOne(i => i.Name.ToUpper() == roleName.ToUpper()));

			return result;
		}

		/// <summary>
		/// Updates the role.
		/// </summary>
		/// <param name="role">The role to update.</param>
		/// <returns></returns>
		public Task UpdateAsync(Role role)
		{
			return Task.Run(() => Save(role));
		}

	}
}
