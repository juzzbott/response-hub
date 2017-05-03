using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.UI.Controllers;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Models.Users;
using Enivate.ResponseHub.UI.Areas.Admin.Models.Units;
using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Spatial;
using Enivate.ResponseHub.Common.Constants;
using Enivate.ResponseHub.Caching;
using Enivate.ResponseHub.Model.Identity.Interface;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{
    public abstract class BaseControlPanelController : BaseController
    {
		
		protected const string AddMemberViewModelSessionKey = "NewUserViewModel";

		protected IUnitService UnitService = ServiceLocator.Get<IUnitService>();
		protected ICapcodeService CapcodeService = ServiceLocator.Get<ICapcodeService>();		
		protected IMailService MailService = ServiceLocator.Get<IMailService>();

		protected string AreaPrefix
		{
			get
			{
				return RouteData.DataTokens["area"].ToString() == "admin" ? "admin" : "control-panel";
			}
		}

		public async Task<bool> CurrentUserIsAdminOfUnit(Guid unitId)
		{

			// Get the user mappings for the current user
			IDictionary<Guid, UserMapping> userUnitMappings = await UnitService.GetUserMappingsForUser(UserId);

			// determine if the user is a unit admin of the specified unit id
			return userUnitMappings.Any(i => i.Key == unitId && i.Value.Role == RoleTypes.UnitAdministrator);

		}

		#region View Unit

		/// <summary>
		/// Gets the ViewResult for the ViewUnit action.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<ActionResult> GetViewUnitViewResult(Guid id, string viewPath)
		{
			// Get the unit
			Unit unit = await UnitService.GetById(id);

			// If the unit is null, throw 404
			if (unit == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Get the list of users in the unit from the Users property of the unit
			List<IdentityUser> unitUsers = new List<IdentityUser>();

			if (String.IsNullOrEmpty(Request.QueryString["q"]))
			{
				// Get all the users for the unit
				unitUsers.AddRange(await UnitService.GetUsersForUnit(id));
			}
			else
			{

				// Get the search results
				IList<IdentityUser> searchResults = await UserService.SearchUsers(Request.QueryString["q"]);

				// Only add the users that have a user id in the unit
				unitUsers.AddRange(searchResults.Where(i => unit.Users.Select(u => u.UserId).Contains(i.Id)));
			}

			// Create the list of unit member view models for the users in the unit
			IList<UnitMemberViewModel> unitUserModels = new List<UnitMemberViewModel>();
			foreach (IdentityUser unitUser in unitUsers)
			{
				unitUserModels.Add(new UnitMemberViewModel()
				{
					EmailAddress = unitUser.EmailAddress,
					FirstName = unitUser.FirstName,
					UnitRole = unit.Users.FirstOrDefault(i => i.UserId == unitUser.Id).Role,
					Id = unitUser.Id,
					Surname = unitUser.Surname,
					Profile = unitUser.Profile,
					Status = unitUser.Status
				});
			}

			// Get the list of additional capcodes
			IList<Capcode> allCapcodes = await CapcodeService.GetAll();
			IList<Capcode> additionalCapcodes = allCapcodes.Where(i => unit.AdditionalCapcodes.Contains(i.Id)).ToList();

			// Create the model for the single view
			SingleUnitViewModel model = new SingleUnitViewModel()
			{
				Id = unit.Id,
				Name = unit.Name,
				Description = unit.Description,
				Service = EnumExtensions.GetEnumDescription(unit.Service),
				Capcode = unit.Capcode,
				AdditionalCapcodes = additionalCapcodes,
				Users = unitUserModels,
				Region = unit.Region.Name,
				HeadquartersCoordinates = unit.HeadquartersCoordinates
			};

			return View(viewPath, model);
		}

		#endregion

		#region Edit Unit

		/// <summary>
		/// Gets the ViewResult for the Edit unit GET request.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<ActionResult> GetEditUnitViewResult(Guid id, string viewPath)
		{

			// Get the unit based on the id
			Unit unit = await UnitService.GetById(id);

			// If the unit is null, throw not found exception
			if (unit == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Create and populate the model
			CreateUnitModel model = new CreateUnitModel();
			model.AvailableRegions = await GetAvailableRegions();
			model.Capcode = unit.Capcode;
			model.AvailableUnitCapcodes = await CapcodeService.GetAllByService(unit.Service, true);
			model.AvailableAdditionalCapcodes = await CapcodeService.GetAllByService(unit.Service, false);
			model.AdditionalCapcodes = String.Format("{0},", String.Join(",", unit.AdditionalCapcodes));
			model.Description = unit.Description;
			model.Latitude = unit.HeadquartersCoordinates.Latitude;
			model.Longitude = unit.HeadquartersCoordinates.Longitude;
			model.Name = unit.Name;
			model.Region = unit.Region.Id;

			// Set the page title.
			ViewBag.Title = "Edit unit";

			return View(viewPath, model);

		}

		public async Task<ActionResult> PostEditUnitViewResult(Guid id, CreateUnitModel model, string viewPath, bool adminEdit)
		{
			// Set the form action and the page title.
			ViewBag.Title = "Edit unit";

			// Get the unit based on the id
			Unit unit = await UnitService.GetById(id);

			// If the unit is null, throw not found exception
			if (unit == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Set the available options
			model.AvailableRegions = await GetAvailableRegions();
			model.AvailableAdditionalCapcodes = await CapcodeService.GetAllByService(unit.Service, false);
			model.AvailableUnitCapcodes = await CapcodeService.GetAllByService(unit.Service, true);

			// Get the service type from the model
			ServiceType service = ServiceType.StateEmergencyService;

			// Get the region based on the posted value
			IList<Region> regions = await UnitService.GetRegions();
			Region region = regions.FirstOrDefault(i => i.Id == model.Region);

			// If the region is null, log the error and return error message
			if (region == null)
			{
				await Log.Error(String.Format("Unable to update unit. Region '{0}' does not exist.", model.Region));
				ModelState.AddModelError("", "There was a system error updating the unit.");
				return View(viewPath, model);
			}

			// Create the headquarters coords
			Coordinates coords = new Coordinates(model.Latitude.Value, model.Longitude.Value);

			IList<Guid> additionalCapcodeIds = GetCapcodeIdsFromHiddenValue(model.AdditionalCapcodes);

			try
			{

				// Update the values of the unit
				unit.Name = model.Name;
				unit.Description = model.Description;
				unit.Updated = DateTime.UtcNow;
				unit.HeadquartersCoordinates = coords;
				unit.Region = region;

				if (adminEdit)
				{
					unit.Capcode = model.Capcode;
					unit.AdditionalCapcodes = GetCapcodeIdsFromHiddenValue(model.AdditionalCapcodes);
					unit.Service = service;
				}

				// Create the capcode if it doesn't exist.
				await CheckAndCreateCapcode(model.Capcode, model.Name, service);

				// Save the unit to the database
				await UnitService.UpdateUnit(unit);

				return new RedirectResult(String.Format("/{0}/units/{1}?saved=1", AreaPrefix, id));

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Unable to update unit. System exception: {0}", ex.Message), ex);
				ModelState.AddModelError("", "There was a system error updating the unit.");
				return View(viewPath, model);
			}
		}

		#endregion

		#region New user

		public async Task<ActionResult> GetAddMemberViewResult(Guid unitId)
		{

			// Get the unit
			Unit unit = await UnitService.GetById(unitId);

			// If the unit is null, return 404 not found error
			if (unit == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			NewUserViewModel model = new NewUserViewModel();
			model.AvailableRoles = GetAvailableRoles();
			return View("~/Areas/Admin/Views/Units/NewUser.cshtml", model);

		}

		public async Task<ActionResult> PostAddMemberViewResult(Guid unitId, NewUserViewModel model)
		{
			// Get the unit
			Unit unit = await UnitService.GetById(unitId);

			// If the unit is null, return 404 not found error
			if (unit == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Get the available roles for the user.
			model.AvailableRoles = GetAvailableRoles();

			// If the model is not valid, return view.
			if (!ModelState.IsValid)
			{
				return View("~/Areas/Admin/Views/Units/NewUser.cshtml", model);
			}

			// If there is "System Administrator" in the role list, show error message
			if (model.Role.Equals(RoleTypes.SystemAdministrator, StringComparison.CurrentCultureIgnoreCase))
			{
				ModelState.AddModelError("", "There was an error setting the role for the user.");
				return View("~/Areas/Admin/Views/Units/NewUser.cshtml", model);
			}

			// Get the identity user related to the specified unit admin
			IdentityUser newUser = await UserService.FindByEmailAsync(model.EmailAddress);

			// If the user exists, and there is already a user mapping in the unit for this user, show error message
			if (newUser != null && unit.Users.Any(i => i.UserId == newUser.Id))
			{
				ModelState.AddModelError("", "The email address you have entered is already a member of this unit.");
				return View("~/Areas/Admin/Views/Units/NewUser.cshtml", model);
			}

			// Add the model to the session for the next screen
			Session[AddMemberViewModelSessionKey] = model;

			return new RedirectResult(String.Format("/{0}/units/{1}/confirm-member", AreaPrefix, unitId));
		}

		public async Task<ActionResult> GetConfirmMemberViewResult(Guid unitId, string viewPath)
		{
			// Get the unit
			Unit unit = await UnitService.GetById(unitId);

			// If the unit is null, return 404 not found error
			if (unit == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Get the CreateUnitViewModel from session
			NewUserViewModel newUserModel = (NewUserViewModel)Session[AddMemberViewModelSessionKey];

			// If the view model is null, redirect back to the Unit home screen
			if (newUserModel == null)
			{
				return new RedirectResult(String.Format("/{0}/units/{1}", AreaPrefix, unitId));
			}

			// Get the identity user related to the specified unit admin
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
				model.MemberNumber = newUser.Profile.MemberNumber;
			}

			return View(viewPath, model);
		}

		public async Task<ActionResult> PostConfirmMemberViewResult(Guid unitId, ConfirmUserViewModel model, string viewPath)
		{
			try
			{

				// Get the unit
				Unit unit = await UnitService.GetById(unitId);

				// If the unit is null, return 404 not found error
				if (unit == null)
				{
					throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
				}

				// If the model is not valid, return view.
				if (!ModelState.IsValid)
				{
					return View(viewPath, model);
				}

				// If there is "System Administrator" in the role list, show error message
				if (model.Role.Equals(RoleTypes.SystemAdministrator, StringComparison.CurrentCultureIgnoreCase))
				{
					ModelState.AddModelError("", "There was an error setting the role for the user.");
					return View(viewPath, model);
				}

				// Get the identity user related to the specified unit admin
				IdentityUser newUser = await UserService.FindByEmailAsync(model.EmailAddress);

				// If the user exists, and there is already a user mapping in the unit for this user, show error message
				if (newUser != null && unit.Users.Any(i => i.UserId == newUser.Id))
				{
					ModelState.AddModelError("", "The email address you have entered is already a member of this unit.");
					return View(viewPath, model);
				}
				else if (newUser != null)
				{
					// Create the user mapping for the existing user
					await UnitService.AddUserToUnit(newUser.Id, model.Role, unitId);
				}
				else
				{

					// Create the profile
					UserProfile profile = new UserProfile()
					{
						MemberNumber = model.MemberNumber
					};

					// Create the list of roles
					List<string> roles = new List<string>() { model.Role };
					
					// If there is no "General User" role, add it so that they can access the basic site areas
					if (!roles.Contains(RoleTypes.GeneralUser))
					{
						roles.Add(RoleTypes.GeneralUser);
					}

					// Create the new user, and then create the unit mapping for the new user.
					newUser = await UserService.CreateAsync(model.EmailAddress, model.FirstName, model.Surname, roles, profile, !model.SkipEmailActivation);

					// Send the activation email, if we don't need to skip it
					if (!model.SkipEmailActivation)
					{
						await MailService.SendAccountActivationEmail(newUser);
					}

					// Now that we have the newUser, create the user mapping.
					await UnitService.AddUserToUnit(newUser.Id, model.Role, unitId);

				}

				// remove the newUserModel from the session
				Session.Remove(AddMemberViewModelSessionKey);

				// redirect back to unit view page
				return new RedirectResult(String.Format("/{0}/units/{1}?member_added=1", AreaPrefix, unitId));

			}
			catch (Exception ex)
			{
				await Log.Error("Error adding new user to unit. Message: " + ex.Message, ex);
				ModelState.AddModelError("", "There was a system error adding the user to the unit.");
				return View(viewPath, model);
			}
		}

		#endregion

		#region Resend Activation Email

		public async Task<ActionResult> ResendActivationEmail(Guid userId)
		{

			// Update the activation code for the user
			string newActivationCode = await UserService.ResetActivationCode(userId);

			// Get the identity user related to the specified unit admin
			IdentityUser newUser = await UserService.FindByIdAsync(userId);

			// Resend the activation email
			await MailService.SendAccountActivationEmail(newUser);

			// Redirect to the complete page
			return new RedirectResult("/control-panel/resend-activation-email/complete");
		}

		#endregion

		#region Change User Role 

		public ActionResult GetChangeUserRoleViewResult(Guid unitId, Guid userId, string viewPath)
		{
			// Create the model
			ChangeUserRoleViewModel model = new ChangeUserRoleViewModel();
			model.AvailableRoles = GetAvailableRoles();
			model.UnitId = unitId;

			// return the view
			return View(viewPath, model);
		}

		public async Task<ActionResult> ViewChangeUserRoleViewResult(Guid unitId, Guid userId, ChangeUserRoleViewModel model, string viewPath, string urlPart)
		{

			// Create the model
			model.AvailableRoles = GetAvailableRoles();
			model.UnitId = unitId;

			// If the model has errors, return
			if (!ModelState.IsValid)
			{
				return View(viewPath, model);
			}

			try
			{

				// Get the unit so that we can validate the unit not getting < 1 unit administrator
				Unit unit = await UnitService.GetById(unitId);

				if (unit == null)
				{
					await Log.Error("Unable to change user role for unit. Unit cannot be found.  ");
					ModelState.AddModelError("", "Unable to change role for user. The unit cannot be found.");
					return View(viewPath, model);
				}

				// If there is only 1 unit administrator, and it's the current user, and the new role is general user, hen show error message stating you must hace one administrator account
				if (unit.Users.Count(i => i.Role.ToUpper() == RoleTypes.UnitAdministrator.ToUpper()) == 1 &&
					unit.Users.First(i => i.Role.ToUpper() == RoleTypes.UnitAdministrator.ToUpper()).UserId == userId &&
					model.Role.ToUpper() == RoleTypes.GeneralUser.ToUpper())
				{
					ModelState.AddModelError("", "You cannot change the role for this user to General User. Your unit must always have at least one Unit Administrator.");
					return View(viewPath, model);
				}


				// Update the role for the user
				await UnitService.ChangeUserRoleInUnit(unitId, userId, model.Role);

				// return the view
				return new RedirectResult(String.Format("/{0}/units/{1}?role_changed=1", urlPart.ToLower(), unitId));

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Unable to change role for user in unit. Message: {0}", ex.Message), ex);
				ModelState.AddModelError("", "Sorry, there was an error changing the role for the user.");
				return View(viewPath, model);
			}

		}

		#endregion

		#region Remove User From Unit

		public async Task<ActionResult> GetRemoveUserFromUnit(Guid unitId, Guid userId, string urlPart)
		{
			try
			{
				// Remover the user
				await UnitService.RemoveUserFromUnit(userId, unitId);

				// return to the view unit screen
				return new RedirectResult(String.Format("/{0}/units/{1}?user_removed=1", urlPart, unitId));
			} 
			catch (Exception ex)
			{
				// Log the error and redirect with error query string
				await Log.Error(String.Format("Unable to remove user '{0}' from unit '{1}'.", userId, unitId), ex);

				// return to the view unit screen
				return new RedirectResult(String.Format("/{0}/units/{1}?remove_user_error=1", urlPart, unitId));
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

			IList<Region> regions = await UnitService.GetRegions();

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
		/// <param name="unitName">The name of the unit to create the capcode for.</param>
		/// <param name="service">The service the capcode is associated with.</param>
		/// <returns></returns>
		protected async Task CheckAndCreateCapcode(string capcode, string unitName, ServiceType service)
		{

			// Check if the capcode exists 
			IList<Capcode> allCapcodes = await CapcodeService.GetAll();

			// Check if the capcode exists in the collection
			if (!allCapcodes.Any(i => i.CapcodeAddress == capcode))
			{

				// Create the capcode
				await CapcodeService.Create(unitName, capcode, "", service, true);

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
			availableRoles.Add(new SelectListItem() { Text = RoleTypes.UnitAdministrator, Value = RoleTypes.UnitAdministrator });

			// return the available roles
			return availableRoles;
		}

		/// <summary>
		/// Gets the current context unit id for the control panel.
		/// </summary>
		/// <returns></returns>
		protected Guid GetControlPanelUnitId()
		{
			if (Session[SessionConstants.ControlPanelContextUnitId] != null)
			{
				return (Guid)Session[SessionConstants.ControlPanelContextUnitId];
			}
			else
			{
				return Guid.Empty;
			}
		}

		#endregion
	}
}