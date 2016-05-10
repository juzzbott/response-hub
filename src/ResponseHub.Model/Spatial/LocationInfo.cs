using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Spatial;

namespace Enivate.ResponseHub.Model.Spatial
{
	public class LocationInfo
	{

		public string MapReference { get; set; }

		public MapType MapType { get; set; }

		public string MapPage { get; set; }

		public string GridSquare { get; set; }

		public string AddressInfo { get; set; }

		public Coordinates Coordinates { get; set; }

		public string GridReference { get; set; }

		public LocationInfo()
		{
			Coordinates = new Coordinates();
		}

	}
}
