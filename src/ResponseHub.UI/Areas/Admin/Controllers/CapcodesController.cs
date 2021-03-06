using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Unity;

using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.UI.Areas.Admin.Models.Capcodes;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Units;

namespace Enivate.ResponseHub.UI.Areas.Admin.Controllers
{

	[RouteArea("admin")]
	[RoutePrefix("capcodes")]
	[ClaimsAuthorize(Roles = RoleTypes.SystemAdministrator)]
	public class CapcodesController : BaseAdminController
    {

		[Route]
        // GET: Admin/Capcodes
        public async Task<ActionResult> Index()
        {

			// Get the capcodes
			List<Capcode> capcodes = new List<Capcode>();

			// If there is no search term, return all results, otherwise return only those that match the search results.
			if (String.IsNullOrEmpty(Request.QueryString["q"]))
			{
				// Get the most recent units
				capcodes.AddRange(await CapcodeService.GetAll());
				capcodes = capcodes.OrderByDescending(i => i.Created).Take(30).ToList();
			}
			else
			{
				capcodes.AddRange(await CapcodeService.FindByName(Request.QueryString["q"]));
			}

            return View(capcodes.OrderBy(i => i.Name).ToList());
        }

		#region Create capcode

		[Route("create")]
		public ActionResult Create()
		{

			// Set the title of the view.
			ViewBag.Title = "Create new capcode";

			// Create a new instance of the model.
			CreateCapcodeViewModel model = new CreateCapcodeViewModel();

			return View("CreateEdit", model);
		}

		[Route("create")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create(CreateCapcodeViewModel model)
		{

			// Set the title of the view.
			ViewBag.Title = "Create new capcode";

			// If the model is invalid, then show error message
			if (!ModelState.IsValid)
			{
				return View("CreateEdit", model);
			}

			// Try to create the new capcode. If successfull, return to capcode list, otherwise show error
			try
			{

				// Get the service type.
				int intServiceType;
				Int32.TryParse(model.Service, out intServiceType);
				ServiceType serviceType = (ServiceType)intServiceType;

				// Create the capcode
				await CapcodeService.Create(model.Name, model.CapcodeAddress, model.ShortName, serviceType, model.IsUnitCapcode);

				// return the to the list screen
				return new RedirectResult("/admin/capcodes?created=1");

			}
			catch (Exception ex)
			{
				// Log the error and return
				await Log.Error(String.Format("Unable to create new capcode. Message: {0}", ex.Message), ex);
				ModelState.AddModelError("", "Sorry, there was a system error creating the new capcode.");
				return View("CreateEdit", model);
			}

		}

		#endregion

		#region ViewEdit capcode

		[Route("{id:guid}")]
		public async Task<ActionResult> View(Guid id)
		{
			ViewBag.Title = "Edit capcode";

			// Get the capcode by id.
			Capcode capcode = await CapcodeService.GetById(id);

			// If the capcode is null, throw not found
			if (capcode == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Create the model and show the view
			CreateCapcodeViewModel model = new CreateCapcodeViewModel()
			{
				CapcodeAddress = capcode.CapcodeAddress,
				Id = capcode.Id,
				Name = capcode.Name,
				Service = ((int)capcode.Service).ToString(),
				ShortName = capcode.ShortName,
				IsUnitCapcode = capcode.IsUnitCapcode
			};

			// return the view
			return View("CreateEdit", model);

		}

		[Route("{id:guid}")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> View(Guid id, CreateCapcodeViewModel model)
		{
			ViewBag.Title = "Edit capcode";

			try
			{

				// Get the capcode by id.
				Capcode capcode = await CapcodeService.GetById(id);

				// If the capcode is null, throw not found
				if (capcode == null)
				{
					throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
				}

				// Set the model id
				model.Id = id;

				// If the model state is inalid, return the view
				if (!ModelState.IsValid)
				{
					return View("CreateEdit", model);
				}

				// Get the service type.
				int intServiceType;
				Int32.TryParse(model.Service, out intServiceType);
				ServiceType serviceType = (ServiceType)intServiceType;

				// Update the capcode details/ and save
				capcode.CapcodeAddress = model.CapcodeAddress;
				capcode.Name = model.Name;
				capcode.ShortName = model.ShortName;
				capcode.Service = serviceType;
				capcode.IsUnitCapcode = model.IsUnitCapcode;

				// Save the capcode
				await CapcodeService.Save(capcode);

				// redirect to list of capcodes
				return new RedirectResult("/admin/capcodes?saved=1");

			}
			catch (Exception ex)
			{
				// Log the error and return
				await Log.Error(String.Format("Unable to save capcode. Message: {0}", ex.Message), ex);
				ModelState.AddModelError("", "Sorry, there was a system error saving the capcode.");
				return View("CreateEdit", model);
			}
			

		}

		#endregion

		#region Delete

		[Route("delete/{id:guid}")]
		public async Task<ActionResult> Delete(Guid id)
		{

			// Get the capcode by id
			Capcode capcode = await CapcodeService.GetById(id);

			// If there is no capcode with this id, return 404
			if (capcode == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "Capcode not found.");
			}

			// Get all the units by the capcode to make sure it's not in use
			IList<Unit> unitsWithCapcode = await UnitService.GetUnitsByCapcode(capcode);

			// If there are units, so show the unable to delete screen
			if (unitsWithCapcode != null && unitsWithCapcode.Count > 0)
			{
				// Create the list of units with the capcode
				IList<string> model = unitsWithCapcode.Select(i => i.Name).ToList();
				return View("CapcodesExist", model);
			}

			// We don't have any units assigned to this capcode so we can just delete it
			await CapcodeService.Remove(id);			

			// Redirect to the capcode index screen.
			return new RedirectResult("/admin/capcodes?deleted=1");
		}

		#endregion

	}
}