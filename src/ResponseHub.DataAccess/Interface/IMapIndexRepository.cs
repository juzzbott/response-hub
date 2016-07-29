using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Spatial;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface IMapIndexRepository
	{

		Task BatchInsert(IList<MapIndex> mapIndexes);

		Task ClearCollection();

		Task<MapIndex> GetMapIndexByPageNumber(MapType mapType, string mapPage);
	}
}
