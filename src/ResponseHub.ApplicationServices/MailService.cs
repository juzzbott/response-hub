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



		/// <summary>
		/// Sends the account activation email to the new user.
		/// </summary>
		/// <param name="newUser">The new user to send the account for.</param>
		/// <returns>Async task.</returns>
		public async Task SendAccountActivationEmail(IdentityUser newUser)
		{

			// Create the tuple for the to override
			Tuple<string, string> to = new Tuple<string, string>(newUser.EmailAddress, newUser.FullName);

			string baseSiteUrl = ConfigurationManager.AppSettings[ConfigurationKeys.BaseWebsiteUrl] ?? "";

			// Create the replacements
			IDictionary<string, string> replacements = new Dictionary<string, string>();
			replacements.Add("#FirstName#", newUser.FirstName);
			replacements.Add("#ActivationLink#", String.Format("{0}/my-account/activate/{1}", baseSiteUrl, newUser.ActivationCode.ToLower()));

			// Create the mail provider and send the message
			MailProvider mailProvider = new MailProvider();
			await mailProvider.SendMailMessage(MailTemplates.ActivateAccount, replacements, to, null);

		}

		public async Task SendGroupCreatedEmail(IdentityUser groupAdmin, string groupName, ServiceType service, string capcode)
		{
			// Create the tuple for the to override
			Tuple<string, string> to = new Tuple<string, string>(groupAdmin.EmailAddress, groupAdmin.FullName);

			// Create the replacements
			IDictionary<string, string> replacements = new Dictionary<string, string>();
			replacements.Add("#FirstName#", groupAdmin.FirstName);
			replacements.Add("#GroupName#", groupName);
			replacements.Add("#ServiceType#", service.GetEnumDescription());
			replacements.Add("#Capcode#", capcode);

			// Create the mail provider and send the message
			MailProvider mailProvider = new MailProvider();
			await mailProvider.SendMailMessage(MailTemplates.GroupCreated, replacements, to, null);

		}

	}
}
