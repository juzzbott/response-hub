using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.Identity;

using Enivate.ResponseHub.Caching;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Common.Constants;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Identity.Interface;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class UserService : UserManager<IdentityUser, Guid>, IUserService
	{

		private readonly string UserObjectCachePrefix = "IdentityUser_";

		private IUserRepository _repository;

		private ILogger _log;

		/// <summary>
		/// Creates a new instance of the UserService class.
		/// </summary>
		/// <param name="repository"></param>
		public UserService(IUserRepository repository, ILogger log) : base((IUserStore<IdentityUser, Guid>)repository)
		{
			_repository = repository;
			_log = log;
		}
		
		#region Password Reset Token

		/// <summary>
		/// Generates the user password reset token and stores it against the account.
		/// </summary>
		/// <param name="userId">The ID of the user to get the token for.</param>
		/// <returns>The password reset token.</returns>
		public override async Task<string> GeneratePasswordResetTokenAsync(Guid userId)
		{

			try
			{

				IdentityUser user = await _repository.FindByIdAsync(userId);

				// Generate the new token, and save it to the user.
				PasswordResetToken token = PasswordResetToken.Generate();

				// Update the user entity to give the password reset token.
				user.PasswordResetToken = token;

				// Update the user object
				await UpdateAsync(user);
				
				// return the token
				return token.Token;


			}
			catch (Exception ex)
			{
				await _log.Error(String.Format("Unable to set password reset token for account '{0}'.", userId), ex);
				return null;
			}
		}

		/// <summary>
		/// Resets the account password for the specific user.
		/// </summary>
		/// <param name="userId">The id of the user to reset the password for.</param>
		/// <param name="token">The password reset token to validate in order to reset the password.</param>
		/// <param name="newPassword">The new password value.</param>
		/// <returns>An IdentityResult indicating the result of the password reset.</returns>
		public override async Task<IdentityResult> ResetPasswordAsync(Guid userId, string token, string newPassword)
		{

			try
			{ 

				// Find the user
				IdentityUser user = await _repository.FindByIdAsync(userId);

				// If the token cannot be found or the token is expired, then show invalid token message
				if (user == null ||
					user.PasswordResetToken == null ||
					!user.PasswordResetToken.Token.Equals(token, StringComparison.CurrentCultureIgnoreCase) ||
					user.PasswordResetToken.Expires < DateTime.UtcNow)
				{
					return IdentityResult.Failed("The password reset token is invalid or has expired.");
				}

				// Hash the new account password.
				PasswordHasher hasher = new PasswordHasher();
				string passwordHash = hasher.HashPassword(newPassword);

				// Reset the password and remove the reset password token
				user.PasswordHash = passwordHash;
				user.PasswordResetToken = null;

				// Save the new password details
				await UpdateAsync(user);

				// Clear the cache object
				CacheManager.RemoveEntity(user);

				return IdentityResult.Success;


			}
			catch (Exception ex)
			{
				await _log.Error(String.Format("Unable to change email address for account '{0}'.", userId), ex);
				return IdentityResult.Failed("Unable to change email address for user account.");
			}

		}

		/// <summary>
		/// Verifies the password reset token for the specified user and ensures it's valid and not expired.
		/// </summary>
		/// <param name="userId">The id of the user to reset the password for.</param>
		/// <param name="purpose">The purpose of the verification.</param>
		/// <param name="token">The password reset token to validate in order to reset the password.</param>
		/// <returns>True if the token is successfully verified, otherwise false.</returns>
		public async override Task<bool> VerifyUserTokenAsync(Guid userId, string purpose, string token)
		{

			try
			{ 

				// Find the user
				IdentityUser user = await _repository.FindByIdAsync(userId);

				// If the token cannot be found or the token is expired, then show invalid token message
				if (user == null ||
					user.PasswordResetToken == null ||
					!user.PasswordResetToken.Token.Equals(token, StringComparison.CurrentCultureIgnoreCase) ||
					user.PasswordResetToken.Expires < DateTime.UtcNow)
				{
					return false;
				}
				else
				{
					return true;
				}
			
			}
			catch (Exception ex)
			{
				await _log.Error(String.Format("Unable to verify password reset token for account '{0}'.", userId), ex);
				return false;
			}
		}

		#endregion

		#region Email address eixsts

		/// <summary>
		/// Determines if the user address exists within a user account.
		/// </summary>
		/// <param name="emailAddress">The email address to check for.</param>
		/// <returns>True if the email address is in use by a user, otherwise false.</returns>
		public async Task<bool> CheckEmailUniqueAsync(string emailAddress)
		{

			try
			{ 

				IdentityUser user = await _repository.FindByNameAsync(emailAddress);

				return (user == null);

			}
			catch (Exception ex)
			{
				await _log.Error(String.Format("Unable to verify email is unqiue for email address '{0}'.", emailAddress), ex);
				return false;
			}
		}

		#endregion

		#region Update Email Address

		/// <summary>
		/// Updated the account user name and email address to the new email address. Validated the account password prior to updated.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="newUsername"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public async Task<IdentityResult> UpdateUserNameAsync(Guid userId, string newUsername, string password)
		{

			if (userId == Guid.Empty)
			{
				throw new ArgumentException("The 'userId' parameter cannot be an empty guid.", "userId");
			}

			try
			{ 

				// Get the user
				IdentityUser user = await FindByIdAsync(userId);

				// Ensure the user exists
				if (user == null)
				{
					return IdentityResult.Failed("The user could not be foumd.");
				}

				// Ensure the password is valid
				bool passwordValid = await CheckPasswordAsync(user, password);
				if (!passwordValid)
				{
					return IdentityResult.Failed("The account password you have entered is incorrect.");
				}

				// If the email address already exists, then show the message to the user
				bool emailUnique = await CheckEmailUniqueAsync(newUsername);
				if (!emailUnique)
				{
					return IdentityResult.Failed("The email account you have entered is in use by another user. Please choose a different email address.");
				}

				// Get the user and update the email
				await _repository.UpdateAccountUsername(userId, newUsername);

				// Clear the cache object
				CacheManager.RemoveEntity(user);

				return IdentityResult.Success;

			}
			catch (Exception ex)
			{
				await _log.Error(String.Format("Unable to change email address for account '{0}'.", userId), ex);
				return IdentityResult.Failed("Unable to change email address for user account.");
			}

		}

		#endregion

		#region Create password 

		/// <summary>
		/// Creates an password for the account. Will only succeed if the account does not currently have a password. 
		/// For accounts with an existing password, the ChangePassword method must be used.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="passwordHash"></param>
		/// <returns></returns>
		public async Task<IdentityResult> CreatePasswordAsync(IdentityUser user, string passwordHash)
		{
			if (user == null)
			{
				throw new ArgumentException("The 'user' parameter cannot be null.", "user");
			}
			if (String.IsNullOrEmpty(passwordHash))
			{
				throw new ArgumentException("The 'passwordHash' parameter cannot be null or empty.", "passwordHash");
			}

			// If the user password is not null, then return a failure that the password needs to be changed
			if (!String.IsNullOrEmpty(user.PasswordHash))
			{
				return IdentityResult.Failed("You cannot create a password for a user where a password already exists. Use the 'ChangePassword' method instead.");
			}

			try
			{

				// Create the account password.
				user.PasswordHash = passwordHash;
				IdentityResult result = await UpdateAsync(user);

				return result;
			}
			catch (Exception ex)
			{
				await _log.Error(String.Format("Unable to create password for account '{0}'.", user.Id), ex);
				return IdentityResult.Failed("Unable to create account password.");
			}
		}

		#endregion

		#region Find By Id

		/// <summary>
		/// Overrides the standard FindByIdAsync method to enable user object caching.
		/// </summary>
		/// <param name="userId">The Id of the user to return.</param>
		/// <returns></returns>
		public override async Task<IdentityUser> FindByIdAsync(Guid userId)
		{

			// Get the user from the cache
			IdentityUser user = CacheManager.GetItem<IdentityUser>(CacheUtility.GetEntityCacheKey(typeof(IdentityUser), userId.ToString()));

			// If the user doesn't exist in the cache, get it from the store
			if (user == null)
			{
				// Get the user from the store
				user = await base.FindByIdAsync(userId);

				// If it's not null, add to cache
				if (user != null)
				{
					CacheManager.AddItem(user);
				}
			}

			// return the user.
			return user;
		}

		#endregion

		#region Update user

		/// <summary>
		/// Overrides the UpdateAsync method to remove the entity from cache on update.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public override async Task<IdentityResult> UpdateAsync(IdentityUser user)
		{
			// Update the user details
			IdentityResult result = await base.UpdateAsync(user);

			// If the result was successful, then remove it from the cache.
			if (result.Succeeded)
			{
				CacheManager.RemoveEntity(user);
			}

			// return the result
			return result;
		}

		#endregion

		#region IUserService

		/// <summary>
		/// Creates a new user object
		/// </summary>
		/// <param name="emailAddress"></param>
		/// <param name="firstName"></param>
		/// <param name="surname"></param>
		/// <param name="roles"></param>
		/// <returns></returns>
		public async Task<IdentityUser> CreateAsync(string emailAddress, string firstName, string surname, IList<string> roles)
		{

			// If the roles list is null, instantiate to empty list
			if (roles == null)
			{
				roles = new List<string>();
			}

			string claimIssuer = ConfigurationManager.AppSettings[ConfigurationKeys.ClaimsIssuer];
			if (String.IsNullOrWhiteSpace(claimIssuer))
			{
				claimIssuer = "ResponseHub";
			}

			IList<Claim> claims = new List<Claim>();
			foreach(string role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role, ClaimValueTypes.String, claimIssuer));
			}

			// Create the IdentityUser object
			IdentityUser user = new IdentityUser()
			{
				Created = DateTime.UtcNow,
				EmailAddress = emailAddress,
				FirstName = firstName,
				Surname = surname,
				UserName = emailAddress,
				Claims = claims,
				ActivationCode = HashGenerator.GetSha256HashString(Guid.NewGuid().ToString(), 1)
			};

			// Add the user
			await _repository.CreateAsync(user);

			// return user
			return user;
		}

		/// <summary>
		/// Gets the collection of users based on the user ids.
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public async Task<IList<IdentityUser>> GetUsersByIds(IEnumerable<Guid> userIds)
		{
			return await _repository.GetUsersByIds(userIds);
		}

		/// <summary>
		/// Gets the user by the specified activation token. 
		/// </summary>
		/// <param name="activationToken">The activation token to get the user by.</param>
		/// <returns>The identity user that has the specified activation token.</returns>
		public async Task<IdentityUser> GetUserByActivationToken(string activationToken)
		{
			return await _repository.GetUserByActivationToken(activationToken);
		}

		/// <summary>
		/// Finds the user account based on the forgotten password token. If the token is expired, no user is returned.
		/// </summary>
		/// <param name="token">The reset password token to find the user by.</param>
		/// <returns></returns>
		public async Task<IdentityUser> GetUserByForgottenPasswordToken(string token)
		{
			// Get the user from the db repository
			IdentityUser user = await _repository.GetUserByForgottenPasswordToken(token);

			// If the token couldn't be found or it's expired, just return null
			if (user == null || user.PasswordResetToken.Expires < DateTime.UtcNow)
			{
				return null;
			}
			else
			{
				return user;
			}
		}

		/// <summary>
		/// Activates the user based on the activation token.
		/// </summary>
		/// <param name="activationToken"></param>
		/// <returns></returns>
		public async Task ActivateAccount(Guid id)
		{
			await _repository.ActivateAccount(id);
		}

		/// <summary>
		/// Searches for users that have an Email Address, Firstname or Surname that match the keywords.
		/// </summary>
		/// <param name="keywords"></param>
		/// <returns></returns>
		public async Task<IList<IdentityUser>> SearchUsers(string keywords)
		{
			return await _repository.SearchUsers(keywords);
		}

		/// <summary>
		/// Determines if the email address exists for a user in the system.
		/// </summary>
		/// <param name="emailAddress">The email address to search for.</param>
		/// <returns>True if a user account with that email address exists, otherwise false.</returns>
		public async Task<bool> EmailAddressExists(string emailAddress)
		{
			return await _repository.EmailAddressExists(emailAddress);
		}

		/// <summary>
		/// Gets all the users currently in the system.
		/// </summary>
		/// <returns></returns>
		public async Task<IList<IdentityUser>> GetAll()
		{
			return await _repository.GetAll();
		}

		/// <summary>
		/// Updates the first name and surname for the specified account. 
		/// </summary>
		/// <param name="userId">The id of the user to update the first name and surname for. </param>
		/// <param name="firstName">The new first name. </param>
		/// <param name="surname">The new surname.</param>
		public async Task UpdateAccountDetails(Guid userId, string firstName, string surname)
		{
			await _repository.UpdateAccountDetails(userId, firstName, surname);

			// Clear the cache object
			ClearUserFromCache(userId);
		}

		#endregion

		#region Clear cache methods

		/// <summary>
		/// Removes all user objects from the cache.
		/// </summary>
		private void ClearAllUsersCacheObjects()
		{
			// Get the list of cache keys for user objects
			IList<string> userCacheKeys = CacheManager.GetCacheKeys().Where(i => i.StartsWith(UserObjectCachePrefix)).ToList();

			// Loop through each of the cache keys and remove it
			foreach (string cacheKey in userCacheKeys)
			{
				CacheManager.RemoveItem(cacheKey);
			}

		}

		/// <summary>
		/// Removes a single user from the cache.
		/// </summary>
		/// <param name="userId"></param>
		private void ClearUserFromCache(Guid userId)
		{
			// Get the cache key
			string cacheKey = CacheUtility.GetEntityCacheKey(typeof(IdentityUser), userId.ToString());

			// remove the user from the cache
			CacheManager.RemoveItem(cacheKey);
		}

		#endregion

	}
}
