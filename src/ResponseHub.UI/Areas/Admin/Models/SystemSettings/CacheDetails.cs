using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.SystemSettings
{
	public class CacheDetails
	{

		public long TotalItems { get; set; }

		public Dictionary<string, DateTime> CacheKeys { get; set; }

		public decimal CacheMemoryLimit { get; set; }

		public long PysicalMemoryLimit { get; set; }

		public TimeSpan PollingInterval { get; set; }

		public CacheDetails()
		{
			CacheKeys = new Dictionary<string, DateTime>();
		}

	}
}