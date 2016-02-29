using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Mail.Configuration
{
	public class MailTemplatesConfigurationSection : ConfigurationSection
	{

		private const string AddItemNameKey = "mailTemplate";
		private const string DefaultToKey = "defaultTo";
		private const string DefaultFromKey = "defaultFrom";
		private const string TemplatesDirectoryKey = "templatesDir";

		[ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
		[ConfigurationCollection(typeof(MailTemplateCollection), AddItemName = AddItemNameKey)]
		public MailTemplateCollection MailTemplates
		{
			get
			{
				return (MailTemplateCollection)this[""];
			}
			set
			{
				this[""] = value;
			}
		}

		[ConfigurationProperty(DefaultToKey, IsRequired = true)]
		public string DefaultTo
		{
			get
			{
				return (string)base[DefaultToKey];
			}
			set
			{
				base[DefaultToKey] = value;
			}
		}

		[ConfigurationProperty(DefaultFromKey, IsRequired = true)]
		public string DefaultFrom
		{
			get
			{
				return (string)base[DefaultFromKey];
			}
			set
			{
				base[DefaultFromKey] = value;
			}
		}

		[ConfigurationProperty(TemplatesDirectoryKey, IsRequired = true)]
		public string TemplatesDirectory
		{
			get
			{
				return (string)base[TemplatesDirectoryKey];
			}
			set
			{
				base[TemplatesDirectoryKey] = value;
			}
		}

	}
}
