using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Upkeep;
using Enivate.ResponseHub.Model.Upkeep.Interface;
using Enivate.ResponseHub.DataAccess.Interface;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class UpkeepService : IUpkeepService
	{

		private IUpkeepRepository _repository;

		public UpkeepService(IUpkeepRepository repository)
		{
			_repository = repository;
		}

		/// <summary>
		/// Creates a new asset in the database.
		/// </summary>
		/// <param name="name">The name of the asset.</param>
		/// <param name="description">A description for the asset.</param>
		/// <param name="unitId">The id of the unit the asset belongs to.</param>
		/// <returns>The created asset.</returns>
		public async Task<Asset> CreateAsset(string name, string description, Guid unitId)
		{
			// Create the model
			Asset asset = new Asset
			{
				Name = name,
				Description = description,
				UnitId = unitId
			};

			// Create the asset
			await _repository.AddAsset(asset);

			// reutrn the asset
			return asset;

		}

		/// <summary>
		/// Gets an asset from the database based on the id.
		/// </summary>
		/// <param name="id">The id of the asset to get from the database.</param>
		/// <returns>The asset based on the id.</returns>
		public async Task<Asset> GetAssetById(Guid id)
		{
			return await _repository.GetAssetById(id);
		}

		/// <summary>
		/// Gets the list of assets for the specific unit id. 
		/// </summary>
		/// <param name="unitId">The Id of the unit to get the assets for.</param>
		/// <returns>The list of assets for the unit.</returns>
		public async Task<IList<Asset>> GetAssetsByUnitId(Guid unitId)
		{
			return await _repository.GetAssetsByUnitId(unitId);
		}
	}
}
