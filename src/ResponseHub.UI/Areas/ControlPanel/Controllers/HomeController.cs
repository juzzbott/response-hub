using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common.Constants;
using Enivate.ResponseHub.UI.Controllers;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Home;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{

	[RouteArea("ControlPanel", AreaPrefix = "control-panel")]
	public class HomeController : BaseControlPanelController
	{

		[Route]
		[ClaimsAuthorize(Roles = RoleTypes.UnitAdministrator)]
		// GET: ControlPanel/Home
		public async Task<ActionResult> Index()
		{

			// Get the unit ids that the user is a unit administrator of
			IList<Guid> unitIds = await UnitService.GetUnitIdsUserIsUnitAdminOf(UserId);

			// If there is 1 unit, add the context unit id to the session
			if (unitIds.Count == 1)
			{
				Guid unitId = unitIds.First();
				Session[SessionConstants.ControlPanelContextUnitId] = unitId;
				return new RedirectResult(String.Format("/control-panel/units/{0}", unitId));
			}
			else
			{
				return new RedirectResult("/control-panel/unit-selection");
			}

		}
	}
}