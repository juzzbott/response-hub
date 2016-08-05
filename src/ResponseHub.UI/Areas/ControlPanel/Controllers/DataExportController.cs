using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.UI.Areas.ControlPanel.Models.DataExport;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.DataExport.Interface;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{

	[RouteArea("ControlPanel", AreaPrefix = "control-panel")]
	[RoutePrefix("data-export")]
	[ClaimsAuthorize(Roles = RoleTypes.GroupAdministrator)]
	public class DataExportController : BaseControlPanelController
    {

		protected IJobMessageService JobMessageService
		{
			get
			{
				return ServiceLocator.Get<IJobMessageService>();
			}
		}

		protected IDataExportService DataExportService
		{
			get
			{
				return ServiceLocator.Get<IDataExportService>();
			}
		}

		[Route]
		[HttpGet]
        // GET: ControlPanel/DataExport
        public async Task<ActionResult> Index()
        {

			// Get the groups
			IList<Guid> groupIds = await GetGroupIdsUserIsGroupAdminOf();
			IList<Group> groups = await GroupService.GetByIds(groupIds);

			// Create the data export model
			DataExportFilterViewModel model = new DataExportFilterViewModel();
			
			// Add the groups to the select list
			foreach(Group group in groups)
			{
				model.AvailableGroups.Add(new SelectListItem { Text = group.Name, Value = group.Id.ToString() });
			}

			// If there is more than one group, add a please select
			if (model.AvailableGroups.Count > 1)
			{
				model.AvailableGroups.Insert(0, new SelectListItem { Text = "Select a group...", Value = "" });
			}

            return View(model);
        }

		[Route]
		[HttpPost]
		public async Task<ActionResult> Index(DataExportFilterViewModel model)
		{

			// Get the group by the id
			Group group = await GroupService.GetById(model.GroupId);

			// Get the list of jobs between the start and end dates
			DateTime dateFrom = model.DateFrom.Date;
			DateTime dateTo = new DateTime(model.DateTo.Year, model.DateTo.Month, model.DateTo.Day, 23, 59, 59);

			// Get the list of messages for the capcode
			IList<JobMessage> messages = await JobMessageService.GetJobMessagesBetweenDates(
				new List<string> { group.Capcode },
				MessageType.Job & MessageType.Message,
				dateFrom,
				dateTo);

			// Now that we have the messages, get the export type from the model, and return
			if (model.ExportType == "csv")
			{
				string export = DataExportService.BuildCsvExportFile(messages);

				// return the file as a download
				FileContentResult result = new FileContentResult(Encoding.UTF8.GetBytes(export), "text/csv");
				result.FileDownloadName = String.Format("data-export-{0}.csv", DateTime.Now.ToString("yyyy-MM-dd-HHmmss"));
				return result;
			}
			else
			{
				FileContentResult result = new FileContentResult(DataExportService.BuildPdfExportFile(messages), "application/pdf");
				result.FileDownloadName = String.Format("data-export-{0}.pdf", DateTime.Now.ToString("yyyy-MM-dd-HHmmss"));
				return result;
			}

		}
		
	}
}