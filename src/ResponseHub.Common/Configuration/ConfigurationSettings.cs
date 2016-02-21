using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Common.Configuration
{
	public class ConfigurationSettings
	{

		private const string WarningSectionKey = "warnings";

		/// <summary>
		/// References the current logging configuration section.
		/// </summary>
		public static WarningsConfigurationSection Warnings
		{
			get
			{
				return (WarningsConfigurationSection)ConfigurationManager.GetSection(WarningSectionKey);
			}
		}

	}
}
