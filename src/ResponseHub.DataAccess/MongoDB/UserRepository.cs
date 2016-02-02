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
	public class UserRepository : MongoRepository<User>, IUserStore<User, Guid>, IUserRepository
	{
		public async Task CreateAsync(User user)
		{
			await Save(user);
		}

		public async Task DeleteAsync(User user)
		{
			await Remove(user);
		}

		public void Dispose()
		{
			
		}

		public async Task<User> FindByIdAsync(Guid userId)
		{
			return await GetById(userId);
		}

		public async Task<User> FindByNameAsync(string userName)
		{
			return await FindOne(i => i.UserName.ToLower() == userName.ToLower());
		}

		public Task<string> GetPasswordHashAsync(User user)
		{
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			return Task.FromResult(user.PasswordHash);
		}

		public Task<bool> HasPasswordAsync(User user)
		{
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			// Determine if the user has a password
			return Task.FromResult(!String.IsNullOrEmpty(user.PasswordHash));

		}

		public async Task SetPasswordHashAsync(User user, string passwordHash)
		{
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			// Set the password hash to the database
			user.PasswordHash = passwordHash;
			await UpdateAsync(user);
			
		}

		public async Task UpdateAsync(User user)
		{
			await Save(user);
		}
	}
}
