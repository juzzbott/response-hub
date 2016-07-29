using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Logs
{
	public class LogViewModel
	{

		public IList<SelectListItem> LogFilenames { get; set; }

		public string SelectedFile { get; set; }

		public string LogFileData { get; set; }

		public LogViewModel()
		{
			LogFilenames = new List<SelectListItem>();
		}

	}
}