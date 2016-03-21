using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Caching;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Agencies;
using Enivate.ResponseHub.Model.Agencies.Interface;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class AgencyService : IAgencyService
	{

		private const string AllAgenciesCacheKey = "AllAgencies";

		private IAgencyRepository _repository;

		public AgencyService(IAgencyRepository repository)
		{
			_repository = repository;
		}

		/// <summary>
		/// Gets all the agencies in the database. Caches the results for next lookup.
		/// </summary>
		/// <returns>The list of all the agencies in the database.</returns>
		public async Task<IList<Agency>> GetAll()
		{

			// Get the agencies from the cache
			IList<Agency> allAgencies = CacheManager.GetItem<IList<Agency>>(AllAgenciesCacheKey);

			// If the cache item does not exist, get from the repository
			if (allAgencies == null)
			{
				// Get all the agencies
				allAgencies = await _repository.GetAll();

				// Add to the cache for next time if it's not null
				if (allAgencies != null)
				{
					CacheManager.AddItem(AllAgenciesCacheKey, allAgencies);
				}

			}

			// return the agencies
			return allAgencies;

		}

	}
}
