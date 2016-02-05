﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.Identity;

using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Identity.Interface;

namespace Enivate.ResponseHub.ApplicationServices.Identity
{
	public class UserService : UserManager<IdentityUser, Guid>, IUserService
	{

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
				await _repository.UpdateAsync(user);

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
				await _repository.UpdateAsync(user);

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
		/// <param name="newUserName"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public async Task<IdentityResult> UpdateUserNameAsync(Guid userId, string newUserName, string password)
		{

			if (userId == Guid.Empty)
			{
				throw new ArgumentException("The 'userId' parameter cannot be an empty guid.", "userId");
			}

			try
			{ 

				// Get the user
				IdentityUser user = await this.FindByIdAsync(userId);

				// Ensure the password is valid
				bool passwordValid = await CheckPasswordAsync(user, password);
				if (!passwordValid)
				{
					return IdentityResult.Failed("The account password you have entered is incorrect.");
				}

				// If the email address already exists, then show the message to the user
				bool emailUnique = await this.CheckEmailUniqueAsync(newUserName);
				if (!emailUnique)
				{
					IdentityResult.Failed("The email account you have entered is in use by another user. Please choose a different email address.");
				}

				// Get the user and update the email
				user.EmailAddress = newUserName;
				user.UserName = newUserName;

				IdentityResult result = await UpdateAsync(user);

				return result;

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

	}
}
