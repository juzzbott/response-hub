using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.Identity;

namespace Enivate.ResponseHub.Model.Identity.Interface
{
	public interface IUserRepository : IRepository<User>, IUserStore<User, Guid>, IUserPasswordStore<User, Guid>
	{

		new Task CreateAsync(User user);

		new Task DeleteAsync(User user);

		new void Dispose();

		new Task<User> FindByIdAsync(Guid userId);

		new Task<User> FindByNameAsync(string userName);

		new Task UpdateAsync(User user);

		new Task<string> GetPasswordHashAsync(User user);

		new Task<bool> HasPasswordAsync(User user);

		new	Task SetPasswordHashAsync(User user, string passwordHash);
		

	}
}
