using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Common.Configuration
{
	public class WeatherLocationElement : ConfigurationElement
	{
	
		private const string NameKey = "name";
		private const string CodeKey = "code";
		private const string BaseRadarProductIdKey = "baseRadarProductId";
		private const string RainRadarProductIdKey = "rainRadarProductId";
		private const string WindRadarProductIdKey = "windRadarProductId";
		private const string ObservationIdKey = "observationId";
		
		[ConfigurationProperty(NameKey, IsRequired = true)]
		public string Name
		{
			get
			{
				return (string)base[NameKey];
			}
			set
			{
				base[NameKey] = value;
			}
		}

		[ConfigurationProperty(CodeKey, IsKey = true, IsRequired = true)]
		public string Code
		{
			get
			{
				return (string)base[CodeKey];
			}
			set
			{
				base[CodeKey] = value;
			}
		}

		[ConfigurationProperty(BaseRadarProductIdKey, IsRequired = true)]
		public string BaseRadarProductId
		{
			get
			{
				return (string)base[BaseRadarProductIdKey];
			}
			set
			{
				base[BaseRadarProductIdKey] = value;
			}
		}

		[ConfigurationProperty(RainRadarProductIdKey, IsRequired = true)]
		public string RainRadarProductId
		{
			get
			{
				return (string)base[RainRadarProductIdKey];
			}
			set
			{
				base[RainRadarProductIdKey] = value;
			}
		}

		[ConfigurationProperty(WindRadarProductIdKey, IsRequired = true)]
		public string WindRadarProductId
		{
			get
			{
				return (string)base[WindRadarProductIdKey];
			}
			set
			{
				base[WindRadarProductIdKey] = value;
			}
		}

		[ConfigurationProperty(ObservationIdKey, IsRequired = true)]
		public string ObservationId
		{
			get
			{
				return (string)base[ObservationIdKey];
			}
			set
			{
				base[ObservationIdKey] = value;
			}
		}
	}
}
