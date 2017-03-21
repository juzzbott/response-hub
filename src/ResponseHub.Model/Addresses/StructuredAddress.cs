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

		public StructuredAddress()
		{
			// Instantiate the id
			Id = Guid.NewGuid();
		}

		/// <summary>
		/// Gets the hash of the address query.
		/// </summary>
		/// <param name="addressQuery">The address query to get the hash of.</param>
		/// <returns>The hashed address query.</returns>
		public static string GetAddressQueryHash(string addressQuery)
		{
			return HashGenerator.GetSha1HashString(addressQuery.ToUpper(), 1);
		}

		/// <summary>
		/// Returns a properly formatted address for the StructuredAddress.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			// If there is a unit number, add that
			if (!String.IsNullOrEmpty(Unit))
			{
				sb.AppendFormat("{0}/", Unit);
			}

			// If there is a stret, add that
			if (!String.IsNullOrEmpty(Street))
			{
				// If there is a street number, add that first
				if (!String.IsNullOrEmpty(StreetNumber))
				{
					sb.AppendFormat("{0} ", StreetNumber);
				}

				sb.AppendFormat("{0}, ", Street);
			}

			// If there is a suburb, add it
			if (!String.IsNullOrEmpty(Suburb))
			{
				sb.AppendFormat("{0} ", Suburb);
			}

			// If there is a State, add it
			if (!String.IsNullOrEmpty(State))
			{
				sb.AppendFormat("{0} ", State);
			}

			// If there is a Postcode, add it
			if (!String.IsNullOrEmpty(Postcode))
			{
				sb.AppendFormat("{0} ", Postcode);
			}

			// If there is already data in the sb, add ", Australia"
			if (sb.Length > 0)
			{
				// If the last char is ' ', remove it
				if (sb[sb.Length - 1] == ' ')
				{
					sb.Remove(sb.Length - 1, 1);
				}
				sb.Append(", Australia");
			}

			// return the stringbuilder
			return sb.ToString().Trim();
		}

	}
}
