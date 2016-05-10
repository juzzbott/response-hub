using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Spatial
{
	public class MapIndexDto : IEntity
	{

		public Guid Id { get; set; }

		public string PageNumber { get; set; }

		public int UtmNumber { get; set; }

		public int Scale { get; set; }

		public IList<MapGridReferenceInfoDto> GridReferences { get; set; }

		public MapType MapType { get; set; }

		public MapIndexDto()
		{
			GridReferences = new List<MapGridReferenceInfoDto>();
		}

	}
}
