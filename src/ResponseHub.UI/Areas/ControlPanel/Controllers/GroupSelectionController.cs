using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common.Constants;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Home;
using Enivate.ResponseHub.UI.Filters;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{

	[RouteArea("ControlPanel", AreaPrefix = "control-panel")]
	[RoutePrefix("group-selection")]
	[ClaimsAuthorize(Roles = RoleTypes.GroupAdministrator)]
	public class GroupSelectionController : BaseControlPanelController
	{

		[Route]
		// GET: Admin/GroupSelection
		public async Task<ActionResult> Index()
        {
			// Get the group ids that the user is a group administrator of
			IList<Guid> groupIds = await GetGroupIdsUserIsGroupAdminOf();

			// Get the groups based on the ids
			IList<Group> groups = await GroupService.GetByIds(groupIds);
			groups = groups.OrderBy(i => i.Name).ToList();

			// Create the dictionary of group ids and names
			IDictionary<Guid, string> groupDictionary = new Dictionary<Guid, string>();
			foreach (Group group in groups)
			{
				groupDictionary.Add(group.Id, group.Name);
			}

			GroupSelectionViewModel model = new GroupSelectionViewModel()
			{
				AvailableGroups = groupDictionary
			};

			return View(model);
		}

		[Route]
		[ClaimsAuthorize(Roles = RoleTypes.GroupAdministrator)]
		[HttpPost]
		// GET: ControlPanel/Home
		public async Task<ActionResult> Index(GroupSelectionViewModel model)
		{

			// Get the group ids that the user is a group administrator of
			IList<Guid> groupIds = await GetGroupIdsUserIsGroupAdminOf();

			// Get the groups based on the ids
			IList<Group> groups = await GroupService.GetByIds(groupIds);
			groups = groups.OrderBy(i => i.Name).ToList();

			// Create the dictionary of group ids and names
			IDictionary<Guid, string> groupDictionary = new Dictionary<Guid, string>();
			foreach (Group group in groups)
			{
				groupDictionary.Add(group.Id, group.Name);
			}

			model.AvailableGroups = groupDictionary;

			// If the model state is invalid, show error messages
			if (!ModelState.IsValid)
			{
				return View("GroupContextSelection", model);
			}

			Session[SessionConstants.ControlPanelContextGroupId] = model.GroupId;
			return new RedirectResult(String.Format("/control-panel/groups/{0}", model.GroupId));
		}
	}
}