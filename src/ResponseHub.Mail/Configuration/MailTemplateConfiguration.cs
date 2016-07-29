using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Mail.Configuration
{
	public class MailTemplateConfiguration
	{

		/// <summary>
		/// References the current logging configuration section.
		/// </summary>
		public static MailTemplatesConfigurationSection Current
		{
			get
			{
				return (MailTemplatesConfigurationSection)ConfigurationManager.GetSection("mailTemplates");
			}
		}

	}
}
