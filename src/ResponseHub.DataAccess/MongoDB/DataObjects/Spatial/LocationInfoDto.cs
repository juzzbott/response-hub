using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Driver.GeoJsonObjectModel;
using Enivate.ResponseHub.Model.Spatial;

namespace Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Spatial
{
	public class LocationInfoDto
	{

		public string MapReference { get; set; }

		public MapType MapType { get; set; }

		public string MapPage { get; set; }

		public string GridSqaure { get; set; }

		public string GridReference { get; set; }

		public string AddressInfo { get; set; }

		public GeoJson2DGeographicCoordinates Coordinates { get; set; }


	}
}
