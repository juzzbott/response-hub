using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

using Enivate.ResponseHub.Model.Warnings;
using Enivate.ResponseHub.Model.Warnings.Interface;
using Enivate.ResponseHub.Common.Configuration;
using Enivate.ResponseHub.Logging;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class WarningService : IWarningService
	{

		private const string BureauOfMeteorologySourceKey = "BOM";
		private const string StateEmergencyServiceSourceKey = "SES";
		private const string CountryFireAuthoritySourceKey = "CFA";

		private ILogger _log;

		public WarningService(ILogger log)
		{
			_log = log;
		}

		public IList<IWarning> GetBureauOfMeteorologyWarnings()
		{

			// Get the configuration
			WarningSourceElement elem = ConfigurationSettings.Warnings.WarningSources[BureauOfMeteorologySourceKey];

			// if the warning source doesn't exist, just return empty list
			if (elem == null)
			{
				return new List<IWarning>();
			};

			SyndicationFeed bomFeed = GetFeed(elem.Url, WarningSource.BureauOfMeteorology);

			// just return the empty list
			return new List<IWarning>();
		}

		public IList<IWarning> GetStateEmergencyServiceWarnings()
		{
			// Get the configuration
			WarningSourceElement elem = ConfigurationSettings.Warnings.WarningSources[StateEmergencyServiceSourceKey];

			// if the warning source doesn't exist, just return empty list
			if (elem == null)
			{
				return new List<IWarning>();
			};

			SyndicationFeed sesFeed = GetFeed(elem.Url, WarningSource.StateEmergencyService);

			// just return the empty list
			return new List<IWarning>();
		}

		public IList<IWarning> GetCountryFireAuthorityWarnings()
		{
			// Get the configuration
			WarningSourceElement elem = ConfigurationSettings.Warnings.WarningSources[CountryFireAuthoritySourceKey];

			// if the warning source doesn't exist, just return empty list
			if (elem == null)
			{
				return new List<IWarning>();
			};

			SyndicationFeed cfaFeed = GetFeed(elem.Url, WarningSource.CountryFireAuthority);

			// just return the empty list
			return new List<IWarning>();
		}

		public IList<IWarning> GetWarnings(WarningSource source)
		{
			// Create the list of warnings
			IList<IWarning> warnings = new List<IWarning>();

			// Add the bureau of meteorology warnings
			if ((source & WarningSource.BureauOfMeteorology) == WarningSource.BureauOfMeteorology)
			{
				foreach(IWarning warning in GetBureauOfMeteorologyWarnings())
				{
					warnings.Add(warning);
				}
			}

			// Add the ses warnings
			if ((source & WarningSource.StateEmergencyService) == WarningSource.StateEmergencyService)
			{
				foreach (IWarning warning in GetStateEmergencyServiceWarnings())
				{
					warnings.Add(warning);
				}
			}

			// Add the cfa warnings
			if ((source & WarningSource.CountryFireAuthority) == WarningSource.CountryFireAuthority)
			{
				foreach (IWarning warning in GetCountryFireAuthorityWarnings())
				{
					warnings.Add(warning);
				}
			}

			// Order the warnings by the timestamp
			warnings = warnings.OrderBy(i => i.Timestamp).ToList();

			// return the warnings
			return warnings;
		}

		// Get the feed.
		private SyndicationFeed GetFeed(string url, WarningSource sourceType)
		{

			// Default to the url feed source.
			string feedSource = url;

			// Get the filename of what the cache file would be based on the source type.
			string cacheFileName = GetCacheFilename(sourceType);

			// Determine if there is a valid cache file, and it's not expired.
			bool cacheValid = IsCacheFileValid(cacheFileName);

			// If the cache is valid, use the cache file path.
			if (cacheValid)
			{
				feedSource = cacheFileName;
			}

			// Create the syndication feed
			SyndicationFeed feed;

			// Load the feed.
			using (XmlReader reader = XmlReader.Create(url))
			{
				feed = SyndicationFeed.Load(reader);
			}

			// If the cache wasn't valid, write it to disk
			if (!cacheValid)
			{

				// Ensure the cache directory exists
				EnsureCacheDirectoryExists(cacheFileName);

				using (XmlWriter writer = XmlWriter.Create(cacheFileName, new XmlWriterSettings() { Indent = true }))
				{
					feed.SaveAsRss20(writer);
				}
			}

			// return feed
			return feed;

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
			TimeSpan cacheDuration = ConfigurationSettings.Warnings.CacheDuration;

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
		/// Gets the absolute path to the cache file based on the source type and the cache directory in the application configurations
		/// </summary>
		/// <param name="sourceType">The source type of the cache file.</param>
		/// <returns>The absolute path to the cache file.</returns>
		private string GetCacheFilename(WarningSource sourceType)
		{
			// Get the cache filename from the warning source
			string cacheFile = String.Format("{0}_cache.xml", Enum.GetName(sourceType.GetType(), sourceType));

			// Get the cache directory
			string cacheDirectory = ConfigurationSettings.Warnings.CacheDirectory;

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
