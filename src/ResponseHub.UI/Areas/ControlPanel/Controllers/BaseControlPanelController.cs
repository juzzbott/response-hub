using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.UI.Controllers;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Identity;
using System.Net;
using Enivate.ResponseHub.UI.Models.Users;
using Enivate.ResponseHub.UI.Areas.Admin.Models.Groups;
using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Spatial;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{
    public abstract class BaseControlPanelController : BaseController
    {
		
		protected const string AddMemberViewModelSessionKey = "NewUserViewModel";

		protected IGroupService GroupService
		{
			get
			{
				return ServiceLocator.Get<IGroupService>();
			}
		}
		
		protected ICapcodeService CapcodeService
		{
			get
			{
				return ServiceLocator.Get<ICapcodeService>();
			}
		}
		
		protected IMailService MailService
		{
			get
			{
				return ServiceLocator.Get<IMailService>();
			}
		}

		protected string AreaPrefix
		{
			get
			{
				return RouteData.DataTokens["area"].ToString() == "admin" ? "admin" : "control-panel";
			}
		}

		/// <summary>
		/// Gets the list of GroupIds the current user is a group admin of.
		/// </summary>
		/// <returns></returns>
		public async Task<IList<Guid>> GetGroupIdsUserIsGroupAdminOf()
		{

			// Get the group ids and user mappings for those groups
			IDictionary<Guid, UserMapping> userGroupMappings = await GroupService.GetUserMappingsForUser(UserId);

			IList<Guid> groupAdminGroupIds = userGroupMappings.Where(i => i.Value.Role == RoleTypes.GroupAdministrator).Select(i => i.Key).ToList();

			// Get the user mappings for groups for the user
			return groupAdminGroupIds;

		}

		public async Task<bool> CurrentUserIsAdminOfGroup(Guid groupId)
		{

			// Get the user mappings for the current user
			IDictionary<Guid, UserMapping> userGroupMappings = await GroupService.GetUserMappingsForUser(UserId);

			// determine if the user is a group admin of the specified group id
			return userGroupMappings.Any(i => i.Key == groupId && i.Value.Role == RoleTypes.GroupAdministrator);

		}

		#region View Group

		/// <summary>
		/// Gets the ViewResult for the ViewGroup action.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<ActionResult> GetViewGroupViewResult(Guid id, string viewPath)
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
				groupUsers.AddRange(await GroupService.GetUsersForGroup(id));
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

			return View(viewPath, model);
		}

		#endregion

		#region Edit Group

		/// <summary>
		/// Gets the ViewResult for the Edit group GET request.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<ActionResult> GetEditGroupViewResult(Guid id)
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
			model.AvailableGroupCapcodes = await CapcodeService.GetAllByService(group.Service, true);
			model.AvailableAdditionalCapcodes = await CapcodeService.GetAllByService(group.Service, false);
			model.AdditionalCapcodes = String.Format("{0},", String.Join(",", group.AdditionalCapcodes));
			model.Description = group.Description;
			model.Latitude = group.HeadquartersCoordinates.Latitude;
			model.Longitude = group.HeadquartersCoordinates.Longitude;
			model.Name = group.Name;
			model.Region = group.Region.Id;
			model.Service = ((int)group.Service).ToString();

			// Set the page title.
			ViewBag.Title = "Edit group";

			return View("~/Areas/Admin/Views/Groups/CreateEdit.cshtml", model);

		}

		public async Task<ActionResult> PostEditGroupViewResult(Guid id, CreateGroupModel model)
		{
			// Set the form action and the page title.
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
			model.AvailableAdditionalCapcodes = await CapcodeService.GetAllByService(group.Service, false);
			model.AvailableGroupCapcodes = await CapcodeService.GetAllByService(group.Service, true);

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
				await Log.Error(String.Format("Unable to update group. Region '{0}' does not exist.", model.Region));
				ModelState.AddModelError("", "There was a system error updating the group.");
				return View("~/Areas/Admin/Views/Groups/CreateEdit.cshtml", model);
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

				// Create the capcode if it doesn't exist.
				await CheckAndCreateCapcode(model.Capcode, model.Name, service);

				// Save the group to the database
				await GroupService.UpdateGroup(group);

				return new RedirectResult(String.Format("/{0}/groups/{1}?saved=1", AreaPrefix, id));

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Unable to update group. System exception: {0}", ex.Message), ex);
				ModelState.AddModelError("", "There was a system error updating the group.");
				return View("~/Areas/Admin/Views/Groups/CreateEdit.cshtml", model);
			}
		}

		#endregion

		#region New user

		public async Task<ActionResult> GetAddMemberViewResult(Guid groupId)
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
			return View("~/Areas/Admin/Views/Groups/NewUser.cshtml", model);

		}

		public async Task<ActionResult> PostAddMemberViewResult(Guid groupId, NewUserViewModel model)
		{
			// Get the group
			Group group = await GroupService.GetById(groupId);

			// If the group is null, return 404 not found error
			if (group == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Get the available roles for the user.
			model.AvailableRoles = GetAvailableRoles();

			// If the model is not valid, return view.
			if (!ModelState.IsValid)
			{
				return View("~/Areas/Admin/Views/Groups/NewUser.cshtml", model);
			}

			// If there is "System Administrator" in the role list, show error message
			if (model.Role.Equals(RoleTypes.SystemAdministrator, StringComparison.CurrentCultureIgnoreCase))
			{
				ModelState.AddModelError("", "There was an error setting the role for the user.");
				return View("~/Areas/Admin/Views/Groups/NewUser.cshtml", model);
			}

			// Get the identity user related to the specified group admin
			IdentityUser newUser = await UserService.FindByEmailAsync(model.EmailAddress);

			// If the user exists, and there is already a user mapping in the group for this user, show error message
			if (newUser != null && group.Users.Any(i => i.UserId == newUser.Id))
			{
				ModelState.AddModelError("", "The email address you have entered is already a member of this group.");
				return View("~/Areas/Admin/Views/Groups/NewUser.cshtml", model);
			}

			// Add the model to the session for the next screen
			Session[AddMemberViewModelSessionKey] = model;

			return new RedirectResult(String.Format("/{0}/groups/{1}/confirm-member", AreaPrefix, groupId));
		}

		public async Task<ActionResult> GetConfirmMemberViewResult(Guid groupId)
		{
			// Get the group
			Group group = await GroupService.GetById(groupId);

			// If the group is null, return 404 not found error
			if (group == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Get the CreateGroupViewModel from session
			NewUserViewModel newUserModel = (NewUserViewModel)Session[AddMemberViewModelSessionKey];

			// If the view model is null, redirect back to the Group home screen
			if (newUserModel == null)
			{
				return new RedirectResult(String.Format("/{0}/groups/{1}", AreaPrefix, groupId));
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

			return View("~/Areas/Admin/Views/Groups/ConfirmUser.cshtml", model);
		}

		public async Task<ActionResult> PostConfirmMemberViewResult(Guid groupId, ConfirmUserViewModel model)
		{
			try
			{

				// Get the group
				Group group = await GroupService.GetById(groupId);

				// If the group is null, return 404 not found error
				if (group == null)
				{
					throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
				}

				// If the model is not valid, return view.
				if (!ModelState.IsValid)
				{
					return View("~/Areas/Admin/Views/Groups/ConfirmUser.cshtml", model);
				}

				// If there is "System Administrator" in the role list, show error message
				if (model.Role.Equals(RoleTypes.SystemAdministrator, StringComparison.CurrentCultureIgnoreCase))
				{
					ModelState.AddModelError("", "There was an error setting the role for the user.");
					return View("~/Areas/Admin/Views/Groups/ConfirmUser.cshtml", model);
				}

				// Get the identity user related to the specified group admin
				IdentityUser newUser = await UserService.FindByEmailAsync(model.EmailAddress);

				// If the user exists, and there is already a user mapping in the group for this user, show error message
				if (newUser != null && group.Users.Any(i => i.UserId == newUser.Id))
				{
					ModelState.AddModelError("", "The email address you have entered is already a member of this group.");
					return View("~/Areas/Admin/Views/Groups/ConfirmUser.cshtml", model);
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

					// Send the activation email
					await MailService.SendAccountActivationEmail(newUser);

					// Now that we have the newUser, create the user mapping.
					await GroupService.AddUserToGroup(newUser.Id, model.Role, groupId);

				}

				// remove the newUserModel from the session
				Session.Remove(AddMemberViewModelSessionKey);

				// redirect back to group view page
				return new RedirectResult(String.Format("/{0}/groups/{1}?member_added=1", AreaPrefix, groupId));

			}
			catch (Exception ex)
			{
				await Log.Error("Error adding new user to group. Message: " + ex.Message, ex);
				ModelState.AddModelError("", "There was a system error adding the user to the group.");
				return View("~/Areas/Admin/Views/Groups/ConfirmUser.cshtml", model);
			}
		}

		#endregion

		#region Change User Role 

		public ActionResult GetChangeUserRoleViewResult(Guid groupId, Guid userId, string viewPath)
		{
			// Create the model
			ChangeUserRoleViewModel model = new ChangeUserRoleViewModel();
			model.AvailableRoles = GetAvailableRoles();
			model.GroupId = groupId;

			// return the view
			return View(viewPath, model);
		}

		public async Task<ActionResult> ViewChangeUserRoleViewResult(Guid groupId, Guid userId, ChangeUserRoleViewModel model, string viewPath, string urlPart)
		{

			// Create the model
			model.AvailableRoles = GetAvailableRoles();
			model.GroupId = groupId;

			// If the model has errors, return
			if (!ModelState.IsValid)
			{
				return View(viewPath, model);
			}

			try
			{

				// Get the group so that we can validate the group not getting < 1 group administrator
				Group group = await GroupService.GetById(groupId);

				if (group == null)
				{
					await Log.Error("Unable to change user role for group. Group cannot be found.  ");
					ModelState.AddModelError("", "Unable to change role for user. The group cannot be found.");
					return View(viewPath, model);
				}

				// If there is only 1 group administrator, and it's the current user, and the new role is general user, hen show error message stating you must hace one administrator account
				if (group.Users.Count(i => i.Role.ToUpper() == RoleTypes.GroupAdministrator.ToUpper()) == 1 &&
					group.Users.First(i => i.Role.ToUpper() == RoleTypes.GroupAdministrator.ToUpper()).UserId == userId &&
					model.Role.ToUpper() == RoleTypes.GeneralUser.ToUpper())
				{
					ModelState.AddModelError("", "You cannot change the role for this user to General User. Your group must always have at least one Group Administrator.");
					return View(viewPath, model);
				}


				// Update the role for the user
				await GroupService.ChangeUserRoleInGroup(groupId, userId, model.Role);

				// return the view
				return new RedirectResult(String.Format("/{0}/groups/{1}?role_changed=1", urlPart.ToLower(), groupId));

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Unable to change role for user in group. Message: {0}", ex.Message), ex);
				ModelState.AddModelError("", "Sorry, there was an error changing the role for the user.");
				return View(viewPath, model);
			}

		}

		#endregion

		#region Remove User From Group

		public async Task<ActionResult> GetRemoveUserFromGroup(Guid groupId, Guid userId, string urlPart)
		{
			try
			{
				// Remover the user
				await GroupService.RemoveUserFromGroup(userId, groupId);

				// return to the view group screen
				return new RedirectResult(String.Format("/{0}/groups/{1}?user_removed=1", urlPart, groupId));
			} 
			catch (Exception ex)
			{
				// Log the error and redirect with error query string
				await Log.Error(String.Format("Unable to remove user '{0}' from group '{1}'.", userId, groupId), ex);

				// return to the view group screen
				return new RedirectResult(String.Format("/{0}/groups/{1}?remove_user_error=1", urlPart, groupId));
			}
		}

		#endregion

		#region Helpers



		/// <summary>
		/// Gets the list of regions in a select list for use on the screens.
		/// </summary>
		/// <returns></returns>
		protected async Task<IList<SelectListItem>> GetAvailableRegions()
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



		/// <summary>
		/// Gets the list of guids from the hidden value for the additional capcodes.
		/// </summary>
		/// <param name="additionalCapcodes">The field containing the Id values.</param>
		/// <returns></returns>
		protected IList<Guid> GetCapcodeIdsFromHiddenValue(string additionalCapcodes)
		{
			// If the additional capcodes is null or empty, return empty list
			if (String.IsNullOrEmpty(additionalCapcodes))
			{
				return new List<Guid>();
			}

			IList<Guid> capcodeIds = new List<Guid>();

			// Split the string by commas, and add each guid to the list
			foreach (string rawId in additionalCapcodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
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

		/// <summary>
		/// Checks to see if the capcode exists. If it doesn't exist, then it's created. 
		/// </summary>
		/// <param name="capcode">The capcode to check or create.</param>
		/// <param name="groupName">The name of the group to create the capcode for.</param>
		/// <param name="service">The service the capcode is associated with.</param>
		/// <returns></returns>
		protected async Task CheckAndCreateCapcode(string capcode, string groupName, ServiceType service)
		{

			// Check if the capcode exists 
			IList<Capcode> allCapcodes = await CapcodeService.GetAll();

			// Check if the capcode exists in the collection
			if (!allCapcodes.Any(i => i.CapcodeAddress == capcode))
			{

				// Create the capcode
				await CapcodeService.Create(groupName, capcode, "", service, true);

			}

		}


		/// <summary>
		/// Gets the list of available roles for the users to be set as.
		/// </summary>
		/// <returns></returns>
		protected IList<SelectListItem> GetAvailableRoles()
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