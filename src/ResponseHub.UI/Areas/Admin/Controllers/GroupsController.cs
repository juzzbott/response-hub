using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Groups.Interface;

using Enivate.ResponseHub.ApplicationServices.Identity;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Identity.Interface;

using Enivate.ResponseHub.UI.Areas.Admin.Models.Groups;
using Enivate.ResponseHub.UI.Areas.Admin.Models.Users;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.UI.Models.Users;

namespace Enivate.ResponseHub.UI.Areas.Admin.Controllers
{

	[RouteArea("admin")]
	[RoutePrefix("groups")]
	[ClaimsAuthorize(Roles = RoleTypes.SystemAdministrator)]
    public class GroupsController : Controller
    {

		private const string CreateGroupViewModelSesionKey = "CreateGroupViewModel";

		private const string AddMemberViewModelSessionKey = "NewUserViewModel";

		private IGroupService _groupService;
		protected IGroupService GroupService
		{
			get
			{
				return _groupService ?? (_groupService = UnityConfiguration.Container.Resolve<IGroupService>());
			}
		}

		private IUserService _userService;
		protected IUserService UserService
		{
			get
			{
				return _userService ?? (_userService = UnityConfiguration.Container.Resolve<IUserService>());
			}
		}

		private ILogger _log;
		protected ILogger Log
		{
			get
			{
				return _log ?? (_log = UnityConfiguration.Container.Resolve<ILogger>());
			}
		}

		[Route]
        // GET: Admin/Groups
        public async Task<ActionResult> Index()
        {
			
			// Get the most recent groups
			IList<Group> recentGroups = await GroupService.GetRecentlyAdded(10);

            return View(recentGroups);
        }

		#region Create group

		[Route("create")]
		public ActionResult Create()
		{
			CreateGroupModel model = new CreateGroupModel();
			return View(model);
		}

		[Route("create")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create(CreateGroupModel model)
		{

			// If the model is not valid, return view.
			if (!ModelState.IsValid)
			{
				return View(model);
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
				return View(model);
			}

			// Store the CreateGroupViewModel in session for the next screen
			Session[CreateGroupViewModelSesionKey] = model;

			// Redirect to the group administrator screen
			return new RedirectResult("/admin/groups/create/group-administrator");

			
			//// Create the group
			//Group newGroup = await GroupService.CreateGroup(model.Name, serviceType, model.Description);
			//
			//return View(model);
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
			
			// If there is a group user, then add to the model.
			if (groupAdminUser != null)
			{
				model.GroupAdministrator.FirstName = groupAdminUser.FirstName;
				model.GroupAdministrator.Surname = groupAdminUser.Surname;
				model.GroupAdministrator.EmailAddress = model.GroupAdministratorEmail;
				model.GroupAdministrator.UserExists = true;
			}

			return View("~/Areas/Admin/Views/Users/ConfirmUser", model.GroupAdministrator);
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
				return View("~/Areas/Admin/Views/Users/ConfirmUser", model);
			}

			// If there is "System Administrator" in the role list, show error message
			if (model.Role.Equals(RoleTypes.SystemAdministrator, StringComparison.CurrentCultureIgnoreCase))
			{
				ModelState.AddModelError("", "There was an error setting the role for the user.");
				return View("~/Areas/Admin/Views/Users/ConfirmUser", model);
			}

			// Get the CreateGroupViewModel from session
			CreateGroupModel createGroupModel = (CreateGroupModel)Session[CreateGroupViewModelSesionKey];

			// If the view model is null, redirect back to the create screen
			if (createGroupModel == null)
			{
				return new RedirectResult("/admin/groups/create");
			}

			// Store the group administrator
			Guid groupAdministratorId;

			// If the group administrator user does not exist, then create the user now
			if (!model.UserExists)
			{

				// Create the new user
				IdentityUser newUser = await UserService.CreateAsync(model.EmailAddress, model.FirstName, model.Surname, new List<string>() { RoleTypes.GroupAdministrator });

				// Set the group administrator to the new user id
				groupAdministratorId = newUser.Id;

			}
			else
			{
				// Get the identity user related to the specified group admin
				IdentityUser groupAdminUser = await UserService.FindByEmailAsync(createGroupModel.GroupAdministratorEmail);

				// If the group admin user is null, return an error, otherwise set the group admin id.
				if (groupAdminUser == null)
				{
					ModelState.AddModelError("", "There was a system error creating the group.");
					await Log.Error(String.Format("Unable to create group. Existing user with email ''  could not be found.", createGroupModel.GroupAdministratorEmail));
					return View("~/Areas/Admin/Views/Users/ConfirmUser", model);
				}
				else
				{
					groupAdministratorId = groupAdminUser.Id;
				}
			}

			// Get the service type from the model
			int groupServiceId;
			Int32.TryParse(createGroupModel.Service, out groupServiceId);
			ServiceType service = (ServiceType)groupServiceId;

			// Create the group
			await GroupService.CreateGroup(createGroupModel.Name, service, createGroupModel.Capcode, groupAdministratorId, createGroupModel.Description);

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

			// Get the group
			Group group = await GroupService.GetById(id);

			// If the place is null, throw 404
			if (group == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Get the list of users in the group from the Users property of the group
			IList<IdentityUser> groupUsers = await UserService.GetUsersByIds(group.Users.Select(i => i.UserId));

			// Create the list of GroupUserViewModels for the users in the group
			IList<GroupUserViewModel> groupUserModels = new List<GroupUserViewModel>();
			foreach(IdentityUser groupUser in groupUsers)
			{
				groupUserModels.Add(new GroupUserViewModel() {
					EmailAddress = groupUser.EmailAddress,
					FirstName = groupUser.FirstName,
					GroupRole = group.Users.FirstOrDefault(i => i.UserId == groupUser.Id).Role,
					Id = groupUser.Id,
					Surname = groupUser.Surname
				});
			}

			// Create the model for the single view
			SingleGroupViewModel model = new SingleGroupViewModel()
			{
				Id = group.Id,
				Name = group.Name,
				Description = group.Description,
				Service = EnumValue.GetEnumDescription(group.Service),
				Capcode = group.Capcode,
				Users = groupUserModels
			};

			return View(model);


		}

		#endregion

		#region New user

		[Route("{groupId:guid}/add-member")]
		[HttpGet]
		public async Task<ActionResult> AddMember(Guid groupId)
		{
			// Get the group
			Group group = await GroupService.GetById(groupId);

			// If the group is null, return 404 not found error
			if (group == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			NewUserViewModel model = new NewUserViewModel();
			model.AvailableRoles = GetAvailableRoles();
			return View("~/Areas/Admin/Views/Users/NewUser.cshtml", model);
		}


		[Route("{groupId:guid}/add-member")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> AddMember(Guid groupId, NewUserViewModel model)
		{

			// Get the group
			Group group = await GroupService.GetById(groupId);

			// If the group is null, return 404 not found error
			if (group == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Set the form action
			ViewBag.FormAction = String.Format("/admin/groups/{0}/confirm-member", groupId);

			// Get the available roles for the user.
			model.AvailableRoles = GetAvailableRoles();

			// If the model is not valid, return view.
			if (!ModelState.IsValid)
			{
				return View("~/Areas/Admin/Views/Users/NewUser.cshtml", model);
			}

			// If there is "System Administrator" in the role list, show error message
			if (model.Role.Equals(RoleTypes.SystemAdministrator, StringComparison.CurrentCultureIgnoreCase))
			{
				ModelState.AddModelError("", "There was an error setting the role for the user.");
				return View("~/Areas/Admin/Views/Users/NewUser.cshtml", model);
			}

			// Get the identity user related to the specified group admin
			IdentityUser newUser = await UserService.FindByEmailAsync(model.EmailAddress);
		
			// If the user exists, and there is already a user mapping in the group for this user, show error message
			if (newUser != null && group.Users.Any(i => i.UserId == newUser.Id))
			{
				ModelState.AddModelError("", "The email address you have entered is already a member of this group.");
				return View("~/Areas/Admin/Views/Users/NewUser.cshtml", model);
			}

			// Add the model to the session for the next screen
			Session[AddMemberViewModelSessionKey] = model;

			return new RedirectResult(String.Format("/admin/groups/{0}/confirm-member", groupId));
		}

		[Route("{groupId:guid}/confirm-member")]
		[HttpGet]
		public async Task<ActionResult> ConfirmMember(Guid groupId)
		{

			// Get the group
			Group group = await GroupService.GetById(groupId);

			// If the group is null, return 404 not found error
			if (group == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Set the form action
			ViewBag.FormAction = String.Format("/admin/groups/{0}/confirm-member", groupId);

			// Get the CreateGroupViewModel from session
			NewUserViewModel newUserModel = (NewUserViewModel)Session[AddMemberViewModelSessionKey];

			// If the view model is null, redirect back to the Group home screen
			if (newUserModel == null)
			{
				return new RedirectResult(String.Format("/admin/groups/{0}", groupId));
			}

			// Get the identity user related to the specified group admin
			IdentityUser newUser = await UserService.FindByEmailAsync(newUserModel.EmailAddress);

			// Create the model
			ConfirmUserViewModel model = new ConfirmUserViewModel()
			{
				EmailAddress = newUserModel.EmailAddress,
				Role = newUserModel.Role
			};

			// If the user exists, add the users information to display.
			if (newUser != null)
			{
				model.UserExists = true;
				model.FirstName = newUser.FirstName;
				model.Surname = newUser.Surname;
			}

			return View("~/Areas/Admin/Views/Users/ConfirmUser.cshtml", model);

		}

		[Route("{groupId:guid}/confirm-member")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ConfirmMember(Guid groupId, ConfirmUserViewModel model)
		{

			// Get the group
			Group group = await GroupService.GetById(groupId);

			// If the group is null, return 404 not found error
			if (group == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Set the form action
			ViewBag.FormAction = String.Format("/admin/groups/{0}/confirm-member", groupId);

			// If the model is not valid, return view.
			if (!ModelState.IsValid)
			{
				return View("~/Areas/Admin/Views/Users/ConfirmUser.cshtml", model);
			}

			// If there is "System Administrator" in the role list, show error message
			if (model.Role.Equals(RoleTypes.SystemAdministrator, StringComparison.CurrentCultureIgnoreCase))
			{
				ModelState.AddModelError("", "There was an error setting the role for the user.");
				return View("~/Areas/Admin/Views/Users/ConfirmUser.cshtml", model);
			}
			
			// Get the identity user related to the specified group admin
			IdentityUser newUser = await UserService.FindByEmailAsync(model.EmailAddress);

			// If the user exists, and there is already a user mapping in the group for this user, show error message
			if (newUser != null && group.Users.Any(i => i.UserId == newUser.Id))
			{
				ModelState.AddModelError("", "The email address you have entered is already a member of this group.");
				return View("~/Areas/Admin/Views/Users/ConfirmUser.cshtml", model);
			}
			else if (newUser != null)
			{
				// Create the user mapping for the existing user
				await GroupService.AddUserToGroup(newUser.Id, model.Role, groupId);
			}
			else
			{

				// Create the new user, and then create the group mapping for the new user.
				newUser = await UserService.CreateAsync(model.EmailAddress, model.FirstName, model.Surname, new List<string>() { model.Role });

				// Now that we have the newUser, create the user mapping.
				await GroupService.AddUserToGroup(newUser.Id, model.Role, groupId);

			}

			// remove the newUserModel from the session
			Session.Remove(AddMemberViewModelSessionKey);

			// redirect back to group view page
			return new RedirectResult(String.Format("/admin/groups/{0}?member_added=1", groupId));
		}

		private IList<SelectListItem> GetAvailableRoles()
		{
			IList<SelectListItem> availableRoles = new List<SelectListItem>();
			availableRoles.Add(new SelectListItem() { Text = "Select role", Value = "" });
			availableRoles.Add(new SelectListItem() { Text = RoleTypes.GeneralUser, Value = RoleTypes.GeneralUser });
			availableRoles.Add(new SelectListItem() { Text = RoleTypes.GroupAdministrator, Value = RoleTypes.GroupAdministrator });

			// return the available roles
			return availableRoles;
		}

		#endregion

	}
}