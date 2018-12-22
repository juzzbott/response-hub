using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Caching;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Addresses;
using Enivate.ResponseHub.Model.Addresses.Interface;
using Enivate.ResponseHub.Wrappers.Google;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class AddressService : IAddressService
	{

		/// <summary>
		/// The logger
		/// </summary>
		private ILogger _log;

		/// <summary>
		/// The address repository.
		/// </summary>
		private IAddressRepository _addressRepository;

		private const int AddressCacheExpiryMinutes = 60;

		public AddressService(ILogger log, IAddressRepository addressRepository)
		{
			_log = log;
			_addressRepository = addressRepository;
		}

		/// <summary>
		/// Get the google geocode data for a given address string.
		/// </summary>
		/// <param name="addressQuery">The address query used to query Google Geocode service.</param>
		/// <returns></returns>
		public async Task<StructuredAddress> GetStructuredAddressByAddressQuery(string addressQuery)
		{

			try
			{

				// Replace all ' ' with + chars for use in the url
				addressQuery = addressQuery.Replace(" ", "+");

				// If the address query does not end in ',+AU', then append it here
				if (!addressQuery.EndsWith(",+AU", StringComparison.CurrentCultureIgnoreCase))
				{
					addressQuery = String.Format("{0},+AU", addressQuery);
				}

				// Get the address query hash
				string addressQueryHash = StructuredAddress.GetAddressQueryHash(addressQuery);

				// First, check the application cache for the address.
				string cacheItemKey = CacheUtility.GetEntityCacheKey(typeof(StructuredAddress), addressQueryHash);
				StructuredAddress address = CacheManager.GetItem<StructuredAddress>(cacheItemKey);

				// If the item exists in cache, return it
				if (address != null)
				{
					return address;
				}

				// Address doesn't exist in cache, so try to get it from the database
				address = await _addressRepository.GetByAddressQueryHash(addressQueryHash);

				// If the address is not null, add to the cache and return it
				if (address != null)
				{
					CacheManager.AddItem(cacheItemKey, address, AddressCacheExpiryMinutes);
				}

				// Address doesn't exist in the data either, so now we need to get it from the google geocode service.
				GoogleGeocodeWrapper wrapper = new GoogleGeocodeWrapper(_log);
				address = await wrapper.GetGoogleGeocodeResult(addressQuery);

				// If the address is not null, add it to both the database and the item cache
				if (address != null)
				{
					// Add to the cache
					CacheManager.AddItem(cacheItemKey, address, AddressCacheExpiryMinutes);
					await _addressRepository.Add(address);
				}

				return address;

			}
			catch (Exception ex)
			{
				await _log.Error(String.Format("Unable to get address from addressQuery. Message: {0}", ex.Message), ex);
				return null;
			}

		}


	}
}
