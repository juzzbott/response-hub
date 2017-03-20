using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Parsers
{
	[Serializable]
	public class AddressParserData
	{

		public string StreetAddressRegex { get; set; }

		public string[] StreetTypes { get; set; }

		public string[] JobTypes { get; set; }

		public string[] ExcludeWords { get; set; }

	}
}
