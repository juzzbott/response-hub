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
			string export = await DataExportService.BuildCsvExportFile(groupId, dateFrom, dateTo);

			// return the file as a download
			FileContentResult result = new FileContentResult(Encoding.UTF8.GetBytes(export), "text/csv");
			result.FileDownloadName = String.Format("data-export-{0}.csv", DateTime.Now.ToString("yyyy-MM-dd-HHmmss"));
			return result;
		}
		
	}
}