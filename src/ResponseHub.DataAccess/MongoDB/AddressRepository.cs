using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Addresses;

using MongoDB.Driver;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{
	public class AddressRepository : MongoRepository<StructuredAddress>, IAddressRepository
	{

		/// <summary>
		/// Gets the structured address based on the address query hash.
		/// </summary>
		/// <param name="addressQueryHash">The address query hash to search by.</param>
		/// <returns>The structured address object if found, otherwise return null.</returns>
		public async Task<StructuredAddress> GetByAddressQueryHash(string addressQueryHash)
		{

			// Build the query
			FilterDefinition<StructuredAddress> filter = Builders<StructuredAddress>.Filter.Eq(i => i.AddressQueryHash, addressQueryHash);

			// Return the structured address that has the hash if one exists
			return await Collection.Find(filter).FirstOrDefaultAsync();

		}

	}
}
