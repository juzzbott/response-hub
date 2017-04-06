using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Common.Configuration;
using Enivate.ResponseHub.Model.WeatherData.Interface;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.WebTasks
{
	class Program
	{
		
		private const string ApplicationNameKey = "ApplicationName";

		protected static IWeatherDataService WeatherDataService;

		static void Main(string[] args)
		{

			try
			{
				// Unity configuration loader
				UnityConfiguration.Container = new UnityContainer().LoadConfiguration();
			}
			catch (Exception ex)
			{
				LogUnityException(ex);
				return;
			}

			// Load the classes from unity
			WeatherDataService = ServiceLocator.Get<IWeatherDataService>();

			// If there is no arguments, show the warning
			if (args.Length == 0)
			{
				Console.WriteLine("You must specify options to perform the tasks:");
				Console.WriteLine("\t-bom - Run the caching of BoM files.");
				return;
			}

			if (args.Contains("-bom"))
			{
				// Download the files required
				DownloadBomFilesForLocations();

				// Remove any files that are older than the configured expiry time
				DeleteExpiredBomFiles();
			}

		}

		#region Task Methods

		private static void DownloadBomFilesForLocations()
		{
			// Loop through each of the locations in the configuration
			for (int i = 0; i < ResponseHubSettings.WeatherData.Locations.Count; i++)
			{
				// Get the location info
				WeatherLocationElement location = ResponseHubSettings.WeatherData.Locations[i];

				// Download the file list for the location
				WeatherDataService.DownloadImageFileFromFtp("", "");



			}
		}

		private static void DeleteExpiredBomFiles()
		{

		}

		#endregion

		#region Helpers



		/// <summary>
		/// Writes the unity resolution issues to the event log.
		/// </summary>
		/// <param name="ex"></param>
		private static void LogUnityException(Exception ex)
		{
			string source = ConfigurationManager.AppSettings[ApplicationNameKey];
			string log = "Application";
			string eventDesc = String.Format("Failure loading unity configurations. Message: {0}\r\n{1}", ex.Message, ex.StackTrace);

			// If the event source doesn't exist, create it
			if (!EventLog.SourceExists(source))
			{
				EventLog.CreateEventSource(source, log);
			}

			// Write the log entry
			EventLog.WriteEntry(source, eventDesc, EventLogEntryType.Error, 0001);

		}

		#endregion
	}
}
