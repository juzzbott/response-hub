using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNet.Identity;
using System.Security.Claims;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Users;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface IUserRepository : IUserStore<IdentityUser, Guid>, 
		IUserPasswordStore<IdentityUser, Guid>,
		IUserLoginStore<IdentityUser, Guid>,
		IUserEmailStore<IdentityUser, Guid>,
		IUserLockoutStore<IdentityUser, Guid>,
		IUserTwoFactorStore<IdentityUser, Guid>,
		IUserClaimStore<IdentityUser, Guid>,
		IRepository<IdentityUserDto>
	{

		#region IUserStore

		new Task CreateAsync(IdentityUser user);

		new Task DeleteAsync(IdentityUser user);

		new void Dispose();

		new Task<IdentityUser> FindByIdAsync(Guid userId);

		new Task<IdentityUser> FindByNameAsync(string userName);

		new Task UpdateAsync(IdentityUser user);

		#endregion

		#region IUserPasswordStore

		new Task<string> GetPasswordHashAsync(IdentityUser user);

		new Task<bool> HasPasswordAsync(IdentityUser user);

		new	Task SetPasswordHashAsync(IdentityUser user, string passwordHash);

		#endregion

		#region IUserLoginStore

		new Task AddLoginAsync(IdentityUser user, UserLoginInfo login);

		new Task<IdentityUser> FindAsync(UserLoginInfo login);

		new Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user);

		new Task RemoveLoginAsync(IdentityUser user, UserLoginInfo login);

		#endregion

		#region IUserEmailStore

		new Task<IdentityUser> FindByEmailAsync(string email);
		
		new Task<string> GetEmailAsync(IdentityUser user);
		
		new Task<bool> GetEmailConfirmedAsync(IdentityUser user);
		
		new Task SetEmailAsync(IdentityUser user, string email);
		
		new Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed);

		#endregion

		#region IUserLockoutStore

		new Task<int> GetAccessFailedCountAsync(IdentityUser user);

		new Task<bool> GetLockoutEnabledAsync(IdentityUser user);

		new Task<DateTimeOffset> GetLockoutEndDateAsync(IdentityUser user);

		new Task<int> IncrementAccessFailedCountAsync(IdentityUser user);

		new Task ResetAccessFailedCountAsync(IdentityUser user);

		new Task SetLockoutEnabledAsync(IdentityUser user, bool enabled);

		new Task SetLockoutEndDateAsync(IdentityUser user, DateTimeOffset lockoutEnd);

		#endregion

		#region IUserTwoFactorStore

		new Task<bool> GetTwoFactorEnabledAsync(IdentityUser user);

		new Task SetTwoFactorEnabledAsync(IdentityUser user, bool enabled);

		#endregion

		#region IUserClaimStore

		new Task AddClaimAsync(IdentityUser user, Claim claim);
		new Task<IList<Claim>> GetClaimsAsync(IdentityUser user);
		new Task RemoveClaimAsync(IdentityUser user, Claim claim);

		#endregion

		#region Response Hub methods

		Task<IList<IdentityUser>> GetUsersByIds(IEnumerable<Guid> userIds);

		#endregion

	}
}
