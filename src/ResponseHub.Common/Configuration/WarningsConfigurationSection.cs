using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Common.Configuration
{

	public class WarningsConfigurationSection : ConfigurationSection
	{

		private const string AddItemNameKey = "source";
		private const string CacheDirectoryKey = "cacheDirectory";
		private const string CacheDurationKey = "cacheDuration";

		[ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
		[ConfigurationCollection(typeof(WarningElementCollection), AddItemName = AddItemNameKey)]
		public WarningElementCollection WarningSources
		{
			get
			{
				return (WarningElementCollection)this[""];
			}
			set
			{
				this[""] = value;
			}
		}

		[ConfigurationProperty(CacheDirectoryKey, IsRequired = true)]
		public string CacheDirectory
		{
			get
			{
				return (string)base[CacheDirectoryKey];
			}
			set
			{
				base[CacheDirectoryKey] = value;
			}
		}

		[ConfigurationProperty(CacheDurationKey, IsRequired = true)]
		public TimeSpan CacheDuration
		{
			get
			{
				return TimeSpan.Parse(base[CacheDurationKey].ToString());
			}
			set
			{
				base[CacheDurationKey] = value;
			}
		}

		

	}
}
