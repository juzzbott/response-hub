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
	[MongoCollectionName("users")]
	public class UserRepository : MongoRepository<IdentityUser>, IUserRepository
	{

		#region IUserStore

		public async Task CreateAsync(IdentityUser user)
		{
			await Save(user);
		}

		public async Task DeleteAsync(IdentityUser user)
		{
			await Remove(user);
		}

		public void Dispose()
		{
			
		}

		public async Task<IdentityUser> FindByIdAsync(Guid userId)
		{
			return await GetById(userId);
		}

		public async Task<IdentityUser> FindByNameAsync(string userName)
		{
			return await FindOne(i => i.UserName.ToLower() == userName.ToLower());
		}

		public async Task UpdateAsync(IdentityUser user)
		{
			await Save(user);
		}

		#endregion

		#region IPasswordStore

		public Task<string> GetPasswordHashAsync(IdentityUser user)
		{
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			return Task.FromResult(user.PasswordHash);
		}

		public Task<bool> HasPasswordAsync(IdentityUser user)
		{
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			// Determine if the user has a password
			return Task.FromResult(!String.IsNullOrEmpty(user.PasswordHash));

		}

		public async Task SetPasswordHashAsync(IdentityUser user, string passwordHash)
		{
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			// Set the password hash to the database
			user.PasswordHash = passwordHash;
			await UpdateAsync(user);
			
		}

		#endregion

		#region IUserLoginStore

		public async Task AddLoginAsync(IdentityUser user, UserLoginInfo login)
		{
			// If the login does not already exist, create it.
			if (!user.Logins.Contains(login))
			{
				user.Logins.Add(login);
				await UpdateAsync(user);
			}

			return;
		}

		public async Task RemoveLoginAsync(IdentityUser user, UserLoginInfo login)
		{
			user.Logins.Remove(login);
			await UpdateAsync(user);
			return;
		}

		public Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user)
		{
			return Task.FromResult((IList<UserLoginInfo>)user.Logins);
		}

		public async Task<IdentityUser> FindAsync(UserLoginInfo login)
		{
			IList<IdentityUser> userWithLogin = await Find(u => u.Logins.Any(l => l.LoginProvider == login.LoginProvider && l.ProviderKey == login.ProviderKey));

			return (IdentityUser)userWithLogin.FirstOrDefault();
		}

		#endregion

		#region IUserEmailStore

		/// <summary>
		/// Gets the user entity based on the users' email address.
		/// </summary>
		/// <param name="email">The email address of the user to get.</param>
		/// <returns>The user object, or null.</returns>
		public async Task<IdentityUser> FindByEmailAsync(string email)
		{

			// Ensure the email is not null or empty before looking up the email address.
			if (String.IsNullOrEmpty(email))
			{
				throw new ArgumentNullException("email");
			}

			// Get the user from the repository
			return await FindOne(i => i.UserName.ToUpperInvariant() == email.ToUpperInvariant());

		}

		/// <summary>
		/// Updates the email address and username properties of the user.
		/// </summary>
		/// <param name="user">The user object to update.</param>
		/// <param name="email">The email address to update.</param>
		public async Task SetEmailAsync(IdentityUser user, string email)
		{
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			// Set the password hash to the database
			user.EmailAddress = email;
			user.UserName = email;
			await UpdateAsync(user);

			return;
		}

		/// <summary>
		/// Gets the email address of the user.
		/// </summary>
		/// <param name="user">The user object to get the email address for.</param>
		/// <returns>The users' email address.</returns>
		public Task<string> GetEmailAsync(IdentityUser user)
		{
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			return Task.FromResult(user.EmailAddress);
		}

		[Obsolete("This method is not implemented.", true)]
		public Task<bool> GetEmailConfirmedAsync(IdentityUser user)
		{
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			return Task.FromResult(true);
		}

		[Obsolete("This method is not implemented.", true)]
		public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed)
		{

			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			return Task.FromResult(0);
		}

		#endregion

		#region IUserRoleStore

		/// <summary>
		/// Adds the user to the specified role.
		/// </summary>
		/// <param name="user">The user to add the role to.</param>
		/// <param name="roleName">The name of the role to add the user to.</param>
		/// <returns></returns>
		public async Task AddToRoleAsync(IdentityUser user, string roleName)
		{
			// If the role does not exist in the list, add it
			if (!user.Roles.Contains(roleName, StringComparer.CurrentCultureIgnoreCase))
			{
				user.Roles.Add(roleName);
				await UpdateAsync(user);
			}
			return;
		}

		/// <summary>
		/// Removes the user from the specified role.
		/// </summary>
		/// <param name="user">The user to remove the role from.</param>
		/// <param name="roleName">The name of the role to remove the user from.</param>
		/// <returns></returns>
		public async Task RemoveFromRoleAsync(IdentityUser user, string roleName)
		{
			user.Roles.Remove(roleName);
			await UpdateAsync(user);
			return;
		}

		/// <summary>
		/// Returns the list of roles the user is a member of.
		/// </summary>
		/// <param name="user">The user to get the roles for.</param>
		/// <returns></returns>
		public Task<IList<string>> GetRolesAsync(IdentityUser user)
		{
			return Task.Run(() => user.Roles);
		}

		/// <summary>
		/// Determines if the user is currently in the specified role or not.
		/// </summary>
		/// <param name="user">The user to check if is in the role.</param>
		/// <param name="roleName">The name of the role to check for.</param>
		/// <returns>True if the user is in the role, otherwise false.</returns>
		public Task<bool> IsInRoleAsync(IdentityUser user, string roleName)
		{
			return Task.Run(() => user.Roles.Contains(roleName, StringComparer.CurrentCultureIgnoreCase));
		}

		#endregion

		#region IUserLockoutStore

		/// <summary>
		/// Not implemented
		/// </summary>
		public Task<DateTimeOffset> GetLockoutEndDateAsync(IdentityUser user)
		{
			return Task.FromResult<DateTimeOffset>(new DateTimeOffset(DateTime.UtcNow.AddYears(99)));
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		public Task SetLockoutEndDateAsync(IdentityUser user, DateTimeOffset lockoutEnd)
		{
			return Task.FromResult(0);
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		public Task<int> IncrementAccessFailedCountAsync(IdentityUser user)
		{
			return Task.FromResult<int>(-1);
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		public Task ResetAccessFailedCountAsync(IdentityUser user)
		{
			return Task.FromResult(0);
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		public Task<int> GetAccessFailedCountAsync(IdentityUser user)
		{
			return Task.FromResult<int>(-1);
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		public Task<bool> GetLockoutEnabledAsync(IdentityUser user)
		{
			return Task.FromResult<bool>(false);
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		public Task SetLockoutEnabledAsync(IdentityUser user, bool enabled)
		{
			return Task.FromResult(0);
		}

		#endregion

		#region IUserTwoFactorStore

		/// <summary>
		/// Not implemented
		/// </summary>
		public Task SetTwoFactorEnabledAsync(IdentityUser user, bool enabled)
		{
			return Task.FromResult(0);
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		public Task<bool> GetTwoFactorEnabledAsync(IdentityUser user)
		{
			return Task.FromResult<bool>(false);
		}

		#endregion

	}
}
