using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Caching;
using Enivate.ResponseHub.Model;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class CapcodeService : ICapcodeService
	{
		/// <summary>
		/// The logging interface
		/// </summary>
		private ILogger _log;

		/// <summary>
		/// The repository for storing the capcodes.
		/// </summary>
		private ICapcodeRepository _repository;

		private const string AllCapcodesCacheKey = "AllCapcodes";

		/// <summary>
		/// Creates a new instance of the CapcodeService.
		/// </summary>
		/// <param name="repository"></param>
		/// <param name="log"></param>
		public CapcodeService(ICapcodeRepository repository, ILogger log)
		{
			_repository = repository;
			_log = log;
		}

		/// <summary>
		/// Gets a list of all the cap codes available in the system, not including individual group capcodes.
		/// </summary>
		/// <returns></returns>
		public async Task<IList<Capcode>> GetAll()
		{

			// Get the capcodes from the cache
			IList<Capcode> capcodes = CacheManager.GetItem<IList<Capcode>>(AllCapcodesCacheKey);

			// If the list is null, it doesn't exist in the cache, so get from the db
			if (capcodes == null)
			{
				capcodes = await _repository.GetAll();

				// If the capcodes are not null and there is at least one in the group, then add to the cache
				if (capcodes != null && capcodes.Any())
				{
					CacheManager.AddItem(AllCapcodesCacheKey, capcodes);
				}
			}

			// return the cap codes
			return capcodes;

		}

		/// <summary>
		/// Creates a new capcode and clears the existing capcode cache.
		/// </summary>
		/// <param name="name">The name of the capcode</param>
		/// <param name="capcodeAddress">The pager address of the capcode.</param>
		/// <param name="shortName">The short name for the address.</param>
		/// <param name="service">The servic type the capcode belongs to.</param>
		/// <returns>The newly created capcode object.</returns>
		public async Task<Capcode> Create(string name, string capcodeAddress, string shortName, ServiceType service)
		{

			// Create the capcode
			Capcode capcode = new Capcode()
			{
				Name = name,
				CapcodeAddress = capcodeAddress,
				ShortName = shortName,
				Service = service,
				Created = DateTime.UtcNow
			};

			// Create the capcode
			await _repository.Save(capcode);

			// Clear the "All capcodes" cache object
			CacheManager.RemoveItem(AllCapcodesCacheKey);

			// return the created capcode
			return capcode;

		}

		/// <summary>
		/// Get all the capcodes for the specified service.
		/// </summary>
		/// <param name="service">The service to get the capcodes for.</param>
		/// <returns></returns>
		public async Task<IList<Capcode>> GetAllByService(ServiceType service)
		{
			// Get all the capcodes
			IList<Capcode> allCapcodes = await GetAll();

			// Return all capcodes for the specified service.
			return allCapcodes.Where(i => i.Service == service).ToList();

		}

		/// <summary>
		/// Gets the capcode based on the ID.
		/// </summary>
		/// <param name="id">The ID of the capcode.</param>
		/// <returns>The capcode from the Id.</returns>
		public async Task<Capcode> GetById(Guid id)
		{

			// Get the capcode from the cache
			Capcode capcode = CacheManager.GetEntity<Capcode>(id);

			// If the capcode doesn't exist in the cache, get from the database and add to the cache
			if (capcode == null)
			{
				// Get the capcode
				capcode = await _repository.GetById(id);

				// If it exists, add to the cache for next time
				if (capcode != null)
				{
					CacheManager.AddItem(capcode);
				}
			}

			// return the capcode
			return capcode;
		}

		/// <summary>
		/// Saves the details of the capcode to the database.
		/// </summary>
		/// <param name="capcode">The capcode to save.</param>
		public async Task Save(Capcode capcode)
		{

			// If the capcode is null, throw exception
			if (capcode == null)
			{
				throw new ArgumentNullException("capcode");
			}

			// Set the updated 
			capcode.Updated = DateTime.UtcNow;

			// Save the capcode
			await _repository.Save(capcode);

			// Clear the cache items
			CacheManager.RemoveItem(AllCapcodesCacheKey);
			CacheManager.RemoveEntity(capcode);

		}

		/// <summary>
		/// Finds capcodes by name or short name. This is a text based search and will match any of the words in the capcode name.
		/// </summary>
		/// <param name="name">The name of the capcode to search for.</param>
		/// <returns>The list of capcodes that match against the name.</returns>
		public async Task<IList<Capcode>> FindByName(string name)
		{
			return await _repository.FindByName(name);
		}

	}
}
