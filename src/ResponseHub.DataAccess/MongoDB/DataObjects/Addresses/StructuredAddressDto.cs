using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Driver.GeoJsonObjectModel;
using Enivate.ResponseHub.Model;

namespace Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Addresses
{
	public class StructuredAddressDto : IEntity
	{

		public Guid Id { get; set; }

		public string Unit { get; set; }

		public string StreetNumber { get; set; }

		public string Street { get; set; }

		public string Suburb { get; set; }

		public string State { get; set; }

		public string Postcode { get; set; }

		public GeoJson2DGeographicCoordinates Coordinates { get; set; }

		public string GoogleGeocodeId { get; set; }

		/// <summary>
		/// Contains a hash of the original address query. This hash is used as an index lookup to prevent additional lookups to the google geocode service.
		/// </summary>
		public string AddressQueryHash { get; set; }

		public StructuredAddressDto()
		{
			Id = Guid.NewGuid();
		}

	}
}
