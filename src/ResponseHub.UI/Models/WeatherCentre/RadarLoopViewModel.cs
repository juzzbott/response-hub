using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.WeatherCentre
{
	public class RadarLoopViewModel
	{
		
		public IList<string> RadarImageFiles { get; set; }

		public string RadarCode { get; set; }

		public string RadarBaseCode { get; set; }

		public RadarLoopViewModel()
		{
			RadarImageFiles = new List<string>();
		}
	}
}