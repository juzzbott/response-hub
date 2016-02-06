using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Spatial;

namespace Enivate.ResponseHub.Model.Messages
{
	public class Location
	{

		public string MapReference { get; set; }

		public string MapType { get; set; }

		public string MapPage { get; set; }

		public string AddressInfo { get; set; }

		public Coordinate Coordinates { get; set; }

		public Location()
		{
			this.Coordinates = new Coordinate();
		}

	}
}
