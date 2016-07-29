using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.SystemSettings
{
	public class SystemSettingsViewModel
	{

		public CacheDetails Cache { get; set; }

		public SystemSettingsViewModel()
		{
			Cache = new CacheDetails();
		}

	}
}