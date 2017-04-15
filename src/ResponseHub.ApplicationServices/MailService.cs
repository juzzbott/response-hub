using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Common.Constants;
using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Mail;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class MailService : IMailService
	{

		private readonly string _baseUrl;

		public MailService()
		{
			_baseUrl = ConfigurationManager.AppSettings[ConfigurationKeys.BaseWebsiteUrl] ?? "";
		}

		/// <summary>
		/// Sends the account activation email to the new user.
		/// </summary>
		/// <param name="newUser">The new user to send the account for.</param>
		/// <returns>Async task.</returns>
		public async Task SendAccountActivationEmail(IdentityUser newUser)
		{

			// Create the tuple for the to override
			Tuple<string, string> to = new Tuple<string, string>(newUser.EmailAddress, newUser.FullName);

			// Create the replacements
			IDictionary<string, string> replacements = new Dictionary<string, string>();
			replacements.Add("#BaseUrl#", _baseUrl);
			replacements.Add("#FirstName#", newUser.FirstName);
			replacements.Add("#ActivationLink#", String.Format("{0}/my-account/activate/{1}", _baseUrl, newUser.ActivationCode.ToLower()));

			// Create the mail provider and send the message
			MailProvider mailProvider = new MailProvider();
			await mailProvider.SendMailMessage(MailTemplates.ActivateAccount, replacements, to, null);

		}

		public async Task SendUnitCreatedEmail(IdentityUser unitAdmin, string unitName, ServiceType service, string capcode)
		{
			// Create the tuple for the to override
			Tuple<string, string> to = new Tuple<string, string>(unitAdmin.EmailAddress, unitAdmin.FullName);

			// Create the replacements
			IDictionary<string, string> replacements = new Dictionary<string, string>();
			replacements.Add("#BaseUrl#", _baseUrl);
			replacements.Add("#UnitAdministratorName#", unitAdmin.FullName);
			replacements.Add("#UnitName#", unitName);
			replacements.Add("#ServiceType#", service.GetEnumDescription());
			replacements.Add("#Capcode#", capcode);

			// Create the mail provider and send the message
			MailProvider mailProvider = new MailProvider();
			await mailProvider.SendMailMessage(MailTemplates.UnitCreated, replacements, to, null);

		}

		public async Task SendForgottenPasswordToken(IdentityUser user, string token)
		{
			// Create the tuple for the to override
			Tuple<string, string> to = new Tuple<string, string>(user.EmailAddress, user.FullName);

			// Create the replacements
			IDictionary<string, string> replacements = new Dictionary<string, string>();
			replacements.Add("#BaseUrl#", _baseUrl);
			replacements.Add("#FirstName#", user.FirstName);
			replacements.Add("#ResetPasswordLink#", String.Format("{0}/my-account/reset-password/{1}", _baseUrl, token));
			replacements.Add("#ChangePasswordLink#", String.Format("{0}/my-account/change-password", _baseUrl));

			// Create the mail provider and send the message
			MailProvider mailProvider = new MailProvider();
			await mailProvider.SendMailMessage(MailTemplates.ForgottenPassword, replacements, to, null);

		}
		
		public async Task SendPasswordResetEmail(IdentityUser user)
		{
			// Create the tuple for the to override
			Tuple<string, string> to = new Tuple<string, string>(user.EmailAddress, user.FullName);

			// Create the replacements
			IDictionary<string, string> replacements = new Dictionary<string, string>();
			replacements.Add("#BaseUrl#", _baseUrl);
			replacements.Add("#FirstName#", user.FirstName);
			replacements.Add("#DateStamp#", DateTime.Now.ToString("HH:mm d MMMM, yyyy"));

			// Create the mail provider and send the message
			MailProvider mailProvider = new MailProvider();
			await mailProvider.SendMailMessage(MailTemplates.PasswordReset, replacements, to, null);

		}

		public async Task SendPasswordChangedEmail(IdentityUser user)
		{
			// Create the tuple for the to override
			Tuple<string, string> to = new Tuple<string, string>(user.EmailAddress, user.FullName);

			// Create the replacements
			IDictionary<string, string> replacements = new Dictionary<string, string>();
			replacements.Add("#BaseUrl#", _baseUrl);
			replacements.Add("#FirstName#", user.FirstName);
			replacements.Add("#DateStamp#", DateTime.Now.ToString("HH:mm d MMMM, yyyy"));

			// Create the mail provider and send the message
			MailProvider mailProvider = new MailProvider();
			await mailProvider.SendMailMessage(MailTemplates.PasswordChanged, replacements, to, null);

		}

		public async Task SendAccountEmailChangedEmail(IdentityUser user, string oldEmailAddress)
		{
			// Create the tuple for the to override
			Tuple<string, string> to = new Tuple<string, string>(user.EmailAddress, user.FullName);

			// Create the replacements
			IDictionary<string, string> replacements = new Dictionary<string, string>();
			replacements.Add("#BaseUrl#", _baseUrl);
			replacements.Add("#FirstName#", user.FirstName);
			replacements.Add("#NewEmailAddress#", user.EmailAddress);
			replacements.Add("#OldEmailAddress#", oldEmailAddress);
			replacements.Add("#DateStamp#", DateTime.Now.ToString("HH:mm d MMMM, yyyy"));

			// Create the mail provider and send the message
			MailProvider mailProvider = new MailProvider();
			await mailProvider.SendMailMessage(MailTemplates.EmailAddressChanged, replacements, to, null);

		}

	}
}
