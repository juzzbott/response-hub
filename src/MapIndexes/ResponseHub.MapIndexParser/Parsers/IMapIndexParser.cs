using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Spatial;

namespace Enivate.ResponseHub.MapIndexParser.Parsers
{
	public interface IMapIndexParser
	{

		IDictionary<string, MapIndex> MapIndexes { get; set; }

	}
}
