using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

using Enivate.ResponseHub.Mail.Configuration;

namespace Enivate.ResponseHub.Mail
{
    public class MailProvider
    {

		/// <summary>
		/// Sends the mail message with the content and settings defined in the mail template.
		/// </summary>
		/// <param name="mailConfig">The mail configuration object.</param>
		/// <param name="replacements">The dictionary of replacement strings used to replace parameters in the message body.</param>
		public async Task SendMailMessage(string mailTemplateName, IDictionary<string, string> replacements, Tuple<string, string> toOverride, Tuple<string, string> fromOverride)
		{

			// If the mail configuration null, then throw exception
			if (String.IsNullOrEmpty(mailTemplateName))
			{
				throw new ArgumentNullException("mailTemplate", "The 'mailTemplate' parameter cannot be null or empty.");
			}

			// Get the mail configuration object based on the name
			MailTemplateElement mailConfig = MailTemplateConfiguration.Current.MailTemplates[mailTemplateName];

			// If the mail config is null, then throw exception
			if (mailConfig == null)
			{
				throw new ApplicationException(String.Format("The mail template '{0}' cannot be found in the configuration.", mailTemplateName));
			}

			// If the mailConfig from address is null or empty, revert back to the default
			string rawFromAddress = (!String.IsNullOrEmpty(mailConfig.From) ? mailConfig.From : MailTemplateConfiguration.Current.DefaultFrom);
			string rawToAddress = (!String.IsNullOrEmpty(mailConfig.To) ? mailConfig.From : MailTemplateConfiguration.Current.DefaultTo);

			// Create the from and to MailAddress objects
			MailAddress from = GetMailAddress(rawFromAddress);
			MailAddressCollection toCollection = GetMailAddressCollection(rawToAddress);

			// if there is a from override, then use that
			if (fromOverride != null)
			{
				from = new MailAddress(fromOverride.Item1, fromOverride.Item2);
			}

			// If there is a To override, then use that
			if (toOverride != null)
			{
				MailAddress to = new MailAddress(toOverride.Item1, toOverride.Item2);
				toCollection = new MailAddressCollection();
				toCollection.Add(to);
			}

			MailMessage msg = new MailMessage();
			foreach(MailAddress to in toCollection)
			{
				msg.To.Add(to);
			}
			msg.From = from;
			msg.ReplyToList.Add(from);
			msg.Subject = mailConfig.Subject;
			msg.IsBodyHtml = true;

			// get the mail body
			string msgBody = GetMailBodyContent(mailConfig, replacements);

			// set the message body to the mail message
			msg.Body = GetMailBodyContent(mailConfig, replacements);

			// Create the mail client and send the message
			SmtpClient client = new SmtpClient();
			await client.SendMailAsync(msg);

		}

		#region Mail Address Helpers

		/// <summary>
		/// Gets the MailAddressCollection from the raw mail string.
		/// </summary>
		/// <param name="mailAddresses">The list of mail addresses in the raw string.</param>
		/// <returns>The mail address collection</returns>
		public MailAddressCollection GetMailAddressCollection(string mailAddresses)
		{

			// Create the collection
			MailAddressCollection collection = new MailAddressCollection();

			// Split based on commas
			foreach(string mailAddress in mailAddresses.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				collection.Add(GetMailAddress(mailAddresses));
			}

			// return the collection
			return collection;

		}

		/// <summary>
		/// Gets the MailAddress from the raw mail string.
		/// </summary>
		/// <param name="mailAddress">The raw mail address.</param>
		/// <returns></returns>
		public MailAddress GetMailAddress(string mailAddress)
		{

			string name = "";
			string emailAddress = "";

			// Match the string for "Email Name <Email Address>". If that doesn't match, then ensure it's just the email address with an '@' simple. Super basic email validation.
			Match mailMatch = Regex.Match(mailAddress, "^(.*)\\s*\\[([^\\]]*)\\]$");
			if (mailMatch.Success && mailMatch.Groups.Count == 3)
			{
				name = mailMatch.Groups[1].Value.Trim();
				emailAddress = mailMatch.Groups[2].Value.Trim();
			}
			else if (mailAddress.Contains("@"))
			{
				emailAddress = mailAddress.Trim();
			}

			// If there is no email address, throw exception
			if (String.IsNullOrEmpty(emailAddress))
			{
				throw new ApplicationException(String.Format("No email address found in mail address: {0}", mailAddress));
			}

			// Return the mail address.
			return new MailAddress(emailAddress, name);
		}

		#endregion

		#region GetBodyContent

		/// <summary>
		/// Gets the mail message body template from the template files, and applies the replacements on top.
		/// </summary>
		/// <param name="mailConfig"></param>
		/// <param name="replacements"></param>
		/// <returns></returns>
		private string GetMailBodyContent(MailTemplateElement mailConfig, IDictionary<string, string> replacements)
		{

			string msgBody = "";

			// Get the base template content.
			string baseTemplateContent = File.ReadAllText(GetTemplateFilePath(mailConfig.BaseTemplateFile));
			string templateContent = File.ReadAllText(GetTemplateFilePath(mailConfig.TemplateFile));

			// If the base template file is not null or empty, then use that and include the individual email template ontop, otherwise just revert to the mail template.
			if (!String.IsNullOrEmpty(baseTemplateContent))
			{
				// Set the base template
				msgBody = baseTemplateContent;

				// Apply the actual mal template ontop
				msgBody = msgBody.Replace("##TEMPLATE##", templateContent);

			}
			else
			{
				// Just use the single mail template
				msgBody = templateContent;
			}

			// If there are replacements, then iterate through each one and replace the tokens with the values
			if (replacements != null && replacements.Count > 0)
			{
				foreach (KeyValuePair<string, string> replacement in replacements)
				{
					msgBody = msgBody.Replace(replacement.Key, replacement.Value);
				}
			}

			// return the msg body
			return msgBody;

		}

		/// <summary>
		/// Gets the full path to the template file.
		/// </summary>
		/// <param name="templateFile"></param>
		/// <returns></returns>
		private string GetTemplateFilePath(string templateFile)
		{

			// Get the templates directory
			string baseDir = MailTemplateConfiguration.Current.TemplatesDirectory;

			// If the http context exists, and it's a virtual path, map it
			if (HttpContext.Current != null && baseDir[0] == '~')
			{
				baseDir = HttpContext.Current.Server.MapPath(baseDir);
			}

			// If the path starts with a . then append it to the current app working directory
			if (templateFile[0] == '.')
			{
				baseDir = String.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory, templateFile);
			}

			// Now we need to append the template file to the templaes directory
			return String.Format("{0}{1}{2}", baseDir, (!baseDir.EndsWith("\\") ? "\\" : ""), templateFile);
		}

		#endregion

	}
}
