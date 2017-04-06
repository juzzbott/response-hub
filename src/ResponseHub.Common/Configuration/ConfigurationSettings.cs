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
		private const string WeatherDataSectionKey = "weatherData";

		/// <summary>
		/// References the current warnings configuration section.
		/// </summary>
		public static WarningsConfigurationSection Warnings
		{
			get
			{
				return (WarningsConfigurationSection)ConfigurationManager.GetSection(WarningSectionKey);
			}
		}

		/// <summary>
		/// References the current weather data configuration section.
		/// </summary>
		public static WeatherDataConfigurationSection WeatherData
		{
			get
			{
				return (WeatherDataConfigurationSection)ConfigurationManager.GetSection(WeatherDataSectionKey);
			}
		}

	}
}
