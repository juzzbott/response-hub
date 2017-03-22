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

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{

	[RouteArea("control-panel")]
    public class HomeController : BaseControlPanelController
    {

		[Route]
		[ClaimsAuthorize(Roles = RoleTypes.GroupAdministrator)]
		// GET: ControlPanel/Home
		public async Task<ActionResult> Index()
        {

			// Get the group ids that the user is a group administrator of
			IList<Guid> groupIds = await GetGroupIdsUserIsGroupAdminOf();

			// If there is 1 group, add the context group id to the session
			if (groupIds.Count == 1)
			{
				Guid groupId = groupIds.First();
				Session[SessionConstants.GroupAdminContextGroupId] = groupId;
				return new RedirectResult(String.Format("/control-panel/groups/{0}", groupId));
			}
			else
			{
				return View();
			}

        }
    }
}