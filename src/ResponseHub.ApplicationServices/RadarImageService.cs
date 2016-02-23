using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Enivate.ResponseHub.Common.Constants;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.RadarImages.Interface;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class RadarImageService : IRadarImageService
	{

		ILogger _log;

		public RadarImageService(ILogger log)
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
				string cacheFileName = GetCacheFilename(productId);

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
						EnsureCacheDirectoryExists(cacheFileName);

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
			string ftpLocation = ConfigurationManager.AppSettings[ConfigurationKeys.BomRadarImageFtpLocation];

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
						yield return fileName;
					}
				}
			}
		}


		/// <summary>
		/// Ensures the cache directory exists.
		/// </summary>
		private void EnsureCacheDirectoryExists(string cacheFileName)
		{
			string cacheDirectory = Path.GetDirectoryName(cacheFileName);
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
			TimeSpan cacheDuration = new TimeSpan(0, 5, 0);

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
		private string GetCacheFilename(string productId)
		{
			// Get the cache filename from the warning source
			string cacheFile = String.Format("{0}_cache.txt", productId.ToUpper());

			// Get the cache directory
			string cacheDirectory = ConfigurationManager.AppSettings[ConfigurationKeys.BomCacheDirectory];

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
			cacheFile = String.Format("{0}{1}{2}",
				cacheDirectory,
				(cacheDirectory.EndsWith("\\") ? "" : "\\"),
				cacheFile);
			return cacheFile;

		}

	}
}
