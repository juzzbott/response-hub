using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common.Constants;
using Enivate.ResponseHub.UI.Controllers;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{

	[RouteArea("control-panel")]
    public class HomeController : BaseControlPanelController
    {

		[Route]
        // GET: ControlPanel/Home
        public ActionResult Index()
        {

			// Get the group ids that the user is a group administrator of
			IList<Guid> groupIds = GetGroupIdsUserIsGroupAdmin();

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