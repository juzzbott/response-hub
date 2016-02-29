using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Mail.Configuration
{
	public class MailTemplateElement : ConfigurationElement
	{

		private const string NameKey = "name";
		private const string ToKey = "to";
		private const string FromKey = "from";
		private const string SubjectKey = "subject";
		private const string BaseTemplateFileKey = "baseTemplateFile";
		private const string TemplateFileKey = "templateFile";

		[ConfigurationProperty(NameKey, IsKey = true, IsRequired = true)]
		public string Name
		{
			get
			{
				return (string)base[NameKey];
			}
			set
			{
				base[NameKey] = value;
			}
		}

		[ConfigurationProperty(ToKey, IsRequired = false)]
		public string To
		{
			get
			{
				return (string)base[ToKey];
			}
			set
			{
				base[ToKey] = value;
			}
		}

		[ConfigurationProperty(FromKey, IsRequired = false)]
		public string From
		{
			get
			{
				return (string)base[FromKey];
			}
			set
			{
				base[FromKey] = value;
			}
		}



		[ConfigurationProperty(SubjectKey, IsRequired = true)]
		public string Subject
		{
			get
			{
				return (string)base[SubjectKey];
			}
			set
			{
				base[SubjectKey] = value;
			}
		}

		[ConfigurationProperty(BaseTemplateFileKey, IsRequired = false)]
		public string BaseTemplateFile
		{
			get
			{
				return (string)base[BaseTemplateFileKey];
			}
			set
			{
				base[BaseTemplateFileKey] = value;
			}
		}

		[ConfigurationProperty(TemplateFileKey, IsRequired = true)]
		public string TemplateFile
		{
			get
			{
				return (string)base[TemplateFileKey];
			}
			set
			{
				base[TemplateFileKey] = value;
			}
		}



	}
}
