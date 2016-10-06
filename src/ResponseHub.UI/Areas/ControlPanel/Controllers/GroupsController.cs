using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Areas.Admin.Models.Groups;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.Common.Constants;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{

	[RouteArea("control-panel")]
	[RoutePrefix("groups")]
	[ClaimsAuthorize(Roles = RoleTypes.GroupAdministrator)]
	public class GroupsController : BaseControlPanelController
    {

		[Route]
        // GET: ControlPanel/Groups
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
		
		#region View group

		[Route("{id:guid}")]
		[HttpGet]
		public async Task<ActionResult> ViewGroup(Guid id)
		{

			// If the current user is not a group admin of the specified group, error out.
			if (await CurrentUserIsAdminOfGroup(id) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await GetViewGroupViewResult(id, "~/Areas/ControlPanel/Views/Groups/ViewGroup.cshtml");
		}

		#endregion

		#region Edit group

		[Route("{id:guid}/edit")]
		[HttpGet]
		public async Task<ActionResult> Edit(Guid id)
		{
			return await GetEditGroupViewResult(id);
		}

		[Route("{id:guid}/edit")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<ActionResult> Edit(Guid id, CreateGroupModel model)
		{
			return await PostEditGroupViewResult(id, model);
		}

		#endregion
		
		#region New user

		[Route("{groupId:guid}/add-member")]
		[HttpGet]
		public async Task<ActionResult> AddMember(Guid groupId)
		{
			return await GetAddMemberViewResult(groupId);
		}


		[Route("{groupId:guid}/add-member")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> AddMember(Guid groupId, NewUserViewModel model)
		{
			return await PostAddMemberViewResult(groupId, model);
		}

		[Route("{groupId:guid}/confirm-member")]
		[HttpGet]
		public async Task<ActionResult> ConfirmMember(Guid groupId)
		{
			return await GetConfirmMemberViewResult(groupId);
		}

		[Route("{groupId:guid}/confirm-member")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ConfirmMember(Guid groupId, ConfirmUserViewModel model)
		{
			return await PostConfirmMemberViewResult(groupId, model);
		}

		#endregion

	}
}