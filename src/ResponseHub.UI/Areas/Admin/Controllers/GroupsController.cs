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

		private ICapcodeService _capcodeService;
		protected ICapcodeService CapcodeService
		{
			get
			{
				return _capcodeService ?? (_capcodeService = UnityConfiguration.Container.Resolve<ICapcodeService>());
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
			model.AvailableCapcodes = await CapcodeService.GetAll();

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
			model.AvailableCapcodes = await CapcodeService.GetAll();

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

			// Get the list of additional capcodes
			IList<Guid> additionalCapcodes = GetCapcodeIdsFromHiddenValue(createGroupModel.AdditionalCapcodes);

			// Create the group
			await GroupService.CreateGroup(createGroupModel.Name, service, createGroupModel.Capcode, additionalCapcodes, groupAdmin.Id, createGroupModel.Description, region, coords);

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
			List<IdentityUser> groupUsers = new List<IdentityUser>();
			
			if (String.IsNullOrEmpty(Request.QueryString["q"]))
			{
				// Get all the users for the group
				groupUsers.AddRange(await UserService.GetUsersByIds(group.Users.Select(i => i.UserId)));
			}
			else
			{

				// Get the search results
				IList<IdentityUser> searchResults = await UserService.SearchUsers(Request.QueryString["q"]);

				// Only add the users that have a user id in the group
				groupUsers.AddRange(searchResults.Where(i => group.Users.Select(u => u.UserId).Contains(i.Id)));
			}

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

			// Get the list of additional capcodes
			IList<Capcode> allCapcodes = await CapcodeService.GetAll();
			IList<Capcode> additionalCapcodes = allCapcodes.Where(i => group.AdditionalCapcodes.Contains(i.Id)).ToList();

			// Create the model for the single view
			SingleGroupViewModel model = new SingleGroupViewModel()
			{
				Id = group.Id,
				Name = group.Name,
				Description = group.Description,
				Service = EnumExtensions.GetEnumDescription(group.Service),
				Capcode = group.Capcode,
				AdditionalCapcodes = additionalCapcodes,
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
			model.AvailableCapcodes = await CapcodeService.GetAllByService(group.Service);
			model.AdditionalCapcodes = String.Format("{0},", String.Join(",", group.AdditionalCapcodes));
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
		[ValidateAntiForgeryToken]
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

			// Set the available options
			model.AvailableRegions = await GetAvailableRegions();
			model.AvailableCapcodes = await CapcodeService.GetAllByService(group.Service);

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

			IList<Guid> additionalCapcodeIds = GetCapcodeIdsFromHiddenValue(model.AdditionalCapcodes);

			try
			{

				// Update the values of the group
				group.Name = model.Name;
				group.Capcode = model.Capcode;
				group.AdditionalCapcodes = GetCapcodeIdsFromHiddenValue(model.AdditionalCapcodes);
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

		#region Change User Role

		[Route("{groupId:guid}/change-role/{userId:guid}")]
		[HttpGet]
		public ActionResult ChangeUserRole(Guid groupId, Guid userId)
		{

			// Create the model
			ChangeUserRoleViewModel model = new ChangeUserRoleViewModel();
			model.AvailableRoles = GetAvailableRoles();
			model.GroupId = groupId;

			// return the view
			return View(model);


		}

		[Route("{groupId:guid}/change-role/{userId:guid}")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ChangeUserRole(Guid groupId, Guid userId, ChangeUserRoleViewModel model)
		{

			// Create the model
			model.AvailableRoles = GetAvailableRoles();
			model.GroupId = groupId;

			// If the model has errors, return
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{

				// Get the group so that we can validate the group not getting < 1 group administrator
				Group group = await GroupService.GetById(groupId);

				if (group == null)
				{
					await Log.Error("Unable to change user role for group. Group cannot be found.  ");
					ModelState.AddModelError("", "Unable to change role for user. The group cannot be found.");
					return View(model);
				}

				// If there is only 1 group administrator, and it's the current user, and the new role is general user, hen show error message stating you must hace one administrator account
				if (group.Users.Count(i => i.Role.ToUpper() == RoleTypes.GroupAdministrator.ToUpper()) == 1 && 
					group.Users.First(i => i.Role.ToUpper() == RoleTypes.GroupAdministrator.ToUpper()).UserId == userId &&
					model.Role.ToUpper() == RoleTypes.GeneralUser.ToUpper())
				{
					ModelState.AddModelError("", "You cannot change the role for this user to General User. Your group must always have at least one Group Administrator.");
					return View(model);
				}


				// Update the role for the user
				await GroupService.ChangeUserRoleInGroup(groupId, userId, model.Role);

				// return the view
				return new RedirectResult(String.Format("/admin/groups/{0}?role_changed=1", groupId));

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Unable to change role for user in group. Message: {0}", ex.Message), ex);
				ModelState.AddModelError("", "Sorry, there was an error changing the role for the user.");
				return View(model);
			}


		}

		#endregion

		#region Helpers

		/// <summary>
		/// Gets the list of available roles for the users to be set as.
		/// </summary>
		/// <returns></returns>
		private IList<SelectListItem> GetAvailableRoles()
		{
			IList<SelectListItem> availableRoles = new List<SelectListItem>();
			availableRoles.Add(new SelectListItem() { Text = "Select role", Value = "" });
			availableRoles.Add(new SelectListItem() { Text = RoleTypes.GeneralUser, Value = RoleTypes.GeneralUser });
			availableRoles.Add(new SelectListItem() { Text = RoleTypes.GroupAdministrator, Value = RoleTypes.GroupAdministrator });

			// return the available roles
			return availableRoles;
		}

		/// <summary>
		/// Gets the list of guids from the hidden value for the additional capcodes.
		/// </summary>
		/// <param name="additionalCapcodes">The field containing the Id values.</param>
		/// <returns></returns>
		private IList<Guid> GetCapcodeIdsFromHiddenValue(string additionalCapcodes)
		{
			// If the additional capcodes is null or empty, return empty list
			if (String.IsNullOrEmpty(additionalCapcodes))
			{
				return new List<Guid>();
			}

			IList<Guid> capcodeIds = new List<Guid>();

			// Split the string by commas, and add each guid to the list
			foreach(string rawId in additionalCapcodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				// If the raw id is null or empty, continue
				if (String.IsNullOrWhiteSpace(rawId))
				{
					continue;
				}

				// Create the Guid from the raw id
				Guid capcodeId = Guid.Empty;
				bool result = Guid.TryParse(rawId.Trim(), out capcodeId);

				// If valid, add to the list
				if (result)
				{
					capcodeIds.Add(capcodeId);
				}
			}

			// return the list of capcodes
			return capcodeIds;

		}

		#endregion

	}
}