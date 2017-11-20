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
			await _repository.SaveAsset(asset);

			// reutrn the asset
			return asset;

		}

		/// <summary>
		/// Saves the asset to the database.
		/// </summary>
		/// <param name="asset">The asset to save to the datbase.</param>
		/// <returns>The saved asset object.</returns>
		public async Task<Asset> SaveAsset(Asset asset)
		{
			// Create the asset
			await _repository.SaveAsset(asset);

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

		/// <summary>
		/// Saves the new task to the database
		/// </summary>
		/// <param name="name">The name of the task.</param>
		/// <param name="unitId">The ID of the unit the task belongs to.</param>
		/// <param name="assetId">The Id of the asset (if any) that the task refers to.</param>
		/// <param name="taskItems">The task items for the current task.</param>
		/// <returns></returns>
		public async Task<UpkeepTask> CreateTask(string name, Guid unitId, Guid? assetId, IList<string> taskItems)
		{

			// Create the upkeep task
			UpkeepTask task = new UpkeepTask
			{
				AssetId = assetId,
				Name = name,
				UnitId = unitId,
				TaskItems = taskItems
			};

			// Create the task
			await _repository.SaveTask(task);

			// reutrn the task
			return task;
		}

		/// <summary>
		/// Saves the task to the database.
		/// </summary>
		/// <param name="task">The task to save to the database</param>
		/// <returns></returns>
		public async Task<UpkeepTask> SaveTask(UpkeepTask task)
		{

			// Create the task
			await _repository.SaveTask(task);

			// reutrn the task
			return task;
		}

		/// <summary>
		/// Gets the specified task from the database based on the id. 
		/// </summary>
		/// <param name="id">The id of the task to return.</param>
		/// <returns>The task if it found, otherwise null.</returns>
		public async Task<UpkeepTask> GetTaskById(Guid id)
		{
			return await _repository.GetTaskById(id);
		}

		/// <summary>
		/// Gets the collection of tasks based on the unit id.
		/// </summary>
		/// <param name="unitId">The ID of the unit to get the tasks for.</param>
		/// <returns>The collection of tasks for the unit.</returns>
		public async Task<IList<UpkeepTask>> GetTasksByUnitId(Guid unitId)
		{
			return await _repository.GetTasksByUnitId(unitId);
		}
	}
}
