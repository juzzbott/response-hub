using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Enivate.ResponseHub.Common.Constants;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.WeatherData.Interface;
using Enivate.ResponseHub.Common.Configuration;

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
		public IList<string> GetRadarImagesForProduct(string productId)
		{

			// Create the list to store the image files.
			List<string> imageFiles = new List<String>();

			try
			{

				// Get the filename of what the cache file would be based on the source type.
				string cacheFileName = GetImageListCacheFilename(productId);

				// Determine if there is a valid cache file, and it's not expired.
				bool cacheValid = IsCacheFileValid(cacheFileName);

				if (cacheValid)
				{
					// Get the image files for the product from the cache file.
					imageFiles.AddRange(GetImageFilesFromCache(cacheFileName));
				}
				else
				{

					// Get the image files from the FTP location
					imageFiles.AddRange(GetFileListFromFtp(productId));

					if (!cacheValid)
					{
						// Ensure the cache directory exists
						EnsureCacheDirectoryExists(productId);

						// Write the list of image files to disk.
						using (StreamWriter writer = new StreamWriter(cacheFileName, false))
						{
							foreach(string imageFile in imageFiles)
							{
								writer.WriteLine(imageFile);
							}
						}
					}

				}

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

			// Get the radar image cache directory and full filename
			string cacheDirectory = GetCacheDirectory(productId);
			string cacheImageFilename = String.Format("{0}.png", radarImageFilename.Replace("_", "."));
			string fullCacheImagePath = String.Format("{0}\\{1}", cacheDirectory, cacheImageFilename);

			// If the file does not exist, download it from the BoM FTP server
			if (!File.Exists(fullCacheImagePath))
			{
				// Get the ftp locaion from the configuration
				string ftpFileLocation = ConfigurationSettings.WeatherData.RadarFtpLocation;
				string ftpFilename = String.Format("{0}{1}", ftpFileLocation, cacheImageFilename);

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

			// load the bytes of the file into a byte array and return it
			byte[] fileBytes = File.ReadAllBytes(fullCacheImagePath);

			// return the byte array
			return fileBytes;

		}

		private IEnumerable<string> GetImageFilesFromCache(string cacheFileName)
		{
			using (StreamReader reader = new StreamReader(cacheFileName))
			{
				while (reader.Peek() >= 0)
				{
					yield return reader.ReadLine();
				}
			}
		}


		/// <summary>
		/// Get cache file contents from the BoM FTP location, and returns the radar images as a list of filenames.
		/// </summary>
		/// <param name="productId">The product id to get the image files for.</param>
		/// <returns>The list of image filenames.</returns>
		private IEnumerable<string> GetFileListFromFtp(string productId)
		{

			// Get the ftp locaion from the configuration
			string ftpLocation = ConfigurationSettings.WeatherData.RadarFtpLocation;

			FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(ftpLocation);
			request.Method = WebRequestMethods.Ftp.ListDirectory;
			request.Credentials = new NetworkCredential("anonymous", "anonymous");

			// Get the response
			FtpWebResponse response = (FtpWebResponse)request.GetResponse();

			// Read the details of the response into a string
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
			{
				// While there is still data to read
				while (reader.Peek() >= 0)
				{
					// Read the filename from the line
					string fileName = reader.ReadLine();

					// If the filename contains the product id, then add to the list, otherwise just continue to next
					if (fileName.ToUpper().Contains(productId.ToUpper() + ".T."))
					{

						// Strip the extension and replace . with _ so it can be used in our routes.
						string actualFiename = fileName.Replace(".png", "").Replace(".", "_");

						yield return actualFiename;
					}
				}
			}
		}
		
		/// <summary>
		/// Ensures the cache directory exists.
		/// </summary>
		private void EnsureCacheDirectoryExists(string productId)
		{
			// Get the cache directory based on the product id
			string cacheDirectory = GetCacheDirectory(productId);

			// If the cache directory doesn't exist, create it
			if (!Directory.Exists(cacheDirectory))
			{
				Directory.CreateDirectory(cacheDirectory);
			}
		}

		/// <summary>
		/// Determines if the cache file is valid. If the cache file exists, but its expired, it will delete the cache file.
		/// </summary>
		/// <param name="cacheFileName">The path to the cache file.</param>
		/// <returns>True if the cache file is valid, otherwise false.</returns>
		private static bool IsCacheFileValid(string cacheFileName)
		{
			// Get the cache duration
			TimeSpan cacheDuration = ConfigurationSettings.WeatherData.RadarCacheDuration;

			// If the file exists, and it was created within the cachefile timeout period, then the feedSource should be the file instead
			if (File.Exists(cacheFileName))
			{

				// Get the date time the file was created
				DateTime createdUtc = File.GetCreationTimeUtc(cacheFileName);

				// If the current datetime is less than or equal to the file creation time + cache duration, cache is valid
				// If not, delete the cache file
				if (DateTime.UtcNow <= createdUtc.Add(cacheDuration))
				{
					return true;
				}
				else
				{
					File.Delete(cacheFileName);
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
		private string GetImageListCacheFilename(string productId)
		{
			// Get the cache filename from the warning source
			return String.Format("{0}\\{1}_cache.txt", GetCacheDirectory(productId), productId.ToUpper());
			
		}

		/// <summary>
		/// Gets the cache directory for the current 
		/// </summary>
		/// <param name="productId"></param>
		/// <returns></returns>
		private string GetCacheDirectory(string productId)
		{
			// Get the cache directory
			string cacheDirectory = ConfigurationSettings.WeatherData.RadarCacheDirectory;

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
			cacheDirectory = String.Format("{0}{1}{2}", cacheDirectory, (cacheDirectory.EndsWith("\\") ? "" : "\\"), productId);
			return cacheDirectory;
		}

	}
}
