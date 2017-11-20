using Enivate.ResponseHub.Model.Upkeep;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Upkeep
{
	public class ViewTaskViewModel
	{
		public Guid Id { get; set; }

		[Required(ErrorMessage = "You must enter a name for the asset.")]
		public string Name { get; set; }

		public Guid? AssetId { get; set; }

		public Asset Asset { get; set; }

		public string TaskItemJson { get; set; }

		public IList<string> TaskItems { get; set; }
	}
}