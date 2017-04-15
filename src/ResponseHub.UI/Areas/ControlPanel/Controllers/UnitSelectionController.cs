using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common.Constants;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Home;
using Enivate.ResponseHub.UI.Filters;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{

	[RouteArea("ControlPanel", AreaPrefix = "control-panel")]
	[RoutePrefix("unit-selection")]
	[ClaimsAuthorize(Roles = RoleTypes.UnitAdministrator)]
	public class UnitSelectionController : BaseControlPanelController
	{

		[Route]
		// GET: Admin/UnitSelection
		public async Task<ActionResult> Index()
        {
			// Get the unit ids that the user is a unit administrator of
			IList<Guid> unitIds = await UnitService.GetUnitIdsUserIsUnitAdminOf(UserId);

			// If there is only one unit, when we just need to set this and return the user.
			if (unitIds.Count == 1)
			{
				return SetUnitIdAndRedirect(unitIds[0]);
			}

			// Get the units based on the ids
			IList<Unit> units = await UnitService.GetByIds(unitIds);
			units = units.OrderBy(i => i.Name).ToList();

			// Create the dictionary of unit ids and names
			IDictionary<Guid, string> unitDictionary = new Dictionary<Guid, string>();
			foreach (Unit unit in units)
			{
				unitDictionary.Add(unit.Id, unit.Name);
			}

			UnitSelectionViewModel model = new UnitSelectionViewModel()
			{
				AvailableUnits = unitDictionary
			};

			return View(model);
		}

		[Route]
		[ClaimsAuthorize(Roles = RoleTypes.UnitAdministrator)]
		[HttpPost]
		// GET: ControlPanel/Home
		public async Task<ActionResult> Index(UnitSelectionViewModel model)
		{

			// Get the unit ids that the user is a unit administrator of
			IList<Guid> unitIds = await UnitService.GetUnitIdsUserIsUnitAdminOf(UserId);

			// Get the units based on the ids
			IList<Unit> units = await UnitService.GetByIds(unitIds);
			units = units.OrderBy(i => i.Name).ToList();

			// Create the dictionary of unit ids and names
			IDictionary<Guid, string> unitDictionary = new Dictionary<Guid, string>();
			foreach (Unit unit in units)
			{
				unitDictionary.Add(unit.Id, unit.Name);
			}

			model.AvailableUnits = unitDictionary;

			// If the model state is invalid, show error messages
			if (!ModelState.IsValid)
			{
				return View("UnitContextSelection", model);
			}


			return SetUnitIdAndRedirect(model.UnitId);
		}

		/// <summary>
		/// Set the session id and return the specific action result.
		/// </summary>
		/// <param name="unitId"></param>
		/// <returns></returns>
		private ActionResult SetUnitIdAndRedirect(Guid unitId)
		{
			// Set the session context unit id
			Session[SessionConstants.ControlPanelContextUnitId] = unitId;

			// If there is a return url, then redirect back, otherwise just return to the unit screen
			if (!String.IsNullOrEmpty(Request.QueryString["return_url"]))
			{
				string returnUrl = Server.UrlDecode(Request.QueryString["return_url"]);
				return new RedirectResult(returnUrl);
			}
			else
			{
				return new RedirectResult(String.Format("/control-panel/units/{0}", unitId));
			}
		}
	}
}