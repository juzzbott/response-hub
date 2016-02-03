using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.Identity;

namespace Enivate.ResponseHub.Model.Identity.Interface
{
	public interface IUserRepository : IRepository<IdentityUser>, IUserStore<IdentityUser, Guid>, IUserPasswordStore<IdentityUser, Guid>
	{

		new Task CreateAsync(IdentityUser user);

		new Task DeleteAsync(IdentityUser user);

		new void Dispose();

		new Task<IdentityUser> FindByIdAsync(Guid userId);

		new Task<IdentityUser> FindByNameAsync(string userName);

		new Task UpdateAsync(IdentityUser user);

		new Task<string> GetPasswordHashAsync(IdentityUser user);

		new Task<bool> HasPasswordAsync(IdentityUser user);

		new	Task SetPasswordHashAsync(IdentityUser user, string passwordHash);
		

	}
}
