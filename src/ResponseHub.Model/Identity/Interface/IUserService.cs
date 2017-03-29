using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Identity.Interface
{

	/// <summary>
	/// Inteface is based of ASP.NET identity UserManager class. Used for unity injection of UserService.
	/// </summary>
	public interface IUserService
	{

		#region UserManager members

		IClaimsIdentityFactory<IdentityUser, Guid> ClaimsIdentityFactory { get; set; }
		TimeSpan DefaultAccountLockoutTimeSpan { get; set; }
		IIdentityMessageService EmailService { get; set; }
		int MaxFailedAccessAttemptsBeforeLockout { get; set; }
		IPasswordHasher PasswordHasher { get; set; }
		IIdentityValidator<string> PasswordValidator { get; set; }
		IIdentityMessageService SmsService { get; set; }
		bool SupportsQueryableUsers { get; }
		bool SupportsUserClaim { get; }
		bool SupportsUserEmail { get; }
		bool SupportsUserLockout { get; }
		bool SupportsUserLogin { get; }
		bool SupportsUserPassword { get; }
		bool SupportsUserPhoneNumber { get; }
		bool SupportsUserRole { get; }
		bool SupportsUserSecurityStamp { get; }
		bool SupportsUserTwoFactor { get; }
		IDictionary<string, IUserTokenProvider<IdentityUser, Guid>> TwoFactorProviders { get; }
		bool UserLockoutEnabledByDefault { get; set; }
		IQueryable<IdentityUser> Users { get; }
		IUserTokenProvider<IdentityUser, Guid> UserTokenProvider { get; set; }
		IIdentityValidator<IdentityUser> UserValidator { get; set; }

		Task<IdentityResult> AccessFailedAsync(Guid userId);
		Task<IdentityResult> AddClaimAsync(Guid userId, Claim claim);
		Task<IdentityResult> AddLoginAsync(Guid userId, UserLoginInfo login);
		Task<IdentityResult> AddPasswordAsync(Guid userId, string password);
		Task<IdentityResult> AddToRoleAsync(Guid userId, string role);
		Task<IdentityResult> AddToRolesAsync(Guid userId, params string[] roles);
		Task<IdentityResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
		Task<IdentityResult> ChangePhoneNumberAsync(Guid userId, string phoneNumber, string token);
		Task<bool> CheckPasswordAsync(IdentityUser user, string password);
		Task<IdentityResult> ConfirmEmailAsync(Guid userId, string token);
		Task<IdentityResult> CreateAsync(IdentityUser user);
		Task<IdentityResult> CreateAsync(IdentityUser user, string password);
		Task<ClaimsIdentity> CreateIdentityAsync(IdentityUser user, string authenticationType);
		Task<IdentityResult> DeleteAsync(IdentityUser user);
		void Dispose();
		Task<IdentityUser> FindAsync(UserLoginInfo login);
		Task<IdentityUser> FindAsync(string userName, string password);
		Task<IdentityUser> FindByEmailAsync(string email);
		Task<IdentityUser> FindByIdAsync(Guid userId);
		Task<IdentityUser> FindByNameAsync(string userName);
		Task<string> GenerateChangePhoneNumberTokenAsync(Guid userId, string phoneNumber);
		Task<string> GenerateEmailConfirmationTokenAsync(Guid userId);
		Task<string> GeneratePasswordResetTokenAsync(Guid userId);
		Task<string> GenerateTwoFactorTokenAsync(Guid userId, string twoFactorProvider);
		Task<string> GenerateUserTokenAsync(string purpose, Guid userId);
		Task<int> GetAccessFailedCountAsync(Guid userId);
		Task<IList<Claim>> GetClaimsAsync(Guid userId);
		Task<string> GetEmailAsync(Guid userId);
		Task<bool> GetLockoutEnabledAsync(Guid userId);
		Task<DateTimeOffset> GetLockoutEndDateAsync(Guid userId);
		Task<IList<UserLoginInfo>> GetLoginsAsync(Guid userId);
		Task<string> GetPhoneNumberAsync(Guid userId);
		Task<IList<string>> GetRolesAsync(Guid userId);
		Task<string> GetSecurityStampAsync(Guid userId);
		Task<bool> GetTwoFactorEnabledAsync(Guid userId);
		Task<IList<string>> GetValidTwoFactorProvidersAsync(Guid userId);
		Task<bool> HasPasswordAsync(Guid userId);
		Task<bool> IsEmailConfirmedAsync(Guid userId);
		Task<bool> IsInRoleAsync(Guid userId, string role);
		Task<bool> IsLockedOutAsync(Guid userId);
		Task<bool> IsPhoneNumberConfirmedAsync(Guid userId);
		Task<IdentityResult> NotifyTwoFactorTokenAsync(Guid userId, string twoFactorProvider, string token);
		void RegisterTwoFactorProvider(string twoFactorProvider, IUserTokenProvider<IdentityUser, Guid> provider);
		Task<IdentityResult> RemoveClaimAsync(Guid userId, Claim claim);
		Task<IdentityResult> RemoveFromRoleAsync(Guid userId, string role);
		Task<IdentityResult> RemoveFromRolesAsync(Guid userId, params string[] roles);
		Task<IdentityResult> RemoveLoginAsync(Guid userId, UserLoginInfo login);
		Task<IdentityResult> RemovePasswordAsync(Guid userId);
		Task<IdentityResult> ResetAccessFailedCountAsync(Guid userId);
		Task<IdentityResult> ResetPasswordAsync(Guid userId, string token, string newPassword);
		Task SendEmailAsync(Guid userId, string subject, string body);
		Task SendSmsAsync(Guid userId, string message);
		Task<IdentityResult> SetEmailAsync(Guid userId, string email);
		Task<IdentityResult> SetLockoutEnabledAsync(Guid userId, bool enabled);
		Task<IdentityResult> SetLockoutEndDateAsync(Guid userId, DateTimeOffset lockoutEnd);
		Task<IdentityResult> SetPhoneNumberAsync(Guid userId, string phoneNumber);
		Task<IdentityResult> SetTwoFactorEnabledAsync(Guid userId, bool enabled);
		Task<IdentityResult> UpdateAsync(IdentityUser user);
		Task<IdentityResult> UpdateSecurityStampAsync(Guid userId);
		Task<bool> VerifyChangePhoneNumberTokenAsync(Guid userId, string token, string phoneNumber);
		Task<bool> VerifyTwoFactorTokenAsync(Guid userId, string twoFactorProvider, string token);
		Task<bool> VerifyUserTokenAsync(Guid userId, string purpose, string token);

		#endregion

		#region IUserService members

		Task<IdentityResult> UpdateUserNameAsync(Guid userId, string newUsername, string password);

		Task<IdentityUser> CreateAsync(string emailAddress, string firstName, string surname, IList<string> roles, UserProfile profile);

		Task<IList<IdentityUser>> GetUsersByIds(IEnumerable<Guid> userIds);

		Task<IdentityUser> GetUserByActivationToken(string activationToken);

		Task ActivateAccount(Guid id);

		Task<IList<IdentityUser>> SearchUsers(string keywords);

		Task<bool> EmailAddressExists(string emailAddress);

		Task<IList<IdentityUser>> GetAll();

		Task<IdentityResult> CreatePasswordAsync(IdentityUser user, string passwordHash);

		Task UpdateAccountDetails(Guid userId, string firstName, string surname);

		Task<IdentityUser> GetUserByForgottenPasswordToken(string token);

		#endregion

	}
}
