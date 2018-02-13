using Enivate.ResponseHub.Model.Upkeep;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Upkeep
{
	public class ViewTaskViewModel
	{
		public Guid Id { get; set; }

		[Required(ErrorMessage = "You must enter a name for the task.")]
		public string Name { get; set; }

		public Guid? AssetId { get; set; }

		public Asset Asset { get; set; }

		public IList<SelectListItem> AvailableAssets { get; set; }

		public string TaskItemsJson { get; set; }

		public IList<string> TaskItems { get; set; }
	}
}