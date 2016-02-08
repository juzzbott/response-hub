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

		public MapType MapType { get; set; }

		public int MapPage { get; set; }

		public string GridReference { get; set; }

		public string AddressInfo { get; set; }

		public Coordinates Coordinates { get; set; }

		public Location()
		{
			this.Coordinates = new Coordinates();
		}

	}
}
