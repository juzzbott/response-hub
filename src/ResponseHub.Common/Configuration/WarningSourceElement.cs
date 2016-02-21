using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Common.Configuration
{
	public class WarningSourceElement : ConfigurationElement
	{

		private const string SourceTypeKey = "sourceType";
		private const string UrlKey = "url";
		private const string FormatKey = "format";

		[ConfigurationProperty(SourceTypeKey, IsKey = true, IsRequired = true)]
		public string SourceType
		{
			get
			{
				return (string)base[SourceTypeKey];
			}
			set
			{
				base[SourceTypeKey] = value;
			}
		}

		[ConfigurationProperty(UrlKey, IsRequired = true)]
		public string Url
		{
			get
			{
				return (string)base[UrlKey];
			}
			set
			{
				base[UrlKey] = value;
			}
		}

		[ConfigurationProperty(FormatKey, IsRequired = false, DefaultValue = "rss")]
		public string Format
		{
			get
			{
				return (string)base[FormatKey];
			}
			set
			{
				base[FormatKey] = value;
			}
		}

	}
}
