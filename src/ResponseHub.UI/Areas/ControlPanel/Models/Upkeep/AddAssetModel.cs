using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.ControlPanel.Admin.Models.Upkeep
{
	public class AddAssetModel
	{

		[Required(ErrorMessage = "You must enter a name for the asset.")]
		public string Name { get; set; }

		public string Description { get; set; }

	}
}