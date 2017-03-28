using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Home
{
	public class GroupSelectionViewModel
	{

		public IDictionary<Guid, string> AvailableGroups { get; set; }

		[Required(ErrorMessage = "You must select a group.")]
		public Guid GroupId { get; set; }
			
	}
}