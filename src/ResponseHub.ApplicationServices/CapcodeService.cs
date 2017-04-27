using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Model.Units;
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

		/// <summary>
		/// THe service for handling units.
		/// </summary>
		private IUnitService _unitService;

		private const string AllCapcodesCacheKey = "AllCapcodes";

		/// <summary>
		/// Creates a new instance of the CapcodeService.
		/// </summary>
		/// <param name="repository"></param>
		/// <param name="log"></param>
		public CapcodeService(IUnitService unitService, ICapcodeRepository repository, ILogger log)
		{
			_unitService = unitService;
			_repository = repository;
			_log = log;
		}

		/// <summary>
		/// Gets a list of all the cap codes available in the system, not including individual unit capcodes.
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

				// If the capcodes are not null and there is at least one in the unit, then add to the cache
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
		public async Task<Capcode> Create(string name, string capcodeAddress, string shortName, ServiceType service, bool isUnitOnly)
		{

			// Create the capcode
			Capcode capcode = new Capcode()
			{
				Name = name,
				CapcodeAddress = capcodeAddress,
				ShortName = shortName,
				Service = service,
				Created = DateTime.UtcNow,
				IsUnitCapcode = isUnitOnly
			};

			// Create the capcode
			await _repository.Save(capcode);

			// Clear the "All capcodes" cache object
			CacheManager.RemoveItem(AllCapcodesCacheKey);

			// return the created capcode
			return capcode;

		}

		/// <summary>
		/// Gets the capcode from the capcode address in the database.
		/// </summary>
		/// <param name="capcodeAddress"></param>
		/// <returns></returns>
		public async Task<Capcode> GetByCapcodeAddress(string capcodeAddress)
		{
			return await _repository.GetByCapcodeAddress(capcodeAddress);
		}

		/// <summary>
		/// Get all the capcodes for the specified service. This will also return any capcodes that are "All Service".
		/// </summary>
		/// <param name="service">The service to get the capcodes for.</param>
		/// <returns></returns>
		public async Task<IList<Capcode>> GetAllByService(ServiceType service)
		{
			// Get all the capcodes
			IList<Capcode> allCapcodes = await GetAll();

			// Return all capcodes for the specified service.
			return allCapcodes.Where(i => i.Service == service || i.Service == ServiceType.AllServices).ToList();

		}

		/// <summary>
		/// Get all the capcodes for the specified service. This will also return any capcodes that are "All Service".
		/// </summary>
		/// <param name="service">The service to get the capcodes for.</param>
		/// <returns></returns>
		public async Task<IList<Capcode>> GetAllByService(ServiceType service, bool isUnitCapcode)
		{
			// Get all the capcodes
			IList<Capcode> allCapcodes = await GetAll();

			// Return all capcodes for the specified service.
			return allCapcodes.Where(i => (i.Service == service || i.Service == ServiceType.AllServices) && i.IsUnitCapcode == isUnitCapcode).ToList();

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

		/// <summary>
		/// Gets the capcodes for the current user.
		/// </summary>
		/// <param name="userId">The id of the user to get the capcodes for.</param>
		/// <returns>The list of capcodes for the user.</returns>
		public async Task<IList<Capcode>> GetCapcodesForUser(Guid userId)
		{

			// Get the units for the user
			IList<Unit> userUnits = await _unitService.GetUnitsForUser(userId);

			// If there are no units for the user, then return
			if (userUnits == null || !userUnits.Any())
			{
				return new List<Capcode>();
			}

			// Select the capcode ids
			IList<Guid> capcodeIds = userUnits.SelectMany(i => i.AdditionalCapcodes).Distinct().ToList();

			// Get the list of capcodes based on the id
			IList<Capcode> capcodes = await _repository.GetCapcodesById(capcodeIds);

			// Get the list of unit capcode objects
			IList<Capcode> unitCapcodes = await _repository.GetCapcodes(userUnits.Select(i => i.Capcode).ToList());

			// Add the unit capcodes to the list of capcodes to return.
			foreach(Capcode unitCapcode in unitCapcodes)
			{
				capcodes.Add(unitCapcode);
			}

			// return the list of capcodes for the user
			return capcodes;

		}

		/// <summary>
		/// Gets the capcodes for the specified unit.
		/// </summary>
		/// <param name="userId">The id of the unit to get the capcodes for.</param>
		/// <returns>The list of capcodes for the unit.</returns>
		public async Task<IList<Capcode>> GetCapcodesForUnit(Guid unitId)
		{

			// Get the unit based on the id
			Unit unit = await _unitService.GetById(unitId);

			// If there are no units for the user, then return
			if (unit == null)
			{
				return new List<Capcode>();
			}

			
			List<Capcode> capcodes = new List<Capcode>();

			// Get the unit capcode
			capcodes.AddRange(await _repository.GetCapcodes(new List<string> { unit.Capcode }));

			// Get the list of capcodes based on the additional capcodes list
			capcodes.AddRange(await _repository.GetCapcodesById(unit.AdditionalCapcodes));

			// return the list of capcodes for the user
			return capcodes;

		}

		/// <summary>
		/// Gets the capcodes that are not specified as Unit capcodes.
		/// </summary>
		/// <returns>The list of capcodes where IsUnitCapcode is false.</returns>
		public async Task<IList<Capcode>> GetSharedCapcodes()
		{
			return await _repository.GetSharedCapcodes();
		}

		/// <summary>
		/// Gets the capcodes that are only specified for use as Unit capcodes or not based on the unitOnly parameter.
		/// </summary>
		/// <returns>The list of capcodes where IsUnitCapcode is false.</returns>
		public async Task<IList<Capcode>> GetAllByUnitOnly(bool unitOnly)
		{
			return await _repository.GetAllByUnitOnly(unitOnly);
		}

		/// <summary>
		/// Removes the capcode based on the id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task Remove(Guid id)
		{

			// Remove the capcode.
			await _repository.Remove(id);

			// Remove the capcodes from cache to reload them
			CacheManager.RemoveItem(AllCapcodesCacheKey);
		}

	}
}
