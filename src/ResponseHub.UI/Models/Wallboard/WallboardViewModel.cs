using Enivate.ResponseHub.Model.Warnings;
using Enivate.ResponseHub.UI.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Wallboard
{
	public class WallboardViewModel
	{

		public IList<ParsedMessageViewModel> Messages { get; set; }

		public IDictionary<WarningSource, IWarning> Warnings { get; set; }

		public IList<string> RadarImageFiles { get; set; }

		public WallboardViewModel()
		{
			Messages = new List<ParsedMessageViewModel>();
			Warnings = new Dictionary<WarningSource, IWarning>();
			RadarImageFiles = new List<string>();
		}

	}
}