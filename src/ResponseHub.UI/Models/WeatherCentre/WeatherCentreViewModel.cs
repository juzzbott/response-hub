using Enivate.ResponseHub.Model.Warnings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.WeatherCentre
{
	public class WeatherCentreViewModel
	{
		
		public IDictionary<WarningSource, IWarning> Warnings { get; set; }

		public RadarLoopViewModel RainRadar { get; set; }

		public RadarLoopViewModel WindRadar { get; set; }

		public WeatherCentreViewModel()
		{
			Warnings = new Dictionary<WarningSource, IWarning>();
		}

	}
}