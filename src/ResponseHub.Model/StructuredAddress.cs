using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model
{
	public class StructuredAddress
	{

		public string Unit { get; set; }

		public string StreetNumber { get; set; }

		public string Street { get; set; }

		public string Suburb { get; set; }

		public string State { get; set; }

		public string Postcode { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public string GoogleGeocodeId { get; set; }

	}
}
