using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Common.Configuration
{
	public class WarningElementCollection : ConfigurationElementCollection
	{
		public WarningSourceElement this[int index]
		{
			get
			{
				return (WarningSourceElement)BaseGet(index);
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

		public new WarningSourceElement this[string sourceType]
		{
			get
			{
				return (WarningSourceElement)BaseGet(sourceType.ToUpper());
			}
			set
			{
				if (BaseGet(sourceType.ToUpper()) != null)
				{
					BaseRemoveAt(BaseIndexOf(BaseGet(sourceType.ToUpper())));
				}
				BaseAdd(value);
			}
		}

		/// <summary>
		/// Creates a new instance of the WarningSource configuration element.
		/// </summary>
		/// <returns></returns>
		protected override ConfigurationElement CreateNewElement()
		{
			return new WarningSourceElement();
		}

		/// <summary>
		/// Gets the configuration element key for the warning source configuration element.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((WarningSourceElement)element).SourceType;
		}

	}
}
