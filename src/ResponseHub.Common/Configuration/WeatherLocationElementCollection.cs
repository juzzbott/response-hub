using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Common.Configuration
{
	public class WeatherLocationElementCollection : ConfigurationElementCollection
	{

		public WeatherLocationElement this[int index]
		{
			get
			{
				return (WeatherLocationElement)BaseGet(index);
			}
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				BaseAdd(index, value);
			}
		}

		public new WeatherLocationElement this[string code]
		{
			get
			{
				return (WeatherLocationElement)BaseGet(code);
			}
			set
			{
				if (BaseGet(code) != null)
				{
					BaseRemoveAt(BaseIndexOf(BaseGet(code)));
				}
				BaseAdd(value);
			}
		}

		/// <summary>
		/// Creates a new instance of the WeatherLocation configuration element.
		/// </summary>
		/// <returns></returns>
		protected override ConfigurationElement CreateNewElement()
		{
			return new WeatherLocationElement();
		}

		/// <summary>
		/// Gets the configuration element key for the weather location configuration element.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((WeatherLocationElement)element).Code;
		}

	}
}
