using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Spatial
{
	public class MapIndex : IEntity
	{

		public Guid Id { get; set; }

		public string PageNumber { get; set; }

		public int UtmNumber { get; set; }

		public int Scale { get; set; }

		public IList<GridReference> GridReferences { get; set; }

		public MapType MapType { get; set; }

		public MapIndex()
		{
			this.Id = Guid.NewGuid();
			GridReferences = new List<GridReference>();
		}


	}
}
