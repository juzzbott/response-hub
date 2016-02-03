using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.Identity;

namespace Enivate.ResponseHub.Model.Identity.Interface
{
	public interface IRoleRepository : IRepository<Role>, IRoleStore<Role, Guid>
	{

		new Task CreateAsync(Role role);

		new Task DeleteAsync(Role role);

		new Task<Role> FindByIdAsync(Guid roleId);

		new Task<Role> FindByNameAsync(string roleName);

		new Task UpdateAsync(Role role);

	}
}
