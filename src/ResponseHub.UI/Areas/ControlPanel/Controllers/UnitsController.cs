using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Areas.Admin.Models.Units;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.Common.Constants;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{

	[RouteArea("ControlPanel", AreaPrefix ="control-panel")]
	[RoutePrefix("units")]
	[ClaimsAuthorize(Roles = RoleTypes.UnitAdministrator)]
	public class UnitsController : BaseControlPanelController
    {

		[Route]
        // GET: ControlPanel/Units
        public ActionResult Index()
        {
			Guid unitId = GetControlPanelUnitId();
			return new RedirectResult(String.Format("/control-panel/units/{0}", unitId));
		}
		
		#region View unit

		[Route("{id:guid}")]
		[HttpGet]
		public async Task<ActionResult> ViewUnit(Guid id)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(id) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await GetViewUnitViewResult(id, "~/Areas/ControlPanel/Views/Units/ViewUnit.cshtml");
		}

		#endregion

		#region Edit unit

		[Route("{id:guid}/edit")]
		[HttpGet]
		public async Task<ActionResult> Edit(Guid id)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(id) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await GetEditUnitViewResult(id, "~/Areas/ControlPanel/Views/Units/Edit.cshtml");
		}

		[Route("{id:guid}/edit")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<ActionResult> Edit(Guid id, CreateUnitModel model)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(id) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await PostEditUnitViewResult(id, model, "~/Areas/ControlPanel/Views/Units/Edit.cshtml", false);
		}

		#endregion
		
		#region New user

		[Route("{unitId:guid}/add-member")]
		[HttpGet]
		public async Task<ActionResult> AddMember(Guid unitId)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(unitId) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await GetAddMemberViewResult(unitId);
		}


		[Route("{unitId:guid}/add-member")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> AddMember(Guid unitId, NewUserViewModel model)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(unitId) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await PostAddMemberViewResult(unitId, model);
		}

		[Route("{unitId:guid}/confirm-member")]
		[HttpGet]
		public async Task<ActionResult> ConfirmMember(Guid unitId)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(unitId) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await GetConfirmMemberViewResult(unitId, "~/Areas/ControlPanel/Views/Units/ConfirmUser.cshtml");
		}

		[Route("{unitId:guid}/confirm-member")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ConfirmMember(Guid unitId, ConfirmUserViewModel model)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(unitId) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await PostConfirmMemberViewResult(unitId, model, "~/Areas/ControlPanel/Views/Units/ConfirmUser.cshtml");
		}

		#endregion

		#region Remove User From Unit

		[Route("{unitId:guid}/remove-user/{userId:guid}")]
		public async Task<ActionResult> RemoveUserFromUnit(Guid unitId, Guid userId)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(unitId) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await GetRemoveUserFromUnit(unitId, userId, "control-panel");
		}

		#endregion

		#region Change User Role

		[Route("{unitId:guid}/change-role/{userId:guid}")]
		[HttpGet]
		public async Task<ActionResult> ChangeUserRole(Guid unitId, Guid userId)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(unitId) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return GetChangeUserRoleViewResult(unitId, userId, "~/Areas/ControlPanel/Views/Units/ChangeUserRole.cshtml");
		}

		[Route("{unitId:guid}/change-role/{userId:guid}")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ChangeUserRole(Guid unitId, Guid userId, ChangeUserRoleViewModel model)
		{
			
			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(unitId) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await ViewChangeUserRoleViewResult(unitId, userId, model, "~/Areas/ControlPanel/Views/Units/ChangeUserRole.cshtml", "control-panel");
		}

		#endregion

	}
}