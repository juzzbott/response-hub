using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Logging.Configuration
{
	public class LoggingConfiguration
	{

		/// <summary>
		/// References the current logging configuration section.
		/// </summary>
		public static LoggingConfigurationSection Current
		{
			get
			{
				return (LoggingConfigurationSection)ConfigurationManager.GetSection("logging");
			}
		}

	}
}
