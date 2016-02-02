using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Logging.Configuration
{
	public class LoggingConfigurationSection : ConfigurationSection
	{

		private const string LogDirectoryKey = "logDirectory";
		private const string LogLevelKey = "logLevel";

		/// <summary>
		/// Specifies the directory that log files will be written to.
		/// </summary>
		[ConfigurationProperty(LogDirectoryKey, IsRequired = true)]
		public string LogDirectory
		{
			get { return (string)this[LogDirectoryKey]; }
			set { this[LogDirectoryKey] = value; }
		}

		/// <summary>
		/// Specifies the minimum level of log entries that will be written. 
		/// Values can be DEBUG|INFO|WARN|ERROR
		/// </summary>
		[ConfigurationProperty(LogLevelKey, IsRequired = false, DefaultValue = "INFO")]
		public string LogLevel
		{
			get { return (string)this[LogLevelKey]; }
			set { this[LogLevelKey] = value; }
		}

	}
}
