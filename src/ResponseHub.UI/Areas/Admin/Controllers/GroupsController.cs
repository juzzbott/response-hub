using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Identity.Interface;
using Enivate.ResponseHub.Model.Spatial;
using Enivate.ResponseHub.UI.Areas.Admin.Models.Groups;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.UI.Models.Users;
using Enivate.ResponseHub.UI.Controllers;
using Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers;

namespace Enivate.ResponseHub.UI.Areas.Admin.Controllers
{

	[RouteArea("admin")]
	[RoutePrefix("groups")]
	[ClaimsAuthorize(Roles = RoleTypes.SystemAdministrator)]
	public class GroupsController : BaseAdminController
	{

		private const string CreateGroupViewModelSesionKey = "CreateGroupViewModel";

		[Route]
		// GET: Admin/Groups
		public async Task<ActionResult> Index()
		{

			List<Group> groups = new List<Group>();

			// If there is no search term, return all results, otherwise return only those that match the search results.
			if (String.IsNullOrEmpty(Request.QueryString["q"]))
			{
				// Get the most recent groups
				groups.AddRange(await GroupService.GetRecentlyAdded(30));
			}
			else
			{
				groups.AddRange(await GroupService.FindByName(Request.QueryString["q"]));
			}

			return View(groups);
		}

		#region Create group

		[Route("create")]
		public async Task<ActionResult> Create()
		{

			// Set the form action and the addGroupAdministrator flag.
			ViewBag.AddGroupAdministrator = true;
			ViewBag.FormAction = "/admin/groups/create";
			ViewBag.Title = "Create new group";

			CreateGroupModel model = new CreateGroupModel();
			model.AvailableRegions = await GetAvailableRegions();
			model.AvailableGroupCapcodes = await CapcodeService.GetAllByGroupOnly(true);
			model.AvailableAdditionalCapcodes = await CapcodeService.GetAllByGroupOnly(false);

			return View("CreateEdit", model);
		}

		[Route("create")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create(CreateGroupModel model)
		{

			// Set the form action and the addGroupAdministrator flag.
			ViewBag.AddGroupAdministrator = true;
			ViewBag.FormAction = "/admin/groups/create";
			ViewBag.Title = "Create new group";

			// Get the regions select list.
			model.AvailableRegions = await GetAvailableRegions();
			model.AvailableGroupCapcodes = await CapcodeService.GetAllByGroupOnly(true);
			model.AvailableAdditionalCapcodes = await CapcodeService.GetAllByGroupOnly(false);

			// If the model is not valid, return view.
			if (!ModelState.IsValid)
			{
				return View("CreateEdit", model);
			}

			// Get the service type.
			int intServiceType;
			Int32.TryParse(model.Service, out intServiceType);
			ServiceType serviceType = (ServiceType)intServiceType;

			// Ensure the group name/service combination is unique
			bool groupExists = await GroupService.CheckIfGroupExists(model.Name, serviceType);

			// If the group exists, then display the group exists message
			if (groupExists)
			{
				ModelState.AddModelError("", "Sorry, there is already a group by that name in the selected service.");
				return View("CreateEdit", model);
			}

			// Store the CreateGroupViewModel in session for the next screen
			Session[CreateGroupViewModelSesionKey] = model;

			// Redirect to the group administrator screen
			return new RedirectResult("/admin/groups/create/group-administrator");

		}

		[Route("create/group-administrator")]
		public async Task<ActionResult> GroupAdministrator()
		{

			// Set the form action
			ViewBag.FormAction = "/admin/groups/create/group-administrator";

			// Get the CreateGroupViewModel from session
			CreateGroupModel model = (CreateGroupModel)Session[CreateGroupViewModelSesionKey];

			// If the view model is null, redirect back to the create screen
			if (model == null)
			{
				return new RedirectResult("/admin/groups/create");
			}

			// Get the identity user related to the specified group admin
			IdentityUser groupAdminUser = await UserService.FindByEmailAsync(model.GroupAdministratorEmail);

			// Set the role as Group Administrator by default.
			model.GroupAdministrator.Role = RoleTypes.GroupAdministrator;

			// Set the Email address regardless of if the user exists or not, as it was set in the previous screen.
			model.GroupAdministrator.EmailAddress = model.GroupAdministratorEmail;

			// If there is a group user, then add to the model.
			if (groupAdminUser != null)
			{
				model.GroupAdministrator.FirstName = groupAdminUser.FirstName;
				model.GroupAdministrator.Surname = groupAdminUser.Surname;
				model.GroupAdministrator.UserExists = true;
			}

			return View("ConfirmUser", model.GroupAdministrator);
		}

		[Route("create/group-administrator")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<ActionResult> GroupAdministrator(ConfirmUserViewModel model)
		{

			// Set the form action
			ViewBag.FormAction = "/admin/groups/create/group-administrator";
			ViewBag.SubmitButtonTitle = "Create group";

			// If the model is not valid, return view.
			if (!ModelState.IsValid)
			{
				return View("ConfirmUser", model);
			}

			// If there is "System Administrator" in the role list, show error message
			if (model.Role.Equals(RoleTypes.SystemAdministrator, StringComparison.CurrentCultureIgnoreCase))
			{
				ModelState.AddModelError("", "There was an error setting the role for the user.");
				return View("ConfirmUser", model);
			}

			// Get the CreateGroupViewModel from session
			CreateGroupModel createGroupModel = (CreateGroupModel)Session[CreateGroupViewModelSesionKey];

			// If the view model is null, redirect back to the create screen
			if (createGroupModel == null)
			{
				return new RedirectResult("/admin/groups/create");
			}

			// Store the group administrator
			IdentityUser groupAdmin = null;

			// If the group administrator user does not exist, then create the user now
			if (!model.UserExists)
			{

				// Create the new user
				groupAdmin = await UserService.CreateAsync(model.EmailAddress, model.FirstName, model.Surname, new List<string>() { RoleTypes.GroupAdministrator, RoleTypes.GeneralUser });

				// Send the email to the user
				await MailService.SendAccountActivationEmail(groupAdmin);

			}
			else
			{
				// Get the identity user related to the specified group admin
				groupAdmin = await UserService.FindByEmailAsync(createGroupModel.GroupAdministratorEmail);

				// If the group admin user is null, return an error, otherwise set the group admin id.
				if (groupAdmin == null)
				{
					ModelState.AddModelError("", "There was a system error creating the group.");
					await Log.Error(String.Format("Unable to create group. Existing user with email ''  could not be found.", createGroupModel.GroupAdministratorEmail));
					return View("ConfirmUser", model);
				}
			}

			// Get the service type from the model
			int groupServiceId;
			Int32.TryParse(createGroupModel.Service, out groupServiceId);
			ServiceType service = (ServiceType)groupServiceId;

			// Get the region based on the posted value
			IList<Region> regions = await GroupService.GetRegions();
			Region region = regions.FirstOrDefault(i => i.Id == createGroupModel.Region);

			// If the region is null, log the error and return error message
			if (region == null)
			{
				await Log.Error(String.Format("Unable to create group. Region '{0}' does not exist.", createGroupModel.Region));
				ModelState.AddModelError("", "There was a system error creating the group.");
				return View("ConfirmUser", model);
			}

			// Create the headquarters coords
			Coordinates coords = new Coordinates(createGroupModel.Latitude.Value, createGroupModel.Longitude.Value);

			// Get the list of additional capcodes
			IList<Guid> additionalCapcodes = GetCapcodeIdsFromHiddenValue(createGroupModel.AdditionalCapcodes);

			// Create the capcode if it doesn't exist.
			await CheckAndCreateCapcode(createGroupModel.Capcode, createGroupModel.Name, service);

			// Create the group
			await GroupService.CreateGroup(createGroupModel.Name, service, createGroupModel.Capcode, additionalCapcodes, groupAdmin.Id, createGroupModel.Description, region, coords);

			// Send the new group email to the group admin
			await MailService.SendGroupCreatedEmail(groupAdmin, createGroupModel.Name, service, createGroupModel.Capcode);

			// Clear the session url
			Session.Remove(CreateGroupViewModelSesionKey);

			// redirect back to group index page
			return new RedirectResult("/admin/groups?group_created=1");
		}

		#endregion

		#region View group

		[Route("{id:guid}")]
		[HttpGet]
		public async Task<ActionResult> ViewGroup(Guid id)
		{
			return await GetViewGroupViewResult(id, "~/Areas/Admin/Views/Groups/ViewGroup.cshtml");		
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
			return await GetConfirmMemberViewResult(groupId, "~/Areas/Admin/Views/Groups/ConfirmUser.cshtml");
		}

		[Route("{groupId:guid}/confirm-member")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ConfirmMember(Guid groupId, ConfirmUserViewModel model)
		{
			return await PostConfirmMemberViewResult(groupId, model, "~/Areas/Admin/Views/Groups/ConfirmUser.cshtml");		
		}

		#endregion

		#region Edit group

		[Route("{id:guid}/edit")]
		[HttpGet]
		public async Task<ActionResult> Edit(Guid id)
		{
			return await GetEditGroupViewResult(id, "~/Areas/Admin/Views/Groups/CreateEdit.cshtml");			
		}

		[Route("{id:guid}/edit")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<ActionResult> Edit(Guid id, CreateGroupModel model)
		{
			return await PostEditGroupViewResult(id, model, "~/Areas/Admin/Views/Groups/CreateEdit.cshtml", true);
		}
		
		#endregion

		#region Change User Role

		[Route("{groupId:guid}/change-role/{userId:guid}")]
		[HttpGet]
		public ActionResult ChangeUserRole(Guid groupId, Guid userId)
		{
			return GetChangeUserRoleViewResult(groupId, userId, "~/Areas/Admin/Views/Groups/ChangeUserRole.cshtml");
		}

		[Route("{groupId:guid}/change-role/{userId:guid}")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ChangeUserRole(Guid groupId, Guid userId, ChangeUserRoleViewModel model)
		{
			return await ViewChangeUserRoleViewResult(groupId, userId, model, "~/Areas/Admin/Views/Groups/ChangeUserRole.cshtml", "admin");
		}

		#endregion
		
	}
}