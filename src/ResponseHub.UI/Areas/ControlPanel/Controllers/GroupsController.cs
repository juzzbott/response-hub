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

	[RouteArea("ControlPanel", AreaPrefix ="control-panel")]
	[RoutePrefix("groups")]
	[ClaimsAuthorize(Roles = RoleTypes.GroupAdministrator)]
	public class GroupsController : BaseControlPanelController
    {

		[Route]
        // GET: ControlPanel/Groups
        public ActionResult Index()
        {
			Guid groupId = GetControlPanelGroupId();
			return new RedirectResult(String.Format("/control-panel/groups/{0}", groupId));
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
			return await GetEditGroupViewResult(id, "~/Areas/ControlPanel/Views/Groups/Edit.cshtml");
		}

		[Route("{id:guid}/edit")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<ActionResult> Edit(Guid id, CreateGroupModel model)
		{
			return await PostEditGroupViewResult(id, model, "~/Areas/ControlPanel/Views/Groups/Edit.cshtml", false);
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
			return await GetConfirmMemberViewResult(groupId, "~/Areas/ControlPanel/Views/Groups/ConfirmUser.cshtml");
		}

		[Route("{groupId:guid}/confirm-member")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ConfirmMember(Guid groupId, ConfirmUserViewModel model)
		{
			return await PostConfirmMemberViewResult(groupId, model, "~/Areas/ControlPanel/Views/Groups/ConfirmUser.cshtml");
		}

		#endregion

		#region Remove User From Group

		[Route("{groupId:guid}/remove-user/{userId:guid}")]
		public async Task<ActionResult> RemoveUserFromGroup(Guid groupId, Guid userId)
		{
			return await GetRemoveUserFromGroup(groupId, userId, "control-panel");
		}

		#endregion

		#region Change User Role

		[Route("{groupId:guid}/change-role/{userId:guid}")]
		[HttpGet]
		public ActionResult ChangeUserRole(Guid groupId, Guid userId)
		{
			return GetChangeUserRoleViewResult(groupId, userId, "~/Areas/ControlPanel/Views/Groups/ChangeUserRole.cshtml");
		}

		[Route("{groupId:guid}/change-role/{userId:guid}")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ChangeUserRole(Guid groupId, Guid userId, ChangeUserRoleViewModel model)
		{
			return await ViewChangeUserRoleViewResult(groupId, userId, model, "~/Areas/ControlPanel/Views/Groups/ChangeUserRole.cshtml", "control-panel");
		}

		#endregion

	}
}