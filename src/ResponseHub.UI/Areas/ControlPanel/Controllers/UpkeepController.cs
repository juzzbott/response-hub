using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Upkeep;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.Model.Upkeep.Interface;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Upkeep;

using Newtonsoft.Json;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{
	[RouteArea("ControlPanel", AreaPrefix = "control-panel")]
	[RoutePrefix("upkeep")]
	[ClaimsAuthorize(Roles = RoleTypes.UnitAdministrator)]
	public class UpkeepController : BaseControlPanelController
    {

		protected IUpkeepService UpkeepService = ServiceLocator.Get<IUpkeepService>();

		// GET: ControlPanel/Upkeep
		[Route]
		public ActionResult Index()
        {
            return View();
        }

		#region Assets

		[Route("assets")]
		public async Task<ActionResult> AssetsIndex()
		{

			// Get the list of assets based on the current unit id. 
			IList<ViewAssetModel> model = new List<ViewAssetModel>();

			// Get the assets based on the unit
			IList<Asset> unitAssets = await UpkeepService.GetAssetsByUnitId(GetControlPanelUnitId());

			// Loop through the assets and create the models
			foreach(Asset unitAsset in unitAssets)
			{
				model.Add(new ViewAssetModel
				{
					Id = unitAsset.Id,
					Name = unitAsset.Name,
					Description = unitAsset.Description
				});
			}

			return View("AssetsIndex", model);
		}

		[Route("assets/add")]
		public ActionResult AddAsset()
		{
			ViewBag.Title = "Add new asset";

			return View("AddAsset");
		}

		[Route("assets/add")]
		[HttpPost]
		public async Task<ActionResult> AddAsset(AddAssetModel model)
		{
			ViewBag.Title = "Add new asset";

			// If the model is invalid, return
			if (!ModelState.IsValid)
			{
				return View("AddAsset");
			}

			// Create the asset and save to the database
			Asset newAsset = await UpkeepService.CreateAsset(model.Name, model.Description, GetControlPanelUnitId());

			return new RedirectResult(String.Format("/control-panel/upkeep/assets/{0}", newAsset.Id));
		}

		[Route("assets/{id:guid}")]
		public async Task<ActionResult> ViewAsset(Guid id)
		{

			// Get the asset based on the id
			Asset asset = await UpkeepService.GetAssetById(id);

			// If the unit is null, throw 404
			if (asset == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// If there is an inventory, then deserialise it
			string inventoryJson = asset.Inventory != null ? JsonConvert.SerializeObject(asset.Inventory) : "";
			
			// Create the model from the asset
			ViewAssetModel model = new ViewAssetModel
			{
				Id = asset.Id,
				Description = asset.Description,
				Name = asset.Name,
				InventoryJson = inventoryJson
			};

			// Set the title
			ViewBag.Title = model.Name;

			// Load the asset from the database
			return View("ViewAsset", model);
		}

		[Route("assets/{id:guid}")]
		[HttpPost]
		public async Task<ActionResult> ViewAsset(Guid id, ViewAssetModel model)
		{

			// Set the title
			ViewBag.Title = model.Name;

			// Get the asset based on the id
			Asset asset = await UpkeepService.GetAssetById(id);

			// If the unit is null, throw 404
			if (asset == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// If the model state is invalid, then return
			if (!ModelState.IsValid)
			{
				return View("ViewAsset", model);
			}

			// From the JSON data, deserialise to an Inventory object
			if (!String.IsNullOrEmpty(model.InventoryJson))
			{
				Inventory inventory = JsonConvert.DeserializeObject<Inventory>(model.InventoryJson);
				asset.Inventory = inventory;

				// If there is an inventory, then deserialise it so that we get the correct case
				string inventoryJson = asset.Inventory != null ? JsonConvert.SerializeObject(asset.Inventory) : "";
				model.InventoryJson = inventoryJson;
			}
			else
			{
				model.InventoryJson = "";
			}
			asset.Description = model.Description;
			asset.Name = model.Name;

			// Save the asset
			await UpkeepService.SaveAsset(asset);

			// Load the asset from the database
			return new RedirectResult(String.Format("/control-panel/upkeep/assets/{0}", asset.Id));
		}

		#endregion

		#region Tasks

		[Route("tasks")]
		public async Task<ActionResult> TasksIndex()
		{

			// Get the list of assets based on the current unit id. 
			IList<ViewTaskViewModel> model = new List<ViewTaskViewModel>();

			// Get the assets based on the unit
			IList<Asset> unitAssets = await UpkeepService.GetAssetsByUnitId(GetControlPanelUnitId());
			
			// Get the tasks based on the unit
			IList<UpkeepTask> unitTasks = await UpkeepService.GetTasksByUnitId(GetControlPanelUnitId());

			// Loop through the assets and create the models
			foreach (UpkeepTask unitTask in unitTasks)
			{
				model.Add(MapUpkeepTaskViewModel(unitTask, unitAssets));
			}

			return View("TasksIndex", model);
		}

		[Route("tasks/add")]
		public async Task<ActionResult> AddTask()
		{

			// Create the model object
			AddTaskViewModel model = new AddTaskViewModel();

			// Set the available assets
			model.AvailableAssets = await GetAvailableAssets();

			return View("AddTask", model);
		}

		[Route("tasks/add")]
		[HttpPost]
		public async Task<ActionResult> AddTask(AddTaskViewModel model)
		{

			// Set the available assets
			model.AvailableAssets = await GetAvailableAssets();

			// If the model is invalid, return to show error messages
			if (!ModelState.IsValid)
			{
				return View("AddTask", model);
			}

			// Create the list of task items
			IList<string> taskItems = new List<string>();

			// From the JSON data, deserialise to an Inventory object
			if (!String.IsNullOrEmpty(model.TaskItemsJson))
			{
				taskItems = JsonConvert.DeserializeObject<IList<string>>(model.TaskItemsJson);
			}

			// Save the task to the database 
			await UpkeepService.CreateTask(model.Name, GetControlPanelUnitId(), model.AssetId, taskItems);

			return new RedirectResult(String.Format("/control-panel/upkeep/tasks"));
		}

		private async Task<IList<SelectListItem>> GetAvailableAssets()
		{

			// Get the assets based on the unit
			IList<Asset> unitAssets = await UpkeepService.GetAssetsByUnitId(GetControlPanelUnitId());

			// Create the list of available assets
			IList<SelectListItem> availableAssets = new List<SelectListItem>
			{
				new SelectListItem() { Text = "Select asset...", Value = ""}
			};

			// Iterate through the assets and convert to a select list item
			foreach(Asset asset in unitAssets)
			{
				availableAssets.Add(new SelectListItem
				{
					Text = asset.Name,
					Value = asset.Id.ToString()
				});
			}

			// return the list of available assets
			return availableAssets;

		}

		#endregion

		#region Helpers

		/// <summary>
		/// Maps an UpkeepTask object to the ViewTaskViewModel class type.
		/// </summary>
		/// <param name="task">The task to map to the view model.</param>
		/// <param name="unitAssets">The list of unit assets.</param>
		/// <returns></returns>
		private ViewTaskViewModel MapUpkeepTaskViewModel(UpkeepTask task, IList<Asset> unitAssets)
		{
			return new ViewTaskViewModel
			{
				Id = task.Id,
				AssetId = task.AssetId,
				TaskItems = task.TaskItems,
				Name = task.Name,
				Asset = unitAssets.FirstOrDefault(i => i.Id == task.AssetId),
				TaskItemJson = JsonConvert.SerializeObject(task.TaskItems)
			};
		}

		#endregion

	}
}