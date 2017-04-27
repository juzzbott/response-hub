using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Common.Configuration;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.WeatherData.Interface;

namespace Enivate.ResponseHub.WebTasks.ApplicationServices
{
	public class BomCacheManager
	{

		private ILogger _log;
		private IWeatherDataService _weatherDataService;

		public BomCacheManager(IWeatherDataService weatherDataService, ILogger log)
		{
			_weatherDataService = weatherDataService;
			_log = log;
		}

		/// <summary>
		/// Download the bom file for the locations configured.
		/// </summary>
		public void DownloadBomFilesForLocations()
		{
			// Loop through each of the locations in the configuration
			for (int i = 0; i < ResponseHubSettings.WeatherData.Locations.Count; i++)
			{
				// Get the location info
				WeatherLocationElement location = ResponseHubSettings.WeatherData.Locations[i];

				_log.Debug(String.Format("Downloading cache files for location: {0}", location.Code));

				// Download the rain and wind radar images
				DownloadRadarImages(location.WindRadarProductId, location.Code);
				DownloadRadarImages(location.RainRadarProductId, location.Code);

				// Download the observation data
				try
				{
					_log.Debug(String.Format("Downloading observation data: {0}", location.ObservationId));
					_weatherDataService.GetObservationData(location.ObservationId, location.Code);
					_log.Debug(String.Format("Completed downloading observation data: {0}\r\n\r\n", location.ObservationId));
				}
				catch (Exception ex)
				{
					_log.Error(String.Format("Error downloading observation data. Message: {0}", ex.Message), ex);
				}

			}
		}

		/// <summary>
		/// Downloads the cache file list and image files for the specific product id and location code
		/// </summary>
		/// <param name="productId"></param>
		/// <param name="locationCode"></param>
		private void DownloadRadarImages(string productId, string locationCode)
		{

			// Store the radar image files.
			IList<string> radarImageFiles = new List<string>();

			try
			{

				// Get the radar images for the for the specific product
				_log.Debug(String.Format("Downloading image file list: {0}", productId));
				radarImageFiles = _weatherDataService.GetRadarImagesForProduct(productId, locationCode);
				_log.Debug(String.Format("Completed downloading image file list: {0}", productId));
			}
			catch (Exception ex)
			{
				_log.Error(String.Format("Error downloading radar image list data. Message: {0}", ex.Message), ex);
			}

			// Now that we have the list of images, go ahead and download it
			foreach (string imageFileName in radarImageFiles)
			{
				try
				{
					// Download the radar image file
					_log.Debug(String.Format("Downloading image file: {0}", imageFileName));
					_weatherDataService.DownloadImageFileFromFtp(imageFileName, locationCode);
					_log.Debug(String.Format("Completed downloading image file: {0}", imageFileName));
				}
				catch (Exception ex)
				{
					_log.Error(String.Format("Error downloading radar image. Message: {0}", ex.Message), ex);
				}
			}
		}

		/// <summary>
		/// Delete the expired cache files.
		/// </summary>
		public void DeleteExpiredBomFiles()
		{

			// Get the cache duration
			TimeSpan cacheDuration = TimeSpan.Parse(ConfigurationManager.AppSettings["BoMCacheFilesTimeToLive"]);

			// Loop through each of the locations in the configuration
			for (int i = 0; i < ResponseHubSettings.WeatherData.Locations.Count; i++)
			{
				// Get the location info
				WeatherLocationElement location = ResponseHubSettings.WeatherData.Locations[i];

				_log.Debug(String.Format("Deleting expired cache files for location: {0}", location.Code));

				// Get the cache directory based on the location
				string cacheDir = _weatherDataService.GetCacheDirectory(location.Code);

				// If the cache dir doesn't exist, we don't have anything to delete, so just move on
				if (!Directory.Exists(cacheDir))
				{
					continue;
				}

				// Count the files deleted
				int filesDeleted = 0;

				// Loop through the files in the location cache dir
				foreach (string file in Directory.GetFiles(cacheDir))
				{

					_log.Debug(String.Format("Checking cache file to delete: {0}", file));

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
								_log.Debug(String.Format("Cache file deleted: {0}", file));
								filesDeleted++;
							}
							catch (Exception ex)
							{
								_log.Error(String.Format("Error deleting cache radar image. Message: {0}", ex.Message), ex);
							}

						}

					}
				}

				_log.Debug(String.Format("Deleted {0} expired cache files\r\n\r\n", filesDeleted));

			}
		}

	}
}
