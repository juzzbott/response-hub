using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Spatial
{
	public class MapReference
	{

		public int PageNumber { get; set; }

		public string GridReference { get; set; }

		public int UtmNumber { get; set; }

		public int Scale { get; set; }

		public Coordinates Coordinates { get; set; }

		public MapReference()
		{
			Coordinates = new Coordinates();
		}


	}
}
