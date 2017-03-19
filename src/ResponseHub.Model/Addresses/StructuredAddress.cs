using Enivate.ResponseHub.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Addresses
{

	[Serializable]
	public class StructuredAddress : IEntity
	{

		public Guid Id { get; set; }

		public string Unit { get; set; }

		public string StreetNumber { get; set; }

		public string Street { get; set; }

		public string Suburb { get; set; }

		public string State { get; set; }

		public string Postcode { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public string GoogleGeocodeId { get; set; }

		/// <summary>
		/// Contains a hash of the original address query. This hash is used as an index lookup to prevent additional lookups to the google geocode service.
		/// </summary>
		public string AddressQueryHash { get; set; }

		/// <summary>
		/// Gets the hash of the address query.
		/// </summary>
		/// <param name="addressQuery">The address query to get the hash of.</param>
		/// <returns>The hashed address query.</returns>
		public static string GetAddressQueryHash(string addressQuery)
		{
			return HashGenerator.GetSha1HashString(addressQuery.ToUpper(), 1);
		}

	}
}
