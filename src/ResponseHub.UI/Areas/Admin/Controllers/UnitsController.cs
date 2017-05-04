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
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Identity.Interface;
using Enivate.ResponseHub.Model.Spatial;
using Enivate.ResponseHub.UI.Areas.Admin.Models.Units;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.UI.Models.Users;
using Enivate.ResponseHub.UI.Controllers;
using Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers;

namespace Enivate.ResponseHub.UI.Areas.Admin.Controllers
{

	[RouteArea("admin")]
	[RoutePrefix("units")]
	[ClaimsAuthorize(Roles = RoleTypes.SystemAdministrator)]
	public class UnitsController : BaseAdminController
	{

		private const string CreateUnitViewModelSesionKey = "CreateUnitViewModel";

		[Route]
		// GET: Admin/Units
		public async Task<ActionResult> Index()
		{

			List<Unit> units = new List<Unit>();

			// If there is no search term, return all results, otherwise return only those that match the search results.
			if (String.IsNullOrEmpty(Request.QueryString["q"]))
			{
				// Get the most recent units
				units.AddRange(await UnitService.GetRecentlyAdded(30));
			}
			else
			{
				units.AddRange(await UnitService.FindByName(Request.QueryString["q"]));
			}

			return View(units);
		}

		#region Create unit

		[Route("create")]
		public async Task<ActionResult> Create()
		{

			// Set the form action and the addUnitAdministrator flag.
			ViewBag.AddUnitAdministrator = true;
			ViewBag.FormAction = "/admin/units/create";
			ViewBag.Title = "Create new unit";

			CreateUnitModel model = new CreateUnitModel();
			model.AvailableRegions = await GetAvailableRegions();
			model.AvailableUnitCapcodes = await CapcodeService.GetAllByUnitOnly(true);
			model.AvailableAdditionalCapcodes = await CapcodeService.GetAllByUnitOnly(false);

			return View("CreateEdit", model);
		}

		[Route("create")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create(CreateUnitModel model)
		{

			// Set the form action and the addUnitAdministrator flag.
			ViewBag.AddUnitAdministrator = true;
			ViewBag.FormAction = "/admin/units/create";
			ViewBag.Title = "Create new unit";

			// Get the regions select list.
			model.AvailableRegions = await GetAvailableRegions();
			model.AvailableUnitCapcodes = await CapcodeService.GetAllByUnitOnly(true);
			model.AvailableAdditionalCapcodes = await CapcodeService.GetAllByUnitOnly(false);

			// If the model is not valid, return view.
			if (!ModelState.IsValid)
			{
				return View("CreateEdit", model);
			}

			// Get the service type.
			ServiceType serviceType = ServiceType.StateEmergencyService;

			// Ensure the unit name/service combination is unique
			bool unitExists = await UnitService.CheckIfUnitExists(model.Name, serviceType);

			// If the unit exists, then display the unit exists message
			if (unitExists)
			{
				ModelState.AddModelError("", "Sorry, there is already a unit by that name in the selected service.");
				return View("CreateEdit", model);
			}

			// Store the CreateUnitViewModel in session for the next screen
			Session[CreateUnitViewModelSesionKey] = model;

			// Redirect to the unit administrator screen
			return new RedirectResult("/admin/units/create/unit-administrator");

		}

		[Route("create/unit-administrator")]
		public async Task<ActionResult> UnitAdministrator()
		{

			// Set the form action
			ViewBag.FormAction = "/admin/units/create/unit-administrator";

			// Get the CreateUnitViewModel from session
			CreateUnitModel model = (CreateUnitModel)Session[CreateUnitViewModelSesionKey];

			// If the view model is null, redirect back to the create screen
			if (model == null)
			{
				return new RedirectResult("/admin/units/create");
			}

			// Get the identity user related to the specified unit admin
			IdentityUser unitAdminUser = await UserService.FindByEmailAsync(model.UnitAdministratorEmail);

			// Set the role as Unit Administrator by default.
			model.UnitAdministrator.Role = RoleTypes.UnitAdministrator;

			// Set the Email address regardless of if the user exists or not, as it was set in the previous screen.
			model.UnitAdministrator.EmailAddress = model.UnitAdministratorEmail;

			// If there is a unit user, then add to the model.
			if (unitAdminUser != null)
			{
				model.UnitAdministrator.FirstName = unitAdminUser.FirstName;
				model.UnitAdministrator.Surname = unitAdminUser.Surname;
				model.UnitAdministrator.UserExists = true;
				model.UnitAdministrator.MemberNumber = unitAdminUser.Profile.MemberNumber;
			}

			return View("ConfirmUser", model.UnitAdministrator);
		}

		[Route("create/unit-administrator")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<ActionResult> UnitAdministrator(ConfirmUserViewModel model)
		{

			// Set the form action
			ViewBag.FormAction = "/admin/units/create/unit-administrator";
			ViewBag.SubmitButtonTitle = "Create unit";

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

			// Get the CreateUnitViewModel from session
			CreateUnitModel createUnitModel = (CreateUnitModel)Session[CreateUnitViewModelSesionKey];

			// If the view model is null, redirect back to the create screen
			if (createUnitModel == null)
			{
				return new RedirectResult("/admin/units/create");
			}

			// Store the unit administrator
			IdentityUser unitAdmin = null;

			// If the unit administrator user does not exist, then create the user now
			if (!model.UserExists)
			{

				// Create the profile
				UserProfile profile = new UserProfile()
				{
					MemberNumber = model.MemberNumber
				};

				// Create the new user
				unitAdmin = await UserService.CreateAsync(model.EmailAddress, model.FirstName, model.Surname, new List<string>() { RoleTypes.UnitAdministrator, RoleTypes.GeneralUser }, profile, true);

				// Send the email to the user
				await MailService.SendAccountActivationEmail(unitAdmin);

			}
			else
			{
				// Get the identity user related to the specified unit admin
				unitAdmin = await UserService.FindByEmailAsync(createUnitModel.UnitAdministratorEmail);

				// If the unit admin user is null, return an error, otherwise set the unit admin id.
				if (unitAdmin == null)
				{
					ModelState.AddModelError("", "There was a system error creating the unit.");
					await Log.Error(String.Format("Unable to create unit. Existing user with email '{0}' could not be found.", createUnitModel.UnitAdministratorEmail));
					return View("ConfirmUser", model);
				}
			}

			// Get the service type from the model
			ServiceType service = ServiceType.StateEmergencyService;

			// Get the region based on the posted value
			IList<Region> regions = await UnitService.GetRegions();
			Region region = regions.FirstOrDefault(i => i.Id == createUnitModel.Region);

			// If the region is null, log the error and return error message
			if (region == null)
			{
				await Log.Error(String.Format("Unable to create unit. Region '{0}' does not exist.", createUnitModel.Region));
				ModelState.AddModelError("", "There was a system error creating the unit.");
				return View("ConfirmUser", model);
			}

			// Create the training night info
			TrainingNightInfo trainingNight = new TrainingNightInfo()
			{
				DayOfWeek = createUnitModel.TrainingNight,
				StartTime = createUnitModel.TrainingStartTime
			};

			// Create the headquarters coords
			Coordinates coords = new Coordinates(createUnitModel.Latitude.Value, createUnitModel.Longitude.Value);

			// Get the list of additional capcodes
			IList<Guid> additionalCapcodes = GetCapcodeIdsFromHiddenValue(createUnitModel.AdditionalCapcodes);

			// Create the capcode if it doesn't exist.
			await CheckAndCreateCapcode(createUnitModel.Capcode, createUnitModel.Name, service);

			// Create the unit
			await UnitService.CreateUnit(createUnitModel.Name, service, createUnitModel.Capcode, additionalCapcodes, unitAdmin.Id, createUnitModel.Description, region, coords, trainingNight);

			// Send the new unit email to the unit admin
			await MailService.SendUnitCreatedEmail(unitAdmin, createUnitModel.Name, service, createUnitModel.Capcode);

			// Clear the session url
			Session.Remove(CreateUnitViewModelSesionKey);

			// redirect back to unit index page
			return new RedirectResult("/admin/units?unit_created=1");
		}

		#endregion

		#region View unit

		[Route("{id:guid}")]
		[HttpGet]
		public async Task<ActionResult> ViewUnit(Guid id)
		{
			return await GetViewUnitViewResult(id, "~/Areas/Admin/Views/Units/ViewUnit.cshtml");		
		}

		#endregion

		#region New user

		[Route("{unitId:guid}/add-member")]
		[HttpGet]
		public async Task<ActionResult> AddMember(Guid unitId)
		{
			return await GetAddMemberViewResult(unitId);
		}


		[Route("{unitId:guid}/add-member")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> AddMember(Guid unitId, NewUserViewModel model)
		{
			return await PostAddMemberViewResult(unitId, model);			
		}

		[Route("{unitId:guid}/confirm-member")]
		[HttpGet]
		public async Task<ActionResult> ConfirmMember(Guid unitId)
		{
			return await GetConfirmMemberViewResult(unitId, "~/Areas/Admin/Views/Units/ConfirmUser.cshtml");
		}

		[Route("{unitId:guid}/confirm-member")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ConfirmMember(Guid unitId, ConfirmUserViewModel model)
		{
			return await PostConfirmMemberViewResult(unitId, model, "~/Areas/Admin/Views/Units/ConfirmUser.cshtml");		
		}

		#endregion

		#region Edit unit

		[Route("{id:guid}/edit")]
		[HttpGet]
		public async Task<ActionResult> Edit(Guid id)
		{
			return await GetEditUnitViewResult(id, "~/Areas/Admin/Views/Units/CreateEdit.cshtml");			
		}

		[Route("{id:guid}/edit")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<ActionResult> Edit(Guid id, CreateUnitModel model)
		{
			return await PostEditUnitViewResult(id, model, "~/Areas/Admin/Views/Units/CreateEdit.cshtml", true);
		}
		
		#endregion

		#region Change User Role

		[Route("{unitId:guid}/change-role/{userId:guid}")]
		[HttpGet]
		public ActionResult ChangeUserRole(Guid unitId, Guid userId)
		{
			return GetChangeUserRoleViewResult(unitId, userId, "~/Areas/Admin/Views/Units/ChangeUserRole.cshtml");
		}

		[Route("{unitId:guid}/change-role/{userId:guid}")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ChangeUserRole(Guid unitId, Guid userId, ChangeUserRoleViewModel model)
		{
			return await ViewChangeUserRoleViewResult(unitId, userId, model, "~/Areas/Admin/Views/Units/ChangeUserRole.cshtml", "admin");
		}

		#endregion
		
	}
}