using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.Identity;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Users;

using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{
	[MongoCollectionName("users")]
	public class UserRepository : MongoRepository<IdentityUserDto>, IUserRepository
	{

		#region IUserStore

		public async Task CreateAsync(IdentityUser user)
		{
			await Save(MapToDbObject(user));
		}

		public async Task DeleteAsync(IdentityUser user)
		{
			await Remove(MapToDbObject(user));
		}

		public void Dispose()
		{
			
		}

		public async Task<IdentityUser> FindByIdAsync(Guid userId)
		{
			IdentityUserDto dbObj = await GetById(userId);

			return MapToModel(dbObj);
		}

		public async Task<IdentityUser> FindByNameAsync(string userName)
		{
			IdentityUserDto dbObj = await FindOne(i => i.UserName.ToLower() == userName.ToLower());

			return MapToModel(dbObj);
		}

		public async Task UpdateAsync(IdentityUser user)
		{
			await Save(MapToDbObject(user));
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
			IList<IdentityUserDto> userWithLogin = await Find(u => u.Logins.Any(l => l.LoginProvider == login.LoginProvider && l.ProviderKey == login.ProviderKey));

			return (IdentityUser)MapToModel(userWithLogin.FirstOrDefault());
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
			IdentityUserDto dbObj = await FindOne(i => i.UserName.ToUpperInvariant() == email.ToUpperInvariant());

			return MapToModel(dbObj);

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

		#region IUserClaimStore 

		/// <summary>
		///  Adds the claim to the user if it does not current exist. Both the type and value will be used to determine if a claim exists.
		/// </summary>
		/// <param name="user">The user to add the claim to.</param>
		/// <param name="claim">The claim to add to the user.</param>
		/// <returns></returns>
		public async Task AddClaimAsync(IdentityUser user, Claim claim)
		{
			// If the existing claim does not exist, add it
			if (!user.Claims.Any(i => i.Type.Equals(claim.Type, StringComparison.CurrentCultureIgnoreCase) && i.Value.Equals(claim.Value, StringComparison.CurrentCultureIgnoreCase)))
			{
				user.Claims.Add(claim);
				await UpdateAsync(user);
			}
		}

		/// <summary>
		/// Gets the users claims.
		/// </summary>
		/// <param name="user">The user to get the claims for.</param>
		/// <returns></returns>
		public Task<IList<Claim>> GetClaimsAsync(IdentityUser user)
		{
			return Task.FromResult(user.Claims);
		}

		/// <summary>
		/// Removes the claim from user. The Type and Value will be matched when removing the claim.
		/// </summary>
		/// <param name="user">The user to remove the claim from.</param>
		/// <param name="claim">The claim to remove.</param>
		/// <returns></returns>
		public async Task RemoveClaimAsync(IdentityUser user, Claim claim)
		{

			// Find the claim to remove based on the type and value
			Claim claimToRemove = user.Claims.FirstOrDefault(i => i.Type.Equals(claim.Type, StringComparison.CurrentCultureIgnoreCase) &&
				i.Value.Equals(claim.Value, StringComparison.CurrentCultureIgnoreCase));

			// If the claim exists, remove it
			if (claimToRemove != null)
			{
				// Remove the claim
				user.Claims.Remove(claim);

				// Update the user
				await UpdateAsync(user);

			}
		}

		#endregion

		#region Response Hub members

		/// <summary>
		/// Gets the collection of users based on the user ids.
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public async Task<IList<IdentityUser>> GetUsersByIds(IEnumerable<Guid> userIds)
		{
			// Find the the list of user dtos based on the collection of User Ids.
			IList<IdentityUserDto> userDtos = await Collection.Find(Builders<IdentityUserDto>.Filter.In(i => i.Id, userIds)).ToListAsync();

			return userDtos.Select(i => MapToModel(i)).ToList();
		}

		/// <summary>
		/// Gets the user by the specified activation token. 
		/// </summary>
		/// <param name="activationToken">The activation token to get the user by.</param>
		/// <returns>The identity user that has the specified activation token.</returns>
		public async Task<IdentityUser> GetUserByActivationToken(string activationToken)
		{

			// Get the user by the activation token.
			IdentityUserDto user = await FindOne(i => i.ActivationCode.ToLower() == activationToken.ToLower());

			// If the user is null, return null
			return (user != null ? MapToModel(user) : null);
		}

		/// <summary>
		/// Activates the account for the user.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task ActivateAccount(Guid id)
		{
			// Update the activation token to null to indicate activated user.
			await Collection.UpdateOneAsync(Builders<IdentityUserDto>.Filter.Eq(i => i.Id, id), Builders<IdentityUserDto>.Update.Set(i => i.ActivationCode, null));
		}

		/// <summary>
		/// Gets the list of users that match the search results.
		/// </summary>
		/// <param name="keywords"></param>
		/// <returns></returns>
		public async Task<IList<IdentityUser>> SearchUsers(string keywords)
		{

			// Specify the keywords as lower case
			PagedResultSet<IdentityUserDto> results = await TextSearch(keywords, Int32.MaxValue, 0, false);

			// Order by first name
			results.Items = results.Items.OrderBy(i => i.FirstName).ToList();

			// Create the list of groups
			List<IdentityUser> mappedUsers = new List<IdentityUser>();

			// For each result, map to a Group model object.
			foreach (IdentityUserDto result in results.Items)
			{
				mappedUsers.Add(MapToModel(result));
			}

			// return the mapped groups.
			return mappedUsers;
		}

		/// <summary>
		/// Determines if the email address exists for a user in the system.
		/// </summary>
		/// <param name="emailAddress">The email address to search for.</param>
		/// <returns>True if a user account with that email address exists, otherwise false.</returns>
		public async Task<bool> EmailAddressExists(string emailAddress)
		{
			// Get the number of users with the email address
			long results = await Collection.CountAsync(i => i.EmailAddress.ToLower() == emailAddress.ToLower());

			// If no results, then no users.
			return (results > 0);

		}

		/// <summary>
		/// Gets all the users in the system. The default sort order is by first name.
		/// </summary>
		/// <returns>The list of all users in the system.</returns>
		public new async Task<IList<IdentityUser>> GetAll()
		{
			IList<IdentityUserDto> allDtoUsers = await Collection.Find<IdentityUserDto>(new BsonDocument()).ToListAsync();

			// Map to identity users
			IList<IdentityUser> users = new List<IdentityUser>();
			foreach(IdentityUserDto dtoUser in allDtoUsers)
			{
				users.Add(MapToModel(dtoUser));
			}

			// Sort the users by first name
			users = users.OrderBy(i => i.FirstName).ToList();

			// return the users
			return users;

		}

		/// <summary>
		/// Updates the first name and surname for the specified account. 
		/// </summary>
		/// <param name="userId">The id of the user to update the first name and surname for. </param>
		/// <param name="firstName">The new first name. </param>
		/// <param name="surname">The new surname.</param>
		public async Task UpdateAccountDetails(Guid userId, string firstName, string surname)
		{
			// Create the filter
			FilterDefinition<IdentityUserDto> filter = Builders<IdentityUserDto>.Filter.Eq(i => i.Id, userId);

			// Create the update
			UpdateDefinition<IdentityUserDto> update = Builders<IdentityUserDto>.Update
				.Set(i => i.FirstName, firstName)
				.Set(i => i.Surname, surname);

			// Update the database
			await Collection.UpdateOneAsync(filter, update);
		}

		/// <summary>
		/// Updates the username and email address properties for the specified user. 
		/// </summary>
		/// <param name="userId">The ID of the user to update the email address and username for.</param>
		/// <param name="newUsername">The new user email address.</param>
		public async Task UpdateAccountUsername(Guid userId, string newUsername)
		{
			// Create the filter
			FilterDefinition<IdentityUserDto> filter = Builders<IdentityUserDto>.Filter.Eq(i => i.Id, userId);

			// Create the update
			UpdateDefinition<IdentityUserDto> update = Builders<IdentityUserDto>.Update
				.Set(i => i.EmailAddress, newUsername)
				.Set(i => i.UserName, newUsername);

			// Update the database
			await Collection.UpdateOneAsync(filter, update);
		}

		/// <summary>
		/// Gets the 
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task<IdentityUser> GetUserByForgottenPasswordToken(string token)
		{

			// If the token is null or empty, then throw exception
			if (String.IsNullOrEmpty(token))
			{
				throw new ArgumentException("The 'token' parameter cannot be null or empty.");
			}

			// Build the query
			IdentityUserDto user = await FindOne(i => i.PasswordResetToken.Token.ToLower() == token.ToLower());

			// return the user from the database.
			return MapToModel(user);

		}

		#endregion

		#region Mappers

		/// <summary>
		/// Maps the IdentityUser DTO object to the IdentityUser model object.
		/// </summary>
		/// <param name="dbObject"></param>
		/// <returns></returns>
		public IdentityUser MapToModel(IdentityUserDto dbObject)
		{

			if (dbObject == null)
			{
				return null;
			}

			IdentityUser user = new IdentityUser()
			{
				Created = dbObject.Created,
				EmailAddress = dbObject.EmailAddress,
				FirstName = dbObject.FirstName,
				Id = dbObject.Id,
				Logins = dbObject.Logins,
				PasswordHash = dbObject.PasswordHash,
				PasswordResetToken = dbObject.PasswordResetToken,
				Surname = dbObject.Surname,
				UserName = dbObject.UserName,
				ActivationCode = dbObject.ActivationCode
			};

			// Map the claims
			user.Claims = dbObject.Claims.Select(i => MapClaimToModel(i)).ToList();

			// return the user.
			return user;

		}

		/// <summary>
		/// Maps the IdentityUser model object to the IdentityUser DTO object.
		/// </summary>
		/// <param name="modelObj"></param>
		/// <returns></returns>
		public IdentityUserDto MapToDbObject(IdentityUser modelObj)
		{


			if (modelObj == null)
			{
				return null;
			}

			IdentityUserDto user = new IdentityUserDto()
			{
				Created = modelObj.Created,
				EmailAddress = modelObj.EmailAddress,
				FirstName = modelObj.FirstName,
				Id = modelObj.Id,
				Logins = modelObj.Logins,
				PasswordHash = modelObj.PasswordHash,
				PasswordResetToken = modelObj.PasswordResetToken,
				Surname = modelObj.Surname,
				UserName = modelObj.UserName,
				ActivationCode = modelObj.ActivationCode
			};

			// Map the claims
			user.Claims = modelObj.Claims.Select(i => MapClaimToDbObject(i)).ToList();

			// return the user.
			return user;
		}

		/// <summary>
		/// Maps a Claim DTO object to the Identity claim object.
		/// </summary>
		/// <param name="dbObject"></param>
		/// <returns></returns>
		public Claim MapClaimToModel(ClaimDto dbObject)
		{
			
			if (dbObject == null)
			{
				return null;
			}

			return new Claim(dbObject.Type, dbObject.Value, ClaimValueTypes.String, dbObject.Issuer);
		}

		/// <summary>
		/// Maps an Identity claim object to the Claim DTO object.
		/// </summary>
		/// <param name="claim"></param>
		/// <returns></returns>
		public ClaimDto MapClaimToDbObject(Claim claim)
		{
			if (claim == null)
			{
				return null;
			}

			return new ClaimDto()
			{
				Issuer = claim.Issuer,
				Type = claim.Type,
				Value = claim.Value
			};
		}

		#endregion

	}
}
