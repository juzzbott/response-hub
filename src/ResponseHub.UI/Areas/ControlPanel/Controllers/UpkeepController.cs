using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.ControlPanel.Admin.Models.Upkeep;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.Model.Upkeep.Interface;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Upkeep;
using System.Net;
using Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Upkeep;

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

			// Create the model from the asset
			ViewAssetModel model = new ViewAssetModel
			{
				Id = asset.Id,
				Description = asset.Description,
				Name = asset.Name
			};

			// Set the title
			ViewBag.Title = model.Name;

			// Load the asset from the database
			return View("ViewAsset", model);
		}

		#endregion
	}
}