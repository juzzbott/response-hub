using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Upkeep;
using Enivate.ResponseHub.Model.Upkeep.Interface;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.UI.Models.Upkeep;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("upkeep")]
	[EnsureContextUnit]
	public class UpkeepController : BaseController
	{

		protected readonly IUpkeepService UpkeepService = ServiceLocator.Get<IUpkeepService>();

		// GET: Upkeep
		[Route]
		public ActionResult Index()
        {

			// Create the model
			UpkeepViewModel model = new UpkeepViewModel();

			// return the view
            return View(model);
        }

		[Route("new-report")]
		public async Task<ActionResult> NewReport()
		{

			// Create the model object
			UpkeepReportViewModel model = new UpkeepReportViewModel
			{
				Name = DateTime.Now.ToString("dddd d MMMM, yyyy")
			};

			// Set the available Tasks
			model.AvailableTasks = await GetAvailableTasksList();

			// Set the title
			ViewBag.Title = "Create new report";

			// return the view
			return View("AddEditReport", model);
		}

		[Route("new-report")]
		[HttpPost]
		public async Task<ActionResult> NewReport(UpkeepReportViewModel model)
		{
			// Set the available Tasks
			model.AvailableTasks = await GetAvailableTasksList();

			// Set the title
			ViewBag.Title = "Create new report";

			// Ensure the model is valid
			if (!ModelState.IsValid)
			{
				// return the view
				return View("AddEditReport", model);
			}

			// Get the list of task ids
			IList<Guid> taskIds = model.SelectedTasks.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(i => new Guid(i)).ToList();
			
			// Get the list of tasks for the unit
			IList<UpkeepTask> tasks = await UpkeepService.GetTasksByUnitId(GetContextUnitId());

			IList<UpkeepTask> selectedTasks = tasks.Where(i => taskIds.Contains(i.Id)).ToList();

			// Create the new report object
			UpkeepReport newReport = await UpkeepService.CreateNewReport(model.Name, DateTime.UtcNow, UserId, selectedTasks, GetContextUnitId());

			return new RedirectResult(String.Format("/upkeep/{0}", newReport.Id));
		}

		/// <summary>
		/// Gets the list of select list items for the tasks for the report. 
		/// </summary>
		/// <returns></returns>
		private async Task<IList<SelectListItem>> GetAvailableTasksList()
		{
			
			// Get the units based on the user
			IList<Unit> units = await UnitService.GetUnitsForUser(UserId);

			// Get the list of tasks for the unit
			IList<UpkeepTask> tasks = await UpkeepService.GetTasksByUnitId(units.FirstOrDefault().Id);

			// Create the list of select list items
			IList<SelectListItem> items = new List<SelectListItem>();

			// Map the tasks to the 
			foreach (UpkeepTask task in tasks)
			{
				items.Add(new SelectListItem() { Text = task.Name, Value = task.Id.ToString() });
			}

			// return the list of tasks
			return items;
		}
	}
}