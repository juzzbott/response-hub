using Enivate.ResponseHub.Model.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.Model.Training.Interface;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Training;
using Enivate.ResponseHub.UI.Areas.Admin.Models.TrainingTypes;

namespace Enivate.ResponseHub.UI.Areas.Admin.Controllers
{

	[RouteArea("admin")]
	[RoutePrefix("training-types")]
	[ClaimsAuthorize(Roles = RoleTypes.SystemAdministrator)]
	public class TrainingTypesController : BaseAdminController
    {

		protected ITrainingService TrainingService = ServiceLocator.Get<ITrainingService>();

		// GET: Admin/TrainingTypes
		[Route]
		public async Task<ActionResult> Index()
        {

			// Get the training types from the db
			IList<TrainingType> trainingTypes = await TrainingService.GetAllTrainingTypes();

			// return the view
            return View(trainingTypes);
        }

		[Route("create")]
		public ActionResult Add()
		{
			// return the view
			return View("AddEdit", new AddEditTrainingTypeViewModel());
		}

		[Route("create")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Add(AddEditTrainingTypeViewModel model)
		{

			// If the model state is invalid, return
			if (!ModelState.IsValid)
			{
				return View("AddEdit", model);
			}

			try
			{ 

				// Create the new training type
				TrainingType trainingType = new TrainingType()
				{
					Description = model.Description,
					Name = model.Name,
					ShortName = model.ShortName
				};

				// Save the training type
				await TrainingService.SaveTrainingType(trainingType);

				// Redirect back to the list of training types
				return new RedirectResult("/admin/training-types?created=1");

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Error saving training type. Message: {0}", ex.Message), ex);
				ModelState.AddModelError("", "There was a system error creating the new training type.");
				return View("AddEdit", model);
			}
		}

		[Route("{id:guid}")]
		public async Task<ActionResult> Edit(Guid id)
		{
			// Get the training type based on the id.
			TrainingType trainingType = await TrainingService.GetTrainingTypeById(id);

			// Create the AddEdit view model
			AddEditTrainingTypeViewModel model = new AddEditTrainingTypeViewModel()
			{
				Description = trainingType.Description,
				Name = trainingType.Name,
				ShortName = trainingType.ShortName,
				EditingMode = true
			};

			// return the view
			return View("AddEdit", model);

		}

		[Route("{id:guid}")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit(Guid id, AddEditTrainingTypeViewModel model)
		{

			// Reset the editing mode
			model.EditingMode = true;

			// If the model state is invalid, return
			if (!ModelState.IsValid)
			{
				return View("AddEdit", model);
			}

			try
			{

				// Get the training type based on the id.
				TrainingType trainingType = await TrainingService.GetTrainingTypeById(id);

				// Set the properties of the training type
				trainingType.Name = model.Name;
				trainingType.ShortName = model.ShortName;
				trainingType.Description = model.Description;

				// Save the training type
				await TrainingService.SaveTrainingType(trainingType);

				// Redirect back to the list of training types
				return new RedirectResult("/admin/training-types?saved=1");

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Error saving training type. Message: {0}", ex.Message), ex);
				ModelState.AddModelError("", "There was a system error saving the changes to the training type.");
				return View("AddEdit", model);
			}

		}
	}
}