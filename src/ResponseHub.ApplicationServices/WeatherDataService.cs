using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.WeatherData.Interface;
using Enivate.ResponseHub.Common.Configuration;
using Enivate.ResponseHub.Model.WeatherData;

using Newtonsoft.Json.Linq;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class WeatherDataService : IWeatherDataService
	{

		ILogger _log;

		public WeatherDataService(ILogger log)
		{
			_log = log;
		}

		/// <summary>
		/// Gets the radar image urls for the specific product id from the BoM public FTP.
		/// </summary>
		/// <param name="productId"></param>
		/// <returns></returns>
		public IList<string> GetRadarImagesForProduct(string productId, string locationCode)
		{

			// Create the list to store the image files.
			List<string> imageFiles = new List<String>();

			try
			{

				// Get the filename of what the cache file would be based on the source type.
				string cacheFileName = GetImageListCacheFilename(productId, locationCode);

				// Determine if there is a valid cache file, and it's not expired.
				bool cacheValid = IsCacheFileValid(cacheFileName);

				if (!cacheValid)
				{
					// Cache file is not valid, so download it
					DownloadRadarImageFileListFromFtp(productId, locationCode);
				}

				// Get the image files for the product from the cache file.
				imageFiles.AddRange(GetRadarImageImageFilesFromCache(cacheFileName));
					

			}
			catch (Exception ex)
			{
				// Log the exception
				_log.Error("Unable to get FTP file list from BoM FTP. Message: " + ex.Message, ex);
			}

			return imageFiles;
		}

		/// <summary>
		/// Returns the bytes for the radar image. If the file does not exist in the local cache, it's downloaded from BoM.
		/// </summary>
		/// <param name="radarImageFilename">The filename of the radar image to download.</param>
		/// <returns>The file bytes.</returns>
		public byte[] GetRadarImageBytes(string radarImageFilename)
		{

			// Get the product id from the radar image filename
			string productId = radarImageFilename.Substring(0, radarImageFilename.IndexOf('_'));

			// Find the location info based on the radar code
			WeatherLocationElement locationElement = null;
			foreach (WeatherLocationElement element in ResponseHubSettings.WeatherData.Locations)
			{
				if (element.RainRadarProductId.Equals(productId, StringComparison.CurrentCultureIgnoreCase) || element.WindRadarProductId.Equals(productId, StringComparison.CurrentCultureIgnoreCase))
				{
					locationElement = element;
					break;
				}
			}

			// Get the full cache image file path
			string fullCacheImagePath = GetRadarImageCacheFilename(radarImageFilename, locationElement.Code);

			// If the file does not exist, download it from the BoM FTP server
			if (!File.Exists(fullCacheImagePath))
			{
				// Download the image file from the ftp location
				DownloadImageFileFromFtp(radarImageFilename, locationElement.Code);
			}

			// load the bytes of the file into a byte array and return it
			byte[] fileBytes = File.ReadAllBytes(fullCacheImagePath);

			// return the byte array
			return fileBytes;

		}

		/// <summary>
		/// Download the image file from the ftp location
		/// </summary>
		/// <param name="imageFilename"></param>
		/// <param name="cacheFilename"></param>
		public void DownloadImageFileFromFtp(string imageFilename, string locationCode)
		{
			// Get the ftp locaion from the configuration
			string ftpFileLocation = ResponseHubSettings.WeatherData.RadarFtpLocation;
			string ftpFilename = String.Format("{0}{1}", ftpFileLocation, String.Format("{0}.png", imageFilename.Replace("_", ".")));

			// Get the full cache image file path
			string fullCacheImagePath = GetRadarImageCacheFilename(imageFilename, locationCode);

			// If the cache image file already exists, don't download it
			if (File.Exists(fullCacheImagePath))
			{
				_log.Debug(String.Format("Skipping downloading image file '{0}'. File already exists in cache.", ftpFilename));
				return;
			}

			FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(ftpFilename);
			request.Method = WebRequestMethods.Ftp.DownloadFile;
			request.Credentials = new NetworkCredential("anonymous", "anonymous");
			request.UseBinary = true;

			// Get the response
			FtpWebResponse response = (FtpWebResponse)request.GetResponse();

			using (Stream responseStream = response.GetResponseStream())
			{

				using (FileStream writer = new FileStream(fullCacheImagePath, FileMode.Create))
				{

					long length = response.ContentLength;
					int bufferSize = 2048;
					int readCount;
					byte[] buffer = new byte[2048];

					readCount = responseStream.Read(buffer, 0, bufferSize);
					while (readCount > 0)
					{
						writer.Write(buffer, 0, readCount);
						readCount = responseStream.Read(buffer, 0, bufferSize);
					}
				}
			}
		}

		/// <summary>
		/// Gets the observation data based on the observation id and the location code.
		/// </summary>
		/// <param name="observationId"></param>
		/// <param name="locationCode"></param>
		/// <returns></returns>
		public IList<ObservationData> GetObservationData(string observationId, string locationCode)
		{
			// Get the cache directory and filename
			string cacheDirectory = GetCacheDirectory(locationCode);
			string filename = String.Format("{0}\\{1}.json", cacheDirectory, observationId);

			// If the file does not exist, then download it
			if (!IsCacheFileValid(filename))
			{
				// Download the observation code
				DownloadObservationData(observationId, locationCode);
			}

			// return the json data
			string jsonData = File.ReadAllText(filename);

			// Map the jsonData to observation data items
			IList<ObservationData> observationData = MapJsonToObservationData(jsonData);
			return observationData;
		}

		/// <summary>
		/// Download the json observation data.
		/// </summary>
		/// <param name="observationId">The observation code to download.</param>
		/// <param name="locationCode">The location code for the observation data.</param>
		public void DownloadObservationData(string observationId, string locationCode)
		{

			// Get the ftp locaion from the configuration
			string observationLocation = ResponseHubSettings.WeatherData.ObservationLocation;
			string productId = observationId.Substring(0, observationId.IndexOf('.'));
			string requestUrl = String.Format("{0}/{1}/{2}.json", observationLocation, productId, observationId);

			// Create the web request
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUrl);

			// Get the response
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			
			// Ensure it's a valid response
			if (response.StatusCode == HttpStatusCode.OK)
			{

				// Store the json data
				string jsonData = "";
				using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				{
					jsonData = reader.ReadToEnd();
				}

				// Write the json file
				string cacheDirectory = GetCacheDirectory(locationCode);
				string filename = String.Format("{0}\\{1}.json", cacheDirectory, observationId);
				using (StreamWriter writer = new StreamWriter(filename, false))
				{
					writer.Write(jsonData);
				}
			}


		}

		/// <summary>
		/// Get cache file contents from the BoM FTP location, and returns the radar images as a list of filenames.
		/// </summary>
		/// <param name="productId">The product id to get the image files for.</param>
		/// <returns>The list of image filenames.</returns>
		public void DownloadRadarImageFileListFromFtp(string productId, string locationCode)
		{

			// Get the filename of what the cache file would be based on the source type.
			string cacheFileName = GetImageListCacheFilename(productId, locationCode);

			// Ensure the cache directory exists
			EnsureCacheDirectoryExists(locationCode);

			// Get the ftp locaion from the configuration
			string ftpLocation = ResponseHubSettings.WeatherData.RadarFtpLocation;

			FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(ftpLocation);
			request.Method = WebRequestMethods.Ftp.ListDirectory;
			request.Credentials = new NetworkCredential("anonymous", "anonymous");

			// Get the response
			FtpWebResponse response = (FtpWebResponse)request.GetResponse();

			// Read the details of the response into a string
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
			{
				using (StreamWriter writer = new StreamWriter(cacheFileName, false))
				{
					// While there is still data to read
					while (reader.Peek() >= 0)
					{
						// Read the filename from the line
						string filename = reader.ReadLine();

						// If the filename contains the product id, then add to the list, otherwise just continue to next
						if (filename.ToUpper().Contains(productId.ToUpper() + ".T."))
						{
							// Strip the extension and replace . with _ so it can be used in our routes.
							string actualFiename = filename.Replace(".png", "").Replace(".", "_");
							writer.WriteLine(actualFiename);
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets the cache directory for the current 
		/// </summary>
		/// <param name="locationCode"></param>
		/// <returns></returns>
		public string GetCacheDirectory(string locationCode)
		{
			// Get the cache directory
			string cacheDirectory = ResponseHubSettings.WeatherData.RadarCacheDirectory;

			// If the http context exists, use the map path, otherwise use the standard file path mapping
			if (HttpContext.Current != null)
			{
				cacheDirectory = HttpContext.Current.Server.MapPath(cacheDirectory);
			}
			else
			{
				cacheDirectory = Path.GetFullPath(cacheDirectory);
			}

			// Now we need to prepend the cache cacheDirectory onto the cacheFile and return it
			cacheDirectory = String.Format("{0}{1}{2}", cacheDirectory, (cacheDirectory.EndsWith("\\") ? "" : "\\"), locationCode);
			return cacheDirectory;
		}

		#region Helpers

		/// <summary>
		/// Gets the cache location to the radar image.
		/// </summary>
		/// <param name="imageFilename">The filename of the image in the cache file.</param>
		/// <param name="locationCode">The location code to get the files for.</param>
		/// <returns>The full path to the cache image file.</returns>
		private string GetRadarImageCacheFilename(string imageFilename, string locationCode)
		{
			// Get the radar image cache directory and full filename
			string cacheDirectory = GetCacheDirectory(locationCode);
			imageFilename = String.Format("{0}.png", imageFilename.Replace("_", "."));
			return String.Format("{0}\\{1}", cacheDirectory, imageFilename);
		}

		/// <summary>
		/// Gets the list of images from the cache file.
		/// </summary>
		/// <param name="cacheFileName">The filename of the cache file.</param>
		/// <returns>The list of image filenames from cache file.</returns>
		private IList<string> GetRadarImageImageFilesFromCache(string cacheFileName)
		{

			// Create the list of filenames
			IList<string> filenames = new List<string>();

			/// Read the filename list from the cache file into the list of filenames.
			using (StreamReader reader = new StreamReader(cacheFileName))
			{
				while (reader.Peek() >= 0)
				{
					filenames.Add(reader.ReadLine());
				}
			}

			// return the list of filenames
			return filenames;
		}

		/// <summary>
		/// Maps the Json observation data to a list of observation data items.
		/// </summary>
		/// <param name="jsonData"></param>
		/// <returns></returns>
		private IList<ObservationData> MapJsonToObservationData(string jsonData)
		{

			// Create the list of observation details
			IList<ObservationData> observationList = new List<ObservationData>();

			// Load the geocode data into JObject for querying
			JObject observationJson = JObject.Parse(jsonData);

			// Loop through the array of data items
			JArray dataItems = (JArray)observationJson["observations"]["data"];

			// Set the observation objects.
			foreach (JObject dataItem in dataItems)
			{
				// Map the observation data
				ObservationData observationItem = new ObservationData();
				observationItem.Name = dataItem["name"].ToString();
				observationItem.Cloud = dataItem["cloud"].ToString();
				observationItem.Latitude = GetObservationDataItemAsDouble(dataItem["lat"]);
				observationItem.Longitude = GetObservationDataItemAsDouble(dataItem["lon"]);
				observationItem.LocalTime = DateTime.ParseExact(dataItem["local_date_time_full"].ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
				observationItem.Pressure = GetObservationDataItemAsDouble(dataItem["press"]);
				observationItem.ProductId = dataItem["history_product"].ToString();
				observationItem.RainTrace = GetObservationDataItemAsDouble(dataItem["rain_trace"]);
				observationItem.RelativeHumidity = GetObservationDataItemAsDouble(dataItem["rel_hum"]);
				observationItem.Temperature = GetObservationDataItemAsDouble(dataItem["air_temp"]);
				observationItem.ApparentTemperature = GetObservationDataItemAsDouble(dataItem["apparent_t"]);
				observationItem.WindDirection = dataItem["wind_dir"].ToString();
				observationItem.WindGustSpeed = GetObservationDataItemAsDouble(dataItem["gust_kmh"]);
				observationItem.WindSpeed = GetObservationDataItemAsDouble(dataItem["wind_spd_kmh"]);

				// Add to the list
				observationList.Add(observationItem);
				
			}

			// return the observation list
			return observationList;

		}

		private double GetObservationDataItemAsDouble(JToken dataItem)
		{
			return (dataItem.ToString() != "-" ? dataItem.ToObject<double>() : 0);
		}

		/// <summary>
		/// Ensures the cache directory exists.
		/// </summary>
		private void EnsureCacheDirectoryExists(string locationCode)
		{
			// Get the cache directory based on the product id
			string cacheDirectory = GetCacheDirectory(locationCode);

			// If the cache directory doesn't exist, create it
			if (!Directory.Exists(cacheDirectory))
			{
				Directory.CreateDirectory(cacheDirectory);
			}
		}

		/// <summary>
		/// Determines if the cache file is valid. If the cache file exists, but its expired, it will delete the cache file.
		/// </summary>
		/// <param name="filename">The path to the cache file.</param>
		/// <returns>True if the cache file is valid, otherwise false.</returns>
		private static bool IsCacheFileValid(string filename)
		{
			// Get the cache duration
			TimeSpan cacheDuration = ResponseHubSettings.WeatherData.RadarCacheDuration;

			// If the file exists, and it was created within the cachefile timeout period, then the feedSource should be the file instead
			if (File.Exists(filename))
			{

				// Get the date time the file was created
				DateTime createdUtc = File.GetCreationTimeUtc(filename);

				// If the current datetime is less than or equal to the file creation time + cache duration, cache is valid
				// If not, delete the cache file
				if (DateTime.UtcNow <= createdUtc.Add(cacheDuration))
				{
					return true;
				}
				else
				{
					File.Delete(filename);
					return false;
				}

			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Gets the absolute path to the cache file based on the BoM cache directory in the application configurations.
		/// </summary>
		/// <param name="productId">The BoM product id to get the cache file for.</param>
		/// <returns>The absolute path to the cache file.</returns>
		private string GetImageListCacheFilename(string productId, string locationCode)
		{
			// Get the cache filename from the warning source
			return String.Format("{0}\\{1}_cache.txt", GetCacheDirectory(locationCode), productId.ToUpper());
			
		}

		#endregion

	}
}
