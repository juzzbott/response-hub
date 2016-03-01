using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Common.Constants;
using Enivate.ResponseHub.Common.Extensions;	
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Identity.Interface;
using Enivate.ResponseHub.Model.Spatial;
using Enivate.ResponseHub.Mail;
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

			List<Group> groups = new List<Group>();

			if (String.IsNullOrEmpty(Request.QueryString["q"]))
			{
				// Get the most recent groups
				groups.AddRange(await GroupService.GetRecentlyAdded(10));
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
			CreateGroupModel model = new CreateGroupModel();
			model.AvailableRegions = await GetAvailableRegions();

			// Set the form action and the addGroupAdministrator flag.
			ViewBag.AddGroupAdministrator = true;
			ViewBag.FormAction = "/admin/groups/create";
			ViewBag.Title = "Create new group";

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

			return View("~/Areas/Admin/Views/Users/ConfirmUser.cshtml", model.GroupAdministrator);
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
				return View("~/Areas/Admin/Views/Users/ConfirmUser.cshtml", model);
			}

			// If there is "System Administrator" in the role list, show error message
			if (model.Role.Equals(RoleTypes.SystemAdministrator, StringComparison.CurrentCultureIgnoreCase))
			{
				ModelState.AddModelError("", "There was an error setting the role for the user.");
				return View("~/Areas/Admin/Views/Users/ConfirmUser.cshtml", model);
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
				await SendAccountActivationEmail(groupAdmin);

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
					return View("~/Areas/Admin/Views/Users/ConfirmUser.cshtml", model);
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
				await _log.Error(String.Format("Unable to create group. Region '{0}' does not exist.", createGroupModel.Region));
				ModelState.AddModelError("", "There was a system error creating the group.");
				return View("~/Areas/Admin/Views/Users/ConfirmUser.cshtml", model);
			}

			// Create the headquarters coords
			Coordinates coords = new Coordinates(createGroupModel.Latitude.Value, createGroupModel.Longitude.Value);

			// Create the group
			await GroupService.CreateGroup(createGroupModel.Name, service, createGroupModel.Capcode, groupAdmin.Id, createGroupModel.Description, region, coords);

			// Send the new group email to the group admin
			await SendGroupCreatedEmail(groupAdmin, createGroupModel.Name, service, createGroupModel.Capcode);

			// Clear the session url
			Session.Remove(CreateGroupViewModelSesionKey);

			// redirect back to group index page
			return new RedirectResult("/admin/groups?group_created=1");
		}

		/// <summary>
		/// Sends the account activation email to the new user.
		/// </summary>
		/// <param name="newUser">The new user to send the account for.</param>
		/// <returns>Async task.</returns>
		private async Task SendAccountActivationEmail(IdentityUser newUser)
		{

			// Create the tuple for the to override
			Tuple<string, string> to = new Tuple<string, string>(newUser.EmailAddress, newUser.FullName);

			string baseSiteUrl = ConfigurationManager.AppSettings[ConfigurationKeys.BaseWebsiteUrl] ?? "";

			// Create the replacements
			IDictionary<string, string> replacements = new Dictionary<string, string>();
			replacements.Add("#FirstName#", newUser.FirstName);
			replacements.Add("#ActivationLink#", String.Format("{0}/my-account/activate/{1}", baseSiteUrl, newUser.ActivationCode.ToLower()));

			// Create the mail provider and send the message
			MailProvider mailProvider = new MailProvider();
			await mailProvider.SendMailMessage(MailTemplates.ActivateAccount, replacements, to, null);

		}

		private async Task SendGroupCreatedEmail(IdentityUser groupAdmin, string groupName, ServiceType service, string capcode)
		{
			// Create the tuple for the to override
			Tuple<string, string> to = new Tuple<string, string>(groupAdmin.EmailAddress, groupAdmin.FullName);
			
			// Create the replacements
			IDictionary<string, string> replacements = new Dictionary<string, string>();
			replacements.Add("#FirstName#", groupAdmin.FirstName);
			replacements.Add("#GroupName#", groupName);
			replacements.Add("#ServiceType#", service.GetEnumDescription());
			replacements.Add("#Capcode#", capcode);

			// Create the mail provider and send the message
			MailProvider mailProvider = new MailProvider();
			await mailProvider.SendMailMessage(MailTemplates.GroupCreated, replacements, to, null);

		}
				
		/// <summary>
		/// Gets the list of regions in a select list for use on the screens.
		/// </summary>
		/// <returns></returns>
		private async Task<IList<SelectListItem>> GetAvailableRegions()
		{

			IList<Region> regions = await GroupService.GetRegions();

			// Create the list of select items
			IList<SelectListItem> items = new List<SelectListItem>();
			items.Add(new SelectListItem() { Text = "Please select", Value = "" });
			foreach (Region region in regions)
			{
				items.Add(new SelectListItem() { Text = region.Name, Value = region.Id.ToString() });
			}

			// return the list of items
			return items;
		}

		#endregion

		#region View group

		[Route("{id:guid}")]
		[HttpGet]
		public async Task<ActionResult> ViewGroup(Guid id)
		{

			// Get the group
			Group group = await GroupService.GetById(id);

			// If the group is null, throw 404
			if (group == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Get the list of users in the group from the Users property of the group
			IList<IdentityUser> groupUsers = await UserService.GetUsersByIds(group.Users.Select(i => i.UserId));

			// Create the list of GroupUserViewModels for the users in the group
			IList<GroupUserViewModel> groupUserModels = new List<GroupUserViewModel>();
			foreach (IdentityUser groupUser in groupUsers)
			{
				groupUserModels.Add(new GroupUserViewModel()
				{
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
				Service = EnumExtensions.GetEnumDescription(group.Service),
				Capcode = group.Capcode,
				Users = groupUserModels,
				Region = group.Region.Name,
				HeadquartersCoordinates = group.HeadquartersCoordinates
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

		#region Edit group

		[Route("{id:guid}/edit")]
		[HttpGet]
		public async Task<ActionResult> Edit(Guid id)
		{

			// Get the group based on the id
			Group group = await GroupService.GetById(id);

			// If the group is null, throw not found exception
			if (group == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Create and populate the model
			CreateGroupModel model = new CreateGroupModel();
			model.AvailableRegions = await GetAvailableRegions();
			model.Capcode = group.Capcode;
			model.Description = group.Description;
			model.Latitude = group.HeadquartersCoordinates.Latitude;
			model.Longitude = group.HeadquartersCoordinates.Longitude;
			model.Name = group.Name;
			model.Region = group.Region.Id;
			model.Service = ((int)group.Service).ToString();

			// Set the form action and the page title.
			ViewBag.FormAction = String.Format("/admin/groups/{0}/edit", id);
			ViewBag.Title = "Edit group";

			return View("CreateEdit", model);
		}

		[Route("{id:guid}/edit")]
		[HttpPost]
		public async Task<ActionResult> Edit(Guid id, CreateGroupModel model)
		{

			// Set the form action and the page title.
			ViewBag.FormAction = String.Format("/admin/groups/{0}/edit", id);
			ViewBag.Title = "Edit group";

			// Get the group based on the id
			Group group = await GroupService.GetById(id);

			// If the group is null, throw not found exception
			if (group == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Get the service type from the model
			int groupServiceId;
			Int32.TryParse(model.Service, out groupServiceId);
			ServiceType service = (ServiceType)groupServiceId;

			// Get the region based on the posted value
			IList<Region> regions = await GroupService.GetRegions();
			Region region = regions.FirstOrDefault(i => i.Id == model.Region);

			// If the region is null, log the error and return error message
			if (region == null)
			{
				await _log.Error(String.Format("Unable to update group. Region '{0}' does not exist.", model.Region));
				ModelState.AddModelError("", "There was a system error updating the group.");
				return View("CreateEdit", model);
			}

			// Create the headquarters coords
			Coordinates coords = new Coordinates(model.Latitude.Value, model.Longitude.Value);

			try
			{

				// Update the values of the group
				group.Name = model.Name;
				group.Capcode = model.Capcode;
				group.Description = model.Description;
				group.Updated = DateTime.UtcNow;
				group.HeadquartersCoordinates = coords;
				group.Region = region;
				group.Service = service;

				// Save the group to the database
				await GroupService.UpdateGroup(group);
				
				return new RedirectResult(String.Format("/admin/groups/{0}?saved=1", id));

			}
			catch (Exception ex)
			{
				await _log.Error(String.Format("Unable to update group. System exception: {0}", ex.Message), ex);
				ModelState.AddModelError("", "There was a system error updating the group.");
				return View("CreateEdit", model);
			}


		}

		#endregion

	}
}