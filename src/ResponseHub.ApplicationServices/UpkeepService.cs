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
		/// Marks the specified asset as deleted within the database. Cannot delete the asset as it be needed for reporting. 
		/// </summary>
		/// <param name="assetId">The ID of the asset to mark for deletion. </param>
		/// <returns></returns>
		public async Task DeleteAsset(Guid assetId)
		{
			await _repository.DeleteAsset(assetId);
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

		/// <summary>
		/// Marks the specified task as deleted within the database. Cannot delete the task as it be needed for reporting. 
		/// </summary>
		/// <param name="taskId">The ID of the task to mark for deletion. </param>
		/// <returns></returns>
		public async Task DeleteTask(Guid taskId)
		{
			await _repository.DeleteTask(taskId);
		}

		/// <summary>
		/// Creates a new upkeep report.
		/// </summary>
		/// <param name="name">The name of the report.</param>
		/// <param name="created">The date and time the report was created.</param>
		/// <param name="createdBy">The Id of the user who created the report.</param>
		/// <param name="tasks">The list of tasks to set for the report.</param>
		/// <returns>The newly created upkeep report item</returns>
		public async Task<UpkeepReport> CreateNewReport(string name, DateTime created, Guid createdBy, IList<UpkeepTask> tasks, Guid unitId)
		{
			// Create the object
			UpkeepReport report = new UpkeepReport
			{
				Created = created,
				CreatedBy = createdBy,
				Name = name,
				UnitId = unitId
			};

			// Map the tasks to the ReportTask class
			foreach(UpkeepTask task in tasks)
			{
				report.Tasks.Add(await ReportTaskFromUpkeepTask(task));
			}

			// Save the report
			await _repository.SaveReport(report);

			// return the saved report
			return report;

		}

		/// <summary>
		/// Maps the UpkeepTask to the ReportClass object.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public async Task<ReportTask> ReportTaskFromUpkeepTask(UpkeepTask task)
		{

			// If the task is null, throw exception
			if (task == null)
			{
				throw new ArgumentNullException("task");
			}

			// Create the report task
			ReportTask reportTask = new ReportTask
			{
				TaskId = task.Id,
				Name = task.Name
			};

			// Set the items
			foreach (string item in task.TaskItems)
			{
				reportTask.TaskItems.Add(new ReportItem() { Name = item });
			}

			// If there is an asset, then get that and convert to a report asset object
			if (task.AssetId.HasValue && task.AssetId.Value != Guid.Empty)
			{
				// Get the asset
				Asset asset = await GetAssetById(task.AssetId.Value);

				// If the asset is not null, map to a ReportAsset item
				if (asset != null)
				{
					reportTask.Asset = MapAssetToReportAsset(asset);
				}

			}

			// return the report task
			return reportTask;
		}

		/// <summary>
		/// Maps the asset class to the ReportAsset class. 
		/// </summary>
		/// <param name="asset"></param>
		/// <returns></returns>
		private ReportAsset MapAssetToReportAsset(Asset asset)
		{
			ReportAsset reportAsset = new ReportAsset
			{
				AssetId = asset.Id,
				Name = asset.Name,
				Description = asset.Description
			};

			// Map the inventory
			reportAsset.Inventory = new ReportInventory
			{
				Containers = MapContainersToReportContainer(asset.Inventory.Containers),
				Items = MapItemsToReportItems(asset.Inventory.Items)
			};

			// return the report asset
			return reportAsset;
		}

		/// <summary>
		/// Maps the Container class to the ReportContainer class. 
		/// </summary>
		/// <param name="containers"></param>
		/// <returns></returns>
		private IList<ReportContainer> MapContainersToReportContainer(IList<Container> containers)
		{

			// create the list
			IList<ReportContainer> reportContainers = new List<ReportContainer>();

			// Loop through the containers
			foreach (Container container in containers)
			{

				// Create the container
				ReportContainer reportContainer = new ReportContainer
				{
					Name = container.Name
				};

				// Set the child containers
				reportContainer.Containers = MapContainersToReportContainer(container.Containers);

				// set the items
				reportContainer.Items = MapItemsToReportItems(container.Items);

				// add the report container to the list
				reportContainers.Add(reportContainer);

			}

			// return the list
			return reportContainers;
		}

		/// <summary>
		/// Maps the CatalogItem class to the ReportItem class.
		/// </summary>
		/// <param name="items"></param>
		/// <returns></returns>
		private IList<ReportItem> MapItemsToReportItems(IList<CatalogItem> items)
		{
			// create the list
			IList<ReportItem> reportItems = new List<ReportItem>();

			// Loop through the containers
			foreach (CatalogItem item in items)
			{
				reportItems.Add(new ReportItem
				{
					Name = item.Name,
					Quantity = item.Quantity
				});
			}

			return reportItems;

		}
	}

}
