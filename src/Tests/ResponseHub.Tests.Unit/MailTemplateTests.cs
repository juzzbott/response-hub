using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Mail;
using Enivate.ResponseHub.Mail.Configuration;

using Xunit;

namespace Enivate.ResponseHub.Tests.Unit
{
	public class MailTemplateTests
	{

		private readonly MailProvider _provider;

		private readonly string _baseTemplateDir;

		private readonly string _outputDir;

		private readonly string _baseUrl = "http://dev.responsehub.com.au";


		public MailTemplateTests()
		{
			_provider = new MailProvider();
			_baseTemplateDir = String.Format("{0}\\App_Data\\EmailTemplates", Environment.CurrentDirectory);
			_outputDir = String.Format("{0}\\EmailTemplateOutput", Environment.CurrentDirectory);
		}

		[Fact(DisplayName = "Can generate mail template - Account Activation")]
		[Trait("Category", "Mail Template Tests")]
		public void CanGenerate_MailTemplate_AccountActivation()
		{

			// Create the mail config object
			MailTemplateElement mailConfig = new MailTemplateElement()
			{
				Name = "ActivateAccount",
				Subject = "Activate your ResponseHub account",
				BaseTemplateFile = "base_template.html",
				TemplateFile = "activate_account.html"
			};

			// Create the replacements
			IDictionary<string, string> replacements = new Dictionary<string, string>();
			replacements.Add("#FirstName#", "Test");
			replacements.Add("#ActivationLink#", String.Format("{0}/my-account/activate/{1}", _baseUrl, "lkjsdf8asdfjhasdf8sdafhasdjfh8"));

			// Generate the mail template 
			string template = _provider.GetMailBodyContent(mailConfig, replacements, _baseTemplateDir);

			// Write the template to the output dir
			WriteTemplateFile(mailConfig.Name, template);

		}

		[Fact(DisplayName = "Can generate mail template - Forgotten Password")]
		[Trait("Category", "Mail Template Tests")]
		public void CanGenerate_MailTemplate_ForgottenPassword()
		{

			// Create the mail config object
			MailTemplateElement mailConfig = new MailTemplateElement()
			{
				Name = "ForgottenPassword",
				Subject = "Activate your ResponseHub account",
				BaseTemplateFile = "base_template.html",
				TemplateFile = "forgotten_password.html"
			};

			// Create the replacements
			IDictionary<string, string> replacements = new Dictionary<string, string>();
			replacements.Add("#FirstName#", "Test");
			replacements.Add("#ResetPasswordLink#", String.Format("{0}/my-account/reset-password/{1}", _baseUrl, "ksdjfhsadf7asdf98s7dfas7df8as7df89sdf7"));
			replacements.Add("#ChangePasswordLink#", String.Format("{0}/my-account/change-password", _baseUrl));

			// Generate the mail template 
			string template = _provider.GetMailBodyContent(mailConfig, replacements, _baseTemplateDir);

			// Write the template to the output dir
			WriteTemplateFile(mailConfig.Name, template);

		}

		/// <summary>
		/// Writes the template file to disk as a html file.
		/// </summary>
		/// <param name="template"></param>
		private void WriteTemplateFile(string templateName, string template)
		{

			// Ensure the output directory exists
			if (!Directory.Exists(_outputDir))
			{
				Directory.CreateDirectory(_outputDir);
			}

			// Write the template file to disk
			using (StreamWriter writer = new StreamWriter(String.Format("{0}\\{1}.html", _outputDir, templateName), false))
			{
				writer.Write(template);
			}
		}
	}
}
