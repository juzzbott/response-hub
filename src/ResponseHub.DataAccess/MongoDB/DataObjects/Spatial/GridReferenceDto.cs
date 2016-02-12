using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Driver.GeoJsonObjectModel;

namespace Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Spatial
{
	public class GridReferenceDto
	{

		public string GridSquare { get; set; }

		public GeoJson2DGeographicCoordinates Coordinates { get; set; }
		

	}
}
