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
using System.Globalization;

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
        public ActionResult Index()
        {
            return View();
        }

		[Route]
		[HttpPost]
		public async Task<ActionResult> Index(DataExportFilterViewModel model)
		{

			// Get the current group id
			Guid groupId = GetControlPanelGroupId();

			// Get the list of jobs between the start and end dates
			DateTime dateFrom = model.DateFrom.Date;
			DateTime dateTo = new DateTime(model.DateTo.Year, model.DateTo.Month, model.DateTo.Day, 23, 59, 59);

			// Now that we have the messages, get the export type from the model, and return
			if (model.ExportType.ToLower() == "csv")
			{
				
				string export = await DataExportService.BuildCsvExportFile(groupId, dateFrom, dateTo);

				// return the file as a download
				FileContentResult result = new FileContentResult(Encoding.UTF8.GetBytes(export), "text/csv");
				result.FileDownloadName = String.Format("data-export-{0}.csv", DateTime.Now.ToString("yyyy-MM-dd-HHmmss"));
				return result;
			}
			else if (model.ExportType.ToLower() == "pdf")
			{

				// Get the PDF bytes
				byte[] pdfBytes = await DataExportService.BuildPdfExportFile(groupId, dateFrom, dateTo);

				FileContentResult result = new FileContentResult(pdfBytes, "application/pdf");
				result.FileDownloadName = String.Format("data-export-{0}.pdf", DateTime.Now.ToString("yyyy-MM-dd-HHmmss"));
				return result;
			}
			else if (model.ExportType.ToLower() == "html")
			{

				// Get the PDF bytes
				string htmlContent = await DataExportService.BuildHtmlExportFile(groupId, dateFrom, dateTo);

				FileContentResult result = new FileContentResult(Encoding.UTF8.GetBytes(htmlContent), "text/html");
				result.FileDownloadName = String.Format("data-export-{0}.html", DateTime.Now.ToString("yyyy-MM-dd-HHmmss"));
				return result;

			}

			throw new HttpException(400, "Bad request.");

		}

		[Route("generate-html-export")]
		[HttpGet]
		[AllowAnonymous]
		public async Task<ActionResult> GenerateHtmlExport()
		{

			// Get the parameters from the query string
			Guid groupId = new Guid(Request.QueryString["group_id"]);
			DateTime dateFrom = DateTime.ParseExact(Request.QueryString["date_from"], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
			DateTime dateTo = DateTime.ParseExact(Request.QueryString["date_to"], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);

			// Get the group by the id
			Group group = await GroupService.GetById(groupId);

			// Ensure the user is a group administrator of the specific group, otherwise 403 forbidden.

			// Get the list of messages for the capcode
			IList<JobMessage> jobMessages = await JobMessageService.GetJobMessagesBetweenDates(
				new List<string> { group.Capcode },
				MessageType.Job & MessageType.Message,
				dateFrom,
				dateTo);

			// Create the model
			HtmlDataExportViewModel model = new HtmlDataExportViewModel
			{
				Messages = jobMessages.Where(i => i.Type == MessageType.Message).ToList(),
				Jobs = jobMessages.Where(i => i.Type == MessageType.Job).OrderBy(i => i.Priority).ToList(),
				StartDate = dateFrom,
				FinishDate = dateTo
			};

			return View(model);

		}

		
	}
}