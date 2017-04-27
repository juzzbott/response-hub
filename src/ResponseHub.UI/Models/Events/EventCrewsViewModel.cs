using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Events
{
	public class EventCrewsViewModel
	{

		public IList<CrewViewModel> Crews { get; set; }

		public EventCrewsViewModel()
		{
			Crews = new List<CrewViewModel>();
		}
	}
}