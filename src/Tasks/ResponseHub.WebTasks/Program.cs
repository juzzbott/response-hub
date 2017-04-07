using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Common.Configuration;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.WeatherData.Interface;

using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace Enivate.ResponseHub.WebTasks
{
	class Program
	{
		
		private const string ApplicationNameKey = "ApplicationName";

		protected static IWeatherDataService WeatherDataService;
		protected static ILogger Log;

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
			Log = ServiceLocator.Get<ILogger>();

			// If there is no arguments, show the warning
			if (args.Length == 0)
			{
				Console.WriteLine("You must specify options to perform the tasks:");
				Console.WriteLine("\t-bom - Run the caching of BoM files.");
				return;
			}

			if (args.Contains("-bom"))
			{

				Log.Info("Executing task: Download BoM cache files.");

				// Remove any files that are older than the configured expiry time
				DeleteExpiredBomFiles();

				// Download the files required
				DownloadBomFilesForLocations();
			}

		}

		#region Task Methods

		#region BoM cache downloads

		/// <summary>
		/// Download the bom file for the locations configured.
		/// </summary>
		private static void DownloadBomFilesForLocations()
		{
			// Loop through each of the locations in the configuration
			for (int i = 0; i < ResponseHubSettings.WeatherData.Locations.Count; i++)
			{
				// Get the location info
				WeatherLocationElement location = ResponseHubSettings.WeatherData.Locations[i];

				Log.Debug(String.Format("Downloading cache files for location: {0}", location.Code));

				// Download the rain and wind radar images
				DownloadRadarImages(location.WindRadarProductId, location.Code);
				DownloadRadarImages(location.RainRadarProductId, location.Code);

				// Download the observation data
				try
				{
					Log.Debug(String.Format("Downloading observation data: {0}", location.ObservationId));
					WeatherDataService.DownloadObservationData(location.ObservationId, location.Code);
					Log.Debug(String.Format("Completed downloading observation data: {0}\r\n\r\n", location.ObservationId));
				}
				catch (Exception ex)
				{
					Log.Error(String.Format("Error downloading observation data. Message: {0}", ex.Message), ex);
				}

			}
		}

		/// <summary>
		/// Downloads the cache file list and image files for the specific product id and location code
		/// </summary>
		/// <param name="productId"></param>
		/// <param name="locationCode"></param>
		private static void DownloadRadarImages(string productId, string locationCode)
		{ 

			// Store the radar image files.
			IList<string> radarImageFiles = new List<string>();

			try
			{

				// Get the radar images for the for the specific product
				Log.Debug(String.Format("Downloading image file list: {0}", productId));
				radarImageFiles = WeatherDataService.GetRadarImagesForProduct(productId, locationCode);
				Log.Debug(String.Format("Completed downloading image file list: {0}", productId));
			}
			catch (Exception ex)
			{
				Log.Error(String.Format("Error downloading radar image list data. Message: {0}", ex.Message), ex);
			}

			// Now that we have the list of images, go ahead and download it
			foreach (string imageFileName in radarImageFiles)
			{
				try
				{ 
					// Download the radar image file
					Log.Debug(String.Format("Downloading image file: {0}", imageFileName));
					WeatherDataService.DownloadImageFileFromFtp(imageFileName, locationCode);
					Log.Debug(String.Format("Completed downloading image file: {0}", imageFileName));
				}
				catch (Exception ex)
				{
					Log.Error(String.Format("Error downloading radar image. Message: {0}", ex.Message), ex);
				}
			}
		}

		/// <summary>
		/// Delete the expired cache files.
		/// </summary>
		private static void DeleteExpiredBomFiles()
		{

			// Get the cache duration
			TimeSpan cacheDuration = TimeSpan.Parse(ConfigurationManager.AppSettings["BoMCacheFilesTimeToLive"]);

			// Loop through each of the locations in the configuration
			for (int i = 0; i < ResponseHubSettings.WeatherData.Locations.Count; i++)
			{
				// Get the location info
				WeatherLocationElement location = ResponseHubSettings.WeatherData.Locations[i];

				Log.Debug(String.Format("Deleting expired cache files for location: {0}", location.Code));

				// Get the cache directory based on the location
				string cacheDir = WeatherDataService.GetCacheDirectory(location.Code);

				// If the cache dir doesn't exist, we don't have anything to delete, so just move on
				if (!Directory.Exists(cacheDir))
				{
					continue;
				}

				// Count the files deleted
				int filesDeleted = 0;

				// Loop through the files in the location cache dir
				foreach(string file in Directory.GetFiles(cacheDir))
				{

					Log.Debug(String.Format("Checking cache file to delete: {0}", file));

					// Ensure the file exists
					if (File.Exists(file))
					{

						// Get the date time the file was created
						DateTime createdUtc = File.GetCreationTimeUtc(file);

						// If the current datetime is less than or equal to the file creation time + cache duration, cache is valid
						// If not, delete the cache file
						if (DateTime.UtcNow > createdUtc.Add(cacheDuration))
						{
							try
							{ 
								File.Delete(file);
								Log.Debug(String.Format("Cache file deleted: {0}", file));
								filesDeleted++;
							}
							catch (Exception ex)
							{
								Log.Error(String.Format("Error deleting cache radar image. Message: {0}", ex.Message), ex);
							}

						}

					}
				}

				Log.Debug(String.Format("Deleted {0} expired cache files\r\n\r\n", filesDeleted));

			}
		}

		#endregion

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
