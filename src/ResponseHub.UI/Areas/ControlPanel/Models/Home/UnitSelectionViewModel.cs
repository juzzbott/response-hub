using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Home
{
	public class UnitSelectionViewModel
	{

		public IDictionary<Guid, string> AvailableUnits { get; set; }

		[Required(ErrorMessage = "You must select a unit.")]
		public Guid UnitId { get; set; }
			
	}
}