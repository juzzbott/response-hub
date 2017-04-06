using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Common.Configuration
{
	public class WeatherDataConfigurationSection : ConfigurationSection
	{
		private const string AddItemNameKey = "location";
		private const string RadarCacheDirectoryKey = "radarCacheDirectory";
		private const string RadarCacheDurationKey = "radarCacheDuration";
		private const string RadarFtpLocationKey = "radarFtpLocation";
		private const string ObservationLocationKey = "observationLocation";

		[ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
		[ConfigurationCollection(typeof(WeatherLocationElementCollection), AddItemName = AddItemNameKey)]
		public WeatherLocationElementCollection Locations
		{
			get
			{
				return (WeatherLocationElementCollection)this[""];
			}
			set
			{
				this[""] = value;
			}
		}

		[ConfigurationProperty(RadarCacheDirectoryKey, IsRequired = true)]
		public string RadarCacheDirectory
		{
			get
			{
				return (string)base[RadarCacheDirectoryKey];
			}
			set
			{
				base[RadarCacheDirectoryKey] = value;
			}
		}

		[ConfigurationProperty(RadarCacheDurationKey, IsRequired = true)]
		public TimeSpan RadarCacheDuration
		{
			get
			{
				return TimeSpan.Parse(base[RadarCacheDurationKey].ToString());
			}
			set
			{
				base[RadarCacheDurationKey] = value;
			}
		}

		[ConfigurationProperty(RadarFtpLocationKey, IsRequired = true)]
		public string RadarFtpLocation
		{
			get
			{
				return (string)base[RadarFtpLocationKey];
			}
			set
			{
				base[RadarFtpLocationKey] = value;
			}
		}

		[ConfigurationProperty(ObservationLocationKey, IsRequired = true)]
		public string ObservationLocation
		{
			get
			{
				return (string)base[ObservationLocationKey];
			}
			set
			{
				base[ObservationLocationKey] = value;
			}
		}
	}
}
