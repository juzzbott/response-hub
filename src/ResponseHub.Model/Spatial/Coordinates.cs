using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Spatial
{
	public class Coordinates
	{

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public Coordinates()
		{

		}

		public Coordinates(double latitude, double longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

	}
}
